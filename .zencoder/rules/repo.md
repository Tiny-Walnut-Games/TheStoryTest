---
description: Repository Information Overview
alwaysApply: true
---

# The Story Test Framework Information

## Summary
The Story Test Framework is a code quality validation tool for Unity and .NET projects that enforces the "Story Test Doctrine": every symbol (method, property, parameter, enum) must be fully implemented and meaningful—no placeholders, TODOs, or unused code in production. It performs IL bytecode analysis to validate code quality through 11 validation acts.

## Structure
- **Packages/com.tinywalnutgames.storytest/**: Unity package with core validation logic
- **storytest/**: Python package for standalone validation
- **scripts/**: Utility scripts for validation and release management
- **docs/**: Documentation for installation and usage
- **.github/**: CI/CD workflows and GitHub configuration
- **ProjectSettings/**: Unity project configuration

## Projects

### Unity Package (com.tinywalnutgames.storytest)
**Configuration File**: Packages/com.tinywalnutgames.storytest/package.json

#### Language & Runtime
**Language**: C#
**Unity Version**: 2020.3.0f1+
**Build System**: Unity Package Manager
**Package Type**: Tool

#### Structure
- **Runtime/**: Core validation logic and 11 validation acts
- **Editor/**: Unity Editor integration
- **Tests/**: NUnit test suite
- **Documentation~/**: Unity package documentation

#### Dependencies
No external dependencies required for the Unity package.

#### Build & Installation
```bash
# Add to Unity Package Manager via git URL
https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest
```

#### Testing
**Framework**: NUnit (Unity Test Framework)
**Test Location**: Packages/com.tinywalnutgames.storytest/Tests/
**Run Command**: Via Unity Test Runner

### Python Package (storytest)
**Configuration File**: pyproject.toml

#### Language & Runtime
**Language**: Python
**Version**: 3.8+
**Build System**: setuptools
**Package Manager**: pip

#### Dependencies
**Main Dependencies**:
- pythonnet>=3.0.0
- clr-loader>=0.2.5
- colorama>=0.4.6

**Development Dependencies**:
- pytest>=7.0.0
- black>=23.0.0
- mypy>=1.0.0

#### Build & Installation
```bash
# Install from source
pip install -r requirements.txt
python setup.py install

# Install from PyPI
pip install storytest
```

#### Usage
```bash
# Run validation on a project
storytest validate /path/to/project --verbose

# Run validation on a specific assembly
storytest validate /path/to/assembly.dll --output report.json
```

### CLI Scripts
**Configuration File**: package.json (npm scripts)

#### Language & Runtime
**Language**: Python
**Version**: 3.8+

#### Usage & Operations
```bash
# Run validation with Unity-safe script
python scripts/story_test_unity_safe.py . --verbose

# Run validation and export report
python scripts/story_test.py . --verbose --output story-test-report.json

# Run validation in CI mode
python scripts/story_test.py . --fail-on-violations --output story-test-report.json
```

## CI/CD
**Workflow**: .github/workflows/story-test.yml
**Validation Command**:
```bash
python scripts/story_test_unity_safe.py . --fail-on-violations --output story-test-report.json
```

## Release Process
**Version Management**: 
- Version is maintained in package.json, pyproject.toml, and Packages/com.tinywalnutgames.storytest/package.json
- Release script: scripts/release.sh or scripts/release.ps1

## Zencoder Personal Guidelines

### Terminal Scripts with Emojis
⚠️ **Important**: When creating or modifying terminal/CLI scripts that use emojis (status indicators, checkmarks, etc.), **always add UTF-8 BOM encoding**.

#### For Python Scripts:
```python
#!/usr/bin/env python3
# -*- coding: utf-8-sig -*-
```
Add this as the first two lines of the file to ensure emojis display correctly in terminal output on all platforms.

#### For PowerShell Scripts:
Add UTF-8 BOM when saving (most editors have this option in "Save with Encoding").

**Reason**: Emojis require explicit UTF-8 encoding. Without BOM, some terminals (especially on Windows) may misinterpret the character encoding, causing display issues or script failures.