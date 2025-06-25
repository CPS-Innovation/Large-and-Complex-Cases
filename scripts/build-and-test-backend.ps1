# LACC - Local Backend Build and Test Script
# This script handles building, testing, and packaging the backend solution

param(
    [Parameter(Mandatory=$false)]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [string]$BackendPath = "backend",
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "build-output",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipTests,
    
    [Parameter(Mandatory=$false)]
    [switch]$RunMigration,
    
    [Parameter(Mandatory=$false)]
    [string]$ConnectionString = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$VerboseOutput
)

# Set strict mode and error action
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "Starting CPS Complex Cases Backend Build Process..." -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Backend Path: $BackendPath" -ForegroundColor Yellow
Write-Host "Output Path: $OutputPath" -ForegroundColor Yellow
Write-Host ""

# Resolve and validate paths
$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptRoot
$BackendFullPath = Join-Path $ProjectRoot $BackendPath
$OutputFullPath = Join-Path $ProjectRoot $OutputPath

if (-not (Test-Path $BackendFullPath)) {
    Write-Host "ERROR: Backend path not found: $BackendFullPath" -ForegroundColor Red
    exit 1
}

$SolutionFile = Join-Path $BackendFullPath "CPS.ComplexCases.sln"
if (-not (Test-Path $SolutionFile)) {
    Write-Host "ERROR: Solution file not found: $SolutionFile" -ForegroundColor Red
    exit 1
}

$TestResultsPath = Join-Path $OutputFullPath "test-results"

# Clean and create output directories
Write-Host "Cleaning previous build artifacts..." -ForegroundColor Cyan

# Clean build-output directory
if (Test-Path $OutputFullPath) {
    Remove-Item -Path $OutputFullPath -Recurse -Force
    Write-Host "Cleaned existing build output directory" -ForegroundColor Green
}

# Clean coverage-reports directory
$CoverageReportsPath = Join-Path $ProjectRoot "coverage-reports"
if (Test-Path $CoverageReportsPath) {
    Remove-Item -Path $CoverageReportsPath -Recurse -Force
    Write-Host "Cleaned existing coverage reports directory" -ForegroundColor Green
}

# Create fresh output directories
New-Item -Path $OutputFullPath -ItemType Directory -Force | Out-Null
Write-Host "Created fresh output directory: $OutputFullPath" -ForegroundColor Green

