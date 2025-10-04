# Quick Start Guide

Welcome to **The Story Test Framework**! This guide walks you from installation to your first validation run—whether you stay inside the Unity Editor or drive everything from CI.

## 1. Install the package

### Git URL (recommended)

Add the dependency to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.tinywalnutgames.storytest": "https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest"
  }
}
```

### Unity Package Manager UI

1. Open `Window > Package Manager`
2. Click the `+` button → **Add package from git URL...**
3. Paste `https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest`

### Manual install

1. Download the latest release archive
2. Extract `Packages/com.tinywalnutgames.storytest` into your Unity project
3. Unity imports the package automatically

## 2. Run your first validation

### Unity menu (one-off check)

1. Open the menu: `Tiny Walnut Games > The Story Test > Run Story Test and Export Report`
2. Watch the Console for a summary
3. Inspect `.debug/storytest_report.txt` for detailed violations

### In-scene validator (repeatable in play mode)

1. Add `ProductionExcellenceStoryTest` to a GameObject in your scene
2. Configure which phases to run (Story Integrity, Code Coverage, Architectural Compliance, etc.)
3. Use the component context menu **Validate Production Excellence** or call `ValidateProductionExcellence()` at runtime

### CLI validator (Unity optional)

```bash
python story_test.py /path/to/UnityProject --verbose
```

- Works on Windows, Linux, and macOS
- Auto-discovers compiled assemblies under `Library/ScriptAssemblies`
- Add `--fail-on-violations` in CI to break builds when any Act fails

> 🧭 **Canonical pipeline:** Our GitHub Actions workflow runs the Linux job automatically for every push/PR. Treat that job as the source of truth for Story Test compliance. Trigger the optional Windows/macOS jobs when you need platform-specific assurance.

## 3. Configure the narrative

`Assets/Tiny Walnut Games/TheStoryTest/Resources/StoryTestSettings.json` controls validation phases, assembly filters, and DOTS/ECS checks. Document intentional exclusions with inline comments or `[StoryIgnore]` so future readers understand the story.

```json
{
  "projectName": "Sample Narrative",
  "assemblyFilters": {
    "include": ["Assembly-CSharp"],
    "exclude": ["Unity", "System", "Mono"]
  },
  "phases": {
    "enableStoryIntegrity": true,
    "enableCodeCoverage": true,
    "enableArchitecturalCompliance": false,
    "enableProductionReadiness": true,
    "enableSyncPointPerformance": false
  },
  "enableDotsValidation": false
}
```

## 4. Know your Acts

Each violation maps to one of the **Nine Acts**. Learn the intent, detection heuristics, and remediation tips in the [Acts Guide](ActsGuide.md) before silencing anything with `[StoryIgnore]`.

## 5. Platform-specific code paths

- Guard Windows/macOS APIs behind `#if UNITY_STANDALONE_WIN` / `#if UNITY_STANDALONE_OSX`
- Provide Linux-safe defaults so the canonical pipeline stays green
- When a violation only reproduces on another OS, re-run the workflow with the `run-windows` or `run-macos` dispatch inputs or execute the CLI on that platform

## 6. Next steps

- Dive into [Dynamic Validation](DynamicValidation.md) for multi-phase runtime checks
- Explore [AssemblyStructure](AssemblyStructure.md) to understand how the package hangs together
- Automate everything with the [CI & Automation guide](CI.md)
- Open the [Example Project](../../Samples~/ExampleProject/) for a guided walkthrough
- Prep team training with the [FAQ & Demo Playbook](FAQ.md)
- Share your story in GitHub Discussions if you uncover new narrative patterns

---

**Narrative rule of thumb:** Every symbol in your assembly should read like a finished chapter. If a parameter, method, or enum feels like foreshadowing, seal it with intent or finish the scene before shipping.
