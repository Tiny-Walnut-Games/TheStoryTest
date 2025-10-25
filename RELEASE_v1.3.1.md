# v1.3.1 Release Checklist - The Story Test Framework

**Status**: ✅ **READY FOR RELEASE**  
**Date**: 2025-12-20  
**Targets**: PyPI (Python) + UPM (C# Unity)

---

## ✅ Pre-Release Verification

### Code Quality
- [x] `python scripts/story_test_unity_safe.py . --verbose` runs without errors
- [x] Exit code: 0 (no violations)
- [x] Assemblies validated: 6 non-Unity assemblies, 98 types checked
- [x] UTF-8 encoding fixed (Unicode output support)

### Version Consistency
- [x] `package.json` version: 1.3.1
- [x] `pyproject.toml` version: 1.3.1
- [x] `Packages/com.tinywalnutgames.storytest/package.json` version: 1.3.1

### Acts 1-13 Status
- [x] Acts 1-11: Core validation fully operational
- [x] Acts 12-13: Registered in `ActRegistry.cs` and ready for opt-in use
- [x] Mental model reporter: Fixed and functional
- [x] Test coverage: `Act12and13Tests.cs` in place

### Documentation
- [x] CHANGELOG.md updated with v1.3.1 entry
- [x] PORTFOLIO_GAMEJAM_USAGE.md created (portfolio guide)
- [x] RELEASE-MATRIX.md verified (multi-target workflow)
- [x] RELEASE_v1.3.1.md (this file)

### Configuration
- [x] storytest-mental-model.json configured and ready
- [x] .npmignore excludes C# files from npm (Python-only PyPI package)
- [x] GitHub Actions workflow ready (publish-matrix.yml)

---

## 🚀 How to Release

### Option 1: GitHub Release (Recommended for Jam)

**Fastest method — releases to both PyPI and UPM automatically**

1. Go to your GitHub repo: https://github.com/jmeyer1980/TheStoryTest
2. Click **Releases** → **Create a new release**
3. Fill in:
   - **Tag**: `v1.3.1`
   - **Title**: `Story Test v1.3.1 - Acts 1-13 Complete`
   - **Description**: Copy from CHANGELOG.md v1.3.1 section
   - **Publish release**: Check the box

4. GitHub Actions automatically:
   - Publishes Python package to PyPI
   - Creates git tag for UPM (available via git URL)
   - Triggers post-release notifications

**Time**: 2 minutes to create + ~10 minutes for CI/CD

---

### Option 2: Manual Workflow Dispatch

**For testing or if GitHub Release doesn't work**

1. Go to **Actions** tab in GitHub
2. Select **📦 Multi-Target Release (PyPI + UPM)**
3. Click **Run workflow**
4. Fill in inputs:
   - **Version**: `1.3.1`
   - **Targets**: `pypi,upm` (leave default)
   - **Dry Run**: `true` (recommended first!)

5. Review the dry-run output
6. Run again with **Dry Run**: `false`

**Time**: 3 minutes setup + ~15 minutes CI/CD

---

## 📦 What Gets Published

### PyPI (Python Package)
- **Name**: `storytest`
- **URL**: https://pypi.org/project/storytest/
- **Install**: `pip install storytest==1.3.1`
- **Includes**:
  - `storytest/` Python module
  - CLI scripts (`story_test.py`, `story_test_unity_safe.py`)
  - Mental model reporter
  - Dependencies: pythonnet, clr-loader, colorama

### UPM (Unity Package Manager)
- **Name**: `com.tinywalnutgames.storytest`
- **URL**: `https://github.com/jmeyer1980/TheStoryTest.git#v1.3.1`
- **Install via UPM**:
  1. Window > Package Manager
  2. Add package from git URL
  3. Paste: `https://github.com/jmeyer1980/TheStoryTest.git#v1.3.1`
- **Includes**:
  - C# Acts 1-13 (Runtime/)
  - Editor integration (Editor/)
  - Tests (Tests/)
  - Documentation (Documentation~/)

---

## 🎮 Game Jam Portfolio Workflow

After v1.3.1 is released:

1. **Your Game Validation**:
   ```bash
   # Install latest Story Test
   pip install --upgrade storytest
   
   # Validate your game
   python -m storytest.cli validate /path/to/your/game --output validation-report.json
   ```

2. **Portfolio Artifact**:
   - Run validation on your game before submission
   - Save the JSON report: `validation-report.json`
   - Include in submission notes: "Game validated with Story Test v1.3.1"

3. **Portfolio Impact**:
   - Shows professional QA practices
   - Proves use of custom tooling
   - Demonstrates code quality discipline

---

## ✅ Post-Release Checklist

After release:

- [ ] Verify PyPI page updated: https://pypi.org/project/storytest/
- [ ] Verify git tag exists: `git tag` should show `v1.3.1`
- [ ] Test PyPI install: `pip install storytest==1.3.1`
- [ ] Test UPM URL works in Unity (add package from git URL)
- [ ] Update README.md to mention v1.3.1
- [ ] Announce in GitHub Discussions or Issues

---

## 🚨 Troubleshooting

### "PyPI upload failed"
- Check `secrets.PYPI_API_TOKEN` is set in GitHub Settings → Secrets
- Run locally: `python -m build && python -m twine check dist/*`

### "UPM not showing in Package Manager"
- Git tags are cached—may take a few minutes
- Try removing and re-adding the git URL
- Verify tag exists: `git ls-remote --tags origin v1.3.1`

### "Version mismatch error"
- All three files must have identical version strings (1.3.1)
- Run: `grep -r "1.3.1" package.json pyproject.toml Packages/*/package.json`

---

## 📝 Release Notes Template

For GitHub Release description:

```markdown
## Story Test v1.3.1 - Acts 1-13 Complete

### 🎯 What's New
- ✅ Acts 12-13 Registry Integration
- ✅ Full Mental Model Support
- ✅ Mental Model Reporter (HTML + JSON)
- ✅ Windows UTF-8 Encoding Fixed

### 🚀 Game Jam Ready
You can now use The Story Test Framework to validate your game before submission:

```bash
pip install --upgrade storytest
python scripts/story_test_unity_safe.py ./YourGame --verbose
```

### 📦 Installation
**PyPI**: `pip install storytest==1.3.1`
**UPM**: `https://github.com/jmeyer1980/TheStoryTest.git#v1.3.1`

### 📚 Documentation
- [Portfolio Usage Guide](./PORTFOLIO_GAMEJAM_USAGE.md)
- [Release Matrix](./docs/RELEASE-MATRIX.md)
- [Full Changelog](./CHANGELOG.md)

### ✨ Validation Status
- Acts 1-11: ✅ Production Ready
- Acts 12-13: ✅ Discoverable (opt-in)
- Test Coverage: ✅ Comprehensive
- Platform Support: ✅ Windows, Mac, Linux
```

---

## 🎉 Success Criteria

✅ v1.3.1 is released when:
1. Git tag `v1.3.1` exists
2. PyPI shows version 1.3.1 on https://pypi.org/project/storytest/
3. UPM URL works: `https://github.com/jmeyer1980/TheStoryTest.git#v1.3.1`
4. GitHub Release published with notes
5. CHANGELOG.md reflects v1.3.1 (already done)

---

**Ready to release?** Pick Option 1 or 2 above and go live! 🚀

**Questions?** Review:
- RELEASE-MATRIX.md (detailed instructions)
- .github/workflows/publish-matrix.yml (workflow code)
- GitHub Actions logs (if anything fails)