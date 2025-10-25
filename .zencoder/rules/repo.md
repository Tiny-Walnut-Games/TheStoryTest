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

---

# GAME JAM RELEASE DIRECTIVES (v1.3.0)

## Implementation Status (2025-10-24)

### ✅ PRODUCTION READY (Acts 1-11)
- **C# Acts 1-11**: Fully implemented in Runtime/Acts/
- **Python Validator**: Implements all 11 acts via pythonnet reflection
- **CLI Interface**: Both `story_test.py` and `story_test_unity_safe.py` functional
- **CI/CD Pipeline**: GitHub Actions workflow validates on every commit
- **Documentation**: Acts 1-11 behavior documented in docs/acts.md

**Status**: Use Acts 1-11 validation as-is. No changes needed.

### 🚧 INCOMPLETE (Acts 12-13)
- **Act 12 (Mental Model Claims)**: C# file exists but never invoked by validator
- **Act 13 (Narrative Coherence)**: C# file exists but never invoked by validator
- **Python Reporter**: Has unresolved variables (line 186, 195) causing runtime crash
- **Integration**: Acts 12-13 not registered in validation pipeline

**Status**: Acts 12-13 are **NOT ready for game jam**. Scope decision: see below.

---

## Game Jam Ready Scope

### MANDATORY for Release
1. **Acts 1-11 validation works end-to-end**
   - Run: `python scripts/story_test_unity_safe.py . --verbose`
   - Must complete without errors and report violations correctly
   - All existing tests must pass

2. **Python package installs cleanly**
   - `pip install -e .` from project root works
   - No import errors when running CLI

3. **Documentation matches implementation**
   - Remove claims about Acts 12-13 being "complete" or "integrated"
   - Update CHANGELOG.md to reflect actual status

4. **Version consistency**
   - package.json, pyproject.toml, Packages/com.tinywalnutgames.storytest/package.json all match (v1.3.0)

### OPTIONAL (Post Game Jam)
- Acts 12-13 full integration
- Mental model reporting CLI command
- Schema validation for storytest-mental-model.json

**Decision**: Deploy v1.3.0 as "Acts 1-11 Enhanced Release" focusing on stability over new features.

---

## Blocking Issues & Required Fixes

### CRITICAL (Must Fix Before Release)

#### Issue 1: mental_model_reporter.py Line 186
**Problem**: Undefined parameter `artifacts_total` passed to function that doesn't accept it
```python
# Line 186 - BREAKS
claims_verified, claims_total, claim_gaps = verify_claims(
    config, artifacts_found, artifacts_total=len(config.get("required_artifacts", []))
)
# verify_claims() signature doesn't have artifacts_total parameter
```
**Fix**: Remove the parameter or update verify_claims() signature to accept it
**Time**: 2 minutes

#### Issue 2: mental_model_reporter.py Line 195
**Problem**: Logic error comparing list type incorrectly
```python
# Line 195 - INCORRECT LOGIC
if gates_passed == gates_total and claim_gaps == []:
    status = "COMPLETE"
```
**Fix**: Check if list is empty with `len(claim_gaps) == 0` or `not claim_gaps`
**Time**: 1 minute

#### Issue 3: Acts 12-13 Documentation Claims
**Problem**: MENTAL_MODEL_INTEGRATION.md and CHANGELOG.md claim Acts 12-13 are "fully integrated" and "complete"
**Files to Update**:
- MENTAL_MODEL_INTEGRATION.md (line 1-30): Mark as "DRAFT" or "IN PROGRESS"
- CHANGELOG.md (v1.3.0): Remove Acts 12-13 from release notes or mark as "experimental"
- docs/mental-model-validation.md: Add disclaimer that feature is not yet integrated

**Fix**: Change celebratory language to honest status
**Time**: 10 minutes

#### Issue 4: Act12MentalModelClaims.cs Never Called
**Problem**: Act12MentalModelClaims.cs exists but validator never registers or invokes it
**Current State**: ValidateMentalModelClaims() returns false for all non-null members (never validates anything)
**Scope Decision**: Leave as-is for game jam. Document in Acts12/13 that they require manual assembly registration.
**Time**: N/A (defer to v1.4.0)

---

## Pre-Release Validation Checklist

Before packaging v1.3.0 for game jam, verify ALL of these pass:

