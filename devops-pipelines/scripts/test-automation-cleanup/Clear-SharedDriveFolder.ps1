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

  [Parameter(Mandatory = $false)]
  [string]$BaseUrl = "https://fa-lacc-api-dev.azurewebsites.net",

  [switch]$Force
)

$ErrorActionPreference = 'Stop'

# ============================================================
# GET AZURE AD TOKEN
# ============================================================
Write-Host "Requesting Azure AD access token..." -ForegroundColor Yellow

$body = @{
    client_id     = $ClientId
    scope         = "api://$ClientId/user_impersonation"
    username      = $AadUsername
    password      = $AadPassword
    grant_type    = "password"
}

try {
  $tokenObj = Invoke-RestMethod -Method Post `
    -Uri "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/token" `
    -ContentType 'application/x-www-form-urlencoded' `
    -Body $body
}
catch {
  Write-Error $_.Exception.Message
  exit 1
}

$aadToken = $tokenObj.access_token

if (-not $aadToken) {
  Write-Error "The request was successful but the response does not contain an access token."
  Write-Error $tokenObj
  exit 1
}

Write-Host "  [OK] AAD Token obtained" -ForegroundColor Green

$headers = @{
  Authorization = "Bearer $aadToken"
  "Content-Type" = "application/json"
}

# ============================================================
# DELETE FOLDER
# ============================================================
Write-Host "Attempting to delete folder $FolderPath..." -ForegroundColor Yellow

$body = @{
  caseId = $CaseId
  operations = @(
    @{
      type = "Folder"     
      sourcePath = "$FolderPath/"
    }
  )
} | ConvertTo-Json

try {
  $response = Invoke-RestMethod -Method Post `
    -Uri "$BaseUrl/api/v1/netapp/delete/batch" `
    -Headers $headers `
    -Body $body

  Write-Host $reponse
  
  if ($response.status -eq 'Completed' -and $response.succeeded -gt 0) {
    Write-Host "  [OK] Folder $FolderPath successfully deleted. Keys deleted: $($response.results[0].keysDeleted)" -ForegroundColor Green
    $folderDeleted = $true
  }
  elseif ($response.status -eq 'NoOp' -and $response.notFound -gt 0) {
    Write-Warning "  [INFO] Folder $FolderPath was already empty or not found, skipping delete. Please check the correct folder path was used."
    $folderDeleted = $false
  }
  elseif ($response.status -eq 'Failed') {
    Write-Error "  [ERROR] Deletion failed: $($response.results[0].error)"
    exit 1
  }
}
catch {
  Write-Error "Failed to delete folder: $_"
  exit 1
}

# ============================================================
# RECREATE FOLDER (skip if deletion was skipped)
# ============================================================
if ($folderDeleted) {
    Write-Host "Attempting to recreate folder $FolderPath..." -ForegroundColor Yellow

    $body = @{
      path = "$FolderPath/"
      caseId = $CaseId
    } | ConvertTo-Json

    try {
      $response = Invoke-RestMethod -Method Post `
        -Uri "$BaseUrl/api/v1/netapp/folders" `
        -Headers $headers `
        -Body $body
      
      if ($response -eq $true) {
        Write-Host "  [OK] Folder $FolderPath successfully recreated." -ForegroundColor Green
      } else {
        Write-Error "Failed to recreate folder $FolderPath." -ForegroundColor Red
        exit 1
      }
    } 
    catch {
      Write-Error "Error during folder recreation: $_"
      exit 1
    }
} else {
    Write-Host "  [INFO] Folder deletion skipped, no need to recreate." -ForegroundColor Cyan
}