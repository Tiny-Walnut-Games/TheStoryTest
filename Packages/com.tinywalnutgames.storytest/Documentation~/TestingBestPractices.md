# Testing Best Practices for Story Test

## The Testing Pyramid 🔺

```
      /\      E2E Tests (Few)
     /  \     Integration Tests (Some)
    /____\    Unit Tests (Many)
```

### Unit Tests (Most of your tests)
- Test individual components in isolation
- Fast (milliseconds)
- Example: Testing Act1TodoComments.Rule() with sample methods

### Integration Tests (Moderate amount)
- Test components working together
- Slower (seconds)
- Example: Testing ProductionExcellenceStoryTest running full validation

### E2E Tests (Minimal)
- Test entire system from user perspective
- Slowest (minutes)
- Example: Running validation via Unity menu, checking report file

---

## Test Naming Conventions

### Good Test Names ✅
```csharp
[Test]
public void Act1_DetectsTodoComments()
public void ValidationReport_AddViolations_TracksByPhase()
public void StoryIgnoreAttribute_RequiresReason()
public void SyncPoint_QuickTest_CompletesSuccessfully()
```

**Pattern:** `[Component]_[Scenario]_[ExpectedBehavior]`

### Bad Test Names ❌
```csharp
[Test]
public void Test1()                    // Too vague
public void TestActOne()               // Doesn't explain what it tests
public void ISSUE_123()                // Issue tracker reference, not descriptive
public void ItWorks()                  // What works?
```

---

## The AAA Pattern

Every test should follow: **Arrange, Act, Assert**

```csharp
[Test]
public void ValidationReport_AddViolations_TracksByPhase()
{
    // ARRANGE - Set up test data
    var report = new ValidationReport();
    var violation = new StoryViolation
    {
        Type = "TestType",
        Member = "TestMember",
        Violation = "Test violation"
    };

    // ACT - Perform the action
    report.AddViolations("TestPhase", new[] { violation });

    // ASSERT - Verify the result
    Assert.AreEqual(1, report.StoryViolations.Count);
    Assert.IsTrue(report.PhaseViolations.ContainsKey("TestPhase"));
}
```

---

## Performance Test Best Practices

### 🎭 The "No Tripping Understudies" Rule

Performance tests ensure actors (validation workers) don't interfere with each other:

```csharp
[Test]
public async Task SyncPoint_Performance_MeetsMinimumThroughput()
{
    var startTime = DateTime.UtcNow;
    var result = await StoryTestSyncPointValidator.QuickSyncPointTest();
    var duration = DateTime.UtcNow - startTime;

    // Log results, don't hard-fail on slow CI
    if (!result)
    {
        Assert.Warn("Performance below threshold");
    }
}
```

**Key Points:**
- ✅ Measure throughput (ops/sec)
- ✅ Detect timing variations (actors blocking each other)
- ✅ Export detailed reports
- ⚠️ Use `Assert.Warn()` not `Assert.Fail()` - CI environments vary
- 📊 Log metrics for trend analysis

---

## Async Testing Patterns

### Good Async Tests ✅

```csharp
[Test]
public async Task SyncPoint_QuickTest_CompletesSuccessfully()
{
    // Use Assert.DoesNotThrowAsync for async operations
    Assert.DoesNotThrowAsync(async () =>
    {
        await StoryTestSyncPointValidator.QuickSyncPointTest();
    });
}

[Test]
public async Task SyncPoint_CanRunConcurrently()
{
    var task1 = StoryTestSyncPointValidator.QuickSyncPointTest();
    var task2 = StoryTestSyncPointValidator.QuickSyncPointTest();

    await Task.WhenAll(task1, task2);

    Assert.IsTrue(task1.IsCompleted && task2.IsCompleted);
}
```

### Bad Async Tests ❌

```csharp
[Test]
public void SyncPoint_Test_Bad()
{
    // ❌ Don't use .Result or .Wait() - deadlock risk
    var result = StoryTestSyncPointValidator.QuickSyncPointTest().Result;
}

[Test]
public async void SyncPoint_Test_AlsoBad()  // ❌ async void
{
    await StoryTestSyncPointValidator.QuickSyncPointTest();
}
```

**Rules:**
- ✅ Use `async Task` (not `async void`)
- ✅ Use `await` (not `.Result` or `.Wait()`)
- ✅ Use `Assert.DoesNotThrowAsync`

---

## Testing Edge Cases

### The "Boundary Value Analysis" Technique

Test values at boundaries:

```csharp
[Test]
public void Act8_HollowEnums_BoundaryValues()
{
    // Test: 0 values (impossible in C#, but validate rejection)
    // Test: 1 value (minimum viable, should fail HollowEnums rule)
    // Test: 2 values (minimum passing)
    // Test: Many values (definitely passing)
}
```

**Common Boundaries:**
- Empty collections
- Single-item collections
- null values
- Empty strings
- Whitespace-only strings
- Maximum values
- Negative values (when not expected)

---

## Test Data Management

### Test Fixtures (Shared Setup)

```csharp
public class ActRulesTests
{
    // Test classes used across multiple tests
    public class TestClassWithViolations
    {
        public void MethodWithNotImplemented()
        {
            throw new NotImplementedException();
        }
    }

    public class TestClassWithoutViolations
    {
        public int ImplementedMethod() => 42;
    }
}
```

