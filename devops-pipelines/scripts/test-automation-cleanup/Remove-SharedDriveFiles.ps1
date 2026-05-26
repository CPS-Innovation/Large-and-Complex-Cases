function Get-FilteredFileList {
    param (
        [Parameter(Mandatory = $true)]
        [object]$ApiResponse,

        # Include pattern (regex) – only items that MATCH will be returned
        [string]$IncludePattern,

        # Exclude pattern (regex) – items that MATCH will be removed
        [string]$ExcludePattern
    )

    # Handle JSON string input
    if ($ApiResponse -is [string]) {
        $ApiResponse = $ApiResponse | ConvertFrom-Json
    }

    $results = @()

    # Helper function to test filters
    function Test-PathFilter($path) {
        $includePass = $true
        $excludePass = $true

        if ($IncludePattern) {
            $includePass = $path -match $IncludePattern
        }

        if ($ExcludePattern) {
            $excludePass = -not ($path -match $ExcludePattern)
        }

        return ($includePass -and $excludePass)
    }

    # Folders
    foreach ($folder in $ApiResponse.data.folderData) {
        if (Test-PathFilter $folder.path) {
            $results += [PSCustomObject]@{
                Path = $folder.path
                Type = "Folder"
            }
        }
    }

    # Files
    foreach ($file in $ApiResponse.data.fileData) {
        if (Test-PathFilter $file.path) {
            $results += [PSCustomObject]@{
                Path = $file.path
                Type = "File"
            }
        }
    }

    return $results
}