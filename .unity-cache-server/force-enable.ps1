# Force Enable Cache Server via Registry (Windows)
# This sets Unity's EditorPrefs directly

Write-Host "Configuring Unity Cache Server settings via Registry..." -ForegroundColor Cyan

# Unity 6 stores preferences in registry at:
# HKCU\Software\Unity Technologies\Unity Editor 6.x

$unityKey = "HKCU:\Software\Unity Technologies\Unity Editor 6.x"

# Check if key exists
if (Test-Path $unityKey) {
    # Enable cache server
    Set-ItemProperty -Path $unityKey -Name "CacheServerMode" -Value 2 -Type DWord
    Set-ItemProperty -Path $unityKey -Name "CacheServerIPAddress" -Value "127.0.0.1" -Type String
    Set-ItemProperty -Path $unityKey -Name "CacheServerPort" -Value 8126 -Type DWord
    Set-ItemProperty -Path $unityKey -Name "CacheServerEnableDownload" -Value 1 -Type DWord
    Set-ItemProperty -Path $unityKey -Name "CacheServerEnableUpload" -Value 1 -Type DWord

    Write-Host "[OK] Cache Server settings configured" -ForegroundColor Green
    Write-Host "  Mode: Remote (2)" -ForegroundColor Gray
    Write-Host "  IP: 127.0.0.1" -ForegroundColor Gray
    Write-Host "  Port: 8126" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Restart Unity for changes to take effect." -ForegroundColor Yellow
} else {
    Write-Host "[WARNING] Unity 6.x registry key not found" -ForegroundColor Yellow
    Write-Host "Looking for alternative keys..." -ForegroundColor Cyan

    # List all Unity Editor keys
    $unityBase = "HKCU:\Software\Unity Technologies"
    Get-ChildItem $unityBase -ErrorAction SilentlyContinue | ForEach-Object {
        Write-Host "  Found: $($_.PSChildName)" -ForegroundColor Gray
    }

    Write-Host ""
    Write-Host "Please configure manually in Unity:" -ForegroundColor Yellow
    Write-Host "  Edit -> Preferences -> Cache Server" -ForegroundColor White
}
