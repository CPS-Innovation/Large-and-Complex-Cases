<#
.SYNOPSIS
    Run E2E LACC tests: Upload file to Egress, then run Newman tests sequentially.

.DESCRIPTION
    This script:
    1. Runs Setup-EgressWorkspaceAndUpload.ps1 to create workspace and upload file
    2. Extracts egressWorkspaceId and egressWorkspaceName from output
    3. Updates the Postman environment AND collection files with new values
    4. Runs Newman E2E tests in sequence:
       - E2E: Egress to NetApp Copy
       - E2E: Egress to NetApp Move
       - E2E: Netapp to Egress Copy

    Configuration is loaded from:
    - secrets.config.ps1 (for local development)
    - Environment variables (for CI/CD pipelines)
    - Command line parameters (highest priority)

.PARAMETER SizeGB
    File size in GB (default: 1)

.PARAMETER SizeMB
    File size in MB (alternative to SizeGB)

.PARAMETER ChunkSizeMB
    Chunk size in MB (default: 50)

.PARAMETER FileCount
    Number of files to upload (default: 1, max: 100)

.PARAMETER CollectionPath
    Path to Postman collection JSON file

.PARAMETER EnvironmentPath
    Path to Postman environment JSON file

.PARAMETER SkipUpload
    Skip the upload step and use existing workspace variables

.PARAMETER TestsToRun
    Which tests to run: "all", "copy", "move", "netapp-to-egress" (default: all)

.EXAMPLE
    .\Run-E2E-Tests.ps1 -SizeMB 100
    # Uses credentials from secrets.config.ps1 or environment variables

.EXAMPLE
    .\Run-E2E-Tests.ps1 -SizeMB 100 -AzureUsername "user@cps.gov.uk" -AzurePassword "pass"
    # Override credentials via command line
#>

param(
    [Parameter(Mandatory=$false)]
    [double]$SizeGB = 1,
    
    [Parameter(Mandatory=$false)]
    [double]$SizeMB = 0,
    
    [Parameter(Mandatory=$false)]
    [double]$ChunkSizeMB = 50,

    [Parameter(Mandatory=$false)]
    [int]$FileCount = 1,
    
    [Parameter(Mandatory=$false)]
    [string]$CollectionPath = ".\LCCUserJourneyTests.postman_collection.json",
    
    [Parameter(Mandatory=$false)]
    [string]$EnvironmentPath = ".\LCCTestEnvironment.postman_environment.template.json",
    
    [Parameter(Mandatory=$false)]
    [string]$UploadScriptPath = ".\Setup-EgressWorkspaceAndUpload.ps1",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipUpload,
    
    [Parameter(Mandatory=$false)]
    [string]$EgressWorkspaceId = "",
    
    [Parameter(Mandatory=$false)]
    [string]$EgressWorkspaceName = "",

    [Parameter(Mandatory=$false)]
    [ValidateSet("all", "copy", "move", "netapp-to-egress")]
    [string]$TestsToRun = "all",

    [Parameter(Mandatory=$false)]
    [switch]$StopOnFailure,

    # Authentication credentials (override config/env vars)
    [Parameter(Mandatory=$false)]
    [string]$AzureUsername = "",

    [Parameter(Mandatory=$false)]
    [string]$AzurePassword = "",

    [Parameter(Mandatory=$false)]
    [string]$CmsUsername = "",

    [Parameter(Mandatory=$false)]
    [string]$CmsPassword = "",

    # When set, registers a new case. Default: skip registration and use existing case.
    [Parameter(Mandatory=$false)]
    [switch]$RegisterCase
)

# ============================================================
# LOAD CONFIGURATION
# ============================================================
$ErrorActionPreference = "Continue"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $ScriptDir) { $ScriptDir = Get-Location }

# Try to load secrets file
$SecretsFile = Join-Path $ScriptDir "secrets.config.ps1"
if (Test-Path $SecretsFile) {
    Write-Host "[CONFIG] Loading from secrets.config.ps1" -ForegroundColor Cyan
    . $SecretsFile
} else {
    Write-Host "[CONFIG] Using environment variables (no secrets.config.ps1 found)" -ForegroundColor Yellow
}

