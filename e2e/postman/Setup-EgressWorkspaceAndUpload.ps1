<#
.SYNOPSIS
    Create Egress workspace and upload one or more generated files to it.

.DESCRIPTION
    This script combines workspace creation and file upload:
    1. Creates a new Egress workspace (auto-incrementing name)
    2. Adds user to workspace
    3. Uploads one or more dynamically generated files of specified size
    4. Verifies files are indexed before completing

    Configuration is loaded from:
    - secrets.config.ps1 (for local development)
    - Environment variables (for CI/CD pipelines)

.PARAMETER SizeGB
    File size in gigabytes (e.g., 1, 2, 0.5)

.PARAMETER SizeMB
    File size in megabytes (e.g., 100, 500)

.PARAMETER FileCount
    Number of files to create and upload (default: 1)

.PARAMETER WorkspaceName
    Custom workspace name (optional - auto-generates if not provided)

.PARAMETER WorkspaceNumber
    Specific workspace number to use (e.g., 5 creates "Automation-Testing5")

.PARAMETER UserEmail
    Email to add as admin. If not provided, reads from config/environment.

.PARAMETER FileName
    Custom filename prefix (optional - auto-generates if not provided)

.PARAMETER FolderPath
    Destination folder in workspace (default: "4. Served Evidence/")

.PARAMETER ChunkSizeMB
    Upload chunk size in megabytes (default: 5, range: 1-100)

.PARAMETER SkipUpload
    Create workspace only, skip file upload

.EXAMPLE
    .\Setup-EgressWorkspaceAndUpload.ps1 -SizeMB 100
    # Uses config from secrets.config.ps1 or environment variables

.EXAMPLE
    .\Setup-EgressWorkspaceAndUpload.ps1 -SizeGB 1 -UserEmail "user@cps.gov.uk"
    # Override with specific email

.EXAMPLE
    .\Setup-EgressWorkspaceAndUpload.ps1 -SizeGB 1 -FileCount 3
    # Upload 3 files of 1GB each
#>

param(
    [Parameter(Mandatory=$false)]
    [double]$SizeGB = 0,

    [Parameter(Mandatory=$false)]
    [double]$SizeMB = 0,

    [Parameter(Mandatory=$false)]
    [int]$FileCount = 1,

    [Parameter(Mandatory=$false)]
    [string]$WorkspaceName = "",

    [Parameter(Mandatory=$false)]
    [int]$WorkspaceNumber = -1,

    [Parameter(Mandatory=$false)]
    [Alias("AzureUsername")]
    [string]$UserEmail = "",

    [Parameter(Mandatory=$false)]
    [string]$FileName = "",

    [Parameter(Mandatory=$false)]
    [string]$FolderPath = "4. Served Evidence/",

    [Parameter(Mandatory=$false)]
    [double]$ChunkSizeMB = 5,

    [Parameter(Mandatory=$false)]
    [switch]$SkipUpload
)

# ============================================================
# CONFIGURATION - Update these values for your environment  
# ============================================================
$script:Config = @{
    # Default folder path in Egress workspace for uploads
    DefaultUploadFolder = "4. Served Evidence/"
    
    # Workspace name prefix for automation
    WorkspaceNamePrefix = "AUTOMATION-TESTING"
}
# ============================================================



# ============================================================
# LOAD CONFIGURATION
# ============================================================
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $ScriptDir) { $ScriptDir = Get-Location }

# Try to load secrets file
$SecretsFile = Join-Path $ScriptDir "secrets.config.ps1"
if (Test-Path $SecretsFile) {
    Write-Host "[CONFIG] Loading from secrets.config.ps1" -ForegroundColor Cyan
    . $SecretsFile
} else {
    Write-Host "[CONFIG] Using environment variables (no secrets.config.ps1 found)" -ForegroundColor Yellow
}

# Load configuration from environment variables
$BaseUrl = if ($env:LCC_EGRESS_BASE_URL) { $env:LCC_EGRESS_BASE_URL } else { 
    Write-Error "LCC_EGRESS_BASE_URL not set. Create secrets.config.ps1 or set environment variable."
    exit 1
}

