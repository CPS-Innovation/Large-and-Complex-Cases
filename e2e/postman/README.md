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
| **Speed** | Faster — skips registration and connection setup | Slower — adds case registration and connection creation on top of default-mode timing |
| **Use case** | Day-to-day testing, CI/CD | Testing case registration flow |

The four journey folders are `E2E: Egress to NetApp Copy`, `E2E: Netapp to Egress Copy`, `E2E: Egress to NetApp Move`, and `E2E: Netapp Move to Egress Copy`. They run in this order so each back-direction journey finds the file its forward counterpart just deposited.

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
# Default mode, all 4 journeys
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
| `-TestsToRun` | all, copy, move, netapp-to-egress, netapp-move-to-egress | all |
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

## Execution models

The collection can be driven three ways. Pick whichever fits the workflow.

### 1. PowerShell + Newman — full E2E orchestration

`Run-E2E-Tests.ps1` is the canonical entry point and the only path with full support for both Copy and Move journeys. Each invocation:

1. Loads credentials (`secrets.config.ps1` / env vars / CLI parameters).
2. Calls `Setup-EgressWorkspaceAndUpload.ps1` to upload the Copy source (`copy-<size>-…-fileN.txt` to `1. New evidence/`) and the Move source (`move-<size>-…-fileN.txt` to `4. Served Evidence/`) into Egress.
3. Materialises `LCCTestEnvironment_updated.postman_environment.json` and `LCCUserJourneyTests.postman_collection_updated.json` with the upload outputs spliced in.
4. Runs the `0. E2E Setup` folder once in Newman with `--export-environment LCCTestEnvironment_post-setup.postman_environment.json`. Setup acquires auth tokens, registers the case (RegisterCase mode) or skips to the pre-existing case (default mode), and stamps a fresh `e2eRunId = {{$guid}}` for per-run path uniquification.
5. Runs each journey folder as its own Newman invocation, all sharing the post-setup env, and writes HTML reports under `newman-reports/`.

### 2. Postman Runner — replay the generated artefacts

After a `Run-E2E-Tests.ps1` pass, the `_updated` collection + `LCCTestEnvironment_post-setup.postman_environment.json` can be imported into Postman; pressing **Run** on the collection executes all four journey folders end-to-end. The Setup folder re-runs and re-stamps `e2eRunId` on every Runner pass, so destination paths are per-run unique (see "Replay safety" below).

| Postman Runner can ✓ | Postman Runner can't ✗ |
|---|---|
| Run all four journeys in one pass after an initial PowerShell run. | Provision an Egress workspace or upload sources — that's `Setup-EgressWorkspaceAndUpload.ps1`. |
| Replay the Copy journeys twice in a row against the same imported env (per-run sub-folders). | Replay the Move journey twice without re-running PowerShell — see below. |
| Pick a subset of folders via the folder filter in the Runner UI. | Substitute for `Run-E2E-Tests.ps1` for full E2E coverage. |

### 3. PowerShell + Newman — single-journey subsets

`-TestsToRun copy | move | netapp-to-egress | netapp-move-to-egress` runs only the named journey. The orchestrator still runs the Setup folder once. The two back-direction journeys (`netapp-to-egress`, `netapp-move-to-egress`) depend on their forward counterparts having deposited a file in NetApp; running them standalone against a fresh workspace fails fast with a clear "run forward first" message rather than silently transferring a file that doesn't exist.

---

## Collection internals

### `0. E2E Setup` folder

Runs once at the start of every top-level execution (Newman invocation, Postman Runner pass, or single-journey subset). It:

- Acquires the Azure AD token, LCC API token, and CMS session.
- In RegisterCase mode, registers the case and creates the Egress + NetApp connections. In default mode, those requests skip themselves and the pre-existing `defaultCaseId` / `defaultCaseUrn` are copied into the active `caseId` / `caseUrn`.
- Stamps `e2eRunId = {{$guid}}` (consumed by the per-run uniquify blocks in every journey folder).

`Run-E2E-Tests.ps1` exports the resulting env to `LCCTestEnvironment_post-setup.postman_environment.json` so the journey folders can share it.

### Per-journey file-set variables

Each forward journey reads its own disjoint source set so Move can't accidentally select a Copy file (or vice versa):

| Variables | Used by | Source |
|---|---|---|
| `egressCopyFileName` / `egressCopyFileNames` / `egressCopyFileId` / `egressCopyFileIds` / `egressCopyFileCount` | `E2E: Egress to NetApp Copy` (`[ENC]`) | Uploaded to `1. New evidence/` |
| `egressMoveFileName` / `egressMoveFileNames` / `egressMoveFileId` / `egressMoveFileIds` / `egressMoveFileCount` | `E2E: Egress to NetApp Move` (`[ENM]`) | Uploaded to `4. Served Evidence/` |

`[ENC]/[ENM] 7b. List Files in Folder` hardcodes the prefix per folder and copies the right values into the generic `egressFile*` variables the rest of the journey reads — no `pm.execution.location` sniffing.

### Replay safety

The collection is designed so that **importing the generated env and pressing Run a second time succeeds without re-running PowerShell** — with one caveat for the Move journeys.

How it works:

- `0. E2E Setup / 1. Get Azure AD Token` stamps a fresh `e2eRunId` (`{{$guid}}`) on every pass.
- Each journey folder reads `e2eRunId` and rewrites `netappFolderPath`, `netappMoveFolderPath`, and `egressDestinationFolder` to a per-run sub-folder (e.g. `Automation-Testing/e2e-run-<id>/`).
- A second Runner pass re-runs Setup, stamps a new `e2eRunId`, and writes to a new sub-folder — no `FileExists` collision with the prior pass.

**Move journey limitation (by design).** A second Runner pass cannot complete `E2E: Egress to NetApp Move` (or its back-direction copy) without re-uploading the Move source. The backend deletes the Egress source on Move (`TransferOrchestrator.cs:243-248`), so on the second pass the configured `egressMoveFileName` is no longer in `4. Served Evidence/` and the strict assertion in `[ENM] 7b. List Files in Folder` fails loudly with the configured file name. To replay the Move journey, re-run `Run-E2E-Tests.ps1` (the upload step re-stages a fresh Move source); the Copy journeys remain replay-safe with or without the re-upload.

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
