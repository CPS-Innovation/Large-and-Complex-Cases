param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Files
)

if (-not $Files -or $Files.Count -eq 0) {
    exit 0
}

$solution="backend/CPS.ComplexCases.sln"

dotnet format $solution `
    --no-restore `
    --include `
    $Files

git diff --name-only -- $Files

exit $LASTEXITCODE
