# Build and publish Story Test Framework to PyPI
# Usage:
#   .\scripts\build_pypi.ps1          # Build only
#   .\scripts\build_pypi.ps1 -Test    # Upload to TestPyPI
#   .\scripts\build_pypi.ps1 -Publish # Upload to PyPI

param(
    [switch]$Test,
    [switch]$Publish,
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

Write-Host "🎯 Story Test Framework - PyPI Build Script" -ForegroundColor Cyan
Write-Host "=" * 60

# Clean previous builds
if ($Clean -or $Test -or $Publish) {
    Write-Host "`n🧹 Cleaning previous builds..." -ForegroundColor Yellow
    if (Test-Path "dist") { Remove-Item -Recurse -Force "dist" }
    if (Test-Path "build") { Remove-Item -Recurse -Force "build" }
    if (Test-Path "storytest.egg-info") { Remove-Item -Recurse -Force "storytest.egg-info" }
    Write-Host "✓ Cleaned" -ForegroundColor Green
}

# Install/upgrade build tools
Write-Host "`n📦 Installing build tools..." -ForegroundColor Yellow
python -m pip install --upgrade pip
python -m pip install --upgrade build twine
Write-Host "✓ Build tools ready" -ForegroundColor Green

# Build the package
Write-Host "`n🔨 Building package..." -ForegroundColor Yellow
python -m build
Write-Host "✓ Package built successfully" -ForegroundColor Green

# List built files
Write-Host "`n📋 Built files:" -ForegroundColor Cyan
Get-ChildItem dist | ForEach-Object { Write-Host "  • $($_.Name)" }

# Check package
Write-Host "`n🔍 Checking package..." -ForegroundColor Yellow
python -m twine check dist/*
Write-Host "✓ Package check passed" -ForegroundColor Green

if ($Test) {
    Write-Host "`n🧪 Uploading to TestPyPI..." -ForegroundColor Yellow
    Write-Host "You will need your TestPyPI API token" -ForegroundColor Gray
    python -m twine upload --repository testpypi dist/*
    Write-Host "`n✓ Uploaded to TestPyPI" -ForegroundColor Green
    Write-Host "`nTest installation with:" -ForegroundColor Cyan
    Write-Host "  pip install --index-url https://test.pypi.org/simple/ storytest" -ForegroundColor White
}
elseif ($Publish) {
    Write-Host "`n🚀 Uploading to PyPI..." -ForegroundColor Yellow
    Write-Host "⚠️  This will publish to the REAL PyPI!" -ForegroundColor Red
    $confirm = Read-Host "Are you sure? (yes/no)"
    
    if ($confirm -eq "yes") {
        Write-Host "You will need your PyPI API token" -ForegroundColor Gray
        python -m twine upload dist/*
        Write-Host "`n✅ Published to PyPI!" -ForegroundColor Green
        Write-Host "`nInstall with:" -ForegroundColor Cyan
        Write-Host "  pip install storytest" -ForegroundColor White
    }
    else {
        Write-Host "`n❌ Publish cancelled" -ForegroundColor Red
    }
}
else {
    Write-Host "`n✅ Build complete!" -ForegroundColor Green
    Write-Host "`nNext steps:" -ForegroundColor Cyan
    Write-Host "  • Test locally: pip install -e ." -ForegroundColor White
    Write-Host "  • Upload to TestPyPI: .\scripts\build_pypi.ps1 -Test" -ForegroundColor White
    Write-Host "  • Publish to PyPI: .\scripts\build_pypi.ps1 -Publish" -ForegroundColor White
}

Write-Host "`n" + ("=" * 60)