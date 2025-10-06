# Code Coverage Guide for Story Test

## Understanding "Coverage" - Two Different Meanings

### 1. 📊 **Code Coverage** (What Unity/dotCover Measure)
**Definition:** Percentage of your *production code lines* that execute when tests run.

**Example:**
```csharp
public int Calculate(int x) {
    if (x < 0) {           // ✅ Line 2 executed
        return 0;          // ❌ Line 3 NOT executed (no test uses negative x)
    }
    return x * 2;          // ✅ Line 5 executed
}
```
**Code Coverage: 66%** (2 out of 3 lines executed)

**Tools:**
- **Unity Code Coverage Package** - Built-in, generates HTML reports
- **JetBrains dotCover** - Commercial tool, IDE integration
- **OpenCover / Coverlet** - Open source alternatives

**Goal:** Aim for 80-90% coverage of critical paths. 100% is often impractical.

---

### 2. 🎯 **Test Coverage** (What Developers Track Manually)
**Definition:** Which *features/components/scenarios* have tests written for them.

**Example:**
- ✅ Act1TodoComments - Has tests
- ✅ ValidationReport - Has tests
- ✅ StoryTestSettings - Has tests
- ❌ Some edge case - No test yet

**No tool needed** - Track via checklists, requirements, or code reviews.

**Goal:** Every major feature should have at least one test.

---

## Setting Up Unity Code Coverage

### Step 1: Install the Package

1. Open Unity Package Manager (`Window > Package Manager`)
2. Click `+` dropdown → `Add package by name`
3. Enter: `com.unity.testtools.codecoverage`
4. Click `Add`

### Step 2: Enable Code Coverage

#### Option A: Via Test Runner (Recommended)
1. Open Test Runner (`Window > General > Test Runner`)
2. Click the hamburger menu (☰) in top-right
3. Select `Enable Code Coverage`
4. Check `Enable Code Coverage` option
5. Run your tests

#### Option B: Via Project Settings
1. Go to `Edit > Project Settings > Editor`
2. Find "Code Coverage" section
3. Enable code coverage options
4. Configure coverage results path

### Step 3: Generate Coverage Report

Run tests with coverage enabled:

```bash
# Command line (for CI/CD)
Unity.exe -runTests -testPlatform EditMode -enableCodeCoverage -coverageResultsPath ./CodeCoverage
```

Or use the Test Runner UI with coverage enabled.

### Step 4: View the Report

After tests complete:
1. Navigate to your project's `CodeCoverage` folder
2. Open `index.html` in a web browser
3. Browse through:
   - **Summary** - Overall coverage percentage
   - **Assembly View** - Coverage by assembly
   - **Class View** - Coverage by class
   - **Source View** - Line-by-line highlighting

### What the Colors Mean
- 🟢 **Green** - Line executed during tests
- 🔴 **Red** - Line never executed
- 🟡 **Yellow** - Partially covered (e.g., only one branch of an if statement)

---

## Interpreting Coverage Results

### Good Coverage (80%+)
```
Act1TodoComments.cs:           95% ✅
Act2PlaceholderImpl.cs:        88% ✅
StoryIntegrityValidator.cs:    92% ✅
ValidationReport.cs:           87% ✅
```

### Areas That Need Attention
```
StoryTestSyncPointValidator.cs: 45% ⚠️ (Complex async code, hard to test)
EditorUI.cs:                    20% ⚠️ (UI code, typically low coverage)
```

### Don't Stress About These:
- **Editor UI Code** - Hard to unit test, requires integration tests
- **Platform-specific code** - May only run on certain platforms
- **Error handling paths** - Hard to trigger artificially
- **Debug-only code** - Won't execute in release builds

---

## Coverage Goals for Story Test

### 🎯 Target Coverage Levels

