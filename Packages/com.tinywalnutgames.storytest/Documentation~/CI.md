# CI & Automation

Keep Story Test enforcement consistent across environments with a Linux-first pipeline plus optional Windows and macOS spot checks.

## Canonical workflow (GitHub Actions)

1. Copy `.github/workflows/story-test.yml` into your repository.
2. Store Unity credentials as encrypted secrets: `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`.
3. Push to any branch — the **Linux job triggers automatically** and serves as the source of truth for Story Test compliance.
4. Trigger Windows or macOS jobs manually through the workflow dispatch inputs when you need platform-specific coverage.

### What each job does

- Checks out the repo and installs Python 3.11
- Restores the Unity `Library/` cache for faster imports
- Builds a headless player with `game-ci/unity-builder`
- Runs `story_test.py` against the produced assemblies
- Uploads violation reports and summarizes failures directly in the job summary

> 💡 **Tip:** Treat the Linux result as authoritative. If you hit a platform-specific concern, run the Windows/macOS jobs only for that investigation.

## Local and CI command line

Use the standalone validator bundled with this repo (`story_test.py`) for fast feedback:

```bash
python story_test.py /path/to/UnityProject --verbose --fail-on-violations
```

- Pass a project root, a single DLL, or a directory of assemblies.
- Add `--output report.json` to export structured data for dashboards.
- Because the validator loads IL via `pythonnet`, ensure the target framework is .NET Standard 2.0+.

### Working with platform conditionals

- Guard APIs that only exist on Windows/macOS with `#if UNITY_STANDALONE_WIN` / `#if UNITY_STANDALONE_OSX`.
- Provide Linux-safe defaults—otherwise the canonical job will report missing implementations or unused parameters.
- When suppressing a violation that is intentional on another platform, add `[StoryIgnore("Platform-specific implementation lives under #if UNITY_STANDALONE_WIN")]` and link to a tracking issue.

## Troubleshooting the pipeline

| Symptom | Likely cause | Fix |
| --- | --- | --- |
| `FileNotFoundException: UnityEditor.CoreModule` | Editor tests aren't supported in headless builders | Convert the test to play mode or run it manually in the Editor |
| `story_test.py` fails to load assemblies | Assemblies not copied to `Library/ScriptAssemblies` or target path wrong | Ensure the Unity build step succeeded and verify the path passed to the CLI |
| Linux job reports missing platform APIs | Windows/macOS code not conditionally compiled | Add `#if UNITY_STANDALONE_*` guards and ensure Linux-friendly fallbacks |
| Cache misses on every run | Shared Library cache key too broad | Include `ProjectSettings/ProjectVersion.txt` and `Packages/packages-lock.json` in cache key |

## Extending to other CI providers

The same steps apply to Azure Pipelines, GitLab CI, or Jenkins:

1. Install Unity silently on the agent (e.g., via `game-ci/unity-builder` container images).
2. Restore the `Library/` directory cache between runs.
3. Build the player or compile assemblies headless.
4. Run `story_test.py` with `--fail-on-violations`.

By keeping the validation logic in Python, every pipeline receives identical verdicts—ensuring your story stays coherent, no matter where it is told.
