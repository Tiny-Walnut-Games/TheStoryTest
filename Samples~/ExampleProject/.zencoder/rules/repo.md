# Repository Testing Configuration

## Testing Frameworks

### Primary Framework: Unity NUnit Testing
**Framework**: Unity Test Framework (NUnit 3.x)
- **Editor Tests**: `Assets/Tests/Editor/` - Fast component and logic validation
- **Play Mode Tests**: `Assets/Tests/Runtime/` - Full game simulation and behavior testing
- **Test Helpers**: `Assets/Tests/Runtime/TestHelpers/` - Utilities and log analysis
- **Assembly Definitions**: Properly configured for test isolation
- **Installation**: Unity Test Framework v1.1.0 + Code Coverage v1.0.0

### Secondary Framework: Playwright (E2E Browser Testing)  
**Framework**: Playwright (TypeScript) - For WebGL build validation

## Unity NUnit Test Structure

### Critical Positioning Tests (`BubblePositioningTests.cs`)
**PRIMARY FOCUS**: Detects the "several cells away" positioning bug
- **Test**: `Test_CollisionPositionMatchesGridAttachment()` - Analyzes collision vs. attachment positions
- **Test**: `Test_SnapGeometryAlgorithm()` - Validates snap geometry calculations  
- **Test**: `Test_GridCoordinateSystem()` - Verifies hexagonal grid integrity
- **Log Analysis**: Automatically captures and analyzes Unity console output
- **Positioning Analysis**: Calculates error distances and consistency metrics

### Play Mode Tests (`BubbleShooterPlayModeTests.cs`)
Comprehensive game scenario testing
- Multi-shot positioning consistency analysis
- Boundary collision behavior validation
- Performance testing under various conditions
- Game startup sequence verification

### Editor Tests (`BubbleShooterEditorTests.cs`)
Fast component validation without Play Mode
- Bubble component initialization
- Hexagonal grid calculations
- Physics setup verification
- Sprite creation testing

### Test Execution Methods
```bash
# Unity Test Runner (Recommended)
Window → General → Test Runner → Run All

# Command Line (CI/CD)
Unity.exe -batchmode -runTests -testPlatform PlayMode -testResults results.xml

# Player Build Testing (Real-world conditions)
Build with Development Build + Script Debugging enabled
```

## Playwright Test Structure
- **Test Directory**: `tests/e2e/`
- **Configuration**: `playwright.config.ts`
- **Page Objects**: `tests/e2e/page-objects/`
- **Test Helpers**: `tests/e2e/test-helpers/`

## Test Categories

### 1. Game Startup Tests (`bubble-shooter-game-startup.spec.ts`)
- Unity WebGL initialization
- Canvas and UI element verification
- Story Test framework integration
- Camera and boundary setup validation

### 2. Shooting Mechanics Tests (`bubble-shooting-mechanics.spec.ts`)
- Aim and shoot functionality
- Bubble collision detection
- Grid attachment logic
- Next bubble loading system

### 3. Positioning Regression Tests (`bubble-positioning-regression.spec.ts`) 
**CRITICAL**: Addresses the primary bug where bubbles contact at one position but attach several grid cells away
- Collision point vs. attachment point analysis
- Y-Shape snap geometry validation
- Multi-shot positioning accuracy measurement
- Grid coordinate system integrity checks

### 4. Matching and Scoring Tests (`bubble-matching-scoring.spec.ts`)
- Color match detection
- Score calculation and updates
- Floating bubble removal
- Game state management during processing

### 5. Game Over and Restart Tests (`game-over-restart.spec.ts`)
- Game over condition detection
- Restart functionality
- High score persistence
- Danger line collision system
- Boundary collider validation

## Key Testing Utilities

### Page Objects
- `BubbleShooterPage`: Main game interaction wrapper
- Centralized canvas management
- Coordinate calculation helpers
- Console message capture system

### Test Helpers
- `UnityTestHelpers`: Unity-specific utilities
- WebGL initialization waiting
- Console message collection and filtering
- Multi-shot testing patterns
- Positioning accuracy analysis

## Test Execution

