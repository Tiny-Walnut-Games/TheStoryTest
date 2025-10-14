# Story Test Release Helper (PowerShell)
# Automates version bumping and tagging for releases

param(
    [Parameter(Position=0)]
    [string]$Command,
    
    [Parameter(Position=1)]
    [string]$Argument
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$PackageJson = Join-Path $ProjectRoot "Packages\com.tinywalnutgames.storytest\package.json"

# Helper functions
function Write-Info {
    param([string]$Message)
    Write-Host "INFO: $Message" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "SUCCESS: $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "WARNING: $Message" -ForegroundColor Yellow
}

function Write-Error-Exit {
    param([string]$Message)
    Write-Host "ERROR: $Message" -ForegroundColor Red
    exit 1
}

# Get current version from package.json
function Get-CurrentVersion {
    if (-not (Test-Path $PackageJson)) {
        Write-Error-Exit "package.json not found at $PackageJson"
    }
    
    $json = Get-Content $PackageJson -Raw | ConvertFrom-Json
    return $json.version
}

# Bump version number
function Get-BumpedVersion {
    param(
        [string]$CurrentVersion,
        [string]$BumpType
    )
    
    $parts = $CurrentVersion -split '\.'
    $major = [int]$parts[0]
    $minor = [int]$parts[1]
    $patchNum = [int]$parts[2]
    
    switch ($BumpType) {
        "major" {
            $major++
            $minor = 0
            $patchNum = 0
        }
        "minor" {
            $minor++
            $patchNum = 0
        }
        "patch" {
            $patchNum++
        }
        default {
            Write-Error-Exit "Invalid bump type: $BumpType (use: major, minor, or patch)"
        }
    }
    
    return "{0}.{1}.{2}" -f $major, $minor, $patchNum
}

# Update version in package.json
function Update-PackageVersion {
    param([string]$NewVersion)
    
    Write-Info "Updating package.json to version $NewVersion..."
    
    $json = Get-Content $PackageJson -Raw | ConvertFrom-Json
    $json.version = $NewVersion
    $json | ConvertTo-Json -Depth 10 | Set-Content $PackageJson
    
    Write-Success "package.json updated"
}

# Create git tag
function New-GitTag {
    param([string]$Version)
    
    $tag = "v$Version"
    
    Write-Info "Creating git tag $tag..."
    
    # Check if tag already exists
    $existingTag = git tag -l $tag 2>$null
    if ($existingTag) {
        Write-Error-Exit "Tag $tag already exists"
    }
    
    # Commit version change
    git add $PackageJson
    try {
        git commit -m "Bump version to $Version"
    } catch {
        Write-Warning "No changes to commit"
    }
    
    # Create annotated tag
    git tag -a $tag -m "Release version $Version"
    
    Write-Success "Tag $tag created"
}

# Show usage
function Show-Usage {
    Write-Host "Story Test Release Helper"
    Write-Host ""
    Write-Host "Usage: .\release.ps1 <command> [options]"
    Write-Host ""
    Write-Host "Commands:"
    Write-Host "    current             Show current version"
    Write-Host "    bump <type>         Bump version (major|minor|patch) and create tag"
    Write-Host "    tag <version>       Create tag for specific version"
    Write-Host "    push                Push commits and tags to remote"
    Write-Host "    dry-run <type>      Show what would happen without making changes"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "    .\release.ps1 current                  # Show current version"
    Write-Host "    .\release.ps1 bump patch               # Bump patch version (1.0.0 -> 1.0.1)"
    Write-Host "    .\release.ps1 bump minor               # Bump minor version (1.0.0 -> 1.1.0)"
    Write-Host "    .\release.ps1 bump major               # Bump major version (1.0.0 -> 2.0.0)"
    Write-Host "    .\release.ps1 tag 1.2.3                # Create tag for version 1.2.3"
    Write-Host "    .\release.ps1 push                     # Push to remote (triggers release workflow)"
    Write-Host "    .\release.ps1 dry-run patch            # Preview patch bump without changes"
    Write-Host ""
    exit 1
}

# Main command handling
switch ($Command) {
    "current" {
        $current = Get-CurrentVersion
        Write-Info "Current version: $current"
    }
    
    "bump" {
        if (-not $Argument) {
            Write-Error-Exit "Bump type required (major|minor|patch)"
        }
        
        $current = Get-CurrentVersion
        $newVersion = Get-BumpedVersion -CurrentVersion $current -BumpType $Argument
        
        Write-Info "Current version: $current"
        Write-Info "New version: $newVersion"
        
        $response = Read-Host "Proceed with version bump? (y/N)"
        if ($response -notmatch '^[Yy]$') {
            Write-Warning "Aborted"
            exit 0
        }
        
        Update-PackageVersion -NewVersion $newVersion
        New-GitTag -Version $newVersion
        
        Write-Success "Version bumped to $newVersion"
        Write-Info "Run '.\release.ps1 push' to push changes and trigger release workflow"
    }
    
    "tag" {
        if (-not $Argument) {
            Write-Error-Exit "Version required"
        }
        
        # Remove 'v' prefix if present
        $version = $Argument -replace '^v', ''
        
        $current = Get-CurrentVersion
        
        if ($version -ne $current) {
            Write-Warning "Version mismatch: package.json has $current but you want to tag $version"
            $response = Read-Host "Update package.json to $version? (y/N)"
            if ($response -match '^[Yy]$') {
                Update-PackageVersion -NewVersion $version
            } else {
                Write-Error-Exit "Aborted - version mismatch"
            }
        }
        
        New-GitTag -Version $version
        Write-Success "Tag created for version $version"
    }
    
    "push" {
        Write-Info "Pushing commits and tags to remote..."
        
        git push origin main
        git push origin --tags
        
        Write-Success "Pushed to remote"
        Write-Info "Release workflow should trigger automatically"
    }
    
    "dry-run" {
        if (-not $Argument) {
            Write-Error-Exit "Bump type required (major|minor|patch)"
        }
        
        $current = Get-CurrentVersion
        $newVersion = Get-BumpedVersion -CurrentVersion $current -BumpType $Argument
        
        Write-Info "DRY RUN - No changes will be made"
        Write-Info "Current version: $current"
        Write-Info "Would bump to: $newVersion"
        Write-Info "Would create tag: v$newVersion"
    }
    
    default {
        Show-Usage
    }
}