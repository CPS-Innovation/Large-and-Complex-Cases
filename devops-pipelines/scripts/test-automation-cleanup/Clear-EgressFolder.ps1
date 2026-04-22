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
Write-Host "[1/4] Authenticating to Egress..." -ForegroundColor Yellow

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
# FIND FOLDER
# ============================================================
Write-Host "[2/4] Finding folder '$FolderName'..." -ForegroundColor Yellow

try {
  $topLevelFiles = Invoke-RestMethod -Method Get `
    -Uri "$BaseUrl/api/v1/workspaces/$WorkspaceId/files" `
    -Headers $AuthHeader
}
catch {
  Write-Error "Failed to retrieve workspace files: $($_.Exception.Message)"
  exit 1
}

$matchingFolders = $topLevelFiles.data | Where-Object {
  $_.filename -eq $FolderName
}

if (-not $matchingFolders) {
  Write-Error "No folder named '$FolderName' was found."
  exit 1
}

if ($matchingFolders.Count -gt 1) {
  Write-Error "Multiple folders named '$FolderName' found. Aborting."
  exit 1
}

$folderId = $matchingFolders.id
Write-Host "  [OK] Folder ID: $folderId" -ForegroundColor Green

# ============================================================
# LIST FILES
# ============================================================
Write-Host "[3/4] Listing files..." -ForegroundColor Yellow

$filesToDelete = @()

try {
  $response = Invoke-RestMethod -Method Get `
    -Uri "$BaseUrl/api/v1/workspaces/$WorkspaceId/files?folder=$folderId" `
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
Write-Host "[4/4] Deleting $($filesToDelete.Count) file(s)..." -ForegroundColor Yellow


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