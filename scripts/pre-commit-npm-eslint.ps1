param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Files
)

$projectGroups = $Files |
    Group-Object { ($_ -split '[\\/]', 2)[0] }

foreach ($group in $projectGroups) {
    $projectDir = $group.Name

    if (-not (Test-Path "$projectDir/eslint.config.js")) {
        continue
    }

    Write-Host "Linting $projectDir"

    npx eslint `
        --config "$projectDir/eslint.config.js" `
        --fix `
        $group.Group

    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}
