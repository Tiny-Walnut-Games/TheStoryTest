# Dynamic Validation Architecture - Story Test Framework

## Overview

The Story Test framework has been refactored to be **completely project-agnostic** and **environment-agnostic**. It now works with any .NET project without requiring Unity, DOTS, Burst, or specific component types.

## Three-Tier Validation System

### Tier 1: Universal Validation (Acts 1-9)

**Always Active** - Works in ANY .NET environment

The core 9 Acts validate fundamental code quality:

- **Act 1**: TODO comments and NotImplementedException
- **Act 2**: Placeholder implementations (minimal IL bytecode)
- **Act 3**: Incomplete class implementations
- **Act 4**: Unsealed abstract members
- **Act 5**: Debug-only implementations without [Obsolete]
- **Act 6**: Phantom properties (never used)
- **Act 7**: Cold methods (empty implementations)
- **Act 8**: Hollow enums (≤1 values or all placeholders)
- **Act 9**: Premature celebrations (marked complete but throws NotImplementedException)

**Requirements**: None - works with pure .NET assemblies
**Configuration**: Controlled by `assemblyFilters` and `includeUnityAssemblies` in settings

### Tier 2: Conceptual Validation

**Optional** - Enabled via `conceptualValidation.enableConceptTests`

Dynamic validation that discovers and validates project patterns:

- **Enum Validation**: Ensures all enums have ≥2 meaningful values
- **Value Type Validation**: Verifies structs can be instantiated and have accessible fields
- **Abstract Member Sealing**: Validates classes with abstract members are marked abstract
- **Environment Detection**: Auto-detects Unity, DOTS, Burst capabilities

**Requirements**: None - adapts to detected environment
**Fallback**: Uses IL bytecode analysis when runtime instantiation fails
**Configuration**: `conceptualValidation` section in settings

### Tier 3: Project-Specific Validation

**Opt-In** - Enabled via `conceptualValidation.validationTiers.projectSpecific`

Custom validation for your specific project needs:

- **Custom Component Types**: Validate specific types listed in `customComponentTypes[]`
- **Enum Patterns**: Additional enum naming/structure rules via `enumValidationPatterns[]`
- **Project-Specific Rules**: Extend via custom validation classes

**Requirements**: User configuration in StoryTestSettings.json
**Configuration**: Define target types in `customComponentTypes` array

## Configuration Reference

### StoryTestSettings.json Structure

```json
{
  "projectName": "YourProjectName",
  "assemblyFilters": [],
  "includeUnityAssemblies": false,
  "strictMode": false,
  
  "conceptualValidation": {
    "enableConceptTests": true,
    "autoDetectEnvironment": true,
    "validationTiers": {
      "universal": true,
      "unityAware": true,
      "projectSpecific": false
    },
    "environmentCapabilities": {
      "hasUnityEngine": false,
      "hasDOTS": false,
      "hasBurst": false,
      "hasEntities": false,
      "canInstantiateComponents": false
    },
    "customComponentTypes": [],
    "enumValidationPatterns": [],
    "fallbackMode": "ilAnalysis"
  }
}
```

### Key Configuration Options

#### `conceptualValidation.enableConceptTests`

- **Type**: boolean
- **Default**: `true`
- **Purpose**: Enable/disable Tier 2 conceptual validation
- **Use Case**: Disable if you only want core Acts 1-9

#### `conceptualValidation.autoDetectEnvironment`

- **Type**: boolean
- **Default**: `true`
- **Purpose**: Automatically detect Unity/DOTS/Burst at runtime
- **Use Case**: Disable if you want manual control via `environmentCapabilities`

#### `conceptualValidation.validationTiers.*`

- **universal**: Core Acts 1-9 (always recommended)
- **unityAware**: Unity-specific pattern validation
- **projectSpecific**: Custom types validation

#### `conceptualValidation.customComponentTypes`

- **Type**: string[]
- **Format**: Fully qualified type names (e.g., `"MyNamespace.MyComponent"`)
- **Purpose**: Explicitly validate specific types in your project
- **Example**:

  ```json
  "customComponentTypes": [
    "MyGame.Player",
    "MyGame.Enemy",
    "MyGame.Weapon"
  ]
  ```

#### `conceptualValidation.fallbackMode`

- **Values**: `"ilAnalysis"` or `"skip"`
- **Purpose**: Validation strategy when component instantiation fails
- **ilAnalysis**: Use IL bytecode parsing (recommended)
- **skip**: Skip validation for non-instantiable types

## Environment Detection

The framework auto-detects your environment at runtime:

```csharp
var capabilities = ConceptualValidator.DetectEnvironment();
// Returns: Unity: true, DOTS: false, Burst: false, etc.
```

**Detected Capabilities:**

- `hasUnityEngine`: UnityEngine assembly available
- `hasDOTS`: Unity.Entities assembly available
- `hasBurst`: Unity.Burst assembly available  
- `hasEntities`: Unity ECS available
- `canInstantiateComponents`: Runtime type instantiation possible

## Migration from Hardcoded Validation

### Before (Project-Specific)

```csharp
[Test]
public void Position_ConstructorsWork() {
    var pos = new Position(new float2(5, 10));
    Assert.AreEqual(new float2(5, 10), pos.Value);
}
```