```bash
# Run all tests
npm test

# Interactive test runner
npm run test:ui

# Debug mode
npm run test:debug

# Specific test suite
npx playwright test bubble-positioning-regression.spec.ts
```

## WebGL Build Requirement
Tests require a WebGL build in the `WebGL-Build/` directory. See `build-webgl.md` for detailed build instructions.

## Critical Bug Documentation
The primary issue being tested is bubble positioning inaccuracy:
- **Symptom**: Bubble collides at position X but attaches at grid cell Y (several cells away)
- **Impact**: Game positioning feels incorrect and unpredictable
- **Test Focus**: `bubble-positioning-regression.spec.ts` provides detailed analysis and tracking

## TDD Approach
Tests are written following Test-Driven Development principles:
1. Tests document expected behavior
2. Tests capture and analyze current behavior
3. Tests will detect when positioning bugs are fixed
4. Tests prevent regression of fixes

## Console Message Analysis
Tests extensively use Unity console output to:
- Track collision positions
- Monitor snap geometry calculations
- Verify grid attachment decisions
- Measure positioning accuracy
- Document behavioral patterns

This testing suite provides comprehensive coverage for identifying, documenting, and eventually validating fixes for the bubble positioning accuracy issues.

---

## Modern C# Coding Standards for Unity 6+

### Language Target & Configuration
- **Target Framework**: .NET 8.0+ (aligned with Unity 6+)
- **C# Version**: C# 12.0+
- **Nullable Reference Types**: ENABLED (`<Nullable>enable</Nullable>`)
- **LangVersion**: latest (`<LangVersion>latest</LangVersion>`)

### Code Organization & Structure

#### 1. Modern Type Declarations
✅ **DO USE**:
```csharp
// Record types for immutable data (C# 9+)
public record BubblePosition(float X, float Y, float Z);

// File-scoped types
file class InternalHelper { }

// Init-only properties
public class Bubble
{
    public required string ColorId { get; init; }
    public int GridX { get; init; }
    public float Velocity { get; init; }
}

// Primary constructors (C# 12+)
public class BubbleController(BubbleModel model, ILogger logger)
{
    private readonly BubbleModel _model = model;
}
```

❌ **AVOID**:
```csharp
// Outdated serializable classes
[System.Serializable]
public class BubbleData { }

// Auto-properties without init
public class Bubble { public string ColorId { get; set; } }

// Manual constructors for simple initialization
public Bubble(string id) { ColorId = id; }
```

#### 2. Nullable Reference Types (NRT)
✅ **DO USE**:
```csharp
public class GameManager
{
    private SceneManager? _sceneManager; // Nullable explicitly
    private GameState _gameState = new(); // Non-nullable, required
    
    public void Initialize(SceneManager sceneManager)
    {
        _sceneManager = sceneManager ?? throw new ArgumentNullException(nameof(sceneManager));
    }
}
```

❌ **AVOID**:
```csharp
[SerializeField] private SceneManager sceneManager; // Implicit nullable
public SceneManager? GetManager() => null; // Implicit nullable return
```

#### 3. Expression-Bodied Members & Modern Operators
✅ **DO USE**:
```csharp
public class Bubble
{
    public float Distance => Vector3.Distance(transform.position, targetPosition);
    
    public bool IsActive => _state is BubbleState.Active or BubbleState.Animated;
    
    public void Reset() => _state = BubbleState.Inactive;
    
    public Bubble? GetNearestBubble() 
        => _bubbles.MinByOrDefault(b => Vector3.Distance(b.Position, Position));
}
```

❌ **AVOID**:
```csharp
public float GetDistance() { return Vector3.Distance(transform.position, targetPosition); }
public bool GetIsActive() { if (_state == BubbleState.Active || _state == BubbleState.Animated) return true; return false; }
```