$ServiceAccountAuth = if ($env:LCC_EGRESS_SERVICE_ACCOUNT_AUTH) { $env:LCC_EGRESS_SERVICE_ACCOUNT_AUTH } else {
    Write-Error "LCC_EGRESS_SERVICE_ACCOUNT_AUTH not set. Create secrets.config.ps1 or set environment variable."
    exit 1
}

$TemplateId = if ($env:LCC_EGRESS_TEMPLATE_ID) { $env:LCC_EGRESS_TEMPLATE_ID } else { "59a6855307087630eb190282" }
$AdminRoleId = if ($env:LCC_EGRESS_ADMIN_ROLE_ID) { $env:LCC_EGRESS_ADMIN_ROLE_ID } else { "591dab08368b665c9c5c5fe0" }

# Get user email from parameter, environment, or leave empty
if ([string]::IsNullOrEmpty($UserEmail)) {
    $UserEmail = $env:LCC_AZURE_USERNAME
}

if ([string]::IsNullOrEmpty($UserEmail)) {
    Write-Host "[WARNING] No UserEmail provided - workspace will be created without adding a user" -ForegroundColor Yellow
}

Write-Host "[CONFIG] Egress URL: $BaseUrl" -ForegroundColor Gray
Write-Host "[CONFIG] User Email: $(if ($UserEmail) { $UserEmail } else { '(not set)' })" -ForegroundColor Gray

# Validate file count
if ($FileCount -lt 1) { $FileCount = 1 }
if ($FileCount -gt 100) { $FileCount = 100 }  # Safety limit

# Validate and set chunk size (min 1MB, max 100MB)
if ($ChunkSizeMB -lt 1) { $ChunkSizeMB = 1 }
if ($ChunkSizeMB -gt 100) { $ChunkSizeMB = 100 }
$ChunkSizeBytes = [long]($ChunkSizeMB * 1024 * 1024)

# Retry configuration for large files
$MaxFileIdRetries = 10
$FileIdRetryDelaySeconds = 5

# ============================================================
# VALIDATE PARAMETERS
# ============================================================
if (-not $SkipUpload) {
    if ($SizeGB -le 0 -and $SizeMB -le 0) {
        Write-Host ""
        Write-Host "ERROR: File size required!" -ForegroundColor Red
        Write-Host ""
        Write-Host "Usage:" -ForegroundColor Yellow
        Write-Host "  .\Setup-EgressWorkspaceAndUpload.ps1 -SizeMB 100                    # 1 x 100 MB file"
        Write-Host "  .\Setup-EgressWorkspaceAndUpload.ps1 -SizeGB 1                      # 1 x 1 GB file"
        Write-Host "  .\Setup-EgressWorkspaceAndUpload.ps1 -SizeGB 1 -FileCount 3         # 3 x 1 GB files"
        Write-Host "  .\Setup-EgressWorkspaceAndUpload.ps1 -SizeMB 500 -FileCount 5       # 5 x 500 MB files"
        Write-Host "  .\Setup-EgressWorkspaceAndUpload.ps1 -SkipUpload                    # Workspace only"
        Write-Host ""
        exit 1
    }
}

# Calculate file size
$FileSize = 0
$SizeLabel = "0MB"
if ($SizeGB -gt 0) {
    $FileSize = [long]($SizeGB * 1024 * 1024 * 1024)
    $SizeLabel = "$($SizeGB)GB"
} elseif ($SizeMB -gt 0) {
    $FileSize = [long]($SizeMB * 1024 * 1024)
    $SizeLabel = "$($SizeMB)MB"
}

$TotalChunks = if ($FileSize -gt 0) { [Math]::Ceiling($FileSize / $ChunkSizeBytes) } else { 0 }

# Determine if this is a large file (needs longer indexing time)
$IsLargeFile = $FileSize -gt (500 * 1024 * 1024)  # > 500MB

