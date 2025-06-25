# LACC - Local Build and Test Script
# This script handles building, testing, and packaging both backend and UI components

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipTests,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipUI,
    
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

Write-Host "Starting CPS Complex Cases Local Build Process..." -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Skip Tests: $SkipTests" -ForegroundColor Yellow
Write-Host "Skip UI: $SkipUI" -ForegroundColor Yellow
Write-Host ""

# Resolve and validate paths
$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptRoot
$BackendPath = Join-Path $ProjectRoot "backend"
$UIPath = Join-Path $ProjectRoot "ui-spa"

if ([string]::IsNullOrEmpty($OutputPath)) {
    $OutputPath = Join-Path $ProjectRoot "build-output"
}

$SolutionFile = Join-Path $BackendPath "CPS.ComplexCases.sln"
$TestResultsPath = Join-Path $OutputPath "test-results"
$UITestResultsPath = Join-Path $OutputPath "ui-test-results"

# Validate paths
if (-not (Test-Path $BackendPath)) {
    Write-Host "ERROR: Backend path not found: $BackendPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $SolutionFile)) {
    Write-Host "ERROR: Solution file not found: $SolutionFile" -ForegroundColor Red
    exit 1
}

if (-not $SkipUI -and -not (Test-Path $UIPath)) {
    Write-Host "ERROR: UI path not found: $UIPath" -ForegroundColor Red
    exit 1
}

# Clean and create output directories
Write-Host "Cleaning previous build artifacts..." -ForegroundColor Cyan

# Clean build-output directory
if (Test-Path $OutputPath) {
    Remove-Item -Path $OutputPath -Recurse -Force
    Write-Host "Cleaned existing build output directory" -ForegroundColor Green
}

# Clean coverage-reports directory
$CoverageReportsPath = Join-Path $ProjectRoot "coverage-reports"
if (Test-Path $CoverageReportsPath) {
    Remove-Item -Path $CoverageReportsPath -Recurse -Force
    Write-Host "Cleaned existing coverage reports directory" -ForegroundColor Green
}

# Clean UI coverage directory (if UI not skipped)
if (-not $SkipUI) {
    $UICoveragePath = Join-Path $UIPath "coverage"
    if (Test-Path $UICoveragePath) {
        Remove-Item -Path $UICoveragePath -Recurse -Force
        Write-Host "Cleaned existing UI coverage directory" -ForegroundColor Green
    }
}

# Create fresh output directories
New-Item -Path $OutputPath -ItemType Directory -Force | Out-Null
Write-Host "Created fresh output directory: $OutputPath" -ForegroundColor Green

New-Item -Path $TestResultsPath -ItemType Directory -Force | Out-Null
Write-Host "Created fresh test results directory: $TestResultsPath" -ForegroundColor Green

if (-not $SkipUI) {
    New-Item -Path $UITestResultsPath -ItemType Directory -Force | Out-Null
    Write-Host "Created fresh UI test results directory: $UITestResultsPath" -ForegroundColor Green
}

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Cyan

