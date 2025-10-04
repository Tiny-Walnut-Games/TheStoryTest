# The Story Test Framework

[![Story Test Validation](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml/badge.svg?branch=jmeyer1980%2Fissue2)](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml)

## **Symbol Integrity & Narrative Completeness for C# Projects**

A code quality validation framework that enforces the "Story Test Doctrine": every symbol (method, property, parameter, enum) must be fully implemented and meaningful—no placeholders, TODOs, or unused code in production.

Originally designed for Unity ECS/DOTS projects, Story Test is now **Unity-agnostic** and works with any C# codebase, including GameObject-based Unity projects and pure .NET applications.

> **📦 Package-Based Distribution**: This repository now uses Unity Package Manager (UPM) format for easy integration into your projects. The framework code is in `Packages/com.tinywalnutgames.storytest/`.

## 🎯 Philosophy

Think of your code as a narrative where every element must serve a purpose. The Story Test Framework uses IL bytecode analysis to detect:

- ❌ TODO comments and `NotImplementedException`
- ❌ Placeholder methods returning only default values
- ❌ Unused properties, methods, and enum values
- ❌ Debug code not marked as temporary
- ❌ Incomplete class implementations
- ❌ Code claiming to be complete but isn't

## 🚀 Quick Start

### Unity Projects - Package Installation

## **Option 1: Via Git URL (Recommended)**

Add this to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.tinywalnutgames.storytest": "https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest"
  }
}
```

## **Option 2: Via Unity Package Manager UI**

1. Open Unity Package Manager (`Window > Package Manager`)
2. Click the `+` button → "Add package from git URL..."
3. Enter: `https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest`

## **Option 3: Manual Installation**

1. Download from [Releases](https://github.com/jmeyer1980/TheStoryTest/releases)
2. Extract to your project's `Packages/` folder
3. Unity will auto-import

### Using the Framework

Once installed, run validation via Unity menu:

`Tiny Walnut Games/The Story Test/Run Story Test and Export Report`

Or add the `ProductionExcellenceStoryTest` MonoBehaviour to a GameObject and run via Context Menu.

### Standalone Python Validator (CI/CD)

**Prerequisites:**

```bash
pip install -r requirements.txt
```

**Usage:**

```bash
# Validate Unity project (finds compiled assemblies automatically)
python story_test.py /path/to/UnityProject --verbose

# Validate specific assembly
python story_test.py MyAssembly.dll --fail-on-violations

# Validate directory of DLLs
python story_test.py ./bin/Release --output report.json
```

**Cross-platform compatible:** Windows, Linux, macOS

### GitHub Actions CI/CD

The repository includes a ready-to-use workflow (`.github/workflows/story-test.yml`) that:

- ✅ Runs on Windows, Linux, and macOS
- ✅ Compiles Unity projects automatically
- ✅ Generates violation reports
- ✅ Includes per-violation file paths and line numbers in the job summary (verified in run #37)
- ✅ Fails builds when violations are found
- ✅ Posts results to PR summaries

**Setup:**

1. Copy `.github/workflows/story-test.yml` to your repository
2. Set Unity secrets: `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`
3. Push to trigger validation

#### Known Limitations

- **Unity Editor tests remain experimental in headless CI.** GameCI's builder currently omits `UnityEditor.*` modules from the player build, so invoking the Editor Test Runner in workflows logs `FileNotFoundException: Could not load file or assembly 'UnityEditor.CoreModule'`. Treat editor-mode tests as opt-in and prefer playmode or standalone assembly coverage until GameCI ships full Editor support.
- The Story Test workflow still validates assemblies successfully—the Python validator runs against the generated player assemblies, which include all runtime IL needed for Acts 1-9.

## 📋 The 9 Acts of Validation

The framework implements 9 validation "Acts" (rules) that analyze IL bytecode:

| Act | Name | Detects |
|-----|------|---------|
| **Act 1** | TODO Comments | `NotImplementedException` and placeholder returns |
| **Act 2** | Placeholder Implementations | Stub methods with minimal IL (≤10 bytes) |
| **Act 3** | Incomplete Classes | Non-abstract classes with unimplemented abstract methods |
| **Act 4** | Unsealed Abstract Members | Abstract methods in non-abstract classes |
| **Act 5** | Debug-Only Implementations | Debug/test methods without `[Obsolete]` attribute |
| **Act 6** | Phantom Props | Auto-properties that are never meaningfully used |
| **Act 7** | Cold Methods | Empty methods (just `ret` instruction) |
| **Act 8** | Hollow Enums | Enums with ≤1 values or placeholder names |
| **Act 9** | Premature Celebrations | Code marked complete but still throwing exceptions |

## 🔧 Development Workflow

### Adding New Validation Rules

1. Create `ActXYourRule.cs` in `Assets/Tiny Walnut Games/TheStoryTest/Runtime/Acts/`
2. Implement the validation rule:

    ```csharp
    using System.Reflection;
    using TinyWalnutGames.StoryTest.Shared;

    namespace TinyWalnutGames.StoryTest.Acts
    {
        [StoryIgnore("Story test validation infrastructure")]
        public static class ActXYourRule
        {
            public static readonly ValidationRule Rule = CheckYourCondition;
            
            private static bool CheckYourCondition(MemberInfo member, out string violation)
            {
                violation = null;
                
                // Your validation logic here
                if (/* condition */)
                {
                    violation = "Description of violation";
                    return true;
                }
                
                return false;
            }
        }
    }
    ```

3. Rules are auto-discovered via reflection on Editor load (no manual registration needed)

### Opting Out: `[StoryIgnore]` Attribute

Use sparingly for test infrastructure and Unity Editor-only code:

```csharp
[StoryIgnore("Infrastructure component for story test validation")]
public class ProductionExcellenceStoryTest : MonoBehaviour { }
```

**Requirements:**

- ✅ MUST provide non-empty reason string
- ✅ Use only for technical infrastructure
- ❌ Don't use to bypass legitimate code quality issues

#### Infrastructure Usage

Core framework symbols that exist purely to report violations—like `StoryViolationShared` and `StoryIgnoreAttribute`—carry `[StoryIgnore("Story test validation infrastructure")]` so the validator never flags itself. Follow the same pattern for future infrastructure helpers and always document the exemption reason inline.

#### Educational Comment Shield

Documentation may reference violation keywords ("TODO", "placeholder", etc.) when teaching best practices. Prefix those words with the 🏳 emoji to signal "education only" and keep Acts 1 & 2 from firing on your docs or inline guidance. Example: `// Use IL analysis to detect 🏳placeholder implementations`.