# Calculate total upload size
$TotalUploadSize = $FileSize * $FileCount
$TotalUploadLabel = if ($TotalUploadSize -ge 1GB) {
    "$([Math]::Round($TotalUploadSize / 1GB, 2)) GB"
} else {
    "$([Math]::Round($TotalUploadSize / 1MB, 2)) MB"
}

# ============================================================
# HEADER
# ============================================================
Write-Host ""
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "  EGRESS WORKSPACE SETUP & MULTI-FILE UPLOAD" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
if (-not $SkipUpload) {
    Write-Host "File Count:   $FileCount file(s)"
    Write-Host "File Size:    $([Math]::Round($FileSize / 1MB, 2)) MB ($SizeLabel) each"
    Write-Host "Total Size:   $TotalUploadLabel"
    Write-Host "Chunk Size:   $ChunkSizeMB MB"
    Write-Host "Chunks/File:  $TotalChunks"
    Write-Host "Destination:  $(if ($FolderPath) { $FolderPath } else { '(root)' })"
    if ($IsLargeFile) {
        Write-Host "Large Files:  Yes (extended verification enabled)" -ForegroundColor Yellow
    }
}
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host ""

# ============================================================
# STEP 1: Get Auth Token
# ============================================================
Write-Host "[1/8] Authenticating with Egress..." -ForegroundColor Yellow

$tokenJson = curl.exe --silent --location "$BaseUrl/api/v1/user/auth/" `
    --header "Accept: application/json" `
    --header "Authorization: Basic $ServiceAccountAuth"

$tokenObj = $tokenJson | ConvertFrom-Json
if (-not $tokenObj.token) {
    Write-Error "Failed to get token: $tokenJson"
    exit 1
}

$Token = $tokenObj.token
$TokenBase64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($Token))
Write-Host "  [OK] Token obtained (expires in $($tokenObj.duration)s)" -ForegroundColor Green