#### 4. Pattern Matching
✅ **DO USE**:
```csharp
// Switch expressions (C# 8+)
public string GetBubbleType(Bubble bubble) => bubble switch
{
    { ColorId: "red", Size: > 1 } => "Large Red",
    { ColorId: "red" } => "Red",
    { ColorId: "blue", Size: > 2 } => "Large Blue",
    _ => "Unknown"
};

// Type patterns
public void ProcessCollision(object collision) => collision switch
{
    Bubble { Health: <= 0 } b => RemoveBubble(b),
    Bubble b => b.TakeDamage(1),
    Obstacle o => o.OnCollision(),
    _ => { }
};

// Property patterns in loops
var activeMatchesBubbles = _bubbles.Where(b => b is { IsActive: true, Matched: false });
```

❌ **AVOID**:
```csharp
public string GetBubbleType(Bubble bubble)
{
    if (bubble.ColorId == "red" && bubble.Size > 1) return "Large Red";
    if (bubble.ColorId == "red") return "Red";
    if (bubble.ColorId == "blue" && bubble.Size > 2) return "Large Blue";
    return "Unknown";
}

if (collision is Bubble)
{
    var bubble = collision as Bubble;
    if (bubble != null && bubble.Health <= 0) RemoveBubble(bubble);
}
```

#### 5. Collection Expressions (C# 12+)
✅ **DO USE**:
```csharp
// Collection initializers
private List<Bubble> _activeBubbles = []; // New syntax
private Bubble[] _bubbleGrid = new Bubble[10];

// Spread operator
var allBubbles = [..activeBubbles, ..inactiveBubbles, newBubble];

// Dictionary initializers
var colorMap = new Dictionary<string, Color> 
{ 
    ["red"] = Color.red,
    ["blue"] = Color.blue
};
```

❌ **AVOID**:
```csharp
private List<Bubble> _activeBubbles = new List<Bubble>();
var allBubbles = new List<Bubble>(activeBubbles);
allBubbles.AddRange(inactiveBubbles);
allBubbles.Add(newBubble);
```

#### 6. LINQ Modern Practices
✅ **DO USE**:
```csharp
// Method chains with modern LINQ
var matchedBubbles = _bubbles
    .Where(b => b.Matched)
    .OrderByDescending(b => b.Score)
    .Take(10)
    .ToList();

// FirstOrDefault/LastOrDefault - these return nullable!
var firstRed = _bubbles.FirstOrDefault(b => b.ColorId == "red"); // Bubble?
if (firstRed?.IsActive ?? false)
{
    // Use
}

// Index and Range (C# 8+)
var recent = _bubbles[^3..]; // Last 3 elements
var middle = _bubbles[1..^1]; // All except first and last
```

❌ **AVOID**:
```csharp
var firstRed = _bubbles.FirstOrDefault(b => b.ColorId == "red");
if (firstRed != null && firstRed.IsActive)

var recentCount = _bubbles.Count - 3;
var recent = _bubbles.GetRange(recentCount, 3);
```

#### 7. Async/Await Patterns
✅ **DO USE**:
```csharp
public async Task InitializeAsync()
{
    await LoadAssetsAsync();
    await SetupPhysicsAsync();
    Debug.Log("Initialization complete");
}

public async Task<Bubble> CreateBubbleAsync(string colorId)
{
    var color = await _colorManager.GetColorAsync(colorId);
    var bubble = new Bubble { ColorId = colorId };
    await bubble.InitializeAsync(color);
    return bubble;
}

// With cancellation support
public async Task SimulateAsync(CancellationToken cancellationToken = default)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        await Task.Delay(16, cancellationToken); // 60 FPS
    }
}
```

❌ **AVOID**:
```csharp
public void Initialize()
{
    LoadAssets();
    SetupPhysics();
}

IEnumerator InitializeCoroutine()
{
    yield return StartCoroutine(LoadAssetsCoroutine());
    yield return StartCoroutine(SetupPhysicsCoroutine());
}
```

#### 8. Resource Management
✅ **DO USE**:
```csharp
// Using declaration (C# 8+) - automatically disposes
public void ProcessBubbles()
{
    using var pooledBubbles = _bubblePool.Rent();
    // Use pooledBubbles
    // Automatically returned to pool
}

// Try-finally with using
public void LoadScene(string sceneName)
{
    using var operation = SceneManager.LoadSceneAsync(sceneName);
    try
    {
        // Handle operation
    }
    finally
    {
        // Cleanup guaranteed
    }
}
```

