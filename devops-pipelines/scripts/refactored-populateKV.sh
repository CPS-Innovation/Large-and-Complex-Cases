#!/bin/bash
kv_name=$1
secrets_array=$2

echo "Initializing Key Vault secrets from Azure DevOps variable groups..."
echo "🔐 Only storing sensitive secrets in Key Vault"

# Function to safely set Key Vault secret
set_kv_secret() {
    local secret_name="$1"
    local secret_value="$2"
    
    if [ -n "$secret_value" ] && [ "$secret_value" != "" ] && [ "$secret_value" != "null" ]; then
    echo "Setting Key Vault secret: $secret_name"
    az keyvault secret set \
        --vault-name "$kv_name" \
        --name "$secret_name" \
        --value "$secret_value" \
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

echo "📋 Setting KV secrets..."
for key in "${!secrets_array[@]}"; do
    set_kv_secret "$key" "${secrets_array[$key]}"
done

# List all secrets that were set (names only, not values)
echo "📋 Current Key Vault secrets:"
az keyvault secret list --vault-name "$kv_name" --query "[].name" -o table