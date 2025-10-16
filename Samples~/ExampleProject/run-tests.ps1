# PowerShell script to run E2E tests for Bubble Shooter

param(
    [string]$TestType = "all",
    [switch]$UI,
    [switch]$Debug,
    [string]$Specific = ""
)

Write-Host "🎮 Bubble Shooter E2E Test Runner" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

# Check if WebGL build exists
if (!(Test-Path "WebGL-Build")) {
    Write-Host "❌ WebGL-Build folder not found!" -ForegroundColor Red
    Write-Host "Please create a Unity WebGL build first. See build-webgl.md for instructions." -ForegroundColor Yellow
    exit 1
}

# Check if index.html exists in build
if (!(Test-Path "WebGL-Build\index.html")) {
    Write-Host "❌ WebGL build appears incomplete - no index.html found!" -ForegroundColor Red
    Write-Host "Please rebuild the Unity project for WebGL. See build-webgl.md for instructions." -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ WebGL build found" -ForegroundColor Green

# Check if npm dependencies are installed
if (!(Test-Path "node_modules")) {
    Write-Host "📦 Installing npm dependencies..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to install dependencies!" -ForegroundColor Red
        exit 1
    }
}

Write-Host "✅ Dependencies ready" -ForegroundColor Green

# Determine test command based on parameters
$testCommand = "npx playwright test"

if ($Specific) {
    $testCommand += " $Specific"
    Write-Host "🎯 Running specific test: $Specific" -ForegroundColor Magenta
}
elseif ($TestType -eq "positioning") {
    $testCommand += " bubble-positioning-regression.spec.ts"
    Write-Host "🎯 Running positioning regression tests" -ForegroundColor Magenta
}
elseif ($TestType -eq "shooting") {
    $testCommand += " bubble-shooting-mechanics.spec.ts"
    Write-Host "🎯 Running shooting mechanics tests" -ForegroundColor Magenta
}
elseif ($TestType -eq "startup") {
    $testCommand += " bubble-shooter-game-startup.spec.ts"
    Write-Host "🎯 Running game startup tests" -ForegroundColor Magenta
}
elseif ($TestType -eq "scoring") {
    $testCommand += " bubble-matching-scoring.spec.ts"
    Write-Host "🎯 Running scoring tests" -ForegroundColor Magenta
}
elseif ($TestType -eq "gameover") {
    $testCommand += " game-over-restart.spec.ts"
    Write-Host "🎯 Running game over tests" -ForegroundColor Magenta
}
else {
    Write-Host "🎯 Running all E2E tests" -ForegroundColor Magenta
}

if ($UI) {
    $testCommand += " --ui"
    Write-Host "🖥️  Opening in UI mode" -ForegroundColor Blue
}

if ($Debug) {
    $testCommand += " --debug"
    Write-Host "🐛 Running in debug mode" -ForegroundColor Blue
}

Write-Host ""
Write-Host "📋 Test Categories Available:" -ForegroundColor White
Write-Host "  - startup    : Game initialization and setup" -ForegroundColor Gray
Write-Host "  - shooting   : Bubble shooting mechanics" -ForegroundColor Gray
Write-Host "  - positioning: Critical positioning regression tests" -ForegroundColor Red
Write-Host "  - scoring    : Matching and scoring system" -ForegroundColor Gray
Write-Host "  - gameover   : End game and restart logic" -ForegroundColor Gray
Write-Host "  - all        : Run complete test suite" -ForegroundColor Gray
Write-Host ""

Write-Host "🚀 Executing: $testCommand" -ForegroundColor Green
Write-Host ""

# Execute the test command
Invoke-Expression $testCommand

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✅ Tests completed successfully!" -ForegroundColor Green
    Write-Host "📊 Check the HTML report with: npm run test:report" -ForegroundColor Cyan
} else {
    Write-Host ""
    Write-Host "❌ Some tests failed. Check the output above for details." -ForegroundColor Red
    Write-Host "💡 Try running with --ui flag for interactive debugging" -ForegroundColor Yellow
}