# 🚀 v1.2.0 Release Checklist

## Phase 1: GitHub Release ✅ (Ready!)

- [x] Git tag `v1.2.0` created and pushed
- [x] CHANGELOG.md updated with v1.2.0 section
- [x] Package version 1.2.0 in package.json
- [x] All files synchronized
- [ ] **TODO:** Create GitHub Release at https://github.com/jmeyer1980/TheStoryTest/releases/new

## Phase 2: PyPI Package 🆕 (Ready to Test!)

### Prerequisites
- [ ] Create PyPI account: https://pypi.org/account/register/
- [ ] Create TestPyPI account: https://test.pypi.org/account/register/
- [ ] Generate PyPI API token: https://pypi.org/manage/account/token/
- [ ] Generate TestPyPI API token: https://test.pypi.org/manage/account/token/
- [ ] Configure `~/.pypirc` with tokens (see `docs/PYPI_PUBLISHING.md`)

### Local Testing
- [x] Package structure created (`storytest/` folder)
- [x] Package imports successfully (`test_package.py` passes)
- [ ] **TODO:** Install locally: `pip install -e .`
- [ ] **TODO:** Test CLI: `storytest version`
- [ ] **TODO:** Test CLI: `storytest validate --help`

### TestPyPI (Staging)
- [ ] **TODO:** Install build tools: `pip install --upgrade build twine`
- [ ] **TODO:** Build package: `.\scripts\build_pypi.ps1`
- [ ] **TODO:** Upload to TestPyPI: `.\scripts\build_pypi.ps1 -Test`
- [ ] **TODO:** Install from TestPyPI: `pip install --index-url https://test.pypi.org/simple/ storytest`
- [ ] **TODO:** Verify TestPyPI installation works

### Production PyPI
- [ ] **TODO:** Publish to PyPI: `.\scripts\build_pypi.ps1 -Publish`
- [ ] **TODO:** Verify on PyPI: https://pypi.org/project/storytest/
- [ ] **TODO:** Test installation: `pip install storytest`
- [ ] **TODO:** Test CLI works after pip install

## Phase 3: OpenUPM (Future)

- [ ] Research OpenUPM submission process
- [ ] Create OpenUPM package manifest
- [ ] Submit to OpenUPM registry
- [ ] Verify OpenUPM installation

## Phase 4: Documentation Updates

### After GitHub Release
- [ ] Update main README.md with release badge
- [ ] Update installation instructions
- [ ] Announce in GitHub Discussions

### After PyPI Release
- [ ] Add PyPI badge to README.md
- [ ] Update GitHub Release notes to include PyPI installation
- [ ] Update docs/getting-started.md with pip installation
- [ ] Update docs/ci-cd.md with pip examples

## Phase 5: Community Announcements

- [ ] Post in GitHub Discussions
- [ ] Update Unity Asset Store listing (if applicable)
- [ ] Social media announcement (optional)
- [ ] Blog post (optional)

---

## 📝 Quick Commands Reference

### GitHub Release
```
1. Go to: https://github.com/jmeyer1980/TheStoryTest/releases/new
2. Choose tag: v1.2.0
3. Title: v1.2.0 - Unity-Safe Validator & Documentation Restructure
4. Copy description from previous conversation
5. Publish release
```

### PyPI Testing
```powershell
# Local test
pip install -e .
storytest version

# Build
.\scripts\build_pypi.ps1

# Test on TestPyPI
.\scripts\build_pypi.ps1 -Test

# Publish to PyPI
.\scripts\build_pypi.ps1 -Publish
```

### Verification
```bash
# After PyPI publish
pip install storytest
storytest version
storytest validate --help
```

---

## 🎯 Current Status

✅ **GitHub Release:** Ready to publish  
🆕 **PyPI Package:** Ready to test locally  
⏳ **OpenUPM:** Future enhancement  

**Next Action:** Choose your path:
1. **GitHub Release First** → Create release on GitHub
2. **PyPI Testing First** → Test package locally with `pip install -e .`
3. **Both Together** → Do GitHub release, then PyPI

---

## 📚 Documentation Created

- ✅ `PYPI_READY.md` - Package verification and quick start
- ✅ `PYPI_PACKAGE_SUMMARY.md` - Complete package overview
- ✅ `docs/PYPI_PUBLISHING.md` - Detailed publishing guide
- ✅ `README.PyPI.md` - PyPI-specific README
- ✅ `scripts/build_pypi.ps1` - Automated build script
- ✅ `test_package.py` - Package verification script

---

## ⚠️ Important Notes

1. **PyPI versions are permanent** - You cannot delete or re-upload the same version
2. **Test on TestPyPI first** - Always test before publishing to real PyPI
3. **API tokens are sensitive** - Never commit `.pypirc` to git
4. **Version numbers must increment** - If you mess up, bump to 1.2.1

---

**Ready to proceed?** Start with local testing:
```powershell
pip install -e .
storytest version
```