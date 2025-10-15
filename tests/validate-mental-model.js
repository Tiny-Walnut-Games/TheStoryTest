#!/usr/bin/env node
/**
 * Dynamic Mental Model Validator
 *
 * Validates the repository's mental model by:
 * 1. Extracting claims from README, package.json, pyproject.toml
 * 2. Mapping claims to concrete evidence in the codebase
 * 3. Detecting "plot holes" - gaps between claimed and actual capabilities
 *
 * The validator is fuzzy and adaptive, learning the project structure
 * rather than enforcing hardcoded assertions.
 *
 * Exit codes:
 *  0 = All checks passed (mental model complete)
 *  1 = One or more gaps found (plot holes detected)
 */

const fs = require('fs');
const path = require('path');

const root = path.resolve(__dirname, '..');

// ============================================================================
// Utility Functions
// ============================================================================

function read(file) {
  const p = path.join(root, file);
  if (!fs.existsSync(p)) return null;
  return fs.readFileSync(p, 'utf8');
}

function exists(file) {
  const p = path.join(root, file);
  return fs.existsSync(p);
}

function listDir(dir, maxDepth = 2, currentDepth = 0) {
  const p = path.join(root, dir);
  if (!fs.existsSync(p)) return [];
  if (currentDepth >= maxDepth) return [];
  
  const items = [];
  try {
    fs.readdirSync(p).forEach(file => {
      const fullPath = path.join(p, file);
      const relPath = path.join(dir, file);
      const stat = fs.statSync(fullPath);
      if (stat.isDirectory()) {
        items.push(...listDir(relPath, maxDepth, currentDepth + 1));
      } else {
        items.push(relPath);
      }
    });
  } catch (e) {
    // Ignore read errors
  }
  return items;
}

