# Mental Model Validation Integration

## Summary

The Story Test Framework now includes **Acts 12 & 13** for mental model validation, ensuring projects don't just have complete code (Acts 1-11) but also maintain narrative coherence between what they claim and what they deliver.

---

## What Was Added

### 1. New Validation Acts (C#)

#### **Act 12: Mental Model Claims Validation**
- **File**: `Packages/com.tinywalnutgames.storytest/Runtime/Acts/Act12MentalModelClaims.cs`
- **Purpose**: Validates that claimed capabilities have concrete evidence
- **Checks**:
  - Required artifacts exist
  - Claimed features are implemented
  - Platform count matches claims
  - Integration claims have support

#### **Act 13: Narrative Coherence** 
- **File**: `Packages/com.tinywalnutgames.storytest/Runtime/Acts/Act13NarrativeCoherence.cs`
- **Purpose**: Ensures architecture aligns with stated narrative
- **Checks**:
  - Quality gates are met (min Acts, docs, tests, platforms)
  - Architectural rules are respected (separation of concerns, symmetry)
  - Documentation is complete and aligned
  - No contradictions in the narrative

### 2. Mental Model Configuration Schema

- **File**: `storytest-mental-model.json`
- **Location**: Project root
- **Purpose**: Define what the project claims to do
- **Sections**:
  - `project` — Basic metadata
  - `claimed_capabilities` — Features, platforms, integrations
  - `required_artifacts` — Files/directories that must exist
  - `architectural_rules` — Design principles
  - `quality_gates` — Minimum standards

### 3. Mental Model Reporting (Python)

- **File**: `storytest/mental_model_reporter.py`
- **Functions**:
  - Loads and validates mental model config
  - Verifies artifacts exist
  - Checks claimed capabilities have evidence
  - Validates quality gates
  - Generates JSON and HTML reports
- **Output**:
  - JSON: Structured report for CI/CD
  - HTML: Visual dashboard

### 4. Documentation

#### **New Documentation Files**
- `docs/mental-model-validation.md` — Complete guide to Acts 12 & 13
- Updated `docs/acts.md` — Added Acts 12 & 13 with examples
- Updated `docs/configuration.md` — Mental model configuration guide

#### **Test Files**
- Enhanced `tests/mental-model.spec.ts` — Improved Playwright tests
- Updated `tests/validate-mental-model.js` — Dynamic validator

---

## How It Works

### The Two-Level Story Test

```
┌─────────────────────────────────────────────────────┐
│  Story Test Framework: The Complete Narrative      │
├─────────────────────────────────────────────────────┤
│                                                     │
│  Level 1: Code Quality (Acts 1-11)                │
│  ├─ IL bytecode analysis                          │
│  ├─ Individual symbols validated                  │
│  └─ Example: No TODOs, empty methods, etc.        │
│                                                     │
│  Level 2: Narrative Coherence (Acts 12-13)       │
│  ├─ Mental model validation                       │
│  ├─ Project-level structure validated             │
│  └─ Example: Claims match implementation          │
│                                                     │
└─────────────────────────────────────────────────────┘
```

### Validation Flow

```
1. Load storytest-mental-model.json
   ↓
2. Extract claimed capabilities
   ↓
3. Scan for evidence (artifacts, code, docs)
   ↓
4. Act 12: Match claims ↔ Evidence
   ├─ ✓ Evidence found → PASS
   └─ ✗ No evidence → FAIL (plot hole)
   ↓
5. Act 13: Validate architecture
   ├─ ✓ Quality gates met → PASS
   ├─ ✓ Rules respected → PASS
   └─ ✗ Gaps or violations → FAIL
   ↓
6. Generate reports (JSON + HTML)
```

---

## Usage Examples

### Example 1: Basic Mental Model

Define your project's narrative:

```json
{
  "project": {
    "name": "My Library",
    "mission": "Provide IL validation for .NET assemblies",
    "platforms": ["Unity", ".NET"]
  },
  "claimed_capabilities": {
    "core": ["IL analysis", "Symbol validation"],
    "platforms": ["Unity", ".NET"]
  },
  "quality_gates": [
    {"gate": "all_acts_implemented", "minimum_acts": 11},
    {"gate": "documentation_complete", "minimum_docs_pages": 3}
  ]
}
```

Act 12 validates:
- ✓ Both `Packages/` (Unity) and `storytest/` (Python) exist
- ✓ IL analysis is implemented in Acts
- ✓ Everything claimed is present

### Example 2: Multi-Platform Project

```json
{
  "claimed_capabilities": {
    "platforms": ["Unity", ".NET", "Python"]
  },
  "quality_gates": [
    {"gate": "multi_platform", "required_platforms": 3}
  ]
}
```

Act 13 checks:
- ✓ C# package exists
- ✓ Python module exists
- ✓ CLI scripts exist
- All 3 platforms implemented ✓

### Example 3: Architectural Rules

```json
{
  "architectural_rules": [
    {
      "rule": "separation_of_concerns",
      "verify": [
        "Runtime/ independent",
        "Editor/ separate",
        "Tests/ isolated"
      ]
    }
  ]
}
```

