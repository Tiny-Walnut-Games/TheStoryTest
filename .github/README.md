# The Story Test Framework

[![Story Test Validation](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml/badge.svg?branch=jmeyer1980%2Fissue2)](https://github.com/jmeyer1980/TheStoryTest/actions/workflows/story-test.yml)
[![Latest Stable Version](https://img.shields.io/github/v/release/jmeyer1980/TheStoryTest?label=Latest%20Stable)](https://github.com/jmeyer1980/TheStoryTest/releases/latest)
[![Latest Pre-release](https://img.shields.io/github/v/release/jmeyer1980/TheStoryTest?include_prereleases&label=Latest%20Pre-release)](https://github.com/jmeyer1980/TheStoryTest/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Python Version](https://img.shields.io/badge/python-3.8+-blue.svg)](https://www.python.org/downloads/)
[![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity.com/releases)
[![StoryTest Validation](https://img.shields.io/badge/StoryTest-11%20Acts-green.svg)](docs/acts.md)
[![Coming Soon](https://img.shields.io/badge/StoryTest-13%20Acts%20in%201.3.0-orange.svg)](https://github.com/jmeyer1980/TheStoryTest/releases)

## **Symbol Integrity & Narrative Completeness for C# Projects**

<table>
<tr>
<td width="220" align="center" valign="middle">
  <img src="https://media.githubusercontent.com/media/Tiny-Walnut-Games/TheStoryTest/develop/.github/WarblerMascotStickerized.png" 
       alt="Warbler Mascot" width="160"/>
</td>
<td width="*" valign="top">

A code quality validation framework that enforces the "Story Test Doctrine": every symbol (method, property, parameter, enum) must be fully implemented and meaningful—no placeholders, TODOs, or unused code in production.

Originally designed for Unity ECS/DOTS projects, Story Test is now **Unity-agnostic** and works with any C# codebase, including GameObject-based Unity projects and pure .NET applications.

## 🚀 Quick Start

### Installation
Download and extract the zip/tar.gz and extract it into your packages or assets folder. I recommend asking your agent to validate your project by setting up The Story Test and its settings.json to match your project's needs. As long as the agent has tool access, it will be able to install all prerequisites and validate your project for you.

</td>
</tr>
</table>

## 🚀 Quick Start

### Installation
Add to Unity Package Manager via git URL:
```
https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest
```

### First Validation
```bash
# Python validator (no Unity required)
pip install -r requirements.txt
python scripts/story_test_unity_safe.py . --verbose

# Unity Editor
Tiny Walnut Games > The Story Test > Run Story Test and Export Report
```

## 📚 Documentation

- **[Getting Started](docs/getting-started.md)** - Installation and first validation
- **[The 11 Acts](docs/acts.md)** - Complete validation rules reference
- **[Configuration](docs/configuration.md)** - Settings and customization
- **[CI/CD Integration](docs/ci-cd.md)** - GitHub Actions and automation
- **[Python Validator](docs/python-validator.md)** - Standalone validation
- **[Changelog](CHANGELOG.md)** - Version history and changes

## ✨ Features

- **11 Validation Acts** - IL bytecode analysis for code quality
- **Cross-Platform** - Unity Editor + standalone Python validator
- **CI/CD Ready** - GitHub Actions, Azure DevOps, GitLab CI integration
- **Zero Dependencies** - Works without Unity installation
- **Production Proven** - Asset Store published developer

## 🎯 Core Validation

- **Act 1**: Todo Comments (`NotImplementedException`)
- **Act 2**: Placeholder Implementations (minimal IL)
- **Act 3**: Incomplete Classes (abstract methods)
- **Act 4**: Unsealed Abstract Members
- **Act 5**: Debug Only Implementations (`[Obsolete]`)
- **Act 6**: Phantom Props (unused properties)
- **Act 7**: Cold Methods (empty methods)
- **Act 8**: Hollow Enums (minimal values)
- **Act 9**: Premature Celebrations (complete but throwing)
- **Act 10**: Suspiciously Simple Methods (constant returns)
- **Act 11**: Dead Code (unused members)

## 🏗️ Architecture

```
Packages/com.tinywalnutgames.storytest/
├── Runtime/                    # Core validation logic
│   ├── Acts/                  # 11 validation rules
│   └── Shared/                # Unity-agnostic types
├── Editor/                     # Unity Editor integration
├── Tests/                      # NUnit test suite
└── Documentation~/             # Unity package docs
```

## 📋 Requirements

- **Unity**: 2020.3 LTS or later
- **Python**: 3.8+ (for standalone validator)
- **.NET**: Standard 2.0 or later

## 📄 License

MIT License - see [LICENSE](LICENSE) for details.

## 📋 Operations & Maintenance

### Release & Branch Management

- **[Workflows & Release Strategy](./WORKFLOWS.md)** - Three-branch workflow, branch protection, dependency management
  - Daily development workflow
  - Release process automation
  - Branch protection configuration
  - Package dependency management

### Historical Reference

- **[Workflow Fixes & Changelog](./FIXES_CHANGELOG.md)** - Historical issues and resolutions
  - Version tag migration (December 2024)
  - Linter error fixes (October 2024)
  - Platform identifier fixes (October 2024)

### For Maintainers

- **[Release Process](docs/RELEASE_PROCESS.md)** - Automated releases and versioning
- Releases are automated via GitHub Actions when version tags are pushed
- Use `./scripts/release.sh` for easy version bumping

### For AI Assistants

- **[AI Guidelines](./../.zencoder/rules/ai-guidelines.md)** - Reality-first instructions for AI helpers
  - Reality anchor protocol
  - Developer communication preferences
  - Verification requirements

## 🆘 Support

- [GitHub Issues](https://github.com/jmeyer1980/TheStoryTest/issues)
- [Discussions](https://github.com/jmeyer1980/TheStoryTest/discussions)
- [Documentation](https://github.com/jmeyer1980/TheStoryTest/tree/main/docs)

---

**Remember**: Every symbol in your assembly should read like a finished chapter. If a parameter, method, or enum feels like foreshadowing, seal it with intent or finish the scene before shipping.
