# Story Test Framework — Unity & Standalone C# Code Quality Validation

## Philosophy: Symbol Integrity & Narrative Completeness

A Unity Editor tool AND standalone Python validator that enforces **"Story Test Doctrine"**: every symbol (method, property, parameter, enum) must be fully implemented and meaningful—no placeholders, TODOs, or unused code in production. Think of your code as a narrative where every element must serve a purpose.

**Now Unity-agnostic**: Works with GameObject-based projects, ECS/DOTS, and pure .NET C# assemblies.

## Core Architecture

### The "9 Acts" Validation System
Located in `Assets/Tiny Walnut Games/TheStoryTest/Runtime/Acts/*.cs`, each "Act" is a static class with a `ValidationRule` delegate that performs IL bytecode analysis to detect code quality issues:

- **Act1TodoComments**: Detects `NotImplementedException` and methods returning only defaults
- **Act2PlaceholderImplementations**: Catches stub methods with minimal IL (≤10 bytes)
- **Act3IncompleteClasses**: Ensures non-abstract classes implement all abstract methods
- **Act4UnsealedAbstractMembers**: Prevents abstract methods in non-abstract classes
- **Act5DebugOnlyImplementations**: Requires `[Obsolete]` on debug/test methods
- **Act6PhantomProps**: Identifies auto-properties that are never meaningfully used
- **Act7ColdMethods**: Finds empty or minimal methods (just `ret` instruction)
- **Act8HollowEnums**: Catches enums with ≤1 values or placeholder names
- **Act9PrematureCelebrations**: Detects code marked complete but still throwing `NotImplementedException`

### Key Components

**StoryIntegrityValidator** (`StoryIntegrityValidator.cs`): Central validator that orchestrates all Acts
- Dynamically loads rules via reflection from `TinyWalnutGames.StoryTest.Acts` assembly
- `ValidateAssemblies(Assembly[])` → returns `List<StoryViolation>`
- Rules registered at Editor load time by `StoryTestRuleBootstrapper`

**ProductionExcellenceStoryTest** (`ProductionExcellenceStoryTest.cs`): MonoBehaviour for async validation
- Multi-phase validation: StoryIntegrity, CodeCoverage, ArchitecturalCompliance, ProductionReadiness, SyncPointPerformance
- Validates against DOTS/ECS patterns (flags improper MonoBehaviour usage) when enabled
- Run via `[ContextMenu("Validate Production Excellence")]` or on Start

**Python Standalone Validator** (`story_test.py`): Cross-platform CLI tool
- Uses `pythonnet` for .NET reflection and IL bytecode analysis
- No Unity runtime required - validates any compiled .NET assembly
- Auto-discovers Unity project assemblies in `Library/ScriptAssemblies/`
- Implements all 9 Acts in pure Python

**StoryIgnoreAttribute** (`Runtime/Shared/StoryIgnoreAttribute.cs`): Opt-out mechanism
```csharp
[StoryIgnore("Infrastructure component for story test validation")]
public class ProductionExcellenceStoryTest : MonoBehaviour { }
```
- **MUST** provide non-empty reason string (enforced by constructor)
- Use sparingly—only for test infrastructure and Unity Editor-only code

## Critical Coding Rules

### 1. Every Parameter Must Be Used
```csharp
// ❌ BAD: quality parameter ignored
public void RenderScene(Scene scene, Camera camera, int quality) {
    scene.Draw(camera);
}

// ✅ GOOD: All parameters consumed
public void RenderScene(Scene scene, Camera camera, int quality) {
    scene.Draw(camera, quality);
}

// ✅ ACCEPTABLE: Sealed with intent
public void RenderScene(Scene scene, Camera camera, int quality) {
    // quality reserved for future fidelity scaling
    scene.Draw(camera);
}
```

### 2. No Placeholder Implementations
```csharp
// ❌ Act1 violation
public float Calculate() {
    throw new NotImplementedException(); // Detected via IL analysis
}

// ❌ Act2 violation  
public int GetValue() {
    return 0; // Only default return, detected by IsOnlyDefaultReturn()
}
```

### 3. Debug Methods Must Be Marked Temporary
```csharp
// ❌ Act5 violation
public void DebugDrawGizmos() { }

// ✅ Correct
[Obsolete("Debug visualization only")]
public void DebugDrawGizmos() { }
```

### 4. Unity Dependencies are Abstracted
The framework uses conditional compilation to work outside Unity:
```csharp
#if UNITY_EDITOR || UNITY_ENGINE
using Debug = UnityEngine.Debug;
#else
using Debug = System.Diagnostics.Debug;
#endif
```
- Core validation logic in `Runtime/Acts/` has NO Unity dependencies
- Only `ProductionExcellenceStoryTest` and Editor tools use Unity APIs
- Python validator reimplements all Acts without Unity runtime

## Development Workflow

### Running Validation (Unity)
1. **Menu**: `Tiny Walnut Games/The Story Test/Run Story Test and Export Report` → exports to `.debug/storytest_report.txt`
2. **In-Scene**: Add `ProductionExcellenceStoryTest` MonoBehaviour, configure phases, run via Context Menu
3. **Tests**: `StoryTestValidationTests.cs` contains NUnit integration tests

