function Get-EgressHttpStatusCode {
  param([object]$Response)

  if ($null -ne $Response -and $null -ne $Response.StatusCode) {
    return [int]$Response.StatusCode
  }

  return $null
}

function Test-EgressTransientNetworkError {
  param([System.Exception]$Exception)

  $retryableWebStatuses = @(
    [System.Net.WebExceptionStatus]::Timeout,
    [System.Net.WebExceptionStatus]::ConnectFailure,
    [System.Net.WebExceptionStatus]::ConnectionClosed,
    [System.Net.WebExceptionStatus]::ReceiveFailure,
    [System.Net.WebExceptionStatus]::SendFailure,
    [System.Net.WebExceptionStatus]::NameResolutionFailure
  )

  while ($null -ne $Exception) {
    if (
      $Exception -is [System.Net.Sockets.SocketException] -or
      $Exception -is [System.Net.Http.HttpRequestException] -or
      $Exception -is [System.TimeoutException] -or
      $Exception -is [System.Threading.Tasks.TaskCanceledException]
    ) {
      return $true
    }

    if (
      $Exception -is [System.Net.WebException] -and
      $Exception.Status -in $retryableWebStatuses
    ) {
      return $true
    }

    $Exception = $Exception.InnerException
  }

  return $false
}

function Get-EgressRetryAfterHeader {
  param([object]$Response)

  if ($Response -is [System.Net.Http.HttpResponseMessage]) {
    $retryAfter = $Response.Headers.RetryAfter

    if ($null -ne $retryAfter) {
      return $retryAfter.ToString()
    }
  }
  elseif ($Response -is [System.Net.HttpWebResponse]) {
    return $Response.Headers['Retry-After']
  }

  return $null
}

function Get-EgressRetryAfterDelay {
  param(
    [string]$Header,
    [int]$MaxDelaySec
  )

  if ([string]::IsNullOrWhiteSpace($Header)) {
    return $null
  }

  $seconds = 0

  if ([int]::TryParse($Header, [ref]$seconds)) {
    if ($seconds -ge 0) {
      return [int][Math]::Min($seconds, $MaxDelaySec)
    }

    return $null
  }

  $retryDate = [DateTimeOffset]::MinValue

  if ([DateTimeOffset]::TryParse($Header, [ref]$retryDate)) {
    $secondsUntilRetry = (
      $retryDate - [DateTimeOffset]::UtcNow
    ).TotalSeconds

    if ($secondsUntilRetry -ge 0) {
      return [int][Math]::Min(
        [Math]::Ceiling($secondsUntilRetry),
        $MaxDelaySec
      )
    }
  }

  return $null
}

function Get-EgressRetryDelay {
  param(
    [int]$Attempt,
    [object]$Response,
    [int]$MaxDelaySec = 60
  )

  $baseDelay = [Math]::Min(
    2 * [Math]::Pow(2, $Attempt - 1),
    30
  )

  $delay = [Math]::Min(
    $baseDelay + (Get-Random -Minimum 0 -Maximum 3),
    $MaxDelaySec
  )

  $retryAfter = Get-EgressRetryAfterHeader -Response $Response
  $serverDelay = Get-EgressRetryAfterDelay `
    -Header $retryAfter `
    -MaxDelaySec $MaxDelaySec

  if ($null -ne $serverDelay) {
    $delay = $serverDelay
  }

  return [int][Math]::Max(0, $delay)
}

function Test-EgressRetryableError {
  param(
    [System.Management.Automation.ErrorRecord]$ErrorRecord
  )

  $response = $ErrorRecord.Exception.Response
  $statusCode = Get-EgressHttpStatusCode -Response $response

  if ($null -ne $statusCode) {
    return $statusCode -in @(429, 502, 503, 504)
  }

  return Test-EgressTransientNetworkError `
    -Exception $ErrorRecord.Exception
}

function Invoke-EgressApiRequestOnce {
  param(
    [string]$Method,
    [string]$Uri,
    [hashtable]$Headers,
    [string]$Body,
    [bool]$HasBody,
    [int]$TimeoutSec
  )

  $params = @{
    Method      = $Method
    Uri         = $Uri
    Headers     = $Headers
    TimeoutSec  = $TimeoutSec
    ErrorAction = 'Stop'
  }

  if ($HasBody) {
    $params.Body = $Body
    $params.ContentType = 'application/json'
  }

  Invoke-RestMethod @params
}

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

  $hasBody = $PSBoundParameters.ContainsKey('Body') -and $null -ne $Body

  for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
    try {
      return Invoke-EgressApiRequestOnce `
        -Method $Method `
        -Uri $Uri `
        -Headers $Headers `
        -Body $Body `
        -HasBody $hasBody `
        -TimeoutSec $TimeoutSec
    }
    catch {
      if (-not (Test-EgressRetryableError -ErrorRecord $_)) {
        throw
      }

      if ($attempt -eq $MaxAttempts) {
        throw
      }

      $response = $_.Exception.Response
      $statusCode = Get-EgressHttpStatusCode -Response $response

      $delay = Get-EgressRetryDelay `
        -Attempt $attempt `
        -Response $response

      Write-EgressRetryWarning `
        -StatusCode $statusCode `
        -Delay $delay `
        -Attempt $attempt `
        -MaxAttempts $MaxAttempts

      if ($delay -gt 0) {
        Start-Sleep -Seconds $delay
      }
    }
  }
}

function Write-EgressRetryWarning {
  param(
    [Nullable[int]]$StatusCode,
    [int]$Delay,
    [int]$Attempt,
    [int]$MaxAttempts
  )

  if ($null -ne $StatusCode) {
    Write-Warning (
      "Egress returned HTTP {0}. Retrying in {1} seconds (attempt {2}/{3})..." -f
      $StatusCode, $Delay, $Attempt, $MaxAttempts
    )
    return
  }

  Write-Warning (
    "Egress request failed with a transient network error. Retrying in {0} seconds (attempt {1}/{2})..." -f
    $Delay, $Attempt, $MaxAttempts
  )
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
