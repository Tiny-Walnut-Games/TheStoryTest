# Release Process

This document describes the automated release process for The Story Test Framework.

## Overview

Releases are automated using GitHub Actions. When you push a version tag, the workflow:
1. Validates the code with Story Test
2. Generates release notes from git commits
3. Creates a GitHub release
4. Updates CHANGELOG.md automatically

## Quick Release

### Using the Helper Script (Recommended)

```bash
# Check current version
./scripts/release.sh current

# Bump patch version (1.0.0 -> 1.0.1)
./scripts/release.sh bump patch

# Bump minor version (1.0.0 -> 1.1.0)
./scripts/release.sh bump minor

# Bump major version (1.0.0 -> 2.0.0)
./scripts/release.sh bump major

# Push to trigger release
./scripts/release.sh push
```

### Manual Process

```bash
# 1. Update version in package.json
jq '.version = "1.2.3"' Packages/com.tinywalnutgames.storytest/package.json > tmp.json
mv tmp.json Packages/com.tinywalnutgames.storytest/package.json

# 2. Commit and tag
git add Packages/com.tinywalnutgames.storytest/package.json
git commit -m "🔖 Bump version to 1.2.3"
git tag -a v1.2.3 -m "Release version 1.2.3"

# 3. Push (triggers release workflow)
git push origin main
git push origin --tags
```

## Commit Message Conventions

For better changelog generation, use conventional commit messages:

- `feat:` - New features
- `fix:` - Bug fixes
- `docs:` - Documentation changes
- `refactor:` - Code refactoring
- `test:` - Test additions/changes
- `chore:` - Maintenance tasks

Examples:
```
feat: Add Act 12 for unused parameters
fix: Correct false positives in enum validation
docs: Update installation instructions
refactor: Simplify IL analysis logic
```

## What Gets Automated

### ✅ Automated
- Version tagging
- Release notes generation
- GitHub release creation
- CHANGELOG.md updates
- Package validation

### ❌ Not Automated (Yet)
- Unity Package Manager publishing
- npm package publishing (if applicable)
- Documentation site deployment

## Testing a Release (Dry Run)

You can test the release workflow without actually creating a release:

1. Go to Actions tab in GitHub
2. Select "Release Automation" workflow
3. Click "Run workflow"
4. Enter version and check "Dry run"
5. Review the output

## Troubleshooting

### Version Mismatch Error
If you get a version mismatch error, ensure `package.json` version matches your tag:
```bash
# Check package.json version
jq -r '.version' Packages/com.tinywalnutgames.storytest/package.json

# Should match your tag (without 'v' prefix)
```

### Tag Already Exists
If a tag already exists, delete it first:
```bash
# Delete local tag
git tag -d v1.2.3

# Delete remote tag
git push origin :refs/tags/v1.2.3
```

### Release Workflow Fails
Check the Actions tab for detailed error messages. Common issues:
- Story Test validation failures
- Missing GITHUB_TOKEN permissions
- Network issues during artifact upload

## Philosophy

This automated process embodies Story Test principles:
- **Narrative Continuity**: Git history becomes the changelog
- **Validation First**: No release without passing tests
- **Reduce Friction**: One command to release
- **Preserve Context**: Release notes capture the story

---

*"Every release tells a story. Let automation write the boring parts."* 📦