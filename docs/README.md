# The Story Test Framework

A comprehensive code quality validation framework for Unity and .NET projects that enforces **"Story Test Doctrine"**: every symbol (method, property, parameter, enum) must be fully implemented and meaningful—no placeholders, TODOs, or unused code in production.

## 🚀 Quick Start

### Installation

#### Unity Package Manager
1. Open Unity Package Manager (`Window > Package Manager`)
2. Click the `+` button → **Add package from git URL...**
3. Enter: `https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest`

#### Git URL
Add to your project's `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.tinywalnutgames.storytest": "https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest"
  }
}
```

### First Validation

#### Unity Editor
1. Open menu: `Tiny Walnut Games > The Story Test > Run Story Test and Export Report`
2. Check the Console for results
3. Find detailed report at `.debug/storytest_report.txt`

#### Python Validator (Standalone)
```bash
# Install dependencies
pip install -r requirements.txt

# Validate Unity project
python scripts/story_test_unity_safe.py . --verbose

# CI/CD usage
python scripts/story_test_unity_safe.py . --fail-on-violations --output report.json
```

## 📚 Documentation

- [**Getting Started**](getting-started.md) - Installation and first validation
- [**The 9+ Acts**](acts.md) - Complete validation rules reference
- [**Configuration**](configuration.md) - Settings and customization
- [**CI/CD Integration**](ci-cd.md) - GitHub Actions and automation
- [**Python Validator**](python-validator.md) - Standalone validation
- [**API Reference**](api.md) - Programming interfaces
- [**Troubleshooting**](troubleshooting.md) - Common issues and solutions

## 🎯 Core Features

### The 11 Acts Validation System
Each "Act" performs IL bytecode analysis to detect specific code quality issues:

1. **Act 1**: Todo Comments - `NotImplementedException` detection
2. **Act 2**: Placeholder Implementations - Minimal IL detection
3. **Act 3**: Incomplete Classes - Abstract method implementation
4. **Act 4**: Unsealed Abstract Members - Abstract method sealing
5. **Act 5**: Debug Only Implementations - `[Obsolete]` requirement
6. **Act 6**: Phantom Props - Unused auto-properties
7. **Act 7**: Cold Methods - Empty/minimal methods
8. **Act 8**: Hollow Enums - Minimal enum values
9. **Act 9**: Premature Celebrations - Complete but throwing
10. **Act 10**: Suspiciously Simple Methods - Constant returns
11. **Act 11**: Dead Code - Unused fields/properties/methods

### Cross-Platform Support
- ✅ Unity Editor (Windows, macOS, Linux)
- ✅ Standalone Python (Windows, macOS, Linux)
- ✅ CI/CD pipelines (GitHub Actions, Azure DevOps, GitLab CI)
- ✅ Pure .NET projects (no Unity required)

## 🏗️ Architecture

```
TheStoryTest/
├── Runtime/                    # Core validation logic
│   ├── Acts/                  # 11 validation rules
│   └── Shared/                # Unity-agnostic types
├── Editor/                     # Unity Editor integration
├── Tests/                      # NUnit test suite
└── Documentation~/             # Unity package docs
```

## 🎮 Examples

### Basic Usage
```csharp
// ❌ BAD: Placeholder implementation
public float CalculateScore() {
    throw new NotImplementedException();
}

// ✅ GOOD: Complete implementation
public float CalculateScore() {
    return _player.CalculateTotalScore();
}
```

### Opt-Out Mechanism
```csharp
[StoryIgnore("Test infrastructure component")]
public class TestValidationHelper : MonoBehaviour { }
```

## 🔧 Requirements

- **Unity**: 2020.3 LTS or later
- **.NET**: Standard 2.0 or later
- **Python**: 3.8+ (for standalone validator)
- **Tests**: NUnit (included with Unity)

## 📄 License

MIT License - see [LICENSE](../LICENSE) for details.

## 🤝 Contributing

Contributions welcome! Please see [CONTRIBUTING.md](contributing.md) for guidelines.

## 🆘 Support

- [GitHub Issues](https://github.com/jmeyer1980/TheStoryTest/issues)
- [Discussions](https://github.com/jmeyer1980/TheStoryTest/discussions)
- [Documentation](https://github.com/jmeyer1980/TheStoryTest/tree/main/docs)

---

**Remember**: Every symbol in your assembly should read like a finished chapter. If a parameter, method, or enum feels like foreshadowing, seal it with intent or finish the scene before shipping.