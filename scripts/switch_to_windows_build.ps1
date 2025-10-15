#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Switches Unity build target to Windows Standalone and triggers recompilation.

.DESCRIPTION
    This script opens Unity in batch mode to switch the build target to Windows Standalone,
    which ensures assemblies are compiled for the correct platform on Windows OS.
#>

param(
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\*\Editor\Unity.exe"
)

$ErrorActionPreference = "Stop"

Write-Host "=== Unity Build Target Switcher ===" -ForegroundColor Cyan
Write-Host ""

# Find Unity executable
$unityExe = Get-ChildItem -Path $UnityPath -ErrorAction SilentlyContinue | 
    Sort-Object -Property LastWriteTime -Descending | 
    Select-Object -First 1 -ExpandProperty FullName

if (-not $unityExe) {
    Write-Host "❌ Unity executable not found at: $UnityPath" -ForegroundColor Red
    Write-Host "Please specify the Unity path manually:" -ForegroundColor Yellow
    Write-Host "  .\scripts\switch_to_windows_build.ps1 -UnityPath 'C:\Path\To\Unity.exe'" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Found Unity: $unityExe" -ForegroundColor Green

$projectPath = "E:\Tiny_Walnut_Games\TheStoryTest\Samples~\ExampleProject"
$logFile = "E:\Tiny_Walnut_Games\TheStoryTest\unity_build_target_switch.log"

Write-Host "📁 Project: $projectPath" -ForegroundColor Cyan
Write-Host "📝 Log file: $logFile" -ForegroundColor Cyan
Write-Host ""

Write-Host "🔄 Switching to Windows Standalone build target..." -ForegroundColor Yellow

# Run Unity in batch mode to switch build target
& $unityExe `
    -batchmode `
    -quit `
    -projectPath $projectPath `
    -buildTarget Win64 `
    -logFile $logFile

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build target switched to Windows Standalone successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Run validation: python scripts/story_test.py Samples~/ExampleProject --verbose --output story-test-report.json" -ForegroundColor White
    Write-Host "  2. If validation passes, bump version: .\scripts\release.ps1 bump patch" -ForegroundColor White
} else {
    Write-Host "❌ Failed to switch build target. Check log file: $logFile" -ForegroundColor Red
    exit 1
}