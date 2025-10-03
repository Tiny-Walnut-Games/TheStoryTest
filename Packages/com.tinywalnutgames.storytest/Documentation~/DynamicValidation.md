# Dynamic Validation Guide

## Overview

The Story Test Framework provides **runtime validation** that goes beyond static analysis, using IL bytecode inspection to detect code quality issues dynamically.

## How It Works

### IL Bytecode Analysis

The framework inspects compiled IL bytecode to detect patterns like:

```csharp
// Detected via IL opcodes 0x73 + 0x7A
throw new NotImplementedException();

// Detected via ldc.i4.* + ret pattern
return 0; // Only default return
```

### Three-Tier Validation

1. **Story Integrity**: The 9 Acts validation rules
2. **Code Coverage**: Ensures all symbols are meaningfully used
3. **Architectural Compliance**: Enforces design patterns (optional DOTS/ECS checks)

## Using ProductionExcellenceStoryTest

### Basic Setup

```csharp
using TinyWalnutGames.StoryTest;

public class GameValidator : MonoBehaviour {
    private ProductionExcellenceStoryTest validator;

    void Start() {
        validator = gameObject.AddComponent<ProductionExcellenceStoryTest>();
        validator.EnableStoryIntegrity = true;
        validator.EnableCodeCoverage = true;
        validator.EnableArchitecturalCompliance = false;
    }

    [ContextMenu("Validate")]
    public void RunValidation() {
        validator.ValidateProductionExcellence();
    }
}
```

### Multi-Phase Validation

```csharp
void ValidateMultiPhase() {
    var test = GetComponent<ProductionExcellenceStoryTest>();
    
    // Phase 1: Story Integrity
    test.EnableStoryIntegrity = true;
    test.ValidateProductionExcellence();
    
    // Phase 2: Code Coverage
    test.EnableStoryIntegrity = false;
    test.EnableCodeCoverage = true;
    test.ValidateProductionExcellence();
    
    // Phase 3: Architectural Compliance
    test.EnableCodeCoverage = false;
    test.EnableArchitecturalCompliance = true;
    test.ValidateProductionExcellence();
}
```

## StoryIntegrityValidator

### Direct API Usage

```csharp
using System.Reflection;
using TinyWalnutGames.StoryTest;

void ValidateAssembly() {
    var validator = new StoryIntegrityValidator();
    var assembly = Assembly.Load("Assembly-CSharp");
    
    var violations = validator.ValidateAssemblies(new[] { assembly });
    
    foreach (var violation in violations) {
        Debug.LogWarning($"[{violation.ActNumber}] {violation.Message}");
        Debug.LogWarning($"  Location: {violation.Location}");
    }
}
```

### Custom Validation Rules

```csharp
using TinyWalnutGames.StoryTest.Acts;

// Create custom rule
public static class Act10CustomRule {
    public static readonly ValidationRule Rule = ValidateCustomPattern;

    private static List<StoryViolation> ValidateCustomPattern(Type type) {
        var violations = new List<StoryViolation>();
        
        // Your validation logic
        if (HasViolation(type)) {
            violations.Add(new StoryViolation {
                ActNumber = 10,
                ViolationType = StoryViolationType.CustomRule,
                Message = "Custom rule violation",
                Location = type.FullName
            });
        }
        
        return violations;
    }
}
```

## DOTS/ECS Validation

### Enable ECS Checks

```csharp
var test = GetComponent<ProductionExcellenceStoryTest>();
test.EnableDOTSValidation = true; // Flags MonoBehaviour in ECS projects
```

### Detected Patterns

The framework validates:

- ❌ MonoBehaviour usage in DOTS projects
- ❌ GameObject dependencies in ECS systems
- ✅ Proper ComponentData and IComponentData usage
- ✅ Pure ECS system architecture

## Performance: Sync Point Validation

### Measuring Validation Performance

```csharp
var test = GetComponent<ProductionExcellenceStoryTest>();
test.EnableSyncPointPerformance = true;

// Validates that multi-threaded validation is faster than single-threaded
test.ValidateProductionExcellence();
```

### Custom Performance Benchmarks

```csharp
using TinyWalnutGames.StoryTest;

void BenchmarkValidation() {
    var validator = new StoryTestSyncPointValidator();
    
    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
    validator.ValidateSyncPointPerformance(assemblies);
    
    Debug.Log($"Single-threaded: {validator.SingleThreadedTime}ms");
    Debug.Log($"Multi-threaded: {validator.MultiThreadedTime}ms");
}
```

## IL Utilities

### Using StoryTestUtilities

```csharp
using TinyWalnutGames.StoryTest;

void InspectMethod(MethodInfo method) {
    byte[] il = method.GetMethodBody()?.GetILAsByteArray();
    
    if (StoryTestUtilities.ContainsThrowNotImplementedException(il)) {
        Debug.LogWarning("Method throws NotImplementedException");
    }
    
    if (StoryTestUtilities.IsOnlyDefaultReturn(method, il)) {
        Debug.LogWarning("Method only returns default value");
    }
}
```

## Exporting Reports

### Menu Export

1. `Tiny Walnut Games > The Story Test > Run Story Test and Export Report`
2. Report saved to `.debug/storytest_report.txt`

### Programmatic Export

```csharp
void ExportReport() {
    var validator = new StoryIntegrityValidator();
    var violations = validator.ValidateAssemblies(
        AppDomain.CurrentDomain.GetAssemblies()
    );
    
    var report = string.Join("\n", violations.Select(v => 
        $"[Act {v.ActNumber}] {v.Message} at {v.Location}"
    ));
    
    File.WriteAllText(".debug/report.txt", report);
}
```

## Best Practices

1. **Run validation early and often** - Catch issues during development
2. **Use CI/CD integration** - Validate on every commit
3. **Review violations carefully** - Don't blindly ignore with `[StoryIgnore]`
4. **Monitor performance** - Ensure validation doesn't slow builds
5. **Document ignored code** - Provide meaningful reasons for `[StoryIgnore]`

## Troubleshooting

### High False Positive Rate

If you see many false positives:

- Review your architecture - are there legitimate placeholders?
- Use `[StoryIgnore]` with detailed justification
- Consider disabling specific Acts via custom configuration

### Performance Issues

If validation is slow:

- Enable `EnableSyncPointPerformance` for parallel validation
- Validate only changed assemblies in CI/CD
- Use `[StoryIgnore]` on infrastructure assemblies

### IL Analysis Failures

If IL analysis throws exceptions:

- Ensure .NET Standard 2.0 compatibility
- Check for obfuscated assemblies (not supported)
- Verify Unity version compatibility (2020.3+)

## See Also

- [Quick Start Guide](QuickStart.md)
- [Assembly Structure](AssemblyStructure.md)
- [The 9 Acts Documentation](../README.md#the-9-acts)
