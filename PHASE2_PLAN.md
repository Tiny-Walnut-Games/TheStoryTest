# Phase 2: Unity Project Cleanup - Execution Plan

**Issue:** #2  
**Branch:** `jmeyer1980/issue2`  
**Date:** October 3, 2025  
**Status:** 🚧 In Progress

## Overview

Now that the UPM package is created in `Packages/com.tinywalnutgames.storytest/`, we need to clean up the Unity project structure and reorganize it as a **sample/demo** project that demonstrates package usage.

## Current State vs Target State

### Current Structure (Unity Project Repo)

```ts
TheStoryTest/
├── Assets/
│   ├── Tiny Walnut Games/TheStoryTest/ (❌ Duplicate - now in Packages/)
│   ├── Scenes/ (Keep for sample)
│   ├── Settings/ (Keep for sample)
│   └── InputSystem_Actions.inputactions (Keep for sample)
├── ProjectSettings/ (❌ Unity project - move to sample)
├── Library/ (❌ Build artifacts - delete)
├── Temp/ (❌ Temp files - delete)
├── Packages/ (✅ Package lives here)
├── *.csproj (❌ Unity-generated - remove)
└── *.sln (❌ Unity-generated - remove)
```

### Target Structure (Package-Based Repo)

```ts
TheStoryTest/
├── Packages/
│   └── com.tinywalnutgames.storytest/ (✅ Main package)
├── Samples~/
│   └── ExampleProject/ (Clean Unity project demonstrating package)
│       ├── Assets/
│       ├── ProjectSettings/
│       └── Packages/
│           └── manifest.json (references main package)
├── Documentation~/
│   ├── QuickStart.md
│   ├── DynamicValidation.md
│   └── AssemblyStructure.md
├── docs/ (GitHub Pages documentation)
├── .github/workflows/ (CI/CD for package testing)
├── story_test.py (Standalone CLI tool)
└── README.md (Package installation guide)
```

## Phase 2 Tasks

### Task 1: Create Sample Project Structure

```bash
mkdir -p "Samples~/ExampleProject"
```

**What to include in sample:**

- Minimal Unity project demonstrating Story Test usage
- Example MonoBehaviour using `ProductionExcellenceStoryTest`
- Example scene with Story Test validation
- README explaining how to use the sample

### Task 2: Move Unity Project Files

**Move these to sample:**

- `Assets/Scenes/` → `Samples~/ExampleProject/Assets/Scenes/`
- `Assets/Settings/` → `Samples~/ExampleProject/Assets/Settings/`
- `Assets/InputSystem_Actions.inputactions` → `Samples~/ExampleProject/Assets/`
- `ProjectSettings/` → `Samples~/ExampleProject/ProjectSettings/`

**Delete from root:**

- `Assets/Tiny Walnut Games/TheStoryTest/` (now in package)
- `Library/` (build artifacts)
- `Temp/` (temporary files)
- `Logs/` (log files)
- `UserSettings/` (user-specific)
- `*.csproj` (Unity-generated)
- `*.sln` (Unity-generated)

### Task 3: Create Sample Package Manifest

Create `Samples~/ExampleProject/Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.tinywalnutgames.storytest": "file:../../../Packages/com.tinywalnutgames.storytest",
    "com.unity.inputsystem": "1.14.2",
    "com.unity.render-pipelines.universal": "17.2.0",
    "com.unity.ugui": "2.0.0"
  }
}
```

### Task 4: Update Root .gitignore

Update `.gitignore` for package-based structure:

```gitignore
# Unity Build Artifacts (from samples)
[Ss]amples~/*/[Ll]ibrary/
[Ss]amples~/*/[Tt]emp/
[Ss]amples~/*/[Oo]bj/
[Ss]amples~/*/[Ll]ogs/

# Unity User Settings (from samples)
[Ss]amples~/*/[Uu]serSettings/

# Visual Studio / Rider
*.csproj
*.sln
*.suo
*.user
*.userprefs
.vs/
.idea/

# Python
__pycache__/
*.pyc
.pytest_cache/
.venv/
venv/

# Story Test Output
.debug/
story-test-report.json
```

### Task 5: Create Documentation~ Folder

Move technical documentation to Unity package documentation:

```bash
mkdir -p "Packages/com.tinywalnutgames.storytest/Documentation~"
```

**Files to move:**

- `QUICKSTART.md` → `Documentation~/QuickStart.md`
- `DYNAMIC_VALIDATION.md` → `Documentation~/DynamicValidation.md`
- `ASSEMBLY_STRUCTURE.md` → `Documentation~/AssemblyStructure.md`

