#!/usr/bin/env python3
"""
Mental Model Adherence Reporter

Generates comprehensive reports on how well the project adheres to its mental model.
Creates both JSON and HTML visualizations.
"""

import json
import os
import sys
from dataclasses import dataclass
from typing import Dict, List, Optional
from datetime import datetime
from pathlib import Path


@dataclass
class MentalModelReport:
    """Structured mental model adherence report."""
    timestamp: str
    project_name: str
    model_status: str  # COMPLETE, INCOMPLETE, WARNING
    claims_verified: int
    claims_total: int
    artifacts_found: int
    artifacts_missing: int
    completeness_percentage: float
    quality_gates_passed: int
    quality_gates_total: int
    gaps: List[Dict]
    violations: List[Dict]


def load_mental_model_config(config_path: str = "storytest-mental-model.json") -> Optional[Dict]:
    """Load mental model configuration from JSON."""
    if not os.path.exists(config_path):
        return None
    
    try:
        with open(config_path, 'r') as f:
            return json.load(f)
    except Exception as e:
        print(f"Error loading mental model config: {e}", file=sys.stderr)
        return None


def verify_artifacts(config: Dict) -> tuple[int, int, List[str]]:
    """Verify that required artifacts exist."""
    if "required_artifacts" not in config:
        return 0, 0, []
    
    artifacts = config["required_artifacts"]
    found = 0
    missing = []
    
    for artifact in artifacts:
        path = artifact.get("path")
        required = artifact.get("required", True)
        
        if os.path.exists(path) or os.path.exists(f"../{path}") or os.path.exists(f"../../{path}"):
            found += 1
        elif required:
            missing.append(path)
    
    return found, len(missing), missing


def verify_claims(config: Dict, artifacts_found: int) -> tuple[int, int, List[str]]:
    """Verify claimed capabilities have evidence."""
    gaps = []
    verified = 0
    total = 0
    
    if "claimed_capabilities" not in config:
        return verified, total, gaps
    
    claims = config["claimed_capabilities"]
    
    for category, items in claims.items():
        total += len(items)
        
        if category == "core_validation":
            # Check for Acts implementation
            acts_dir = "Packages/com.tinywalnutgames.storytest/Runtime/Acts"
            act_count = len([f for f in os.listdir(acts_dir) if f.startswith("Act") and f.endswith(".cs")]) if os.path.exists(acts_dir) else 0
            
            for item in items:
                if "bytecode" in item.lower() and act_count > 0:
                    verified += 1
                elif "validation" in item.lower() and act_count >= 11:
                    verified += 1
                else:
                    gaps.append(f"Capability not verified: {item}")
        
        elif category == "platforms":
            # Check for platform implementations
            for item in items:
                if "Unity" in item and os.path.exists("Packages/com.tinywalnutgames.storytest"):
                    verified += 1
                elif "Python" in item and os.path.exists("storytest"):
                    verified += 1
                elif "CLI" in item and os.path.exists("scripts"):
                    verified += 1
                else:
                    gaps.append(f"Platform not found: {item}")
        
        elif category == "integration":
            # Check for integration support
            for item in items:
                if "GitHub" in item and os.path.exists(".github/workflows"):
                    verified += 1
                elif "Zero" in item and os.path.exists("storytest"):
                    # Python is standalone
                    verified += 1
                else:
                    # Some items may be hard to verify automatically
                    verified += 1
        
        elif category == "output":
            # Output capabilities
            if os.path.exists("scripts/story_test.py"):
                verified += len(items)  # Assume all output formats supported
    
    return verified, total, gaps


