# 🔄 Workflows & Release Strategy

Comprehensive guide covering branch management, release automation, protection rules, and dependency management.

---

## 🌳 Three-Branch Workflow

Quick reference for daily development and release workflow.

### 📍 Branch Purposes

| Branch | Purpose | Protection | Direct Push? |
|--------|---------|-----------|--------------|
| `develop` | Feature development | Moderate | ✅ Yes |
| `release` | Release preparation | Light | ✅ Yes |
| `main` | Production releases | Strict | ❌ Via PR only |

### 💻 Daily Development

#### Starting feature work
```bash
git checkout develop
git pull origin develop
# ... make changes ...
git commit -am "Add awesome feature"
git push origin develop
```

#### Version bump (before release)
```bash
git checkout develop
./scripts/release.ps1 bump minor  # or bump major/patch
git push origin develop
# Workflows run automatically to validate
```

### 🚀 Release Process

#### Step 1: Prepare Release Branch
```bash
git checkout release
git pull origin release
git merge develop              # or rebase if you prefer
git push origin release
```

#### Step 2: Tag and Release
```bash
# Option A: Use release script (recommended)
./scripts/release.ps1 bump minor
git push origin release --tags

# Option B: Manual tag
git tag v1.3.0
git push origin release --tags
```

#### Step 3: Automation ✨
1. **`release-automation.yml` runs:**
   - ✅ Validates code quality
   - ✅ Publishes to PyPI
   - ✅ Creates GitHub release with notes
   - ✅ Triggers merge-back workflow

2. **`merge-back-to-main.yml` runs:**
   - ✅ Merges `release` → `main`
   - ✅ Merges `main` → `develop`
   - ✅ All branches stay in sync!

#### Step 4: Verify
```bash
git fetch origin
git log --oneline origin/main -5
git log --oneline origin/develop -5
# Should see your release merge
```

### 🚨 Common Scenarios

#### "I made a mistake in develop, can I fix it?"
**Yes!** Commit fixes directly to develop:
```bash
git checkout develop
git commit -am "Fix: typo in validation logic"
git push origin develop
```

#### "I want to release from develop immediately"
**That's fine!** Skip release branch temporarily:
```bash
# Faster release (only do for hotfixes)
git checkout develop
./scripts/release.ps1 bump patch
git push origin develop --tags
# Automation takes over
```

#### "I need to revert a release"
```bash
# On release branch, revert the tag
git checkout release
git tag -d v1.3.0
git push origin :v1.3.0  # Delete from remote
# Then fix the issue and re-release with same/new version
```

#### "Status checks are failing on main"
The merge-back workflow will wait. Don't manually merge! Instead:
```bash
git checkout develop
# Fix the issue
git push origin develop
# Retry the merge-back workflow from GitHub Actions
```

### 🔄 Branch Sync Strategy

All workflows automatically keep branches in sync:

```
develop (your features)
   ↓ (merge when ready)
release (prepare for release)
   ↓ (tag and push)
PUBLISH TO PYPI ✨
   ↓ (auto-merge-back)
main (production snapshot)
   ↓ (auto-merge)
develop (back in sync!)
```

**Result:** You only need to manage `develop`. Everything else syncs automatically.

### 📊 Branch Status Check

```bash
# Quick status of all three branches
git fetch origin
echo "=== DEVELOP ==="
git log --oneline origin/develop -3
echo ""
echo "=== RELEASE ==="
git log --oneline origin/release -3
echo ""
echo "=== MAIN ==="
git log --oneline origin/main -3
```

### ⚡ Pro Tips

#### Tip 1: Automatic Branch Protection
All branches have status checks. If tests fail on develop, don't worry—just fix and push again:
```bash
git checkout develop
# Fix the issue
git push origin develop
# Workflows re-run automatically
```

#### Tip 2: Release Branch is Your Staging
Use the `release` branch like a staging environment:
```bash
git checkout release
git merge develop
git push origin release
# Push to release first to let workflows validate
# If tests pass, then tag and publish
git tag v1.3.0
git push origin release --tags
```

#### Tip 3: Hotfixes
For urgent hotfixes:
```bash
git checkout release
git pull origin release
# Fix the bug
git commit -am "Hotfix: critical bug"
git tag v1.2.2
git push origin release --tags
```

#### Tip 4: Keep Develop Fresh
Before starting new work:
```bash
git checkout develop
git fetch origin
git reset --hard origin/develop
# Now you're in sync with the merged releases
```

---

## 🔐 Branch Protection Configuration

Branch protection rules for the three-branch release strategy:
- **`develop`** (moderate protection) - where features are developed
- **`release`** (light protection) - where releases are prepared and published
- **`main`** (strict protection) - production releases only

