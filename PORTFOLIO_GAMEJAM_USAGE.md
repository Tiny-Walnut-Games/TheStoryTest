# Story Test v1.3.1 - Game Jam Portfolio Usage

## 🎯 Your Game Jam Advantage

You now have **The Story Test Framework v1.3.1** ready to validate your game before submission.

**Portfolio Impact**: Using a professional code validation framework on your submission demonstrates:
- ✅ Code quality discipline
- ✅ Proactive quality assurance
- ✅ Use of custom tooling for validation
- ✅ Professional development practices

---

## 🚀 Quick Start: Validate Your Game

### 1. **Run Acts 1-11 Validation** (Core Quality Checks)

```bash
# Quick validation (default)
python scripts/story_test_unity_safe.py /path/to/your/game --verbose

# Generate JSON report
python scripts/story_test_unity_safe.py /path/to/your/game --output game-validation.json

# CI mode (fail if violations found)
python scripts/story_test_unity_safe.py /path/to/your/game --fail-on-violations
```

**What it checks**:
- ❌ No TODO/FIXME comments in production code
- ❌ No placeholder implementations ("dummy", "test", "temp")
- ❌ All classes are fully implemented (no abstract stubs)
- ❌ No unsealed abstract members
- ❌ No debug-only code (Debug.Log left in)
- ❌ No phantom properties (unused decorations)
- ❌ No "cold" methods (never called)
- ❌ No hollow enums (incomplete state machines)
- ❌ No premature celebrations (overconfident code)
- ❌ No suspiciously simple code (oversimplified logic)
- ❌ No dead code (unreachable, unused variables)

---

## 📊 Portfolio Artifact: Validation Report

After running validation, you'll have:

1. **Console Output** - Direct validation feedback
2. **JSON Report** - Machine-readable violations list (if `--output` specified)
3. **Clean Exit Code** - Zero violations = exit code 0

### Example Portfolio Statement

```
I validated my game submission using The Story Test Framework (v1.3.1):

$ python scripts/story_test_unity_safe.py ./MyGame --fail-on-violations
✅ No Story Test violations found in non-Unity assemblies!

This tool validates code quality across 11 dimensions:
- No incomplete implementations or TODOs
- No debug-only code left in production
- No unreachable dead code
- All classes fully implemented

Result: PASSED - Code quality verified before submission
```

---

## 🎨 Optional: Use Acts 12-13 (Mental Model Validation)

If you want to go deeper and **document your game's architecture**:

### 1. Create `storytest-mental-model.json`

```json
{
  "project": {
    "name": "My Game",
    "mission": "A [genre] game where [core mechanic]"
  },
  "claimed_capabilities": {
    "core_systems": [
      "Player movement and input handling",
      "Enemy AI pathfinding",
      "Inventory management"
    ]
  },
  "quality_gates": [
    {
      "gate": "all_acts_implemented",
      "minimum_acts": 11
    }
  ]
}
```

### 2. Use the Mental Model Reporter

```bash
# Generate HTML mental model report
python -m storytest.mental_model_reporter
```

This creates `mental-model-report.html` showing:
- ✅ Architecture alignment
- ✅ Claimed capabilities verified
- ✅ Quality gates passed
- ✅ Gap analysis

---

## 📦 Release Information

**Framework**: The Story Test Framework v1.3.1  
**Package**: storytest (PyPI)  
**Installation**: `pip install storytest`  
**License**: MIT  
**Status**: Production Ready (Acts 1-11) + Optional Extended Validation (Acts 12-13)

---

## 💡 Tips for Best Portfolio Use

1. **Run Early & Often**
   - Validate weekly during development
   - Fix violations before final submission
   - Build a history of clean passes

2. **Document Your Score**
   - Take a screenshot of the clean validation report
   - Include in your portfolio README
   - Link to the framework: [The Story Test on GitHub](https://github.com/jmeyer1980/TheStoryTest)

3. **Combine with Other Tools**
   - Use alongside standard Unity tests
   - Integrate into your CI/CD pipeline
   - Show multiple validation layers

4. **Submit with Proof**
   - Include validation report in submission package
   - Add README noting "Validated with Story Test v1.3.1"
   - Show you care about code quality

---

## 🔗 Useful Commands

```bash
# Install the framework locally
pip install -e .

# Run on multiple game assemblies
python scripts/story_test_unity_safe.py . --verbose --output full-report.json

# Check specific violation types
python scripts/story_test.py . --verbose 2>&1 | grep -i "todo\|debug\|placeholder"

# Get the version
storytest --version
```

---

## 🎮 Final Checklist Before Submission

- [ ] Ran `story_test_unity_safe.py` on your game code
- [ ] Got a PASSED result (0 violations or acceptable list)
- [ ] Saved the validation report (JSON or screenshot)
- [ ] Documented Acts 1-11 validation in your submission notes
- [ ] (Optional) Used Acts 12-13 for deeper architecture validation
- [ ] Included portfolio note: "Validated with Story Test Framework v1.3.1"

---

**Questions?** See the full documentation in `docs/` or visit [The Story Test Repository](https://github.com/jmeyer1980/TheStoryTest)

Good luck with your game jam! 🚀
