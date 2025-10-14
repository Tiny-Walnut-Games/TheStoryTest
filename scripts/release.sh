#!/bin/bash
# Story Test Release Helper
# Automates version bumping and tagging for releases

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PACKAGE_JSON="$PROJECT_ROOT/Packages/com.tinywalnutgames.storytest/package.json"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Helper functions
info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

success() {
    echo -e "${GREEN}✅ $1${NC}"
}

warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

error() {
    echo -e "${RED}❌ $1${NC}"
    exit 1
}

# Get current version from package.json
get_current_version() {
    if [ ! -f "$PACKAGE_JSON" ]; then
        error "package.json not found at $PACKAGE_JSON"
    fi
    
    jq -r '.version' "$PACKAGE_JSON"
}

# Bump version number
bump_version() {
    local current_version="$1"
    local bump_type="$2"
    
    IFS='.' read -r major minor patch <<< "$current_version"
    
    case "$bump_type" in
        major)
            major=$((major + 1))
            minor=0
            patch=0
            ;;
        minor)
            minor=$((minor + 1))
            patch=0
            ;;
        patch)
            patch=$((patch + 1))
            ;;
        *)
            error "Invalid bump type: $bump_type (use: major, minor, or patch)"
            ;;
    esac
    
    echo "$major.$minor.$patch"
}

# Update version in package.json
update_package_version() {
    local new_version="$1"
    
    info "Updating package.json to version $new_version..."
    
    # Use jq to update version
    jq ".version = \"$new_version\"" "$PACKAGE_JSON" > "$PACKAGE_JSON.tmp"
    mv "$PACKAGE_JSON.tmp" "$PACKAGE_JSON"
    
    success "package.json updated"
}

# Create git tag
create_tag() {
    local version="$1"
    local tag="v$version"
    
    info "Creating git tag $tag..."
    
    # Check if tag already exists
    if git rev-parse "$tag" >/dev/null 2>&1; then
        error "Tag $tag already exists"
    fi
    
    # Commit version change
    git add "$PACKAGE_JSON"
    git commit -m "🔖 Bump version to $version" || warning "No changes to commit"
    
    # Create annotated tag
    git tag -a "$tag" -m "Release version $version"
    
    success "Tag $tag created"
}

# Show usage
usage() {
    cat <<EOF
Story Test Release Helper

Usage: $0 <command> [options]

Commands:
    current             Show current version
    bump <type>         Bump version (major|minor|patch) and create tag
    tag <version>       Create tag for specific version
    push                Push commits and tags to remote
    dry-run <type>      Show what would happen without making changes

Examples:
    $0 current                  # Show current version
    $0 bump patch               # Bump patch version (1.0.0 -> 1.0.1)
    $0 bump minor               # Bump minor version (1.0.0 -> 1.1.0)
    $0 bump major               # Bump major version (1.0.0 -> 2.0.0)
    $0 tag 1.2.3                # Create tag for version 1.2.3
    $0 push                     # Push to remote (triggers release workflow)
    $0 dry-run patch            # Preview patch bump without changes

EOF
    exit 1
}

# Main command handling
case "${1:-}" in
    current)
        CURRENT=$(get_current_version)
        info "Current version: $CURRENT"
        ;;
    
    bump)
        BUMP_TYPE="${2:-}"
        if [ -z "$BUMP_TYPE" ]; then
            error "Bump type required (major|minor|patch)"
        fi
        
        CURRENT=$(get_current_version)
        NEW_VERSION=$(bump_version "$CURRENT" "$BUMP_TYPE")
        
        info "Current version: $CURRENT"
        info "New version: $NEW_VERSION"
        
        read -p "Proceed with version bump? (y/N) " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            warning "Aborted"
            exit 0
        fi
        
        update_package_version "$NEW_VERSION"
        create_tag "$NEW_VERSION"
        
        success "Version bumped to $NEW_VERSION"
        info "Run '$0 push' to push changes and trigger release workflow"
        ;;
    
    tag)
        VERSION="${2:-}"
        if [ -z "$VERSION" ]; then
            error "Version required"
        fi
        
        # Remove 'v' prefix if present
        VERSION="${VERSION#v}"
        
        CURRENT=$(get_current_version)
        
        if [ "$VERSION" != "$CURRENT" ]; then
            warning "Version mismatch: package.json has $CURRENT but you want to tag $VERSION"
            read -p "Update package.json to $VERSION? (y/N) " -n 1 -r
            echo
            if [[ $REPLY =~ ^[Yy]$ ]]; then
                update_package_version "$VERSION"
            else
                error "Aborted - version mismatch"
            fi
        fi
        
        create_tag "$VERSION"
        success "Tag created for version $VERSION"
        ;;
    
    push)
        info "Pushing commits and tags to remote..."
        
        git push origin main
        git push origin --tags
        
        success "Pushed to remote"
        info "Release workflow should trigger automatically"
        ;;
    
    dry-run)
        BUMP_TYPE="${2:-}"
        if [ -z "$BUMP_TYPE" ]; then
            error "Bump type required (major|minor|patch)"
        fi
        
        CURRENT=$(get_current_version)
        NEW_VERSION=$(bump_version "$CURRENT" "$BUMP_TYPE")
        
        info "DRY RUN - No changes will be made"
        info "Current version: $CURRENT"
        info "Would bump to: $NEW_VERSION"
        info "Would create tag: v$NEW_VERSION"
        ;;
    
    *)
        usage
        ;;
esac