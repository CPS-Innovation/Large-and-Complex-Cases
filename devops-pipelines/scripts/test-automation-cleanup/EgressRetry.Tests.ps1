Describe 'Invoke-EgressApiRequest retry logic' {
    BeforeAll {
        Import-Module (Join-Path $PSScriptRoot 'EgressCleanupHelperModule.psm1') -Force
    }

    It 'retries a 429 response and returns the subsequent successful response' {
        InModuleScope EgressCleanupHelperModule {
            $script:attemptCount = 0

            Mock Invoke-RestMethod {
                $script:attemptCount++

                if ($script:attemptCount -eq 1) {
                    # Simulate an HTTP 429 response.
                    $exception = [System.Exception]::new('Simulated HTTP 429')
                    $exception | Add-Member -MemberType NoteProperty `
                        -Name Response `
                        -Value ([pscustomobject]@{
                            StatusCode = 429
                            Headers    = @{}
                        })

                    throw $exception
                }

                # Second attempt succeeds.
                return [pscustomobject]@{
                    success = $true
                }
            }

            Mock Start-Sleep {}

            $result = Invoke-EgressApiRequest `
                -Method Get `
                -Uri 'https://example.com/test' `
                -Headers @{} `
                -MaxAttempts 3

            $result.success | Should Be $true
            $script:attemptCount | Should Be 2

            Assert-MockCalled Invoke-RestMethod -Times 2 -Exactly -Scope It
            Assert-MockCalled Start-Sleep -Times 1 -Exactly -Scope It
        }
    }


  It 'does not retry a non-retryable HTTP 400 response' {
    InModuleScope EgressCleanupHelperModule {
      $script:attemptCount = 0

      Mock Invoke-RestMethod {
        $script:attemptCount++

        $exception = [System.Exception]::new('Simulated HTTP 400')
        $exception | Add-Member -MemberType NoteProperty `
          -Name Response `
          -Value ([pscustomobject]@{
            StatusCode = 400
            Headers    = @{}
          })

        throw $exception
      }

      Mock Start-Sleep {}

      {
        Invoke-EgressApiRequest `
          -Method Get `
          -Uri 'https://example.com/test' `
          -Headers @{} `
          -MaxAttempts 3
      } | Should Throw

      $script:attemptCount | Should Be 1
      Assert-MockCalled Invoke-RestMethod -Times 1 -Exactly -Scope It
      Assert-MockCalled Start-Sleep -Times 0 -Exactly -Scope It
    }
  }

  It 'stops after MaxAttempts when every request returns HTTP 503' {
    InModuleScope EgressCleanupHelperModule {
      $script:attemptCount = 0

      Mock Invoke-RestMethod {
        $script:attemptCount++

        $exception = [System.Exception]::new('Simulated HTTP 503')
        $exception | Add-Member -MemberType NoteProperty `
          -Name Response `
          -Value ([pscustomobject]@{
            StatusCode = 503
            Headers    = @{}
          })

        throw $exception
      }

      Mock Start-Sleep {}

      {
        Invoke-EgressApiRequest `
          -Method Get `
          -Uri 'https://example.com/test' `
          -Headers @{} `
          -MaxAttempts 3
      } | Should Throw

      $script:attemptCount | Should Be 3
      Assert-MockCalled Invoke-RestMethod -Times 3 -Exactly -Scope It
      Assert-MockCalled Start-Sleep -Times 2 -Exactly -Scope It
    }
  }

  It 'makes only one request when MaxAttempts is 1' {
    InModuleScope EgressCleanupHelperModule {
      $script:attemptCount = 0

      Mock Invoke-RestMethod {
        $script:attemptCount++

        $exception = [System.Exception]::new('Simulated HTTP 503')
        $exception | Add-Member -MemberType NoteProperty `
          -Name Response `
          -Value ([pscustomobject]@{
            StatusCode = 503
            Headers    = @{}
          })

        throw $exception
      }

      Mock Start-Sleep {}

      {
        Invoke-EgressApiRequest `
          -Method Get `
          -Uri 'https://example.com/test' `
          -Headers @{} `
          -MaxAttempts 1
      } | Should Throw

      $script:attemptCount | Should Be 1
      Assert-MockCalled Invoke-RestMethod -Times 1 -Exactly -Scope It
      Assert-MockCalled Start-Sleep -Times 0 -Exactly -Scope It
    }
  }

  It 'can report failure when a DELETE succeeds but its response is lost' {
    InModuleScope EgressCleanupHelperModule {
      $script:attemptCount = 0

      Mock Invoke-RestMethod {
        $script:attemptCount++

        if ($script:attemptCount -eq 1) {
          # Simulate the server processing the DELETE, but the connection
          # closing before the client receives the response.
          throw [System.Net.WebException]::new(
            'Simulated lost response',
            [System.Net.WebExceptionStatus]::ConnectionClosed
          )
        }

        # The retry finds the resource already deleted.
        $exception = [System.Exception]::new('Simulated HTTP 404')
        $exception | Add-Member -MemberType NoteProperty `
          -Name Response `
          -Value ([pscustomobject]@{
            StatusCode = 404
            Headers    = @{}
          })

        throw $exception
      }

      Mock Start-Sleep {}

      {
        Invoke-EgressApiRequest `
          -Method Delete `
          -Uri 'https://example.com/api/v1/workspaces/test-id' `
          -Headers @{} `
          -MaxAttempts 3
      } | Should Throw

      # First attempt: response lost.
      # Second attempt: HTTP 404.
      $script:attemptCount | Should Be 2
      Assert-MockCalled Invoke-RestMethod -Times 2 -Exactly -Scope It
      Assert-MockCalled Start-Sleep -Times 1 -Exactly -Scope It
    }
  }
}