# Load config with priority: CLI params > env vars > defaults
$Config = @{
    BaseUrl       = if ($env:LCC_API_BASE_URL) { $env:LCC_API_BASE_URL } else { "" }
    TenantId      = if ($env:LCC_TENANT_ID) { $env:LCC_TENANT_ID } else { "" }
    RegisterCaseClientId = if ($env:LCC_REGISTER_CASE_CLIENT_ID) { $env:LCC_REGISTER_CASE_CLIENT_ID } else { "" }
    AzureUsername = if ($AzureUsername) { $AzureUsername } elseif ($env:LCC_AZURE_USERNAME) { $env:LCC_AZURE_USERNAME } else { "" }
    AzurePassword = if ($AzurePassword) { $AzurePassword } elseif ($env:LCC_AZURE_PASSWORD) { $env:LCC_AZURE_PASSWORD } else { "" }
    CmsUsername   = if ($CmsUsername) { $CmsUsername } elseif ($env:LCC_CMS_USERNAME) { $env:LCC_CMS_USERNAME } else { "" }
    CmsPassword   = if ($CmsPassword) { $CmsPassword } elseif ($env:LCC_CMS_PASSWORD) { $env:LCC_CMS_PASSWORD } else { "" }
    DdeiBaseUrl   = if ($env:LCC_DDEI_BASE_URL) { $env:LCC_DDEI_BASE_URL } else { "" }
    DdeiAccessKey = if ($env:LCC_DDEI_ACCESS_KEY) { $env:LCC_DDEI_ACCESS_KEY } else { "" }
    BaseUrl       = if ($env:LCC_BASE_URL) { $env:LCC_BASE_URL } else { "" }
    CaseApiBaseUrl = if ($env:LCC_CASE_API_BASE_URL) { $env:LCC_CASE_API_BASE_URL } else { "" }
    EgressBaseUrl = if ($env:LCC_EGRESS_BASE_URL) { $env:LCC_EGRESS_BASE_URL } else { "" }
    DdeiBaseUrl   = if ($env:LCC_DDEI_BASE_URL) { $env:LCC_DDEI_BASE_URL } else { "" }
    LccApiId      = if ($env:LCC_API_ID) { $env:LCC_API_ID } else { "" }
    LccApiClientSecret = if ($env:LCC_API_CLIENT_SECRET) { $env:LCC_API_CLIENT_SECRET } else { "" }
    DefaultCaseId      = if ($env:LCC_DEFAULT_CASE_ID) { $env:LCC_DEFAULT_CASE_ID } else { "" }
    DefaultCaseUrn     = if ($env:LCC_DEFAULT_CASE_URN) { $env:LCC_DEFAULT_CASE_URN } else { "" }
    DefaultWorkspaceId   = if ($env:LCC_DEFAULT_WORKSPACE_ID) { $env:LCC_DEFAULT_WORKSPACE_ID } else { "" }
    DefaultWorkspaceName = if ($env:LCC_DEFAULT_WORKSPACE_NAME) { $env:LCC_DEFAULT_WORKSPACE_NAME } else { "" }
}

# Validate required config
$missingConfig = @()
if (-not $Config.BaseUrl) { $missingConfig += "LCC_API_BASE_URL" }
if (-not $Config.TenantId) { $missingConfig += "LCC_TENANT_ID" }
if (-not $Config.LccApiId) { $missingConfig += "LCC_API_ID" }
if (-not $Config.AzureUsername) { $missingConfig += "LCC_AZURE_USERNAME (or -AzureUsername)" }
if (-not $Config.AzurePassword) { $missingConfig += "LCC_AZURE_PASSWORD (or -AzurePassword)" }
if (-not $Config.CmsUsername) { $missingConfig += "LCC_CMS_USERNAME (or -CmsUsername)" }
if (-not $Config.CmsPassword) { $missingConfig += "LCC_CMS_PASSWORD (or -CmsPassword)" }
if (-not $Config.DdeiAccessKey) { $missingConfig += "LCC_DDEI_ACCESS_KEY" }
if (-not $Config.BaseUrl) { $missingConfig += "LCC_BASE_URL" }
if (-not $Config.CaseApiBaseUrl) { $missingConfig += "LCC_CASE_API_BASE_URL" }
if (-not $Config.EgressBaseUrl) { $missingConfig += "LCC_EGRESS_BASE_URL" }
if (-not $Config.DdeiBaseUrl) { $missingConfig += "LCC_DDEI_BASE_URL" }

