Describe 'Invoke-EgressApiRequest retry logic' {
  BeforeAll {
    Import-Module (Join-Path $PSScriptRoot 'EgressCleanupHelperModule.psm1') -Force

    InModuleScope EgressCleanupHelperModule {
      function script:New-TestHttpException {
        param(
          [Parameter(Mandatory)]
          [int]$StatusCode
        )

        $exception = [System.Exception]::new(
          "Simulated HTTP $StatusCode"
        )

        $exception | Add-Member -MemberType NoteProperty `
          -Name Response `
          -Value ([pscustomobject]@{
            StatusCode = $StatusCode
            Headers    = @{}
          })

        return $exception
      }

      function script:Set-TestRequestOutcomes {
        param(
          [Parameter(Mandatory)]
          [object[]]$Outcomes
        )

        $script:attemptCount = 0
        $script:mockOutcomes = $Outcomes
      }

      function script:Assert-TestRequestCounts {
        param(
          [Parameter(Mandatory)]
          [int]$Requests,

          [Parameter(Mandatory)]
          [int]$Sleeps
        )

        $script:attemptCount | Should Be $Requests

        Assert-MockCalled Invoke-RestMethod `
          -Times $Requests -Exactly -Scope It

        Assert-MockCalled Start-Sleep `
          -Times $Sleeps -Exactly -Scope It
      }
    }
  }

  BeforeEach {
    InModuleScope EgressCleanupHelperModule {
      Mock Invoke-RestMethod {
        $script:attemptCount++

        $outcome = $script:mockOutcomes[
        $script:attemptCount - 1
        ]

        if ($outcome.ContainsKey('Exception')) {
          throw $outcome.Exception
        }

        return $outcome.Response
      }

      Mock Start-Sleep {}
    }
  }

  It 'retries a 429 response and returns the subsequent successful response' {
    InModuleScope EgressCleanupHelperModule {
      Set-TestRequestOutcomes -Outcomes @(
        @{
          Exception = New-TestHttpException -StatusCode 429
        },
        @{
          Response = [pscustomobject]@{ success = $true }
        }
      )

      $result = Invoke-EgressApiRequest `
        -Method Get `
        -Uri 'https://example.com/test' `
        -Headers @{} `
        -MaxAttempts 3

      $result.success | Should Be $true
      Assert-TestRequestCounts -Requests 2 -Sleeps 1
    }
  }

  It 'does not retry a non-retryable HTTP 400 response' {
    InModuleScope EgressCleanupHelperModule {
      Set-TestRequestOutcomes -Outcomes @(
        @{
          Exception = New-TestHttpException -StatusCode 400
        }
      )

      {
        Invoke-EgressApiRequest `
          -Method Get `
          -Uri 'https://example.com/test' `
          -Headers @{} `
          -MaxAttempts 3
      } | Should Throw

      Assert-TestRequestCounts -Requests 1 -Sleeps 0
    }
  }

  It 'stops after MaxAttempts when every request returns HTTP 503' {
    InModuleScope EgressCleanupHelperModule {
      Set-TestRequestOutcomes -Outcomes @(
        @{ Exception = New-TestHttpException -StatusCode 503 },
        @{ Exception = New-TestHttpException -StatusCode 503 },
        @{ Exception = New-TestHttpException -StatusCode 503 }
      )

      {
        Invoke-EgressApiRequest `
          -Method Get `
          -Uri 'https://example.com/test' `
          -Headers @{} `
          -MaxAttempts 3
      } | Should Throw

      Assert-TestRequestCounts -Requests 3 -Sleeps 2
    }
  }

  It 'makes only one request when MaxAttempts is 1' {
    InModuleScope EgressCleanupHelperModule {
      Set-TestRequestOutcomes -Outcomes @(
        @{
          Exception = New-TestHttpException -StatusCode 503
        }
      )

      {
        Invoke-EgressApiRequest `
          -Method Get `
          -Uri 'https://example.com/test' `
          -Headers @{} `
          -MaxAttempts 1
      } | Should Throw

      Assert-TestRequestCounts -Requests 1 -Sleeps 0
    }
  }

  It 'can report failure when a DELETE succeeds but its response is lost' {
    InModuleScope EgressCleanupHelperModule {
      Set-TestRequestOutcomes -Outcomes @(
        @{
          Exception = [System.Net.WebException]::new(
            'Simulated lost response',
            [System.Net.WebExceptionStatus]::ConnectionClosed
          )
        },
        @{
          Exception = New-TestHttpException -StatusCode 404
        }
      )

      {
        Invoke-EgressApiRequest `
          -Method Delete `
          -Uri 'https://example.com/api/v1/workspaces/test-id' `
          -Headers @{} `
          -MaxAttempts 3
      } | Should Throw

      Assert-TestRequestCounts -Requests 2 -Sleeps 1
    }
  }
}
