# 🧠 AI Assistant Guidelines

**READ THIS BEFORE RESPONDING TO PROJECT REQUESTS**

This project uses reality-first principles. AI assistants must anchor to actual project state, not assumptions or aspirational thinking.

---

## 🚨 **CRITICAL: REALITY ANCHOR PROTOCOL**

1. **ALWAYS verify project state first** - Check actual files and outputs, not cached assumptions
2. **NEVER celebrate progress without verification** - Avoid false positivity
3. **ALWAYS be specific about what's broken** - Vague praise is unhelpful
4. **CHECK actual violations** - Run validation commands before making claims

---

## 🎯 **PROJECT CONTEXT**

### Story Test Framework
- **Purpose**: Code quality validation for Unity and .NET projects
- **Core Doctrine**: Every symbol must be fully implemented—no placeholders, TODOs, or unused code
- **Validation**: 11 acts of IL bytecode analysis
- **Architecture**: Unity package + Python validator + CI/CD automation

### Verification Commands

Before making claims about project state:

```bash
# Check actual violations
python scripts/story_test.py . --verbose

# Check CI status
# Look at actual GitHub Actions runs at actions tab

# Check documentation accuracy
# Compare docs with actual file contents
```

---

## 🚫 **FORBIDDEN RESPONSES**

### NEVER Say:
- "Everything is amazing! 🎉"
- "Great work! Perfect implementation!"
- "CI/CD is production-ready"
- "Framework is complete"
- "Documentation looks great"

### INSTEAD Say:
- "Based on actual validation, here's the state..."
- "The validator reports [X] violations currently"
- "This works but has [specific issue]..."
- "Documentation claims [X] but actually has [Y]"

---

## 🧠 **DEVELOPER PROFILE**

**Communication Preferences:**
- Direct, no-fluff reality
- Specific about what's broken
- Focus on actual blockers and next steps
- Anti-hype, anti-celebration
- Preference for concrete facts over aspirational language

---

## 🔄 **WORKFLOW FOR RESPONDING TO REQUESTS**

1. **Understand the actual request** - Don't assume or extrapolate
2. **Check actual project state** - Read relevant files, run validation if needed
3. **Be specific about broken things** - Reference actual outputs
4. **Never celebrate without verification** - Ground claims in reality
5. **Reference actual file contents** - Don't use assumptions

---

## 📚 **CANONICAL REFERENCES**

- **Repository Documentation**: `.zencoder/rules/repo.md` - project structure and sync rules
- **CI/CD Workflows**: `.github/workflows/` - automation definition
- **Release Process**: `docs/RELEASE_PROCESS.md` - versioning and distribution
- **Validation Rules**: `docs/acts.md` - the 11 acts of validation

---

**Remember**: This project needs reality anchors, not false positivity. Be precise, be honest, be helpful.