<#
.SYNOPSIS
    Update Postman environment file with workspace and file variables.

.DESCRIPTION
    Utility script to manually update environment file with Egress workspace 
    and file details. Useful when you need to update variables without 
    running the full E2E test suite.

    Note: Run-E2E-Tests.ps1 does this automatically - this script is only 
    needed for manual updates.

.PARAMETER EnvironmentFile
    Path to the Postman environment file to update

.PARAMETER WorkspaceId
    Egress workspace ID

.PARAMETER WorkspaceName
    Egress workspace name (also used as defendant surname)

.PARAMETER FileId
    Primary file ID (first file)

.PARAMETER FileName
    Primary file name (first file)

.PARAMETER FileIds
    Comma-separated list of all file IDs (for multi-file uploads)

.PARAMETER FileNames
    Comma-separated list of all file names (for multi-file uploads)

.PARAMETER FileCount
    Number of files uploaded

.EXAMPLE
    .\update-env-multifile.ps1 -WorkspaceId "abc123" -WorkspaceName "AUTOMATION-TESTING5" -FileId "def456" -FileName "test.txt"

.EXAMPLE
    .\update-env-multifile.ps1 -WorkspaceId "abc123" -WorkspaceName "AUTOMATION-TESTING5" -FileIds "id1,id2,id3" -FileNames "file1.txt,file2.txt,file3.txt" -FileCount 3
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$EnvironmentFile = ".\LCCTestEnvironment_updated.postman_environment",

    [Parameter(Mandatory=$true)]
    [string]$WorkspaceId,

    [Parameter(Mandatory=$true)]
    [string]$WorkspaceName,

    [Parameter(Mandatory=$false)]
    [string]$FileId = "",

    [Parameter(Mandatory=$false)]
    [string]$FileName = "",

    [Parameter(Mandatory=$false)]
    [string]$FileIds = "",

    [Parameter(Mandatory=$false)]
    [string]$FileNames = "",

    [Parameter(Mandatory=$false)]
    [int]$FileCount = 1
)

# Check if environment file exists
if (-not (Test-Path $EnvironmentFile)) {
    Write-Host "ERROR: Environment file not found: $EnvironmentFile" -ForegroundColor Red
    exit 1
}

Write-Host "Updating environment file: $EnvironmentFile" -ForegroundColor Cyan
Write-Host ""

$env = Get-Content $EnvironmentFile -Raw | ConvertFrom-Json

# Build variables to update
$varsToUpdate = @{
    'egressWorkspaceId' = $WorkspaceId
    'egressWorkspaceName' = $WorkspaceName
    'defendantSurname' = $WorkspaceName
    'searchDefendantName' = $WorkspaceName
}

# Add file variables if provided
if ($FileId) { $varsToUpdate['egressFileId'] = $FileId }
if ($FileName) { $varsToUpdate['egressFileName'] = $FileName }
if ($FileIds) { $varsToUpdate['egressFileIds'] = $FileIds }
if ($FileNames) { $varsToUpdate['egressFileNames'] = $FileNames }
if ($FileCount -gt 0) { $varsToUpdate['egressFileCount'] = [string]$FileCount }

# Update environment
foreach ($key in $varsToUpdate.Keys) {
    $value = $varsToUpdate[$key]
    $found = $false
    
    for ($i = 0; $i -lt $env.values.Count; $i++) {
        if ($env.values[$i].key -eq $key) {
            $env.values[$i].value = $value
            $found = $true
            Write-Host "  Updated: $key = $value" -ForegroundColor Green
            break
        }
    }
    
    if (-not $found) {
        $env.values += @{ key = $key; value = $value; type = 'default'; enabled = $true }
        Write-Host "  Added: $key = $value" -ForegroundColor Yellow
    }
}

$env | ConvertTo-Json -Depth 100 | Set-Content $EnvironmentFile -Encoding UTF8

Write-Host ""
Write-Host "Environment updated successfully!" -ForegroundColor Green
