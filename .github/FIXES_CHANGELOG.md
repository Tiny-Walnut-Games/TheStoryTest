# 🔧 Workflow Fixes & Historical Changes

Historical record of GitHub Actions workflow issues and resolutions.

---

## Latest Update: December 2024 - Version Tag Migration

### Problem: Deprecated Action Versions

GitHub Actions workflow was using pinned commit SHAs for action versions, which can become deprecated and cause workflow failures.

### Solution: Migrated to Major Version Tags

Updated all GitHub Actions to use major version tags (`@v4`, `@v5`) instead of commit SHAs, following GitHub's recommended best practices.

**Before:**

```yaml
uses: actions/checkout@b4ffde65f46336ab88eb53be808477a3936bae11  # v4.1.1
uses: actions/setup-python@0a5c61591373683505ea898e09a3ea4f39ef2b9c  # v5.0.0
uses: actions/cache@13aacd865c20de90d75de3b17ebe84f7a17d57d2  # v4.0.0
uses: actions/upload-artifact@5d5d22a31266ced268874388b861e4b58bb5c2f3  # v4.3.1
uses: actions/setup-dotnet@4d6c8fcf3c8f7a60068d26b594648e99df24cee3  # v4.0.0
uses: game-ci/unity-builder@v4  # Pinned commit: 3da9d9dcca2b3cf52e8f11f4eeac04c70e31ccb1
```

**After:**

```yaml
uses: actions/checkout@v4
uses: actions/setup-python@v5
uses: actions/cache@v4
uses: actions/upload-artifact@v4
uses: actions/setup-dotnet@v4
uses: game-ci/unity-builder@v4
```

**Benefits:**

- ✅ Automatic updates to latest compatible versions
- ✅ Automatic security patches
- ✅ Reduced maintenance burden
- ✅ Cleaner, more readable code
- ✅ GitHub best practice compliance

---

## October 2024 - Linter Errors Resolution

### Problem: 72 Duplicate Linter Errors

**Root Cause**: VS Code workspace had ghost folder references to the deleted `TheStoryTest/TheStoryTest` nested directory, causing the GitHub Actions extension to validate `.github/workflows/story-test.yml` multiple times (~10 instances × 7 errors each = 72 total errors).

### Solutions Implemented

#### 1. Fixed Action Resolution Errors

**Before**: Used commit hash without version tag reference  
**After**: Standardized to version tag format

#### 2. Cleaned Up Workspace Configuration

**File**: `TheStoryTest.code-workspace`

- Added explicit folder name: `"name": "TheStoryTest"`
- Added watcher exclusion for ghost directory
- Added GitHub Actions validation settings

#### 3. Added VS Code Settings

**File**: `.vscode/settings.json`

- Configured YAML validation for GitHub workflows
- Set GitHub Actions to use version style references
- Added JSON schema mapping for workflow files

#### 4. Created Actionlint Configuration

**File**: `.github/actionlint.yaml`

- Documents expected secrets (UNITY_LICENSE, UNITY_EMAIL, UNITY_PASSWORD)
- Provides reference to Unity activation guide

**Result**: ✅ ALL ERRORS RESOLVED

- **Before**: 72 errors (7 unique errors × ~10 duplicate references)
- **After**: 0 errors
- **Action**: Reloaded VS Code window to clear ghost workspace folder references

### Expected Warnings When Secrets Are Not Configured

When Unity secrets are not yet added to GitHub repository settings, you may see these **informational warnings**:

- `Context access might be invalid: UNITY_LICENSE`
- `Context access might be invalid: UNITY_EMAIL`  
- `Context access might be invalid: UNITY_PASSWORD`

**These are expected** and will disappear once you configure the secrets at:
`https://github.com/jmeyer1980/TheStoryTest/settings/secrets/actions`

See Unity activation guide: <https://game.ci/docs/github/activation>

---

## October 2024 - Platform Identifier Fix

### Issue

The workflow was failing on Linux builds with the error:

```
Platform must be one of the ones described in the documentation.
"Linux64" is currently not supported.
```

### Root Cause

The workflow was using an incorrect platform identifier `Linux64` which is not recognized by Unity's build system.

### Solution

Changed from dynamic platform selection to explicit matrix mapping using correct Unity platform identifiers:

#### Before (Incorrect)

```yaml
matrix:
  os: [ubuntu-latest, windows-latest, macos-latest]
  
targetPlatform: ${{ matrix.os == 'windows-latest' && 'StandaloneWindows64' || matrix.os == 'macos-latest' && 'StandaloneOSX' || 'Linux64' }}
```

#### After (Correct)

```yaml
matrix:
  include:
    - os: ubuntu-latest
      unity-platform: StandaloneLinux64  # ✅ Correct name
    - os: windows-latest
      unity-platform: StandaloneWindows64
    - os: macos-latest
      unity-platform: StandaloneOSX
  unity-version: ['2022.3.17f1']

targetPlatform: ${{ matrix.unity-platform }}
```

### Valid Unity Platform Identifiers

According to [game-ci/unity-builder documentation](https://game.ci/docs/github/builder#targetplatform):

- **Linux**: `StandaloneLinux64` (not ~~Linux64~~)
- **Windows**: `StandaloneWindows64` (or `StandaloneWindows` for 32-bit)
- **macOS**: `StandaloneOSX`
- **WebGL**: `WebGL`
- **Android**: `Android`
- **iOS**: `iOS`

### Benefits of This Approach

1. ✅ **Explicit mapping** - Clear relationship between OS runner and Unity build target
2. ✅ **Maintainable** - Easy to add new platform combinations
3. ✅ **Type-safe** - No complex ternary expressions that can fail silently
4. ✅ **Extensible** - Can add platform-specific configurations (e.g., Android API level)

### Testing

After this fix, all three platforms should build successfully:

- ✅ **Ubuntu** → StandaloneLinux64
- ⏳ **Windows** → StandaloneWindows64
- ⏳ **macOS** → StandaloneOSX

---

## Workflow Features

✅ Cross-platform testing (Ubuntu, Windows, macOS)  
✅ Python 3.11 with pythonnet for .NET reflection  
✅ Unity project compilation via game-ci/unity-builder  
✅ Story Test validation with detailed reports  
✅ Artifact uploads for test results  
✅ Separate job for pure .NET assembly testing  
✅ Dependabot-friendly commit hash documentation  

---

## Next Steps When Deploying

1. **Add Unity Secrets** to GitHub repository (see Unity activation link above)
2. **Push workflow** to GitHub: `git add .github/ && git commit -m "fix: GitHub Actions workflow" && git push`
3. **Monitor first run** at: `https://github.com/jmeyer1980/TheStoryTest/actions`
4. **Review artifacts** uploaded by successful workflow runs