### Sealing Unused Parameters

When parameters can't be used immediately, document intent instead of deleting:

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

## 🏗️ Architecture

### Unity Package Structure

```ts
Assets/Tiny Walnut Games/TheStoryTest/
├── Runtime/
│   ├── Acts/                  # The 9 validation rule acts
│   │   ├── Act1TodoComments.cs
│   │   ├── Act2PlaceholderImplementations.cs
│   │   └── ...
│   └── Shared/                # Shared types and utilities
│       ├── StoryIgnoreAttribute.cs
│       ├── StoryViolationShared.cs
│       └── StoryTestSharedTypes.cs
├── Editor/
│   ├── StoryTestExportMenu.cs
│   └── StrengtheningValidationSuite.cs
├── Tests/
│   └── StoryTestValidationTests.cs
├── StoryIntegrityValidator.cs
├── ProductionExcellenceStoryTest.cs
└── StoryTestRuleBootstrapper.cs
```

### Key Components

- **StoryIntegrityValidator**: Central orchestrator that runs all validation rules
- **StoryTestRuleBootstrapper**: Auto-discovers and registers rules at Editor load
- **ProductionExcellenceStoryTest**: MonoBehaviour for multi-phase async validation
- **IL Analysis Utilities**: Bytecode pattern detection (`StoryTestUtilities.cs`)

### Python Validator Architecture

The standalone Python validator (`story_test.py`) uses:

- **pythonnet**: For .NET reflection and IL bytecode access
- **ILAnalyzer**: Pattern detection for common violations
- **StoryTestValidator**: Main validation engine implementing all 9 Acts

## 🧪 Testing

### Unity Tests (NUnit)

```bash
# In Unity Test Runner
Window > General > Test Runner > Run All
```

### Python Validator Tests

```bash
# Validate the Story Test framework itself
python story_test.py ./Library/ScriptAssemblies --verbose
```

## 📦 Installation

### For Unity Projects

1. Clone or download this repository
2. Copy `Assets/Tiny Walnut Games/TheStoryTest/` into your Unity project
3. Unity will auto-compile and register validation rules

### For Python Validator

```bash
# Clone repository
git clone https://github.com/jmeyer1980/TheStoryTest.git
cd TheStoryTest

# Install dependencies
pip install -r requirements.txt

# Run validation
python story_test.py /path/to/your/unity/project
```

## 🤝 Contributing

Contributions welcome! Please:

1. Follow the Story Test Doctrine in your own code
2. Add tests for new validation rules
3. Update documentation for new features
4. Ensure cross-platform compatibility (Windows/Linux/macOS)

## 📄 License

See [LICENSE](LICENSE) file for details.

## 🔗 Resources

- **GitHub Repository**: [https://github.com/jmeyer1980/TheStoryTest](https://github.com/jmeyer1980/TheStoryTest)
- **Unity Asset Store**: *(Coming soon)*
- **Documentation**: See `.github/copilot-instructions.md` for AI coding agent guidance

## 📘 Phase 3: Documentation & Polish

We're now focusing on richer guidance, storyteller-friendly documentation, and improved developer ergonomics. Track progress and upcoming tasks in `PHASE3_PLAN.md`, including:

- Consolidating docs into `Documentation~/` for UPM consumption
- Capturing CI nuances (file/line reporting, editor test limitations)
- Expanding tutorials and storytelling examples that highlight each Act

Phase 3 builds on the stable CI foundation established in run #37—no violations reported across Windows, macOS, and Linux.

---

**Remember**: Every symbol matters. No placeholders, no TODOs in production. Seal unused elements with intent, never silence.
