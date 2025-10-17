#!/usr/bin/env python3
"""
Script to automatically sync README.md across all branches
This ensures consistent badges, formatting, and content across main, develop, pre-release, and release branches
"""

import subprocess
import sys
from pathlib import Path

def run_command(cmd, check=True):
    """Run a git command and return output"""
    if isinstance(cmd, str):
        cmd_list = cmd.split()
    else:
        cmd_list = cmd

    try:
        result = subprocess.run(cmd_list, capture_output=True, text=True, check=check)
        return result.stdout.strip(), result.stderr.strip()
    except subprocess.CalledProcessError as e:
        if check:
            print(f"❌ Error running command: {' '.join(cmd_list)}")
            print(f"Error: {e.stderr}")
            sys.exit(1)
        return e.stdout.strip(), e.stderr.strip()

def get_current_branch():
    """Get current git branch"""
    stdout, _ = run_command("git branch --show-current")
    return stdout

def checkout_branch(branch):
    """Checkout a branch, creating it if it doesn't exist locally"""
    print(f"🔄 Checking out {branch}...")

    # Try to checkout the branch
    stdout, stderr = run_command(f"git checkout {branch}", check=False)

    if "did not match any file" in stderr or "is not a git branch" in stderr:
        print(f"📥 Branch {branch} doesn't exist locally, fetching from remote...")
        run_command(f"git fetch origin {branch}")
        run_command(f"git checkout -b {branch} origin/{branch}")
    elif "already exists" in stderr:
        # Branch exists but we're not on it
        run_command(f"git checkout {branch}")

    # Pull latest changes
    run_command(f"git pull origin {branch}", check=False)  # Don't fail if no remote

def update_readme_for_branch(branch):
    """Update README content based on branch type"""
    readme_path = Path("README.md")

    if not readme_path.exists():
        print(f"❌ README.md not found in {branch}")
        return False

    with open(readme_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # Define badge configurations for each branch
    if branch == "main":
        badges = """[![Story Test Validation](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml/badge.svg?branch=jmeyer1980%2Fissue2)](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml)
[![Latest Stable Version](https://img.shields.io/github/v/release/jmeyer1980/TheStoryTest?label=Latest%20Stable)](https://github.com/jmeyer1980/TheStoryTest/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Python Version](https://img.shields.io/badge/python-3.8+-blue.svg)](https://www.python.org/downloads/)
[![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity.com/releases)
[![StoryTest Validation](https://img.shields.io/badge/StoryTest-11%20Acts-green.svg)](docs/acts.md)"""

    elif branch == "develop":
        badges = """[![Story Test Validation](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml/badge.svg?branch=jmeyer1980%2Fissue2)](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml)
[![Latest Stable Version](https://img.shields.io/github/v/release/jmeyer1980/TheStoryTest?label=Latest%20Stable)](https://github.com/jmeyer1980/TheStoryTest/releases/latest)
[![Latest Pre-release](https://img.shields.io/github/v/release/jmeyer1980/TheStoryTest?include_prereleases&label=Latest%20Pre-release)](https://github.com/jmeyer1980/TheStoryTest/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Python Version](https://img.shields.io/badge/python-3.8+-blue.svg)](https://www.python.org/downloads/)
[![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity.com/releases)
[![StoryTest Validation](https://img.shields.io/badge/StoryTest-11%20Acts-green.svg)](docs/acts.md)
[![Coming Soon](https://img.shields.io/badge/StoryTest-13%20Acts%20in%201.3.0-orange.svg)](https://github.com/jmeyer1980/TheStoryTest/releases)"""

    elif branch == "pre-release":
        badges = """[![Story Test Validation](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml/badge.svg?branch=jmeyer1980%2Fissue2)](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml)
[![Latest Stable Version](https://img.shields.io/github/v/release/jmeyer1980/TheStoryTest?label=Latest%20Stable)](https://github.com/jmeyer1980/TheStoryTest/releases/latest)
[![Latest Pre-release](https://img.shields.io/github/v/release/jmeyer1980/TheStoryTest?include_prereleases&label=Latest%20Pre-release)](https://github.com/jmeyer1980/TheStoryTest/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Python Version](https://img.shields.io/badge/python-3.8+-blue.svg)](https://www.python.org/downloads/)
[![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity.com/releases)
[![StoryTest Validation](https://img.shields.io/badge/StoryTest-13%20Acts-green.svg)](docs/acts.md)
[![Version](https://img.shields.io/badge/Version-1.3.0%20Pre--release-orange.svg)](https://github.com/jmeyer1980/TheStoryTest/releases)"""

    elif branch == "release":
        badges = """[![Story Test Validation](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml/badge.svg?branch=jmeyer1980%2Fissue2)](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml)
[![Latest Stable Version](https://img.shields.io/github/v/release/jmeyer1980/TheStoryTest?label=Latest%20Stable)](https://github.com/jmeyer1980/TheStoryTest/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Python Version](https://img.shields.io/badge/python-3.8+-blue.svg)](https://www.python.org/downloads/)
[![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity.com/releases)
[![StoryTest Validation](https://img.shields.io/badge/StoryTest-11%20Acts-green.svg)](docs/acts.md)"""

    else:
        print(f"⚠️  Unknown branch type: {branch}, skipping")
        return False

    # Find and replace the badge section
    lines = content.split('\n')
    new_lines = []
    i = 0

    while i < len(lines):
        line = lines[i]

        # Replace badge section
        if line.startswith('[![Story Test Validation]'):
            # Skip existing badges until we find the end
            while i < len(lines) and (lines[i].startswith('[![') or lines[i].strip() == ''):
                i += 1
            # Add new badges
            new_lines.extend(badges.split('\n'))
            continue

        new_lines.append(line)
        i += 1

    # Write updated content
    with open(readme_path, 'w', encoding='utf-8') as f:
        f.write('\n'.join(new_lines))

    print(f"✅ Updated README.md for {branch}")
    return True

def main():
    """Main sync function"""
    branches = ["main", "develop", "pre-release", "release"]
    original_branch = get_current_branch()

    print(f"🚀 Starting README sync across branches...")
    print(f"📍 Current branch: {original_branch}")

    try:
        for branch in branches:
            print(f"\n{'='*50}")
            print(f"📂 Processing branch: {branch}")
            print(f"{'='*50}")

            # Checkout branch
            checkout_branch(branch)

            # Update README
            if update_readme_for_branch(branch):
                # Commit changes
                run_command(['git', 'add', 'README.md'])
                run_command(['git', 'commit', '-m', f'Auto-sync README badges for {branch} branch'])

                # Push changes
                run_command(f"git push origin {branch}")

                print(f"✅ Successfully updated and pushed {branch}")
            else:
                print(f"⚠️  Skipped {branch}")

        print(f"\n{'='*50}")
        print(f"🎉 README sync completed!")
        print(f"{'='*50}")

    finally:
        # Return to original branch
        print(f"\n🔄 Returning to {original_branch}...")
        checkout_branch(original_branch)

if __name__ == "__main__":
    main()