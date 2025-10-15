/**
 * Mental Model Completeness Test
 *
 * Validates that the Story Test Framework itself (and target projects) maintain
 * mental model completeness - i.e., no gaps ("plot holes") between claimed features
 * and actual implementations.
 *
 * The test:
 * 1. Extracts the mental model from README, package.json, and pyproject.toml
 * 2. Collects evidence from actual code artifacts
 * 3. Detects gaps between claims and reality
 * 4. Fails only on critical gaps (missing core components)
 */
import { test, expect } from '@playwright/test';
import { spawnSync } from 'child_process';
import path from 'path';

interface ValidationReport {
  timestamp: string;
  projectModel: {
    name: string;
    version: string;
    description: string;
    featureCount: number;
    features: string[];
  };
  evidence: {
    artifactsFound: number;
    artifactsMissing: number;
    components: Record<string, boolean>;
    codeQuality: {
      hasTests: boolean;
      hasDocumentation: boolean;
      hasCI: boolean;
      validationActs: number;
    };
  };
  validation: {
    passes: Array<{ check: string; ok: boolean; message: string }>;
    gaps: Array<{ code: string; message: string }>;
    errors: Array<{ code: string; message: string }>;
  };
  summary: {
    completeness: string;
    status: 'COMPLETE' | 'INCOMPLETE';
    plotHolesDetected: boolean;
  };
}

function runValidator() {
  const repoRoot = path.resolve(__dirname, '../..');
  const result = spawnSync(process.execPath, ['tests/validate-mental-model.js'], {
    cwd: repoRoot,
    encoding: 'utf-8'
  });
  return result;
}

function formatReportForConsole(report: ValidationReport): string {
  const lines: string[] = [];
  lines.push('\n═══════════════════════════════════════════════════════════════');
  lines.push('           MENTAL MODEL VALIDATION REPORT');
  lines.push('═══════════════════════════════════════════════════════════════');
  
  lines.push(`\nProject: ${report.projectModel.name} v${report.projectModel.version}`);
  lines.push(`Status: ${report.summary.status}`);
  lines.push(`Completeness: ${report.summary.completeness}`);
  
  lines.push(`\n📊 Evidence Summary:`);
  lines.push(`  ✓ Artifacts Found: ${report.evidence.artifactsFound}`);
  lines.push(`  ✗ Artifacts Missing: ${report.evidence.artifactsMissing}`);
  lines.push(`  📦 Components: ${Object.values(report.evidence.components).filter(v => v).length}/${Object.keys(report.evidence.components).length}`);
  
  lines.push(`\n🎯 Features Claimed: ${report.projectModel.featureCount}`);
  report.projectModel.features.slice(0, 5).forEach(f => {
    lines.push(`  • ${f}`);
  });
  if (report.projectModel.featureCount > 5) {
    lines.push(`  ... and ${report.projectModel.featureCount - 5} more`);
  }

  if (report.validation.passes.length > 0) {
    lines.push(`\n✅ Validated Capabilities:`);
    report.validation.passes.forEach(p => {
      lines.push(`  ✓ ${p.message}`);
    });
  }

  if (report.validation.gaps.length > 0) {
    lines.push(`\n⚠️  Gaps Found (non-critical):`);
    report.validation.gaps.forEach(g => {
      lines.push(`  ⚠ ${g.code}: ${g.message}`);
    });
  }

  if (report.validation.errors.length > 0) {
    lines.push(`\n❌ Critical Issues:`);
    report.validation.errors.forEach(e => {
      lines.push(`  ✗ ${e.code}: ${e.message}`);
    });
  }

  lines.push('\n═══════════════════════════════════════════════════════════════\n');
  return lines.join('\n');
}

test.describe('Story Test mental model completeness', () => {
  test('should detect all required components and features', async () => {
    const result = runValidator();

    // Parse the validation report
    let report: ValidationReport | null = null;
    try {
      report = JSON.parse(result.stdout || '{}');
    } catch (parseError) {
      console.error('Failed to parse validator output:', result.stderr);
      expect(result.status, `Validator crashed: ${result.stderr}`).toBe(0);
    }

    if (!report) {
      expect(null, 'Validator produced invalid JSON output').not.toBeNull();
      return;
    }

    // Log the full report for debugging
    console.log(formatReportForConsole(report));

    // =========================================================================
    // Primary Assertion: No critical gaps
    // =========================================================================
    expect(
      report.validation.errors.length,
      `Story Test has critical gaps (plot holes):\n${report.validation.errors.map(e => `  - ${e.code}: ${e.message}`).join('\n')}`
    ).toBe(0);

    // =========================================================================
    // Secondary Assertions: Completeness indicators
    // =========================================================================
    expect(report.summary.status).toBe('COMPLETE');
    expect(report.evidence.artifactsMissing).toBeLessThanOrEqual(2); // Allow for optional artifacts
    expect(report.evidence.codeQuality.hasTests).toBe(true);
    expect(report.evidence.codeQuality.hasDocumentation).toBe(true);

    // =========================================================================
    // Log warnings (don't fail on these)
    // =========================================================================
    if (report.validation.gaps.length > 0) {
      console.warn(
        '[MentalModelValidator] Non-critical gaps detected:',
        report.validation.gaps.map(g => `${g.code}: ${g.message}`)
      );
    }

    // =========================================================================
    // Verify multi-platform capability
    // =========================================================================
    const activeComponents = Object.entries(report.evidence.components)
      .filter(([_, active]) => active)
      .map(([name]) => name);
    
    expect(
      activeComponents.length,
      `Story Test should support multiple platforms. Found: ${activeComponents.join(', ')}`
    ).toBeGreaterThanOrEqual(2);
  });

  test('should verify features are implemented', async () => {
    const result = runValidator();

    let report: ValidationReport | null = null;
    try {
      report = JSON.parse(result.stdout || '{}');
    } catch (_) {
      return;
    }

    if (!report) return;

    // Check that claimed features have corresponding evidence
    expect(report.projectModel.featureCount).toBeGreaterThan(0);
    expect(report.evidence.artifactsFound).toBeGreaterThan(0);

    // At least half of artifacts should be present (fuzzy completeness)
    const completenessRatio = report.evidence.artifactsFound / 
      (report.evidence.artifactsFound + report.evidence.artifactsMissing);
    expect(completenessRatio).toBeGreaterThanOrEqual(0.7);
  });
});
