# LCC E2E Test Suite

Automated end-to-end tests for the Large and Complex Cases (LCC) file transfer system.

## Quick Start

### 1. Setup

```powershell
# Copy the secrets template
Copy-Item secrets.config.template.ps1 secrets.config.ps1

# Copy the environment template
Copy-Item LCCTestEnvironment.postman_environment.template.json LCCTestEnvironment.postman_environment.json

# Edit with your credentials
notepad secrets.config.ps1
```

### 2. Run Tests

The test suite supports two run modes:

```powershell
# Default mode: uses existing case, skips registration and connection setup
.\Run-E2E-Tests.ps1 -SizeMB 100

# RegisterCase mode: registers a new case and creates connections
.\Run-E2E-Tests.ps1 -SizeMB 100 -RegisterCase
```

The scripts automatically load credentials from `secrets.config.ps1`.

### Run Modes

| | Default Mode | RegisterCase Mode |
|---|---|---|
| **Command** | `.\Run-E2E-Tests.ps1 -SizeMB 100` | `.\Run-E2E-Tests.ps1 -SizeMB 100 -RegisterCase` |
| **Case** | Pre-existing case | Registers one case on the first folder, reuses it for remaining folders |
| **Workspace** | Uploads to existing workspace | Creates new workspace |
| **Connections** | Skipped (already exist) | Created (Egress + NetApp) |
| **Speed** | Approx. 4 min (all 3 tests) | Approx. 6 min (all 3 tests) |
| **Use case** | Day-to-day testing, CI/CD | Testing case registration flow |

---

## Configuration

### Option A: Local Development (secrets.config.ps1)

Create `secrets.config.ps1` and the environment file from their templates:

```powershell
Copy-Item secrets.config.template.ps1 secrets.config.ps1
Copy-Item LCCTestEnvironment.postman_environment.template.json LCCTestEnvironment.postman_environment.json
```

Edit `secrets.config.ps1` with your values. See `secrets.config.template.ps1` for the full list of variables and inline comments.

### Option B: CI/CD Pipeline (Environment Variables)

Set these environment variables in your pipeline:

| Variable | Description | Required |
|----------|-------------|----------|
| `LCC_TENANT_ID` | Azure AD Tenant ID | Yes |
| `LCC_REGISTER_CASE_CLIENT_ID` | Client ID for Case Registration API | Yes |
| `LCC_API_ID` | LCC API Application ID (used in token scope) | Yes |
| `LCC_API_CLIENT_SECRET` | LCC API Client Secret (confidential client flows) | Yes |
| `LCC_AZURE_USERNAME` | Azure AD email | Yes |
| `LCC_AZURE_PASSWORD` | Azure AD password | Yes |
| `LCC_CMS_USERNAME` | CMS username (e.g. Name.CIN3) | Yes |
| `LCC_CMS_PASSWORD` | CMS password | Yes |
| `LCC_DDEI_ACCESS_KEY` | DDEI API access key | Yes |
| `LCC_BASE_URL` | LCC API base URL | Yes |
| `LCC_CASE_API_BASE_URL` | Case Management API base URL | Yes |
| `LCC_DDEI_BASE_URL` | DDEI API base URL | Yes |
| `LCC_EGRESS_BASE_URL` | Egress API URL | Yes |
| `LCC_EGRESS_SERVICE_ACCOUNT_AUTH` | Base64 service account auth | Yes |
| `LCC_EGRESS_TEMPLATE_ID` | Egress template ID | No (has default) |
| `LCC_EGRESS_ADMIN_ROLE_ID` | Egress admin role ID | No (has default) |
| `LCC_DEFAULT_CASE_ID` | Pre-existing case ID (default mode only) | Conditional |
| `LCC_DEFAULT_CASE_URN` | Pre-existing case URN (default mode only) | Conditional |
| `LCC_DEFAULT_WORKSPACE_ID` | Pre-existing workspace ID (default mode only) | Conditional |
| `LCC_DEFAULT_WORKSPACE_NAME` | Pre-existing workspace name (default mode only) | Conditional |

Conditional variables are required when running in default mode (without `-RegisterCase`). They are not needed for RegisterCase mode.

#### Azure DevOps Example