### Test-Specific Data

```csharp
[Test]
public void ValidationReport_AddViolations_TracksByPhase()
{
    // Create data inline for single-use scenarios
    var violation = new StoryViolation
    {
        Type = "TestType",
        Member = "TestMember",
        Violation = "Test violation"
    };
}
```

---

## What NOT to Test

### ❌ Skip These:

1. **Simple Getters/Setters**
   ```csharp
   public string Name { get; set; }  // Don't test this
   ```

2. **Third-Party Code**
   ```csharp
   // Don't test Unity's GameObject.AddComponent
   // Trust that Unity tested it
   ```

3. **Framework Internals**
   ```csharp
   // Don't test that NUnit's Assert.AreEqual works
   ```

4. **Trivial Constructors**
   ```csharp
   public MyClass() { }  // Nothing to test here
   ```

### ✅ Do Test These:

1. **Business Logic**
   ```csharp
   // Act rules that detect violations
   Act1TodoComments.Rule(method, out message)
   ```

2. **Complex Calculations**
   ```csharp
   // Production readiness score
   report.ProductionReadinessScore
   ```

3. **Conditional Logic**
   ```csharp
   // StoryIgnore attribute validation
   if (string.IsNullOrWhiteSpace(reason))
       throw new ArgumentException();
   ```

4. **State Changes**
   ```csharp
   // Validation report tracking violations by phase
   report.AddViolations("Phase", violations)
   ```

---

## Test Organization

### File Structure

```
Tests/
├── StoryTestValidationTests.cs      # Core framework tests
├── ConceptualValidationTests.cs     # Conceptual validation
├── IntegrationTests.cs              # End-to-end tests
├── ActRulesTests.cs                 # Individual Act tests
├── ValidationReportTests.cs         # Reporting tests
├── StoryTestSettingsTests.cs        # Settings tests
├── StoryViolationTests.cs           # Data structure tests
└── SyncPointPerformanceTests.cs     # Performance tests
```

### Test Class Naming

- `[Component]Tests.cs` - e.g., `ValidationReportTests.cs`
- `[Feature]Tests.cs` - e.g., `ActRulesTests.cs`
- Group related tests in the same file

---

## Debugging Failed Tests

### Steps:

1. **Read the Assertion Message**
   ```
   Assert.AreEqual(2, violations.Count)
   Expected: 2
   But was:  3
   ```

2. **Check the Stack Trace**
   ```
   at ActRulesTests.Act1_DetectsTodoComments() line 42
   ```

3. **Add Debug Logging**
   ```csharp
   UnityEngine.Debug.Log($"Violations: {violations.Count}");
   ```

4. **Run Single Test**
   - Right-click test in Test Runner
   - Select "Run"

5. **Use Debugger**
   - Set breakpoint
   - Right-click test → "Debug"

---

## CI/CD Integration

### Example: Running Tests in CI

```yaml
# .github/workflows/tests.yml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2

      - name: Run Tests
        uses: game-ci/unity-test-runner@v2
        with:
          testMode: EditMode

      - name: Upload Test Results
        uses: actions/upload-artifact@v2
        with:
          name: test-results
          path: artifacts/
```

---

## Common Test Smells (Anti-Patterns)

### 🚨 Test Smells to Avoid:

1. **Tests That Depend on Execution Order**
   ```csharp
   // ❌ Bad: Test2 depends on Test1 running first
   [Test] public void Test1_SetupData() { }
   [Test] public void Test2_UseData() { }
   ```

2. **Tests That Depend on External State**
   ```csharp
   // ❌ Bad: Depends on specific file existing
   var data = File.ReadAllText("C:/specific/path/file.txt");
   ```

3. **Flaky Tests (Pass Sometimes, Fail Sometimes)**
   ```csharp
   // ❌ Bad: Depends on timing
   await Task.Delay(100);  // Hope this is enough time!
   Assert.IsTrue(isCompleted);
   ```

4. **Tests That Test Multiple Things**
   ```csharp
   // ❌ Bad: Tests settings AND report AND validation
   [Test] public void TestEverything() { }
   ```

---

## Measuring Test Quality

### Good Tests Have:

- ✅ **Clear intent** - Name describes what's tested
- ✅ **Focused** - Tests one thing
- ✅ **Fast** - Runs in <1 second
- ✅ **Independent** - Doesn't depend on other tests
- ✅ **Repeatable** - Same result every time
- ✅ **Self-validating** - Pass/fail is clear

### Test Quality Checklist:

- [ ] Can I understand what this test does in 10 seconds?
- [ ] If this test fails, will I know exactly what broke?
- [ ] Does this test run fast (<1s for unit tests)?
- [ ] Can I run this test in isolation?
- [ ] Will this test give the same result every time?

---

## Resources

- [Unity Test Framework Documentation](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)
- [NUnit Documentation](https://docs.nunit.org/)
- [Story Test Code Coverage Guide](./CodeCoverageGuide.md)
- [Test-Driven Development (TDD)](https://en.wikipedia.org/wiki/Test-driven_development)

---

**Remember:** The goal of testing is **confidence**, not just coverage numbers! 🎯
