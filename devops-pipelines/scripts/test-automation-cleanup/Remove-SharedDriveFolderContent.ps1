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

	# Include patterns (regex) – only items that MATCH will be returned
	[string[]]$IncludePatterns,

	# Exclude patterns (regex) – items that MATCH will be removed
	[string[]]$ExcludePatterns,

	[switch]$Force
)

$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'SharedDriveCleanupHelperModule.psm1')

# ============================================================
# VALIDATE INPUT
# ============================================================
$FolderPath = ($FolderPath.TrimEnd('/')) + '/'

# Validate include patterns
if ($IncludePatterns -and $IncludePatterns.Count -gt 0) {
	foreach ($pattern in $IncludePatterns) {

		if ([string]::IsNullOrWhiteSpace($pattern)) {
			throw "IncludePatterns cannot contain null or empty values."
		}

		if ($pattern.Length -lt 5) {
			throw "Include pattern '$pattern' must be at least 5 characters long."
		}
	}
}

# Validate exclude patterns
if ($ExcludePatterns -and $ExcludePatterns.Count -gt 0) {
	foreach ($pattern in $ExcludePatterns) {

		if ([string]::IsNullOrWhiteSpace($pattern)) {
			throw "ExcludePatterns cannot contain null or empty values."
		}

		if ($pattern.Length -lt 5) {
			throw "Exclude pattern '$pattern' must be at least 5 characters long."
		}
	}
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
Write-Host "[2/3] Listing items for deletion..." -ForegroundColor Yellow

$objectsToDelete = New-Object System.Collections.Generic.List[object]
$continuationToken = $null

# Helper function to test filters
function Test-PathFilter($path) {
	$includePass = $true
	$excludePass = $true

	# INCLUDE logic: if any include pattern exists, path must match at least ONE
	if ($IncludePatterns -and $IncludePatterns.Count -gt 0) {
		$includePass = $false
		foreach ($pattern in $IncludePatterns) {
			if ($path -match $pattern) {
				$includePass = $true
				break
			}
		}
	}

	# EXCLUDE logic: if ANY exclude pattern matches, reject the path
	if ($ExcludePatterns -and $ExcludePatterns.Count -gt 0) {
		foreach ($pattern in $ExcludePatterns) {
			if ($path -match $pattern) {
				$excludePass = $false
				break
			}
		}
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
	Write-Warning "No matching items were found. Please check correct filter patterns were supplied as input."
	return
}

Write-Host "  [OK] $($objectsToDelete.Count) matching item(s) found:" -ForegroundColor Green

$objectsToDelete | ForEach-Object {
	Write-Host " - $($_.Path) ($($_.Type))" -ForegroundColor Cyan
}

# ============================================================
# PROMPT FOR CONFIRMATION
# ============================================================

if (-not $Force) {
	Write-Host ""
	Write-Host "About to delete $($objectsToDelete.Count) item(s)." -ForegroundColor Yellow
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
Write-Host "[3/3] Deleting $($objectsToDelete.Count) item(s)...." -ForegroundColor Yellow

# Batching objectsToDelete into arrays of max 100 items to avoid exceeding command‑line length limit:
$batchSize = 100
$totalBatches = [math]::Ceiling($objectsToDelete.Count / $batchSize)
$success = $true
$failed = @{
	NotFound = @()
	Failed   = @()
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