# ============================================================
# STEP 2: Generate Workspace Name
# ============================================================
if ([string]::IsNullOrEmpty($WorkspaceName)) {
    Write-Host "[2/8] Finding next workspace number..." -ForegroundColor Yellow

    # Get all workspaces
    $allWorkspaces = @()
    $page = 1
    do {
        $wsResult = curl.exe --silent --location "$BaseUrl/api/v1/workspaces/?page=$page" `
            --header "Authorization: Basic $TokenBase64"
        $wsObj = $wsResult | ConvertFrom-Json
        $allWorkspaces += $wsObj.data
        $page++
    } while ($wsObj.pagination.next_url -ne "")

    # Find highest Automation-Testing number
    $maxNum = -1
    foreach ($ws in $allWorkspaces) {
        if ($ws.name -eq "Automation-Testing" -or $ws.name -eq "AUTOMATION-TESTING") {
            if ($maxNum -lt 0) { $maxNum = 0 }
        }
        elseif ($ws.name -match "^(?i)Automation-Testing(\d+)$") {
            $num = [int]$matches[1]
            if ($num -gt $maxNum) { $maxNum = $num }
        }
    }

    # Generate name
    if ($WorkspaceNumber -ge 0) {
        $WorkspaceName = if ($WorkspaceNumber -eq 0) { "AUTOMATION-TESTING" } else { "AUTOMATION-TESTING$WorkspaceNumber" }
    } else {
        $nextNum = $maxNum + 1
        $WorkspaceName = if ($nextNum -eq 0) { "AUTOMATION-TESTING" } else { "AUTOMATION-TESTING$nextNum" }
    }

    Write-Host "  [OK] Workspace name: $WorkspaceName" -ForegroundColor Green
} else {
    Write-Host "[2/8] Using provided name: $WorkspaceName" -ForegroundColor Yellow
}

# ============================================================
# STEP 3: Create Workspace
# ============================================================
Write-Host "[3/8] Creating workspace..." -ForegroundColor Yellow

$bodyFile = Join-Path $env:TEMP "egress_create.json"
@{
    name = $WorkspaceName
    template_id = $TemplateId
    description = "Automation test workspace - Created $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - $FileCount file(s)"
} | ConvertTo-Json -Compress | Set-Content $bodyFile -Encoding UTF8

$createResult = curl.exe --silent --location --request POST "$BaseUrl/api/v1/workspaces/" `
    --header "Authorization: Basic $TokenBase64" `
    --header "Content-Type: application/json" `
    --data "@$bodyFile"

Remove-Item $bodyFile -Force -ErrorAction SilentlyContinue

$WorkspaceId = ""
try {
    $createObj = $createResult | ConvertFrom-Json
    if ($createObj.id) {
        $WorkspaceId = $createObj.id
        Write-Host "  [OK] Workspace created: $WorkspaceId" -ForegroundColor Green
    } else {
        Write-Error "Failed to create workspace: $createResult"
        exit 1
    }
} catch {
    Write-Error "Failed to parse response: $createResult"
    exit 1
}

# ============================================================
# STEP 4: Add User
# ============================================================
if ($UserEmail) {
    Write-Host "[4/8] Adding user: $UserEmail..." -ForegroundColor Yellow

    $addUserBody = '[{"switch_id":"' + $UserEmail + '","role_id":"' + $AdminRoleId + '"}]'
    $bodyFile = Join-Path $env:TEMP "egress_adduser.json"
    $addUserBody | Set-Content $bodyFile -Encoding UTF8

    $addResult = curl.exe --silent --location --request POST "$BaseUrl/api/v1/workspaces/$WorkspaceId/users/" `
        --header "Authorization: Basic $TokenBase64" `
        --header "Content-Type: application/json" `
        --data "@$bodyFile"

    Remove-Item $bodyFile -Force -ErrorAction SilentlyContinue

    # Verify user was added
    $usersResult = curl.exe --silent --location "$BaseUrl/api/v1/workspaces/$WorkspaceId/users/" `
        --header "Authorization: Basic $TokenBase64"

    if ($usersResult -match $UserEmail) {
        Write-Host "  [OK] User added as Administrator" -ForegroundColor Green
    } else {
        Write-Host "  [WARN] User may not have been added" -ForegroundColor Yellow
    }
} else {
    Write-Host "[4/8] Skipping user add (no email provided)" -ForegroundColor Gray
}

# ============================================================
# INITIALIZE ARRAYS FOR MULTI-FILE TRACKING
# ============================================================
$UploadedFiles = @()  # Array to track all uploaded files

# ============================================================
# SKIP UPLOAD IF REQUESTED
# ============================================================
if ($SkipUpload) {
    Write-Host "[5/8] Skipping file upload (-SkipUpload specified)" -ForegroundColor Gray
    Write-Host "[6/8] Skipping..." -ForegroundColor Gray
    Write-Host "[7/8] Skipping..." -ForegroundColor Gray
    Write-Host "[8/8] Skipping..." -ForegroundColor Gray

    $FolderId = ""
    $TotalTime = [TimeSpan]::Zero
} else {
    # ============================================================
    # STEP 5: Create Folder (if needed)
    # ============================================================
    Write-Host "[5/8] Preparing upload destination..." -ForegroundColor Yellow

    $ActualFolderPath = $FolderPath
    $FolderId = ""

    if ($FolderPath -and $FolderPath -ne "") {
        Write-Host "  Upload destination: $FolderPath" -ForegroundColor Cyan
    } else {
        Write-Host "  Uploading to root folder" -ForegroundColor Gray
    }

    # ============================================================
    # STEP 6-7: Upload Multiple Files
    # ============================================================
    $OverallStartTime = Get-Date

    for ($fileIndex = 1; $fileIndex -le $FileCount; $fileIndex++) {
        Write-Host ""
        Write-Host "========================================================" -ForegroundColor Magenta
        Write-Host "  UPLOADING FILE $fileIndex OF $FileCount" -ForegroundColor Magenta
        Write-Host "========================================================" -ForegroundColor Magenta

        # Generate unique filename for this file
        $timestamp = Get-Date -Format "yyyy-MM-dd-HH-mm-ss"
        if ([string]::IsNullOrEmpty($FileName)) {
            $CurrentFileName = "generated-$SizeLabel-$timestamp-file$fileIndex.txt"
        } else {
            if ($FileCount -gt 1) {
                $CurrentFileName = "$FileName-$fileIndex.txt"
            } else {
                $CurrentFileName = "$FileName.txt"
            }
        }

        Write-Host "[6/8] Initiating upload for: $CurrentFileName" -ForegroundColor Yellow

        # Initiate upload for this file
        $initBodyFile = Join-Path $env:TEMP "egress_init.json"
        $initBody = @{
            filename = $CurrentFileName
            filesize = $FileSize
        }

        if ($ActualFolderPath -and $ActualFolderPath -ne "") {
            $initBody.folder_path = $ActualFolderPath
        }

        $initBody | ConvertTo-Json | Set-Content $initBodyFile -Encoding UTF8

        $initJson = curl.exe --silent --location "$BaseUrl/api/v1/workspaces/$WorkspaceId/uploads" `
            --header "Content-Type: application/json" `
            --header "Authorization: Basic $TokenBase64" `
            --data "@$initBodyFile"

        Remove-Item $initBodyFile -Force -ErrorAction SilentlyContinue

        $initObj = $initJson | ConvertFrom-Json
        if (-not $initObj.id) {
            if ($ActualFolderPath -and $ActualFolderPath -ne "") {
                Write-Host "  Failed with folder, trying root..." -ForegroundColor Yellow
                $ActualFolderPath = ""
                $FolderId = ""

                $initBody = @{
                    filename = $CurrentFileName
                    filesize = $FileSize
                }
                $initBody | ConvertTo-Json | Set-Content $initBodyFile -Encoding UTF8

                $initJson = curl.exe --silent --location "$BaseUrl/api/v1/workspaces/$WorkspaceId/uploads" `
                    --header "Content-Type: application/json" `
                    --header "Authorization: Basic $TokenBase64" `
                    --data "@$initBodyFile"

                Remove-Item $initBodyFile -Force -ErrorAction SilentlyContinue
                $initObj = $initJson | ConvertFrom-Json
            }

            if (-not $initObj.id) {
                Write-Host "  [FAILED] Could not initiate upload for $CurrentFileName" -ForegroundColor Red
                Write-Host "  Error: $initJson" -ForegroundColor Red
                continue
            }
        }

        $UploadId = $initObj.id
        Write-Host "  [OK] Upload initiated (ID: $UploadId)" -ForegroundColor Green

        # Upload chunks
        Write-Host "[7/8] Generating and uploading $TotalChunks chunks..." -ForegroundColor Yellow

        $Random = New-Object System.Random
        $BytesUploaded = 0
        $ChunkNum = 1
        $FileStartTime = Get-Date
        $TempFile = Join-Path $env:TEMP "egress_chunk.tmp"
        $uploadSuccess = $true

        try {
            while ($BytesUploaded -lt $FileSize) {
                $BytesToUpload = [Math]::Min($ChunkSizeBytes, $FileSize - $BytesUploaded)

                # Generate random bytes
                $ChunkData = New-Object byte[] $BytesToUpload
                $Random.NextBytes($ChunkData)

                # Add header to first chunk
                if ($ChunkNum -eq 1) {
                    $header = @"
============================================================
EGRESS GENERATED TEST FILE
============================================================
Generated:   $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Filename:    $CurrentFileName
File:        $fileIndex of $FileCount
Size:        $([Math]::Round($FileSize / 1GB, 3)) GB ($FileSize bytes)
Workspace:   $WorkspaceName
WorkspaceId: $WorkspaceId
Folder:      $ActualFolderPath
============================================================

"@
                    $headerBytes = [Text.Encoding]::UTF8.GetBytes($header)
                    [Array]::Copy($headerBytes, $ChunkData, [Math]::Min($headerBytes.Length, $ChunkData.Length))
                }

                # Write to temp file
                [System.IO.File]::WriteAllBytes($TempFile, $ChunkData)

                # Calculate Content-Range
                $StartByte = $BytesUploaded
                $EndByte = $BytesUploaded + $BytesToUpload - 1
                $ContentRange = "bytes $StartByte-$EndByte/$FileSize"

                # Progress
                $Percent = [Math]::Round(($BytesUploaded / $FileSize) * 100, 1)
                $Elapsed = (Get-Date) - $FileStartTime
                $Speed = if ($Elapsed.TotalSeconds -gt 0) { [Math]::Round($BytesUploaded / $Elapsed.TotalSeconds / 1MB, 2) } else { 0 }
                $ETA = if ($Speed -gt 0) { [Math]::Round(($FileSize - $BytesUploaded) / 1MB / $Speed / 60, 1) } else { 0 }

                Write-Host "  [$ChunkNum/$TotalChunks] $Percent% | $Speed MB/s | ETA: ${ETA}m" -NoNewline

                # Upload chunk with retry
                $chunkSuccess = $false
                $chunkRetries = 3
                for ($retry = 1; $retry -le $chunkRetries; $retry++) {
                    $chunkResult = curl.exe --silent --location --request PATCH `
                        "$BaseUrl/api/v1/workspaces/$WorkspaceId/uploads/$UploadId/" `
                        --header "Authorization: Basic $TokenBase64" `
                        --header "Content-Range: $ContentRange" `
                        --form "file_content=@$TempFile"

                    if ($chunkResult -match '"error_code"') {
                        if ($retry -lt $chunkRetries) {
                            Write-Host " RETRY($retry)" -ForegroundColor Yellow -NoNewline
                            Start-Sleep -Seconds 2
                        }
                    } else {
                        $chunkSuccess = $true
                        break
                    }
                }

                if (-not $chunkSuccess) {
                    Write-Host " FAILED" -ForegroundColor Red
                    Write-Error "Chunk $ChunkNum failed after $chunkRetries retries"
                    $uploadSuccess = $false
                    break
                }

                Write-Host " OK" -ForegroundColor Green

                $BytesUploaded += $BytesToUpload
                $ChunkNum++

                $ChunkData = $null
                [System.GC]::Collect()
            }
        }
        finally {
            Remove-Item $TempFile -Force -ErrorAction SilentlyContinue
        }

        if (-not $uploadSuccess) {
            Write-Host "  [FAILED] Upload failed for $CurrentFileName" -ForegroundColor Red
            continue
        }

        Write-Host "  [OK] All $($ChunkNum - 1) chunks uploaded!" -ForegroundColor Green

        # Complete upload
        Write-Host "[8/8] Completing upload..." -ForegroundColor Yellow

        $completeBodyFile = Join-Path $env:TEMP "egress_complete.json"
        '{"done":true}' | Set-Content $completeBodyFile -Encoding UTF8

        $completeSuccess = $false
        $completeRetries = if ($IsLargeFile) { 5 } else { 3 }

        for ($retry = 1; $retry -le $completeRetries; $retry++) {
            Write-Host "  Sending completion request (attempt $retry/$completeRetries)..." -NoNewline

            $completeResult = curl.exe --silent --location --request PUT `
                "$BaseUrl/api/v1/workspaces/$WorkspaceId/uploads/$UploadId/" `
                --header "Authorization: Basic $TokenBase64" `
                --header "Content-Type: application/json" `
                --data "@$completeBodyFile"

            if ($completeResult -match '"error_code"') {
                Write-Host " RETRY" -ForegroundColor Yellow
                Start-Sleep -Seconds 3
            } else {
                Write-Host " OK" -ForegroundColor Green
                $completeSuccess = $true
                break
            }
        }

        Remove-Item $completeBodyFile -Force -ErrorAction SilentlyContinue

        # Verify file
        Write-Host "  Verifying file upload..." -ForegroundColor Yellow

        $FileId = ""
        $ActualFilePath = ""
        $retryCount = if ($IsLargeFile) { $MaxFileIdRetries } else { 5 }
        $retryDelay = if ($IsLargeFile) { $FileIdRetryDelaySeconds } else { 2 }

        for ($i = 1; $i -le $retryCount; $i++) {
            Write-Host "  Attempt $i/$retryCount..." -NoNewline

            Start-Sleep -Seconds $retryDelay

            $filesResult = curl.exe --silent --location "$BaseUrl/api/v1/workspaces/$WorkspaceId/files?folder_id=$FolderId" `
                --header "Authorization: Basic $TokenBase64"

            try {
                $filesObj = $filesResult | ConvertFrom-Json
                $uploadedFile = $filesObj | Where-Object { $_.name -eq $CurrentFileName -and $_.isFolder -eq $false }

                if ($uploadedFile) {
                    $FileId = $uploadedFile.id
                    $ActualFilePath = if ($uploadedFile.path) { $uploadedFile.path + "/" } else { "" }

                    if ($uploadedFile.filesize -eq $FileSize) {
                        Write-Host " FOUND!" -ForegroundColor Green
                        break
                    } else {
                        Write-Host " SIZE MISMATCH" -ForegroundColor Yellow
                    }
                } else {
                    Write-Host " not indexed yet" -ForegroundColor Gray
                }
            } catch {
                Write-Host " error" -ForegroundColor Yellow
            }
        }

        if (-not $FileId) {
            Write-Host "  [WARN] Using upload ID as fallback" -ForegroundColor Yellow
            $FileId = $UploadId
            $ActualFilePath = $ActualFolderPath
        }

        $FileTime = (Get-Date) - $FileStartTime
        $FileSpeed = [Math]::Round($FileSize / $FileTime.TotalSeconds / 1MB, 2)

        # Add to tracking array
        $UploadedFiles += @{
            Index = $fileIndex
            FileName = $CurrentFileName
            FileId = $FileId
            UploadId = $UploadId
            FileSize = $FileSize
            FilePath = $ActualFilePath
            UploadTime = $FileTime
            Speed = $FileSpeed
        }

        Write-Host "  [OK] File $fileIndex complete: $CurrentFileName (ID: $FileId)" -ForegroundColor Green
    }

    $TotalTime = (Get-Date) - $OverallStartTime
}

# ============================================================
# OUTPUT SUMMARY
# ============================================================
Write-Host ""
Write-Host "========================================================" -ForegroundColor Green
Write-Host "  COMPLETE!" -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Green
Write-Host ""
Write-Host "WORKSPACE DETAILS:" -ForegroundColor Cyan
Write-Host "  Workspace ID:   $WorkspaceId"
Write-Host "  Workspace Name: $WorkspaceName"
Write-Host "  User:           $UserEmail"
Write-Host ""

if (-not $SkipUpload -and $UploadedFiles.Count -gt 0) {
    Write-Host "UPLOADED FILES ($($UploadedFiles.Count) of $FileCount):" -ForegroundColor Cyan
    Write-Host ""

    foreach ($file in $UploadedFiles) {
        Write-Host "  File $($file.Index):" -ForegroundColor White
        Write-Host "    Name:     $($file.FileName)"
        Write-Host "    ID:       $($file.FileId)"
        Write-Host "    Size:     $([Math]::Round($file.FileSize / 1MB, 2)) MB"
        Write-Host "    Path:     $(if ($file.FilePath) { $file.FilePath } else { '(root)/' })$($file.FileName)"
        Write-Host "    Time:     $([Math]::Round($file.UploadTime.TotalMinutes, 1)) min @ $($file.Speed) MB/s"
        Write-Host ""
    }

    $TotalAvgSpeed = [Math]::Round(($UploadedFiles.Count * $FileSize) / $TotalTime.TotalSeconds / 1MB, 2)
    Write-Host "UPLOAD SUMMARY:" -ForegroundColor Cyan
    Write-Host "  Total Files:    $($UploadedFiles.Count)"
    Write-Host "  Total Size:     $TotalUploadLabel"
    Write-Host "  Total Time:     $([Math]::Round($TotalTime.TotalMinutes, 1)) minutes"
    Write-Host "  Avg Speed:      $TotalAvgSpeed MB/s"
    Write-Host ""
}

Write-Host "========================================================" -ForegroundColor Yellow
Write-Host "  POSTMAN VARIABLES" -ForegroundColor Yellow
Write-Host "========================================================" -ForegroundColor Yellow
Write-Host "egressWorkspaceId = $WorkspaceId"
Write-Host "egressWorkspaceName = $WorkspaceName"
if (-not $SkipUpload -and $UploadedFiles.Count -gt 0) {
    $firstFile = $UploadedFiles[0]
    Write-Host "egressFileId = $($firstFile.FileId)"
    Write-Host "egressUploadId = $($firstFile.UploadId)"
    Write-Host "egressFolderId = $FolderId"
    Write-Host "egressFileName = $($firstFile.FileName)"
    Write-Host "egressUploadFolderPath = $($firstFile.FilePath)"

    $allFileNames = ($UploadedFiles | ForEach-Object { $_.FileName }) -join ","
    $allFileIds = ($UploadedFiles | ForEach-Object { $_.FileId }) -join ","
    Write-Host ""
    Write-Host "# All files (comma-separated):"
    Write-Host "egressFileNames = $allFileNames"
    Write-Host "egressFileIds = $allFileIds"
    Write-Host "egressFileCount = $($UploadedFiles.Count)"
}
Write-Host ""

Write-Host "========================================================" -ForegroundColor Yellow
Write-Host "  ADO PIPELINE OUTPUT" -ForegroundColor Yellow
Write-Host "========================================================" -ForegroundColor Yellow
Write-Host "##vso[task.setvariable variable=egressWorkspaceId]$WorkspaceId"
Write-Host "##vso[task.setvariable variable=egressWorkspaceName]$WorkspaceName"
if (-not $SkipUpload -and $UploadedFiles.Count -gt 0) {
    $firstFile = $UploadedFiles[0]
    Write-Host "##vso[task.setvariable variable=egressFileId]$($firstFile.FileId)"
    Write-Host "##vso[task.setvariable variable=egressUploadId]$($firstFile.UploadId)"
    Write-Host "##vso[task.setvariable variable=egressFolderId]$FolderId"
    Write-Host "##vso[task.setvariable variable=egressFileName]$($firstFile.FileName)"
    Write-Host "##vso[task.setvariable variable=egressUploadFolderPath]$($firstFile.FilePath)"

    $allFileNames = ($UploadedFiles | ForEach-Object { $_.FileName }) -join ","
    $allFileIds = ($UploadedFiles | ForEach-Object { $_.FileId }) -join ","
    Write-Host "##vso[task.setvariable variable=egressFileNames]$allFileNames"
    Write-Host "##vso[task.setvariable variable=egressFileIds]$allFileIds"
    Write-Host "##vso[task.setvariable variable=egressFileCount]$($UploadedFiles.Count)"
}
Write-Host ""

Write-Host "========================================================" -ForegroundColor Magenta
Write-Host "  DEFENDANT NAME FOR CASE REGISTRATION" -ForegroundColor Magenta
Write-Host "========================================================" -ForegroundColor Magenta
Write-Host "defendantSurname = $WorkspaceName"
Write-Host ""
Write-Host "Use this as defendant surname to link case with workspace!"
Write-Host "========================================================" -ForegroundColor Magenta

# Output JSON summary for programmatic parsing
if (-not $SkipUpload -and $UploadedFiles.Count -gt 0) {
    Write-Host ""
    Write-Host "========================================================" -ForegroundColor DarkGray
    Write-Host "  JSON OUTPUT (for programmatic parsing)" -ForegroundColor DarkGray
    Write-Host "========================================================" -ForegroundColor DarkGray

    $jsonOutput = @{
        workspaceId = $WorkspaceId
        workspaceName = $WorkspaceName
        folderId = $FolderId
        fileCount = $UploadedFiles.Count
        files = $UploadedFiles | ForEach-Object {
            @{
                index = $_.Index
                fileName = $_.FileName
                fileId = $_.FileId
                uploadId = $_.UploadId
                fileSize = $_.FileSize
                filePath = $_.FilePath
            }
        }
    }

    $jsonOutput | ConvertTo-Json -Depth 3 -Compress
}
