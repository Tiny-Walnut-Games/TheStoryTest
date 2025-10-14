# Getting Started

Welcome to The Story Test Framework! This guide will get you up and running with code quality validation in minutes.

## Installation

### Unity Package Manager (Recommended)

1. Open Unity Editor
2. Go to `Window > Package Manager`
3. Click the `+` button in the top-left
4. Select **Add package from git URL...**
5. Enter: `https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest`
6. Wait for the package to download and import

### Manual Installation

1. Download the latest release from [GitHub Releases](https://github.com/jmeyer1980/TheStoryTest/releases)
2. Extract to your project's `Packages/` folder
3. Unity will automatically detect and import the package

### Git URL Method

Add to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.tinywalnutgames.storytest": "https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest"
  }
}
```

## First Validation

### Unity Editor Validation

#### Quick Menu Validation
1. Open the Unity menu: `Tiny Walnut Games > The Story Test > Run Story Test and Export Report`
2. Watch the Console for validation results
3. Find detailed report at `.debug/storytest_report.txt`

#### In-Scene Validation (Repeatable)
1. Add `ProductionExcellenceStoryTest` component to any GameObject
2. Configure which validation phases to run
3. Use component context menu: **Validate Production Excellence**
4. Or call `ValidateProductionExcellence()` at runtime

### Python Standalone Validation

#### Setup
```bash
# Install Python dependencies
pip install -r requirements.txt

# Verify installation
python scripts/story_test_unity_safe.py --help
```

#### Basic Usage
```bash
# Validate current Unity project
python scripts/story_test_unity_safe.py . --verbose

# Validate specific assembly
python scripts/story_test_unity_safe.py Library/ScriptAssemblies/MyAssembly.dll

# Export JSON report
python scripts/story_test_unity_safe.py . --output report.json

# CI/CD usage (fails on violations)
python scripts/story_test_unity_safe.py . --fail-on-violations --output ci-report.json
```

## Understanding Results

### Violation Types

- **IncompleteImplementation**: `NotImplementedException` or empty methods
- **DebuggingCode**: Debug methods without `[Obsolete]` attribute
- **UnusedCode**: Dead code, unused properties, hollow enums
- **PrematureCelebration**: Marked complete but still throwing exceptions

### Example Output
```
❌ Found 2 violation(s):

  [IncompleteImplementation] PlayerController.CalculateDamage: Method contains NotImplementedException
  [UnusedCode] GameSettings.UnusedProperty: Phantom property detected - name suggests it's unused

Violations by type:
  - IncompleteImplementation: 1
  - UnusedCode: 1
```

## Configuration

### Basic Settings

Create or edit `Assets/Tiny Walnut Games/TheStoryTest/Resources/StoryTestSettings.json`:

```json
{
  "projectName": "YourProjectName",
  "menuPath": "Tiny Walnut Games/The Story Test/",
  "assemblyFilters": {
    "include": ["Assembly-CSharp", "YourCustomAssembly"],
    "exclude": ["Unity", "System", "Mono"]
  },
  "phases": {
    "enableStoryIntegrity": true,
    "enableCodeCoverage": false,
    "enableArchitecturalCompliance": false,
    "enableProductionReadiness": true,
    "enableSyncPointPerformance": false
  }
}
```

### Assembly Filtering

Control which assemblies get validated:

```json
"assemblyFilters": {
  "include": ["Assembly-CSharp", "MyGame.Logic"],
  "exclude": ["Unity.*", "System.*", "Mono.*"]
}
```

## Common Workflows

### During Development
1. Run validation after major feature completion
2. Use Unity menu for quick checks
3. Focus on **IncompleteImplementation** violations first

### Before Commit
```bash
# Quick validation
python scripts/story_test_unity_safe.py . --verbose

# Fail build if violations found
python scripts/story_test_unity_safe.py . --fail-on-violations
```

### CI/CD Integration
Add to your pipeline:
```yaml
- name: Run Story Test Validation
  run: python scripts/story_test_unity_safe.py . --fail-on-violations --output report.json
```

## Next Steps

- [Learn about the 11 Acts](acts.md)
- [Configure advanced settings](configuration.md)
- [Set up CI/CD integration](ci-cd.md)
- [Explore Python validator options](python-validator.md)

## Troubleshooting

### Common Issues

**Python validator crashes on Unity assemblies**
- Use `story_test_unity_safe.py` instead of `story_test.py`
- This version skips Unity-dependent assemblies automatically

**No violations found but code has issues**
- Check assembly filters in settings
- Ensure your assemblies are included in validation
- Verify `[StoryIgnore]` isn't hiding real violations

**Performance issues**
- Reduce assembly scope with filters
- Disable validation phases you don't need
- Use Unity-safe validator for faster execution

### Getting Help

- [Troubleshooting Guide](troubleshooting.md)
- [GitHub Issues](https://github.com/jmeyer1980/TheStoryTest/issues)
- [Discussions](https://github.com/jmeyer1980/TheStoryTest/discussions)