# Default mode requires pre-existing case and workspace config
if (-not $RegisterCase) {
    if (-not $Config.DefaultCaseId) { $missingConfig += "LCC_DEFAULT_CASE_ID (required for default mode)" }
    if (-not $Config.DefaultCaseUrn) { $missingConfig += "LCC_DEFAULT_CASE_URN (required for default mode)" }
    if (-not $Config.DefaultWorkspaceId) { $missingConfig += "LCC_DEFAULT_WORKSPACE_ID (required for default mode)" }
    if (-not $Config.DefaultWorkspaceName) { $missingConfig += "LCC_DEFAULT_WORKSPACE_NAME (required for default mode)" }
}

# Warn about optional config that may cause issues if missing
if (-not $Config.LccApiClientSecret) {
    Write-Host "[WARNING] LCC_API_CLIENT_SECRET is not set - client credential auth flows will not work" -ForegroundColor Yellow
}

if ($missingConfig.Count -gt 0) {
    Write-Host ""
    Write-Host "ERROR: Missing required configuration:" -ForegroundColor Red
    $missingConfig | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Yellow
    Write-Host "  1. Create secrets.config.ps1 from secrets.config.template.ps1"
    Write-Host "  2. Set environment variables (for CI/CD)"
    Write-Host "  3. Pass credentials via command line parameters"
    Write-Host ""
    exit 1
}

Write-Host "[CONFIG] Azure User: $($Config.AzureUsername)" -ForegroundColor Gray
Write-Host "[CONFIG] CMS User:   $($Config.CmsUsername)" -ForegroundColor Gray

$Timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$ReportsDir = Join-Path $ScriptDir "newman-reports"

# E2E test folders mapping
$E2EFolderMap = @{
    "copy" = "E2E: Egress to NetApp Copy"
    "move" = "E2E: Egress to NetApp Move"
    "netapp-to-egress" = "E2E: Netapp to Egress Copy"
}

