# ============================================================
# LCC Test Suite - Local Secrets Configuration
# ============================================================
# 
# INSTRUCTIONS:
# 1. Copy this file to: secrets.config.ps1
# 2. Fill in your actual values
# 3. NEVER commit secrets.config.ps1 to git!
#
# For CI/CD pipelines, set these as environment variables instead.
# The scripts will automatically use environment variables if this
# file doesn't exist.
# ============================================================

# Azure AD Configuration
$env:LCC_TENANT_ID = "YOUR_AZURE_TENANT_ID"
$env:LCC_REGISTER_CASE_CLIENT_ID = ""  # Client ID for Register Case API
$env:LCC_API_ID = ""                    # LCC API Application/Client ID (auth and scope)
$env:LCC_API_CLIENT_SECRET = ""         # LCC API Client Secret (for confidential client flows)

# Azure AD User Credentials (for ROPC flow)
$env:LCC_AZURE_USERNAME = "your.name@cps.gov.uk"
$env:LCC_AZURE_PASSWORD = "your-azure-password"

# CMS Credentials
$env:LCC_CMS_USERNAME = "YourName.CIN3"
$env:LCC_CMS_PASSWORD = "your-cms-password"

# DDEI Access Keys (two separate keys: one for the LCC app DDEI auth,
# one for the Case Register DDEI auth)
$env:LCC_DDEI_ACCESS_KEY = "your-ddei-access-key-lcc-app"
$env:LCC_DDEI_ACCESS_KEY_REGCASE = "your-ddei-access-key-case-register"

# API Endpoints
$env:LCC_BASE_URL = "your-lacc-api-base-url"
$env:LCC_CASE_API_BASE_URL = "your-case-api-base-url"
$env:LCC_DDEI_BASE_URL = "your-ddei-api-base-url"

# Egress Configuration
$env:LCC_EGRESS_BASE_URL = "your-instance.egress"
$env:LCC_EGRESS_SERVICE_ACCOUNT_AUTH = "YOUR_BASE64_ENCODED_SERVICE_ACCOUNT_CREDENTIALS"
$env:LCC_EGRESS_TEMPLATE_ID = "YOUR_EGRESS_TEMPLATE_ID"
$env:LCC_EGRESS_ADMIN_ROLE_ID = "YOUR_EGRESS_ADMIN_ROLE_ID"

# Default Mode - Pre-existing case and workspace (used when -RegisterCase is NOT passed)
$env:LCC_DEFAULT_CASE_ID = "YOUR_EXISTING_CASE_ID"
$env:LCC_DEFAULT_CASE_URN = "YOUR_EXISTING_CASE_URN"
$env:LCC_DEFAULT_WORKSPACE_ID = "YOUR_EXISTING_WORKSPACE_ID"
$env:LCC_DEFAULT_WORKSPACE_NAME = "YOUR_EXISTING_WORKSPACE_NAME"

# Optional: Egress direct login (if not using service account)
# $env:LCC_EGRESS_USERNAME = "your-egress-username"
# $env:LCC_EGRESS_PASSWORD = "your-egress-password"
