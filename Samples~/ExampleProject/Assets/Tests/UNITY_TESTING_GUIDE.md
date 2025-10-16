# 🎮 Unity NUnit Testing Suite for Bubble Shooter

## 🎯 Overview

This comprehensive Unity NUnit testing suite is designed to identify, document, and track the critical positioning bug in the Bubble Shooter game where "bubbles collide at one point but attach several cells away from the intended location."

## 📋 Test Suite Structure

### 🔧 **Editor Tests** (`Assets/Tests/Editor/`)
Run without Play Mode - Fast execution for component validation

- **`BubbleShooterEditorTests.cs`** - Component initialization, static logic validation
  - Bubble component setup verification
  - Hexagonal grid calculations
  - Collision constants validation
  - Sprite creation testing
  - Boundary detection logic

### 🎮 **Runtime/Play Mode Tests** (`Assets/Tests/Runtime/`)
Run with full game simulation - Comprehensive behavior testing

- **`BubblePositioningTests.cs`** - 🔴 **CRITICAL positioning bug detection**
  - Collision vs. grid attachment analysis
  - Multi-shot positioning consistency
  - Snap geometry algorithm validation
  - Detailed console log analysis

- **`BubbleShooterPlayModeTests.cs`** - Complete game scenario testing
  - Full game startup sequence
  - Multiple bubble shots analysis
  - Boundary collision behavior
  - Performance and stress testing

### 🛠️ **Test Helpers** (`Assets/Tests/Runtime/TestHelpers/`)
Utilities for test execution and analysis

- **`UnityTestHelpers.cs`** - Common testing utilities
  - Log capture and analysis system
  - Game environment setup/cleanup
  - Positioning accuracy analysis
  - Test bubble creation and simulation

## 🚀 Running the Tests

### Method 1: Unity Test Runner Window
1. Open Unity Editor
2. Go to **Window → General → Test Runner**
3. Choose **EditMode** or **PlayMode** tab
4. Click **Run All** or select specific tests
5. View results and logs in the Test Runner

### Method 2: Command Line (Unity Cloud Build/CI)
```bash
# Run all Edit Mode tests
Unity.exe -batchmode -quit -projectPath "path/to/project" -runTests -testPlatform EditMode -testResults results.xml

# Run all Play Mode tests
Unity.exe -batchmode -quit -projectPath "path/to/project" -runTests -testPlatform PlayMode -testResults results.xml
```

### Method 3: Player Build Testing
1. Build the project with **Development Build** enabled
2. Include **Script Debugging** for detailed logs
3. Run tests in the built player for real-world conditions

## 📊 Understanding Test Results

### 🔍 **Critical Positioning Tests**

The `Test_CollisionPositionMatchesGridAttachment()` test is designed to detect your positioning bug:

#### Expected Log Output:
```
=== UNITY POSITIONING TEST: Collision vs Grid Attachment Analysis ===
[SHOT 12ab34cd] Starting positioning test with direction: (0.0, 1.0)
*** COLLISION at position (8.44, 8.29, 0.00) with: TestBubble
[SNAP GEOMETRY] Final choice for contact (18,9): (19,8)
Shot Result - Direction: (0.0, 1.0), Collision: (8.44, 8.29, 0.00), Grid: (19, 8), Distance: 15.23
POSITIONING BUG DETECTED: Collision at (8.44, 8.29, 0.00) but attached at grid (19, 8). Error distance: 15.23 units
```

#### Key Metrics to Monitor:
- **Positioning Error**: Distance between collision point and final attachment
- **Error Percentage**: % of shots with significant positioning errors (>2 units)
- **Consistency**: Variation in positioning accuracy across multiple shots

### 📈 **Success Criteria**

**Tests will PASS when:**
- Positioning error < 2.0 units for 90%+ of shots
- Collision position ≈ Final attachment position
- No "several cells away" displacements occur
- Snap geometry algorithm calculates expected positions

**Current Expected Status: FAILING ❌**
- Tests document the existing positioning bug
- Provide detailed analysis for debugging
- Will validate fixes when implemented

## 🔧 Log Analysis System

The test suite includes an advanced log capture system:

### **Log Categories:**
- **Collision** - `*** COLLISION at position` messages
- **Positioning** - `[SNAP GEOMETRY]` calculations  
- **Shooting** - `[BARREL FIRED]` shot initiation
- **Synchronization** - `[SYNC]` grid updates
- **Testing** - Test framework messages

### **Positioning Analysis:**
- Automatic extraction of collision coordinates
- Grid attachment position parsing
- Error distance calculations
- Statistical analysis across multiple shots

## 🐛 Debugging with Test Results