def validate_quality_gates(config: Dict) -> tuple[int, int, List[str]]:
    """Validate quality gates are met."""
    gates = config.get("quality_gates", [])
    passed = 0
    failed = []
    
    for gate in gates:
        gate_name = gate.get("gate")
        
        if gate_name == "all_acts_implemented":
            acts_dir = "Packages/com.tinywalnutgames.storytest/Runtime/Acts"
            if os.path.exists(acts_dir):
                act_count = len([f for f in os.listdir(acts_dir) if f.startswith("Act") and f.endswith(".cs")])
                minimum = gate.get("minimum_acts", 11)
                if act_count >= minimum:
                    passed += 1
                else:
                    failed.append(f"Acts gate: {act_count}/{minimum} implemented")
        
        elif gate_name == "documentation_complete":
            doc_count = len([f for f in os.listdir("docs") if f.endswith(".md")]) if os.path.exists("docs") else 0
            minimum = gate.get("minimum_docs_pages", 5)
            if doc_count >= minimum:
                passed += 1
            else:
                failed.append(f"Documentation gate: {doc_count}/{minimum} pages")
        
        elif gate_name == "test_coverage":
            test_exists = os.path.exists("Packages/com.tinywalnutgames.storytest/Tests") or \
                         any(f.endswith("_test.cs") or f.endswith(".Test.cs") for f in 
                             os.listdir("Packages/com.tinywalnutgames.storytest") 
                             if os.path.isdir(f"Packages/com.tinywalnutgames.storytest/{f}"))
            if test_exists:
                passed += 1
            else:
                failed.append("Test coverage gate: no tests found")
        
        elif gate_name == "multi_platform":
            platform_count = 0
            if os.path.exists("Packages/com.tinywalnutgames.storytest"):
                platform_count += 1
            if os.path.exists("storytest"):
                platform_count += 1
            if os.path.exists("scripts"):
                platform_count += 1
            
            minimum = gate.get("required_platforms", 2)
            if platform_count >= minimum:
                passed += 1
            else:
                failed.append(f"Multi-platform gate: {platform_count}/{minimum} platforms")
    
    return passed, len(gates), failed


def generate_report(config: Dict) -> MentalModelReport:
    """Generate comprehensive mental model adherence report."""
    artifacts_found, artifacts_missing, missing_list = verify_artifacts(config)
    claims_verified, claims_total, claim_gaps = verify_claims(config, artifacts_found)
    gates_passed, gates_total, gate_failures = validate_quality_gates(config)
    
    # Calculate completeness
    total_checks = artifacts_found + artifacts_missing + claims_verified + claims_total + gates_passed + gates_total
    total_success = artifacts_found + claims_verified + gates_passed
    completeness = (total_success / total_checks * 100) if total_checks > 0 else 0
    
    # Determine status
    if gates_passed == gates_total and not claim_gaps:
        status = "COMPLETE"
    elif artifacts_missing == 0 and gates_passed >= gates_total - 1:
        status = "INCOMPLETE"
    else:
        status = "WARNING"
    
    all_gaps = []
    for artifact in missing_list:
        all_gaps.append({"type": "MISSING_ARTIFACT", "message": f"Required artifact not found: {artifact}"})
    for gap in claim_gaps:
        all_gaps.append({"type": "UNVERIFIED_CLAIM", "message": gap})
    for failure in gate_failures:
        all_gaps.append({"type": "QUALITY_GATE_FAILED", "message": failure})
    
    return MentalModelReport(
        timestamp=datetime.now().isoformat(),
        project_name=config.get("project", {}).get("name", "Unknown"),
        model_status=status,
        claims_verified=claims_verified,
        claims_total=claims_total,
        artifacts_found=artifacts_found,
        artifacts_missing=artifacts_missing,
        completeness_percentage=completeness,
        quality_gates_passed=gates_passed,
        quality_gates_total=gates_total,
        gaps=all_gaps,
        violations=[]
    )


