<#
.SYNOPSIS
    Tear down Egress artefacts created by automation.

.DESCRIPTION
    Supports two explicit teardown modes:
      1. Delete entire workspace (if created by automation)
      2. Delete specific uploaded files (if workspace already existed)

    Script will refuse to run unless an explicit mode is selected.

.PARAMETER WorkspaceId
    Egress workspace ID.

.PARAMETER FileIds
    One or more file IDs to delete (used with -DeleteFiles).

.PARAMETER DeleteWorkspace
    Deletes the entire workspace.

.PARAMETER DeleteFiles
    Deletes only the specified files.

.PARAMETER Force
    Required safety switch to actually perform deletion.
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$WorkspaceId,

    [string[]]$FileIds = @(),

    [switch]$DeleteWorkspace,
    [switch]$DeleteFiles,
    [switch]$Force
)

# ============================================================
# SAFETY CHECKS
# ============================================================
if (-not $Force) {
    Write-Host "ERROR: -Force is required to perform destructive actions." -ForegroundColor Red
    exit 1
}

if ($DeleteWorkspace -and $DeleteFiles) {
    Write-Host "ERROR: Choose either -DeleteWorkspace OR -DeleteFiles, not both." -ForegroundColor Red
    exit 1
}

if (-not $DeleteWorkspace -and -not $DeleteFiles) {
    Write-Host "ERROR: Must specify either -DeleteWorkspace or -DeleteFiles." -ForegroundColor Red
    exit 1
}

if ($DeleteFiles -and $FileIds.Count -eq 0) {
    Write-Host "ERROR: -DeleteFiles requires at least one FileId." -ForegroundColor Red
    exit 1
}

# ============================================================
# LOAD CONFIGURATION (same pattern as setup script)
# ============================================================
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$SecretsFile = Join-Path $ScriptDir "secrets.config.ps1"

if (Test-Path $SecretsFile) {
    . $SecretsFile
}

$BaseUrl = $env:LCC_EGRESS_BASE_URL
$ServiceAccountAuth = $env:LCC_EGRESS_SERVICE_ACCOUNT_AUTH

if (-not $BaseUrl -or -not $ServiceAccountAuth) {
    Write-Error "Missing Egress configuration."
    exit 1
}

$curl = Get-Command curl.exe -ErrorAction SilentlyContinue
if (-not $curl) { $curl = "curl" }

# ============================================================
# AUTHENTICATE
# ============================================================
Write-Host "[1/3] Authenticating..." -ForegroundColor Yellow

$tokenJson = & $curl --silent --location "$BaseUrl/api/v1/user/auth/" `
    --header "Accept: application/json" `
    --header "Authorization: Basic $ServiceAccountAuth"

$tokenObj = $tokenJson | ConvertFrom-Json
if (-not $tokenObj.token) {
    Write-Error "Authentication failed."
    exit 1
}

$TokenBase64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($tokenObj.token))
Write-Host "  [OK] Authenticated" -ForegroundColor Green

# ============================================================
# MODE 1: DELETE FILES ONLY
# ============================================================
if ($DeleteFiles) {
    Write-Host "[2/3] Deleting files..." -ForegroundColor Yellow

    foreach ($fileId in $FileIds) {
        Write-Host "  Deleting file $fileId" -ForegroundColor Gray

        $result = & $curl --silent --location --request DELETE `
            "$BaseUrl/api/v1/workspaces/$WorkspaceId/files/$fileId" `
            --header "Authorization: Basic $TokenBase64"

        if ($result -match '"error_code"') {
            Write-Host "    [WARN] Failed to delete file $fileId" -ForegroundColor Yellow
        } else {
            Write-Host "    [OK] Deleted" -ForegroundColor Green
        }
    }

    Write-Host "[3/3] File cleanup complete." -ForegroundColor Green
    exit 0
}

# ============================================================
# MODE 2: DELETE ENTIRE WORKSPACE
# ============================================================
if ($DeleteWorkspace) {
    Write-Host "[2/3] Deleting workspace $WorkspaceId..." -ForegroundColor Yellow

    $result = & $curl --silent --location --request DELETE `
        "$BaseUrl/api/v1/workspaces/$WorkspaceId" `
        --header "Authorization: Basic $TokenBase64"

    if ($result -match '"error_code"') {
        Write-Error "Workspace deletion failed."
        exit 1
    }

    Write-Host "  [OK] Workspace deleted" -ForegroundColor Green
    Write-Host "[3/3] Teardown complete." -ForegroundColor Green
    exit 0
}