Act 13 validates:
- ✓ Each layer exists
- ✓ Boundaries are respected
- ✓ No inappropriate dependencies

---

## Running Validation

### Command Line

```bash
# Full validation (Acts 1-13)
python scripts/story_test.py . --output report.json

# Mental model only
python -m storytest.mental_model_reporter

# With HTML report
python -m storytest.mental_model_reporter
open mental-model-report.html
```

### In CI/CD

```yaml
- name: Validate Code Quality (Acts 1-11)
  run: python scripts/story_test.py . --output report.json

- name: Validate Mental Model (Acts 12-13)
  run: python -m storytest.mental_model_reporter

- name: Check Status
  run: |
    if ! python -m storytest.mental_model_reporter | grep -q '"status":"COMPLETE"'; then
      exit 1
    fi
```

### In Playwright Tests

```typescript
npx playwright test tests/mental-model.spec.ts
```

Expected output:
```
✓ should detect all required components and features
✓ should verify features are implemented
```

---

## Report Examples

### JSON Report

```json
{
  "timestamp": "2025-10-15T18:27:56.449Z",
  "project": "The Story Test Framework",
  "status": "COMPLETE",
  "metrics": {
    "completeness": "100%",
    "claims": "16/16",
    "artifacts": "10/10",
    "quality_gates": "4/4"
  },
  "gaps": [],
  "violations": []
}
```

### HTML Report

Visual dashboard showing:
- Overall status (COMPLETE / INCOMPLETE / WARNING)
- Completeness percentage
- Claims verified vs. total
- Artifacts found
- Quality gates passed
- Detailed gaps and violations

---

## Configuration Reference

### Mental Model Config Structure

```json
{
  "$schema": "...",           // JSON Schema URI
  "version": "1.0",           // Config version
  "description": "...",       // Config description
  
  "project": {
    "name": "string",         // Project name
    "mission": "string",      // Project purpose
    "platforms": ["string"]   // Supported platforms
  },
  
  "claimed_capabilities": {
    "category": ["string"]    // Features claimed
  },
  
  "required_artifacts": [
    {
      "path": "string",       // File/directory path
      "type": "file|dir",     // Type
      "required": true|false  // Critical?
    }
  ],
  
  "architectural_rules": [
    {
      "rule": "string",       // Rule name
      "description": "string",// What it ensures
      "verify": ["string"]    // Checks
    }
  ],
  
  "quality_gates": [
    {
      "gate": "string",       // Gate name
      "minimum_acts": number, // Minimum Acts
      "minimum_docs_pages": number,
      "required_platforms": number
    }
  ]
}
```

---

## Integration Points

### With Story Test Framework

- Acts 12 & 13 run after Acts 1-11
- Violations reported in same format
- Configurable via `StoryTestSettings.json`
- Works with both C# and Python validators

### With CI/CD

- GitHub Actions workflows included
- JSON output for parsing
- HTML reports for review
- Exit codes for automation

### With Playwright Tests

- TypeScript test suite
- Comprehensive reporting
- Visual output formatting
- Integration with test runners

---

## Benefits

### For Development Teams

✓ **Catch Narrative Gaps** — Detect broken promises early
✓ **Architecture Alignment** — Ensure design matches reality
✓ **Quality Gates** — Enforce minimum standards
✓ **Documentation Sync** — Keep docs in sync with code

### For Projects

✓ **Narrative Integrity** — The story stays coherent
✓ **User Expectations** — Features match claims
✓ **Maintainability** — Clear, consistent architecture
✓ **Automation** — Validate in CI/CD

### For Users

✓ **Trust** — Project delivers on promises
✓ **Clarity** — Understanding what's supported
✓ **Reliability** — No surprise missing features
✓ **Quality** — Evidence-based claims

---

## Files Created/Modified

### New Files
- `Packages/com.tinywalnutgames.storytest/Runtime/Acts/Act12MentalModelClaims.cs`
- `Packages/com.tinywalnutgames.storytest/Runtime/Acts/Act13NarrativeCoherence.cs`
- `storytest/mental_model_reporter.py`
- `docs/mental-model-validation.md`
- `storytest-mental-model.json`

### Modified Files
- `docs/acts.md` — Added Acts 12 & 13 documentation
- `docs/configuration.md` — Added mental model configuration guide
- `tests/mental-model.spec.ts` — Enhanced with new report structure
- `tests/validate-mental-model.js` — Already dynamic validator

---

## Next Steps

1. **Review** the mental model config: `storytest-mental-model.json`
2. **Run** validation: `python scripts/story_test.py . --output report.json`
3. **Check** report: `story-test-report.json`
4. **View** HTML: `mental-model-report.html`
5. **Integrate** into CI/CD workflows

---

## Documentation

- [Mental Model Validation](docs/mental-model-validation.md) — Complete guide
- [All Acts Reference](docs/acts.md) — All 13 Acts explained
- [Configuration Guide](docs/configuration.md) — Settings and options

Remember: The Story Test validates not just that your code is complete, but that your narrative is coherent. Every claim should have evidence. Every architecture rule should be respected. Keep your story straight.