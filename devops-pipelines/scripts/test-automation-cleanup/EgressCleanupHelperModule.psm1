function Invoke-EgressApiRequest {
  param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$Method,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$Uri,

    [Parameter(Mandatory = $true)]
    [ValidateNotNull()]
    [hashtable]$Headers,

    [string]$Body,

    [ValidateRange(1, 3600)]
    [int]$TimeoutSec = 60,

    [ValidateRange(1, 10)]
    [int]$MaxAttempts = 3
  )

  if (-not [uri]::IsWellFormedUriString($Uri, [UriKind]::Absolute)) {
    throw "Uri must be a valid absolute URL."
  }

  $retryableStatusCodes = @(429, 502, 503, 504)
  $maxRetryDelaySec = 60

  for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
    try {
      $params = @{
        Method      = $Method
        Uri         = $Uri
        Headers     = $Headers
        TimeoutSec  = $TimeoutSec
        ErrorAction = 'Stop'
      }

      if ($PSBoundParameters.ContainsKey('Body') -and $null -ne $Body) {
        $params.Body = $Body
        $params.ContentType = 'application/json'
      }

      return Invoke-RestMethod @params
    }
    catch {
      $response = $_.Exception.Response
      $statusCode = $null

      if ($null -ne $response -and $null -ne $response.StatusCode) {
        $statusCode = [int]$response.StatusCode
      }

      $retryable = $statusCode -in $retryableStatusCodes

      # Retry only known transient network failures when no HTTP status is available.
      if ($null -eq $statusCode) {
        $exception = $_.Exception

        while ($null -ne $exception) {
          if ($exception -is [System.Net.Sockets.SocketException]) {
            $retryable = $true
            break
          }

          if ($exception -is [System.Net.Http.HttpRequestException]) {
            $retryable = $true
            break
          }

          if ($exception -is [System.TimeoutException]) {
            $retryable = $true
            break
          }

          if ($exception -is [System.Threading.Tasks.TaskCanceledException]) {
            $retryable = $true
            break
          }

          if ($exception -is [System.Net.WebException]) {
            if ($exception.Status -in @(
              [System.Net.WebExceptionStatus]::Timeout,
              [System.Net.WebExceptionStatus]::ConnectFailure,
              [System.Net.WebExceptionStatus]::ConnectionClosed,
              [System.Net.WebExceptionStatus]::ReceiveFailure,
              [System.Net.WebExceptionStatus]::SendFailure,
              [System.Net.WebExceptionStatus]::NameResolutionFailure
            )) {
              $retryable = $true
              break
            }
          }

          $exception = $exception.InnerException
        }
      }

      if (-not $retryable -or $attempt -eq $MaxAttempts) {
        throw
      }

      # Exponential backoff with jitter.
      $baseDelay = [Math]::Min(
        2 * [Math]::Pow(2, $attempt - 1),
        30
      )

      $delay = [Math]::Min(
        $baseDelay + (Get-Random -Minimum 0 -Maximum 3),
        $maxRetryDelaySec
      )

      # Respect Retry-After when the server supplies a valid value.
      $retryAfterHeader = $null

      if ($null -ne $response) {
        if ($response -is [System.Net.Http.HttpResponseMessage]) {
          $retryAfterHeader = $response.Headers.RetryAfter.ToString()
        }
        elseif ($response -is [System.Net.HttpWebResponse]) {
          $retryAfterHeader = $response.Headers['Retry-After']
        }
      }

      if (-not [string]::IsNullOrWhiteSpace([string]$retryAfterHeader)) {
        $retryAfterSeconds = 0

        if ([int]::TryParse(
          [string]$retryAfterHeader,
          [ref]$retryAfterSeconds
        )) {
          if ($retryAfterSeconds -ge 0) {
            $delay = [Math]::Min(
              $retryAfterSeconds,
              $maxRetryDelaySec
            )
          }
        }
        else {
          $retryAfterDate = [DateTimeOffset]::MinValue

          if ([DateTimeOffset]::TryParse(
            [string]$retryAfterHeader,
            [ref]$retryAfterDate
          )) {
            $secondsUntilRetry = (
              $retryAfterDate - [DateTimeOffset]::UtcNow
            ).TotalSeconds

            if ($secondsUntilRetry -ge 0) {
              $delay = [Math]::Min(
                [Math]::Ceiling($secondsUntilRetry),
                $maxRetryDelaySec
              )
            }
          }
        }
      }

      $delay = [int][Math]::Max(0, $delay)

      if ($null -ne $statusCode) {
        Write-Warning `
          "Egress returned HTTP $statusCode. Retrying in $delay seconds (attempt $attempt/$MaxAttempts)..."
      }
      else {
        Write-Warning `
          "Egress request failed with a transient network error. Retrying in $delay seconds (attempt $attempt/$MaxAttempts)..."
      }

      if ($delay -gt 0) {
        Start-Sleep -Seconds $delay
      }
    }
  }
}