**Files to move to docs/ (GitHub Pages):**

- `README.md` → Keep at root (package overview)
- `CHANGELOG.md` → Keep at root
- Migration guides → `docs/migration/`
- Technical specs → `docs/technical/`

### Task 6: Create Sample README

Create `Samples~/ExampleProject/README.md`:

```markdown
# Story Test Example Project

This is a minimal Unity project demonstrating how to use The Story Test Framework package.

## What's Included

- Example scene with `ProductionExcellenceStoryTest` MonoBehaviour
- Configured Story Test settings
- Example validation integration

## How to Use

1. Open this project in Unity 2020.3+
2. The Story Test package is referenced from the repository
3. Open the example scene: `Assets/Scenes/SampleScene.unity`
4. Run validation via menu: `Tiny Walnut Games/The Story Test/Run Story Test`

## Package Reference

This sample project references the package using a file path:

\`\`\`json
"com.tinywalnutgames.storytest": "file:../../../Packages/com.tinywalnutgames.storytest"
\`\`\`

In your own projects, use a git URL instead:

\`\`\`json
"com.tinywalnutgames.storytest": "https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest"
\`\`\`
```

## Execution Order

1. ✅ Create `Samples~/ExampleProject/` directory structure
2. ✅ Create sample manifest.json
3. ✅ Move Assets/Scenes/ to sample
4. ✅ Move ProjectSettings/ to sample (minimal required files only)
5. ✅ Delete root Unity project files
6. ✅ Create Documentation~ folder
7. ✅ Move documentation files
8. ✅ Update root .gitignore
9. ✅ Create sample README
10. ✅ Test package installation

## Files to Delete from Root

```bash
# Unity build/temp files
rm -rf Library/
rm -rf Temp/
rm -rf Logs/
rm -rf UserSettings/

# Unity-generated project files
rm -f *.csproj
rm -f *.sln

# Unity project-specific files (moving to sample)
rm -rf "Assets/Tiny Walnut Games/"
rm -f Assets/DefaultVolumeProfile.asset*
rm -f Assets/UniversalRenderPipelineGlobalSettings.asset*
```

## Files to Keep at Root

```bash
# Package
Packages/com.tinywalnutgames.storytest/

# Documentation
README.md
CHANGELOG.md
LICENSE
*.md (technical docs will move to docs/)

# Standalone tools
story_test.py
requirements.txt

# CI/CD
.github/workflows/

# Git
.git/
.gitignore
.gitattributes
```

## Testing After Cleanup

### Test 1: Package Structure Validation

```bash
# Check package.json is valid
cat Packages/com.tinywalnutgames.storytest/package.json | jq .

# Verify all required files exist
ls Packages/com.tinywalnutgames.storytest/Runtime/
ls Packages/com.tinywalnutgames.storytest/Editor/
ls Packages/com.tinywalnutgames.storytest/Tests/
```

### Test 2: Sample Project Opens

```bash
# Open sample in Unity
unity-editor -projectPath "Samples~/ExampleProject"
```

### Test 3: Package Installation Test

Create a new Unity project and test:

```json
{
  "dependencies": {
    "com.tinywalnutgames.storytest": "file:../TheStoryTest/Packages/com.tinywalnutgames.storytest"
  }
}
```

## Expected Outcome

**After Phase 2:**

- ✅ Root directory is clean (no Unity project clutter)
- ✅ Sample project demonstrates package usage
- ✅ Documentation organized properly
- ✅ Package can be installed in any Unity project
- ✅ CI/CD can test package in isolation
- ✅ Repository structure follows UPM best practices

## Success Criteria

1. Package installs cleanly in empty Unity project
2. Sample project opens without errors
3. Story Test validation runs in sample
4. No Unity-specific files polluting root
5. Documentation is accessible in Unity Package Manager
6. GitHub Actions can test package

## Next Steps After Phase 2

**Phase 3:** Documentation & Polish

- Create comprehensive package documentation
- Add code examples and tutorials
- Create video walkthrough
- Polish README and CHANGELOG

**Phase 4:** CI/CD Updates

- Update workflows to test package installation
- Add package validation step
- Test across Unity versions
- Verify cross-platform compatibility

**Phase 5:** Distribution

- Create release workflow
- Set up automatic versioning
- Publish to UPM registry
- Create GitHub Release

---

**Ready to execute!** Let's start with Task 1: Create Sample Project Structure.
