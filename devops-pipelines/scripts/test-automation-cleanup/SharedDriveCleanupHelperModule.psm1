function Get-AzureAdBearerHeader {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory = $true)]
    [string]$ClientId,

    [Parameter(Mandatory = $true)]
    [string]$TenantId,

    [Parameter(Mandatory = $true)]
    [string]$AadUsername,

    [Parameter(Mandatory = $true)]
    [string]$AadPassword
  )

  $body = @{
    client_id  = $ClientId
    scope      = "api://${ClientId}/user_impersonation"
    username   = $AadUsername
    password   = $AadPassword
    grant_type = "password"
  }

  try {
    $tokenObj = Invoke-RestMethod -Method Post `
      -Uri "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/token" `
      -ContentType 'application/x-www-form-urlencoded' `
      -Body $body
  }
  catch {
    throw "Authentication failed: $($_.Exception.Message)"
  }

  $aadToken = $tokenObj.access_token

  if (-not $aadToken) {
    Write-Warning "Token Object: $tokenObj"
    throw "The request was successful but the response does not contain an access token."
  }

  return @{
    Authorization = "Bearer $aadToken"
  }
}

function Remove-SharedDriveObjects {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory = $true)]
    [int]$CaseId,

    [Parameter(Mandatory = $true)]
    [hashtable]$AuthHeader,

    [Parameter(Mandatory = $true)]
    [array]$ObjectsToDelete,

    [Parameter(Mandatory = $true)]
    [string]$BaseUrl
  )

  $operations = @(
    foreach ($item in $ObjectsToDelete) {
      $path = $item.Path

      if ($item.Type -eq "Folder" -and -not $path.EndsWith("/")) {
        $path = "$path/"
      }

      @{
        type       = $item.Type
        sourcePath = $path
      }
    }
  )

  $body = @{
    caseId     = $CaseId
    operations = $operations
  } | ConvertTo-Json -Depth 5

  Write-Host $body
  
  try {
    $response = Invoke-RestMethod -Method Post `
      -Uri "$BaseUrl/api/v1/netapp/delete/batch" `
      -Headers $AuthHeader `
      -ContentType 'application/json' `
      -Body $body

    if ($response.notFound -gt 0) {
      Write-Warning "$($response.notFound) item(s) were not found."
    }

    if ($response.failed -gt 0) {
      Write-Error "$($response.failed) item(s) failed to delete."
    }

    # Return structured per-item results
    $results = foreach ($item in $response.results) {
      [PSCustomObject]@{
        Path        = $item.sourcePath
        Status      = $item.status
        Succeeded   = ($item.status -eq "Deleted")
        NotFound    = ($item.status -eq "NotFound")
        Failed      = ($item.status -eq "Failed")
        KeysDeleted = $item.keysDeleted
        Error       = $item.error
      }
    }

    return $results
  }
  catch {
    throw
  }
}

Export-ModuleMember -Function Get-AzureAdBearerHeader, Remove-SharedDriveObjects