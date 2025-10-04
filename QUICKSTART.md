# Story Test Quick Start Guide

## For Unity Developers (In-Editor Validation)

### 1. Import the Package

Copy the `Assets/Tiny Walnut Games/TheStoryTest/` folder into your Unity project.

### 2. Run Your First Validation

**Option A - Menu:**

1. Open Unity Editor
2. Go to `Tiny Walnut Games/The Story Test/Run Story Test and Export Report`
3. Check the generated report at `.debug/storytest_report.txt`

**Option B - MonoBehaviour:**

1. Add `ProductionExcellenceStoryTest` component to any GameObject
2. Configure validation phases in Inspector
3. Right-click component → `Validate Production Excellence`

### 3. Review Violations

Violations are categorized by type:

- **IncompleteImplementation**: TODOs, placeholders, incomplete classes
- **UnusedCode**: Phantom properties, cold methods, hollow enums
- **DebuggingCode**: Debug methods not marked `[Obsolete]`
- **PrematureCelebration**: Code claiming to be complete but isn't

### 4. Fix or Seal

Either implement the missing functionality OR seal it with intent:

```csharp
public void RenderWithQuality(Scene scene, int quality) {
    // quality reserved for future rendering optimization
    scene.Draw();
}
```

---

## For DevOps/CI Engineers (Python Validator)

### 1. Install Dependencies

```bash
pip install -r requirements.txt
```

### 2. Validate a Unity Project

```bash
# From project root
python story_test.py . --verbose
```

The validator will:

- Auto-discover compiled assemblies in `Library/ScriptAssemblies/`
- Run all 9 validation Acts
- Report violations to console

### 3. Integrate with CI/CD

**GitHub Actions:**
Copy `.github/workflows/story-test.yml` to your repository and set secrets:

```secrets
UNITY_LICENSE
UNITY_EMAIL  
UNITY_PASSWORD
```

**GitLab CI:**

```yaml
story-test:
  stage: test
  script:
    - pip install -r requirements.txt
    - python story_test.py . --fail-on-violations
  artifacts:
    reports:
      junit: story-test-report.json
```

**Azure Pipelines:**

```yaml
- task: UsePythonVersion@0
  inputs:
    versionSpec: '3.11'
    
- script: |
    pip install -r requirements.txt
    python story_test.py $(Build.SourcesDirectory) --output $(Build.ArtifactStagingDirectory)/story-test.json
  displayName: 'Run Story Test Validation'
```

### 4. Fail Builds on Violations

Use `--fail-on-violations` flag:

```bash
python story_test.py . --fail-on-violations
```

Exits with code 1 if violations are found.

---

## For C# Library Developers (Non-Unity)

### 1. Compile Your Project

```bash
dotnet build --configuration Release
```

### 2. Validate Assemblies

```bash
python story_test.py ./bin/Release --verbose --output report.json
```

### 3. Integrate into Build Process

Add to your `.csproj`:

```xml
<Target Name="StoryTest" AfterTargets="Build">
  <Exec Command="python $(SolutionDir)story_test.py $(OutputPath) --fail-on-violations" />
</Target>
```

---

## Common Scenarios

### Scenario: "I have TODOs that I can't implement yet"

**Option 1 - Seal with Intent:**

```csharp
public void AdvancedFeature() {
    // Advanced physics integration planned for v2.0
    // Using basic implementation for v1.0 release
    BasicImplementation();
}
```

**Option 2 - Mark as Debug/Temporary:**

```csharp
[Obsolete("Temporary placeholder for advanced physics")]
public void AdvancedFeature() {
    throw new NotImplementedException();
}
```

**Option 3 - StoryIgnore (Last Resort):**

```csharp
[StoryIgnore("Experimental feature, not ready for validation")]
public void ExperimentalFeature() {
    throw new NotImplementedException();
}
```

### Scenario: "False positive - my method isn't empty"

The validator uses IL bytecode analysis. If your method does work but still flagged:

1. Check if it's only calling another method without any logic
2. Ensure it's not just returning a default value
3. Add meaningful logic or seal with intent comment

### Scenario: "I want to validate only specific assemblies"

**Unity (In-Editor):**
Configure `assemblyNameFilters` on `ProductionExcellenceStoryTest` component.

**Python:**

```bash
python story_test.py ./Library/ScriptAssemblies/MyGame.dll
```

### Scenario: "How do I exclude test assemblies?"

Test assemblies should use `[StoryIgnore]` attribute:

```csharp
[assembly: StoryIgnore("Test assembly")]
```

Or filter in Python:

```bash
python story_test.py . --verbose | grep -v "Tests.dll"
```

---

## Troubleshooting

### "pythonnet installation failed"

On Windows, you may need Visual C++ Build Tools:
<https://visualstudio.microsoft.com/downloads/> (Build Tools for Visual Studio)

### "No assemblies found in Unity project"

1. Open Unity Editor
2. Let it compile (check bottom-right status bar)
3. Menu > Assets > Reimport All (if necessary)
4. Check that `Library/ScriptAssemblies/` exists and contains `.dll` files

### "Getting false positives for properties"

Auto-properties are only flagged if they have suspicious names like:

- Contains "Unused", "Temp", or "Placeholder"

If you're getting false positives, rename or add `[StoryIgnore]` with justification.

### "CI/CD workflow fails with 'Unity license required'"

You need to add GitHub secrets for Unity activation. See:
<https://game.ci/docs/github/activation>

---

## Next Steps

- Read full documentation in `README.md`
- Explore the 9 Acts in `Runtime/Acts/` folder
- Check `.github/copilot-instructions.md` for AI coding agent guidance
- Contribute new validation rules via pull requests

**Philosophy reminder**: Every symbol in your codebase tells a story. Make sure it's a complete one.
