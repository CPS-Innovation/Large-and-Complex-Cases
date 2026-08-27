param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Files
)

if (-not $Files -or $Files.Count -eq 0) {
    exit 0
}

$projectGroups = $Files |
    Group-Object { ($_ -split '[\\/]', 2)[0] }

foreach ($group in $projectGroups) {
    $projectDir = $group.Name

    Push-Location $projectDir

    try {
        $projectFiles = $group.Group| ForEach-Object { $_ -replace "^$projectDir[\\/]", "" }

        npx prettier --write $projectFiles

        if ($LASTEXITCODE -ne 0) {
            exit $LASTEXITCODE
        }
    }
    finally {
        Pop-Location
    }
}
