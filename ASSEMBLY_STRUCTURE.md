# Assembly Definition Files - Complete Structure

## Assembly Architecture

The Story Test framework uses a modular assembly structure for clean dependencies and Unity compatibility:

```flow
TinyWalnutGames.TheStoryTest.Shared (Foundation)
    ↑
    ├── TinyWalnutGames.TheStoryTest.Acts (Validation Rules)
    ↑
TinyWalnutGames.TheStoryTest (Core Framework)
    ↑
    ├── TinyWalnutGames.TheStoryTest.Editor (Unity Editor Integration)
    └── TinyWalnutGames.TheStoryTest.Tests (NUnit Tests)
```

---

## Assembly Definitions

### 1. **TinyWalnutGames.TheStoryTest.Shared.asmdef**

**Location**: `Assets/Tiny Walnut Games/TheStoryTest/Runtime/Shared/`

**Purpose**: Foundation assembly containing shared types used across all assemblies

**Contains**:

- `StoryIgnoreAttribute` - Attribute to mark code excluded from validation
- `StoryViolation` - Data structure for validation failures
- `StoryViolationType` - Enum categorizing violation types
- `ValidationRule` - Delegate type for validation rules
- `StoryTestUtilities` - Shared IL analysis helpers
- `StoryTestSettings` - Configuration system

**References**: None (no dependencies)

**Platform**: All platforms (Runtime)

---

### 2. **TinyWalnutGames.TheStoryTest.Acts.asmdef**

**Location**: `Assets/Tiny Walnut Games/TheStoryTest/Runtime/Acts/`

**Purpose**: Contains the 9 validation "Acts" that perform IL bytecode analysis

**Contains**:

- `Act1TodoComments` - Detects NotImplementedException
- `Act2PlaceholderImplementations` - Catches stub methods
- `Act3IncompleteClasses` - Ensures abstract methods are implemented
- `Act4UnsealedAbstractMembers` - Prevents invalid abstract members
- `Act5DebugOnlyImplementations` - Requires Obsolete on debug code
- `Act6PhantomProps` - Identifies unused properties
- `Act7ColdMethods` - Finds empty methods
- `Act8HollowEnums` - Catches incomplete enums
- `Act9PrematureCelebrations` - Detects falsely complete code

**References**:

- `TinyWalnutGames.TheStoryTest.Shared`

**Platform**: All platforms (Runtime)

---

### 3. **TinyWalnutGames.TheStoryTest.asmdef**

**Location**: `Assets/Tiny Walnut Games/TheStoryTest/` (root)

**Purpose**: Core framework assembly containing validators and orchestration

**Contains**:

- `StoryIntegrityValidator` - Central validation orchestrator
- `ProductionExcellenceStoryTest` - MonoBehaviour for in-editor validation
- `StoryTestSyncPointValidator` - Performance testing for parallel validation
- `StoryTestRuleBootstrapper` - Auto-registration of validation rules
- `StoryTestUtilities` - IL analysis utilities

**References**:

- `TinyWalnutGames.TheStoryTest.Shared`
- `TinyWalnutGames.TheStoryTest.Acts`

**Platform**: All platforms (Runtime)

---

### 4. **TinyWalnutGames.TheStoryTest.Editor.asmdef**

**Location**: `Assets/Tiny Walnut Games/TheStoryTest/Editor/`

**Purpose**: Unity Editor integration and tooling

**Contains**:

- `StoryTestExportMenu` - Unity menu item for exporting reports
- `StrengtheningValidationSuite` - Comprehensive validation pipeline UI
- `StrengtheningConfigurationWindow` - Settings editor window
- `StoryValidationWindow` - Results display window

**References**:

- `TinyWalnutGames.TheStoryTest`
- `TinyWalnutGames.TheStoryTest.Shared`

**Platform**: Editor only

**Special**: Editor-only assembly, excluded from builds

---

### 5. **TinyWalnutGames.TheStoryTest.Tests.asmdef**

**Location**: `Assets/Tiny Walnut Games/TheStoryTest/Tests/`

**Purpose**: NUnit test assembly for framework validation

**Contains**:

- `StoryTestValidationTests` - Unit tests for the framework itself
- Test cases for each Act
- Integration tests

**References**:

