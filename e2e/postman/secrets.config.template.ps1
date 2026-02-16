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
$env:LCC_CLIENT_ID = "YOUR_AZURE_CLIENT_ID"

# Azure AD User Credentials (for ROPC flow)
$env:LCC_AZURE_USERNAME = "your.name@cps.gov.uk"
$env:LCC_AZURE_PASSWORD = "your-azure-password"

# CMS Credentials
$env:LCC_CMS_USERNAME = "YourName.CIN3"
$env:LCC_CMS_PASSWORD = "your-cms-password"

# Egress Configuration
$env:LCC_EGRESS_BASE_URL = "https://your-instance.egresscloud.com"
$env:LCC_EGRESS_SERVICE_ACCOUNT_AUTH = "YOUR_BASE64_ENCODED_SERVICE_ACCOUNT_CREDENTIALS"
$env:LCC_EGRESS_TEMPLATE_ID = "YOUR_EGRESS_TEMPLATE_ID"
$env:LCC_EGRESS_ADMIN_ROLE_ID = "YOUR_EGRESS_ADMIN_ROLE_ID"

# Optional: Egress direct login (if not using service account)
# $env:LCC_EGRESS_USERNAME = "your-egress-username"
# $env:LCC_EGRESS_PASSWORD = "your-egress-password"