```yaml
variables:
  - group: LCC-Test-Secrets

steps:
  - task: PowerShell@2
    displayName: 'Run E2E Tests'
    inputs:
      filePath: '$(Build.SourcesDirectory)/e2e/postman/Run-E2E-Tests.ps1'
      arguments: '-SizeMB 100 -TestsToRun all'
    env:
      LCC_TENANT_ID: $(LCC_TENANT_ID)
      LCC_API_ID: $(LCC_API_ID)
      LCC_API_CLIENT_SECRET: $(LCC_API_CLIENT_SECRET)
      LCC_REGISTER_CASE_CLIENT_ID: $(LCC_REGISTER_CASE_CLIENT_ID)
      LCC_AZURE_USERNAME: $(LCC_AZURE_USERNAME)
      LCC_AZURE_PASSWORD: $(LCC_AZURE_PASSWORD)
      LCC_CMS_USERNAME: $(LCC_CMS_USERNAME)
      LCC_CMS_PASSWORD: $(LCC_CMS_PASSWORD)
      LCC_DDEI_ACCESS_KEY: $(LCC_DDEI_ACCESS_KEY)
      LCC_BASE_URL: $(LCC_BASE_URL)
      LCC_CASE_API_BASE_URL: $(LCC_CASE_API_BASE_URL)
      LCC_DDEI_BASE_URL: $(LCC_DDEI_BASE_URL)
      LCC_EGRESS_BASE_URL: $(LCC_EGRESS_BASE_URL)
      LCC_EGRESS_SERVICE_ACCOUNT_AUTH: $(LCC_EGRESS_SERVICE_ACCOUNT_AUTH)
```

### Option C: Command Line Override

Pass credentials directly (useful for one-off runs):

```powershell
.\Run-E2E-Tests.ps1 -SizeMB 100 `
    -AzureUsername "your.name@domain.com" `
    -AzurePassword "your-password" `
    -CmsUsername "YourName.CIN3" `
    -CmsPassword "your-cms-password"
```

Priority order: CLI parameters > Environment variables > secrets.config.ps1

---

## Available Scripts

### Run-E2E-Tests.ps1

Main test orchestrator. Uploads files to Egress and runs Newman tests.

```powershell
# Default mode, all 3 tests
.\Run-E2E-Tests.ps1 -SizeMB 100

# Default mode, specific test only
.\Run-E2E-Tests.ps1 -SizeMB 100 -TestsToRun copy

# RegisterCase mode, full flow with new case registration
.\Run-E2E-Tests.ps1 -SizeMB 100 -RegisterCase

# RegisterCase mode, specific test only
.\Run-E2E-Tests.ps1 -SizeMB 100 -RegisterCase -TestsToRun copy

# Multiple files
.\Run-E2E-Tests.ps1 -SizeMB 100 -FileCount 3

# Skip upload, reuse existing workspace
.\Run-E2E-Tests.ps1 -SkipUpload -EgressWorkspaceId "abc123" -EgressWorkspaceName "WORKSPACE-NAME"
```

Parameters:

| Parameter | Description | Default |
|-----------|-------------|---------|
| `-SizeMB` | File size in MB | - |
| `-SizeGB` | File size in GB | 1 |
| `-FileCount` | Number of files to upload | 1 |
| `-TestsToRun` | all, copy, move, netapp-to-egress | all |
| `-RegisterCase` | Register a new case. Without this flag, uses pre-existing case. | false |
| `-SkipUpload` | Skip file upload step | false |
| `-StopOnFailure` | Stop on first test failure | false |

### Setup-EgressWorkspaceAndUpload.ps1

Creates an Egress workspace and uploads test files.

```powershell
# Create workspace with 100MB file
.\Setup-EgressWorkspaceAndUpload.ps1 -SizeMB 100

# Multiple files
.\Setup-EgressWorkspaceAndUpload.ps1 -SizeMB 500 -FileCount 5

# Workspace only, no upload
.\Setup-EgressWorkspaceAndUpload.ps1 -SkipUpload
```

---

## Files

| File | Description | Commit |
|------|-------------|--------|
| `Run-E2E-Tests.ps1` | Main test runner | Yes |
| `Setup-EgressWorkspaceAndUpload.ps1` | Egress setup utilities | Yes |
| `secrets.config.template.ps1` | Template for local secrets | Yes |
| `secrets.config.ps1` | Your local secrets | No |
| `LCCUserJourneyTests.postman_collection.json` | Postman test collection | Yes |
| `LCCTestEnvironment.postman_environment.template.json` | Environment template | Yes |
| `LCCTestEnvironment.postman_environment.json` | Your local environment | No |
| `newman-reports/` | Newman HTML/JSON test reports | No |
| `LCCTestEnvironment_updated.*` | Auto-generated updated env files | No |
| `README.md` | This file | Yes |

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

### curl.exe Not Found

The upload script requires `curl.exe` (the Windows native binary), not the PowerShell `curl` alias (`Invoke-WebRequest`). `curl.exe` is included with Windows 10 1803+ and Windows Server 2019+. If missing, install it from https://curl.se/windows/.

### Missing Configuration

If you see "Missing required configuration" errors:

1. Check that `secrets.config.ps1` exists and has all values filled in.
2. Verify environment variables are set (for CI/CD).
3. Pass credentials via command line.

### View Available Parameters

```powershell
Get-Help .\Run-E2E-Tests.ps1 -Full
Get-Help .\Setup-EgressWorkspaceAndUpload.ps1 -Full
```

---

## Security

Do not commit files containing credentials.

- `secrets.config.ps1` is gitignored.
- `*.postman_environment` files (except templates) are gitignored.
- Use Azure Key Vault or pipeline variable groups for CI/CD secrets.
- Rotate credentials on a regular basis.
