# The Story Test Framework

A comprehensive code quality validation framework for Unity and .NET projects that enforces **"Story Test Doctrine"**: every symbol (method, property, parameter, enum) must be fully implemented and meaningful—no placeholders, TODOs, or unused code in production.

## Features

### The "9 Acts" Validation System

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

### Three-Tier Validation System

- **Tier 1 (Universal)**: Acts 1-9 IL bytecode analysis - works in ANY .NET environment
- **Tier 2 (Conceptual)**: Dynamic discovery (enums, structs, abstract members) with environment detection
- **Tier 3 (Project-Specific)**: User-configured custom component validation

### Environment Support

- ✅ Pure .NET projects
- ✅ Unity GameObject-based projects  
- ✅ Unity ECS/DOTS projects
- ✅ Hybrid Unity projects
- ✅ Standalone CLI tool (Python)

## Installation

### Via Git URL (Recommended)

Add this to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.tinywalnutgames.storytest": "https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest"
  }
}
```

### Via Unity Package Manager

1. Open Unity Package Manager (`Window > Package Manager`)
2. Click the `+` button
3. Select "Add package from git URL..."
4. Enter: `https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest`

### Manual Installation

1. Download the latest release from [GitHub Releases](https://github.com/jmeyer1980/TheStoryTest/releases)
2. Extract to your project's `Packages/` folder
3. Unity will automatically detect and import the package

## Quick Start

### In Unity Editor

1. **Menu**: `Tiny Walnut Games/The Story Test/Run Story Test and Export Report` – automatically enters Play Mode when required, spins up `ProductionExcellenceStoryTest`, runs the full validation pipeline, and then saves a summary to `.debug/storytest_report.txt` with a prompt to reveal the file when finished.

1. **In-Scene**: Add `ProductionExcellenceStoryTest` MonoBehaviour and configure the phases you care about; you can trigger validation from the component context menu or on Start.

1. **Tests**: Use the included NUnit tests as reference examples for extending or integrating the framework.

### Configuration

Create or edit `Assets/Tiny Walnut Games/TheStoryTest/Resources/StoryTestSettings.json`:

```json
{
  "projectName": "YourProjectName",
  "menuPath": "Tiny Walnut Games/The Story Test/",
  "assemblyFilters": {
    "include": ["YourAssembly"],
    "exclude": ["Unity", "System", "Mono"]
  },
  "conceptualValidation": {
    "enableConceptTests": true,
    "autoDetectEnvironment": true
  }
}
```

## Usage Examples

### Opt-Out with StoryIgnoreAttribute

```csharp
[StoryIgnore("Infrastructure component for story test validation")]
public class ProductionExcellenceStoryTest : MonoBehaviour { }
```

**MUST** provide non-empty reason string - use sparingly!

### Every Parameter Must Be Used

```csharp
// ❌ BAD: quality parameter ignored
public void RenderScene(Scene scene, Camera camera, int quality) {
    scene.Draw(camera);
}

// ✅ GOOD: All parameters consumed
public void RenderScene(Scene scene, Camera camera, int quality) {
    scene.Draw(camera, quality);
}
```

### Debug Methods Must Be Marked Temporary

```csharp
// ❌ Act5 violation
public void DebugDrawGizmos() { }

// ✅ Correct
[Obsolete("Debug visualization only")]
public void DebugDrawGizmos() { }
```

## Requirements

- Unity 2020.3 or later
- .NET Standard 2.0 or later
- NUnit (for tests)

## Documentation

- [Quick Start Guide](Documentation~/QuickStart.md)
- [Acts Guide](Documentation~/ActsGuide.md)
- [CI & Automation](Documentation~/CI.md)
- [Dynamic Validation](Documentation~/DynamicValidation.md)
- [Assembly Structure](Documentation~/AssemblyStructure.md)
- [FAQ & Demo Playbook](Documentation~/FAQ.md)

## License

See [LICENSE](https://github.com/jmeyer1980/TheStoryTest/blob/main/LICENSE)

## Support

- [Issues](https://github.com/jmeyer1980/TheStoryTest/issues)
- [Discussions](https://github.com/jmeyer1980/TheStoryTest/discussions)

---

**Remember**: When AI suggests code completions, ensure every symbol is meaningful and contributes to the "narrative" of the codebase. Seal unused elements with explicit comments rather than deleting context.
