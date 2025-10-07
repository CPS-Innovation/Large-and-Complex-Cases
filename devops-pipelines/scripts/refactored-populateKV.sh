#!/bin/bash
echo "Initializing Key Vault secrets from Azure DevOps variable groups..."
echo "🔐 Only storing sensitive secrets in Key Vault"

# Hash table of secret keys to their secret values
declare -A secrets_array=(
[ConnectionStrings--CaseManagementDatastoreConnection]=$CASE_MANAGEMENT_DATASTORE_CONNECTION
[EgressOptions--Username]=$EGRESS_OPTIONS_USERNAME 
[EgressOptions--Password]=$EGRESS_OPTIONS_PASSWORD
[DDEIOptions--AccessKey]=$DDEI_OPTIONS_ACCESS_KEY
[NetAppOptions--AccessKey]=$NET_APP_OPTIONS_ACCESS_KEY
[NetAppOptions--SecretKey]=$NET_APP_OPTIONS_SECRET_KEY
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
        exit_code=1
        exit_message="⚠️ Skipping '$secret_name' - value is empty or not provided"
        echo $exit_message
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