# Configuration Guide

The Story Test Framework provides extensive configuration options to tailor validation to your project's specific needs.

## Settings File Location

Create or edit: `Assets/Tiny Walnut Games/TheStoryTest/Resources/StoryTestSettings.json`

## Basic Configuration

```json
{
  "projectName": "YourProjectName",
  "menuPath": "Tiny Walnut Games/The Story Test/",
  "validateOnStart": false,
  "strictMode": true,
  "exportPath": ".debug/storytest_report.txt"
}
```

### Settings Overview

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `projectName` | string | "TheStoryTest" | Project identifier for reports |
| `menuPath` | string | "Tiny Walnut Games/The Story Test/" | Unity menu path |
| `validateOnStart` | bool | false | Auto-validate on scene start |
| `strictMode` | bool | true | Fail fast on first violation |
| `exportPath` | string | ".debug/storytest_report.txt" | Report output location |

## Assembly Filtering

Control which assemblies get validated:

```json
{
  "assemblyFilters": {
    "include": ["Assembly-CSharp", "MyGame.Logic", "MyGame.Core"],
    "exclude": ["Unity.*", "System.*", "Mono.*", "UnityEngine.*"]
  }
}
```

### Filter Patterns

- **Exact Match**: `"Assembly-CSharp"`
- **Wildcard**: `"Unity.*"` (matches all Unity assemblies)
- **Common Excludes**: `"System.*"`, `"Mono.*"`, `"UnityEngine.*"`

### Recommended Filters

```json
{
  "assemblyFilters": {
    "include": [
      "Assembly-CSharp",
      "Assembly-CSharp-firstpass",
      "YourCompany.*"
    ],
    "exclude": [
      "Unity.*",
      "System.*",
      "Mono.*",
      "Microsoft.*",
      "nunit.*"
    ]
  }
}
```

## Validation Phases

Enable/disable different validation phases:

```json
{
  "phases": {
    "enableStoryIntegrity": true,
    "enableCodeCoverage": false,
    "enableArchitecturalCompliance": false,
    "enableProductionReadiness": true,
    "enableSyncPointPerformance": false
  }
}
```

### Phase Descriptions

| Phase | Purpose | Performance Impact |
|-------|---------|-------------------|
| `enableStoryIntegrity` | Core 11 Acts validation | Low |
| `enableCodeCoverage` | Code coverage analysis | Medium |
| `enableArchitecturalCompliance` | Architecture pattern validation | High |
| `enableProductionReadiness` | Production readiness checks | Medium |
| `enableSyncPointPerformance` | Performance benchmarking | High |

## Individual Acts Configuration

Fine-tune specific validation rules:

```json
{
  "acts": {
    "enableAct1": true,  // Todo Comments
    "enableAct2": true,  // Placeholder Implementations
    "enableAct3": true,  // Incomplete Classes
    "enableAct4": true,  // Unsealed Abstract Members
    "enableAct5": true,  // Debug Only Implementations
    "enableAct6": true,  // Phantom Props
    "enableAct7": true,  // Cold Methods
    "enableAct8": true,  // Hollow Enums
    "enableAct9": true,  // Premature Celebrations
    "enableAct10": true, // Suspiciously Simple Methods
    "enableAct11": true  // Dead Code
  }
}
```

## Conceptual Validation

Advanced validation for conceptual integrity:

```json
{
  "conceptualValidation": {
    "enableConceptTests": true,
    "autoDetectEnvironment": true,
    "validationTiers": {
      "universal": true,
      "unityAware": true,
      "projectSpecific": true
    },
    "environmentCapabilities": {
      "hasUnityEngine": false,
      "hasDots": false,
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

### Environment Detection

The framework automatically detects:
- Unity runtime availability
- DOTS/ECS packages
- Burst compilation
- Entity systems
- Component instantiation capabilities

## DOTS/ECS Configuration

Special handling for DOTS projects:

```json
{
  "enableDotsValidation": true,
  "dotsSettings": {
    "validateComponentData": true,
    "validateSystemQueries": true,
    "validateWorldAccess": true,
    "allowJobSystemPatterns": true
  }
}
```

## Performance Settings

Optimize validation performance:

```json
{
  "performance": {
    "maxConcurrentValidations": 4,
    "timeoutSeconds": 30,
    "enableCaching": true,
    "cacheExpiryMinutes": 60,
    "enableProgressReporting": true
  }
}
```

## Reporting Configuration

Control how results are reported:

```json
{
  "reporting": {
    "includeStackTrace": false,
    "includeFileContext": true,
    "groupByType": true,
    "sortBySeverity": true,
    "maxViolationsPerType": 50,
    "exportFormats": ["txt", "json"]
  }
}
```

## Integration Settings

Configure external tool integrations:

```json
{
  "integrations": {
    "enableUnityConsole": true,
    "enableFileOutput": true,
    "enableJsonExport": false,
    "enableCiMode": false,
    "slackWebhook": "",
    "teamsWebhook": ""
  }
}
```

## Advanced Configuration

### Custom Validation Rules

Define project-specific validation patterns:

```json
{
  "customRules": [
    {
      "name": "NoMagicStrings",
      "pattern": "\\\"[A-Z_]+\\\"",
      "description": "Avoid magic strings in constants",
      "severity": "Warning"
    }
  ]
}
```

### Namespace Filtering

Filter validation by namespace:

```json
{
  "namespaceFilters": {
    "include": ["MyGame.*", "MyCompany.*"],
    "exclude": ["MyGame.Tests.*", "MyGame.Editor.*"]
  }
}
```

### Type Filtering

Filter validation by type patterns:

```json
{
  "typeFilters": {
    "include": ["*Controller", "*Service", "*Manager"],
    "exclude": ["*Test*", "*Editor*", "*Debug*"]
  }
}
```

## Environment-Specific Settings

### Development Environment
```json
{
  "profile": "development",
  "strictMode": false,
  "enableAllPhases": true,
  "verboseLogging": true
}
```

### Production Environment
```json
{
  "profile": "production",
  "strictMode": true,
  "enableCorePhasesOnly": true,
  "failOnWarnings": true
}
```

### CI/CD Environment
```json
{
  "profile": "ci",
  "strictMode": true,
  "enableFastValidation": true,
  "exportJson": true,
  "failOnViolations": true
}
```

## Configuration Profiles

Predefined configurations for common scenarios:

```json
{
  "profiles": {
    "strict": {
      "acts": { "enableAct1": true, "enableAct2": true, /* ... */ },
      "strictMode": true,
      "failOnWarnings": true
    },
    "lenient": {
      "acts": { "enableAct1": true, "enableAct5": true },
      "strictMode": false,
      "failOnWarnings": false
    },
    "performance": {
      "acts": { "enableAct1": true, "enableAct7": true, "enableAct11": true },
      "enableFastValidation": true,
      "maxConcurrentValidations": 8
    }
  }
}
```

## Runtime Configuration

Change settings programmatically:

```csharp
// Get current settings
var settings = StoryTestSettings.LoadOrCreate();

// Modify settings
settings.StrictMode = true;
settings.AssemblyFilters.Include.Add("MyNewAssembly");

// Save settings
settings.Save();
```

## Validation Context

Control validation context:

```json
{
  "validationContext": {
    "isDevelopmentBuild": false,
    "isEditor": true,
    "isPlayer": false,
    "platform": "StandaloneWindows64",
    "scriptingBackend": "IL2CPP"
  }
}
```

## Troubleshooting Configuration

### Common Issues

**Settings not loading**
- Ensure file is in `Resources/` folder
- Check JSON syntax validity
- Verify file name: `StoryTestSettings.json`

**Too many violations**
- Review assembly filters
- Disable non-essential phases
- Use `[StoryIgnore]` for legitimate exceptions

**Performance issues**
- Reduce concurrent validations
- Disable expensive phases
- Use assembly filtering to reduce scope

### Debug Mode

Enable detailed logging:

```json
{
  "debug": {
    "enableVerboseLogging": true,
    "logAssemblyLoading": true,
    "logValidationSteps": true,
    "logPerformanceMetrics": true
  }
}
```

## Best Practices

1. **Start Conservative**: Enable only core Acts initially
2. **Iterative Refinement**: Add more validation as team adapts
3. **Team Consensus**: Get team buy-in on strictness level
4. **Regular Review**: Periodically review and adjust settings
5. **Environment Awareness**: Use different profiles for dev/prod/CI

Remember: Configuration should serve your team's needs, not hinder productivity.