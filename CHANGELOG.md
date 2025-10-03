# Changelog

All notable changes to The Story Test Framework will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-10-03

### Added

- **Package-based distribution**: Converted from full Unity project to UPM package
- **9 Acts Validation System**: IL bytecode analysis for code quality
  - Act1: Todo Comments detection
  - Act2: Placeholder Implementations
  - Act3: Incomplete Classes
  - Act4: Unsealed Abstract Members
  - Act5: Debug Only Implementations
  - Act6: Phantom Props
  - Act7: Cold Methods
  - Act8: Hollow Enums
  - Act9: Premature Celebrations
- **Three-Tier Validation Architecture**:
  - Universal (Acts 1-9)
  - Conceptual (dynamic discovery)
  - Project-Specific (configurable)
- **Environment Detection**: Auto-detects Unity/DOTS/Burst capabilities
- **Cross-Platform Support**: Pure .NET, Unity GameObject, Unity ECS/DOTS, hybrid
- **Standalone Python Validator**: CLI tool for CI/CD pipelines
- **GitHub Actions Integration**: Cross-platform testing (Windows, macOS, Linux)
- **JSON Configuration System**: `StoryTestSettings.json` for project-agnostic setup
- **ConceptualValidator**: Dynamic validation utilities
- **ExtendedConceptualValidator**: Bridge between Shared and Main assemblies
- **StoryIgnoreAttribute**: Opt-out mechanism with required justification
- **Clean Assembly Architecture**: Shared → Acts → Main → Editor/Tests

### Changed

- **Repository Structure**: Moved from Unity project to package-based repo
- **Installation Method**: Now distributed via Unity Package Manager
- **Documentation**: Reorganized for package format
- **Assembly Definitions**: Updated with proper package references

### Fixed

- **Compilation Errors**: Resolved all C# compilation issues
- **Nullable Bool Operators**: Fixed operator precedence in LINQ expressions
- **Circular Dependencies**: Clean dependency flow between assemblies
- **GitHub Actions Matrix**: Fixed syntax to run on all 3 OS platforms
- **Unity Package Dependencies**: Removed incompatible packages for Unity 2022.3

### Removed

- **Hardcoded Project References**: Removed "Toxicity" project-specific code
- **DOTS/Burst Dependencies**: Made framework environment-agnostic
- **Incompatible Unity Packages**: Cleaned up manifest.json

### Security

- **IL Bytecode Analysis**: Framework uses reflection and IL analysis, no code generation
- **No External Dependencies**: Core framework is self-contained

## [Unreleased]

### Planned

- NuGet package distribution for pure .NET projects
- Scoped registry support
- HTML report generation
- Performance profiling integration
- Additional conceptual validation rules
- Sample Unity projects demonstrating usage

---

For migration guides and detailed changelogs for older versions, see the [full documentation](https://github.com/jmeyer1980/TheStoryTest).
