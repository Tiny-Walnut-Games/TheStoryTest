# Python Validator

The Story Test Framework includes a standalone Python validator that works without Unity, perfect for CI/CD pipelines and cross-platform validation.

## Overview

The Python validator analyzes compiled .NET assemblies using IL bytecode analysis, providing the same validation power as the Unity Editor version without requiring Unity installation.

## Two Validators

### 1. `story_test.py` (Original)
- **Purpose**: Full validation including Unity-dependent assemblies
- **Requirement**: Unity installation required for Unity assemblies
- **Use Case**: Development environments with Unity installed

### 2. `story_test_unity_safe.py` (Recommended)
- **Purpose**: Unity-safe validation without Unity dependencies
- **Requirement**: No Unity installation needed
- **Use Case**: CI/CD, cross-platform validation, production environments

## Installation

### Dependencies

```bash
# Install Python dependencies
pip install -r requirements.txt
```

### Requirements.txt
```txt
# Story Test Framework - Python Requirements
pythonnet>=3.0.0
clr-loader>=0.2.5
colorama>=0.4.6
```

## Usage

### Basic Validation

```bash
# Validate current Unity project (auto-detects assemblies)
python scripts/story_test_unity_safe.py . --verbose

# Validate specific assembly
python scripts/story_test_unity_safe.py Library/ScriptAssemblies/Assembly-CSharp.dll

# Validate directory of assemblies
python scripts/story_test_unity_safe.py path/to/assemblies/
```

### CI/CD Usage

```bash
# Fail build on violations (CI mode)
python scripts/story_test_unity_safe.py . --fail-on-violations

# Export JSON report
python scripts/story_test_unity_safe.py . --output report.json

# Combined CI usage
python scripts/story_test_unity_safe.py . --fail-on-violations --output ci-report.json
```

### Advanced Options

```bash
# Show help
python scripts/story_test_unity_safe.py --help

# Verbose output with detailed logging
python scripts/story_test_unity_safe.py . --verbose

# Quiet mode (minimal output)
python scripts/story_test_unity_safe.py . --quiet

# Custom timeout (seconds)
python scripts/story_test_unity_safe.py . --timeout 60
```

## Command Line Options

| Option | Short | Description |
|--------|-------|-------------|
| `--verbose` | `-v` | Enable detailed logging |
| `--output` | `-o` | Export JSON report to file |
| `--fail-on-violations` | | Exit with code 1 if violations found |
| `--timeout` | | Validation timeout in seconds |
| `--help` | `-h` | Show help message |

## Output Formats

### Console Output

```
[Story Test] Detected Unity project. Searching for non-Unity assemblies...
[Story Test] Found 4 non-Unity assemblies to validate
[Story Test] Loading non-Unity assembly: Library/ScriptAssemblies/MyGame.Logic.dll
[Story Test] Found 23 types to validate
[Story Test] Validation complete. Found 2 violations

================================================================================
STORY TEST UNITY-SAFE VALIDATION REPORT
================================================================================

❌ Found 2 violation(s):

  [IncompleteImplementation] PlayerController.CalculateDamage: Method contains NotImplementedException
  [UnusedCode] GameSettings.UnusedProperty: Phantom property detected

Violations by type:
  - IncompleteImplementation: 1
  - UnusedCode: 1
================================================================================
```

### JSON Report

```json
{
  "totalViolations": 2,
  "violations": [
    {
      "type": "PlayerController",
      "member": "CalculateDamage",
      "violation": "Method contains NotImplementedException",
      "filePath": "Library/ScriptAssemblies/MyGame.Logic.dll",
      "lineNumber": 0,
      "violationType": "IncompleteImplementation"
    }
  ],
  "violationsByType": {
    "IncompleteImplementation": 1,
    "UnusedCode": 1
  }
}
```

## Assembly Detection

### Unity Project Detection

The validator automatically detects Unity projects by looking for:
- `Assets/` directory
- `ProjectSettings/` directory
- `Library/ScriptAssemblies/` directory

### Assembly Discovery

```bash
# Auto-discovery in Unity project
python scripts/story_test_unity_safe.py /path/to/unity/project

# Manual assembly specification
python scripts/story_test_unity_safe.py /path/to/assembly.dll

# Directory scanning
python scripts/story_test_unity_safe.py /path/to/assemblies/
```

### Assembly Filtering

The validator automatically filters assemblies:

**Included by Default:**
- User assemblies (non-Unity)
- Custom game logic assemblies
- Third-party libraries

**Excluded by Default:**
- Unity engine assemblies (`UnityEngine.*`)
- Unity editor assemblies (`UnityEditor.*`)
- System assemblies (`System.*`, `Mono.*`)
- Test assemblies (configurable)

## Unity-Safe Features

### Dependency Resolution

The Unity-safe validator handles Unity dependencies gracefully:

```bash
# Skips Unity-dependent assemblies automatically
[Story Test] Skipping Unity-dependent assembly: TinyWalnutGames.TheStoryTest.dll

# Validates non-Unity assemblies successfully
[Story Test] Loading non-Unity assembly: TinyWalnutGames.TheStoryTest.Shared.dll
```

### Compiler Artifact Filtering

Automatically filters out compiler-generated artifacts:

- Enum interface methods (`HasFlag`, `CompareTo`)
- Delegate serialization methods (`GetObjectData`, `CombineImpl`)
- `IConvertible` implementations
- Constructor/finalizer methods
- Test assembly methods