function sectionFromMarkdown(md, heading) {
  if (!md) return '';
  const lines = md.split(/\r?\n/);
  const startIdx = lines.findIndex(l => l.trim().toLowerCase().startsWith(heading.toLowerCase()));
  if (startIdx === -1) return '';
  const collected = [];
  for (let i = startIdx + 1; i < lines.length; i++) {
    const l = lines[i];
    if (/^##\s/.test(l) || /^---+$/.test(l.trim())) break;
    collected.push(l);
  }
  return collected.join('\n');
}

function extractBullets(text) {
  if (!text) return [];
  return text
    .split(/\r?\n/)
    .map(l => l.trim())
    .filter(l => /^[-•*]\s+/.test(l))
    .map(l => l.replace(/^[-•*]\s+/, '').trim())
    .filter(l => l.length > 0);
}

function fuzzyMatch(text, patterns) {
  if (typeof patterns === 'string') patterns = [patterns];
  return patterns.some(p => {
    const regex = new RegExp(p, 'i');
    return regex.test(text);
  });
}

function addError(arr, code, message, meta = {}) {
  arr.push({ code, message, ...meta });
}

function addWarning(arr, code, message, meta = {}) {
  arr.push({ code, message, ...meta });
}

// ============================================================================
// Mental Model Extraction Engine
// ============================================================================

function extractMentalModel() {
  const model = {
    name: null,
    version: null,
    description: null,
    purpose: null,
    features: [],
    components: [],
    dependencies: [],
    platforms: []
  };

  // Extract from package.json
  const pkgJson = read('package.json');
  if (pkgJson) {
    try {
      const pkg = JSON.parse(pkgJson);
      model.name = pkg.name;
      model.version = pkg.version;
      model.description = pkg.description;
    } catch (e) {
      // Ignore parse errors
    }
  }

  // Extract from pyproject.toml
  const pyproject = read('pyproject.toml');
  if (pyproject) {
    const nameMatch = pyproject.match(/^name\s*=\s*['"](.*?)['"]/m);
    const versionMatch = pyproject.match(/^version\s*=\s*['"](.*?)['"]/m);
    const descMatch = pyproject.match(/^description\s*=\s*['"](.*?)['"]/m);
    if (nameMatch) model.name = nameMatch[1];
    if (versionMatch) model.version = versionMatch[1];
    if (descMatch) model.description = descMatch[1];
  }

  // Extract from README
  const readme = read('README.md');
  if (readme) {
    // Extract purpose from opening sections
    const lines = readme.split(/\r?\n/).slice(0, 15);
    const purposeLines = lines.filter(l => !l.match(/^#+/) && l.trim().length > 0);
    if (purposeLines.length > 0) {
      model.purpose = purposeLines.join(' ').substring(0, 200);
    }

    // Extract features from Features section
    const featuresSection = sectionFromMarkdown(readme, '## ✨ Features');
    model.features = extractBullets(featuresSection);

    // Extract other sections that describe capabilities
    const coreSection = sectionFromMarkdown(readme, '## 🎯 Core Validation');
    const coreItems = extractBullets(coreSection);
    model.features.push(...coreItems);
  }

  return model;
}

// ============================================================================
// Evidence Collection Engine
// ============================================================================

function collectEvidence() {
  const evidence = {
    filesExist: [],
    filesMissing: [],
    artifacts: {
      unity: false,
      python: false,
      csharp: false,
      documentation: false,
      tests: false,
      ci: false
    },
    components: {
      unityPackage: false,
      pythonValidator: false,
      cliScripts: false,
      editorIntegration: false
    },
    codeQuality: {
      hasTests: false,
      hasDocumentation: false,
      hasCI: false,
      hasValidationActs: 0
    }
  };

  // Check for key artifacts
  const requiredArtifacts = [
    'Packages/com.tinywalnutgames.storytest/Runtime',
    'Packages/com.tinywalnutgames.storytest/Editor',
    'Packages/com.tinywalnutgames.storytest/Tests',
    'storytest/',
    'scripts/story_test.py',
    'scripts/story_test_unity_safe.py',
    'docs/',
    'README.md',
    'CHANGELOG.md',
    '.github/workflows'
  ];

  for (const artifact of requiredArtifacts) {
    if (exists(artifact)) {
      evidence.filesExist.push(artifact);
    } else {
      evidence.filesMissing.push(artifact);
    }
  }

  // Check for file types
  if (evidence.filesExist.some(f => f.includes('Packages'))) {
    evidence.artifacts.unity = true;
    evidence.components.unityPackage = true;
  }
  if (evidence.filesExist.some(f => f.includes('storytest'))) {
    evidence.artifacts.python = true;
    evidence.components.pythonValidator = true;
  }
  if (evidence.filesExist.some(f => f.includes('scripts'))) {
    evidence.components.cliScripts = true;
  }
  if (evidence.filesExist.some(f => f.includes('Editor'))) {
    evidence.components.editorIntegration = true;
  }

  // Check for tests
  evidence.codeQuality.hasTests = listDir('Packages/com.tinywalnutgames.storytest/Tests').length > 0 ||
                                  listDir('storytest').some(f => f.includes('.test.') || f.includes('_test.'));

  // Check for documentation
  evidence.codeQuality.hasDocumentation = exists('docs/') && 
                                         listDir('docs').length > 0;

  // Check for CI/CD
  evidence.codeQuality.hasCI = listDir('.github/workflows').length > 0;

  // Count validation acts (looking for Act1, Act2, ... in Runtime)
  const runtimeFiles = listDir('Packages/com.tinywalnutgames.storytest/Runtime');
  evidence.codeQuality.validationActs = runtimeFiles.filter(f => /Act\d+/.test(f)).length;

  return evidence;
}

// ============================================================================
// Gap Detection Engine
// ============================================================================

function detectGaps(model, evidence) {
  const gaps = [];
  const validations = [];

  // Gap 1: Missing core components
  if (!evidence.components.unityPackage && model.features.some(f => fuzzyMatch(f, 'Unity|Editor'))) {
    addError(gaps, 'COMPONENT_MISSING', 'Claimed Unity integration but no Unity package found');
  }

  if (!evidence.components.pythonValidator && model.features.some(f => fuzzyMatch(f, 'Python|standalone'))) {
    addError(gaps, 'COMPONENT_MISSING', 'Claimed Python validator but storytest/ module incomplete');
  }

  if (!evidence.codeQuality.hasDocumentation && model.description && model.description.length > 50) {
    addWarning(gaps, 'DOCUMENTATION_SPARSE', 'Limited documentation for claimed functionality');
  }

  if (!evidence.codeQuality.hasCI) {
    addWarning(gaps, 'CI_MISSING', 'No CI/CD workflows found');
  }

  // Gap 2: Feature claims without evidence
  const actPattern = /Act \d+|validation.*act|rule \d+/i;
  const claimedActs = model.features.filter(f => actPattern.test(f)).length;
  if (claimedActs > 0 && evidence.codeQuality.validationActs === 0) {
    addError(gaps, 'VALIDATION_ACTS_MISSING', `Claimed ${claimedActs} validation acts but found 0 implementations`);
  }

  // Gap 3: Testing gap
  if (model.features.some(f => fuzzyMatch(f, 'test|testing|validation'))) {
    if (!evidence.codeQuality.hasTests) {
      addWarning(gaps, 'TESTS_MISSING', 'Testing claimed but no test suite found');
    } else {
      validations.push({ check: 'test-suite-exists', ok: true, message: 'Test suite present' });
    }
  }

  // Gap 4: Multi-platform claim
  const multiPlatformClaimed = model.description && fuzzyMatch(model.description, 'cross-platform|unity.*dotnet|standalone');
  if (multiPlatformClaimed) {
    const platformCount = Object.values(evidence.components).filter(v => v === true).length;
    if (platformCount < 2) {
      addWarning(gaps, 'MULTI_PLATFORM_INCOMPLETE', `Claimed cross-platform but only ${platformCount} platforms implemented`);
    } else {
      validations.push({ check: 'multi-platform', ok: true, message: `${platformCount} platforms implemented` });
    }
  }

  // Gap 5: Artifact completeness
  if (evidence.filesMissing.length > 0) {
    const critical = evidence.filesMissing.filter(f => /Runtime|cli|tests/i.test(f));
    if (critical.length > 0) {
      addError(gaps, 'CRITICAL_ARTIFACTS_MISSING', `Missing critical artifacts: ${critical.join(', ')}`);
    } else {
      addWarning(gaps, 'ARTIFACTS_MISSING', `Missing non-critical artifacts: ${evidence.filesMissing.join(', ')}`);
    }
  }

  return { gaps, validations };
}

// ============================================================================
// Report Generation
// ============================================================================

function main() {
  // Phase 1: Extract mental model
  const model = extractMentalModel();
  
  // Phase 2: Collect evidence
  const evidence = collectEvidence();
  
  // Phase 3: Detect gaps
  const { gaps, validations } = detectGaps(model, evidence);

  // Phase 4: Generate report
  const report = {
    timestamp: new Date().toISOString(),
    projectModel: {
      name: model.name,
      version: model.version,
      description: model.description,
      featureCount: model.features.length,
      features: model.features
    },
    evidence: {
      artifactsFound: evidence.filesExist.length,
      artifactsMissing: evidence.filesMissing.length,
      components: evidence.components,
      codeQuality: evidence.codeQuality
    },
    validation: {
      passes: validations,
      gaps: gaps.filter(g => g.code && g.code.startsWith('ERROR') === false),
      errors: gaps.filter(g => g.code && (g.code.includes('MISSING') || g.code.includes('NOT_FOUND')))
    },
    summary: {
      completeness: `${Math.round((evidence.filesExist.length / (evidence.filesExist.length + evidence.filesMissing.length)) * 100)}%`,
      status: gaps.filter(g => g.code && g.code.includes('ERROR')).length === 0 ? 'COMPLETE' : 'INCOMPLETE',
      plotHolesDetected: gaps.length > 0
    }
  };

  console.log(JSON.stringify(report, null, 2));

  // Exit with error if critical gaps found
  const criticalGaps = gaps.filter(g => g.code && (g.code.includes('MISSING') || g.code.includes('NOT_FOUND')));
  process.exit(criticalGaps.length > 0 ? 1 : 0);
}

main();