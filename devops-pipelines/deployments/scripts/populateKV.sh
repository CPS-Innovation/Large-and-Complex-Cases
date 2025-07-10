#!/bin/bash

echo "Initializing Key Vault secrets from Azure DevOps variable groups..."
echo "🔐 Only storing sensitive secrets in Key Vault"

# Function to safely set Key Vault secret
set_kv_secret() {
    local secret_name="$1"
    local secret_value="$2"
    local description="$3"
    
    if [ -n "$secret_value" ] && [ "$secret_value" != "" ] && [ "$secret_value" != "null" ]; then
    echo "Setting Key Vault secret: $secret_name"
    az keyvault secret set \
        --vault-name "$KEY_VAULT_NAME" \
        --name "$secret_name" \
        --value "$secret_value" \
        --description "$description" \
        --output none
    
    if [ $? -eq 0 ]; then
        echo "✅ Secret '$secret_name' set successfully"
    else
        echo "❌ Failed to set secret '$secret_name'"
    fi
    else
    echo "⚠️ Skipping '$secret_name' - value is empty or not provided"
    fi
}

echo "📋 Setting storage connection secrets..."

# Database Connection String
# set_kv_secret "ConnectionStrings--CaseManagementDatastoreConnection" "$CASE_MANAGEMENT_DATASTORE_CONNECTION" "connection string"
set_kv_sercet "ConnectionStrings--CaseManagementDatastoreConnection" $(CaseManagementDatastoreConnection) "connection string"

echo "📋 Setting external API authentication secrets..."

# Egress API Credentials
set_kv_secret "EgressOptions--Username" "$EGRESS_OPTIONS_USERNAME" "Egress API username"
set_kv_secret "EgressOptions--Password" "$EGRESS_OPTIONS_PASSWORD" "Egress API password"

# DDEI API Credentials
set_kv_secret "DDEIOptions--AccessKey" "$DDEI_OPTIONS_ACCESS_KEY" "DDEI API access key"

# NetApp API Credentials
set_kv_secret "NetAppOptions--AccessKey" "$NET_APP_OPTIONS_ACCESS_KEY" "NetApp API access key"
set_kv_secret "NetAppOptions--SecretKey" "$NET_APP_OPTIONS_SECRET_KEY" "NetApp API secret key"

# File Transfer API Credentials
set_kv_secret "FileTransferApiOptions--AccessKey" "$FILE_TRANSFER_API_OPTIONS_ACCESS_KEY" "File Transfer API access key"

echo "✅ Key Vault secret initialization completed"
echo "📋 Secrets stored in Key Vault:"
echo "  - EgressOptions--Username"
echo "  - EgressOptions--Password"
echo "  - DDEIOptions--AccessKey"
echo "  - ConnectionStrings--CaseManagementDatastoreConnection"
echo "  - NetAppOptions--AccessKey"
echo "  - NetAppOptions--SecretKey"
echo "  - FileTransferApiOptions--AccessKey"

# List all secrets that were set (names only, not values)
echo "📋 Current Key Vault secrets:"
az keyvault secret list --vault-name "$KEY_VAULT_NAME" --query "[].name" -o table

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
    echo "Check variable group: 'CaseManagementDatastoreConnection' variable"
    fi
else
    echo "❌ PostgreSQL connection string secret was not created in Key Vault"
    echo "This indicates the variable 'CaseManagementDatastoreConnection' was empty or not provided"
    echo "Check variable group: lacc-backend-config-${{ lower(parameters.environment) }}-variables"
fi