### 1. **Collect Test Logs**
Run the positioning tests and collect the Unity console output:
```csharp
[TEST LOGS] [2.45s] [Collision] *** COLLISION at position (8.44, 8.29, 0.00)
[TEST LOGS] [2.47s] [Positioning] [SNAP GEOMETRY] Final choice: (19,8)
```

### 2. **Analyze Positioning Data**
The tests automatically calculate:
- Distance between collision and attachment
- Frequency of positioning errors
- Pattern analysis across shot directions

### 3. **Report Bug Data**
Share the test results including:
- Console log output
- Positioning analysis summary
- Screenshots of failed test results
- Unity version and platform information

## 🔄 Test-Driven Development Workflow

### Current Phase: **RED** ❌
1. **Document the Bug**: Tests capture current incorrect behavior
2. **Analyze Root Cause**: Use positioning data to investigate snap geometry
3. **Provide Debugging Tools**: Detailed logs and analysis for investigation

### Future Phase: **GREEN** ✅
1. **Validate Fixes**: Tests confirm positioning accuracy improvements
2. **Prevent Regressions**: Continuous testing catches future positioning issues
3. **Performance Monitoring**: Ensure fixes don't impact game performance

## 🎯 Specific Test Scenarios

### **Multi-Direction Positioning Test**
Tests bubble shots in various directions to identify positioning patterns:
- Straight up (0.0, 1.0)
- Slight angles (±0.2, 1.0)
- Diagonal shots (±0.4, 0.8)
- Sharp angles (±0.6, 0.6)

### **Rapid Shot Performance Test**
Validates positioning consistency under rapid-fire conditions

### **Dense Grid Testing**
Tests positioning accuracy when the grid is nearly full

### **Boundary Collision Testing**
Verifies wall bounce behavior doesn't affect subsequent positioning

## 📝 Customizing Tests

### Adding New Test Cases:
```csharp
[UnityTest]
public IEnumerator Test_YourCustomScenario()
{
    yield return UnityTestHelpers.SetupTestGameEnvironment();
    
    // Your test logic here
    var testBubble = UnityTestHelpers.CreateTestBubble(0, 0, Color.red);
    yield return UnityTestHelpers.SimulateBubbleShot(testBubble, Vector2.up);
    
    // Analyze results
    var analysis = logCapture.AnalyzePositioning();
    analysis.LogAnalysis();
}
```

### Modifying Log Capture:
```csharp
// Capture specific log patterns
var collisionLogs = logCapture.GetLogsByKeyword("COLLISION");
var snapLogs = logCapture.GetLogsByKeyword("SNAP GEOMETRY");
```

## ⚡ Performance Considerations

- **Editor Tests**: Run quickly, suitable for frequent execution
- **Play Mode Tests**: More comprehensive but slower
- **Build Tests**: Most realistic but longest execution time

## 🔍 Troubleshooting

### Common Issues:

1. **Tests Not Appearing in Test Runner**
   - Verify assembly definition files are correctly configured
   - Check that test classes inherit from appropriate base classes
   - Ensure `[Test]` and `[UnityTest]` attributes are present

2. **Game Manager Not Found**
   - Tests create their own game environment
   - Ensure `UnityTestHelpers.SetupTestGameEnvironment()` is called
   - Check that BubbleShooterGameManager exists in the project

3. **Log Capture Not Working**
   - Verify `LogCapture.StartCapture()` is called in `OneTimeSetUp`
   - Check that Unity console logging is enabled
   - Ensure test execution doesn't filter out debug messages

4. **Assembly Reference Errors**
   - Verify `BubbleShooter.Scripts.asmdef` exists and is configured
   - Check that test assembly definitions reference the main scripts
   - Ensure NUnit references are correctly set up

## 📋 Next Steps

1. **Run Initial Tests**: Execute the full test suite to establish baseline
2. **Analyze Results**: Review positioning analysis and error patterns
3. **Share Data**: Provide test logs and analysis for debugging
4. **Iterate**: Use test results to guide investigation and fixes
5. **Validate**: Confirm tests pass after positioning fixes are implemented

---

## 🎉 **Your Unity NUnit Testing Suite is Ready!**

This comprehensive testing framework will help identify the root cause of the positioning bug and ensure it's properly fixed without introducing regressions. The tests provide detailed console logging and analysis that can be collected and shared for debugging purposes.

**Framework**: Unity NUnit (Editor + Play Mode) ✅  
**TDD Implementation**: Complete ✅  
**Critical Bug Focus**: Collision vs. Attachment Analysis ✅  
**Log Analysis**: Automated positioning analysis ✅  
**Documentation**: Comprehensive testing guide ✅