### Cross-Platform Compatibility

Works on all platforms without Unity:

- **Windows**: Native .NET runtime
- **Linux**: CoreCLR runtime
- **macOS**: CoreCLR runtime
- **Docker**: Container-friendly

## Performance

### Optimization Features

- **CoreCLR Runtime**: Better performance than Mono
- **Assembly Caching**: Avoids repeated loading
- **Parallel Validation**: Multiple assemblies concurrently
- **IL Analysis**: Fast bytecode inspection

### Performance Tuning

```bash
# Limit concurrent validations (reduce memory usage)
export STORY_TEST_MAX_CONCURRENT=2

# Increase timeout for large projects
python scripts/story_test_unity_safe.py . --timeout 120

# Use specific assembly for faster validation
python scripts/story_test_unity_safe.py Library/ScriptAssemblies/MyGame.Core.dll
```

### Benchmarks

| Project Size | Assemblies | Types | Validation Time |
|-------------|------------|-------|-----------------|
| Small | 2-3 | <50 | <2 seconds |
| Medium | 5-8 | 50-200 | 5-10 seconds |
| Large | 10+ | 200+ | 10-30 seconds |

## Integration Examples

### GitHub Actions

```yaml
- name: Run Story Test Validation
  run: |
    python scripts/story_test_unity_safe.py . --fail-on-violations --output report.json
```

### Pre-commit Hook

```bash
#!/bin/sh
# .git/hooks/pre-commit
python scripts/story_test_unity_safe.py . --fail-on-violations
```

### Docker Integration

```dockerfile
FROM python:3.11-slim
COPY requirements.txt .
RUN pip install -r requirements.txt
COPY scripts/ ./scripts/
COPY . .
CMD ["python", "scripts/story_test_unity_safe.py", ".", "--fail-on-violations"]
```

## Configuration

### Environment Variables

```bash
# Python runtime configuration
export PYTHONPATH="/path/to/project"

# Validation settings
export STORY_TEST_TIMEOUT=60
export STORY_TEST_VERBOSE=true
export STORY_TEST_MAX_CONCURRENT=4
```

### Settings File

The validator respects `StoryTestSettings.json` when available:

```json
{
  "assemblyFilters": {
    "include": ["MyGame.*"],
    "exclude": ["*.Tests"]
  },
  "acts": {
    "enableAct1": true,
    "enableAct2": true
  }
}
```

## Troubleshooting

### Common Issues

**ImportError: No module named 'clr'**
```bash
# Install missing dependencies
pip install -r requirements.txt

# Verify pythonnet installation
python -c "import clr; print('pythonnet ready')"
```

**Assembly loading failures**
```bash
# Use Unity-safe validator
python scripts/story_test_unity_safe.py . --verbose

# Check assembly paths
python scripts/story_test_unity_safe.py Library/ScriptAssemblies/
```

**Performance issues**
```bash
# Reduce concurrency
export STORY_TEST_MAX_CONCURRENT=1

# Increase timeout
python scripts/story_test_unity_safe.py . --timeout 120
```

**Memory issues**
```bash
# Validate specific assemblies only
python scripts/story_test_unity_safe.py Library/ScriptAssemblies/Core.dll

# Use 64-bit Python
python3 -c "import sys; print('64-bit:', sys.maxsize > 2**32)"
```

### Debug Mode

```bash
# Enable verbose logging
python scripts/story_test_unity_safe.py . --verbose

# Check assembly loading
python -c "
import clr
clr.AddReference('path/to/assembly.dll')
print('Assembly loaded successfully')
"
```

### Platform-Specific Issues

**Windows**
- Ensure .NET Framework 4.7.2+ is installed
- Use 64-bit Python for large assemblies

**Linux**
- Install CoreCLR dependencies: `sudo apt-get install libc6 libicu`
- Use Python 3.8+ for better compatibility

**macOS**
- Install Xcode command line tools
- Use Python 3.8+ from python.org or Homebrew

## Best Practices

### 1. Use Unity-Safe Validator
- Prefer `story_test_unity_safe.py` for CI/CD
- Works without Unity installation
- Handles cross-platform differences

### 2. Optimize Performance
- Use assembly filtering for large projects
- Enable parallel validation for multiple assemblies
- Cache results between runs

### 3. Integrate Early
- Add to pre-commit hooks
- Include in CI/CD pipelines
- Set up failure notifications

### 4. Monitor Results
- Track violation trends over time
- Alert on increasing violation counts
- Use JSON reports for automation

## Advanced Usage

### Custom Validation Scripts

```python
#!/usr/bin/env python3
import subprocess
import sys
import json

def run_validation():
    result = subprocess.run([
        'python', 'scripts/story_test_unity_safe.py',
        '.', '--output', 'report.json'
    ], capture_output=True, text=True)

    if result.returncode != 0:
        print("Validation failed:")
        print(result.stdout)
        print(result.stderr)
        sys.exit(1)

    with open('report.json') as f:
        report = json.load(f)

    return report['totalViolations']

if __name__ == '__main__':
    violations = run_validation()
    print(f"Found {violations} violations")
```

### Batch Processing

```bash
#!/bin/bash
# Validate multiple projects
for project in /path/to/projects/*/; do
    echo "Validating $project"
    python scripts/story_test_unity_safe.py "$project" --output "$project/report.json"
done
```

The Python validator provides flexible, cross-platform code quality validation that integrates seamlessly into any development workflow.