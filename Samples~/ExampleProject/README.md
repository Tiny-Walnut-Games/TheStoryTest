# Story Test Example Project

## Overview

This is a minimal Unity project demonstrating how to use **The Story Test Framework** package in your own projects.

## What's Included

- ✅ Example scene with Story Test validation
- ✅ Configured Story Test settings (auto-loads from Resources/)
- ✅ Example integration showing best practices
- ✅ Package referenced locally for development

## How to Use This Sample

### Opening the Sample

1. **Install Unity 2020.3 or later**
2. **Open this sample project:**

   ```bash
   unity-editor -projectPath "Samples~/ExampleProject"
   ```

3. **Wait for Unity to import the package** (it's referenced locally)

### Running Story Test Validation

## Method 1: Unity Menu

1. Open Unity menu: `Tiny Walnut Games > The Story Test > Run Story Test and Export Report`
2. Check the console for validation results
3. Review violations report in `.debug/storytest_report.txt`

## Method 2: In-Scene MonoBehaviour

1. Open the example scene: `Assets/Scenes/SampleScene.unity`
2. Select the GameObject with `ProductionExcellenceStoryTest` component
3. Right-click component → `Validate Production Excellence`
4. Review results in Console

### Configuration

Story Test settings are loaded from:

```text
Assets/Tiny Walnut Games/TheStoryTest/Resources/StoryTestSettings.json
```

Customize the settings for your project by editing this file.

## Package Reference

This sample uses a **local file reference** to the package for development:

```json
{
  "dependencies": {
    "com.tinywalnutgames.storytest": "file:../../../Packages/com.tinywalnutgames.storytest"
  }
}
```

## Using in Your Own Projects

### Installation Method 1: Git URL (Recommended)

Add this to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.tinywalnutgames.storytest": "https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest"
  }
}
```

### Installation Method 2: Unity Package Manager UI

1. Open Unity Package Manager (`Window > Package Manager`)
2. Click `+` button → "Add package from git URL..."
3. Enter: `https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest`

### Installation Method 3: Manual

1. Download the package from [GitHub Releases](https://github.com/jmeyer1980/TheStoryTest/releases)
2. Extract to your project's `Packages/` folder
3. Unity will auto-import

## What to Learn From This Sample

### Story Test Integration

The sample demonstrates:

- ✅ **Menu-based validation**: Quick validation via Unity menu
- ✅ **MonoBehaviour integration**: In-scene validation during gameplay
- ✅ **Settings configuration**: Project-specific validation rules
- ✅ **Opt-out with StoryIgnore**: When and how to use `[StoryIgnore]`

### Best Practices

1. **Every parameter must be used** - No unused method parameters
2. **No placeholder implementations** - All methods fully implemented
3. **Debug code marked temporary** - Use `[Obsolete]` on debug methods
4. **No phantom properties** - Properties must be meaningfully used
5. **Complete implementations** - No TODOs or `NotImplementedException`

## Example Code Patterns

### Correct Usage

```csharp
// All parameters used
public void RenderScene(Scene scene, Camera camera, int quality) {
    scene.Draw(camera, quality); // quality parameter consumed
}

// Debug method properly marked
[Obsolete("Debug visualization only")]
public void DebugDrawGizmos() {
    // Debug implementation
}

// Opt-out with justification
[StoryIgnore("Infrastructure component for validation framework")]
public class StoryTestValidator : MonoBehaviour { }
```

### Violations Detected

```csharp
// ❌ Act 1: Placeholder implementation
public float Calculate() {
    throw new NotImplementedException();
}

// ❌ Act 2: Unused parameter (quality)
public void RenderScene(Scene scene, Camera camera, int quality) {
    scene.Draw(camera); // quality ignored!
}

// ❌ Act 5: Debug method not marked temporary
public void DebugDrawGizmos() {
    // Missing [Obsolete] attribute
}
```

## Requirements

- Unity 2020.3 or later
- .NET Standard 2.0 or later
- NUnit (for running tests)

## Troubleshooting

### Package Not Found

If Unity can't find the package:

1. Close Unity
2. Delete `Library/` folder
3. Reopen project and let Unity reimport

### Validation Errors

If you see validation errors:

1. Check console for specific violations
2. Review `.debug/storytest_report.txt`
3. Fix violations or use `[StoryIgnore]` with justification

### Missing Menu Items

If Story Test menu doesn't appear:

1. Ensure package imported correctly (`Packages/com.tinywalnutgames.storytest/`)
2. Check Package Manager shows the package
3. Reimport package if needed

## Documentation

For complete documentation:

- [Full Documentation](https://github.com/jmeyer1980/TheStoryTest#readme)
- [Dynamic Validation Guide](https://github.com/jmeyer1980/TheStoryTest/blob/main/docs/dynamic-validation.md)
- [Assembly Structure](https://github.com/jmeyer1980/TheStoryTest/blob/main/docs/assembly-structure.md)
- [Quick Start Guide](https://github.com/jmeyer1980/TheStoryTest/blob/main/docs/quickstart.md)

## Support

- [GitHub Issues](https://github.com/jmeyer1980/TheStoryTest/issues)
- [GitHub Discussions](https://github.com/jmeyer1980/TheStoryTest/discussions)

---

**Remember:** When AI suggests code completions, ensure every symbol is meaningful and contributes to the "narrative" of the codebase!
