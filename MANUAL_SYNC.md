# README Badge Sync Instructions

## Current Status ✅

All branches now have proper badges:

- **main**: 11 Acts (stable release)
- **develop**: 11 Acts + "Coming Soon 13 Acts" badge
- **pre-release**: 13 Acts + "1.3.0 Pre-release" badge

## How to Update Badges in Future

### Option 1: Manual Update (Recommended)
1. Make your README changes on the current branch
2. For each branch that needs updating:
   ```bash
   git checkout <branch-name>
   # Update badges manually in README.md
   git add README.md
   git commit -m "Update badges for <branch-name>"
   git push origin <branch-name>
   ```

### Option 2: Use the sync scripts
- `sync_readme.py` - Full Python script (may have encoding issues)
- `quick_sync_readme.py` - Simpler Python script
- `sync-readme.ps1` - PowerShell script (needs fixing)

### Badge Configurations

**Main Branch (11 Acts):**
```markdown
[![Story Test Validation](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml/badge.svg?branch=jmeyer1980%2Fissue2)](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml)
[![Latest Stable Version](https://img.shields.io/github/v/release/jmeyer1980/TheStoryTest?label=Latest%20Stable)](https://github.com/jmeyer1980/TheStoryTest/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Python Version](https://img.shields.io/badge/python-3.8+-blue.svg)](https://www.python.org/downloads/)
[![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity.com/releases)
[![StoryTest Validation](https://img.shields.io/badge/StoryTest-11%20Acts-green.svg)](docs/acts.md)
```

**Develop Branch (11 Acts + Coming Soon):**
```markdown
[![Story Test Validation](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml/badge.svg?branch=jmeyer1980%2Fissue2)](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml)
[![Latest Stable Version](https://img.shields.io/github/v/release/jmeyer1980/TheStoryTest?label=Latest%20Stable)](https://github.com/jmeyer1980/TheStoryTest/releases/latest)
[![Latest Pre-release](https://img.shields.io/github/v/release/jmeyer1980/TheStoryTest?include_prereleases&label=Latest%20Pre-release)](https://github.com/jmeyer1980/TheStoryTest/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Python Version](https://img.shields.io/badge/python-3.8+-blue.svg)](https://www.python.org/downloads/)
[![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity.com/releases)
[![StoryTest Validation](https://img.shields.io/badge/StoryTest-11%20Acts-green.svg)](docs/acts.md)
[![Coming Soon](https://img.shields.io/badge/StoryTest-13%20Acts%20in%201.3.0-orange.svg)](https://github.com/jmeyer1980/TheStoryTest/releases)
```

**Pre-release Branch (13 Acts):**
```markdown
[![Story Test Validation](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml/badge.svg?branch=jmeyer1980%2Fissue2)](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml)
[![Latest Stable Version](https://img.shields.io/github/v/release/jmeyer1980/TheStoryTest?label=Latest%20Stable)](https://github.com/jmeyer1980/TheStoryTest/releases/latest)
[![Latest Pre-release](https://img.shields.io/github/v/release/jmeyer1980/TheStoryTest?include_prereleases&label=Latest%20Pre-release)](https://github.com/jmeyer1980/TheStoryTest/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Python Version](https://img.shields.io/badge/python-3.8+-blue.svg)](https://www.python.org/downloads/)
[![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity.com/releases)
[![StoryTest Validation](https://img.shields.io/badge/StoryTest-13%20Acts-green.svg)](docs/acts.md)
[![Version](https://img.shields.io/badge/Version-1.3.0%20Pre--release-orange.svg)](https://github.com/jmeyer1980/TheStoryTest/releases)
```

## Notes
- Always check the badge URLs are correct after updating
- Give GitHub a few minutes to refresh badge images
- Test on each branch to ensure they display properly