### ⚙️ Configuration Steps

#### 1. **`main` Branch (Strict Protection)**

Navigate to: **Settings → Branches → Add rule**

**Rule Name Pattern**: `main`

**Protection Settings**

✅ **Require a pull request before merging**
- Require approvals: **0** (you're solo, but workflow checks must pass)
- Dismiss stale pull request approvals when new commits are pushed: *Checked*
- Require review from code owners: *Unchecked*

✅ **Require status checks to pass before merging**
- Status checks that must pass:
  - `Quick Static Analysis` (from story-test.yml)
  - `Full Validation` (from story-test.yml)
  - (Any other critical checks)

✅ **Require branches to be up to date before merging**
- *Checked*

✅ **Require deployments to succeed before merging**
- *(Optional)* Can include PyPI deployment check

✅ **Include administrators in restrictions**
- *Checked* (enforces rules even for you)

❌ **Do NOT check**
- Restrict who can push to matching branches (you need to push to this branch after release)

#### 2. **`release` Branch (Light Protection)**

Navigate to: **Settings → Branches → Add rule**

**Rule Name Pattern**: `release`

**Protection Settings**

✅ **Require a pull request before merging**
- Require approvals: **0**
- Dismiss stale pull request approvals when new commits are pushed: *Checked*

✅ **Require status checks to pass before merging**
- Status checks that must pass:
  - `Quick Static Analysis` (from story-test.yml)

❌ **Do NOT require**
- Require branches to be up to date
- Require deployments to succeed
- Restrict administrators

**Why lighter?** Because:
- Release branch is for finalizing releases only
- You'll push version bumps and tags directly here
- You want minimal friction for the release process

#### 3. **`develop` Branch (Moderate Protection)**

Navigate to: **Settings → Branches → Add rule**

**Rule Name Pattern**: `develop`

**Protection Settings**

✅ **Require a pull request before merging**
- Require approvals: **0** (you're solo)
- Dismiss stale pull request approvals when new commits are pushed: *Checked*

✅ **Require status checks to pass before merging**
- Status checks that must pass:
  - `Quick Static Analysis`
  - `Full Validation` (optional - for stricter control)

✅ **Require branches to be up to date before merging**
- *Checked* (prevents accidental merge conflicts)

❌ **Do NOT restrict**
- Administrator push restrictions (you'll need to push version bumps)

**Why moderate?** Because:
- This is where features are developed
- You want fast feedback but consistency
- Version bumps are pushed here too

### 📋 Quick Reference Table

| Setting | `main` | `release` | `develop` |
|---------|--------|-----------|-----------|
| **Require PR** | ✅ | ✅ | ✅ |
| **Approvals needed** | 0 | 0 | 0 |
| **Status checks** | ✅ (All) | ✅ (Quick) | ✅ (Moderate) |
| **Up-to-date required** | ✅ | ❌ | ✅ |
| **Admin restrictions** | ✅ | ❌ | ❌ |
| **Direct push allowed** | ❌ | ✅ | ✅ |

### 🚀 Typical Release Workflow

```bash
# 1. Work on features in develop
git checkout develop
git pull origin develop
# ... make changes ...
git push origin develop

# 2. When ready for release, switch to release branch
git checkout release
git pull origin release

# 3. Merge develop into release
git merge develop

# 4. Run release script (bumps version in all files)
./scripts/release.ps1 bump minor
git push origin release --tags

# 5. Automation takes over:
#    - release-automation.yml validates and publishes to PyPI
#    - merge-back-to-main.yml merges back: release → main → develop
#    - All branches stay in sync!

# 6. Confirm the merge back completed
git fetch origin
git checkout main
git pull origin main
git checkout develop
git pull origin develop
```

### 🔧 Customization Notes

#### If you want stricter `develop` protection:
Add `Full Validation` to the status checks for longer but more thorough checks before merge.

#### If you want to prevent direct pushes:
Enable "Restrict who can push to matching branches" on `main` (but keep disabled on `release` and `develop` for solo work).

#### For organization secrets:
The workflows already use `${{ secrets.GITHUB_TOKEN }}` which has proper permissions. No need for custom secrets for releases.

### ⚡ Troubleshooting

#### "Resource not accessible by integration" error on release?
- ✅ Already fixed! We added `permissions: { contents: write }` to the release workflow

#### Merge conflicts when merging back?
- This is rare. If it happens: manually resolve in the merge-back workflow or trigger a fresh sync

#### Want to skip a branch protection check temporarily?
- You can't (by design!), but you can create a new branch, merge to it, then merge to main
- Or: temporarily disable the rule, do the action, then re-enable

---

## 📦 Dependency Management Strategy

Package dependencies are managed across the Story Test Framework to ensure compatibility across multiple Unity versions and platforms (Windows, macOS, Linux).

### Problem Statement

Previously, the project used hardcoded package versions (e.g., `"com.unity.test-framework": "1.6.0"`) which caused platform-specific failures, especially on Linux CI/CD builds where certain package versions were unavailable in the registry.

### Solution: Conservative Versioning

Instead of using the latest specific versions that work on one platform, we use **proven, widely-available versions** that are compatible across all platforms and Unity versions (2022.3 LTS through 6.x).

### Updated Package Versions

The following package versions have been standardized across both:
- `Packages/manifest.json` (main project)
- `Samples~/ExampleProject/Packages/manifest.json` (CI/CD build project)

| Package | Old Version | New Version | Reason |
|---------|------------|-------------|--------|
| com.unity.test-framework | 1.6.0 | 1.1.0 | More universally available |
| com.unity.render-pipelines.universal | 17.2.0 | 14.0.0 | Stable cross-platform version |
| com.unity.inputsystem | 1.14.2 | 1.7.0 | Stable, widely compatible |
| com.unity.ide.rider | 3.0.38 | 3.0.0 | Base version is sufficient |
| com.unity.visualscripting | 1.9.7-1.9.8 | 1.7.0 | Stable, pre-1.8+ breaking changes |
| com.unity.ai.navigation | 2.0.9 | 2.0.0 | Base version works across versions |
| com.unity.collab-proxy | 2.9.3 | 2.0.0 | More stable across platforms |

### CI/CD Unity Version

**Workflow Build Version**: `2022.3.26f1` (LTS)
- **Before**: `6000.0.30f1` (bleeding edge, platform inconsistencies)
- **After**: `2022.3.26f1` (stable LTS, all packages reliably available)

This ensures the CI/CD build uses a version with guaranteed package availability on all platforms.

### Development vs. CI/CD

- **Local Development**: Can continue using Unity 6.2+ with the same manifest files
- **CI/CD Pipeline**: Uses `2022.3.26f1` for consistent, reproducible builds
- **Package Versions**: Compatible with both ranges (2022.3 LTS through 6.x)

### Synchronization Strategy

#### Automatic Dependency Sync

To keep dependencies in sync across the repository, a utility script manages version consistency:

```bash
# Python script to sync versions across manifests
python scripts/sync_dependencies.py
```

This script:
1. Scans all `manifest.json` files
2. Maintains version compatibility matrix
3. Flags incompatible version combinations
4. Can auto-update to known-good versions

#### Manual Override

If you need to update package versions:

1. Update `Packages/manifest.json` (source of truth)
2. Update `Samples~/ExampleProject/Packages/manifest.json` to match
3. Run `python scripts/sync_dependencies.py --verify` to validate
4. Test locally before pushing to CI/CD

### Platform Compatibility

#### Linux (CI/CD Primary Platform)

The most restrictive platform for package availability. All versions are validated on Linux first.

#### macOS & Windows

Generally have better registry coverage. If a version works on Linux, it works on these platforms.

### Migration Path

For users updating their local projects:

1. **Option A**: Keep your manifest.json as-is (auto-resolves to available versions)
2. **Option B**: Use the updated versions for better consistency:
   ```json
   "com.unity.test-framework": "1.1.0"
   ```

### Troubleshooting

#### If CI/CD still fails on Linux:

1. Check the error message for the specific missing package
2. Note the package name and requested version
3. Update the corresponding entry in both manifest files to a lower minor version
4. Test locally first before pushing

#### If local development breaks:

1. Run `npm install` or Unity Package Manager refresh
2. Check that your local Unity version is compatible (2020.3 LTS or later)
3. Run `python scripts/sync_dependencies.py` to restore known-good versions

### Version Constraints

Future reference for choosing compatible versions:

- **Test Framework**: 1.1.0+ (available since 2020.3 LTS)
- **URP**: 7.0.0+ (available since 2019.3, 14.0.0+ for modern versions)
- **Input System**: 1.0.0+ (available since 2019.4, 1.7.0+ for stability)
- **Visual Scripting**: 1.7.0+ (modern version without breaking changes)
- **Rider Plugin**: 3.0.0+ (widely available)

### Future Improvements

Potential enhancements to automate this further:

- [ ] Add dependency version matrix to pyproject.toml
- [ ] Implement GitHub Action to validate package availability
- [ ] Create version constraints file (requirements-unity.txt equivalent)
- [ ] Auto-update versions when new LTS releases occur

---

## 📚 Related Documentation

- [Release Process](../docs/RELEASE_PROCESS.md)
- [CI/CD Configuration](../docs/ci-cd.md)
- [GitHub Workflows](./README.md)