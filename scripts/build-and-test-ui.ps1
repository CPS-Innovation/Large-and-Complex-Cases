# LACC - Local UI Build and Test Script
# This script handles building, testing, and packaging the UI component

param(
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipTests,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipE2E,
    
    [Parameter(Mandatory=$false)]
    [switch]$VerboseOutput
)

Write-Host "Starting CPS Complex Cases UI Build Process..." -ForegroundColor Green

# Set default paths
$RootPath = Split-Path -Parent $PSScriptRoot
$UIPath = Join-Path $RootPath "ui-spa"

if ([string]::IsNullOrEmpty($OutputPath)) {
    $OutputPath = Join-Path $RootPath "build-output"
}

$UITestResultsPath = Join-Path $OutputPath "ui-test-results"

# Clean and create output directories
Write-Host "Cleaning previous UI build artifacts..." -ForegroundColor Cyan

# Clean build-output directory
if (Test-Path $OutputPath) {
    Remove-Item -Path $OutputPath -Recurse -Force
    Write-Host "Cleaned existing UI build output directory" -ForegroundColor Green
}

# Clean UI coverage directory in the ui-spa folder
$UICoveragePath = Join-Path $UIPath "coverage"
if (Test-Path $UICoveragePath) {
    Remove-Item -Path $UICoveragePath -Recurse -Force
    Write-Host "Cleaned existing UI coverage directory" -ForegroundColor Green
}

# Create fresh output directories
New-Item -Path $OutputPath -ItemType Directory -Force | Out-Null
Write-Host "Created fresh UI output directory: $OutputPath" -ForegroundColor Green

New-Item -Path $UITestResultsPath -ItemType Directory -Force | Out-Null
Write-Host "Created fresh UI test results directory: $UITestResultsPath" -ForegroundColor Green

function Test-Node {
    try {
        $nodeVersion = node --version
        $npmVersion = npm --version
        Write-Host "OK Node.js Version: $nodeVersion" -ForegroundColor Green
        Write-Host "OK npm Version: $npmVersion" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "ERROR Node.js or npm not found" -ForegroundColor Red
        Write-Host "Please install Node.js from https://nodejs.org/" -ForegroundColor Yellow
        return $false
    }
}

