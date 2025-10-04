# FAQ & Demo Playbook

Quick answers for the questions we hear most often, plus a lightweight script you can follow when recording a walkthrough or Loom demo.

## Frequently asked questions

### Why is Linux the canonical CI platform?

Linux builds finish fastest and avoid proprietary API bindings. The workflow treats the Linux job as the source of truth while leaving Windows/macOS as on-demand spot checks. Keep platform-specific code behind `#if` directives so Linux validation stays deterministic.

### When is `[StoryIgnore]` acceptable?

Use it only for infrastructure or editor-only scaffolding, always with a non-empty justification (e.g., `"Infrastructure component for validation framework"`). Never ignore violations that represent missing production work—fix the narrative instead.

### How do I run Story Test without Unity?

Install Python 3.11 and execute `python story_test.py MyAssembly.dll --fail-on-violations`. The validator loads IL via `pythonnet`, so any compiled .NET assembly can be scanned in CI or local scripts.

### Which Unity versions are supported?

The package targets Unity 2020.3+ and .NET Standard 2.0 assemblies. Earlier Unity versions lack the IL metadata guarantees required by the validator.

### Can I add my own validation rule?

Yes. Create a new `ActX*.cs` file under `Runtime/Acts/`, expose a `ValidationRule`, and Story Test discovers it automatically on Editor load. Mirror the rule in `story_test.py` if you want parity with the standalone validator.

### How do I keep validation fast on big projects?

- Cache the Unity `Library/` folder in CI (see `story-test.yml`).
- Run `story_test.py` only on assemblies that changed in a given commit.
- Use the Sync Point Performance phase to compare single vs. multi-threaded runs.

## Loom / demo walkthrough script

1. **Introduce the doctrine (30s).** Explain that every symbol must serve the story and that the Linux CI job is the canonical arbiter.
2. **Unity tour (60s).** Show the sample scene, open the `ProductionExcellenceStoryTest` component, and run validation from the context menu.
3. **Intentional violation (45s).** Add a `throw new NotImplementedException()` to a sample script, rerun validation, and highlight the Act 1 entry in `.debug/storytest_report.txt`.
4. **CLI parity (45s).** Switch to a terminal, run `python story_test.py Samples~/ExampleProject --verbose`, and compare the CLI output to the Unity report.
5. **Platform guard (30s).** Demonstrate `#if UNITY_STANDALONE_WIN` around a Windows-specific method and mention the optional Windows/macOS CI triggers.
6. **Wrap-up (30s).** Point viewers to the Quick Start, Acts Guide, and CI docs for next steps.

Recordings typically run 3–4 minutes; encourage teams to keep the narrative tight and focused on how Story Test reinforces production readiness.
