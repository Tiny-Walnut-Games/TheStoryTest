# TheStoryTest Package Conversion Plan

**Issue:** #2  
**Branch:** jmeyer1980/issue2  
**Status:** In Progress  
**Date:** October 3, 2025

## Overview

Converting TheStoryTest from a full Unity project repository to a package-based repository for easier distribution and integration into existing projects.

## Current Structure Analysis

### What We Have Now (Unity Project Repo)

```ts
TheStoryTest/
├── Assets/
│   └── Tiny Walnut Games/
│       └── TheStoryTest/
│           ├── Runtime/ (Core framework code)
│           ├── Editor/ (Unity Editor integrations)
│           └── Tests/ (NUnit tests)
├── ProjectSettings/ (Unity project settings)
├── Packages/ (Unity package dependencies)
├── Library/ (Unity build artifacts)
└── story_test.py (Standalone Python validator)
```

### What We Want (Package-Based Repo)

```ts
TheStoryTest/
├── Packages/
│   └── com.tinywalnutgames.storytest/
│       ├── package.json (UPM manifest)
│       ├── Runtime/
│       ├── Editor/
│       ├── Tests/
│       └── Documentation~/
├── samples/ (Example Unity projects demonstrating usage)
├── docs/ (Package documentation)
├── .github/workflows/ (CI/CD for package testing)
└── story_test.py (Standalone validator)
```

## Conversion Strategy

### Phase 1: Package Structure Setup ✅

1. Create `Packages/com.tinywalnutgames.storytest/` directory
2. Move core framework from `Assets/Tiny Walnut Games/TheStoryTest/` to package
3. Create proper `package.json` manifest
4. Update all assembly definitions (.asmdef) with package references

### Phase 2: Unity Project Cleanup

1. Remove unnecessary Unity project files (ProjectSettings, Library, Temp)
2. Keep minimal Unity project for testing in `samples/`
3. Update .gitignore for package-based structure
4. Clean up root-level Unity-specific files

### Phase 3: Documentation & Samples

1. Create `Documentation~/` folder with package docs
2. Move existing markdown docs to `docs/` folder
3. Create sample Unity project demonstrating package usage
4. Update README.md for package installation instructions

### Phase 4: CI/CD Pipeline Updates

1. Update GitHub Actions workflows for package testing
2. Add UPM package validation
3. Keep cross-platform testing (Windows, macOS, Linux)
4. Add automatic package versioning

### Phase 5: Distribution Preparation

1. Create package release workflow
2. Set up UPM registry integration (GitHub Packages or OpenUPM)
3. Document installation methods (git URL, scoped registry, manual)
4. Create CHANGELOG.md for versioning

## File Migration Map

### Core Framework Files (Move to Package)

```ts
Assets/Tiny Walnut Games/TheStoryTest/
→ Packages/com.tinywalnutgames.storytest/
```

### Assembly Definitions

- Keep existing .asmdef structure
- Update root namespace to package namespace
- Ensure proper package references

### Tests

```ts
Assets/Tiny Walnut Games/TheStoryTest/Tests/
→ Packages/com.tinywalnutgames.storytest/Tests/
```

### Documentation

```ts
ASSEMBLY_STRUCTURE.md → docs/assembly-structure.md
DYNAMIC_VALIDATION.md → docs/dynamic-validation.md
QUICKSTART.md → Documentation~/QuickStart.md
README.md → Updated for package usage
```

### Standalone Components

```ts
story_test.py → Keep at root (standalone CLI tool)
requirements.txt → Keep at root
```

## Package.json Configuration

```json
{
  "name": "com.tinywalnutgames.storytest",
  "version": "1.0.0",
  "displayName": "The Story Test Framework",
  "description": "A comprehensive code quality validation framework for Unity and .NET projects that enforces Story Test Doctrine: every symbol must be fully implemented and meaningful—no placeholders, TODOs, or unused code in production.",
  "unity": "2020.3",
  "keywords": [
    "testing",
    "validation",
    "code-quality",
    "ecs",
    "dots",
    "bytecode-analysis"
  ],
  "author": {
    "name": "Tiny Walnut Games",
    "url": "https://github.com/jmeyer1980/TheStoryTest"
  },
  "dependencies": {},
  "samples": [
    {
      "displayName": "Story Test Example",
      "description": "Example Unity project showing Story Test integration",
      "path": "Samples~/ExampleProject"
    }
  ]
}
```

