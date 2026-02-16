# LCC E2E Test Suite

Automated end-to-end tests for the Legal Case Collaboration (LCC) file transfer system.

## Quick Start

### 1. Clone and Setup

```powershell
# Copy the secrets template
Copy-Item secrets.config.template.ps1 secrets.config.ps1

# Edit with your credentials
notepad secrets.config.ps1
```

### 2. Run Tests

```powershell
# Run with 100MB test file
.\Run-E2E-Tests.ps1 -SizeMB 100

# Run specific test only
.\Run-E2E-Tests.ps1 -SizeMB 100 -TestsToRun copy
```

That's it! The scripts automatically load credentials from `secrets.config.ps1`.

---

## Configuration

### Option A: Local Development (secrets.config.ps1)

Create `secrets.config.ps1` from the template:

```powershell
Copy-Item secrets.config.template.ps1 secrets.config.ps1
```

Edit the file with your values:

```powershell
# Azure AD Configuration
$env:LCC_TENANT_ID = "your-tenant-id"
$env:LCC_CLIENT_ID = "your-client-id"

# Azure AD Credentials
$env:LCC_AZURE_USERNAME = "your.name@cps.gov.uk"
$env:LCC_AZURE_PASSWORD = "your-azure-password"

# CMS Credentials
$env:LCC_CMS_USERNAME = "YourName.CIN3"
$env:LCC_CMS_PASSWORD = "your-cms-password"

# Egress Configuration
$env:LCC_EGRESS_BASE_URL = "https://cps-qa.egresscloud.com"
$env:LCC_EGRESS_SERVICE_ACCOUNT_AUTH = "your-base64-encoded-credentials"
```

### Option B: CI/CD Pipeline (Environment Variables)

Set these environment variables in your pipeline:

| Variable | Description | Required |
|----------|-------------|----------|
| `LCC_TENANT_ID` | Azure AD Tenant ID | Yes |
| `LCC_CLIENT_ID` | Azure AD Client/App ID | Yes |
| `LCC_AZURE_USERNAME` | Azure AD email | Yes |
| `LCC_AZURE_PASSWORD` | Azure AD password | Yes |
| `LCC_CMS_USERNAME` | CMS username (e.g., Name.CIN3) | Yes |
| `LCC_CMS_PASSWORD` | CMS password | Yes |
| `LCC_EGRESS_BASE_URL` | Egress API URL | Yes |
| `LCC_EGRESS_SERVICE_ACCOUNT_AUTH` | Base64 service account auth | Yes |
| `LCC_EGRESS_TEMPLATE_ID` | Egress template ID | No (has default) |
| `LCC_EGRESS_ADMIN_ROLE_ID` | Egress admin role ID | No (has default) |

#### Azure DevOps Example

```yaml
variables:
  - group: LCC-Test-Secrets  # Variable group with secrets

steps:
  - task: PowerShell@2
    displayName: 'Run E2E Tests'
    inputs:
      filePath: '$(Build.SourcesDirectory)/Run-E2E-Tests.ps1'
      arguments: '-SizeMB 100 -TestsToRun all'
    env:
      LCC_TENANT_ID: $(LCC_TENANT_ID)
      LCC_CLIENT_ID: $(LCC_CLIENT_ID)
      LCC_AZURE_USERNAME: $(LCC_AZURE_USERNAME)
      LCC_AZURE_PASSWORD: $(LCC_AZURE_PASSWORD)
      LCC_CMS_USERNAME: $(LCC_CMS_USERNAME)
      LCC_CMS_PASSWORD: $(LCC_CMS_PASSWORD)
      LCC_EGRESS_BASE_URL: $(LCC_EGRESS_BASE_URL)
      LCC_EGRESS_SERVICE_ACCOUNT_AUTH: $(LCC_EGRESS_SERVICE_ACCOUNT_AUTH)
```

### Option C: Command Line Override

Pass credentials directly (useful for one-off runs):

```powershell
.\Run-E2E-Tests.ps1 -SizeMB 100 `
    -AzureUsername "your.name@cps.gov.uk" `
    -AzurePassword "your-password" `
    -CmsUsername "YourName.CIN3" `
    -CmsPassword "your-cms-password"
```

**Priority Order:** CLI parameters > Environment variables > secrets.config.ps1

---

## Available Scripts

### Run-E2E-Tests.ps1

Main test orchestrator that uploads files and runs Newman tests.

```powershell
# Basic usage
.\Run-E2E-Tests.ps1 -SizeMB 100

# Multiple files
.\Run-E2E-Tests.ps1 -SizeMB 100 -FileCount 3

# Specific test only
.\Run-E2E-Tests.ps1 -SizeMB 100 -TestsToRun copy

# Skip upload, use existing workspace
.\Run-E2E-Tests.ps1 -SkipUpload -EgressWorkspaceId "abc123" -EgressWorkspaceName "AUTOMATION-TESTING5"
```

**Parameters:**

| Parameter | Description | Default |
|-----------|-------------|---------|
| `-SizeMB` | File size in MB | - |
| `-SizeGB` | File size in GB | 1 |
| `-FileCount` | Number of files to upload | 1 |
| `-TestsToRun` | all, copy, move, netapp-to-egress | all |
| `-SkipUpload` | Skip file upload step | false |
| `-StopOnFailure` | Stop on first failure | false |

### Setup-EgressWorkspaceAndUpload.ps1

Creates Egress workspace and uploads test files.

```powershell
# Create workspace with 100MB file
.\Setup-EgressWorkspaceAndUpload.ps1 -SizeMB 100

# Multiple files
.\Setup-EgressWorkspaceAndUpload.ps1 -SizeMB 500 -FileCount 5

# Workspace only (no upload)
.\Setup-EgressWorkspaceAndUpload.ps1 -SkipUpload
```

---

## Files

| File | Description | Commit? |
|------|-------------|---------|
| `Run-E2E-Tests.ps1` | Main test runner | ✅ Yes |
| `Setup-EgressWorkspaceAndUpload.ps1` | Egress setup utilities | ✅ Yes |
| `Load-Config.psm1` | Configuration loader module | ✅ Yes |
| `secrets.config.template.ps1` | Template for local secrets | ✅ Yes |
| `secrets.config.ps1` | Your local secrets | ❌ No |
| `LCCUserJourneyTests_fixed.postman_collection` | Postman test collection | ✅ Yes |
| `LCCTestEnvironment.postman_environment.template` | Environment template | ✅ Yes |
| `LCCTestEnvironment.postman_environment` | Your local environment | ❌ No |
| `.gitignore` | Git ignore rules | ✅ Yes |
| `README.md` | This file | ✅ Yes |

---

## Troubleshooting

### Execution Policy Error

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
```

### Newman Not Found

```powershell
npm install -g newman
npm install -g newman-reporter-htmlextra
```

### Missing Configuration

If you see "Missing required configuration" errors:

1. Check that `secrets.config.ps1` exists and has all values filled in
2. Or verify environment variables are set (for CI/CD)
3. Or pass credentials via command line

### View Available Parameters

```powershell
Get-Help .\Run-E2E-Tests.ps1 -Full
Get-Help .\Setup-EgressWorkspaceAndUpload.ps1 -Full
```

---

## Security Notes

⚠️ **Never commit files containing actual credentials**

- `secrets.config.ps1` is gitignored
- `*.postman_environment` files (except templates) are gitignored
- Use Azure Key Vault or pipeline secrets for CI/CD
- Rotate credentials regularly
