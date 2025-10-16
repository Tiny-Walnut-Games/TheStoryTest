# 🌳 Three-Branch Workflow Guide

Quick reference for your daily development and release workflow.

## 📍 Branch Purposes

| Branch | Purpose | Protection | Direct Push? |
|--------|---------|-----------|--------------|
| `develop` | Feature development | Moderate | ✅ Yes |
| `release` | Release preparation | Light | ✅ Yes |
| `main` | Production releases | Strict | ❌ Via PR only |

---

## 💻 Daily Development

### Starting feature work
```bash
git checkout develop
git pull origin develop
# ... make changes ...
git commit -am "Add awesome feature"
git push origin develop
```

### Version bump (before release)
```bash
git checkout develop
./scripts/release.ps1 bump minor  # or bump major/patch
git push origin develop
# Workflows run automatically to validate
```

---

## 🚀 Release Process

### Step 1: Prepare Release Branch
```bash
git checkout release
git pull origin release
git merge develop              # or rebase if you prefer
git push origin release
```

### Step 2: Tag and Release
```bash
# Option A: Use release script (recommended)
./scripts/release.ps1 bump minor
git push origin release --tags

# Option B: Manual tag
git tag v1.3.0
git push origin release --tags
```

### Step 3: Automation ✨
1. **`release-automation.yml` runs:**
   - ✅ Validates code quality
   - ✅ Publishes to PyPI
   - ✅ Creates GitHub release with notes
   - ✅ Triggers merge-back workflow

2. **`merge-back-to-main.yml` runs:**
   - ✅ Merges `release` → `main`
   - ✅ Merges `main` → `develop`
   - ✅ All branches stay in sync!

### Step 4: Verify
```bash
git fetch origin
git log --oneline origin/main -5
git log --oneline origin/develop -5
# Should see your release merge
```

---

## 🚨 Common Scenarios

### "I made a mistake in develop, can I fix it?"
**Yes!** Commit fixes directly to develop:
```bash
git checkout develop
git commit -am "Fix: typo in validation logic"
git push origin develop
```

### "I want to release from develop immediately"
**That's fine!** Skip release branch temporarily:
```bash
# Faster release (only do for hotfixes)
git checkout develop
./scripts/release.ps1 bump patch
git push origin develop --tags
# Automation takes over
```

### "I need to revert a release"
```bash
# On release branch, revert the tag
git checkout release
git tag -d v1.3.0
git push origin :v1.3.0  # Delete from remote
# Then fix the issue and re-release with same/new version
```

### "Status checks are failing on main"
The merge-back workflow will wait. Don't manually merge! Instead:
```bash
git checkout develop
# Fix the issue
git push origin develop
# Retry the merge-back workflow from GitHub Actions
```

---

## 🔄 Branch Sync Strategy

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

---

## 📊 Branch Status Check

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

---

## ⚡ Pro Tips

### Tip 1: Automatic Branch Protection
All branches have status checks. If tests fail on develop, don't worry—just fix and push again:
```bash
git checkout develop
# Fix the issue
git push origin develop
# Workflows re-run automatically
```

### Tip 2: Release Branch is Your Staging
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

### Tip 3: Hotfixes
For urgent hotfixes:
```bash
git checkout release
git pull origin release
# Fix the bug
git commit -am "Hotfix: critical bug"
git tag v1.2.2
git push origin release --tags
```

### Tip 4: Keep Develop Fresh
Before starting new work:
```bash
git checkout develop
git fetch origin
git reset --hard origin/develop
# Now you're in sync with the merged releases
```

---

## 🎯 The Big Picture

You only need to think about **two branches**:

1. **`develop`** - where you work
2. **`release`** - where you publish from

Everything else (`main`, branches syncing, PyPI publishing) is **automated**. ✨

```
Your workflow:
1. Code in develop (push anytime)
2. When ready to release:
   - Merge develop to release
   - Push tags to release
   - Go have coffee ☕
3. Automation handles:
   - Validation ✓
   - PyPI publishing ✓
   - Main branch update ✓
   - Develop syncing ✓
4. You're done! 🎉
```

---

## 📚 See Also
- [Branch Protection Configuration](./BRANCH_PROTECTION_GUIDE.md)
- [Release Process Documentation](../docs/RELEASE_PROCESS.md)