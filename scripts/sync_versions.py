#!/usr/bin/env python3
# -*- coding: utf-8-sig -*-
"""
Synchronize version numbers across all Story Test Framework distributions.

This script reads VERSION.txt and updates:
- Packages/com.tinywalnutgames.storytest/package.json
- Packages/com.tinywalnutgames.editor-tools/package.json
- pyproject.toml
- package.json (root)

Usage:
    python scripts/sync_versions.py <new-version>
    python scripts/sync_versions.py 1.3.0

Or without arguments to read from VERSION.txt:
    python scripts/sync_versions.py
"""

import json
import sys
import re
from pathlib import Path


def read_version_from_file(version_file: Path) -> str:
    """Read version from VERSION.txt."""
    if not version_file.exists():
        raise FileNotFoundError(f"VERSION.txt not found at {version_file}")
    return version_file.read_text().strip()


def update_package_json(file_path: Path, new_version: str) -> bool:
    """Update version in a package.json file."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        old_version = data.get('version', 'unknown')
        data['version'] = new_version
        
        with open(file_path, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=4)
        
        print(f"✅ {file_path.relative_to(file_path.parents[2])}: {old_version} → {new_version}")
        return True
    except Exception as e:
        print(f"❌ Failed to update {file_path}: {e}")
        return False


def update_pyproject_toml(file_path: Path, new_version: str) -> bool:
    """Update version in pyproject.toml."""
    try:
        content = file_path.read_text(encoding='utf-8')
        
        # Extract old version first
        old_version_match = re.search(r'version\s*=\s*["\']([^"\']+)["\']', content)
        if not old_version_match:
            print(f"⚠️  No version found in {file_path.name}")
            return False
        
        old_version_str = old_version_match.group(1)
        
        # Match: version = "1.3.0" (with flexible whitespace and quote style)
        # This pattern matches: version = "X.Y.Z" or version = 'X.Y.Z'
        pattern = r'(version\s*=\s*)["\'][\d\.]+["\']'
        new_content = re.sub(
            pattern,
            f'version = "{new_version}"',
            content,
            flags=re.MULTILINE
        )
        
        # Always write if we found a version (even if it's the same)
        file_path.write_text(new_content, encoding='utf-8')
        
        if old_version_str == new_version:
            print(f"✅ pyproject.toml: already at {new_version}")
        else:
            print(f"✅ pyproject.toml: {old_version_str} → {new_version}")
        return True
    except Exception as e:
        print(f"❌ Failed to update {file_path}: {e}")
        return False


def update_version_txt(file_path: Path, new_version: str) -> bool:
    """Update VERSION.txt."""
    try:
        old_version = file_path.read_text(encoding='utf-8').strip() if file_path.exists() else "none"
        file_path.write_text(f"{new_version}\n", encoding='utf-8')
        print(f"✅ VERSION.txt: {old_version} → {new_version}")
        return True
    except Exception as e:
        print(f"❌ Failed to update VERSION.txt: {e}")
        return False


def main():
    """Main entry point."""
    repo_root = Path(__file__).parent.parent
    
    # Get version from argument or VERSION.txt
    if len(sys.argv) > 1:
        new_version = sys.argv[1]
    else:
        version_file = repo_root / "VERSION.txt"
        new_version = read_version_from_file(version_file)
    
    # Validate version format
    if not re.match(r'^\d+\.\d+\.\d+', new_version):
        print(f"❌ Invalid version format: {new_version}")
        print("   Expected format: X.Y.Z (e.g., 1.3.0)")
        sys.exit(1)
    
    print(f"\n🔄 Syncing version to: {new_version}")
    print("=" * 60)
    
    files_to_update = [
        (repo_root / "VERSION.txt", "version_txt"),
        (repo_root / "package.json", "package_json"),
        (repo_root / "Packages" / "com.tinywalnutgames.storytest" / "package.json", "package_json"),
        (repo_root / "Packages" / "com.tinywalnutgames.editor-tools" / "package.json", "package_json"),
        (repo_root / "pyproject.toml", "pyproject_toml"),
    ]
    
    results = []
    for file_path, file_type in files_to_update:
        if not file_path.exists():
            print(f"⚠️  Skipping {file_path.name} (not found)")
            continue
        
        if file_type == "package_json":
            results.append(update_package_json(file_path, new_version))
        elif file_type == "pyproject_toml":
            results.append(update_pyproject_toml(file_path, new_version))
        elif file_type == "version_txt":
            results.append(update_version_txt(file_path, new_version))
    
    print("=" * 60)
    if all(results):
        print(f"\n✅ All versions synced to {new_version}")
        return 0
    else:
        print(f"\n⚠️  Some updates failed. Check messages above.")
        return 1


if __name__ == "__main__":
    sys.exit(main())