# Check .NET SDK
try {
    $dotnetVersion = dotnet --version
    Write-Host "OK .NET SDK Version: $dotnetVersion" -ForegroundColor Green
}
catch {
    Write-Host "ERROR .NET SDK not found" -ForegroundColor Red
    Write-Host "Please install .NET 8 SDK from https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

# Check Node.js (if UI not skipped)
if (-not $SkipUI) {
    try {
        $nodeVersion = node --version
        $npmVersion = npm --version
        Write-Host "OK Node.js Version: $nodeVersion" -ForegroundColor Green
        Write-Host "OK npm Version: $npmVersion" -ForegroundColor Green
    }
    catch {
        Write-Host "ERROR Node.js or npm not found" -ForegroundColor Red
        Write-Host "Please install Node.js from https://nodejs.org/" -ForegroundColor Yellow
        exit 1
    }
}

# Function to run dotnet commands with error handling
function Invoke-DotNetCommand {
    param(
        [string]$Command,
        [string]$Description,
        [string]$WorkingDirectory = $BackendPath
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

# Function to run npm commands with error handling
function Invoke-NpmCommand {
    param(
        [string]$Command,
        [string]$Description,
        [string]$WorkingDirectory = $UIPath,
        [switch]$AllowFailure
    )
    
    Write-Host "$Description..." -ForegroundColor Cyan
    
    Push-Location $WorkingDirectory
    try {
        if ($VerboseOutput) {
            Write-Host "Executing: npm $Command" -ForegroundColor Gray
        }
        
        $npmArgs = $Command.Split(' ')
        & npm @npmArgs
        
        if ($LASTEXITCODE -ne 0) {
            if ($AllowFailure) {
                Write-Host "WARNING: $Description failed but continuing..." -ForegroundColor Yellow
            } else {
                throw "$Description failed with exit code $LASTEXITCODE"
            }
        } else {
            Write-Host "OK $Description completed successfully" -ForegroundColor Green
        }
    }
    finally {
        Pop-Location
    }
}

try {
    # BACKEND BUILD AND TEST
    Write-Host ""
    Write-Host "=== BACKEND BUILD AND TEST ===" -ForegroundColor Magenta
    
    # Step 1: Restore packages
    Invoke-DotNetCommand -Command "restore `"$SolutionFile`" --verbosity minimal" -Description "Restoring NuGet packages"
    
    # Step 2: Build solution
    Invoke-DotNetCommand -Command "build `"$SolutionFile`" --configuration $Configuration --no-restore --verbosity minimal" -Description "Building solution"
    
    # Step 3: Run tests (if not skipped)
    if (-not $SkipTests) {
        Write-Host "Running backend tests with coverage..." -ForegroundColor Cyan
        
        # Find test projects
        $testProjects = Get-ChildItem -Path $BackendPath -Name "*Tests.csproj" -Recurse
        
        if ($testProjects.Count -eq 0) {
            Write-Host "No backend test projects found" -ForegroundColor Yellow
        } else {
            Write-Host "Found $($testProjects.Count) backend test projects:" -ForegroundColor Yellow
            $testProjects | ForEach-Object { Write-Host "  - $_" -ForegroundColor Gray }
            
            # Run tests with coverage
            $testCommand = "test `"$SolutionFile`" --configuration $Configuration --no-build --logger `"trx;LogFileName=TestResults.trx`" --results-directory `"$TestResultsPath`" --collect:`"XPlat Code Coverage`" --settings `"$BackendPath/CodeCoverage.runsettings`" --verbosity minimal"
            
            Invoke-DotNetCommand -Command $testCommand -Description "Running backend tests with coverage"
            
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
                        Write-Host "`nBackend Coverage Summary:" -ForegroundColor Yellow
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
                Write-Host "Backend test results saved to:" -ForegroundColor Yellow
                $trxFiles | ForEach-Object { Write-Host "  - $TestResultsPath\$_" -ForegroundColor Gray }
            }
        }
    } else {
        Write-Host "Skipping backend tests as requested" -ForegroundColor Yellow
    }
    
    # Step 4: Publish Function Apps
    Write-Host "Publishing Function Apps..." -ForegroundColor Cyan
    
    $functionApps = @(
        @{ Name = "MainAPI"; Project = "CPS.ComplexCases.API/CPS.ComplexCases.API.csproj"; OutputDir = "MainAPI" },
        @{ Name = "FileTransferAPI"; Project = "CPS.ComplexCases.FileTransfer.API/CPS.ComplexCases.FileTransfer.API.csproj"; OutputDir = "FileTransferAPI" }
    )
    
    foreach ($app in $functionApps) {
        Write-Host "Publishing $($app.Name)..." -ForegroundColor Yellow
        
        $publishPath = Join-Path $OutputPath $app.OutputDir
        $projectPath = Join-Path $BackendPath $app.Project
        
        if (Test-Path $projectPath) {
            $publishCommand = "publish `"$projectPath`" --configuration $Configuration --output `"$publishPath`" --no-restore --verbosity minimal"
            Invoke-DotNetCommand -Command $publishCommand -Description "Publishing $($app.Name)"
            
            # Create zip file
            $zipPath = Join-Path $OutputPath "$($app.Name).zip"
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
    
    $migrationScriptPath = Join-Path $OutputPath "migration-script.sql"
    $dataProject = Join-Path $BackendPath "CPS.ComplexCases.Data/CPS.ComplexCases.Data.csproj"
    $startupProject = Join-Path $BackendPath "CPS.ComplexCases.API/CPS.ComplexCases.API.csproj"
    
    if ((Test-Path $dataProject) -and (Test-Path $startupProject)) {
        $migrationCommand = "ef migrations script --output `"$migrationScriptPath`" --project `"$dataProject`" --startup-project `"$startupProject`""
        if ($VerboseOutput) { $migrationCommand += " --verbose" }
        
        Invoke-DotNetCommand -Command $migrationCommand -Description "Generating migration script"
        
        # Copy migration files
        $migrationsSource = Join-Path $BackendPath "CPS.ComplexCases.Data/Migrations"
        if (Test-Path $migrationsSource) {
            $migrationsTarget = Join-Path $OutputPath "migrations"
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
    
    # UI BUILD AND TEST
    if (-not $SkipUI) {
        Write-Host ""
        Write-Host "=== UI BUILD AND TEST ===" -ForegroundColor Magenta
        
        # Step 1: Install UI packages
        Invoke-NpmCommand -Command "ci" -Description "Installing UI packages"
        
        # Step 2: Lint TypeScript code
        Invoke-NpmCommand -Command "run lint" -Description "Running UI linting" -AllowFailure
        
        # Step 3: Run UI tests (if not skipped)
        if (-not $SkipTests) {
            Write-Host "Running UI tests with coverage..." -ForegroundColor Cyan
            
            # Run unit tests with coverage
            Invoke-NpmCommand -Command "run coverage" -Description "Running UI unit tests with coverage" -AllowFailure
            
            # Copy coverage results and create enhanced report
            $coveragePath = Join-Path $UIPath "coverage"
            if (Test-Path $coveragePath) {
                $targetCoveragePath = Join-Path $UITestResultsPath "coverage"
                Copy-Item -Path $coveragePath -Destination $targetCoveragePath -Recurse -Force
                Write-Host "OK UI test coverage saved to: $targetCoveragePath" -ForegroundColor Green
                
                # Create enhanced UI coverage report
                Write-Host "Creating enhanced UI coverage report..." -ForegroundColor Yellow
                
                $coverageReportsDir = Join-Path $ProjectRoot "coverage-reports"
                $enhancedReportDir = Join-Path $coverageReportsDir "ui-html"
                
                if (-not (Test-Path $coverageReportsDir)) {
                    New-Item -Path $coverageReportsDir -ItemType Directory -Force | Out-Null
                }
                if (-not (Test-Path $enhancedReportDir)) {
                    New-Item -Path $enhancedReportDir -ItemType Directory -Force | Out-Null
                }
                
                # Copy existing coverage report
                $lcovReportPath = Join-Path $coveragePath "lcov-report"
                if (Test-Path $lcovReportPath) {
                    Copy-Item -Path "$lcovReportPath\*" -Destination $enhancedReportDir -Recurse -Force
                    
                    # Try to open the enhanced report
                    $uiIndexPath = Join-Path $enhancedReportDir "index.html"
                    if (Test-Path $uiIndexPath) {
                        try {
                            Start-Process $uiIndexPath
                            Write-Host "UI coverage report opened in your default browser" -ForegroundColor Green
                        } catch {
                            Write-Host "Could not auto-open browser. Please manually open: $uiIndexPath" -ForegroundColor Yellow
                        }
                    }
                }
            }
            
            # Run E2E tests
            Write-Host "Running E2E tests..." -ForegroundColor Cyan
            
            # Install Playwright browsers if needed
            Invoke-NpmCommand -Command "exec playwright install --with-deps" -Description "Installing Playwright browsers" -AllowFailure
            
            # Run E2E tests with Windows-compatible CI environment variable
            Write-Host "Preparing for E2E tests..." -ForegroundColor Cyan
            Push-Location $UIPath
            try {
                $env:CI = "true"
                try {
                    # First, build the app in playwright mode
                    Write-Host "Building app in Playwright mode..." -ForegroundColor Yellow
                    & npx vite build --mode playwright
                    if ($LASTEXITCODE -ne 0) { 
                        Write-Host "WARNING: Failed to build app in Playwright mode, skipping E2E tests" -ForegroundColor Yellow
                        return
                    }
                    
                    Write-Host "Running E2E tests with real-time output..." -ForegroundColor Yellow
                    
                    # Build command with proper arguments for better output
                    $command = "npx playwright test --reporter=list,html,junit --output=./playwright/test-results --workers=1"
                    
                    if ($VerboseOutput) {
                        $command += " --timeout=60000"
                    }
                    
                    Write-Host "Executing: $command" -ForegroundColor Gray
                    Write-Host "Note: E2E tests may take several minutes. Please wait..." -ForegroundColor Cyan
                    
                    # Use cmd /c to ensure proper output streaming
                    cmd /c $command
                    $exitCode = $LASTEXITCODE
                    
                    if ($exitCode -ne 0) {
                        Write-Host "WARNING: E2E tests failed with exit code $exitCode but continuing..." -ForegroundColor Yellow
                    } else {
                        Write-Host "OK E2E tests completed successfully" -ForegroundColor Green
                    }
                } finally {
                    Remove-Item env:CI -ErrorAction SilentlyContinue
                }
            } finally {
                Pop-Location
            }
            
            # Copy E2E test results
            $playwrightReportPath = Join-Path $UIPath "playwright-report"
            if (Test-Path $playwrightReportPath) {
                $targetE2EPath = Join-Path $UITestResultsPath "e2e-report"
                Copy-Item -Path $playwrightReportPath -Destination $targetE2EPath -Recurse -Force
                Write-Host "OK E2E test report saved to: $targetE2EPath" -ForegroundColor Green
            }
        } else {
            Write-Host "Skipping UI tests as requested" -ForegroundColor Yellow
        }
        
        # Step 4: Build UI
        Invoke-NpmCommand -Command "run build" -Description "Building UI application"
        
        # Step 5: Package UI for deployment
        Write-Host "Packaging UI for deployment..." -ForegroundColor Cyan
        
        $uiBuildPath = Join-Path $OutputPath "ui-build"
        $distPath = Join-Path $UIPath "dist"
        
        if (Test-Path $distPath) {
            Copy-Item -Path $distPath -Destination $uiBuildPath -Recurse -Force
            Write-Host "OK UI build artifacts copied to: $uiBuildPath" -ForegroundColor Green
            
            # Copy web.config for Azure Web App deployment
            $webConfigSource = Join-Path $UIPath "public/web.config"
            $webConfigTarget = Join-Path $uiBuildPath "web.config"
            if (Test-Path $webConfigSource) {
                Copy-Item -Path $webConfigSource -Destination $webConfigTarget -Force
                Write-Host "OK web.config copied for Azure Web App deployment" -ForegroundColor Green
            }
            
            # Create UI deployment package
            $uiZipPath = Join-Path $OutputPath "UI-SPA.zip"
            if (Test-Path $uiZipPath) { Remove-Item $uiZipPath -Force }
            
            Compress-Archive -Path "$uiBuildPath\*" -DestinationPath $uiZipPath -Force
            Write-Host "OK UI deployment package created: $uiZipPath" -ForegroundColor Green
            
            # Calculate and display package size
            $uiZipInfo = Get-Item $uiZipPath
            $uiSizeMB = [math]::Round($uiZipInfo.Length / 1MB, 2)
            Write-Host "UI package size: $uiSizeMB MB" -ForegroundColor Cyan
        } else {
            Write-Host "WARNING: UI dist folder not found: $distPath" -ForegroundColor Yellow
        }
    } else {
        Write-Host ""
        Write-Host "=== UI BUILD SKIPPED ===" -ForegroundColor Yellow
    }
    
    # SUMMARY
    Write-Host ""
    Write-Host "=== BUILD SUMMARY ===" -ForegroundColor Green
    Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
    Write-Host "Backend Path: $BackendPath" -ForegroundColor Yellow
    if (-not $SkipUI) {
        Write-Host "UI Path: $UIPath" -ForegroundColor Yellow
    }
    Write-Host "Output Directory: $OutputPath" -ForegroundColor Yellow
    
    if (Test-Path $OutputPath) {
        $items = Get-ChildItem -Path $OutputPath
        Write-Host "Generated Files:" -ForegroundColor Yellow
        $items | ForEach-Object {
            $size = if ($_.PSIsContainer) { "(folder)" } else { "($([math]::Round($_.Length / 1MB, 2)) MB)" }
            Write-Host "  - $($_.Name) $size" -ForegroundColor Gray
        }
    }
    
    if (-not $SkipTests) {
        if (Test-Path $TestResultsPath) {
            Write-Host "Backend Test Results: $TestResultsPath" -ForegroundColor Yellow
        }
        if (-not $SkipUI -and (Test-Path $UITestResultsPath)) {
            Write-Host "UI Test Results: $UITestResultsPath" -ForegroundColor Yellow
        }
    }
    
    # Coverage reports
    $coverageReportsPath = Join-Path $ProjectRoot "coverage-reports"
    if (Test-Path $coverageReportsPath) {
        Write-Host "Coverage Reports: $coverageReportsPath" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "Local build process completed successfully!" -ForegroundColor Green
    
} catch {
    Write-Host ""
    Write-Host "Local build failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($VerboseOutput) {
        Write-Host $_.ScriptStackTrace -ForegroundColor Red
    }
    exit 1
} 