function Connect-EgressServiceAccount {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$BaseUrl,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$AuthToken
  )

  if (-not [uri]::IsWellFormedUriString($BaseUrl, [UriKind]::Absolute)) {
    throw "BaseUrl must be a valid absolute URL."
  }

  try {
    $tokenObj = Invoke-EgressApiRequest `
      -Method Get `
      -Uri "$($BaseUrl.TrimEnd('/'))/api/v1/user/auth/" `
      -Headers @{
        Accept        = "application/json"
        Authorization = "Basic $AuthToken"
      }
  }
  catch {
    $exception = [System.Exception]::new(
      "Authentication failed: $($_.Exception.Message)",
      $_.Exception
    )

    $errorRecord = [System.Management.Automation.ErrorRecord]::new(
      $exception,
      'EgressAuthenticationFailed',
      [System.Management.Automation.ErrorCategory]::AuthenticationError,
      $BaseUrl
    )

    $PSCmdlet.ThrowTerminatingError($errorRecord)
  }

  if ([string]::IsNullOrWhiteSpace([string]$tokenObj.token)) {
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
    [ValidateNotNullOrEmpty()]
    [string]$BaseUrl,

    [Parameter(Mandatory = $true)]
    [ValidateNotNull()]
    [hashtable]$AuthorizationHeader,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$WorkspaceId,

    [Parameter(Mandatory = $true)]
    [ValidateNotNull()]
    [string[]]$FileIds
  )

  if (-not [uri]::IsWellFormedUriString($BaseUrl, [UriKind]::Absolute)) {
    throw "BaseUrl must be a valid absolute URL."
  }

  if ($FileIds.Count -eq 0) {
    throw "FileIds must contain at least one file ID."
  }

  if (@($FileIds | Where-Object {
    [string]::IsNullOrWhiteSpace($_)
  }).Count -gt 0) {
    throw "FileIds must not contain null, empty or whitespace-only values."
  }

  # Avoid submitting the same file ID more than once.
  $FileIds = @($FileIds | Select-Object -Unique)

  $headers = $AuthorizationHeader

  $body = @{
    file_ids = $FileIds
  } | ConvertTo-Json

  $response = Invoke-EgressApiRequest `
    -Method Delete `
    -Uri "$($BaseUrl.TrimEnd('/'))/api/v1/workspaces/$WorkspaceId/files" `
    -Headers $headers `
    -Body $body `
    -TimeoutSec 180

  # A successful HTTP status alone does not confirm successful deletion.
  if (
    $null -eq $response -or
    $null -eq $response.PSObject.Properties['all_successful'] -or
    $response.all_successful -isnot [bool]
  ) {
    throw "Egress returned an invalid deletion response: all_successful is missing or is not a boolean."
  }

  if ($response.all_successful) {
    return
  }

  if (
    $null -eq $response.PSObject.Properties['results'] -or
    $null -eq $response.results
  ) {
    throw "Egress reported deletion failure but returned no results."
  }

  $results = @($response.results)

  if ($results.Count -eq 0) {
    throw "Egress reported deletion failure but returned an empty results collection."
  }

  foreach ($result in $results) {
    if (
      $null -eq $result -or
      $null -eq $result.PSObject.Properties['code'] -or
      $null -eq $result.code
    ) {
      throw "Egress returned a deletion result without a valid code."
    }

    $resultCode = 0

    if (-not [int]::TryParse(
      [string]$result.code,
      [ref]$resultCode
    )) {
      throw "Egress returned an invalid deletion result code."
    }

    if ($resultCode -ne 0) {
      $fileId = $result.file_id
      $reason = $result.reason

      if ([string]::IsNullOrWhiteSpace([string]$reason)) {
        $reason = $result.status
      }

      if ([string]::IsNullOrWhiteSpace([string]$reason)) {
        $reason = "Error code $resultCode"
      }

      Write-Warning "Failed to delete FileId=$fileId : $reason"
    }
  }

  $failed = @($results | Where-Object {
    [int]$_.code -ne 0
  })

  if ($failed.Count -eq 0) {
    throw "Egress reported all_successful=false but returned no failed file results."
  }

  throw "One or more files failed to delete ($($failed.Count) failed)."
}

Export-ModuleMember -Function `
  Invoke-EgressApiRequest, `
  Connect-EgressServiceAccount, `
  Remove-EgressFiles
