# Multi-Target Release Matrix

The Story Test Framework can be released to **three separate package ecosystems simultaneously**:

- 🐍 **PyPI** - Python package for CLI validation
- 🎮 **Unity Package Manager (UPM)** - C# framework for Unity
- 📦 **npm** - Node package manager (scripts & tooling)

---

## Release Targets Explained

### PyPI (🐍 Python Package)

**What**: The `storytest` Python package distributed via [pypi.org](https://pypi.org/project/storytest/)

**Installation**:
```bash
pip install storytest
storytest validate /path/to/project --verbose
```

**Includes**:
- `storytest/` module with CLI and validator
- Python dependencies (pythonnet, clr-loader, colorama)

**Does NOT include**:
- C# sources or assemblies
- Unity-specific files

---

### UPM (🎮 Unity Package Manager)

**What**: The `com.tinywalnutgames.storytest` package from GitHub via UPM

**Installation**:
```
Via Package Manager (Recommended):
1. Window > Package Manager
2. Click + icon → Add package from git URL
3. Paste: https://github.com/jmeyer1980/TheStoryTest.git#v1.3.0
4. Click Add

Or directly in manifest.json:
{
  "dependencies": {
    "com.tinywalnutgames.storytest": "https://github.com/jmeyer1980/TheStoryTest.git#v1.3.0"
  }
}
```

**Includes**:
- C# validation framework (Acts 1-11 + optional 12-13)
- Editor integration tools
- Tests and samples

**Does NOT include**:
- Python files or dependencies
- CLI scripts

---

### npm (📦 Node Package Manager)

**What**: The `@tinywalnutgames/storytest` package on npmjs.com

**Installation**:
```bash
npm install @tinywalnutgames/storytest

# Or use directly:
npx storytest validate /path/to/project
```

**Includes**:
- CLI scripts for validation
- Package metadata
- Documentation

**Note**: Currently not actively used. Priority is PyPI + UPM.

---

## How to Release (Multi-Target)

### Prerequisites
1. Update version in:
   - `package.json` (root)
   - `pyproject.toml`
   - `Packages/com.tinywalnutgames.storytest/package.json`

2. Update `CHANGELOG.md`

### Release Steps

#### Option 1: Manual Dispatch (Recommended for Jam)
1. Go to **Actions** tab in GitHub
2. Select **📦 Multi-Target Release (PyPI + UPM)**
3. Click **Run workflow**
4. Fill in:
   - **Version**: e.g., `1.3.0`
   - **Targets**: `pypi,upm` (default recommended)
   - **Dry Run**: Toggle to test first (recommended!)

#### Option 2: Automatic on Release
1. Create a GitHub Release
2. Tag it as `v1.3.0`
3. Workflow auto-triggers on publish
4. Publishes to PyPI + UPM automatically

#### Option 3: Command Line (If you have access)
```bash
# Update versions
npm version 1.3.0

# Tag and push
git tag v1.3.0
git push origin v1.3.0

# GitHub Actions will take it from there
```

---

## Release Checklist

Before triggering release workflow:

- [ ] All versions match: `package.json`, `pyproject.toml`, Unity `package.json`
- [ ] CHANGELOG.md updated with v1.3.0 entry
- [ ] Acts 1-11 validation passes: `python scripts/story_test_unity_safe.py . --fail-on-violations`
- [ ] Python package builds cleanly: `python -m build`
- [ ] Tests pass: `npm test`
- [ ] Documentation updated (if changed)
- [ ] No "experimental" or "WIP" language in docs

---

## Dry Run First!

**Always do a dry run before real release**:

1. In workflow dispatch, set **Dry Run: true**
2. Check the output to verify what would be published
3. Once confirmed, run again with **Dry Run: false**

---

## What Gets Published Where

### PyPI
- `storytest/` Python package only
- Excludes: C#, Assets, Library, ProjectSettings
- Package size: ~50KB
- Installed with: `pip install storytest`

### UPM
- `Packages/com.tinywalnutgames.storytest/` only
- Uses git tag (e.g., `v1.3.0`)
- Available via: `https://github.com/jmeyer1980/TheStoryTest.git#v1.3.0`
- No separate build/upload needed—git tag is the "release"

### npm (Optional)
- Root `package.json` + scripts
- Scripts symlink to `scripts/`
- Excludes: Packages/, Assets/, C# files (via .npmignore)
- Installed with: `npm install @tinywalnutgames/storytest`

---

## Troubleshooting Release

### "Version mismatch error"
- Ensure all three version fields match exactly
- `package.json`, `pyproject.toml`, `Packages/.../package.json`

### "PyPI upload failed"
- Check `secrets.PYPI_API_TOKEN` is set in GitHub Secrets
- Verify package builds locally: `python -m build`

### "UPM not appearing in Package Manager"
- UPM uses git tags directly—no separate publish step needed
- If git tag exists, UPM URL should work immediately
- May take a few minutes to appear in searches

### "npm publish failed"
- Check `secrets.NPM_API_TOKEN` is set
- Verify package name in `package.json` matches npm registry

---

## Game Jam Release Path

For a fast game jam release:

```
1. Update versions in all three files
2. Run workflow with:
   - Targets: pypi,upm (skip npm unless needed)
   - Dry Run: true (preview first!)
3. Review output
4. Run again with Dry Run: false
5. Done! 🎉
```

Total time: ~5 minutes (mostly CI/CD execution)

---

## Post-Release

After successful release:

1. ✅ Check that PyPI page updates: https://pypi.org/project/storytest/
2. ✅ Verify UPM git URL works: `https://github.com/.../TheStoryTest.git#v1.3.0`
3. ✅ Announce in CHANGELOG and README
4. ✅ Update any installation docs if paths changed

---

## Questions?

- **PyPI issues**: Check `docs/PYPI_PUBLISHING.md`
- **Unity integration**: Check `Packages/com.tinywalnutgames.storytest/package.json`
- **Workflow issues**: Check `.github/workflows/publish-matrix.yml`