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

    if (-not (Test-Path "$projectDir/.prettierrc")) {
        continue
    }

    $projectFiles = $group.Group | ForEach-Object {
        $_ -replace "^$projectDir[\\/]", ""
    }

    Push-Location $projectDir

    try {
        npx --no prettier -- `
            --write `
            $projectFiles

        $exitCode = $LASTEXITCODE
    }
    finally {
        Pop-Location
    }

    if ($exitCode -ne 0) {
        exit $exitCode
    }
}
