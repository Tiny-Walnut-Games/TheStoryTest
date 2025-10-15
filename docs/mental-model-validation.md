# Mental Model Validation

## Overview

The Story Test Framework validates code completeness through 11 Acts of IL analysis. **Acts 12 & 13** extend this to validate **mental model adherence** — ensuring the project's stated narrative (what it claims to do) aligns with its actual implementation.

### The Problem

A project can pass all 11 code quality Acts but still have "plot holes":
- Claims multi-platform support but only implements one
- Documents features that don't exist
- Violates stated architectural rules
- Missing required components despite claims

### The Solution

**Acts 12 & 13** detect these narrative gaps:

| Act | Name | Purpose |
|-----|------|---------|
| 12  | **Mental Model Claims** | Validates claimed capabilities have concrete evidence |
| 13  | **Narrative Coherence** | Ensures architecture matches stated design |

---

## Act 12: Mental Model Claims Validation

### What It Checks

✓ **Required Artifacts Exist**
- All `required: true` artifacts in mental model config are present
- Fails if critical components are missing

✓ **Claimed Capabilities Have Evidence**
- Features claimed in README have implementation
- Platforms claimed are implemented

✓ **Platform Symmetry**
- Claims match implementation count
- No over-claiming features

### Configuration

Projects define claims via `storytest-mental-model.json`:

```json
{
  "claimed_capabilities": {
    "core_validation": [
      "IL bytecode analysis",
      "11 Validation Acts"
    ],
    "platforms": ["Unity", ".NET", "Python"],
    "integration": ["GitHub Actions", "Zero Dependencies"]
  },
  "required_artifacts": [
    {
      "path": "Packages/core",
      "type": "directory",
      "required": true
    }
  ]
}
```

### Violations

Act 12 reports:
- `MISSING_ARTIFACT` — Required file/directory not found
- `UNVERIFIED_CLAIM` — Claimed feature has no evidence
- `PLATFORM_MISMATCH` — Platform count doesn't match claims

---

## Act 13: Narrative Coherence

### What It Checks

✓ **Quality Gates Met**
- Minimum Acts implemented (11)
- Documentation pages (5+)
- Test suite exists
- Multi-platform support (2+)

✓ **Architectural Rules Respected**
- Separation of concerns (Runtime / Editor / Tests layers)
- Validation symmetry (C# and Python equivalent)
- Zero dependencies principle (Python validator)

✓ **Documentation Alignment**
- docs/ directory exists
- Acts are documented in docs/acts.md
- Key documentation files present

### Configuration Rules

```json
{
  "architectural_rules": [
    {
      "rule": "separation_of_concerns",
      "verify": [
        "Runtime/ exists independently",
        "Editor/ has no core dependencies"
      ]
    }
  ],
  "quality_gates": [
    {
      "gate": "all_acts_implemented",
      "minimum_acts": 11
    }
  ]
}
```

### Violations

Act 13 reports:
- `QUALITY_GATE_FAILED` — Minimum standard not met
- `ARCHITECTURE_VIOLATION` — Layer boundaries crossed
- `DOCUMENTATION_GAP` — Missing or incomplete docs
- `RULE_VIOLATION` — Architectural rule not followed

---

## How to Use

### 1. Define Your Mental Model

Create `storytest-mental-model.json` in project root:

```json
{
  "version": "1.0",
  "project": {
    "name": "My Project",
    "mission": "What the project does",
    "platforms": ["Platform1", "Platform2"]
  },
  "claimed_capabilities": {
    "core": ["Feature 1", "Feature 2"],
    "platforms": ["Platform 1", "Platform 2"]
  },
  "required_artifacts": [
    {"path": "src/", "type": "directory", "required": true}
  ],
  "quality_gates": [
    {"gate": "documentation_complete", "minimum_docs_pages": 5}
  ]
}
```

### 2. Validate During Development

```bash
# Validate with Story Test
python scripts/story_test.py . --output report.json

# Generate mental model report
python -m storytest.mental_model_reporter
```

### 3. Review the Reports

**JSON Report** (`story-test-report.json`):
```json
{
  "violations": [
    {
      "Type": "Act12MentalModelClaims",
      "Member": "ProjectModel",
      "Violation": "Mental model claims not fully supported: Missing required artifact: docs/API.md"
    }
  ]
}
```

**HTML Report** (`mental-model-report.html`):
- Visual dashboard of mental model adherence
- Completeness percentage
- Gaps and violations
- Quality gate status

---

## Integration with CI/CD

Add to GitHub Actions workflow:

```yaml
- name: Validate Mental Model
  run: python scripts/story_test.py . --output report.json --fail-on-violations
  
- name: Generate Mental Model Report
  if: always()
  run: python -m storytest.mental_model_reporter > model-report.json

- name: Upload Reports
  uses: actions/upload-artifact@v3
  with:
    name: story-test-reports
    path: |
      report.json
      model-report.json
      mental-model-report.html
```

---

## Best Practices

### ✓ Do

- Update `storytest-mental-model.json` when adding features
- Run mental model validation in CI
- Keep quality gates realistic but challenging
- Document the "why" behind architectural rules

### ✗ Don't

- Over-claim features in mental model
- Set quality gates too high initially
- Ignore coherence violations in favor of code quality
- Leave mental model config outdated

---

## Examples

### Example 1: Multi-Platform Library

```json
{
  "project": {
    "name": "MyLib",
    "platforms": ["Unity", ".NET", "Python"]
  },
  "quality_gates": [
    {
      "gate": "multi_platform",
      "required_platforms": 3
    }
  ]
}
```

Act 13 ensures:
- C# implementation exists
- Python bindings exist
- Unity package exists

### Example 2: Enterprise API

```json
{
  "claimed_capabilities": {
    "api": ["REST endpoints", "GraphQL support"],
    "security": ["JWT auth", "Rate limiting"]
  },
  "architectural_rules": [
    {
      "rule": "separation_of_concerns",
      "verify": ["API layer separate", "Business logic independent"]
    }
  ]
}
```

Act 12 ensures features are implemented.
Act 13 ensures proper layering.

---

## Troubleshooting

### "Mental model configuration not found"

**Solution**: Create `storytest-mental-model.json` in project root.

### "Quality gate failed: only 3 Acts found (minimum 11)"

**Solution**: Implement missing validation acts or adjust `minimum_acts` in quality gates.

### "Documentation gap: Act7ColdMethods not documented"

**Solution**: Add Act7 documentation to `docs/acts.md`.

### "Narrative coherence: only 1 platform implemented (minimum 2)"

**Solution**: Add support for second platform or adjust `required_platforms` in mental model.

---

## See Also

- [Acts Reference](acts.md) — All 13 validation acts
- [Configuration](configuration.md) — Story Test settings
- [CI/CD Integration](ci-cd.md) — Automation setup