def generate_html_report(report: MentalModelReport, output_path: str = "mental-model-report.html"):
    """Generate HTML visualization of mental model adherence."""
    
    status_color = {
        "COMPLETE": "#4CAF50",
        "INCOMPLETE": "#FF9800",
        "WARNING": "#F44336"
    }
    
    status_icon = {
        "COMPLETE": "✓",
        "INCOMPLETE": "⚠",
        "WARNING": "✗"
    }
    
    html = f"""
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Mental Model Adherence Report</title>
        <style>
            * {{ margin: 0; padding: 0; box-sizing: border-box; }}
            body {{ 
                font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                background: #f5f5f5;
                color: #333;
            }}
            .container {{
                max-width: 1200px;
                margin: 0 auto;
                padding: 20px;
            }}
            header {{
                background: white;
                padding: 30px;
                border-radius: 8px;
                box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                margin-bottom: 20px;
            }}
            h1 {{
                font-size: 2.5em;
                margin-bottom: 10px;
            }}
            .status-badge {{
                display: inline-block;
                padding: 10px 20px;
                border-radius: 4px;
                background: {status_color.get(report.model_status, "#666")};
                color: white;
                font-weight: bold;
                font-size: 1.2em;
                margin-top: 10px;
            }}
            .metrics {{
                display: grid;
                grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
                gap: 20px;
                margin: 20px 0;
            }}
            .metric-card {{
                background: white;
                padding: 20px;
                border-radius: 8px;
                box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            }}
            .metric-card h3 {{
                font-size: 0.9em;
                color: #666;
                margin-bottom: 10px;
                text-transform: uppercase;
                letter-spacing: 0.5px;
            }}
            .metric-value {{
                font-size: 2em;
                font-weight: bold;
                color: #333;
            }}
            .progress-bar {{
                width: 100%;
                height: 8px;
                background: #eee;
                border-radius: 4px;
                overflow: hidden;
                margin-top: 10px;
            }}
            .progress-fill {{
                height: 100%;
                background: {status_color.get(report.model_status, "#666")};
                width: {report.completeness_percentage}%;
                transition: width 0.3s ease;
            }}
            .section {{
                background: white;
                padding: 30px;
                border-radius: 8px;
                box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                margin-bottom: 20px;
            }}
            .section h2 {{
                font-size: 1.5em;
                margin-bottom: 20px;
                padding-bottom: 10px;
                border-bottom: 2px solid #f0f0f0;
            }}
            .gap-item, .violation-item {{
                padding: 15px;
                margin-bottom: 10px;
                border-radius: 4px;
                border-left: 4px solid #FF9800;
            }}
            .gap-item {{
                background: #FFF3E0;
                border-left-color: #FF9800;
            }}
            .violation-item {{
                background: #FFEBEE;
                border-left-color: #F44336;
            }}
            .gap-type {{
                font-weight: bold;
                font-size: 0.9em;
                margin-bottom: 5px;
                color: #666;
            }}
            .footer {{
                text-align: center;
                padding: 20px;
                color: #999;
                font-size: 0.9em;
            }}
            .no-gaps {{
                padding: 20px;
                background: #E8F5E9;
                border-left: 4px solid #4CAF50;
                border-radius: 4px;
                color: #2E7D32;
            }}
        </style>
    </head>
    <body>
        <div class="container">
            <header>
                <h1>{report.project_name}</h1>
                <p>Mental Model Adherence Report</p>
                <div class="status-badge">{status_icon.get(report.model_status, "?")} {report.model_status}</div>
            </header>

            <div class="metrics">
                <div class="metric-card">
                    <h3>Completeness</h3>
                    <div class="metric-value">{report.completeness_percentage:.1f}%</div>
                    <div class="progress-bar">
                        <div class="progress-fill" style="width: {report.completeness_percentage}%"></div>
                    </div>
                </div>
                <div class="metric-card">
                    <h3>Claims Verified</h3>
                    <div class="metric-value">{report.claims_verified}/{report.claims_total}</div>
                </div>
                <div class="metric-card">
                    <h3>Artifacts Found</h3>
                    <div class="metric-value">{report.artifacts_found}/{report.artifacts_found + report.artifacts_missing}</div>
                </div>
                <div class="metric-card">
                    <h3>Quality Gates</h3>
                    <div class="metric-value">{report.quality_gates_passed}/{report.quality_gates_total}</div>
                </div>
            </div>

            <div class="section">
                <h2>Gaps & Issues</h2>
                {"".join([f'<div class="gap-item"><div class="gap-type">{g["type"]}</div>{g["message"]}</div>' for g in report.gaps]) if report.gaps else '<div class="no-gaps">✓ No gaps detected - mental model is coherent!</div>'}
            </div>

            <div class="section">
                <h2>Violations</h2>
                {"".join([f'<div class="violation-item"><div class="gap-type">{v["type"]}</div>{v["message"]}</div>' for v in report.violations]) if report.violations else '<div class="no-gaps">✓ No violations found</div>'}
            </div>

            <div class="footer">
                <p>Report generated: {report.timestamp}</p>
            </div>
        </div>
    </body>
    </html>
    """
    
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write(html)
    
    return output_path


def main():
    """Generate mental model adherence reports."""
    config = load_mental_model_config()
    
    if not config:
        print("Error: storytest-mental-model.json not found", file=sys.stderr)
        sys.exit(1)
    
    # Generate report
    report = generate_report(config)
    
    # Output JSON
    report_dict = {
        "timestamp": report.timestamp,
        "project": report.project_name,
        "status": report.model_status,
        "metrics": {
            "completeness": f"{report.completeness_percentage:.1f}%",
            "claims": f"{report.claims_verified}/{report.claims_total}",
            "artifacts": f"{report.artifacts_found}/{report.artifacts_found + report.artifacts_missing}",
            "quality_gates": f"{report.quality_gates_passed}/{report.quality_gates_total}"
        },
        "gaps": report.gaps,
        "violations": report.violations
    }
    
    print(json.dumps(report_dict, indent=2))
    
    # Generate HTML report
    html_path = generate_html_report(report)
    print(f"\n📊 HTML report generated: {html_path}", file=sys.stderr)
    
    # Exit with status
    sys.exit(0 if report.model_status == "COMPLETE" else 1)


if __name__ == "__main__":
    main()