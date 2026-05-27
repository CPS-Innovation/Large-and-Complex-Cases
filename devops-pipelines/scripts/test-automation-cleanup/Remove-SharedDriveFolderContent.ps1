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

$objectsToDelete = New-Object System.Collections.Generic.List[object]
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

	$result = ($includePass -and $excludePass)

	Write-Debug "Filter decision: Path='$path' Include=$includePass ExcludePass=$excludePass Result=$result"

	return $result
}

do {
	Write-Verbose "Fetching page with continuationToken='$continuationToken'"
	try {
		$encodedPath = [uri]::EscapeDataString($FolderPath)

		if ($continuationToken) {
				$encodedToken = [uri]::EscapeDataString($continuationToken)
				$uri = "$BaseUrl/api/v1/netapp/files?path=$encodedPath&continuation-token=$encodedToken"
		}
		else {
				$uri = "$BaseUrl/api/v1/netapp/files?path=$encodedPath"
		}

		Write-Verbose "GET $uri"
		
		$response = Invoke-RestMethod -Method Get `
			-Uri $Uri `
			-Headers $authHeader `
			-ContentType 'application/json'

		if ((-not $response.data.folderData) -and (-not $response.data.fileData)) {
			Write-Warning "No items were found in folder '$FolderPath'.`n
				If that was not expected, please make sure the correct folder path input was supplied."
			return
		}
	}
	catch {
		throw
	}

	foreach ($folder in $response.data.folderData) {
		if (Test-PathFilter $folder.path) {
			Write-Verbose "Marked for deletion: $($folder.path) (Folder)"
			$objectsToDelete.Add(
				[PSCustomObject]@{
					Path = $folder.path
					Type = "Folder"
				}
			)
		}
	}

	foreach ($file in $response.data.fileData) {
		if (Test-PathFilter $file.path) {
			Write-Verbose "Marked for deletion: $($file.path) (Material)"
			$objectsToDelete.Add(
				[PSCustomObject]@{
					Path = $file.path
					Type = "Material"
				}
			)
		}
	}

	$continuationToken = $response.pagination.nextContinuationToken
	Write-Verbose "Next continuation token: $continuationToken"
} 
until ([string]::IsNullOrWhiteSpace($continuationToken))


if ($objectsToDelete.Count -eq 0) {
	Write-Warning "No items were marked for deletion.`n
		Please check correct filter patterns were supplied as input."
	return
}

Write-Host "$($objectsToDelete.Count) item(s) will be deleted:" -ForegroundColor Cyan

$objectsToDelete | ForEach-Object {
	Write-Host " - $($_.Path) ($($_.Type))" -ForegroundColor Cyan
}

# ============================================================
# PROMPT FOR CONFIRMATION
# ============================================================

if (-not $Force) {
  Write-Host ""
  Write-Host "About to delete $($objectsToDelete.Count) items." -ForegroundColor Yellow
  $confirmation = Read-Host "Type 'yes' to confirm"

  if ($confirmation -ne 'yes') {
    Write-Host "Deletion cancelled by user." -ForegroundColor Cyan
    return
  }
}
else {
  Write-Host "Confirmation bypassed (-Force specified)." -ForegroundColor DarkYellow
}

# ============================================================
# DELETE ITEMS
# ============================================================

# Batching objectsToDelete into arrays of max 100 items to avoid exceeding command‑line length limit:
$batchSize = 100
$totalBatches = [math]::Ceiling($objectsToDelete.Count / $batchSize)
$success = $true
$failed = @{
	NotFound  = @()
  Failed    = @()
}

for ($i = 0; $i -lt $objectsToDelete.Count; $i += $batchSize) {
  $batch = $objectsToDelete[$i..([math]::Min($i + $batchSize - 1, $objectsToDelete.Count - 1))]  
  $batchIndex = [int]($i / $batchSize) + 1

  Write-Host ("Deleting batch {0}/{1} ({2} item(s))..." -f `
    $batchIndex, $totalBatches, $batch.Count)
	
	Write-Debug "Batch payload:`n$($batch | ConvertTo-Json -Depth 5)"
	
	$results = Remove-SharedDriveObjects `
		-AuthHeader $authHeader `
		-BaseUrl $BaseUrl `
		-CaseId $CaseId `
		-ObjectsToDelete $batch

	Write-Verbose "Batch $batchIndex results: 
		Succeeded=$($results.Succeeded.Count), 
		NotFound=$($results.NotFound.Count), 
		Failed=$($results.Failed.Count)"

	if ($results.Succeeded.Count -ne $batch.Count) {
		$success = $false
		$failed.NotFound += $results.NotFound
		$failed.Failed += $results.Failed

		Write-Warning ("Batch {0}/{1} completed with some issues." -f `
		$batchIndex, $totalBatches)
	}
	else {
		Write-Host ("Batch {0}/{1} completed successfully." -f `
		$batchIndex, $totalBatches)
	}
}

if (-not $success) {
	$msg = "Some items failed to be deleted.`n"

	if ($failed.NotFound.Count -gt 0) {
		$msg += "NotFound:`n"
		foreach ($i in $failed.NotFound) {
			$msg += " - $($i.Path)`n"
		}
	}

	if ($failed.Failed.Count -gt 0) {
		$msg += "Failed:`n"
		foreach ($i in $failed.Failed) {
			$msg += " - $($i.Path): $($i.Error)`n"
		}
	}

	throw $msg
}

Write-Host "  [OK] $($objectsToDelete.Count) item(s) deleted successfully." -ForegroundColor Green