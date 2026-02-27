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
    [string]$CollectionPath = ".\LCCUserJourneyTests_fixed.postman_collection",
    
    [Parameter(Mandatory=$false)]
    [string]$EnvironmentPath = ".\LCCTestEnvironment.postman_environment",
    
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
    [string]$CmsPassword = ""
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
    TenantId      = if ($env:LCC_TENANT_ID) { $env:LCC_TENANT_ID } else { "" }
    ClientId      = if ($env:LCC_CLIENT_ID) { $env:LCC_CLIENT_ID } else { "" }
    AzureUsername = if ($AzureUsername) { $AzureUsername } elseif ($env:LCC_AZURE_USERNAME) { $env:LCC_AZURE_USERNAME } else { "" }
    AzurePassword = if ($AzurePassword) { $AzurePassword } elseif ($env:LCC_AZURE_PASSWORD) { $env:LCC_AZURE_PASSWORD } else { "" }
    CmsUsername   = if ($CmsUsername) { $CmsUsername } elseif ($env:LCC_CMS_USERNAME) { $env:LCC_CMS_USERNAME } else { "" }
    CmsPassword   = if ($CmsPassword) { $CmsPassword } elseif ($env:LCC_CMS_PASSWORD) { $env:LCC_CMS_PASSWORD } else { "" }
    DdeiAccessKey = if ($env:LCC_DDEI_ACCESS_KEY) { $env:LCC_DDEI_ACCESS_KEY } else { "" }
}

# Validate required config
$missingConfig = @()
if (-not $Config.TenantId) { $missingConfig += "LCC_TENANT_ID" }
if (-not $Config.ClientId) { $missingConfig += "LCC_CLIENT_ID" }
if (-not $Config.AzureUsername) { $missingConfig += "LCC_AZURE_USERNAME (or -AzureUsername)" }
if (-not $Config.AzurePassword) { $missingConfig += "LCC_AZURE_PASSWORD (or -AzurePassword)" }
if (-not $Config.CmsUsername) { $missingConfig += "LCC_CMS_USERNAME (or -CmsUsername)" }
if (-not $Config.CmsPassword) { $missingConfig += "LCC_CMS_PASSWORD (or -CmsPassword)" }

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
    if (-not $newman) {
        Write-Err "Newman is not installed!"
        Write-Host ""
        Write-Host "Install Newman with:" -ForegroundColor Yellow
        Write-Host "  npm install -g newman" -ForegroundColor White
        Write-Host "  npm install -g newman-reporter-htmlextra" -ForegroundColor White
        Write-Host ""
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
    $updatedEnvPath = Join-Path $envDir "LCCTestEnvironment_updated.postman_environment"
    
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
        [hashtable]$Variables
    )
    
    Write-Info "Updating collection variables in place..."
    
    $collection = Get-Content $CollectionPath -Raw | ConvertFrom-Json
    
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
    
    $collection | ConvertTo-Json -Depth 100 | Set-Content $CollectionPath -Encoding UTF8
    
    Write-Success "Collection updated: $CollectionPath"
}

function Run-NewmanFolder {
    param(
        [string]$CollectionPath,
        [string]$FolderName,
        [string]$EnvironmentPath,
        [string]$ReportsDir
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
    
    & newman run $fullCollectionPath `
        --folder $FolderName `
        --environment $fullEnvPath `
        --timeout-request 120000 `
        --delay-request 2000 `
        -r "cli,htmlextra,json" `
        --reporter-htmlextra-export $fullHtmlReport `
        --reporter-htmlextra-logs `
        --reporter-json-export $fullJsonReport
    
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

    Write-Info "Running upload..."
    Write-Host "  Size: $(if ($SizeMB -gt 0) { "$SizeMB MB" } else { "$SizeGB GB" })"
    Write-Host "  File Count: $FileCount"
    Write-Host ""
    
    $tempOutputFile = Join-Path $env:TEMP "egress_upload_output_$((Get-Date).Ticks).txt"
    
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
    "egressWorkspaceId" = $EgressWorkspaceId
    "egressWorkspaceName" = $EgressWorkspaceName
    "defendantSurname" = $EgressWorkspaceName
    "searchDefendantName" = $EgressWorkspaceName
    "tenantId" = $Config.TenantId
    "apiClientId" = $Config.ClientId
    "clientId" = $Config.ClientId
    "uiClientId" = $Config.ClientId
    "netappFolderPath" = "Automation-Testing/"
    "sourceRootFolderPath" = "Automation-Testing/"
    "egressDestinationFolder" = "4. Served Evidence/"
}

if ($EgressFileId) { $variables["egressFileId"] = $EgressFileId }
if ($EgressFileName) { $variables["egressFileName"] = $EgressFileName }
if ($EgressFileIds) { $variables["egressFileIds"] = $EgressFileIds }
if ($EgressFileNames) { $variables["egressFileNames"] = $EgressFileNames }
if ($EgressFileCount) { $variables["egressFileCount"] = $EgressFileCount }

# Add authentication credentials from config
$variables["azureUsername"] = $Config.AzureUsername
$variables["azurePassword"] = $Config.AzurePassword
$variables["cmsUsername"] = $Config.CmsUsername
$variables["username"] = $Config.CmsUsername
$variables["cmsPassword"] = $Config.CmsPassword
$variables["password"] = $Config.CmsPassword
$variables["ddeiAccessKey"] = $Config.DdeiAccessKey

# Update environment file
$UpdatedEnvPath = Update-EnvironmentFile -EnvironmentPath $EnvironmentPath -Variables $variables -OutputDir $ScriptDir

# Update collection variables
Update-CollectionVariables -CollectionPath $CollectionPath -Variables $variables

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

Write-Header "STEP 4: Run E2E Tests"
Write-Host "Workspace: $EgressWorkspaceId ($EgressWorkspaceName)" -ForegroundColor Cyan
Write-Host ""

foreach ($folder in $foldersToRun) {
    Write-Host ""
    Write-Host "─────────────────────────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host "Preparing: $folder" -ForegroundColor Cyan
    Write-Host "─────────────────────────────────────────────────────────────" -ForegroundColor DarkGray
    
    $result = Run-NewmanFolder `
        -CollectionPath $CollectionPath `
        -FolderName $folder `
        -EnvironmentPath $UpdatedEnvPath `
        -ReportsDir $ReportsDir
    
    $results[$folder] = $result
    
    if (-not $result.Success) {
        $allPassed = $false
        
        if ($StopOnFailure) {
            Write-Err "Stopping due to -StopOnFailure flag"
            break
        }
        
        Write-Host ""
        Write-Host "Test failed. Continue with remaining tests? (Y/N)" -ForegroundColor Yellow -NoNewline
        $continue = Read-Host " "
        if ($continue -ne 'Y' -and $continue -ne 'y') {
            break
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