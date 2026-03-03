<#
.SYNOPSIS
    Cleanup Egress Automation Workspace and test files.

.DESCRIPTION
    Deletes everything created by the Setup-EgressWorkspaceAndUpload.ps1 script:
      • Deletes entire workspace (recommended)
      • OR deletes only uploaded files (optional)
      • Removes the added admin user (optional)

    Automatically loads:
      • LCC_EGRESS_BASE_URL
      • LCC_EGRESS_SERVICE_ACCOUNT_AUTH

.PARAMETER WorkspaceId
    ID of the workspace to delete.

.PARAMETER DeleteFilesOnly
    If set, only deletes uploaded files, not the workspace.

.PARAMETER RemoveUser
    If set, removes the admin user that was added.

.EXAMPLE
    .\Cleanup-EgressWorkspace.ps1 -WorkspaceId "abc123"

.EXAMPLE
    .\Cleanup-EgressWorkspace.ps1 -WorkspaceId "abc123" -DeleteFilesOnly
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$WorkspaceId
)

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "        EGRESS CLEANUP SCRIPT"              -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# Load configuration
$BaseUrl = $env:LCC_EGRESS_BASE_URL
$ServiceAccountAuth = $env:LCC_EGRESS_SERVICE_ACCOUNT_AUTH

if (-not $BaseUrl -or -not $ServiceAccountAuth) {
    Write-Error "Missing environment variables. Ensure LCC_EGRESS_BASE_URL and LCC_EGRESS_SERVICE_ACCOUNT_AUTH are set."
    exit 1
}

# Authenticate
Write-Host "[1/4] Authenticating..." -ForegroundColor Yellow

$tokenJson = & $curl --silent --location "$BaseUrl/api/v1/user/auth/" `
    --header "Accept: application/json" `
    --header "Authorization: Basic $ServiceAccountAuth"

$tokenObj = $tokenJson | ConvertFrom-Json
if (-not $tokenObj.token) {
    Write-Error "Failed to authenticate: $tokenJson"
    exit 1
}

$Token = $tokenObj.token
$TokenBase64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($Token))

Write-Host "  [OK] Authenticated" -ForegroundColor Green

# ==============================================================
# DELETE WORKSPACE
# ==============================================================

Write-Host "[4/4] Deleting workspace..." -ForegroundColor Yellow

$deleteJson = & $curl --silent --request DELETE `
    "$BaseUrl/api/v1/workspaces/$WorkspaceId/" `
    --header "Authorization: Basic $TokenBase64"

if ($deleteJson -match '"success"') {
    Write-Host "  [OK] Workspace deleted" -ForegroundColor Green
} else {
    Write-Host "  [WARN] Response: $deleteJson" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host " CLEANUP COMPLETE" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green