# Unity Cache Server Startup Script
# Run this script to start the Unity Cache Server locally

Write-Host "Starting Unity Cache Server..." -ForegroundColor Green
Write-Host "Cache Directory: $(Get-Location)\cache" -ForegroundColor Cyan
Write-Host "Server will listen on: http://127.0.0.1:8126" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
Write-Host ""

# Get absolute paths
$cacheDir = Join-Path $PSScriptRoot "cache"

# Create cache directory if it doesn't exist
if (-not (Test-Path $cacheDir)) {
    Write-Host "Creating cache directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $cacheDir | Out-Null
}

Write-Host "Cache directory: $cacheDir" -ForegroundColor Cyan

# Start the cache server with absolute path
npx unity-cache-server -P $cacheDir -h "127.0.0.1" -p 8126 -w 2 -l 3
