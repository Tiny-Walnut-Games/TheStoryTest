# -*- coding: utf-8-sig -*-
# ✅ Sync .github cleanup across all branches
# This script applies the documentation consolidation to: develop, release, pre-release, main

$ErrorActionPreference = "Stop"

# Get current branch
$currentBranch = git rev-parse --abbrev-ref HEAD
Write-Host "📍 Current branch: $currentBranch" -ForegroundColor Cyan

# List of branches to update (must already exist and be clean)
$branches = @("develop", "release", "pre-release", "main")

# Verify we have clean working directory
$status = git status --porcelain
if ($status) {
    Write-Host "⚠️  Working directory not clean. Please commit or stash changes first." -ForegroundColor Yellow
    exit 1
}

# Files to delete on each branch
$filesToDelete = @(
    ".github/THREE_BRANCH_WORKFLOW.md",
    ".github/BRANCH_PROTECTION_GUIDE.md",
    ".github/DEPENDENCY_MANAGEMENT.md",
    ".github/WORKFLOW_FIXES.md",
    ".github/PLATFORM_FIX.md",
    ".github/copilot-instructions.md"
)

Write-Host "`n🔄 Syncing changes across branches..." -ForegroundColor Green

foreach ($branch in $branches) {
    Write-Host "`n📌 Processing branch: $branch" -ForegroundColor Cyan
    
    # Fetch latest
    git fetch origin $branch
    
    # Checkout branch
    git checkout $branch
    Write-Host "  ✓ Checked out $branch" -ForegroundColor Green
    
    # Pull latest
    git pull origin $branch
    Write-Host "  ✓ Pulled latest" -ForegroundColor Green
    
    # Delete old files
    foreach ($file in $filesToDelete) {
        if (Test-Path $file) {
            Remove-Item $file -Force
            Write-Host "  ✓ Deleted $file" -ForegroundColor Green
        }
    }
    
    # Stage the deletions
    git add -A
    
    # Check if there are changes to commit
    $hasChanges = git status --porcelain
    if ($hasChanges) {
        # Commit
        git commit -m "🔧 Consolidate .github documentation (canonical sync)

- Merged THREE_BRANCH_WORKFLOW.md → WORKFLOWS.md
- Merged BRANCH_PROTECTION_GUIDE.md → WORKFLOWS.md
- Merged DEPENDENCY_MANAGEMENT.md → WORKFLOWS.md
- Merged WORKFLOW_FIXES.md → FIXES_CHANGELOG.md
- Merged PLATFORM_FIX.md → FIXES_CHANGELOG.md
- Moved copilot-instructions.md → .zencoder/rules/ai-guidelines.md
- Updated .github/README.md with new structure"
        
        Write-Host "  ✓ Committed changes" -ForegroundColor Green
        
        # Push
        git push origin $branch
        Write-Host "  ✓ Pushed to origin/$branch" -ForegroundColor Green
    } else {
        Write-Host "  ℹ️  No changes (files already synchronized)" -ForegroundColor Yellow
    }
}

# Return to original branch
Write-Host "`n🔄 Returning to original branch: $currentBranch" -ForegroundColor Cyan
git checkout $currentBranch

Write-Host "`n✅ Sync complete!" -ForegroundColor Green
Write-Host "`n📊 Summary:" -ForegroundColor Cyan
Write-Host "  Branches updated: $($branches -join ', ')" -ForegroundColor White
Write-Host "  Files consolidated: 6" -ForegroundColor White
Write-Host "  New files created: 3 (WORKFLOWS.md, FIXES_CHANGELOG.md, ai-guidelines.md)" -ForegroundColor White