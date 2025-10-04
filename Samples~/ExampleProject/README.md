# Story Test Example Project

Bring the framework to life with a sample Unity scene that already knows how to tell a coherent story. Use this project to rehearse validation flows, demo findings to teammates, or verify your pipeline.

## What's included

- ✅ Story Test package referenced locally (no network fetch required)
- ✅ `ProductionExcellenceStoryTest` pre-wired in the sample scene
- ✅ `StoryTestSettings.json` tuned for quick iteration
- ✅ Helper scripts that illustrate best practices for `[StoryIgnore]`, debug gating, and parameter usage

## Open the project

1. Install **Unity 2020.3 or later**.
2. From the repo root, launch the sample:

   ```bash
   unity-editor -projectPath "Samples~/ExampleProject"
   ```

3. Let Unity import assets and scripts (the package is linked via `file:` in `Packages/manifest.json`).

## Run Story Test in the Editor

### Option A – Menu command

1. Choose `Tiny Walnut Games > The Story Test > Run Story Test and Export Report`.
2. Watch the Console for a pass/fail summary.
3. Inspect `.debug/storytest_report.txt` for the full list of violations, if any.

### Option B – Scene component

1. Open `Assets/Scenes/SampleScene.unity`.
2. Select the GameObject named **Story Test Driver**.
3. In the Inspector, use the `ProductionExcellenceStoryTest` context menu → **Validate Production Excellence**.
4. Toggle phases (Story Integrity, Code Coverage, etc.) directly on the component to explore multi-phase validation.

## Try the CLI validator

The sample works great with the standalone Python validator. From the repo root:

```bash
python story_test.py Samples~/ExampleProject --verbose --fail-on-violations
```

- Auto-discovers `Library/ScriptAssemblies` after Unity compiles.
- Mirrors the Linux canonical CI job so you can reproduce reports locally.
- Add `--output sample-report.json` to capture machine-readable artifacts.

## Customize the narrative

- Settings live in `Assets/Tiny Walnut Games/TheStoryTest/Resources/StoryTestSettings.json`.
- Guard platform-specific code with `#if` directives so Linux builds (our canonical validator) stay green.
- When you truly must opt out, use `[StoryIgnore("Infrastructure component for validation framework")]` and explain the decision in the attribute string.

## Learning checklist

- **Menu vs. runtime**: Experience both validation flows and decide which suits your team.
- **Acts in action**: Intentionally introduce an issue (e.g., add `throw new NotImplementedException()`) and rerun to see Act 1 fire.
- **CI rehearsal**: Conform your pipeline to the `story-test.yml` workflow, then compare the local CLI output with the Linux job summary.

## Requirements

- Unity 2020.3+
- Python 3.11 (for the CLI demo)
- .NET Standard 2.0 compatible assemblies

## Documentation & support

- [Package Quick Start](../../Packages/com.tinywalnutgames.storytest/Documentation~/QuickStart.md)
- [Acts Guide](../../Packages/com.tinywalnutgames.storytest/Documentation~/ActsGuide.md)
- [CI & Automation](../../Packages/com.tinywalnutgames.storytest/Documentation~/CI.md)
- [GitHub Issues](https://github.com/jmeyer1980/TheStoryTest/issues)
- [GitHub Discussions](https://github.com/jmeyer1980/TheStoryTest/discussions)

---

**Reminder:** Keep every scene, symbol, and sample purposeful. When you modify the sample to match your own narrative, run Story Test one more time to confirm the tale still holds together.
