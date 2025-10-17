#!/usr/bin/env python3
"""
Quick README sync script - run this after making README changes to keep all branches in sync
Usage: python quick_sync_readme.py
"""

import subprocess
import sys

def run_git(cmd):
    """Run git command and return success status"""
    try:
        result = subprocess.run(cmd, shell=True, capture_output=True, text=True, check=True)
        return True, result.stdout.strip()
    except subprocess.CalledProcessError as e:
        return False, e.stderr.strip()

def sync_branch(branch, badges):
    """Sync README for a specific branch"""
    print(f"🔄 Syncing {branch}...")

    # Checkout branch
    success, output = run_git(f"git checkout {branch}")
    if not success:
        print(f"⚠️  Could not checkout {branch}: {output}")
        return False

    # Update badges in README
    try:
        with open("README.md", "r") as f:
            content = f.read()

        # Replace badge section
        lines = content.split('\n')
        new_lines = []
        i = 0

        while i < len(lines):
            line = lines[i]
            if line.startswith('[![Story Test Validation]'):
                # Skip existing badges
                while i < len(lines) and (lines[i].startswith('[![') or lines[i].strip() == ''):
                    i += 1
                # Add new badges
                new_lines.extend(badges.split('\n'))
            else:
                new_lines.append(line)
            i += 1

        with open("README.md", "w") as f:
            f.write('\n'.join(new_lines))

        # Commit and push
        run_git("git add README.md")
        run_git(f"git commit -m 'Sync README badges for {branch}'")
        run_git(f"git push origin {branch}")

        print(f"✅ {branch} synced successfully")
        return True

    except Exception as e:
        print(f"❌ Error syncing {branch}: {e}")
        return False

def main():
    """Main function"""
    # Define badges for each branch
    branch_configs = {
        "main": """[![Story Test Validation](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml/badge.svg?branch=jmeyer1980%2Fissue2)](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml)
[![Latest Stable Version](https://img.shields.io/github/v/release/jmeyer1980/TheStoryTest?label=Latest%20Stable)](https://github.com/jmeyer1980/TheStoryTest/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Python Version](https://img.shields.io/badge/python-3.8+-blue.svg)](https://www.python.org/downloads/)
[![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity.com/releases)
[![StoryTest Validation](https://img.shields.io/badge/StoryTest-11%20Acts-green.svg)](docs/acts.md)""",

        "develop": """[![Story Test Validation](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml/badge.svg?branch=jmeyer1980%2Fissue2)](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml)
[![Latest Stable Version](https://img.shields.io/github/v/release/jmeyer1980/TheStoryTest?label=Latest%20Stable)](https://github.com/jmeyer1980/TheStoryTest/releases/latest)
[![Latest Pre-release](https://img.shields.io/github/v/release/jmeyer1980/TheStoryTest?include_prereleases&label=Latest%20Pre-release)](https://github.com/jmeyer1980/TheStoryTest/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Python Version](https://img.shields.io/badge/python-3.8+-blue.svg)](https://www.python.org/downloads/)
[![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity.com/releases)
[![StoryTest Validation](https://img.shields.io/badge/StoryTest-11%20Acts-green.svg)](docs/acts.md)
[![Coming Soon](https://img.shields.io/badge/StoryTest-13%20Acts%20in%201.3.0-orange.svg)](https://github.com/jmeyer1980/TheStoryTest/releases)""",

        "pre-release": """[![Story Test Validation](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml/badge.svg?branch=jmeyer1980%2Fissue2)](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml)
[![Latest Stable Version](https://img.shields.io/github/v/release/jmeyer1980/TheStoryTest?label=Latest%20Stable)](https://github.com/jmeyer1980/TheStoryTest/releases/latest)
[![Latest Pre-release](https://img.shields.io/github/v/release/jmeyer1980/TheStoryTest?include_prereleases&label=Latest%20Pre-release)](https://github.com/jmeyer1980/TheStoryTest/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Python Version](https://img.shields.io/badge/python-3.8+-blue.svg)](https://www.python.org/downloads/)
[![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity.com/releases)
[![StoryTest Validation](https://img.shields.io/badge/StoryTest-13%20Acts-green.svg)](docs/acts.md)
[![Version](https://img.shields.io/badge/Version-1.3.0%20Pre--release-orange.svg)](https://github.com/jmeyer1980/TheStoryTest/releases)"""
    }

    # Get current branch to return to later
    success, current_branch = run_git("git branch --show-current")
    if not success:
        print("❌ Could not determine current branch")
        return

    print(f"🚀 Starting README sync from {current_branch}...")

    # Sync each branch
    for branch, badges in branch_configs.items():
        sync_branch(branch, badges)

    # Return to original branch
    run_git(f"git checkout {current_branch}")
    print(f"\n🎉 README sync completed! Returned to {current_branch}")

if __name__ == "__main__":
    main()