## Benefits of This Approach

### For Package Users

- ✅ Easy installation via Unity Package Manager
- ✅ No project pollution (framework isolated in Packages folder)
- ✅ Clear versioning and updates
- ✅ Works with existing Unity projects without conflicts

### For Maintainers

- ✅ Cleaner repository structure
- ✅ Easier to test in isolation
- ✅ Standard UPM distribution
- ✅ Better separation of concerns

### For CI/CD

- ✅ Faster builds (no full Unity project compilation)
- ✅ Package-specific testing
- ✅ Automated package validation
- ✅ Easy version management

## Migration Checklist

### Immediate Tasks

- [ ] Create package directory structure
- [ ] Move Runtime/ folder to package
- [ ] Move Editor/ folder to package
- [ ] Move Tests/ folder to package
- [ ] Create package.json manifest
- [ ] Update assembly definitions
- [ ] Create sample Unity project

### Documentation Tasks

- [ ] Update README.md with UPM installation instructions
- [ ] Create Documentation~/ folder
- [ ] Move technical docs to docs/ folder
- [ ] Write package usage guide
- [ ] Document installation methods (git URL, tarball, scoped registry)

### Testing Tasks

- [ ] Verify package compiles in clean Unity project
- [ ] Test all 9 Acts validation rules
- [ ] Test conceptual validation
- [ ] Test across Unity versions (2020.3+)
- [ ] Verify standalone Python validator still works

### CI/CD Tasks

- [ ] Update workflow to test package installation
- [ ] Add package validation step
- [ ] Create package release workflow
- [ ] Set up automatic versioning
- [ ] Configure package publishing

### Cleanup Tasks

- [ ] Remove ProjectSettings/ (move to sample)
- [ ] Remove Library/ and Temp/
- [ ] Update .gitignore
- [ ] Remove root-level Unity .csproj files
- [ ] Clean up workspace structure

## Installation Methods (Post-Conversion)

### Method 1: Git URL (Recommended for Development)

```json
{
  "dependencies": {
    "com.tinywalnutgames.storytest": "https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest"
  }
}
```

### Method 2: Manual Installation

1. Download package from Releases
2. Extract to `Packages/com.tinywalnutgames.storytest/`
3. Unity will auto-detect and import

### Method 3: Scoped Registry (Future)

```json
{
  "scopedRegistries": [
    {
      "name": "Tiny Walnut Games",
      "url": "https://npm.pkg.github.com/@tinywalnutgames",
      "scopes": ["com.tinywalnutgames"]
    }
  ]
}
```

## Breaking Changes

### For Existing Users

- Menu path remains: `Tiny Walnut Games/The Story Test/`
- Assembly names unchanged
- API remains compatible
- Settings file location unchanged (Resources/)

### Migration Guide for Existing Users

1. Remove `Assets/Tiny Walnut Games/TheStoryTest/` folder
2. Install package via UPM
3. Settings automatically migrate (uses Resources/)
4. No code changes required

## Timeline

- **Week 1**: Phase 1 - Package structure setup
- **Week 2**: Phase 2 & 3 - Cleanup and documentation
- **Week 3**: Phase 4 & 5 - CI/CD and distribution
- **Week 4**: Testing and release

## Success Criteria

- ✅ Package installs cleanly in empty Unity project
- ✅ All validation rules work identically to current version
- ✅ CI/CD pipeline passes on all platforms
- ✅ Documentation is clear and comprehensive
- ✅ No breaking changes for existing users
- ✅ Standalone Python validator continues to work

## Notes

- Keep backward compatibility with existing Unity projects
- Maintain all current functionality (9 Acts + Conceptual validation)
- Preserve cross-platform support (Unity + standalone .NET)
- Ensure GitHub Actions workflows continue to work
- Python validator (`story_test.py`) remains standalone tool

---

**Next Actions:** Begin Phase 1 - Create package directory structure and move core files.
