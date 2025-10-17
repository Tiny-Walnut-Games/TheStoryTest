# E2E Testing Suite for Bubble Shooter Game

## 🎯 Overview

This comprehensive E2E testing suite is designed using **Test-Driven Development (TDD)** principles to identify, document, and prevent regression of the critical bubble positioning bug in the Bubble Shooter game.

**Primary Issue**: Bubbles collide at one position but attach several grid cells away from the intended location.

## 🧪 Testing Framework

**Framework**: Playwright (TypeScript)  
**Approach**: TDD with detailed console analysis  
**Target**: Unity WebGL build  

## 📁 Test Structure

```
tests/e2e/
├── bubble-shooter-game-startup.spec.ts          # Game initialization
├── bubble-shooting-mechanics.spec.ts            # Shooting system
├── bubble-positioning-regression.spec.ts        # 🔴 CRITICAL positioning tests
├── bubble-matching-scoring.spec.ts              # Match & score system
├── game-over-restart.spec.ts                    # End game logic
├── page-objects/
│   └── bubble-shooter-page.ts                   # Page object model
└── test-helpers/
    └── unity-helpers.ts                         # Unity-specific utilities
```

## 🔍 Test Categories

### 1. Game Startup Tests ✅
- **Purpose**: Verify Unity WebGL loads properly
- **Key Tests**:
  - Canvas initialization and visibility
  - Story Test framework integration
  - Camera and grid setup validation
  - UI element presence

### 2. Shooting Mechanics Tests 🎯
- **Purpose**: Test core shooting functionality
- **Key Tests**:
  - Mouse aim and click shooting
  - Bubble trajectory and collision detection
  - Next bubble loading system
  - Bullet flight state management

### 3. Positioning Regression Tests 🔴 **CRITICAL**
- **Purpose**: Document and track the positioning bug
- **Key Tests**:
  - **Collision vs. Attachment Analysis**: Measures distance between collision point and final attachment
  - **Y-Shape Snap Geometry Validation**: Tests the grid snapping algorithm
  - **Multi-Shot Accuracy**: Measures positioning consistency across multiple shots
  - **Grid Coordinate System Integrity**: Validates the underlying grid system

### 4. Matching and Scoring Tests 🎊
- **Purpose**: Verify bubble matching and scoring
- **Key Tests**:
  - Color match detection (3+ bubbles)
  - Score calculation and UI updates
  - Floating bubble removal system
  - Game state during match processing

### 5. Game Over and Restart Tests 🏁
- **Purpose**: Test end-game scenarios
- **Key Tests**:
  - Game over condition detection
  - Danger line collision system  
  - Restart functionality
  - High score persistence
  - Boundary collider validation

## 🚀 Quick Start

### Prerequisites
1. **Unity WebGL Build**: Create a WebGL build in `WebGL-Build/` folder
   ```bash
   # See build-webgl.md for detailed instructions
   ```

2. **Install Dependencies**:
   ```bash
   npm install
   npx playwright install
   ```

### Running Tests

```bash
# Run all tests
npm test

# Interactive test runner with visual debugging
npm run test:ui

# Run only positioning regression tests (most critical)
npx playwright test bubble-positioning-regression.spec.ts

# Debug mode with step-by-step execution
npm run test:debug

# PowerShell helper script with options
.\run-tests.ps1 -TestType positioning -UI
```

## 🔬 Positioning Bug Analysis

The most critical tests are in `bubble-positioning-regression.spec.ts`:

### Test: "CRITICAL: should attach bubble at or near collision point"
- **What it does**: Shoots a bubble and measures collision vs. attachment positions
- **Expected**: Bubble attaches at or very near the collision point
- **Current Reality**: Documents the actual positioning behavior
- **Output**: Detailed console analysis showing:
  ```
  Collision detected at: (8.44, 8.29, 0.00)
  Final grid attachment: (18,9)
  Time between collision and attachment: 45ms
  ```

### Test: "should verify Y-Shape snap geometry algorithm" 
- **What it does**: Analyzes the grid snapping calculations
- **Purpose**: Identifies if the issue is in the snap geometry logic
- **Monitors**: `[SNAP Y-SHAPE]` and `FindBestCellForContact` messages

### Test: "should measure positioning accuracy across multiple shots"
- **What it does**: Takes multiple shots and compares positioning consistency
- **Purpose**: Determines if the bug is consistent or random
- **Output**: Statistical analysis of positioning accuracy

## 🎮 Console Message Analysis

The tests extensively analyze Unity console output to track:

- **Collision Events**: `COLLISION at position (x, y, z)`
- **Grid Calculations**: `[SNAP Y-SHAPE] Processing N overlapping colliders`
- **Final Positioning**: `[SNAP GEOMETRY] Final choice for contact`
- **State Transitions**: `Bubble in flight: True/False`

## 📊 TDD Benefits

1. **Documentation**: Tests document expected vs. actual behavior
2. **Regression Prevention**: Will catch if positioning gets worse
3. **Fix Validation**: Will confirm when the bug is fixed
4. **Behavioral Analysis**: Provides detailed data about the positioning system

## 🔧 Troubleshooting

### Common Issues:
- **Unity WebGL not loading**: Check browser console, ensure build is complete
- **Tests timeout**: Increase timeout values, Unity may take longer to initialize
- **Canvas not found**: Ensure Unity template includes `canvas#unity-canvas`
- **Console messages not captured**: Verify Unity debug logging is enabled

### Debug Tips:
- Use `npm run test:ui` for visual debugging
- Check `test-results/` folder for screenshots and videos
- Run individual test files to isolate issues
- Use `console.log` statements in tests to trace execution

## 🎯 Success Criteria

**Tests will PASS when:**
1. Bubbles attach within 1-2 grid cells of collision point
2. Positioning is consistent across multiple shots
3. Snap geometry algorithm calculates correct final positions
4. No "several cells away" positioning occurs

**Current Status**: Tests document the bug and provide analysis tools for debugging and eventual validation of fixes.

---

*This E2E suite follows TDD principles: write tests first, capture current behavior, then use tests to validate fixes and prevent regressions.*