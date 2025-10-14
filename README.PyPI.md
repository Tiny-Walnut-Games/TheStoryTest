# The Story Test Framework

[![PyPI version](https://badge.fury.io/py/storytest.svg)](https://badge.fury.io/py/storytest)
[![Python 3.8+](https://img.shields.io/badge/python-3.8+-blue.svg)](https://www.python.org/downloads/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A comprehensive code quality validation framework for Unity and .NET projects that enforces **"Story Test Doctrine"**: every symbol (method, property, parameter, enum) must be fully implemented and meaningful—no placeholders, TODOs, or unused code in production.

## 🚀 Quick Start

### Installation

```bash
pip install storytest
```

### Basic Usage

```bash
# Validate a Unity project
storytest validate /path/to/unity/project

# Validate a single assembly
storytest validate MyAssembly.dll

# Validate with verbose output
storytest validate /path/to/project --verbose

# Generate JSON report
storytest validate /path/to/project --output report.json

# Fail CI/CD on violations
storytest validate /path/to/project --fail-on-violations
```

### Python API

```python
from storytest import StoryTestValidator

validator = StoryTestValidator(verbose=True)
violations = validator.validate_assembly("MyAssembly.dll")

for violation in violations:
    print(f"{violation.type_name}.{violation.member}: {violation.violation}")
```

## 📋 The 11 Acts

Each "Act" performs IL bytecode analysis to detect code quality issues:

1. **Act 1: Todo Comments** - Detects `NotImplementedException` and methods returning only defaults
2. **Act 2: Placeholder Implementations** - Catches stub methods with minimal IL (≤10 bytes)
3. **Act 3: Incomplete Classes** - Ensures non-abstract classes implement all abstract methods
4. **Act 4: Unsealed Abstract Members** - Prevents abstract methods in non-abstract classes
5. **Act 5: Debug Only Implementations** - Requires `[Obsolete]` on debug/test methods
6. **Act 6: Phantom Props** - Identifies auto-properties that are never meaningfully used
7. **Act 7: Cold Methods** - Finds empty or minimal methods (just `ret` instruction)
8. **Act 8: Hollow Enums** - Catches enums with ≤1 values or placeholder names
9. **Act 9: Premature Celebrations** - Detects code marked complete but still throwing `NotImplementedException`
10. **Act 10: Unused Parameters** - Detects method parameters that are never referenced
11. **Act 11: Empty Interfaces** - Identifies interfaces with no members (marker interfaces)

## 🎯 Features

### Unity-Safe Validation
- **No Unity installation required** - validates .NET assemblies directly
- **CI/CD friendly** - works in GitHub Actions, Azure DevOps, GitLab CI
- **Cross-platform** - Windows, macOS, Linux
- **Graceful fallback** - skips Unity-dependent assemblies automatically

### False Positive Filtering
- Smart detection of compiler-generated artifacts
- Unity lifecycle method filtering (Awake, Start, Update, etc.)
- Enum interface method exclusions
- Delegate and event handler artifact filtering

### Multiple Distribution Channels
- **PyPI** - `pip install storytest` (this package)
- **Unity UPM** - Git URL installation for Unity projects
- **GitHub Releases** - Manual download and installation

## 🔧 CI/CD Integration

### GitHub Actions

```yaml
name: Story Test Validation

on: [push, pull_request]

jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Set up Python
        uses: actions/setup-python@v4
        with:
          python-version: '3.11'
      
      - name: Install Story Test
        run: pip install storytest
      
      - name: Run Validation
        run: storytest validate . --fail-on-violations
```

### Azure DevOps

```yaml
- task: UsePythonVersion@0
  inputs:
    versionSpec: '3.11'

- script: |
    pip install storytest
    storytest validate $(Build.SourcesDirectory) --fail-on-violations
  displayName: 'Story Test Validation'
```

## 📚 Documentation

- [Full Documentation](https://github.com/jmeyer1980/TheStoryTest/tree/main/docs)
- [Getting Started Guide](https://github.com/jmeyer1980/TheStoryTest/blob/main/docs/getting-started.md)
- [The 11 Acts Explained](https://github.com/jmeyer1980/TheStoryTest/blob/main/docs/acts.md)
- [CI/CD Integration Guide](https://github.com/jmeyer1980/TheStoryTest/blob/main/docs/ci-cd.md)
- [Configuration Guide](https://github.com/jmeyer1980/TheStoryTest/blob/main/docs/configuration.md)

## 🤝 Unity Package

For Unity developers, you can also install via Unity Package Manager:

```
https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest
```

This provides:
- In-editor validation
- MonoBehaviour component for runtime validation
- Unity menu integration
- Visual Studio integration

## 📊 Requirements

- Python 3.8 or later
- .NET assemblies to validate
- pythonnet (automatically installed)

## 📝 License

MIT License - see [LICENSE](https://github.com/jmeyer1980/TheStoryTest/blob/main/LICENSE)

## 🙏 Support

- [GitHub Issues](https://github.com/jmeyer1980/TheStoryTest/issues)
- [GitHub Discussions](https://github.com/jmeyer1980/TheStoryTest/discussions)
- [Changelog](https://github.com/jmeyer1980/TheStoryTest/blob/main/CHANGELOG.md)

---

**Remember**: Every symbol must tell a story. No placeholders, no TODOs, no unused code in production.