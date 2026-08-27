param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Files
)

if (-not $Files -or $Files.Count -eq 0) {
    exit 0
}

$projectDir = "ui-spa"

Push-Location $projectDir

try {
    $Files = $Files | ForEach-Object { $_ -replace "^$projectDir[\\/]", "" }

    npx prettier --write $Files

    exit $LASTEXITCODE
}
finally {
    Pop-Location
}