❌ **Problem**: Requires `Position` type and Unity.Mathematics to exist in all projects

### After (Dynamic/Conceptual)

```csharp
[Test]
public void ValueTypesHaveValidDefaultConstructors() {
    var assemblies = ConceptualValidator.GetProjectAssemblies(settings);
    foreach (var assembly in assemblies) {
        var valueTypes = assembly.GetTypes()
            .Where(t => t.IsValueType && !t.IsEnum);
        foreach (var type in valueTypes) {
            var instance = Activator.CreateInstance(type);
            Assert.IsNotNull(instance);
        }
    }
}
```

✅ **Solution**: Discovers value types dynamically, works with ANY project structure

## Usage Examples

### Example 1: Pure .NET Library Validation

```json
{
  "projectName": "MyLibrary",
  "assemblyFilters": ["MyLibrary"],
  "includeUnityAssemblies": false,
  "conceptualValidation": {
    "enableConceptTests": true,
    "validationTiers": {
      "universal": true,
      "unityAware": false,
      "projectSpecific": false
    }
  }
}
```

### Example 2: Unity GameObject Project

```json
{
  "projectName": "MyUnityGame",
  "assemblyFilters": [],
  "conceptualValidation": {
    "enableConceptTests": true,
    "autoDetectEnvironment": true,
    "validationTiers": {
      "universal": true,
      "unityAware": true,
      "projectSpecific": true
    },
    "customComponentTypes": [
      "MyGame.PlayerController",
      "MyGame.GameManager"
    ]
  }
}
```

### Example 3: DOTS/ECS Project

```json
{
  "projectName": "MyDOTSProject",
  "conceptualValidation": {
    "enableConceptTests": true,
    "autoDetectEnvironment": true,
    "validationTiers": {
      "universal": true,
      "unityAware": true,
      "projectSpecific": true
    },
    "customComponentTypes": [
      "MyGame.PositionComponent",
      "MyGame.VelocityComponent"
    ],
    "fallbackMode": "ilAnalysis"
  }
}
```

## API Reference

### ConceptualValidator Class

```csharp
// Detect environment capabilities
var capabilities = ConceptualValidator.DetectEnvironment();

// Validate an enum type
var enumViolations = ConceptualValidator.ValidateEnum(typeof(MyEnum));

// Validate a value type (struct)
var structViolations = ConceptualValidator.ValidateValueType(typeof(MyStruct));

// Validate abstract member sealing
var abstractViolations = ConceptualValidator.ValidateAbstractMemberSealing(typeof(MyClass));

// Get project assemblies based on settings
var assemblies = ConceptualValidator.GetProjectAssemblies(settings);

// Validate custom components from settings
var customViolations = ConceptualValidator.ValidateCustomComponents(
    settings.conceptualValidation.customComponentTypes
);
```

## Test Structure

### Universal Tests (StoryTestValidationTests.cs)

- **StoryIgnoreAttribute_RequiresReason**: Validates attribute contract
- **StoryIntegrityValidator_ValidatesAssemblies**: Core validator functionality
- **StoryIntegrityValidator_RespectsStoryIgnoreAttribute**: Opt-out mechanism
- **ProductionExcellenceStoryTest_ValidatesConfiguration**: MonoBehaviour integration

### Conceptual Tests (ConceptualValidationTests.cs)

- **AllEnumTypesHaveValidValues**: Dynamic enum validation
- **ValueTypesHaveValidDefaultConstructors**: Dynamic struct validation
- **ClassesWithAbstractMembersAreAbstract**: Abstract sealing validation
- **ConceptualValidator_SettingsLoaded**: Configuration system test

### Integration Tests (IntegrationTests.cs)

- **StoryTestCompliance_CoreFrameworkHasNoViolations**: Self-validation
- **EnvironmentDetection_WorksCorrectly**: Capability detection test

## Benefits of Dynamic Validation

✅ **Universal**: Works with any .NET project (Console, Library, Unity, Standalone)
✅ **Scalable**: Add new projects without changing validation code
✅ **Flexible**: Configure validation via JSON, not code changes
✅ **Maintainable**: Single test suite for all project types
✅ **Extensible**: Add custom validators without forking
✅ **CI/CD Friendly**: Same tests run in all environments

## Migration Checklist

- [x] Remove Unity.Mathematics, Unity.Collections, Unity.Burst dependencies
- [x] Replace hardcoded component tests with dynamic discovery
- [x] Add ConceptualValidator utility class
- [x] Update StoryTestSettings with conceptualValidation section
- [x] Create project-agnostic test suite
- [x] Document three-tier validation system
- [ ] Update README.md with new architecture
- [ ] Update QUICKSTART.md with configuration examples
- [ ] Update copilot-instructions.md with new mental model

## Next Steps

1. **Configure for Your Project**: Edit `StoryTestSettings.json` with your project name and assembly filters
2. **Enable Tiers**: Choose which validation tiers suit your needs
3. **Add Custom Types**: List specific types to validate in `customComponentTypes`
4. **Run Validation**: Execute tests and review results
5. **Iterate**: Refine configuration based on violation reports

---

**Philosophy**: The Story Test framework should adapt to YOUR project, not force your project to adapt to IT. Dynamic validation enables universal code quality assurance without coupling to specific frameworks or architectures.
