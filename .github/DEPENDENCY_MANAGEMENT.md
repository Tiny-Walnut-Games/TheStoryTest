# Dependency Management Strategy

## Overview

This document outlines how package dependencies are managed across the Story Test Framework to ensure compatibility across multiple Unity versions and platforms (Windows, macOS, Linux).

## Problem Statement

Previously, the project used hardcoded package versions (e.g., `"com.unity.test-framework": "1.6.0"`) which caused platform-specific failures, especially on Linux CI/CD builds where certain package versions were unavailable in the registry.

## Solution: Conservative Versioning

Instead of using the latest specific versions that work on one platform, we use **proven, widely-available versions** that are compatible across all platforms and Unity versions (2022.3 LTS through 6.x).

### Updated Package Versions

The following package versions have been standardized across both:
- `Packages/manifest.json` (main project)
- `Samples~/ExampleProject/Packages/manifest.json` (CI/CD build project)

| Package | Old Version | New Version | Reason |
|---------|------------|-------------|--------|
| com.unity.test-framework | 1.6.0 | 1.1.0 | More universally available |
| com.unity.render-pipelines.universal | 17.2.0 | 14.0.0 | Stable cross-platform version |
| com.unity.inputsystem | 1.14.2 | 1.7.0 | Stable, widely compatible |
| com.unity.ide.rider | 3.0.38 | 3.0.0 | Base version is sufficient |
| com.unity.visualscripting | 1.9.7-1.9.8 | 1.7.0 | Stable, pre-1.8+ breaking changes |
| com.unity.ai.navigation | 2.0.9 | 2.0.0 | Base version works across versions |
| com.unity.collab-proxy | 2.9.3 | 2.0.0 | More stable across platforms |

### CI/CD Unity Version

**Workflow Build Version**: `2022.3.26f1` (LTS)
- **Before**: `6000.0.30f1` (bleeding edge, platform inconsistencies)
- **After**: `2022.3.26f1` (stable LTS, all packages reliably available)

This ensures the CI/CD build uses a version with guaranteed package availability on all platforms.

## Development vs. CI/CD

- **Local Development**: Can continue using Unity 6.2+ with the same manifest files
- **CI/CD Pipeline**: Uses `2022.3.26f1` for consistent, reproducible builds
- **Package Versions**: Compatible with both ranges (2022.3 LTS through 6.x)

## Synchronization Strategy

### Automatic Dependency Sync

To keep dependencies in sync across the repository, a utility script manages version consistency:

```bash
# Python script to sync versions across manifests
python scripts/sync_dependencies.py
```

This script:
1. Scans all `manifest.json` files
2. Maintains version compatibility matrix
3. Flags incompatible version combinations
4. Can auto-update to known-good versions

### Manual Override

If you need to update package versions:

1. Update `Packages/manifest.json` (source of truth)
2. Update `Samples~/ExampleProject/Packages/manifest.json` to match
3. Run `python scripts/sync_dependencies.py --verify` to validate
4. Test locally before pushing to CI/CD

## Platform Compatibility

### Linux (CI/CD Primary Platform)

The most restrictive platform for package availability. All versions are validated on Linux first.

### macOS & Windows

Generally have better registry coverage. If a version works on Linux, it works on these platforms.

## Migration Path

For users updating their local projects:

1. **Option A**: Keep your manifest.json as-is (auto-resolves to available versions)
2. **Option B**: Use the updated versions for better consistency:
   ```json
   "com.unity.test-framework": "1.1.0"
   ```

## Troubleshooting

### If CI/CD still fails on Linux:

1. Check the error message for the specific missing package
2. Note the package name and requested version
3. Update the corresponding entry in both manifest files to a lower minor version
4. Test locally first before pushing

### If local development breaks:

1. Run `npm install` or Unity Package Manager refresh
2. Check that your local Unity version is compatible (2020.3 LTS or later)
3. Run `python scripts/sync_dependencies.py` to restore known-good versions

## Version Constraints

Future reference for choosing compatible versions:

- **Test Framework**: 1.1.0+ (available since 2020.3 LTS)
- **URP**: 7.0.0+ (available since 2019.3, 14.0.0+ for modern versions)
- **Input System**: 1.0.0+ (available since 2019.4, 1.7.0+ for stability)
- **Visual Scripting**: 1.7.0+ (modern version without breaking changes)
- **Rider Plugin**: 3.0.0+ (widely available)

## Future Improvements

Potential enhancements to automate this further:

- [ ] Add dependency version matrix to pyproject.toml
- [ ] Implement GitHub Action to validate package availability
- [ ] Create version constraints file (requirements-unity.txt equivalent)
- [ ] Auto-update versions when new LTS releases occur