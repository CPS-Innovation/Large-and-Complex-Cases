function Connect-EgressServiceAccount {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory = $true)]
    [string]$BaseUrl,

    [Parameter(Mandatory = $true)]
    [string]$AuthToken
  )

  try {
    $tokenObj = Invoke-RestMethod -Method Get `
      -Uri "$BaseUrl/api/v1/user/auth/" `
      -Headers @{
        Accept        = "application/json"
        Authorization = "Basic $AuthToken"
      } `
      -ContentType 'application/json'
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

  $headers =  $AuthorizationHeader + @{
    "Content-Type" = "application/json"
  }

  $body = @{
    file_ids = $FileIds
  } | ConvertTo-Json

  $response = Invoke-RestMethod -Method Delete `
    -Uri "$BaseUrl/api/v1/workspaces/$WorkspaceId/files" `
    -Headers $headers `
    -Body $body `
    -ContentType 'application/json'
  
  $failed = $response.results | Where-Object { $_.code -ne 0 }

  if ($failed) {
    foreach ($r in $failed) {
      Write-Warning "Failed to delete FileId=$($r.file_id): $($r.status)"
    }

    throw "One or more files failed to delete."
  }
}


Export-ModuleMember -Function Connect-EgressServiceAccount, Remove-EgressFiles