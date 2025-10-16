# Changelog

All notable changes to The Story Test Framework will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]


## [1.2.1] - 2025-10-16


- Bump version to 1.2.1 (a9168ea)
- Update build_pypi.ps1 for CLI options and cleanup, chaning file encoding to UTF-16BE for emoji support. Performance hit is minimal. (ff05e31)
- Add new project files for TheStoryTest suite (c517031)
- Revert "Fix PowerShell syntax and encoding issues in build script. Linux workflow was failing upon lack of dlls to test. This should be more graceful now." (eeb320c)
- security: add PyPI publishing with GitHub secrets (no hardcoded tokens) (5417748)
- Update JSON chat metadata with timestamp and checksum (bbaad95)



## [1.2.1] - 2025-10-16


- Bump version to 1.2.1 (a9168ea)
- Update build_pypi.ps1 for CLI options and cleanup, chaning file encoding to UTF-16BE for emoji support. Performance hit is minimal. (ff05e31)
- Add new project files for TheStoryTest suite (c517031)
- Revert "Fix PowerShell syntax and encoding issues in build script. Linux workflow was failing upon lack of dlls to test. This should be more graceful now." (eeb320c)
- security: add PyPI publishing with GitHub secrets (no hardcoded tokens) (5417748)
- Update JSON chat metadata with timestamp and checksum (bbaad95)


## [1.3.0] - 2025-10-16

### Added
- Acts 12-13 Integration
  - Act 12 (Act12MentalModelClaims.cs) - Validates claims have evidence
  - Act 13 (Act13NarrativeCoherence.cs) - Validates architecture coherence
  - Reporter (mental_model_reporter.py) - Dynamic analysis & reporting
- Configuration-Driven Validation
  - storytest-mental-model.json for project narrative definition
  - Supports: claimed capabilities, required artifacts, architectural rules, quality gates
- Multi-Output Reporting
  - JSON for automation
  - HTML for visualization
  - Exit codes for CI/CD flow control

## [1.2.1] - 2025-10-15

### Fixed
- CI/CD workflow Unity version mismatch (updated from 2022.3.17f1 to 6000.0.30f1 to match project version)
- False positive violation for `IEnumerator.Reset()` in compiler-generated iterator state machines
- Enhanced compiler-generated member detection in both C# and Python validators to skip explicit interface implementations in state machines
- Python validation script (`story_test.py`) now properly filters compiler-generated iterator/async state machine members

### Changed
- Improved release script to update all three version files atomically (Unity package.json, root package.json, pyproject.toml)

## [1.2.0] - 2025-10-14

### Added
- Unity-safe Python validator (`story_test_unity_safe.py`) for standalone validation
- Reality anchor system (`REALITY_CHECK.md`) for accurate project status tracking
- False positive filtering for compiler-generated artifacts (Roslyn, Unity lifecycle)
- Anti-false-celebration documentation practices for AI assistants

### Fixed
- Python validator Unity dependency crashes (`UnityEngine.CoreModule` loading failures)
- 30 false positives from enum interface methods and delegate artifacts
- Configuration path issues in `StoryTestSettings.json`
- Documentation drift (9 Acts vs actual 11 Acts)

### Improved
- CI/CD pipeline with Linux-first canonical builds
- Cross-platform Python validation without Unity installation requirements
- Assembly loading with graceful fallback for Unity-dependent assemblies

## [1.1.0] - 2025-10-07

### Added
- Complete GitHub Actions workflow with Linux canonical builds
- Unity Playmode test integration with sync-point performance testing
- Python CoreCLR runtime support for better cross-platform compatibility
- Automated assembly discovery and validation

### Fixed
- Platform identifier errors (Linux64 → StandaloneLinux64)
- Action version deprecation warnings (migrated to @v4/@v5 tags)
- Unity assembly loading in standalone Python validator
- Linter configuration for GitHub Actions validation

### Changed
- Migrated from commit SHAs to major version tags for GitHub Actions
- Implemented cost-optimized CI/CD (Linux-first, manual Windows/macOS)
- Enhanced error handling for Unity dependency resolution

## [1.0.0] - 2025-09-30

### Added
- Initial release of Story Test Framework
- 11 validation Acts for IL bytecode analysis
- Unity Editor integration with menu system
- ProductionExcellenceStoryTest MonoBehaviour
- StoryIntegrityValidator with automatic rule discovery
- Python standalone validator with .NET reflection
- Comprehensive test suite with NUnit integration

### Features
- **Act 1**: Todo Comments (NotImplementedException detection)
- **Act 2**: Placeholder Implementations (minimal IL detection)
- **Act 3**: Incomplete Classes (abstract method implementation)
- **Act 4**: Unsealed Abstract Members (abstract method sealing)
- **Act 5**: Debug Only Implementations ([Obsolete] requirement)
- **Act 6**: Phantom Props (unused auto-properties)
- **Act 7**: Cold Methods (empty/minimal methods)
- **Act 8**: Hollow Enums (minimal enum values)
- **Act 9**: Premature Celebrations (complete but throwing)
- **Act 10**: Suspiciously Simple Methods (constant returns)
- **Act 11**: Dead Code (unused fields/properties/methods)

### Architecture
- Multi-assembly structure with clear separation of concerns
- Unity-agnostic core validation logic
- Conditional compilation for cross-platform compatibility
- StoryIgnoreAttribute for intentional exclusions
- Dynamic rule registration via reflection

## Development History Archive

### Phase 4: Production Readiness (2025-10)
- Focus on human handoff preparation
- Documentation consolidation and versioning
- Python validator production hardening
- False positive elimination

### Phase 3.5: Runtime Restoration (2025-09)
- Restored Unity runtime integration
- Fixed assembly loading issues
- Enhanced sync-point performance testing
- Improved error handling and reporting

### Phase 2.5: Workflow Fixes (2025-09)
- Resolved GitHub Actions platform issues
- Fixed Unity build configuration
- Optimized CI/CD performance
- Enhanced cross-platform compatibility

### Phase 2: Package Conversion (2025-08)
- Migrated to Unity Package format
- Implemented proper assembly structure
- Added comprehensive documentation
- Established CI/CD pipeline

### Phase 1: Foundation (2025-07)
- Initial framework development
- Core validation logic implementation
- Unity Editor integration
- Python validator creation

---

## Version Scheme

This project follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html):

- **MAJOR**: Breaking changes that require user intervention
- **MINOR**: New features, improvements, non-breaking changes
- **PATCH**: Bug fixes, documentation updates, performance improvements

### Release Cadence
- **Major releases**: Significant architectural changes or breaking API modifications
- **Minor releases**: New validation Acts, feature enhancements, platform support
- **Patch releases**: Bug fixes, false positive elimination, documentation updates

### Compatibility Guarantees
- **Backward compatibility**: Maintained within major versions
- **Forward compatibility**: Best effort, but not guaranteed
- **API stability**: Public interfaces stable within major versions
- **Configuration stability**: Settings format stable within major versions