# ============================================================
# FUNCTIONS
# ============================================================
function Write-Header {
    param([string]$Text)
    Write-Host ""
    Write-Host "========================================================" -ForegroundColor Cyan
    Write-Host "  $Text" -ForegroundColor Cyan
    Write-Host "========================================================" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Success {
    param([string]$Text)
    Write-Host "[OK] $Text" -ForegroundColor Green
}

function Write-Err {
    param([string]$Text)
    Write-Host "[ERROR] $Text" -ForegroundColor Red
}

function Write-Info {
    param([string]$Text)
    Write-Host "[INFO] $Text" -ForegroundColor Yellow
}

function Check-Newman {
    $newman = Get-Command newman -ErrorAction SilentlyContinue
    $htmlExtra = Get-Command newman-reporter-htmlextra -ErrorAction SilentlyContinue
    if (-not $newman) {
        Write-Err "Newman is not installed!"
        Write-Host "Install Newman with:" -ForegroundColor Yellow
        Write-Host "  npm install -g newman" -ForegroundColor White
        Write-Host "  npm install -g newman-reporter-htmlextra" -ForegroundColor White
        return $false
    } elseif (-not $htmlExtra) {
        Write-Err "Newman reporter HTML Extra is not installed!"
        Write-Host "Install htmlextra reporter with:" -ForegroundColor Yellow
        Write-Host "  npm install -g newman-reporter-htmlextra" -ForegroundColor White
        return $false
    }
    Write-Success "Newman found: $($newman.Source)"
    return $true
}

function Update-EnvironmentFile {
    param(
        [string]$EnvironmentPath,
        [hashtable]$Variables,
        [string]$OutputDir
    )
    
    Write-Info "Updating environment file..."
    
    $envDir = if ($OutputDir) { $OutputDir } else { Split-Path -Parent $EnvironmentPath }
    if (-not $envDir) { $envDir = Get-Location }
    $updatedEnvPath = Join-Path $envDir "LCCTestEnvironment_updated.postman_environment.json"
    
    if (Test-Path $updatedEnvPath) {
        Write-Host "  Using existing updated environment file" -ForegroundColor Gray
        $envContent = Get-Content $updatedEnvPath -Raw | ConvertFrom-Json
    } else {
        Write-Host "  Creating new updated environment file from original" -ForegroundColor Gray
        $envContent = Get-Content $EnvironmentPath -Raw | ConvertFrom-Json
    }
    
    foreach ($key in $Variables.Keys) {
        $value = $Variables[$key]
        $found = $false
        
        for ($i = 0; $i -lt $envContent.values.Count; $i++) {
            if ($envContent.values[$i].key -eq $key) {
                $envContent.values[$i].value = $value
                $found = $true
                Write-Host "  Updated: $key" -ForegroundColor Gray
                break
            }
        }
        
        if (-not $found) {
            $envContent.values += @{
                key = $key
                value = $value
                type = "default"
                enabled = $true
            }
            Write-Host "  Added: $key" -ForegroundColor Gray
        }
    }
    
    $envContent | ConvertTo-Json -Depth 100 | Set-Content $updatedEnvPath -Encoding UTF8
    
    Write-Success "Environment saved to: $updatedEnvPath"
    return $updatedEnvPath
}

function Update-CollectionVariables {
    param(
        [string]$CollectionPath,
        [hashtable]$Variables,
        [string]$OutputDir
    )

    Write-Info "Updating collection variables..."

    $collDir = if ($OutputDir) { $OutputDir } else { Split-Path -Parent $CollectionPath }
    if (-not $collDir) { $collDir = Get-Location }
    $collFilename = [System.IO.Path]::GetFileNameWithoutExtension($CollectionPath)
    $collExtension = [System.IO.Path]::GetExtension($CollectionPath)
    $updatedCollPath = Join-Path $collDir "${collFilename}_updated${collExtension}"

    if (Test-Path $updatedCollPath) {
        Write-Host "  Using existing updated collection file" -ForegroundColor Gray
        $collection = Get-Content $updatedCollPath -Raw | ConvertFrom-Json
    } else {
        Write-Host "  Creating new updated collection file from original" -ForegroundColor Gray
        $collection = Get-Content $CollectionPath -Raw | ConvertFrom-Json
    }

    if (-not $collection.variable) {
        $collection | Add-Member -NotePropertyName "variable" -NotePropertyValue @()
    }

    foreach ($key in $Variables.Keys) {
        $value = $Variables[$key]
        $found = $false

        for ($i = 0; $i -lt $collection.variable.Count; $i++) {
            if ($collection.variable[$i].key -eq $key) {
                $collection.variable[$i].value = $value
                $found = $true
                Write-Host "  Updated: $key" -ForegroundColor Gray
                break
            }
        }

        if (-not $found) {
            $collection.variable += @{
                key = $key
                value = $value
                type = "default"
            }
            Write-Host "  Added: $key" -ForegroundColor Gray
        }
    }

    $collection | ConvertTo-Json -Depth 100 | Set-Content $updatedCollPath -Encoding UTF8

    Write-Success "Collection saved to: $updatedCollPath"
    return $updatedCollPath
}

function Run-NewmanFolder {
    param(
        [string]$CollectionPath,
        [string]$FolderName,
        [string]$EnvironmentPath,
        [string]$ReportsDir,
        [string]$ExportEnvironmentPath = ""
    )
    
    Write-Header "Running: $FolderName"
    
    if (-not (Test-Path $ReportsDir)) {
        New-Item -ItemType Directory -Path $ReportsDir -Force | Out-Null
    }
    
    $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
    $reportName = ($FolderName -replace '[^a-zA-Z0-9]', '_') + "_$timestamp"
    $htmlReport = Join-Path $ReportsDir "$reportName.html"
    $jsonReport = Join-Path $ReportsDir "$reportName.json"
    
    Write-Host "Folder: $FolderName" -ForegroundColor Gray
    
    $fullCollectionPath = (Resolve-Path $CollectionPath -ErrorAction SilentlyContinue).Path
    if (-not $fullCollectionPath) {
        Write-Err "Collection file not found: $CollectionPath"
        return @{ Success = $false; Duration = [TimeSpan]::Zero; ExitCode = 1 }
    }
    
    $fullEnvPath = (Resolve-Path $EnvironmentPath -ErrorAction SilentlyContinue).Path
    if (-not $fullEnvPath) {
        Write-Err "Environment file not found: $EnvironmentPath"
        return @{ Success = $false; Duration = [TimeSpan]::Zero; ExitCode = 1 }
    }
    
    $startTime = Get-Date
    
    Write-Host "Starting Newman..." -ForegroundColor Cyan
    
    $fullHtmlReport = Join-Path (Resolve-Path $ReportsDir).Path "$reportName.html"
    $fullJsonReport = Join-Path (Resolve-Path $ReportsDir).Path "$reportName.json"
    
    # Build Newman arguments as array for clean conditional flags
    $newmanArgs = @(
        "run", $fullCollectionPath,
        "--folder", $FolderName,
        "--environment", $fullEnvPath,
        "--timeout-request", "120000",
        "--delay-request", "1000",
        "-r", "cli,htmlextra,json",
        "--reporter-htmlextra-export", $fullHtmlReport,
        "--reporter-htmlextra-logs",
        "--reporter-json-export", $fullJsonReport
    )
    
    if ($ExportEnvironmentPath) {
        $newmanArgs += @("--export-environment", $ExportEnvironmentPath)
        Write-Host "  Export environment to: $ExportEnvironmentPath" -ForegroundColor Gray
    }
    
    & newman @newmanArgs
    
    $exitCode = $LASTEXITCODE
    $htmlReport = $fullHtmlReport
    $jsonReport = $fullJsonReport
    
    $duration = (Get-Date) - $startTime
    
    Write-Host ""
    
    if (Test-Path $htmlReport) {
        $reportSize = (Get-Item $htmlReport).Length
        Write-Success "HTML Report saved: $htmlReport ($([Math]::Round($reportSize/1KB, 1)) KB)"
    }
    
    # Update history file
    $summaryFile = Join-Path $ReportsDir "test-history.txt"
    $summaryLine = "$timestamp | $FolderName | $(if ($exitCode -eq 0) { 'PASS' } else { 'FAIL' }) | Duration: $([Math]::Round($duration.TotalSeconds, 1))s"
    Add-Content -Path $summaryFile -Value $summaryLine
    
    if ($exitCode -eq 0) {
        Write-Success "$FolderName completed in $([Math]::Round($duration.TotalSeconds, 1))s"
        return @{ Success = $true; Duration = $duration; ReportPath = $htmlReport }
    }
    else {
        Write-Err "$FolderName failed with exit code $exitCode"
        return @{ Success = $false; Duration = $duration; ExitCode = $exitCode; ReportPath = $htmlReport }
    }
}

# ============================================================
# MAIN SCRIPT
# ============================================================
Clear-Host
Write-Header "LACC E2E TEST ORCHESTRATOR"

Write-Host "Configuration:" -ForegroundColor Cyan
Write-Host "  Collection:    $CollectionPath"
Write-Host "  Environment:   $EnvironmentPath"
Write-Host "  Upload Script: $UploadScriptPath"
Write-Host "  Tests to Run:  $TestsToRun"
Write-Host ""

# ============================================================
# PREREQUISITES CHECK
# ============================================================
Write-Info "Checking prerequisites..."

if (-not (Check-Newman)) {
    exit 1
}

if (-not (Test-Path $CollectionPath)) {
    Write-Err "Collection file not found: $CollectionPath"
    exit 1
}
Write-Success "Collection found"

if (-not (Test-Path $EnvironmentPath)) {
    Write-Err "Environment file not found: $EnvironmentPath"
    exit 1
}
Write-Success "Environment found"

New-Item -ItemType Directory -Path $ReportsDir -Force | Out-Null
Write-Success "Reports directory: $ReportsDir"

# ============================================================
# STEP 1: Upload to Egress (or use existing workspace)
# ============================================================
if (-not $SkipUpload) {
    Write-Header "STEP 1: Upload File to Egress"
    
    if (-not (Test-Path $UploadScriptPath)) {
        Write-Err "Upload script not found: $UploadScriptPath"
        exit 1
    }
    
    $uploadArgs = @{}
    if ($SizeMB -gt 0) {
        $uploadArgs["SizeMB"] = $SizeMB
    } else {
        $uploadArgs["SizeGB"] = $SizeGB
    }
    $uploadArgs["ChunkSizeMB"] = $ChunkSizeMB
    $uploadArgs["FileCount"] = $FileCount

    # Pass through CLI credential overrides to upload script
    if ($AzureUsername) { $uploadArgs["UserEmail"] = $AzureUsername }

    # Default mode: upload to existing workspace instead of creating new one
    if (-not $RegisterCase) {
        $uploadArgs["ExistingWorkspaceId"] = $Config.DefaultWorkspaceId
        $uploadArgs["WorkspaceName"] = $Config.DefaultWorkspaceName
        Write-Host "  Mode: Default (uploading to existing workspace $($Config.DefaultWorkspaceName))" -ForegroundColor Cyan
    } else {
        Write-Host "  Mode: RegisterCase (creating new workspace)" -ForegroundColor Cyan
    }

    Write-Info "Running upload..."
    Write-Host "  Size: $(if ($SizeMB -gt 0) { "$SizeMB MB" } else { "$SizeGB GB" })"
    Write-Host "  File Count: $FileCount"
    Write-Host ""

    
    $TempFolder = $env:TEMP
    if ([string]::IsNullOrWhiteSpace($TempFolder)) {
        $TempFolder = $env:TMPDIR
    }
    if ([string]::IsNullOrWhiteSpace($TempFolder)) {
        $TempFolder = "/tmp"
    }
    
    $tempOutputFile = Join-Path $TempFolder "egress_upload_output_$((Get-Date).Ticks).txt"
    
    & $UploadScriptPath @uploadArgs *>&1 | Tee-Object -FilePath $tempOutputFile
    
    Write-Host ""
    Write-Info "Parsing upload output..."
    
    $uploadLog = Get-Content $tempOutputFile -Raw
    
    # Parse output for workspace details
    $EgressWorkspaceId = ""
    $EgressWorkspaceName = ""
    $EgressFileId = ""
    $EgressFileName = ""
    $EgressFileIds = ""
    $EgressFileNames = ""
    $EgressFileCount = "1"

    # Try to parse JSON output first
    $uploadLines = $uploadLog -split "`r?`n"
    foreach ($line in $uploadLines) {
        if ($line -match '^\s*\{"workspaceId"') {
            try {
                $jsonData = $line.Trim() | ConvertFrom-Json
                $EgressWorkspaceId = $jsonData.workspaceId
                $EgressWorkspaceName = $jsonData.workspaceName
                $EgressFileCount = [string]$jsonData.fileCount

                if ($jsonData.files -and $jsonData.files.Count -gt 0) {
                    $EgressFileId = $jsonData.files[0].fileId
                    $EgressFileName = $jsonData.files[0].fileName
                    $EgressFileIds = ($jsonData.files | ForEach-Object { $_.fileId }) -join ","
                    $EgressFileNames = ($jsonData.files | ForEach-Object { $_.fileName }) -join ","
                }
                Write-Host "[JSON] Successfully parsed JSON output" -ForegroundColor Green
                break
            }
            catch {
                Write-Host "[JSON] Failed to parse: $_" -ForegroundColor Yellow
            }
        }
    }

    # Fallback: regex parsing
    if (-not $EgressWorkspaceId -and $uploadLog -match "egressWorkspaceId\s*=\s*([a-f0-9]{24})") {
        $EgressWorkspaceId = $matches[1]
    }
    if (-not $EgressWorkspaceName -and $uploadLog -match "egressWorkspaceName\s*=\s*(AUTOMATION-TESTING\d+|[A-Z0-9\-_]+)") {
        $EgressWorkspaceName = $matches[1]
    }
    if (-not $EgressFileId -and $uploadLog -match "egressFileId\s*=\s*([a-f0-9]{24})") {
        $EgressFileId = $matches[1]
    }
    if (-not $EgressFileName -and $uploadLog -match "egressFileName\s*=\s*(generated-[^\s]+\.txt|[^\s]+\.(txt|bin))") {
        $EgressFileName = $matches[1]
    }

    Remove-Item $tempOutputFile -Force -ErrorAction SilentlyContinue
    
    if (-not $EgressWorkspaceId) {
        Write-Err "Failed to extract egressWorkspaceId from upload output"
        exit 1
    }
    
    Write-Success "Upload completed!"
    Write-Host "  egressWorkspaceId:   $EgressWorkspaceId"
    Write-Host "  egressWorkspaceName: $EgressWorkspaceName"
}
else {
    Write-Header "STEP 1: Using Existing Workspace (Skip Upload)"
    
    if (-not $EgressWorkspaceId -or -not $EgressWorkspaceName) {
        Write-Err "When using -SkipUpload, provide -EgressWorkspaceId and -EgressWorkspaceName"
        exit 1
    }
    
    Write-Success "Using provided workspace:"
    Write-Host "  egressWorkspaceId:   $EgressWorkspaceId"
    Write-Host "  egressWorkspaceName: $EgressWorkspaceName"
}

# ============================================================
# STEP 2: Update Environment AND Collection Variables
# ============================================================
Write-Header "STEP 2: Update Variables"

$variables = @{
    "tenantId" = $Config.TenantId
    "apiClientId" = $Config.ApiClientId
    "baseUrl" = $Config.BaseUrl
    "ddeiBaseUrl" = $Config.DdeiBaseUrl
    "egressWorkspaceId" = $EgressWorkspaceId
    "egressWorkspaceName" = $EgressWorkspaceName
    "defendantSurname" = $EgressWorkspaceName
    "tenantId" = $Config.TenantId
    "registerCaseClientId" = $Config.RegisterCaseClientId
    "lccApiId" = $Config.LccApiId
    "netappFolderPath" = "Automation-Testing/"
    "egressDestinationFolder" = "4. Served Evidence/"
    "registerCase" = if ($RegisterCase) { "true" } else { "false" }
    "defaultCaseId" = $Config.DefaultCaseId
    "defaultCaseUrn" = $Config.DefaultCaseUrn
    "baseUrl" = $Config.BaseUrl
    "caseApiBaseUrl" = $Config.CaseApiBaseUrl
    "egressBaseUrl" = $Config.EgressBaseUrl
    "ddeiBaseUrl" = $Config.DdeiBaseUrl
}

if ($EgressFileId) { $variables["egressFileId"] = $EgressFileId }
if ($EgressFileName) { $variables["egressFileName"] = $EgressFileName }
if ($EgressFileIds) { $variables["egressFileIds"] = $EgressFileIds }
if ($EgressFileNames) { $variables["egressFileNames"] = $EgressFileNames }
if ($EgressFileCount) { $variables["egressFileCount"] = $EgressFileCount }

# Separate secrets from non-secret variables.
# Secrets go ONLY into the environment file (loaded via Newman --environment).
# Non-secret variables go into both the environment and the collection copy.
$secretVariables = @{
    "azureUsername" = $Config.AzureUsername
    "azurePassword" = $Config.AzurePassword
    "cmsUsername"   = $Config.CmsUsername
    "cmsPassword"   = $Config.CmsPassword
    "ddeiAccessKey" = $Config.DdeiAccessKey
    "lccApiClientSecret" = $Config.LccApiClientSecret
}

# All variables (secrets + non-secrets) go into the environment file
$allVariables = $variables.Clone()
foreach ($key in $secretVariables.Keys) {
    $allVariables[$key] = $secretVariables[$key]
}

# Update environment file with ALL variables (including secrets)
$UpdatedEnvPath = Update-EnvironmentFile -EnvironmentPath $EnvironmentPath -Variables $allVariables -OutputDir $ScriptDir

# Update collection variables with NON-SECRET variables only (writes to a _updated copy)
$UpdatedCollectionPath = Update-CollectionVariables -CollectionPath $CollectionPath -Variables $variables -OutputDir $ScriptDir

# ============================================================
# STEP 3: Determine Which Tests to Run
# ============================================================
$foldersToRun = @()

if ($TestsToRun -eq "all") {
    $foldersToRun = @(
        "E2E: Egress to NetApp Copy",
        "E2E: Netapp to Egress Copy",
        "E2E: Egress to NetApp Move"
    )
}
else {
    $foldersToRun = @($E2EFolderMap[$TestsToRun])
}

Write-Header "STEP 3: Run E2E Tests"
Write-Host "Tests to run:" -ForegroundColor Cyan
$foldersToRun | ForEach-Object { Write-Host "  - $_" }
Write-Host ""

# ============================================================
# STEP 4: Run E2E Tests Sequentially
# ============================================================
$results = @{}
$allPassed = $true
$isFirstRun = $true

Write-Header "STEP 4: Run E2E Tests"
Write-Host "Workspace: $EgressWorkspaceId ($EgressWorkspaceName)" -ForegroundColor Cyan
if ($RegisterCase -and $foldersToRun.Count -gt 1) {
    Write-Host "RegisterCase: Will register once on first folder, then reuse for remaining folders" -ForegroundColor Cyan
}
Write-Host ""

foreach ($folder in $foldersToRun) {
    Write-Host ""
    Write-Host "Preparing: $folder" -ForegroundColor Cyan
    Write-Host ""
    
    # When RegisterCase is set and running multiple folders, export environment
    # from the first run so we can capture the caseId/caseUrn for subsequent runs
    $exportEnvPath = ""
    if ($RegisterCase -and $isFirstRun -and $foldersToRun.Count -gt 1) {
        $exportEnvPath = Join-Path $ScriptDir "LCCTestEnvironment_exported.postman_environment.json"
        Write-Info "First RegisterCase run - will capture caseId for subsequent folders"
    }
    
    $result = Run-NewmanFolder `
        -CollectionPath $UpdatedCollectionPath `
        -FolderName $folder `
        -EnvironmentPath $UpdatedEnvPath `
        -ReportsDir $ReportsDir `
        -ExportEnvironmentPath $exportEnvPath
    
    $results[$folder] = $result
    
    # After the first RegisterCase run, capture caseId/caseUrn and disable
    # registration for subsequent folders to prevent creating multiple cases
    if ($RegisterCase -and $isFirstRun -and $foldersToRun.Count -gt 1) {
        $isFirstRun = $false
        
        if ($result.Success -and $exportEnvPath -and (Test-Path $exportEnvPath)) {
            Write-Info "Capturing registered case for subsequent test folders..."
            
            $exportedEnv = Get-Content $exportEnvPath -Raw | ConvertFrom-Json
            $capturedCaseId = ""
            $capturedCaseUrn = ""
            
            foreach ($val in $exportedEnv.values) {
                if ($val.key -eq "caseId") { $capturedCaseId = $val.value }
                if ($val.key -eq "caseUrn") { $capturedCaseUrn = $val.value }
            }
            
            if ($capturedCaseId) {
                Write-Success "Captured caseId: $capturedCaseId, caseUrn: $capturedCaseUrn"
                Write-Info "Subsequent folders will reuse this case (registerCase=false)"
                
                # Update environment file: disable registration, inject captured case
                $reuseVars = @{
                    "registerCase"  = "false"
                    "defaultCaseId" = $capturedCaseId
                    "defaultCaseUrn" = $capturedCaseUrn
                    "caseId"        = $capturedCaseId
                    "caseUrn"       = $capturedCaseUrn
                }
                $UpdatedEnvPath = Update-EnvironmentFile -EnvironmentPath $UpdatedEnvPath -Variables $reuseVars -OutputDir $ScriptDir
                
                # Also update collection variables
                $UpdatedCollectionPath = Update-CollectionVariables -CollectionPath $UpdatedCollectionPath -Variables $reuseVars -OutputDir $ScriptDir
            } else {
                Write-Err "Failed to capture caseId from first run - subsequent runs may register new cases"
            }
            
            # Clean up exported env file
            Remove-Item $exportEnvPath -Force -ErrorAction SilentlyContinue
        } elseif (-not $result.Success) {
            Write-Err "First RegisterCase run failed - cannot capture caseId for reuse"
        }
    } else {
        $isFirstRun = $false
    }
    
    if (-not $result.Success) {
        $allPassed = $false
        
        if ($StopOnFailure) {
            Write-Err "Stopping due to -StopOnFailure flag"
            break
        }
        
        if ([Environment]::UserInteractive -eq $false) {
            Write-Host "Non-interactive mode: auto-continuing after failure" -ForegroundColor Yellow
        } else {
            Write-Host ""
            Write-Host "Test failed. Continue with remaining tests? (Y/N)" -ForegroundColor Yellow -NoNewline
            $continue = Read-Host " "
            if ($continue -notin @('Y', 'y')) {
                break
            }
        }
    }
    
    Start-Sleep -Seconds 3
}

# ============================================================
# SUMMARY
# ============================================================
Write-Header "TEST RESULTS SUMMARY"

Write-Host "Workspace: $EgressWorkspaceId ($EgressWorkspaceName)" -ForegroundColor Cyan
Write-Host ""

Write-Host "Test Results:" -ForegroundColor Cyan
$totalDuration = [TimeSpan]::Zero

foreach ($folder in $foldersToRun) {
    if ($results.ContainsKey($folder)) {
        $result = $results[$folder]
        $duration = if ($result.Duration) { $result.Duration.ToString("mm\:ss") } else { "--:--" }
        
        if ($result.Success) {
            Write-Host "  [PASS] $folder ($duration)" -ForegroundColor Green
        } else {
            Write-Host "  [FAIL] $folder ($duration)" -ForegroundColor Red
        }
        
        if ($result.Duration) {
            $totalDuration += $result.Duration
        }
    }
    else {
        Write-Host "  [SKIP] $folder" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "Total Duration: $($totalDuration.ToString('mm\:ss'))" -ForegroundColor Cyan
Write-Host "Reports Directory: $ReportsDir" -ForegroundColor Cyan
Write-Host ""

if ($allPassed) {
    Write-Host "========================================================" -ForegroundColor Green
    Write-Host "  ALL TESTS PASSED!" -ForegroundColor Green
    Write-Host "========================================================" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "========================================================" -ForegroundColor Red
    Write-Host "  SOME TESTS FAILED!" -ForegroundColor Red
    Write-Host "========================================================" -ForegroundColor Red
    exit 1
}