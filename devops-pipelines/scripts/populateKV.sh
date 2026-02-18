#!/bin/bash
echo "Initializing Key Vault secrets from Azure DevOps variable groups..."
echo "🔐 Only storing sensitive secrets in Key Vault"

# Hash table of secret keys to their secret values
declare -A secrets_array=(
[ConnectionStrings--CaseManagementDatastoreConnection]=$CASE_MANAGEMENT_DATASTORE_CONNECTION
[EgressOptions--Username]=$EGRESS_OPTIONS_USERNAME 
[EgressOptions--Password]=$EGRESS_OPTIONS_PASSWORD
[DDEIOptions--AccessKey]=$DDEI_OPTIONS_ACCESS_KEY
[NetAppOptions--RootCaCert]=$NET_APP_OPTIONS_ROOT_CA_CERT
[NetAppOptions--IssuingCaCert]=$NET_APP_OPTIONS_ISSUING_CA_CERT
[NetAppOptions--IssuingCaCert2]=$NET_APP_OPTIONS_ISSUING_CA_CERT2
[FileTransferApiOptions--AccessKey]=$FILE_TRANSFER_API_OPTIONS_ACCESS_KEY
)

exit_code=0

# Function to safely set Key Vault secrets
set_kv_secret() {
    local secret_name="$1"
    local secret_value="$2"
    
    if [ -n "$secret_value" ] && [ "$secret_value" != "" ] && [ "$secret_value" != "null" ]; then
        echo "Setting Key Vault secret: $secret_name"
        az keyvault secret set \
            --vault-name "$KEY_VAULT_NAME" \
            --name "$secret_name" \
            --value "$secret_value" \
            --output none
        
        if [ $? -eq 0 ]; then
            echo "✅ Secret '$secret_name' set successfully"
        else
            exit_code=1
            exit_message="❌ Failed to set secret '$secret_name'"
            echo $exit_message
        fi
    else
        exit_code=0
        exit_message="⚠️ Skipping '$secret_name' - value is empty or not provided"
        echo "##vso[task.logissue type=warning]$exit_message"
    fi
}

echo "📋 Setting KV secrets..."
for key in "${!secrets_array[@]}"; do
    set_kv_secret "$key" "${secrets_array[$key]}"
done

# List all secrets that were set (names only, not values)
echo "📋 Current Key Vault secrets:"
az keyvault secret list --vault-name "$KEY_VAULT_NAME" --query "[].name" -o table

if [ "$exit_code" -eq 1 ]; then
    echo "Some secrets were not set: $exit_message"
    exit 1
fi

# Verify PostgreSQL connection string was set correctly (SECURELY)
echo "🔍 Verifying PostgreSQL connection string secret..."
POSTGRES_SECRET_EXISTS=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "ConnectionStrings--CaseManagementDatastoreConnection" --query "id" -o tsv 2>/dev/null || echo "")

if [ -n "$POSTGRES_SECRET_EXISTS" ]; then
    echo "✅ PostgreSQL connection string secret exists in Key Vault"
    
    # Test if we can retrieve the value WITHOUT storing it in a variable (for security)
    # We only check if the secret can be retrieved, not store its content
    if az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "ConnectionStrings--CaseManagementDatastoreConnection" --query value -o tsv >/dev/null 2>&1; then
    # Get just the length without storing the actual secret
        SECRET_LENGTH=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "ConnectionStrings--CaseManagementDatastoreConnection" --query value -o tsv 2>/dev/null | wc -c)
        echo "✅ PostgreSQL connection string value is accessible (length: $SECRET_LENGTH characters)"
    else
        echo "❌ PostgreSQL connection string secret exists but value is empty or inaccessible"
        exit 1
    fi
else
    echo "❌ PostgreSQL connection string secret was not created in Key Vault"
    echo "This indicates the variable 'CaseManagementDatastoreConnection' was empty or not provided"
    echo "Check variable group: lacc-backend-config-${{ lower(parameters.environment) }}"
    exit 1
fi