❌ **AVOID**:
```csharp
public void ProcessBubbles()
{
    var pooledBubbles = _bubblePool.Rent();
    try
    {
        // Use pooledBubbles
    }
    finally
    {
        _bubblePool.Return(pooledBubbles);
    }
}
```

#### 9. Parameter Validation
✅ **DO USE**:
```csharp
public class BubbleController
{
    public BubbleController(BubbleModel model, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(logger);
        
        _model = model;
        _logger = logger;
    }
    
    public void SetPosition(Vector3 position)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position.magnitude);
        _position = position;
    }
}
```

❌ **AVOID**:
```csharp
public BubbleController(BubbleModel model, ILogger logger)
{
    if (model == null) throw new ArgumentNullException(nameof(model));
    if (logger == null) throw new ArgumentNullException(nameof(logger));
    // ...
}
```

### Performance Considerations

#### 1. Avoid Allocations in Hot Paths
✅ **DO**:
```csharp
private readonly struct BubbleQuery
{
    public string ColorId { get; init; }
    public float MinDistance { get; init; }
}

public Bubble? FindNearest(in BubbleQuery query) // Using 'in' for efficiency
{
    return _bubbles.MinByOrDefault(b => 
        b.ColorId == query.ColorId ? Distance(b.Position) : float.MaxValue);
}

private float Distance(Vector3 pos) => Vector3.Distance(_position, pos);
```

❌ **AVOID**:
```csharp
public Bubble FindNearest(string colorId, float maxDistance)
{
    var query = new BubbleQuery { ColorId = colorId, MinDistance = maxDistance };
    return _bubbles.Where(b => b.ColorId == query.ColorId).FirstOrDefault();
}
```

#### 2. Object Pool Pattern (Modern)
✅ **DO**:
```csharp
public class BubblePool
{
    private readonly Queue<Bubble> _available = [];
    private readonly int _capacity;
    
    public BubblePool(int capacity) => _capacity = capacity;
    
    public Bubble Rent()
    {
        return _available.Count > 0 ? _available.Dequeue() : new Bubble();
    }
    
    public void Return(Bubble bubble)
    {
        if (_available.Count < _capacity)
        {
            bubble.Reset();
            _available.Enqueue(bubble);
        }
    }
}
```

### Obsolete Patterns to Remove

| ❌ Obsolete | ✅ Modern Replacement |
|---|---|
| `OnGUI()` | UI Toolkit or Canvas UI |
| `WWW` class | `HttpClient` or `UnityWebRequest` |
| `Rigidbody.velocity = ...` then `GetComponent<>()` calls | Direct references, dependency injection |
| `string.Format()` | String interpolation `$"{value}"` |
| `Instantiate()` without type | Typed `Instantiate<T>()` |
| Coroutines everywhere | Async/await with `UniTask` or Tasks |
| `FindObjectOfType()` in Update loops | Cached references or service locator |
| `[SerializeField]` for public data | `public required` properties with init |
| Manual singleton patterns | Dependency injection or `[RuntimeInitializeOnLoadMethod]` |

### Assembly Definitions & Code Organization

Each project area should have clear boundaries:
```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── Core.asmdef
│   │   └── [core game logic]
│   ├── UI/
│   │   ├── UI.asmdef (references: Core)
│   │   └── [UI systems]
│   ├── Physics/
│   │   ├── Physics.asmdef (references: Core)
│   │   └── [collision & movement]
│   └── Gameplay/
│       ├── Gameplay.asmdef (references: Core, Physics, UI)
│       └── [game flow]
├── Tests/
│   ├── Editor/
│   │   └── [editor tests]
│   └── Runtime/
│       └── [play mode tests]
```

### Testing Standards

- Use `[Test]` and `[UnityTest]` attributes (not `[NUnit]`)
- Leverage nullable reference types in tests for clarity
- Use expression-bodied members for simple assertions
- Modern async testing: `async Task` tests with proper await patterns

This ensures consistent, maintainable, and performant code across the project.