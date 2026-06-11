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
      Write-Warning "$($response.failed) item(s) failed to delete."
    }

    $results = @{
      Succeeded = @()
      NotFound  = @()
      Failed    = @()
    }

    foreach ($item in $response.results) {
      switch ($item.status) {

        "Deleted" {
          $results.Succeeded += [PSCustomObject]@{
            Path        = $item.sourcePath
            KeysDeleted = $item.keysDeleted
          }
        }

        "NotFound" {
          $results.NotFound += [PSCustomObject]@{
            Path = $item.sourcePath
          }
        }

        "Failed" {
          $results.Failed += [PSCustomObject]@{
            Path  = $item.sourcePath
            Error = $item.error
          }
        }

        default {
          # treat unknown as failure
          $results.Failed += [PSCustomObject]@{
            Path  = $item.sourcePath
            Error = "Unknown status: $($item.status)"
          }
        }
      }
    }

    return $results
  }
  catch {
    throw
  }
}

Export-ModuleMember -Function Get-AzureAdBearerHeader, Remove-SharedDriveObjects