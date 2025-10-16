#!/usr/bin/env python3
# -*- coding: utf-8-sig -*-
"""
Synchronize Unity package dependencies across manifest files.

This script ensures that package versions are consistent across:
- Packages/manifest.json (main project)
- Samples~/ExampleProject/Packages/manifest.json (CI/CD build)

It helps maintain compatibility across Windows, macOS, and Linux platforms
by flagging and fixing version inconsistencies.
"""

import json
import sys
from pathlib import Path
from typing import Dict, Any, List, Tuple


# Known-good versions that work across all platforms
CANONICAL_VERSIONS = {
    "com.tinywalnutgames.storytest": "https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest",
    "com.unity.2d.sprite": "1.0.0",
    "com.unity.ai.navigation": "2.0.0",
    "com.unity.collab-proxy": "2.0.0",
    "com.unity.feature.ecs": "1.0.0",
    "com.unity.ide.rider": "3.0.0",
    "com.unity.ide.visualstudio": "2.0.0",
    "com.unity.inputsystem": "1.7.0",
    "com.unity.multiplayer.center": "1.0.0",
    "com.unity.render-pipelines.universal": "14.0.0",
    "com.unity.test-framework": "1.1.0",
    "com.unity.testtools.codecoverage": "1.0.0",
    "com.unity.timeline": "1.6.0",
    "com.unity.toolchain.win-x86_64-linux-x86_64": "2.0.0",
    "com.unity.ugui": "2.0.0",
    "com.unity.visualscripting": "1.7.0",
}


def load_manifest(path: Path) -> Dict[str, Any]:
    """Load and parse a manifest.json file."""
    try:
        with open(path, 'r') as f:
            return json.load(f)
    except Exception as e:
        print(f"❌ Error loading {path}: {e}")
        return {}


def save_manifest(path: Path, data: Dict[str, Any]) -> bool:
    """Save manifest.json file with proper formatting."""
    try:
        with open(path, 'w') as f:
            json.dump(data, f, indent=2)
        return True
    except Exception as e:
        print(f"❌ Error saving {path}: {e}")
        return False


def compare_dependencies(
    deps1: Dict[str, str], 
    deps2: Dict[str, str]
) -> List[Tuple[str, str, str]]:
    """
    Find version mismatches between two dependency sets.
    
    Returns list of (package, version1, version2) tuples where versions differ.
    """
    mismatches = []
    all_packages = set(deps1.keys()) | set(deps2.keys())
    
    for package in all_packages:
        v1 = deps1.get(package)
        v2 = deps2.get(package)
        if v1 != v2:
            mismatches.append((package, v1 or "MISSING", v2 or "MISSING"))
    
    return mismatches


def sync_dependencies(fix: bool = False, verbose: bool = False) -> int:
    """
    Synchronize dependencies across all manifest files.
    
    Args:
        fix: If True, fix mismatches using canonical versions
        verbose: If True, print detailed output
    
    Returns:
        0 if OK, 1 if mismatches found/not fixed
    """
    root = Path(__file__).parent.parent
    
    manifests = [
        root / "Packages" / "manifest.json",
        root / "Samples~" / "ExampleProject" / "Packages" / "manifest.json",
    ]
    
    all_loaded = {}
    for manifest_path in manifests:
        if manifest_path.exists():
            all_loaded[manifest_path] = load_manifest(manifest_path)
        else:
            print(f"⚠️  Manifest not found: {manifest_path}")
    
    if not all_loaded:
        print("❌ No manifests found to sync")
        return 1
    
    print(f"📋 Scanning {len(all_loaded)} manifest files...\n")
    
    # Check for inconsistencies between manifests
    mismatches_found = False
    manifest_paths = list(all_loaded.keys())
    
    for i in range(len(manifest_paths)):
        for j in range(i + 1, len(manifest_paths)):
            path1 = manifest_paths[i]
            path2 = manifest_paths[j]
            deps1 = all_loaded[path1].get("dependencies", {})
            deps2 = all_loaded[path2].get("dependencies", {})
            
            mismatches = compare_dependencies(deps1, deps2)
            
            if mismatches:
                mismatches_found = True
                print(f"⚠️  Mismatches between:")
                print(f"   {path1.relative_to(root)}")
                print(f"   {path2.relative_to(root)}\n")
                
                for package, v1, v2 in sorted(mismatches):
                    print(f"   {package}:")
                    print(f"     - {path1.relative_to(root)}: {v1}")
                    print(f"     - {path2.relative_to(root)}: {v2}")
                    
                    if fix and package in CANONICAL_VERSIONS:
                        canonical = CANONICAL_VERSIONS[package]
                        print(f"     → fixing to: {canonical}")
                
                print()
    
    # Check against canonical versions
    print("🔍 Checking canonical versions...\n")
    out_of_sync = False
    
    for manifest_path, manifest_data in all_loaded.items():
        deps = manifest_data.get("dependencies", {})
        
        for package, version in deps.items():
            if package in CANONICAL_VERSIONS:
                canonical = CANONICAL_VERSIONS[package]
                if version != canonical:
                    out_of_sync = True
                    print(f"⚠️  {package}")
                    print(f"   Current:   {version}")
                    print(f"   Canonical: {canonical}")
                    print(f"   In: {manifest_path.relative_to(root)}\n")
    
    if not mismatches_found and not out_of_sync:
        print("✅ All dependencies are synchronized!\n")
        return 0
    
    # Apply fixes if requested
    if fix:
        print("\n🔧 Applying fixes...\n")
        
        for manifest_path, manifest_data in all_loaded.items():
            deps = manifest_data.get("dependencies", {})
            changes = 0
            
            for package in list(deps.keys()):
                if package in CANONICAL_VERSIONS:
                    old_version = deps[package]
                    new_version = CANONICAL_VERSIONS[package]
                    
                    if old_version != new_version:
                        deps[package] = new_version
                        changes += 1
                        if verbose:
                            print(f"   Updated {package}: {old_version} → {new_version}")
            
            if changes > 0:
                if save_manifest(manifest_path, manifest_data):
                    print(f"✅ Updated {manifest_path.relative_to(root)}: {changes} package(s)")
                else:
                    print(f"❌ Failed to save {manifest_path.relative_to(root)}")
                    return 1
        
        print("\n✅ Dependencies synchronized successfully!")
        return 0
    else:
        print("\n💡 Run with --fix to automatically update to canonical versions")
        return 1


def main():
    """Main entry point."""
    import argparse
    
    parser = argparse.ArgumentParser(
        description="Synchronize Unity package dependencies across manifest files"
    )
    parser.add_argument(
        "--fix",
        action="store_true",
        help="Fix mismatches using canonical versions"
    )
    parser.add_argument(
        "--verbose",
        "-v",
        action="store_true",
        help="Print detailed output"
    )
    parser.add_argument(
        "--check",
        action="store_true",
        help="Check only, don't fix (default behavior)"
    )
    
    args = parser.parse_args()
    
    return sync_dependencies(fix=args.fix, verbose=args.verbose)


if __name__ == "__main__":
    sys.exit(main())