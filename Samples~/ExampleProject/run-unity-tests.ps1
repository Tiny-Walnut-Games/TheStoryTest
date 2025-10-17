# Unity Test Runner PowerShell Script
# Executes Unity NUnit tests and captures output for analysis

param(
    [string]$TestType = "all",        # Options: "all", "editor", "playmode", "positioning"
    [switch]$LogOutput = $false,      # Capture and save logs
    [string]$UnityPath = "",          # Path to Unity executable (auto-detect if empty)
    [switch]$Interactive = $false,    # Open Unity Test Runner GUI
    [string]$ResultsPath = "test-results"  # Directory for test results
)

Write-Host "🎮 Unity Bubble Shooter Test Runner" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

# Project paths
$ProjectPath = $PSScriptRoot
$ResultsDir = Join-Path $ProjectPath $ResultsPath

# Create results directory
if (!(Test-Path $ResultsDir)) {
    New-Item -ItemType Directory -Path $ResultsDir -Force | Out-Null
    Write-Host "📁 Created results directory: $ResultsDir" -ForegroundColor Green
}

# Auto-detect Unity if path not provided
if ([string]::IsNullOrEmpty($UnityPath)) {
    $UnityVersionFile = Join-Path $ProjectPath "ProjectSettings\ProjectVersion.txt"
    if (Test-Path $UnityVersionFile) {
        $versionContent = Get-Content $UnityVersionFile | Where-Object { $_ -like "m_EditorVersion:*" }
        if ($versionContent) {
            $version = ($versionContent -split ": ")[1].Trim()
            $UnityPath = "C:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe"
            if (!(Test-Path $UnityPath)) {
                Write-Host "⚠️  Auto-detected Unity path not found: $UnityPath" -ForegroundColor Yellow
                $UnityPath = ""
            } else {
                Write-Host "🔍 Auto-detected Unity: $version" -ForegroundColor Green
            }
        }
    }
}

# Final Unity path check
if ([string]::IsNullOrEmpty($UnityPath) -or !(Test-Path $UnityPath)) {
    Write-Host "❌ Unity executable not found!" -ForegroundColor Red
    Write-Host "Please specify the Unity path with -UnityPath parameter" -ForegroundColor Yellow
    Write-Host "Example: -UnityPath 'C:\Program Files\Unity\Hub\Editor\2023.3.0f1\Editor\Unity.exe'" -ForegroundColor Yellow
    exit 1
}

Write-Host "🚀 Using Unity: $UnityPath" -ForegroundColor Green
Write-Host "📂 Project Path: $ProjectPath" -ForegroundColor Green

