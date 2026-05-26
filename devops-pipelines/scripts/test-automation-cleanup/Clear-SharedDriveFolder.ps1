param(
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
  [string]$BaseUrl
)

$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'SharedDriveCleanupHelperModule.psm1')

# Normalise Folder Path input
$FolderPath = $FolderPath.TrimEnd('/')

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
# DELETE FOLDER
# ============================================================
Write-Host "[2/3] Attempting to delete folder $FolderPath..." -ForegroundColor Yellow

$objectsToDelete = @(
  @{
    Type = "Folder"
    Path = "$FolderPath/"
  }
)

try {
  $result = Remove-SharedDriveObjects `
    -AuthHeader $authHeader `
    -BaseUrl $BaseUrl `
    -CaseId $CaseId `
    -ObjectsToDelete $objectsToDelete |
  Select-Object -First 1
  
  if ($result.Status -ne "Deleted") {
    $msg = "Failed to delete folder '$FolderPath'. Status: $($result.Status)"

    if ($result.Error) {
      $msg += " | Error: $($result.Error)"
    }

    throw $msg
  }
  else {
    Write-Host "  [OK] Folder $FolderPath successfully deleted. Keys deleted: $($result.KeysDeleted)" -ForegroundColor Green
  }
}
catch {
  throw $_
}

# ============================================================
# RECREATE FOLDER
# ============================================================
Write-Host "[3/3] Attempting to recreate folder $FolderPath..." -ForegroundColor Yellow

$body = @{
  path   = "$FolderPath/"
  caseId = $CaseId
} | ConvertTo-Json

try {
  $response = Invoke-RestMethod -Method Post `
    -Uri "$BaseUrl/api/v1/netapp/folders" `
    -Headers $authHeader `
    -ContentType 'application/json' `
    -Body $body
  
  if ($response -eq $true) {
    Write-Host "  [OK] Folder $FolderPath successfully recreated." -ForegroundColor Green
  }
  else {
    throw "Failed to recreate folder $FolderPath."
  }
} 
catch {
  throw "Error during folder recreation: $_"
}