| Component                  | Target | Why                                    |
|----------------------------|--------|----------------------------------------|
| **Act Rules** (Act1-9)     | 90%+   | Core validation logic - critical       |
| **StoryIntegrityValidator**| 85%+   | Orchestration logic - important        |
| **ValidationReport**       | 80%+   | Reporting - users depend on this       |
| **Settings**               | 75%+   | Configuration - medium priority        |
| **Editor Tools**           | 40%+   | UI code - low priority for unit tests  |
| **Sync Point Performance** | 60%+   | Async/concurrent - harder to test      |

---

## Continuous Integration Setup

### GitHub Actions Example

```yaml
name: Tests with Coverage

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2

      - uses: game-ci/unity-test-runner@v2
        with:
          testMode: EditMode
          coverageOptions: 'generateAdditionalMetrics;generateHtmlReport;generateBadgeReport'

      - name: Upload Coverage Report
        uses: actions/upload-artifact@v2
        with:
          name: coverage-report
          path: CodeCoverage/

      - name: Coverage Badge
        run: echo "Coverage: $(cat CodeCoverage/Report/Summary.xml | grep -oP 'linecoverage="\K[^"]*')%"
```

---

## Best Practices

### ✅ DO:
- **Focus on critical paths** - Test the most important code first
- **Test business logic** - Act rules, validation logic, core algorithms
- **Aim for 80% overall** - Good balance of coverage vs. effort
- **Track trends** - Monitor coverage over time, don't let it drop
- **Use coverage to find gaps** - Red lines = missing tests

### ❌ DON'T:
- **Chase 100% coverage** - Law of diminishing returns
- **Test getters/setters** - Waste of time unless they have logic
- **Test Unity lifecycle methods** - Hard to unit test, use integration tests
- **Game the system** - Writing tests that don't validate behavior just to hit 100%
- **Ignore test quality** - High coverage with bad tests = false confidence

---

## Story Test Coverage Status (Current)

### Test Suites ✅
- [x] **StoryTestValidationTests** - Core framework tests
- [x] **ConceptualValidationTests** - Conceptual validation (enums, structs, etc.)
- [x] **IntegrationTests** - End-to-end validation
- [x] **ActRulesTests** - Individual Act rule validation ⭐ NEW
- [x] **ValidationReportTests** - Reporting functionality ⭐ NEW
- [x] **StoryTestSettingsTests** - Configuration system ⭐ NEW
- [x] **StoryViolationTests** - Violation data structures ⭐ NEW
- [x] **SyncPointPerformanceTests** - Performance validation ⭐ NEW

### Estimated Coverage
- **Acts (validation rules)**: ~85% (solid)
- **Core framework**: ~90% (excellent)
- **Reporting**: ~85% (solid)
- **Settings**: ~80% (good)
- **Editor tools**: ~30% (expected - UI code)
- **Overall**: ~75-80% (very good!)

---

## Next Steps

1. **Install Unity Code Coverage Package** - See Step 1 above
2. **Run Tests with Coverage Enabled** - See Step 2 above
3. **Review the HTML Report** - Identify gaps
4. **Add Tests for Red Areas** - Focus on critical paths
5. **Set Up CI Pipeline** - Automate coverage tracking

---

## Questions?

**Q: What's a good coverage target for a validation framework?**
A: 80-85% is excellent. Higher is great but has diminishing returns.

**Q: Should I test private methods?**
A: No - test through public APIs. Private methods get covered indirectly.

**Q: My coverage is 95% but tests are still failing. Why?**
A: High coverage ≠ good tests. You need both execution AND assertions.

**Q: How do I increase coverage?**
A: 1) Look at HTML report for red lines, 2) Add tests for those scenarios, 3) Repeat.

**Q: Unity Code Coverage vs dotCover?**
A: Same concept, different tools. Unity Code Coverage is free and integrates better with Unity Test Runner.

---

**Remember:** Coverage is a tool, not a goal. The real goal is **confidence** that your code works correctly! 🎯
