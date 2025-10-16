# 🔐 Branch Protection Configuration Guide

This document outlines the branch protection rules for the three-branch release strategy:
- **`develop`** (moderate protection) - where features are developed
- **`release`** (light protection) - where releases are prepared and published
- **`main`** (strict protection) - production releases only

## Three-Branch Strategy Overview

```
develop (feature branches merge here)
   ↓
develop → release (when ready to release)
   ↓
release (tag version, publish to PyPI, tests run)
   ↓
release → main (merge back after successful release)
   ↓
main (production - always stable)
   ↓
main → develop (merge back to keep in sync)
```

**As a solo developer**, you can push directly to `develop` and `release` branches, but the workflows ensure quality and consistency.

---

## ⚙️ Configuration Steps

### 1. **`main` Branch (Strict Protection)**

Navigate to: **Settings → Branches → Add rule**

#### Rule Name Pattern
- `main`

#### Protection Settings

**✅ Require a pull request before merging**
- Require approvals: **0** (you're solo, but workflow checks must pass)
- Dismiss stale pull request approvals when new commits are pushed: *Checked*
- Require review from code owners: *Unchecked*

**✅ Require status checks to pass before merging**
- Status checks that must pass:
  - `Quick Static Analysis` (from story-test.yml)
  - `Full Validation` (from story-test.yml)
  - (Any other critical checks)

**✅ Require branches to be up to date before merging**
- *Checked*

**✅ Require deployments to succeed before merging**
- *(Optional)* Can include PyPI deployment check

**✅ Include administrators in restrictions**
- *Checked* (enforces rules even for you)

**❌ Do NOT check**
- Restrict who can push to matching branches (you need to push to this branch after release)

---

### 2. **`release` Branch (Light Protection)**

Navigate to: **Settings → Branches → Add rule**

#### Rule Name Pattern
- `release`

#### Protection Settings

**✅ Require a pull request before merging**
- Require approvals: **0**
- Dismiss stale pull request approvals when new commits are pushed: *Checked*

**✅ Require status checks to pass before merging**
- Status checks that must pass:
  - `Quick Static Analysis` (from story-test.yml)

**❌ Do NOT require**
- Require branches to be up to date
- Require deployments to succeed
- Restrict administrators

**💡 Why lighter?** Because:
- Release branch is for finalizing releases only
- You'll push version bumps and tags directly here
- You want minimal friction for the release process

---

### 3. **`develop` Branch (Moderate Protection)**

Navigate to: **Settings → Branches → Add rule**

#### Rule Name Pattern
- `develop`

#### Protection Settings

**✅ Require a pull request before merging**
- Require approvals: **0** (you're solo)
- Dismiss stale pull request approvals when new commits are pushed: *Checked*

**✅ Require status checks to pass before merging**
- Status checks that must pass:
  - `Quick Static Analysis`
  - `Full Validation` (optional - for stricter control)

**✅ Require branches to be up to date before merging**
- *Checked* (prevents accidental merge conflicts)

**❌ Do NOT restrict**
- Administrator push restrictions (you'll need to push version bumps)

**💡 Why moderate?** Because:
- This is where features are developed
- You want fast feedback but consistency
- Version bumps are pushed here too

---

## 📋 Quick Reference Table

| Setting | `main` | `release` | `develop` |
|---------|--------|-----------|-----------|
| **Require PR** | ✅ | ✅ | ✅ |
| **Approvals needed** | 0 | 0 | 0 |
| **Status checks** | ✅ (All) | ✅ (Quick) | ✅ (Moderate) |
| **Up-to-date required** | ✅ | ❌ | ✅ |
| **Admin restrictions** | ✅ | ❌ | ❌ |
| **Direct push allowed** | ❌ | ✅ | ✅ |

---

## 🚀 Typical Release Workflow

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

---

## 🔧 Customization Notes

### If you want stricter `develop` protection:
Add `Full Validation` to the status checks for longer but more thorough checks before merge.

### If you want to prevent direct pushes:
Enable "Restrict who can push to matching branches" on `main` (but keep disabled on `release` and `develop` for solo work).

### For organization secrets:
The workflows already use `${{ secrets.GITHUB_TOKEN }}` which has proper permissions. No need for custom secrets for releases.

---

## ⚡ Troubleshooting

### "Resource not accessible by integration" error on release?
- ✅ Already fixed! We added `permissions: { contents: write }` to the release workflow

### Merge conflicts when merging back?
- This is rare. If it happens: manually resolve in the merge-back workflow or trigger a fresh sync

### Want to skip a branch protection check temporarily?
- You can't (by design!), but you can create a new branch, merge to it, then merge to main
- Or: temporarily disable the rule, do the action, then re-enable

---

## 📚 Related Documentation

- [Release Process](../docs/RELEASE_PROCESS.md)
- [CI/CD Configuration](../docs/ci-cd.md)
- [GitHub Workflows](./README.md)