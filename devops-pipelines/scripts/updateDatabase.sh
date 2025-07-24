#!/bin/bash

echo "Installing PostgreSQL client..."
sudo apt-get update -qq
sudo apt-get install -y postgresql-client

# SECURITY: Disable command echoing to prevent credential exposure
set +x

# SECURITY: Parse connection string components WITHOUT storing the full secret in variables
echo "Retrieving and parsing connection string securely from Key Vault: $KEY_VAULT_NAME"

# Add retry logic for Key Vault access
for attempt in {1..3}; do
    echo "Key Vault access attempt $attempt..."

# Extract components directly from Key Vault command output without storing full connection string
DB_HOST=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "ConnectionStrings--CaseManagementDatastoreConnection" --query value -o tsv 2>/dev/null | grep -oP '(?<=Host=)[^;]+' | head -1)
DB_NAME=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "ConnectionStrings--CaseManagementDatastoreConnection" --query value -o tsv 2>/dev/null | grep -oP '(?<=Database=)[^;]+' | head -1)
DB_USER=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "ConnectionStrings--CaseManagementDatastoreConnection" --query value -o tsv 2>/dev/null | grep -oP '(?<=Username=)[^;]+' | head -1)
DB_PASS=$(az keyvault secret show --vault-name "$KEY_VAULT_NAME" --name "ConnectionStrings--CaseManagementDatastoreConnection" --query value -o tsv 2>/dev/null | grep -oP '(?<=Password=)[^;]+' | head -1)

# Check if we successfully retrieved all components
if [ -n "$DB_HOST" ] && [ -n "$DB_NAME" ] && [ -n "$DB_USER" ] && [ -n "$DB_PASS" ]; then
    echo "✅ Successfully retrieved connection string components on attempt $attempt"
    break
else
    if [ $attempt -eq 3 ]; then
    echo "❌ Failed to retrieve connection string components after 3 attempts"
    else
    echo "Key Vault access failed, waiting 10 seconds before retry..."
    sleep 10
    fi
fi
done

# Validate that we successfully retrieved and parsed credentials without exposing them
if [ -z "$DB_HOST" ]; then
    echo "❌ Could not parse PostgreSQL host from connection string"
    echo "This could indicate:"
    echo "1. PostgreSQL connection string not found in Key Vault"
    echo "2. Connection string format is incorrect"
    echo "3. Key Vault access permissions issue"
    echo "Please ensure ConnectionStrings--CaseManagementDatastoreConnection is set in Key Vault"
    exit 1
fi
if [ -z "$DB_NAME" ]; then
    echo "❌ Could not parse PostgreSQL database name from connection string"
    exit 1
fi
if [ -z "$DB_USER" ]; then
    echo "❌ Could not parse PostgreSQL username from connection string"
    exit 1
fi
if [ -z "$DB_PASS" ]; then
    echo "❌ Could not parse PostgreSQL password from connection string"
    exit 1
fi

echo "Testing database connection..."
echo "Host: $DB_HOST"
echo "Database: $DB_NAME"
echo "User: $DB_USER"
echo "Password: [REDACTED FOR SECURITY]"

# First test server connectivity
echo "Testing server connectivity..."
PGPASSWORD="$DB_PASS" psql -h "$DB_HOST" -U "$DB_USER" -d "postgres" -c "SELECT 'Server connection successful' as status;" -t

if [ $? -ne 0 ]; then
    echo "❌ Cannot connect to PostgreSQL server"
    exit 1
fi

# Then test target database connection with detailed output
echo "Testing target database connection..."
PGPASSWORD="$DB_PASS" psql -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" -c "SELECT 'Database connection successful' as status, version(), current_database(), current_user, inet_server_addr(), inet_server_port();" -t

if [ $? -ne 0 ]; then
    echo "❌ Database connection test failed"
    echo "Please check:"
    echo "1. PostgreSQL server is running and accessible"
    echo "2. Connection credentials are correct"
    echo "3. Network connectivity is available"
    exit 1
fi

echo "✅ Database connection test passed"

# Run migration script
echo "Running database migration script..."
PGPASSWORD="$DB_PASS" psql -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" -f "$MIGRATION_SCRIPT_PATH"

if [ $? -ne 0 ]; then
    echo "❌ Database migration failed"
    exit 1
fi 

echo "✅ Database migration completed successfully"

# Additional checks
echo "Running additional database health checks..."

# Check if we can query system tables
PGPASSWORD="$DB_PASS" psql -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" -c "SELECT count(*) as table_count FROM information_schema.tables WHERE table_schema = 'public';" -t

echo "✅ Database health check completed successfully"