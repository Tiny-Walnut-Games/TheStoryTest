# GitHub Actions Configuration

This directory contains CI/CD workflows for the Story Test Framework.

## Workflows

### story-test.yml

**Purpose:** Cross-platform validation of Unity sample project

**Triggers:**

- Push to `main` or `develop` branches
- Pull requests to `main`
- Manual dispatch

**Strategy:** Matrix build across 3 platforms

- ✅ Ubuntu (Linux)
- ✅ Windows
- ✅ macOS

**Steps:**

1. **Checkout** - Clone repository
2. **Setup Python** - Install Python 3.11
3. **Install Dependencies** - `pip install -r requirements.txt`
4. **Cache Unity Library** - Speed up builds
5. **Build Sample Project** - Compile `Samples~/ExampleProject/`
6. **Run Story Test** - Validate with `story_test.py`
7. **Upload Report** - Artifact for review
8. **Display Results** - Summary in GitHub UI
9. **Fail on Violations** - Block merge if issues found

## Required Secrets

These must be configured in repository **Settings > Secrets**:

| Secret | Description | Setup Guide |
|--------|-------------|-------------|
| `UNITY_LICENSE` | Unity license file (base64) | [game.ci activation](https://game.ci/docs/github/activation) |
| `UNITY_EMAIL` | Unity account email | Unity Hub account |
| `UNITY_PASSWORD` | Unity account password | Unity Hub account |

## Report Artifacts

**Location:** GitHub Actions > Workflow Run > Artifacts

**Naming Convention:** `story-test-report-{os}`

**Examples:**

- `story-test-report-ubuntu-latest`
- `story-test-report-windows-latest`
- `story-test-report-macos-latest`

**Contents:** `story-test-report.json`

```json
{
  "totalViolations": 0,
  "violationsByType": {
    "TodoComment": 0,
    "PlaceholderImplementation": 0
  },
  "violations": []
}
```

## Stub Reports

**Purpose:** Test CI/CD workflow without Unity build

**Location:** `.github/story-test-report-stub.json`

**Usage:**

```yaml
# In workflow (for testing only)
- name: Use Stub Report
  run: cp .github/story-test-report-stub.json story-test-report.json
```

## Workflow Customization

### Enable .NET-Only Validation

The workflow includes a disabled job for **pure C# projects** (no Unity):

```yaml
story-test-dotnet:
  if: false  # Change to true to enable
```

**When to use:**

- Validating standalone .NET libraries
- Testing without Unity secrets configured
- Faster CI for non-Unity code changes

### Adjust Build Targets

**Default:** `StandaloneLinux64`, `StandaloneWindows64`, `StandaloneOSX`

**Other options:**

```yaml
matrix:
  include:
    - os: ubuntu-latest
      unity-platform: Android  # Mobile build
    - os: windows-latest
      unity-platform: WebGL    # Browser build
```

See [Unity build targets](https://docs.unity3d.com/Manual/BuildSettings.html)

## Troubleshooting

### Unity License Activation Fails

**Symptom:** `UNITY_LICENSE secret is not set`

**Solution:**

1. Follow [game.ci activation guide](https://game.ci/docs/github/activation)
2. Add license file as base64 to GitHub Secrets
3. Verify secret name is exactly `UNITY_LICENSE`

### Build Takes Too Long

**Symptom:** Workflow times out (>60 minutes)

**Solutions:**

- Enable Library caching (already configured)
- Reduce Unity version downloads with caching
- Use `.NET-only` job for non-Unity changes

### Report Not Generated

**Symptom:** `story-test-report.json` missing

**Causes:**

- `story_test.py` crashed before writing report
- Wrong output path specified
- Python dependencies not installed

**Debug:**

```yaml
- name: Debug Report Location
  if: always()
  run: |
    ls -la
    find . -name "*.json"
```

### Workflow Fails Locally But Passes in CI

**Symptom:** Local validation fails, GitHub passes (or vice versa)

**Causes:**

- Different Unity versions
- Different Python versions
- OS-specific path issues

**Solution:** Match local environment to CI:

```bash
# Use same Python version
python3.11 -m venv venv
source venv/bin/activate  # or venv\Scripts\activate on Windows

# Same Unity version
# Install Unity 2022.3.17f1 via Unity Hub
```

## Local Testing

**Simulate CI workflow locally:**

```bash
# 1. Install dependencies
pip install -r requirements.txt

# 2. Build Unity project (manual via Unity Editor)
# Open: Samples~/ExampleProject/
# File > Build Settings > Build

# 3. Run validation
python story_test.py Samples~/ExampleProject --verbose --output story-test-report.json

# 4. Check report
cat story-test-report.json | jq
```

## See Also

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [game.ci Unity Builder](https://game.ci/docs/github/builder)
- [Unity Cloud Build](https://unity.com/products/cloud-build)
- [Story Test Framework README](../README.md)
