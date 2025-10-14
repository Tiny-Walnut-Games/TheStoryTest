# The 11 Acts: Complete Validation Rules

The Story Test Framework implements 11 validation "Acts" that analyze IL bytecode to detect code quality issues. Each Act targets specific anti-patterns that violate the "Story Test Doctrine" - every symbol must be meaningful and complete.

## Act 1: Todo Comments

**Detects**: `NotImplementedException` and methods returning only defaults

### What it Catches
```csharp
// ❌ VIOLATION
public float CalculateDamage() {
    throw new NotImplementedException(); // Detected via IL analysis
}

// ❌ VIOLATION
public int GetScore() {
    return 0; // Only default return, detected by IL pattern
}
```

### Why it Matters
Incomplete implementations break the narrative - they're plot holes in your code story.

---

## Act 2: Placeholder Implementations

**Detects**: Stub methods with minimal IL (≤10 bytes)

### What it Catches
```csharp
// ❌ VIOLATION - Minimal IL bytecode
public void ProcessData() {
    // Empty or just return statement
}

// ❌ VIOLATION - Too simple to be meaningful
public bool IsValid() => true;
```

### Why it Matters
Placeholder methods suggest unfinished logic or forgotten implementation.

---

## Act 3: Incomplete Classes

**Detects**: Non-abstract classes that don't implement all abstract methods

### What it Catches
```csharp
// ❌ VIOLATION
public class PlayerController : BaseController {
    // Missing implementation of abstract method from BaseController
    // public override void Update() { /* not implemented */ }
}
```

### Why it Matters
Incomplete classes can't fulfill their contract and will cause runtime errors.

---

## Act 4: Unsealed Abstract Members

**Detects**: Abstract methods in non-abstract classes

### What it Catches
```csharp
// ❌ VIOLATION
public class DataManager {
    public abstract void SaveData(); // Abstract method in concrete class
}
```

### Why it Matters
Abstract methods in concrete classes create inheritance confusion and design inconsistencies.

---

## Act 5: Debug Only Implementations

**Detects**: Debug/test methods without `[Obsolete]` attribute

### What it Catches
```csharp
// ❌ VIOLATION
public void DebugDrawGizmos() {
    // Debug visualization code
}

// ✅ CORRECT
[Obsolete("Debug visualization only")]
public void DebugDrawGizmos() {
    // Debug visualization code
}
```

### Why it Matters
Debug code in production creates confusion and potential performance issues.

---

## Act 6: Phantom Props

**Detects**: Auto-properties that are never meaningfully used

### What it Catches
```csharp
// ❌ VIOLATION
public class GameSettings {
    public int UnusedSetting { get; set; } // Never read or written meaningfully
    public int UsedSetting { get; set; } // Actually used in game logic
}
```

### Why it Matters
Unused properties suggest dead code or forgotten functionality.

---

## Act 7: Cold Methods

**Detects**: Empty or minimal methods (just `ret` instruction)

### What it Catches
```csharp
// ❌ VIOLATION
public void Initialize() {
    // Completely empty method body
}

// ❌ VIOLATION
private void Cleanup() {
    return; // Only return statement
}
```

### Why it Matters
Empty methods serve no purpose and clutter the codebase.

---

## Act 8: Hollow Enums

**Detects**: Enums with ≤1 values or placeholder names

### What it Catches
```csharp
// ❌ VIOLATION - Single value
public enum GameState {
    Ready
}

// ❌ VIOLATION - Placeholder names
public enum Status {
    Value1,
    Value2,
    Value3
}

// ✅ CORRECT
public enum GameState {
    Ready,
    Playing,
    Paused,
    GameOver
}
```

### Why it Matters
Hollow enums don't provide meaningful categorization.

---

## Act 9: Premature Celebrations

**Detects**: Code marked complete but still throwing `NotImplementedException`

### What it Catches
```csharp
// ❌ VIOLATION
[ReadyForProduction]
public class AdvancedAI {
    public void MakeDecision() {
        throw new NotImplementedException(); // Claims ready but isn't
    }
}
```

### Why it Matters
Marking incomplete code as complete creates false confidence.

---

## Act 10: Suspiciously Simple Methods

**Detects**: Methods that only return constants/null/default values

### What it Catches
```csharp
// ❌ VIOLATION
public float GetDifficulty() {
    return 1.0f; // Always returns same constant
}

// ❌ VIOLATION
public GameObject GetPlayer() {
    return null; // Always returns null
}
```

### Why it Matters
Constant-return methods suggest incomplete implementation or placeholder logic.

---

## Act 11: Dead Code

**Detects**: Fields, properties, and methods that are never used

### What it Catches
```csharp
// ❌ VIOLATION
public class Player {
    private int _unusedField; // Written but never read
    private string UnusedProperty { get; set; } // Never accessed

    private void UnusedMethod() { // Never called
        Debug.Log("This is never executed");
    }
}
```

### Why it Matters
Dead code clutters the codebase and creates maintenance burden.

---

## Opt-Out Mechanism

Use `[StoryIgnore]` to exclude legitimate exceptions:

```csharp
[StoryIgnore("Required by Unity serialization system")]
public class SerializableData : MonoBehaviour {
    // Unity-specific implementation that violates Acts but is necessary
}
```

**Note**: Always provide a non-empty reason explaining why the violation is intentional.

---

## Understanding IL Analysis

The framework doesn't rely on source code parsing - it analyzes **IL bytecode**:

- **Platform Independent**: Works on any .NET assembly
- **Compiler Agnostic**: Catches issues regardless of coding style
- **Runtime Accurate**: Analyzes what actually gets executed
- **Dependency Free**: Doesn't need source code or symbols

### IL Patterns Detected

- `0x73` + `0x7A`: `throw new NotImplementedException()`
- `0x14-0x17` + `0x2A`: Load constant + return (default value)
- IL length ≤3: Empty method (just `ret`)
- IL length ≤10: Minimal/placeholder implementation

---

## Configuration

Control which Acts run in your `StoryTestSettings.json`:

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

---

## Best Practices

1. **Fix Act 1 violations first** - These are critical incomplete implementations
2. **Review Act 11 violations carefully** - Some dead code may be intentional (API contracts)
3. **Use `[StoryIgnore]` sparingly** - Only for legitimate exceptions
4. **Focus on narrative coherence** - Each symbol should tell part of your application's story

Remember: The goal isn't blind compliance, but meaningful, complete code that serves its purpose.