# Acts Guide

Every Story Test violation ties back to one of nine narrative "Acts." Use this guide to understand what triggered the report, why it matters, and how to fix the script.

| Act | Detects | Why it matters | How to fix |
| --- | --- | --- | --- |
| **Act 1 – Todo Comments** | `NotImplementedException`, default-only returns, 🏳TODO markers | Placeholders imply the story is unfinished | Ship a real implementation or justify the gap with `[StoryIgnore]` and a precise reason |
| **Act 2 – Placeholder Implementations** | Methods whose IL is ≤10 bytes (usually `return default`) | Stubs hide risk and mislead reviewers | Flesh out the code or, if the symbol is intentionally abstract, refactor into an interface/abstract base |
| **Act 3 – Incomplete Classes** | Non-abstract classes that leave abstract members unimplemented | Consumers expect concrete classes to deliver on the contract | Implement every abstract member or mark the class `abstract` |
| **Act 4 – Unsealed Abstract Members** | Abstract methods in non-abstract classes | Breaks the principle that subclasses either override or the parent seals the deal | Remove the `abstract`, seal the method, or mark the entire class `abstract` |
| **Act 5 – Debug Only Implementations** | Methods that look like debug hooks without `[Obsolete]` | Debug affordances must be clearly temporary | Add `[Obsolete("Debug visualization only")]` (or similar) and revisit before release |
| **Act 6 – Phantom Props** | Auto-properties never read meaningfully | Phantom state complicates reasoning & indicates dead narrative threads | Remove the property, wire it into the story, or annotate why it exists |
| **Act 7 – Cold Methods** | Methods whose IL is just `ret` | Empty methods confuse callers and suggest half-finished arcs | Delete the method or add purposeful logic |
| **Act 8 – Hollow Enums** | Enums with ≤1 member or placeholder names | Signals that the design space is undefined | Rename the members, add meaningful values, or replace with constants |
| **Act 9 – Premature Celebrations** | Symbols marked complete but still throwing `NotImplementedException` | Declaring victory while throwing exceptions erodes trust | Remove the throw, or downgrade the status until the story is truly finished |

## How the detector thinks

- **Bytecode first.** The validator operates on IL, not just C#. If your code emits the same IL as a placeholder, it will be treated as such.
- **Context aware.** Acts skip members decorated with `[StoryIgnore]`, but only when the justification string is non-empty and specific.
- **Narrative cadence.** We lean on heuristics tuned for production assemblies, so legitimate short methods (e.g., `=> _value`) should still pass if they read concrete state.

## Best practices

- Break big fixes into commits that map to individual Acts so reviewers can keep pace with the narrative.
- When suppressing a violation, document why it is narratively safe and add a follow-up ticket if it is truly temporary.
- Run the standalone validator (`story_test.py`) locally before pushing to keep the Linux canonical job green.

For deeper troubleshooting of bytecode patterns, jump to [CI & Automation](CI.md) or explore runtime checks in [Dynamic Validation](DynamicValidation.md).
