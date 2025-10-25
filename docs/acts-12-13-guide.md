# Acts 12-13: Extended Validation (Optional)

Acts 12 and 13 are **optional, assembly-level** validations that check your project's architecture and mental model claims. They are not enabled by default but can be used for additional validation.

## Act 12: Mental Model Claims

**Purpose**: Verify that your `storytest-mental-model.json` configuration claims are actually supported by code artifacts.

**What it checks**:
- Required artifacts exist at their declared paths
- Claimed capabilities (platforms, features) have implementation evidence
- No "narrative holes" where you claimed something but didn't implement it

**How to enable**:
```csharp
using TinyWalnutGames.StoryTest.Acts;

// Get the Act 12 rule
var act12Rule = ActRegistry.GetActRule(12);

// Validate (pass null for assembly-level validation)
bool hasViolation = act12Rule(null, out string violation);

if (hasViolation)
{
    Debug.LogError($"Mental model violation: {violation}");
}
```

**Requirements**:
- You must have a `storytest-mental-model.json` file in your project root
- See `storytest-mental-model.json` for the schema

---

## Act 13: Narrative Coherence

**Purpose**: Ensure your project structure and organization align with your stated architecture and narrative.

**What it checks**:
- Architecture rules are followed (separation of concerns, layer boundaries)
- Documentation completeness (required docs exist and match implementation)
- Multi-platform support claims are accurate
- Quality gates are met

**How to enable**:
```csharp
using TinyWalnutGames.StoryTest.Acts;

// Get the Act 13 rule
var act13Rule = ActRegistry.GetActRule(13);

// Validate (pass null for assembly-level validation)
bool hasViolation = act13Rule(null, out string violation);

if (hasViolation)
{
    Debug.LogError($"Narrative coherence violation: {violation}");
}
```

**Requirements**:
- You must have a properly configured `storytest-mental-model.json`
- Required documentation: `docs/README.md`, `docs/acts.md`, `docs/getting-started.md`
- Proper folder structure: `Runtime/` and `Editor/` layers separated

---

## Using the ActRegistry

The `ActRegistry` provides a centralized way to access all validation acts (1-13):

```csharp
using TinyWalnutGames.StoryTest.Acts;

// Get a specific act rule
var rule = ActRegistry.GetActRule(actNumber); // 1-13

// Get all core acts (1-11)
int[] coreActNumbers = ActRegistry.GetCoreActNumbers();

// Get extended acts (12-13)
int[] extendedActNumbers = ActRegistry.GetExtendedActNumbers();

// Get the human-readable name
string actName = ActRegistry.GetActName(12);  // "Act12MentalModelClaims"

// Validate directly
ActRegistry.ValidateMember(1, myMethod, out string violation);
```

---

## When to Use Acts 12-13

### ✅ Use Acts 12-13 if:
- You want to validate that your project follows its own architecture rules
- You have a documented mental model or architecture diagram
- You want to enforce consistency between docs and code

### ❌ Don't use Acts 12-13 if:
- You're just prototyping (skip the extra validation overhead)
- You don't have a formal architecture documented
- Your project doesn't use the mental model config

---

## Game Jam Recommendation

For game jam submissions, focus on **Acts 1-11** (the core validation suite). Acts 12-13 are useful for mature projects but add complexity.

**Fast path for jam**:
```
✅ Use: Acts 1-11 (default Story Test validation)
✅ Use: Python CLI for quick validation
❌ Skip: Acts 12-13 unless you have formal architecture docs
```

---

## Troubleshooting Acts 12-13

### "Mental model configuration file not found"
- Create `storytest-mental-model.json` in your project root
- See the template in the repository

### "Documentation directory missing"
- Create `docs/` folder with required markdown files:
  - `docs/README.md` - Project overview
  - `docs/acts.md` - Story Test acts explanation
  - `docs/getting-started.md` - Quick start guide

### "Quality gates failed"
- Check that you have all 11+ Acts implemented
- Verify documentation is complete
- Ensure multi-platform support claims match reality

---

## Next Steps

1. If you need Acts 12-13, configure `storytest-mental-model.json`
2. Document your architecture in `docs/`
3. Enable optional validation in your validation pipeline
4. For game jam: stick with Acts 1-11 unless architecture validation is critical