New-Item -Path $TestResultsPath -ItemType Directory -Force | Out-Null
Write-Host "Created fresh test results directory: $TestResultsPath" -ForegroundColor Green

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Cyan
try {
    $dotnetVersion = dotnet --version
    Write-Host "OK .NET SDK Version: $dotnetVersion" -ForegroundColor Green
}
catch {
    Write-Host "ERROR .NET SDK not found" -ForegroundColor Red
    Write-Host "Please install .NET 8 SDK from https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

# Function to run dotnet commands with error handling
function Invoke-DotNetCommand {
    param(
        [string]$Command,
        [string]$Description,
        [string]$WorkingDirectory = $BackendFullPath
    )
    
    Write-Host "$Description..." -ForegroundColor Cyan
    
    Push-Location $WorkingDirectory
    try {
        if ($VerboseOutput) {
            Write-Host "Executing: dotnet $Command" -ForegroundColor Gray
        }
        
        Invoke-Expression "dotnet $Command"
        
        if ($LASTEXITCODE -ne 0) {
            throw "$Description failed with exit code $LASTEXITCODE"
        }
        
        Write-Host "OK $Description completed successfully" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

try {
    # Step 1: Restore packages
    Invoke-DotNetCommand -Command "restore `"$SolutionFile`" --verbosity minimal" -Description "Restoring NuGet packages"
    
    # Step 2: Build solution
    Invoke-DotNetCommand -Command "build `"$SolutionFile`" --configuration $Configuration --no-restore --verbosity minimal" -Description "Building solution"
    
    # Step 3: Run tests (if not skipped)
    if (-not $SkipTests) {
        Write-Host "Running tests with coverage..." -ForegroundColor Cyan
        
        # Find test projects
        $testProjects = Get-ChildItem -Path $BackendFullPath -Name "*Tests.csproj" -Recurse
        
        if ($testProjects.Count -eq 0) {
            Write-Host "No test projects found" -ForegroundColor Yellow
        } else {
            Write-Host "Found $($testProjects.Count) test projects:" -ForegroundColor Yellow
            $testProjects | ForEach-Object { Write-Host "  - $_" -ForegroundColor Gray }
            
            # Run tests with coverage
            $testCommand = "test `"$SolutionFile`" --configuration $Configuration --no-build --logger `"trx;LogFileName=TestResults.trx`" --results-directory `"$TestResultsPath`" --collect:`"XPlat Code Coverage`" --settings `"$BackendFullPath/CodeCoverage.runsettings`" --verbosity minimal"
            
            Invoke-DotNetCommand -Command $testCommand -Description "Running tests with coverage"
            
            # Generate HTML coverage report
            Write-Host "Generating beautiful HTML coverage report..." -ForegroundColor Cyan
            
            $coverageFiles = Get-ChildItem -Path $TestResultsPath -Filter "coverage.cobertura.xml" -Recurse
            
            if ($coverageFiles.Count -gt 0) {
                Write-Host "Found $($coverageFiles.Count) coverage file(s)" -ForegroundColor Green
                
                # Install ReportGenerator
                Write-Host "Installing ReportGenerator tool..." -ForegroundColor Yellow
                $reportGenInstall = dotnet tool install --global dotnet-reportgenerator-globaltool 2>&1
                if ($LASTEXITCODE -ne 0 -and $reportGenInstall -notlike "*already installed*") {
                    Write-Host "ReportGenerator installation result: $reportGenInstall" -ForegroundColor Yellow
                }
                
                # Create reports directory
                $reportsDir = Join-Path $ProjectRoot "coverage-reports"
                $htmlReportDir = Join-Path $reportsDir "html"
                $historyDir = Join-Path $reportsDir "history"
                
                New-Item -Path $htmlReportDir -ItemType Directory -Force | Out-Null
                New-Item -Path $historyDir -ItemType Directory -Force | Out-Null
                
                # Generate report
                $coverageFilePaths = ($coverageFiles.FullName -join ";")
                $timestamp = Get-Date -Format 'yyyy-MM-dd-HH-mm'
                
                Write-Host "Generating comprehensive HTML report..." -ForegroundColor Yellow
                
                # Use & operator with argument array to avoid parsing issues
                $reportGenArgs = @(
                    "-reports:$coverageFilePaths"
                    "-targetdir:$htmlReportDir"
                    "-reporttypes:Html;HtmlSummary;Badges;TextSummary;Cobertura"
                    "-historydir:$historyDir"
                    "-title:CPS Complex Cases Backend Coverage Report"
                    "-tag:$timestamp"
                    "-assemblyfilters:+*;-*.Tests;-*.WireMock"
                    "-classfilters:+*;-*Tests*;-*Mock*;-*Migrations*"
                    "-verbosity:Info"
                )
                
                if ($VerboseOutput) {
                    Write-Host "Executing: reportgenerator $($reportGenArgs -join ' ')" -ForegroundColor Gray
                }
                
                $reportGenResult = & reportgenerator @reportGenArgs 2>&1
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "Beautiful HTML coverage report generated successfully!" -ForegroundColor Green
                    Write-Host "Report location: $htmlReportDir" -ForegroundColor Cyan
                    Write-Host "Open index.html in your browser to view the report" -ForegroundColor Cyan
                    
                    # Try to open report
                    $indexPath = Join-Path $htmlReportDir "index.html"
                    if (Test-Path $indexPath) {
                        try {
                            Start-Process $indexPath
                            Write-Host "Coverage report opened in your default browser" -ForegroundColor Green
                        } catch {
                            Write-Host "Could not auto-open browser. Please manually open: $indexPath" -ForegroundColor Yellow
                        }
                    }
                    
                    # Display summary
                    $summaryPath = Join-Path $htmlReportDir "Summary.txt"
                    if (Test-Path $summaryPath) {
                        Write-Host "`nCoverage Summary:" -ForegroundColor Yellow
                        Get-Content $summaryPath | Where-Object { $_ -match "Line coverage|Branch coverage|Method coverage" } | ForEach-Object {
                            Write-Host "  $_" -ForegroundColor Cyan
                        }
                    }
                } else {
                    Write-Host "Failed to generate HTML coverage report" -ForegroundColor Red
                    Write-Host "Error: $reportGenResult" -ForegroundColor Red
                }
            } else {
                Write-Host "No coverage files found in test results" -ForegroundColor Yellow
            }
            
            # Display test results
            $trxFiles = @(Get-ChildItem -Path $TestResultsPath -Name "*.trx" -Recurse)
            if ($trxFiles.Count -gt 0) {
                Write-Host "Test results saved to:" -ForegroundColor Yellow
                $trxFiles | ForEach-Object { Write-Host "  - $TestResultsPath\$_" -ForegroundColor Gray }
            }
        }
    } else {
        Write-Host "Skipping tests as requested" -ForegroundColor Yellow
    }
    
    # Step 4: Publish Function Apps
    Write-Host "Publishing Function Apps..." -ForegroundColor Cyan
    
    $functionApps = @(
        @{ Name = "MainAPI"; Project = "CPS.ComplexCases.API/CPS.ComplexCases.API.csproj"; OutputDir = "MainAPI" },
        @{ Name = "FileTransferAPI"; Project = "CPS.ComplexCases.FileTransfer.API/CPS.ComplexCases.FileTransfer.API.csproj"; OutputDir = "FileTransferAPI" }
    )
    
    foreach ($app in $functionApps) {
        Write-Host "Publishing $($app.Name)..." -ForegroundColor Yellow
        
        $publishPath = Join-Path $OutputFullPath $app.OutputDir
        $projectPath = Join-Path $BackendFullPath $app.Project
        
        if (Test-Path $projectPath) {
            $publishCommand = "publish `"$projectPath`" --configuration $Configuration --output `"$publishPath`" --no-restore --verbosity minimal"
            Invoke-DotNetCommand -Command $publishCommand -Description "Publishing $($app.Name)"
            
            # Create zip file
            $zipPath = Join-Path $OutputFullPath "$($app.Name).zip"
            if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
            
            Compress-Archive -Path "$publishPath\*" -DestinationPath $zipPath -Force
            Write-Host "OK $($app.Name) packaged to $zipPath" -ForegroundColor Green
        } else {
            Write-Host "WARNING: Project not found: $projectPath" -ForegroundColor Yellow
        }
    }
    
    # Step 5: Generate migration script
    Write-Host "Generating database migration script..." -ForegroundColor Cyan
    
    # Install EF Core tools if needed
    $efInstalled = dotnet tool list --global | Select-String "dotnet-ef"
    if (-not $efInstalled) {
        Write-Host "Installing Entity Framework Core tools..." -ForegroundColor Yellow
        dotnet tool install --global dotnet-ef
    }
    
    $migrationScriptPath = Join-Path $OutputFullPath "migration-script.sql"
    $dataProject = Join-Path $BackendFullPath "CPS.ComplexCases.Data/CPS.ComplexCases.Data.csproj"
    $startupProject = Join-Path $BackendFullPath "CPS.ComplexCases.API/CPS.ComplexCases.API.csproj"
    
    if ((Test-Path $dataProject) -and (Test-Path $startupProject)) {
        $migrationCommand = "ef migrations script --output `"$migrationScriptPath`" --project `"$dataProject`" --startup-project `"$startupProject`""
        if ($VerboseOutput) { $migrationCommand += " --verbose" }
        
        Invoke-DotNetCommand -Command $migrationCommand -Description "Generating migration script"
        
        # Copy migration files
        $migrationsSource = Join-Path $BackendFullPath "CPS.ComplexCases.Data/Migrations"
        if (Test-Path $migrationsSource) {
            $migrationsTarget = Join-Path $OutputFullPath "migrations"
            Copy-Item -Path $migrationsSource -Destination $migrationsTarget -Recurse -Force
            Write-Host "OK Migration files copied to: $migrationsTarget" -ForegroundColor Green
        }
    } else {
        Write-Host "WARNING: Could not find Data or API project for migration generation" -ForegroundColor Yellow
    }
    
    # Step 6: Run database migration (if requested)
    if ($RunMigration) {
        if ([string]::IsNullOrEmpty($ConnectionString)) {
            Write-Host "Connection string not provided. Skipping migration execution." -ForegroundColor Yellow
            Write-Host "Use -ConnectionString parameter to run migrations" -ForegroundColor Gray
        } else {
            Write-Host "Running database migration..." -ForegroundColor Cyan
            
            $env:ConnectionStrings__CaseManagementDatastoreConnection = $ConnectionString
            
            try {
                $updateCommand = "ef database update --project `"$dataProject`" --startup-project `"$startupProject`""
                if ($VerboseOutput) { $updateCommand += " --verbose" }
                
                Invoke-DotNetCommand -Command $updateCommand -Description "Applying database migrations"
            }
            finally {
                Remove-Item Env:ConnectionStrings__CaseManagementDatastoreConnection -ErrorAction SilentlyContinue
            }
        }
    } else {
        Write-Host "Skipping database migration" -ForegroundColor Yellow
    }
    
    # Step 7: Show summary
    Write-Host ""
    Write-Host "Backend Build Summary" -ForegroundColor Green
    Write-Host "====================" -ForegroundColor Green
    
    Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
    Write-Host "Backend Path: $BackendFullPath" -ForegroundColor Yellow
    Write-Host "Output Directory: $OutputFullPath" -ForegroundColor Yellow
    
    if (Test-Path $OutputFullPath) {
        $items = Get-ChildItem -Path $OutputFullPath
        Write-Host "Generated Files:" -ForegroundColor Yellow
        $items | ForEach-Object {
            $size = if ($_.PSIsContainer) { "(folder)" } else { "($([math]::Round($_.Length / 1MB, 2)) MB)" }
            Write-Host "  - $($_.Name) $size" -ForegroundColor Gray
        }
    }
    
    if (-not $SkipTests -and (Test-Path $TestResultsPath)) {
        Write-Host "Test Results: $TestResultsPath" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "Backend build process completed successfully!" -ForegroundColor Green
    
} catch {
    Write-Host ""
    Write-Host "Backend build failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($VerboseOutput) {
        Write-Host $_.ScriptStackTrace -ForegroundColor Red
    }
    exit 1
} 