# Interactive mode - open Unity Test Runner
if ($Interactive) {
    Write-Host "`n🎯 Opening Unity Test Runner (Interactive Mode)" -ForegroundColor Cyan
    Write-Host "Instructions:" -ForegroundColor Yellow
    Write-Host "1. In Unity Editor, go to Window → General → Test Runner" -ForegroundColor Yellow
    Write-Host "2. Select EditMode or PlayMode tab" -ForegroundColor Yellow
    Write-Host "3. Click 'Run All' or select specific tests" -ForegroundColor Yellow
    Write-Host "4. Monitor console for positioning analysis logs" -ForegroundColor Yellow
    
    $args = @(
        "-projectPath", "`"$ProjectPath`""
    )
    
    Start-Process -FilePath $UnityPath -ArgumentList $args
    Write-Host "✅ Unity Editor launched" -ForegroundColor Green
    return
}

# Function to run Unity tests
function Run-UnityTest {
    param(
        [string]$Platform,
        [string]$Description
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
    $resultsFile = Join-Path $ResultsDir "test-results-$Platform-$timestamp.xml"
    $logFile = Join-Path $ResultsDir "test-log-$Platform-$timestamp.txt"
    
    Write-Host "`n🧪 Running $Description" -ForegroundColor Cyan
    Write-Host "Platform: $Platform" -ForegroundColor White
    Write-Host "Results: $resultsFile" -ForegroundColor White
    
    $args = @(
        "-batchmode",
        "-quit",
        "-projectPath", "`"$ProjectPath`"",
        "-runTests",
        "-testPlatform", $Platform,
        "-testResults", "`"$resultsFile`""
    )
    
    if ($LogOutput) {
        $args += @("-logFile", "`"$logFile`"")
        Write-Host "Log File: $logFile" -ForegroundColor White
    }
    
    Write-Host "Executing: $UnityPath $($args -join ' ')" -ForegroundColor Gray
    
    try {
        $process = Start-Process -FilePath $UnityPath -ArgumentList $args -Wait -PassThru -NoNewWindow
        
        if ($process.ExitCode -eq 0) {
            Write-Host "✅ $Description completed successfully" -ForegroundColor Green
        } else {
            Write-Host "❌ $Description failed (Exit Code: $($process.ExitCode))" -ForegroundColor Red
        }
        
        # Display results if available
        if (Test-Path $resultsFile) {
            try {
                [xml]$results = Get-Content $resultsFile
                $testRun = $results.SelectSingleNode("//test-run")
                if ($testRun) {
                    $total = $testRun.total
                    $passed = $testRun.passed
                    $failed = $testRun.failed
                    $skipped = $testRun.skipped
                    
                    Write-Host "📊 Test Results: $passed passed, $failed failed, $skipped skipped (Total: $total)" -ForegroundColor Cyan
                    
                    if ([int]$failed -gt 0) {
                        Write-Host "⚠️  Some tests failed - check results file for details" -ForegroundColor Yellow
                    }
                }
            } catch {
                Write-Host "⚠️  Could not parse results file" -ForegroundColor Yellow
            }
        }
        
        # Show log highlights if positioning tests were run
        if ($LogOutput -and (Test-Path $logFile) -and ($Platform -eq "PlayMode" -or $TestType -eq "positioning")) {
            Write-Host "`n📋 Key Log Highlights:" -ForegroundColor Cyan
            $logContent = Get-Content $logFile
            
            # Extract positioning-related logs
            $positioningLogs = $logContent | Where-Object { 
                $_ -match "COLLISION at position" -or 
                $_ -match "SNAP GEOMETRY" -or 
                $_ -match "POSITIONING.*ERROR" -or
                $_ -match "Test.*Position"
            }
            
            if ($positioningLogs) {
                $positioningLogs | Select-Object -First 10 | ForEach-Object {
                    Write-Host "  $_" -ForegroundColor White
                }
                
                if ($positioningLogs.Count -gt 10) {
                    Write-Host "  ... and $($positioningLogs.Count - 10) more positioning log entries" -ForegroundColor Gray
                }
            } else {
                Write-Host "  No positioning-specific logs found in this run" -ForegroundColor Gray
            }
        }
        
    } catch {
        Write-Host "❌ Error running tests: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Execute based on test type
switch ($TestType.ToLower()) {
    "editor" {
        Run-UnityTest -Platform "EditMode" -Description "Editor Tests (Fast Component Validation)"
    }
    "playmode" {
        Run-UnityTest -Platform "PlayMode" -Description "Play Mode Tests (Full Game Simulation)"
    }
    "positioning" {
        Write-Host "🎯 Running CRITICAL positioning bug tests" -ForegroundColor Magenta
        Write-Host "These tests specifically detect the 'several cells away' positioning issue" -ForegroundColor Yellow
        Run-UnityTest -Platform "PlayMode" -Description "Positioning Regression Tests"
    }
    "all" {
        Write-Host "🎯 Running complete Unity test suite" -ForegroundColor Magenta
        Run-UnityTest -Platform "EditMode" -Description "Editor Tests"
        Run-UnityTest -Platform "PlayMode" -Description "Play Mode Tests" 
    }
    default {
        Write-Host "❌ Invalid test type: $TestType" -ForegroundColor Red
        Write-Host "Valid options: all, editor, playmode, positioning" -ForegroundColor Yellow
        exit 1
    }
}

# Summary
Write-Host "`n📋 Test Execution Summary" -ForegroundColor Cyan
Write-Host "=========================" -ForegroundColor Cyan
Write-Host "Test Type: $TestType" -ForegroundColor White
Write-Host "Results Directory: $ResultsDir" -ForegroundColor White
Write-Host "Logs Captured: $($LogOutput)" -ForegroundColor White

if (Test-Path $ResultsDir) {
    $resultFiles = Get-ChildItem $ResultsDir -Name "test-results-*.xml" | Sort-Object -Descending | Select-Object -First 3
    if ($resultFiles) {
        Write-Host "Latest Result Files:" -ForegroundColor White
        $resultFiles | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
    }
}

Write-Host "`n🎉 Unity test execution complete!" -ForegroundColor Green
Write-Host "`n💡 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Review test results in the results directory" -ForegroundColor Yellow
Write-Host "2. For positioning bug analysis, check for 'COLLISION' and 'SNAP GEOMETRY' in logs" -ForegroundColor Yellow
Write-Host "3. Share positioning test results for debugging assistance" -ForegroundColor Yellow
Write-Host "4. Use -Interactive flag to manually run tests in Unity Editor for detailed analysis" -ForegroundColor Yellow