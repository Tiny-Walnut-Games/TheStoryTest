# Phase 4: CI/CD Reinforcement — Execution Plan

**Issue:** #4  
**Branch:** `jmeyer1980/phase4-ci` *(proposed)*  
**Date:** October 3, 2025  
**Status:** 📋 Planning

## Overview

Phase 4 focuses on validating the Story Test package in production-like environments and hardening automation. Once Phase 3 lands on `main`, we will:

1. Exercise the package exactly as consumers do (UPM + git URL) in a clean Unity project.
2. Rehearse the GitHub Actions workflows from an external repository to confirm onboarding steps are smooth.
3. Expand CI coverage to include package-install validation and Unity version matrices.

## Prerequisites

- ✅ Phase 3 documentation updates merged into `main` (`jmeyer1980/issue2` closed or synced).
- ✅ `main` tagged or checkpointed so Stage 4 work starts from a clean base.
- ✅ Sample project assets stabilized (no relocations during CI iteration).
- ✅ Secrets available for CI rehearsal: `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`, and any report upload tokens.

## Objectives

1. **Validate Package Consumption** — Prove the published UPM package works end-to-end in an isolated Unity project on Windows, macOS, and Linux.
2. **External Workflow Dry Run** — Fork/clone workflows into a sandbox repository and ensure contributors can bootstrap CI with minimal steps.
3. **CI Coverage Expansion** — Add automated package-install tests and broaden Unity/Mono runtimes in the matrix.
4. **Observability & Reporting** — Capture Story Test reports as artifacts, summarize results with file/line data, and document remediation flow.

## Task Breakdown

### Task 1: Branch Hygiene & Merge Backlog

- Merge `jmeyer1980/issue2` (Phase 3 branch) into `main`.
- Create `jmeyer1980/phase4-ci` branch from updated `main`.
- Document merge steps in CHANGELOG ("Phase 3 docs integrated prior to CI upgrades").

### Task 2: Package Validation in a Clean Unity Project

- Spin up a new Unity LTS project (Unity Hub or CLI) outside this repository.
- Add the package via git URL in `Packages/manifest.json`:

  ```json
  "com.tinywalnutgames.storytest": "https://github.com/jmeyer1980/TheStoryTest.git?path=Packages/com.tinywalnutgames.storytest"
  ```

- Open project, confirm compilation, run `Tiny Walnut Games/The Story Test/Run Story Test and Export Report`.
- Capture screenshots/logs of a clean run.
- Invoke the standalone validator against the new project's `Library/ScriptAssemblies` to verify CLI parity.
- Record any onboarding friction in a troubleshooting doc.

### Task 3: Workflow Rehearsal from External Repository

- Create sandbox repo (e.g., `TheStoryTest-e2e-demo`).
- Copy `.github/workflows/story-test.yml` and minimal README instructions.
- Configure required secrets in the sandbox repo.
- Trigger workflow (push or manual dispatch) and ensure it completes across all OS targets.
- Confirm reports attach as artifacts and PR annotations appear if violations exist.
- File issues/PRs back in primary repo for any blockers discovered.

### Task 4: CI Enhancements in Primary Repo

- Add package-install validation job that runs `story_test.py` against a freshly generated Unity sample using the git dependency.
- Expand Unity version matrix (e.g., current LTS + next tech stream) if builder runtime supports it.
- Cache python wheels / Unity build artifacts to reduce run time.
- Ensure workflow surfaces per-violation metadata in summary (already achieved in run #37) and preserve in new jobs.

### Task 5: Documentation & Knowledge Transfer

- Update `Documentation~/CI.md` with step-by-step guides for both internal and external workflow usage.
- Add "Testing the Package as a Consumer" tutorial to `Documentation~/QuickStart.md` or a dedicated guide.
- Extend README "GitHub Actions CI/CD" section with link to sandbox rehearsal notes.
- Update `CHANGELOG.md` with Phase 4 accomplishments upon completion.

## Milestones

1. **Milestone A:** New Unity project validates package via UPM + CLI (Windows proof).  
2. **Milestone B:** Sandbox repository workflow succeeds end-to-end across OS matrix.  
3. **Milestone C:** Primary workflow updated with package-install job and any new matrix entries.  
4. **Milestone D:** Documentation refreshed and merged to `main`; tag release candidate.

## Risks & Mitigations

- **Unity license availability** — Ensure sandbox repo has access to required secrets; rotate tokens if needed.
- **Workflow runtime limits** — Matrix expansion may exceed time/credit budgets; start with nightly or optional jobs.
- **Package import failures** — Capture editor logs; use `--force-free` builder options if license contention occurs.

## Validation Checklist

- [ ] Clean Unity project imports and runs Story Test without manual tweaks.
- [ ] Standalone validator yields identical results to in-editor run in the clean project.
- [ ] External sandbox workflow run completes successfully and artifacts include Story Test report.
- [ ] Primary repo workflow gains package-install job and passes across matrix.
- [ ] Documentation (README + Documentation~/CI.md) describes replication steps.
- [ ] `main` updated with Phase 4 deliverables and tagged for next release.

---

*Phase 4 ensures our automated story stays truthful—every consumer path we recommend is one we have rehearsed ourselves.*