### Code Quality
- [ ] Run: `python scripts/story_test_unity_safe.py . --fail-on-violations --output story-test-report.json`
- [ ] Exit code is 0 (no violations found in the framework itself)
- [ ] story-test-report.json is valid JSON with no violations in storytest/ and Packages/

### Python Package
- [ ] `pip install -e .` succeeds without warnings
- [ ] `storytest --version` returns 1.3.0
- [ ] `storytest validate --help` runs without import errors
- [ ] No undefined variable errors in mental_model_reporter.py (fixes 1-2 applied)

### Documentation Accuracy
- [ ] CHANGELOG.md v1.3.0 does NOT claim Acts 12-13 are "fully integrated"
- [ ] MENTAL_MODEL_INTEGRATION.md clearly states Acts 12-13 are "planned" or "draft"
- [ ] README.md mentions v1.3.0 with accurate feature list (Acts 1-11 only)

### Version Consistency
- [ ] package.json version = "1.3.0"
- [ ] pyproject.toml version = "1.3.0"
- [ ] Packages/com.tinywalnutgames.storytest/package.json version = "1.3.0"
- [ ] .github/workflows/story-test.yml references correct version if hardcoded

### C# Tests (Unity Package)
- [ ] Run Unity Test Runner on Packages/com.tinywalnutgames.storytest/Tests/
- [ ] All tests pass (or document known failures)
- [ ] Acts 1-11 test coverage is current

### CI/CD Green
- [ ] GitHub Actions workflow passes on latest commit
- [ ] No false positives (workflow should only fail if real violations exist)

---

## Coding Rules for Acts 1-13 Framework

### The Story Test Doctrine (Applies to Framework Itself)
Every symbol in the framework must:
1. **Be Fully Implemented** - No TODO, FIXME, or stub methods
2. **Have a Single Clear Purpose** - Class/method name must match intent
3. **Pass Its Own Validation** - `storytest validate` must report zero violations in storytest/ and Packages/
4. **Be Documented** - Docstrings for public methods; inline comments for non-obvious logic

### Acts 12-13 Specific Rules (When Ready for Integration)
- Act 12 (Mental Model Claims): Must verify that `storytest-mental-model.json` claims have concrete code evidence
- Act 13 (Narrative Coherence): Must validate that architecture described in README.md aligns with actual folder structure

For now: **Mark these as "Phase 2" in code comments until integration is complete.**

### File Organization Rules
- Acts go in: `Packages/com.tinywalnutgames.storytest/Runtime/Acts/Act{N}{Name}.cs`
- Python validators go in: `storytest/` with clear module names
- Config files go in: repo root (e.g., `storytest-mental-model.json`)
- Tests go in: same folder structure with `.Tests` suffix

---

## Quick Reference: What's Safe to Use Right Now

### ✅ Use in Your Game
```bash
# Validate your game's C# code meets the Story Test Doctrine
python scripts/story_test_unity_safe.py /path/to/your/game --verbose
```

### ✅ Use in Automation
```bash
# CI/CD: Catch violations before build
python scripts/story_test.py . --fail-on-violations --output report.json
```

### ❌ DO NOT USE YET (Will Crash)
- Acts 12-13 (not integrated)
- Mental model reporting CLI (Python errors in reporter)
- storytest-mental-model.json validation (schema not completed)

---

## Known Limitations (Game Jam v1.3.0)

1. **Acts 12-13 Exist But Don't Run**: C# Act files exist but aren't called during validation
2. **Mental Model Reporter Has Bugs**: Python reporter crashes on runtime variable errors
3. **No Mental Model CLI Command**: Can't run `storytest mental-model` yet
4. **Path Assumptions in Acts 12-13**: They assume relative paths that may not work from all directories

**These will be fixed in v1.4.0 post-game jam.**

---

## Next Steps (After Game Jam)

### v1.4.0 Roadmap
1. Fix Acts 12-13 to integrate into validation pipeline
2. Repair mental_model_reporter.py and wire it as CLI command
3. Add schema validation for storytest-mental-model.json
4. Add end-to-end tests for Acts 12-13
5. Update release checklist for multi-act releases

### Testing Acts 12-13 (When Ready)
```bash
# Will work after v1.4.0:
storytest validate . --mental-model --output report.json
```