# Publishing to PyPI

This guide explains how to publish The Story Test Framework to PyPI (Python Package Index).

## 📋 Prerequisites

### 1. PyPI Account
- Create account at https://pypi.org/account/register/
- Create account at https://test.pypi.org/account/register/ (for testing)

### 2. API Tokens
- PyPI: https://pypi.org/manage/account/token/
- TestPyPI: https://test.pypi.org/manage/account/token/

Create tokens with "Entire account" scope (you can narrow this later).

### 3. Configure ~/.pypirc

Copy `.pypirc.example` to `~/.pypirc` and add your tokens:

```ini
[distutils]
index-servers =
    pypi
    testpypi

[pypi]
username = __token__
password = pypi-YOUR_REAL_TOKEN_HERE

[testpypi]
repository = https://test.pypi.org/legacy/
username = __token__
password = pypi-YOUR_TEST_TOKEN_HERE
```

**Security Note:** Keep this file private! Add to `.gitignore`.

## 🔨 Building the Package

### Local Development Install

```powershell
# Install in editable mode for development
pip install -e .

# Test the CLI
storytest version
storytest validate --help
```

### Build Distribution

```powershell
# Build wheel and source distribution
.\scripts\build_pypi.ps1

# Or manually:
python -m build
```

This creates:
- `dist/storytest-1.2.0-py3-none-any.whl` (wheel)
- `dist/storytest-1.2.0.tar.gz` (source)

## 🧪 Testing on TestPyPI

### Upload to TestPyPI

```powershell
.\scripts\build_pypi.ps1 -Test

# Or manually:
python -m twine upload --repository testpypi dist/*
```

### Install from TestPyPI

```bash
# Create a test environment
python -m venv test_env
source test_env/bin/activate  # Linux/Mac
# or
test_env\Scripts\activate  # Windows

# Install from TestPyPI
pip install --index-url https://test.pypi.org/simple/ storytest

# Test it
storytest version
storytest validate /path/to/project
```

### Verify Installation

```python
# Test Python API
from storytest import StoryTestValidator, __version__
print(f"Story Test v{__version__}")

validator = StoryTestValidator(verbose=True)
print("✓ Package imported successfully")
```

## 🚀 Publishing to PyPI

### Pre-Release Checklist

- [ ] Version number updated in `pyproject.toml`
- [ ] Version number updated in `storytest/__init__.py`
- [ ] CHANGELOG.md updated with release notes
- [ ] All tests passing
- [ ] Documentation updated
- [ ] README.PyPI.md reviewed
- [ ] Git tag created (`v1.2.0`)
- [ ] Tested on TestPyPI

### Publish to PyPI

```powershell
# Build and publish
.\scripts\build_pypi.ps1 -Publish

# Or manually:
python -m twine upload dist/*
```

### Verify Publication

```bash
# Wait a few minutes, then:
pip install storytest

# Test
storytest version
```

Visit: https://pypi.org/project/storytest/

## 📦 Package Structure

```
TheStoryTest/
├── storytest/              # Python package
│   ├── __init__.py        # Package entry point
│   ├── validator.py       # Core validation logic
│   └── cli.py             # Command-line interface
├── pyproject.toml         # Modern Python packaging config
├── setup.py               # Backward compatibility
├── MANIFEST.in            # Include/exclude files
├── README.PyPI.md         # PyPI-specific README
└── scripts/
    └── build_pypi.ps1     # Build automation script
```

## 🔄 Release Process

### 1. Update Version

**pyproject.toml:**
```toml
[project]
version = "1.2.0"
```

**storytest/__init__.py:**
```python
__version__ = "1.2.0"
```

### 2. Update CHANGELOG.md

```markdown
## [1.2.0] - 2025-10-14

### Added
- PyPI package distribution
- CLI command: `storytest validate`
- Python API for programmatic validation
```

### 3. Commit and Tag

```powershell
git add .
git commit -m "chore: prepare v1.2.0 PyPI release"
git tag -a v1.2.0 -m "Release v1.2.0 - PyPI package"
git push origin main
git push origin v1.2.0
```

### 4. Build and Test

```powershell
.\scripts\build_pypi.ps1 -Clean -Test
```

### 5. Publish

```powershell
.\scripts\build_pypi.ps1 -Publish
```

### 6. Create GitHub Release

- Go to: https://github.com/jmeyer1980/TheStoryTest/releases/new
- Choose tag: `v1.2.0`
- Add release notes
- Mention PyPI availability: `pip install storytest`

## 🐛 Troubleshooting

### "File already exists" Error

PyPI doesn't allow re-uploading the same version. You must:
1. Increment version number
2. Build new distribution
3. Upload new version

### Import Errors

Check package structure:
```powershell
python -m zipfile -l dist/storytest-1.2.0-py3-none-any.whl
```

Ensure `storytest/__init__.py` exists and imports correctly.

### Missing Dependencies

Verify `pyproject.toml` dependencies match `requirements.txt`:
```toml
dependencies = [
    "pythonnet>=3.0.0",
    "clr-loader>=0.2.5",
    "colorama>=0.4.6",
]
```

### CLI Command Not Found

After `pip install storytest`, the `storytest` command should be available.

Check:
```powershell
pip show storytest
which storytest  # Linux/Mac
where storytest  # Windows
```

If missing, verify `pyproject.toml`:
```toml
[project.scripts]
storytest = "storytest.cli:main"
```

## 📊 Post-Release

### Monitor Downloads

- PyPI Stats: https://pypistats.org/packages/storytest
- GitHub Insights: https://github.com/jmeyer1980/TheStoryTest/pulse

### Update Documentation

- Add PyPI badge to README.md
- Update installation instructions
- Announce on GitHub Discussions

### Community

- Monitor GitHub Issues for bug reports
- Respond to PyPI project comments
- Update documentation based on feedback

## 🔐 Security

### API Token Security

- **Never commit** `.pypirc` to git
- Use **scoped tokens** (limit to specific project)
- **Rotate tokens** periodically
- Store in **password manager**

### Package Security

- Run `pip-audit` to check dependencies
- Keep `pythonnet` and `clr-loader` updated
- Monitor security advisories

## 📚 Resources

- [Python Packaging Guide](https://packaging.python.org/)
- [PyPI Help](https://pypi.org/help/)
- [Twine Documentation](https://twine.readthedocs.io/)
- [PEP 517 - Build System](https://peps.python.org/pep-0517/)
- [PEP 518 - pyproject.toml](https://peps.python.org/pep-0518/)

---

**Next Steps:**
1. Test locally: `pip install -e .`
2. Upload to TestPyPI: `.\scripts\build_pypi.ps1 -Test`
3. Verify installation from TestPyPI
4. Publish to PyPI: `.\scripts\build_pypi.ps1 -Publish`
5. Create GitHub Release with PyPI link