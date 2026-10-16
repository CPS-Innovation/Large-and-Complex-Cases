
Import-Module (Join-Path $PSScriptRoot 'EgressCleanupHelperModule.psm1') -Force

Describe "Assert-EgressDeletionResponse" {
  It "returns without warning when all_successful is true" {
    InModuleScope EgressCleanupHelperModule {
      Mock Write-EgressDeletionWarnings {
        return 1
      }

      $response = [pscustomobject]@{
        all_successful = $true
      }

      { Assert-EgressDeletionResponse -Response $response } |
      Should Not Throw

      Assert-MockCalled Write-EgressDeletionWarnings -Times 0
    }
  }

  It "throws when the response is null" {
    InModuleScope EgressCleanupHelperModule {
      {
        Assert-EgressDeletionResponse -Response $null
      } | Should Throw "Egress returned an invalid deletion response: all_successful is missing or is not a boolean."
    }
  }

  It "throws when all_successful is missing" {
    InModuleScope EgressCleanupHelperModule {
      $response = [pscustomobject]@{}

      {
        Assert-EgressDeletionResponse -Response $response
      } | Should Throw "Egress returned an invalid deletion response: all_successful is missing or is not a boolean."
    }
  }

  It "throws when all_successful is not a boolean" {
    InModuleScope EgressCleanupHelperModule {
      $response = [pscustomobject]@{
        all_successful = "false"
      }

      {
        Assert-EgressDeletionResponse -Response $response
      } | Should Throw "Egress returned an invalid deletion response: all_successful is missing or is not a boolean."
    }
  }

  It "throws when both results and files are missing" {
    InModuleScope EgressCleanupHelperModule {
      $response = [pscustomobject]@{
        all_successful = $false
      }

      {
        Assert-EgressDeletionResponse -Response $response
      } | Should Throw "Egress returned an invalid deletion response: results collection is missing."
    }
  }

  It "throws when results and files are null" {
    InModuleScope EgressCleanupHelperModule {
      $response = [pscustomobject]@{
        all_successful = $false
        results        = $null
        files          = $null
      }

      {
        Assert-EgressDeletionResponse -Response $response
      } | Should Throw "Egress returned an invalid deletion response: results collection is missing."
    }
  }

  It "throws when the results collection is empty" {
    InModuleScope EgressCleanupHelperModule {
      Mock Write-EgressDeletionWarnings {
        return 1
      }

      $response = [pscustomobject]@{
        all_successful = $false
        results        = @()
      }

      {
        Assert-EgressDeletionResponse -Response $response
      } | Should Throw "Egress reported deletion failure but returned an empty results collection."

      Assert-MockCalled Write-EgressDeletionWarnings -Times 0
    }
  }

  It "uses files when results is missing" {
    InModuleScope EgressCleanupHelperModule {
      Mock Write-EgressDeletionWarnings {
        return 1
      }

      $response = [pscustomobject]@{
        all_successful = $false
        files          = @(
          [pscustomobject]@{ file_id = "file-1" }
        )
      }

      {
        Assert-EgressDeletionResponse -Response $response
      } | Should Throw "One or more files failed to delete (1 failed)."

      Assert-MockCalled Write-EgressDeletionWarnings -Times 1
    }
  }

  It "uses files when results is null" {
    InModuleScope EgressCleanupHelperModule {
      Mock Write-EgressDeletionWarnings {
        return 1
      }

      $response = [pscustomobject]@{
        all_successful = $false
        results        = $null
        files          = @(
          [pscustomobject]@{ file_id = "file-1" }
        )
      }

      {
        Assert-EgressDeletionResponse -Response $response
      } | Should Throw "One or more files failed to delete (1 failed)."

      Assert-MockCalled Write-EgressDeletionWarnings -Times 1
    }
  }

  It "throws when all results are reported successful despite all_successful being false" {
    InModuleScope EgressCleanupHelperModule {
      Mock Write-EgressDeletionWarnings {
        return 0
      }

      $response = [pscustomobject]@{
        all_successful = $false
        results        = @(
          [pscustomobject]@{ file_id = "file-1" }
        )
      }

      {
        Assert-EgressDeletionResponse -Response $response
      } | Should Throw "Egress reported all_successful=false but returned no failed file results."

      Assert-MockCalled Write-EgressDeletionWarnings -Times 1
    }
  }

  It "throws with the failed file count when multiple files fail" {
    InModuleScope EgressCleanupHelperModule {
      Mock Write-EgressDeletionWarnings {
        return 2
      }

      $response = [pscustomobject]@{
        all_successful = $false
        results        = @(
          [pscustomobject]@{ file_id = "file-1" }
          [pscustomobject]@{ file_id = "file-2" }
        )
      }

      {
        Assert-EgressDeletionResponse -Response $response
      } | Should Throw "One or more files failed to delete (2 failed)."

      Assert-MockCalled Write-EgressDeletionWarnings -Times 1
    }
  }
}
