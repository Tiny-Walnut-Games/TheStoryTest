# Check if Unity Cache Server is Running

Write-Host "Checking Unity Cache Server status..." -ForegroundColor Cyan
Write-Host ""

# Check if port 8126 is listening
$listening = Get-NetTCPConnection -LocalPort 8126 -ErrorAction SilentlyContinue

if ($listening) {
    Write-Host "[OKAY] Cache Server is RUNNING" -ForegroundColor Green
    Write-Host "  Listening on: 127.0.0.1:8126" -ForegroundColor Gray

    # Try to show process info
    $process = Get-Process -Id $listening[0].OwningProcess -ErrorAction SilentlyContinue
    if ($process) {
        Write-Host "  Process: $($process.Name) (PID: $($process.Id))" -ForegroundColor Gray
    }

    # Check cache size
    $cachePath = Join-Path $PSScriptRoot "cache"
    if (Test-Path $cachePath) {
        $items = Get-ChildItem $cachePath -Recurse -ErrorAction SilentlyContinue
        if ($items) {
            $cacheSize = ($items | Measure-Object -Property Length -Sum).Sum
            if ($cacheSize) {
                $cacheSizeMB = [math]::Round($cacheSize / 1MB, 2)
                Write-Host "  Cache Size: $cacheSizeMB MB" -ForegroundColor Gray
            }
        }
    }
} else {
    Write-Host "[NOT RUNNING] Cache Server is not running" -ForegroundColor Red
    Write-Host ""
    Write-Host "To start the server, run:" -ForegroundColor Yellow
    Write-Host "  .\\.unity-cache-server\\start.ps1" -ForegroundColor White
}
