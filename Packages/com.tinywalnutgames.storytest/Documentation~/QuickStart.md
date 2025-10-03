# Quick Start Guide

## Installation

### Method 1: Git URL (Recommended)

Add to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.tinywalnutgames.storytest": "https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest"
  }
}
```

### Method 2: Unity Package Manager UI

1. Open `Window > Package Manager`
2. Click `+` → "Add package from git URL..."
3. Enter: `https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest`

## Basic Usage

### Running Validation via Menu

1. Open Unity menu: `Tiny Walnut Games > The Story Test > Run Story Test and Export Report`
2. Check console for results
3. Review report in `.debug/storytest_report.txt`

### Running Validation in Scene

1. Add `ProductionExcellenceStoryTest` component to any GameObject
2. Right-click component → `Validate Production Excellence`
3. Review console output

## The 9 Acts

The Story Test Framework enforces 9 validation rules:

1. **Act 1: Todo Comments** - No `NotImplementedException` or default-only returns
2. **Act 2: Placeholder Implementations** - No stub methods with minimal IL bytecode
3. **Act 3: Incomplete Classes** - Non-abstract classes must implement all abstract methods
4. **Act 4: Unsealed Abstract Members** - No abstract methods in non-abstract classes
5. **Act 5: Debug Only Implementations** - Debug methods must have `[Obsolete]` attribute
6. **Act 6: Phantom Props** - Auto-properties must be meaningfully used
7. **Act 7: Cold Methods** - No empty or minimal methods
8. **Act 8: Hollow Enums** - Enums must have meaningful values
9. **Act 9: Premature Celebrations** - Complete code can't throw `NotImplementedException`

## Opt-Out with StoryIgnore

Use `[StoryIgnore]` for infrastructure code:

```csharp
[StoryIgnore("Test infrastructure component")]
public class MyTestHelper : MonoBehaviour { }
```

**Important:** Must provide non-empty reason string.

## Example: Correct Usage

```csharp
// ✅ All parameters used
public void RenderScene(Scene scene, Camera camera, int quality) {
    scene.Draw(camera, quality);
}

// ✅ Debug method marked temporary
[Obsolete("Debug visualization only")]
public void DebugDrawGizmos() {
    Gizmos.DrawWireSphere(transform.position, 1.0f);
}
```

## Example: Violations

```csharp
// ❌ Act 1: Placeholder
public float Calculate() {
    throw new NotImplementedException();
}

// ❌ Act 2: Unused parameter
public void RenderScene(Scene scene, Camera camera, int quality) {
    scene.Draw(camera); // quality ignored!
}

// ❌ Act 5: Debug method not marked
public void DebugDrawGizmos() {
    // Missing [Obsolete]
}
```

## Configuration

Customize validation in `Resources/StoryTestSettings.json`:

```json
{
  "EnableStoryIntegrity": true,
  "EnableCodeCoverage": true,
  "EnableArchitecturalCompliance": true,
  "EnableProductionReadiness": true,
  "EnableSyncPointPerformance": true,
  "EnableDOTSValidation": false
}
```

## CI/CD Integration

See [GitHub Actions Workflow](.github/workflows/story-test.yml) for automated validation.

## Next Steps

- Review [Dynamic Validation](DynamicValidation.md) for runtime validation
- Explore [Assembly Structure](AssemblyStructure.md) for architecture details
- Check the [Example Project](../../Samples~/ExampleProject/) for practical examples