function Install-UIPackages {
    Write-Host "Installing UI packages..." -ForegroundColor Cyan
    
    Push-Location $UIPath
    try {
        if ($VerboseOutput) {
            & npm ci
        } else {
            & npm ci --silent
        }
        if ($LASTEXITCODE -ne 0) { throw "UI package installation failed" }
        Write-Host "OK UI packages installed" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

function Build-UI {
    Write-Host "Building UI..." -ForegroundColor Cyan
    
    Push-Location $UIPath
    try {
        # Run TypeScript compilation and linting
        Write-Host "Running linting..." -ForegroundColor Yellow
        npm run lint
        if ($LASTEXITCODE -ne 0) { throw "UI linting failed" }
        
        # Build the application
        Write-Host "Building application..." -ForegroundColor Yellow
        npm run build
        if ($LASTEXITCODE -ne 0) { throw "UI build failed" }
        
        Write-Host "OK UI build completed" -ForegroundColor Green
        
        # Copy build artifacts
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
            Compress-Archive -Path "$uiBuildPath\*" -DestinationPath $uiZipPath -Force
            Write-Host "OK UI deployment package created: $uiZipPath" -ForegroundColor Green
        }
    }
    finally {
        Pop-Location
    }
}

function Run-UITests {
    if ($SkipTests) {
        Write-Host "Skipping UI tests" -ForegroundColor Yellow
        return
    }
    
    Write-Host "Running UI tests..." -ForegroundColor Cyan
    
    Push-Location $UIPath
    try {
        # Run unit tests with coverage
        Write-Host "Running unit tests..." -ForegroundColor Yellow
        if ($VerboseOutput) {
            npm run coverage
        } else {
            npm run coverage --silent
        }
        if ($LASTEXITCODE -ne 0) { throw "UI unit tests failed" }
        
        # Generate beautiful HTML coverage report
        Write-Host "Generating beautiful HTML coverage report..." -ForegroundColor Cyan
        
        $coveragePath = Join-Path $UIPath "coverage"
        if (Test-Path $coveragePath) {
            # Copy original coverage results
            $targetCoveragePath = Join-Path $UITestResultsPath "coverage"
            Copy-Item -Path $coveragePath -Destination $targetCoveragePath -Recurse -Force
            Write-Host "UI test coverage saved to: $targetCoveragePath" -ForegroundColor Green
            
            # Create enhanced HTML report directory
            $enhancedReportDir = Join-Path $targetCoveragePath "html-enhanced"
            New-Item -Path $enhancedReportDir -ItemType Directory -Force | Out-Null
            
            # Copy existing HTML report if available
            $lcovReportPath = Join-Path $coveragePath "lcov-report"
            if (Test-Path $lcovReportPath) {
                Copy-Item -Path "$lcovReportPath\*" -Destination $enhancedReportDir -Recurse -Force
                Write-Host "Existing HTML report copied" -ForegroundColor Green
            }
            
            # Generate coverage badges and enhanced report
            $coverageSummaryPath = Join-Path $coveragePath "coverage-summary.json"
            if (Test-Path $coverageSummaryPath) {
                Write-Host "Generating coverage badges and enhanced report..." -ForegroundColor Yellow
                
                # Create badges directory
                $badgesDir = Join-Path $enhancedReportDir "badges"
                New-Item -Path $badgesDir -ItemType Directory -Force | Out-Null
                
                # Generate badges using Node.js inline script
                $badgeScript = @"
const fs = require('fs');
const path = require('path');

try {
  const summary = JSON.parse(fs.readFileSync('coverage/coverage-summary.json', 'utf8'));
  const total = summary.total;
  
  // Create a simple badge SVG
  const createBadge = (label, value) => {
    const percentage = Math.round(value);
    const color = percentage >= 80 ? '#4c1' : percentage >= 60 ? '#dfb317' : '#e05d44';
    return `<svg xmlns='http://www.w3.org/2000/svg' width='104' height='20'>
      <linearGradient id='b' x2='0' y2='100%'>
        <stop offset='0' stop-color='#bbb' stop-opacity='.1'/>
        <stop offset='1' stop-opacity='.1'/>
      </linearGradient>
      <mask id='a'>
        <rect width='104' height='20' rx='3' fill='#fff'/>
      </mask>
      <g mask='url(#a)'>
        <path fill='#555' d='M0 0h63v20H0z'/>
        <path fill='`+color+`' d='M63 0h41v20H63z'/>
        <path fill='url(#b)' d='M0 0h104v20H0z'/>
      </g>
      <g fill='#fff' text-anchor='middle' font-family='DejaVu Sans,Verdana,Geneva,sans-serif' font-size='110'>
        <text x='325' y='150' fill='#010101' fill-opacity='.3' transform='scale(.1)' textLength='530'>`+label+`</text>
        <text x='325' y='140' transform='scale(.1)' textLength='530'>`+label+`</text>
        <text x='825' y='150' fill='#010101' fill-opacity='.3' transform='scale(.1)' textLength='310'>`+percentage+`%</text>
        <text x='825' y='140' transform='scale(.1)' textLength='310'>`+percentage+`%</text>
      </g>
    </svg>`;
  };
  
  // Generate badges
  fs.writeFileSync('$($badgesDir.Replace('\', '\\'))/lines.svg', createBadge('lines', total.lines.pct));
  fs.writeFileSync('$($badgesDir.Replace('\', '\\'))/functions.svg', createBadge('functions', total.functions.pct));
  fs.writeFileSync('$($badgesDir.Replace('\', '\\'))/branches.svg', createBadge('branches', total.branches.pct));
  fs.writeFileSync('$($badgesDir.Replace('\', '\\'))/statements.svg', createBadge('statements', total.statements.pct));
  
  console.log('Coverage badges generated successfully');
  
  // Display coverage summary
  console.log('\\n📈 Coverage Summary:');
  console.log('  Lines: ' + Math.round(total.lines.pct) + '%');
  console.log('  Functions: ' + Math.round(total.functions.pct) + '%');
  console.log('  Branches: ' + Math.round(total.branches.pct) + '%');
  console.log('  Statements: ' + Math.round(total.statements.pct) + '%');
} catch (error) {
  console.log('Error generating badges:', error.message);
}
"@
                
                # Write the script to a temporary file and execute it
                $tempScriptPath = Join-Path $UIPath "temp-badge-script.js"
                $badgeScript | Out-File -FilePath $tempScriptPath -Encoding UTF8
                
                try {
                    node $tempScriptPath
                    Remove-Item $tempScriptPath -Force
                } catch {
                    Write-Host "Could not generate badges: $($_.Exception.Message)" -ForegroundColor Yellow
                    if (Test-Path $tempScriptPath) { Remove-Item $tempScriptPath -Force }
                }
                
                # Create enhanced index.html with project branding
                $currentDate = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
                $enhancedHtml = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>CPS Complex Cases UI - Coverage Report</title>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; margin: 0; padding: 20px; background: #f5f5f5; }
        .header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; border-radius: 12px; margin-bottom: 20px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }
        .header h1 { margin: 0; font-size: 28px; font-weight: 600; }
        .header p { margin: 8px 0 0 0; opacity: 0.9; font-size: 16px; }
        .badges { display: flex; gap: 12px; margin: 20px 0; flex-wrap: wrap; }
        .badge { background: white; padding: 8px 12px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
        .badge img { height: 20px; display: block; }
        .report-frame { background: white; border-radius: 12px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); overflow: hidden; }
        .report-frame iframe { width: 100%; height: 80vh; border: none; }
        .footer { text-align: center; margin-top: 30px; color: #666; font-size: 14px; }
        .footer a { color: #667eea; text-decoration: none; }
        .stats { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 15px; margin: 20px 0; }
        .stat-card { background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); text-align: center; }
        .stat-value { font-size: 24px; font-weight: bold; color: #667eea; }
        .stat-label { color: #666; margin-top: 5px; }
    </style>
</head>
<body>
    <div class="header">
        <h1>🎯 CPS Complex Cases UI - Coverage Report</h1>
        <p>Generated on $currentDate | React SPA Test Coverage</p>
    </div>
    
    <div class="badges">
        <div class="badge"><img src="badges/lines.svg" alt="Lines Coverage" title="Lines Coverage" /></div>
        <div class="badge"><img src="badges/functions.svg" alt="Functions Coverage" title="Functions Coverage" /></div>
        <div class="badge"><img src="badges/branches.svg" alt="Branches Coverage" title="Branches Coverage" /></div>
        <div class="badge"><img src="badges/statements.svg" alt="Statements Coverage" title="Statements Coverage" /></div>
    </div>
    
    <div class="report-frame">
        <iframe src="index.html" title="Detailed Coverage Report"></iframe>
    </div>
    
    <div class="footer">
        <p>Generated by <a href="https://vitest.dev/" target="_blank">Vitest Coverage</a> | Enhanced for CPS Complex Cases Project</p>
        <p>Interactive coverage report with drill-down capabilities</p>
    </div>
</body>
</html>
"@
                
                # Write enhanced HTML if original exists
                $originalIndexPath = Join-Path $enhancedReportDir "index.html"
                if (Test-Path $originalIndexPath) {
                    $enhancedIndexPath = Join-Path $enhancedReportDir "enhanced-report.html"
                    $enhancedHtml | Out-File -FilePath $enhancedIndexPath -Encoding UTF8
                    Write-Host "Enhanced HTML coverage report generated" -ForegroundColor Green
                    
                    # Try to open the enhanced report in default browser
                    try {
                        Start-Process $enhancedIndexPath
                        Write-Host "Enhanced coverage report opened in your default browser" -ForegroundColor Green
                    } catch {
                        Write-Host "Could not auto-open browser. Please manually open: $enhancedIndexPath" -ForegroundColor Yellow
                    }
                } else {
                    Write-Host "Original coverage report not found, enhanced report not created" -ForegroundColor Yellow
                }
            } else {
                Write-Host "Coverage summary not found, badges not generated" -ForegroundColor Yellow
            }
            
            Write-Host "Coverage report location: $enhancedReportDir" -ForegroundColor Cyan
        } else {
            Write-Host "No coverage files found" -ForegroundColor Yellow
        }
        
        Write-Host "OK UI unit tests completed" -ForegroundColor Green
        
        # Run E2E tests (Playwright) if not skipped
        if (-not $SkipE2E) {
            Write-Host "Running E2E tests..." -ForegroundColor Yellow
            
            # Install Playwright browsers if needed
            Write-Host "Installing Playwright browsers..." -ForegroundColor Gray
            npx playwright install --with-deps
            if ($LASTEXITCODE -ne 0) { 
                Write-Host "WARNING Failed to install Playwright browsers" -ForegroundColor Yellow
            }
            
            # Run E2E tests in CI mode with Windows-compatible environment variable
            $env:CI = "true"
            try {
                Write-Host "Preparing for E2E tests..." -ForegroundColor Yellow
                
                # First, build the app in playwright mode
                Write-Host "Building app in Playwright mode..." -ForegroundColor Cyan
                & npx vite build --mode playwright
                if ($LASTEXITCODE -ne 0) { 
                    Write-Host "WARNING: Failed to build app in Playwright mode" -ForegroundColor Yellow
                    return
                }
                
                Write-Host "Starting Playwright E2E tests with real-time output..." -ForegroundColor Yellow
                
                # Build command with proper arguments for better output
                $command = "npx playwright test --reporter=list,html,junit --output=./playwright/test-results --workers=1"
                
                if ($VerboseOutput) {
                    $command += " --timeout=60000"
                }
                
                Write-Host "Executing: $command" -ForegroundColor Gray
                Write-Host "Note: E2E tests may take several minutes. Please wait for completion..." -ForegroundColor Cyan
                
                # Use cmd /c to ensure proper output streaming
                cmd /c $command
                $exitCode = $LASTEXITCODE
                
                if ($exitCode -ne 0) { 
                    Write-Host "WARNING E2E tests failed with exit code $exitCode" -ForegroundColor Yellow
                } else {
                    Write-Host "OK E2E tests completed successfully" -ForegroundColor Green
                }
            } finally {
                Remove-Item env:CI -ErrorAction SilentlyContinue
            }
            
            # Copy E2E test results
            $playwrightReportPath = Join-Path $UIPath "playwright/playwright-report"
            if (Test-Path $playwrightReportPath) {
                $targetE2EPath = Join-Path $UITestResultsPath "e2e-report"
                Copy-Item -Path $playwrightReportPath -Destination $targetE2EPath -Recurse -Force
                Write-Host "OK E2E test report saved to: $targetE2EPath" -ForegroundColor Green
            }
        } else {
            Write-Host "Skipping E2E tests as requested" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "UI tests encountered issues: $($_.Exception.Message)" -ForegroundColor Yellow
        Write-Host "Continuing with build process..." -ForegroundColor Yellow
    }
    finally {
        Pop-Location
    }
}

function Show-Summary {
    Write-Host ""
    Write-Host "UI Build Summary" -ForegroundColor Green
    Write-Host "================" -ForegroundColor Green
    
    Write-Host "UI Path: $UIPath" -ForegroundColor Yellow
    Write-Host "Output Directory: $OutputPath" -ForegroundColor Yellow
    
    if (Test-Path $OutputPath) {
        $items = Get-ChildItem -Path $OutputPath
        Write-Host "Generated Files:" -ForegroundColor Yellow
        $items | ForEach-Object {
            $size = if ($_.PSIsContainer) { "(folder)" } else { "($([math]::Round($_.Length / 1MB, 2)) MB)" }
            Write-Host "  - $($_.Name) $size" -ForegroundColor Gray
        }
    }
    
    if (-not $SkipTests -and (Test-Path $UITestResultsPath)) {
        Write-Host "UI Test Results: $UITestResultsPath" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "UI build process completed successfully!" -ForegroundColor Green
}

# Main execution
try {
    Write-Host "UI Path: $UIPath" -ForegroundColor Yellow
    Write-Host "Output Path: $OutputPath" -ForegroundColor Yellow
    Write-Host ""
    
    # Check prerequisites
    if (-not (Test-Node)) {
        Write-Host "Prerequisites not met. Exiting." -ForegroundColor Red
        exit 1
    }
    
    # UI build and test
    Install-UIPackages
    Build-UI
    Run-UITests
    
    Show-Summary
} catch {
    Write-Host ""
    Write-Host "UI build failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor Red
    exit 1
} 