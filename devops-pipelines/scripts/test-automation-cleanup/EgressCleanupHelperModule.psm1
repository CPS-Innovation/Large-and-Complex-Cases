function Invoke-EgressApiRequest {
  param(
    [Parameter(Mandatory = $true)]
    [string]$Method,

    [Parameter(Mandatory = $true)]
    [string]$Uri,

    [Parameter(Mandatory = $true)]
    [hashtable]$Headers,

    [string]$Body,

    [int]$TimeoutSec = 60,

    [int]$MaxAttempts = 3
  )

  for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
    try {
      $params = @{
        Method      = $Method
        Uri         = $Uri
        Headers     = $Headers
        TimeoutSec  = $TimeoutSec
        ErrorAction = 'Stop'
      }

      if ($Body) {
        $params.Body = $Body
        $params.ContentType = 'application/json'
      }

      return Invoke-RestMethod @params
    }
    catch {
      $statusCode = $_.Exception.Response.StatusCode.value__

      if ($statusCode -notin @(502, 503, 504) -or $attempt -eq $MaxAttempts) {
        throw
      }

      $delay = 2 * $attempt

      Write-Warning `
        "Egress returned HTTP $statusCode. Retrying in $delay seconds (attempt $attempt/$MaxAttempts)..."

      Start-Sleep -Seconds $delay
    }
  }
}

function Connect-EgressServiceAccount {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory = $true)]
    [string]$BaseUrl,

    [Parameter(Mandatory = $true)]
    [string]$AuthToken
  )

  try {
    $tokenObj = Invoke-EgressApiRequest `
      -Method Get `
      -Uri "$BaseUrl/api/v1/user/auth/" `
      -Headers @{
        Accept        = "application/json"
        Authorization = "Basic $AuthToken"
    }
  }
  catch {
    throw "Authentication failed: $($_.Exception.Message)"
  }

  if (-not $tokenObj.token) {
    throw "Authentication succeeded but no token was returned."
  }

  $tokenBase64 = [Convert]::ToBase64String(
    [Text.Encoding]::UTF8.GetBytes($tokenObj.token)
  )

  return @{
    Authorization = "Basic $tokenBase64"
  }
}

function Remove-EgressFiles {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory = $true)]
    [string]$BaseUrl,

    [Parameter(Mandatory = $true)]
    [hashtable]$AuthorizationHeader,

    [Parameter(Mandatory = $true)]
    [string]$WorkspaceId,

    [Parameter(Mandatory = $true)]
    [string[]]$FileIds
  )

  $headers = $AuthorizationHeader

  $body = @{
    file_ids = $FileIds
  } | ConvertTo-Json

  $response = Invoke-EgressApiRequest `
    -Method Delete `
    -Uri "$BaseUrl/api/v1/workspaces/$WorkspaceId/files" `
    -Headers $headers `
    -Body $body

  $failed = $response.results | Where-Object { $_.code -ne 0 }

  if ($failed) {
    foreach ($r in $failed) {
      Write-Warning "Failed to delete FileId=$($r.file_id): $($r.status)"
    }

    throw "One or more files failed to delete."
  }
}

Export-ModuleMember -Function `
  Invoke-EgressApiRequest, `
  Connect-EgressServiceAccount, `
  Remove-EgressFiles