- `UnityEngine.TestRunner`
- `UnityEditor.TestRunner`
- `TinyWalnutGames.TheStoryTest`
- `TinyWalnutGames.TheStoryTest.Shared`
- `TinyWalnutGames.TheStoryTest.Acts`

**Precompiled References**:

- `nunit.framework.dll`

**Platform**: Test assemblies (excluded from builds)

**Special**:

- `autoReferenced: false` - Not included in regular builds
- `overrideReferences: true` - Uses explicit NUnit reference
- `defineConstraints: ["UNITY_INCLUDE_TESTS"]` - Only compiles in test context

---

## Dependency Graph

```flow
Shared (no deps)
  ↓
Acts (→ Shared)
  ↓
Main (→ Shared, Acts)
  ↓
  ├── Editor (→ Main, Shared) [Editor-only]
  └── Tests (→ Main, Shared, Acts, NUnit) [Test-only]
```

---

## Key Design Decisions

### 1. **Shared First Architecture**

The `Shared` assembly has zero dependencies, making it the foundation. This allows:

- Any assembly to reference common types
- Python validator to use the same type definitions
- No circular dependencies

### 2. **Acts Isolation**

The `Acts` assembly only depends on `Shared`, making validation rules:

- Portable to non-Unity contexts
- Easy to test independently
- Simple to extend with new rules

### 3. **Editor Separation**

Editor-only code is isolated in its own assembly:

- Excluded from runtime builds
- Clear separation of concerns
- No editor dependencies in runtime code

### 4. **Test Assembly Configuration**

The Tests assembly uses:

- `autoReferenced: false` - Prevents inclusion in builds
- `UNITY_INCLUDE_TESTS` - Only compiles when tests are enabled
- Explicit NUnit reference for test framework support

---

## Assembly Compilation Order

Unity compiles assemblies in dependency order:

1. **TinyWalnutGames.TheStoryTest.Shared** (no deps)
2. **TinyWalnutGames.TheStoryTest.Acts** (depends on Shared)
3. **TinyWalnutGames.TheStoryTest** (depends on Shared + Acts)
4. **TinyWalnutGames.TheStoryTest.Editor** (depends on Main + Shared)
5. **TinyWalnutGames.TheStoryTest.Tests** (depends on all above + NUnit)

---

## Platform Targeting

| Assembly | Windows | macOS | Linux | WebGL | Mobile | Editor |
|----------|---------|-------|-------|-------|--------|--------|
| Shared   | ✅      | ✅    | ✅    | ✅    | ✅     | ✅     |
| Acts     | ✅      | ✅    | ✅    | ✅    | ✅     | ✅     |
| Main     | ✅      | ✅    | ✅    | ✅    | ✅     | ✅     |
| Editor   | ❌      | ❌    | ❌    | ❌    | ❌     | ✅     |
| Tests    | ❌      | ❌    | ❌    | ❌    | ❌     | ✅*    |

*Tests only compile when `UNITY_INCLUDE_TESTS` is defined

---

## Troubleshooting

### "Assembly could not be found"

**Cause**: Unity hasn't recompiled assemblies yet
**Solution**: Wait 10-30 seconds for Unity to finish compilation

### "Circular dependency detected"

**Cause**: Assembly references form a cycle
**Solution**: Check that dependencies follow the graph above (Shared → Acts → Main → Editor/Tests)

### "Type not found in assembly"

**Cause**: Missing assembly reference in `.asmdef`
**Solution**: Add the required assembly to the `references` array

### "NUnit tests not running"

**Cause**: Test assembly not configured correctly
**Solution**: Ensure `overrideReferences: true` and `nunit.framework.dll` in `precompiledReferences`

---

## Adding New Assemblies

If you need to add a new assembly:

1. Create folder for new assembly
2. Create `.asmdef` file with unique name
3. Add to `references` array in dependent assemblies
4. Follow dependency graph (always reference "lower" assemblies)
5. Wait for Unity to recompile

Example:

```json
{
    "name": "TinyWalnutGames.TheStoryTest.NewFeature",
    "rootNamespace": "TinyWalnutGames.TheStoryTest.NewFeature",
    "references": [
        "TinyWalnutGames.TheStoryTest.Shared"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

---

**Date**: October 3, 2025  
**Status**: ✅ All assembly definitions complete and properly configured
