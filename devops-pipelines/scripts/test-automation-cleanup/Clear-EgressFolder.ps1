param(
  [Parameter(Mandatory = $true)]
  [string]$BaseUrl,

  [Parameter(Mandatory = $true)]
  [string]$ServiceAccountAuth,

  [Parameter(Mandatory = $true)]
  [string]$WorkspaceId,

  [Parameter(Mandatory = $true)]
  [string]$FolderName,

  [switch]$Force
)

$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'EgressCleanupHelperModule.psm1')

# ============================================================
# AUTHENTICATE
# ============================================================
Write-Host "[1/3] Authenticating to Egress..." -ForegroundColor Yellow

try {
  $AuthHeader = Connect-EgressServiceAccount `
    -BaseUrl $BaseUrl `
    -AuthToken $ServiceAccountAuth
}
catch {
  Write-Error $_.Exception.Message
  exit 1
}

Write-Host "  [OK] Authenticated" -ForegroundColor Green

# ============================================================
# LIST FILES
# ============================================================
Write-Host "[2/3] Listing files..." -ForegroundColor Yellow

$filesToDelete = @()

try {
  $response = Invoke-RestMethod -Method Get `
    -Uri "$BaseUrl/api/v1/workspaces/$WorkspaceId/files?path=$folderName" `
    -Headers $AuthHeader
}
catch {
  Write-Error "Failed to list folder contents: $($_.Exception.Message)"
  exit 1
}

while ($true) {
  if ($response.data) {
    $filesToDelete += $response.data
  }

  if ([string]::IsNullOrWhiteSpace($response.pagination.next_url)) {
    break
  }

  $nextUrl = $response.pagination.next_url -replace '^http://', 'https://'

  $response = Invoke-RestMethod -Method Get `
    -Uri $nextUrl `
    -Headers $AuthHeader
}

if (-not $filesToDelete) {
  Write-Host "  [OK] Folder is empty - nothing to delete." -ForegroundColor Green
  exit 0
}

Write-Host "Files to be deleted:" -ForegroundColor Cyan

$filesToDelete | ForEach-Object {
  Write-Host " - $($_.filename) [$($_.id)]" -ForegroundColor Cyan
}

# ============================================================
# PROMPT FOR CONFIRMATION
# ============================================================

if (-not $Force) {
  Write-Host ""
  Write-Host "About to delete $($filesToDelete.Count) file(s) from folder '$FolderName'." -ForegroundColor Yellow
  $confirmation = Read-Host "Type 'yes' to confirm"

  if ($confirmation -ne 'yes') {
    Write-Host "Deletion cancelled by user." -ForegroundColor Cyan
    exit 0
  }
}
else {
  Write-Host "Confirmation bypassed (-Force specified)." -ForegroundColor DarkYellow
}

# ============================================================
# DELETE FILES
# ============================================================
Write-Host "[3/3] Deleting $($filesToDelete.Count) file(s)..." -ForegroundColor Yellow


$fileIds = @($filesToDelete | Select-Object -ExpandProperty id)

try {
  Remove-EgressFiles `
    -BaseUrl $BaseUrl `
    -AuthorizationHeader $AuthHeader `
    -WorkspaceId $WorkspaceId `
    -FileIds $fileIds
}
catch {
  Write-Error $_.Exception.Message
  exit 1
}

Write-Host "  [OK] All files deleted successfully." -ForegroundColor Green