# GitHub Actions Unity Platform Fix

## Issue

The workflow was failing on Linux builds with the error:

```ts
Platform must be one of the ones described in the documentation.
"Linux64" is currently not supported.
```

## Root Cause

The workflow was using an incorrect platform identifier `Linux64` which is not recognized by Unity's build system.

## Solution

Changed from dynamic platform selection to explicit matrix mapping using correct Unity platform identifiers:

### Before (Incorrect)

```yaml
matrix:
  os: [ubuntu-latest, windows-latest, macos-latest]
  
targetPlatform: ${{ matrix.os == 'windows-latest' && 'StandaloneWindows64' || matrix.os == 'macos-latest' && 'StandaloneOSX' || 'Linux64' }}
```

### After (Correct)

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

## Valid Unity Platform Identifiers

According to [game-ci/unity-builder documentation](https://game.ci/docs/github/builder#targetplatform):

- **Linux**: `StandaloneLinux64` (not ~~Linux64~~)
- **Windows**: `StandaloneWindows64` (or `StandaloneWindows` for 32-bit)
- **macOS**: `StandaloneOSX`
- **WebGL**: `WebGL`
- **Android**: `Android`
- **iOS**: `iOS`

## Benefits of This Approach

1. ✅ **Explicit mapping** - Clear relationship between OS runner and Unity build target
2. ✅ **Maintainable** - Easy to add new platform combinations
3. ✅ **Type-safe** - No complex ternary expressions that can fail silently
4. ✅ **Extensible** - Can add platform-specific configurations (e.g., Android API level)

## Testing

After this fix, all three platforms should build successfully:

- ✅ **Ubuntu** → StandaloneLinux64
- ⏳ **Windows** → StandaloneWindows64 (waiting)
- ⏳ **macOS** → StandaloneOSX (waiting)

---

**Date Fixed**: October 2, 2025
**Issue**: Platform identifier error on Linux builds
**Resolution**: Use correct Unity platform names with explicit matrix mapping
