param(
  [Parameter(Mandatory = $true)]
  [string]$BaseUrl,

  [Parameter(Mandatory = $true)]
  [string]$ServiceAccountAuth,

  [Parameter(Mandatory = $true)]
  [string]$WorkspaceNamePrefix,

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
# LIST WORKSPACES
# ============================================================
Write-Host "[2/3] Listing Workspaces..." -ForegroundColor Yellow

$workspacesToRemove = @()

try {
  $response = Invoke-RestMethod -Method Get `
    -Uri "$BaseUrl/api/v1/workspaces?name=$WorkspaceNamePrefix" `
    -Headers $AuthHeader `
    -ContentType 'application/json'
}
catch {
  Write-Error "Failed to list workspaces: $($_.Exception.Message)"
  exit 1
}

while ($true) {
  if ($response.data) {
    $workspacesToRemove += $response.data
  }

  if ([string]::IsNullOrWhiteSpace($response.pagination.next_url)) {
    break
  }

  $nextUrl = $response.pagination.next_url -replace '^http://', 'https://'

  $response = Invoke-RestMethod -Method Get `
    -Uri $nextUrl `
    -Headers $AuthHeader
}

if (-not $workspacesToRemove) {
  Write-Host "  [OK] No workspaces found with the specified name prefix - nothing to delete." -ForegroundColor Green
  exit 0
}

Write-Host "Workspaces to be deleted:" -ForegroundColor Cyan

$workspacesToRemove | ForEach-Object {
  Write-Host " - $($_.name) [$($_.id)]" -ForegroundColor Cyan
}

# ============================================================
# PROMPT FOR CONFIRMATION
# ============================================================

if (-not $Force) {
  Write-Host ""
  Write-Host "About to delete $($workspacesToRemove.Count) workspaces." -ForegroundColor Yellow
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
# DELETE WORKSPACES
# ============================================================
Write-Host "[3/3] Deleting $($workspacesToRemove.Count) file(s)..." -ForegroundColor Yellow


$failed = @()

foreach ($ws in $workspacesToRemove) {
  try {
    $null = Invoke-RestMethod -Method Delete `
      -Uri "$BaseUrl/api/v1/workspaces/$($ws.id)" `
      -Headers $AuthHeader `
      -ContentType 'application/json'
  }
  catch {
    $failed += [pscustomobject]@{
      Id    = $ws.id
      Name  = $ws.name
      Error = $_.Exception.Message
    }

  }
}

if ($failed) {
  Write-Error "Failed to delete one or more workspaces:"
  foreach ($f in $failed) {
    Write-Error " - $($f.Name) [$($f.Id)]: $($f.Error)"
  }
  exit 1
}

Write-Host "  [OK] All workspaces deleted successfully." -ForegroundColor Green