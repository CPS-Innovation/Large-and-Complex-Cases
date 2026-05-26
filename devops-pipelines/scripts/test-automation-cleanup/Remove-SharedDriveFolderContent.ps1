param (
	[Parameter(Mandatory = $true)]
  [string]$ClientId,

  [Parameter(Mandatory = $true)]
  [string]$TenantId,

  [Parameter(Mandatory = $true)]
  [string]$AadUsername,

  [Parameter(Mandatory = $true)]
  [string]$AadPassword,

  [Parameter(Mandatory = $true)]
  [int]$CaseId,

  [Parameter(Mandatory = $true)]
  [string]$FolderPath,

  [Parameter(Mandatory = $true)]
  [string]$BaseUrl,

	# Include pattern (regex) – only items that MATCH will be returned
	[string]$IncludePattern,

	# Exclude pattern (regex) – items that MATCH will be removed
	[string]$ExcludePattern,

	[switch]$Force
)

$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'SharedDriveCleanupHelperModule.psm1')

# ============================================================
# VALIDATE INPUT
# ============================================================
$FolderPath = ($FolderPath.TrimEnd('/'))  + '/'

if (-not [string]::IsNullOrWhiteSpace($IncludePattern) -and $IncludePattern.Length -lt 5) {
  throw "IncludePattern must be at least 5 characters long."
}
if (-not [string]::IsNullOrWhiteSpace($ExcludePattern) -and $ExcludePattern.Length -lt 5) {
	throw "ExcludePattern must be at least 5 characters long."
}

$IncludePattern = [regex]::Escape($IncludePattern)
$ExcludePattern = [regex]::Escape($ExcludePattern)

# ============================================================
# GET AZURE AD TOKEN
# ============================================================
Write-Host "[1/3] Authenticating with Azure AD..." -ForegroundColor Yellow

try {
  $authHeader = Get-AzureAdBearerHeader `
    -ClientId $ClientId `
    -TenantId $TenantId `
    -AadUsername $AadUsername `
    -AadPassword $AadPassword
}
catch {
  throw
}

Write-Host "  [OK] Authenticated" -ForegroundColor Green

# ============================================================
# LIST FOLDER CONTENTS
# ============================================================
$objectsToDelete = @()
$continuationToken = $null

# Helper function to test filters
function Test-PathFilter($path) {
	$includePass = $true
	$excludePass = $true

	if ($IncludePattern) {
		$includePass = $path -match $IncludePattern
	}

	if ($ExcludePattern) {
		$excludePass = -not ($path -match $ExcludePattern)
	}

	return ($includePass -and $excludePass)
}

do {
	try {
		$response = Invoke-RestMethod -Method Get `
			-Uri "$BaseUrl/api/v1/netapp/files?path=$FolderPath&continuation-token=$continuationToken" `
			-Headers $authHeader `
			-ContentType 'application/json'
	}
	catch {
		throw
	}

	foreach ($folder in $response.data.folderData) {
		if (Test-PathFilter $folder.path) {
			$objectsToDelete += [PSCustomObject]@{
				Path = $folder.path
				Type = "Folder"
			}
		}
	}

	foreach ($file in $response.data.fileData) {
		if (Test-PathFilter $file.path) {
			$objectsToDelete += [PSCustomObject]@{
				Path = $file.path
				Type = "Material"
			}
		}
	}

	$continuationToken = $response.pagination.nextContinuationToken
} 
until ([string]::IsNullOrWhiteSpace($continuationToken))

Write-Host "$($objectsToDelete.length) item(s) will be deleted:" -ForegroundColor Cyan

$objectsToDelete | ForEach-Object {
	Write-Host " - $($_.Path) ($($_.Type))" -ForegroundColor Cyan
}

# ============================================================
# PROMPT FOR CONFIRMATION
# ============================================================

if (-not $Force) {
  Write-Host ""
  Write-Host "About to delete $($objectsToDelete.length) items." -ForegroundColor Yellow
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
# DELETE ITEMS
# ============================================================

try {
  $results = Remove-SharedDriveObjects `
    -AuthHeader $authHeader `
    -BaseUrl $BaseUrl `
    -CaseId $CaseId `
    -ObjectsToDelete $objectsToDelete
}
catch {
	throw
}