### Running Validation (Standalone Python)
```bash
# Install dependencies first
pip install -r requirements.txt

# Validate Unity project (auto-finds assemblies)
python story_test.py /path/to/UnityProject --verbose

# Validate specific DLL
python story_test.py MyAssembly.dll --fail-on-violations

# Export JSON report
python story_test.py . --output report.json
```

### CI/CD Integration (GitHub Actions)
The repository includes `.github/workflows/story-test.yml` with:
- Cross-platform matrix: Windows, Linux, macOS
- Auto-compiles Unity projects via `game-ci/unity-builder`
- Runs Python validator on compiled assemblies
- Generates violation reports in PR summaries
- Fails builds when violations found

**Setup Requirements:**
- Set GitHub secrets: `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`
- Install `requirements.txt` in workflow
- Configure Unity version in workflow matrix

### IL Analysis Utilities
`StoryTestUtilities.cs` / Python `ILAnalyzer` class provide helpers:
- `ContainsThrowNotImplementedException(byte[])`: Detects opcodes `0x73` + `0x7A`
- `IsOnlyDefaultReturn(MethodInfo, byte[])`: Checks for `ldc.i4.*` + `ret` patterns

### Adding New Validation Rules

**C# (Unity Package):**
1. Create `ActXYourRule.cs` in `Assets/Tiny Walnut Games/TheStoryTest/Runtime/Acts/`
2. Implement `public static readonly ValidationRule Rule = YourCheckMethod;`
3. `StoryTestRuleBootstrapper` auto-discovers via reflection on Editor load

**Python (Standalone):**
1. Add validation method to `StoryTestValidator` class in `story_test.py`
2. Call from `_validate_method()` or `_validate_type()` as appropriate
3. Return `StoryViolation` instances for failures

## Project Structure

```
TheStoryTest/
├── .github/
│   ├── copilot-instructions.md          # This file
│   └── workflows/
│       └── story-test.yml               # GitHub Actions CI/CD
├── Assets/Tiny Walnut Games/TheStoryTest/
│   ├── Runtime/
│   │   ├── Acts/                        # 9 validation rule Acts
│   │   │   ├── Act1TodoComments.cs
│   │   │   ├── Act2PlaceholderImplementations.cs
│   │   │   └── ...
│   │   └── Shared/                      # Shared types (no Unity deps)
│   │       ├── StoryIgnoreAttribute.cs
│   │       ├── StoryViolationShared.cs
│   │       └── StoryTestSharedTypes.cs
│   ├── Editor/
│   │   ├── StoryTestExportMenu.cs
│   │   └── StrengtheningValidationSuite.cs
│   ├── Tests/
│   │   └── StoryTestValidationTests.cs
│   ├── StoryIntegrityValidator.cs       # Central validator
│   ├── ProductionExcellenceStoryTest.cs # Unity MonoBehaviour
│   └── StoryTestRuleBootstrapper.cs     # Auto-registration
├── story_test.py                        # Standalone Python validator
├── requirements.txt                     # Python dependencies
└── README.md                            # Full documentation
```

## Unity Project Context
- **Target**: Any Unity project (GameObject, ECS/DOTS, or hybrid)
- **Assembly**: Validation rules live in `TinyWalnutGames.StoryTest.Acts` assembly
- **Testing**: Uses NUnit (`using NUnit.Framework`)
- **Python**: Uses pythonnet for .NET interop

## Anti-Patterns to Avoid
- Generic TODO comments without implementation plans
- Unused enum values or properties (Act6/Act8 will catch these)
- Empty methods that serve no purpose (Act7)
- Claiming code is complete while it's not (Act9)
- Deleting symbols instead of sealing/documenting them
- Using `[StoryIgnore]` to bypass legitimate quality issues

## Key Files Reference
- `StoryTestRuleBootstrapper.cs`: `[InitializeOnLoad]` runtime registration
- `StoryViolationShared.cs`: Defines `StoryViolation` and `StoryViolationType` enums
- `StoryTestSyncPointValidator.cs`: Performance testing for parallel validation
- `StrengtheningValidationSuite.cs`: Additional validation layers
- `story_test.py`: Standalone Python validator with IL bytecode analysis
- `.github/workflows/story-test.yml`: GitHub Actions CI/CD configuration

## Python Validator Details

**IL Bytecode Patterns Detected:**
- `0x73` + `0x7A` at offset +5: `throw new NotImplementedException()`
- `0x14-0x17` + `0x2A`: Load constant + return (default value only)
- IL length ≤3: Empty method (just `ret`)
- IL length ≤10: Minimal/placeholder implementation

**Command-Line Options:**
```bash
python story_test.py <path> [options]
  <path>              Unity project, assembly DLL, or directory
  -v, --verbose       Enable detailed logging
  -o, --output FILE   Export JSON report
  --fail-on-violations Exit with code 1 if violations found (for CI)
```

---

**Remember**: When AI suggests code completions, ensure every symbol is meaningful and contributes to the "narrative" of the codebase. Seal unused elements with explicit comments rather than deleting context. The framework now works both in Unity Editor and in CI/CD pipelines via Python.

