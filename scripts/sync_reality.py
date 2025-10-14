#!/usr/bin/env python3
"""
🧠 REALITY SYNC - Project State Synchronization
Automatically updates REALITY_CHECK.md with actual project state
Fights LLM false celebration and documentation drift
"""

import os
import json
import subprocess
import sys
from datetime import datetime
from pathlib import Path

def run_command(cmd):
    """Run command and return output"""
    try:
        result = subprocess.run(cmd, shell=True, capture_output=True, text=True, cwd=Path(__file__).parent.parent)
        return result.stdout.strip(), result.stderr.strip()
    except Exception as e:
        return "", str(e)

def get_violations():
    """Get current Story Test violations"""
    stdout, stderr = run_command("python scripts/story_test.py . --verbose")

    if "Found 0 violation" in stdout:
        return 0, {}

    # Parse violations from output
    lines = stdout.split('\n')
    violations_by_type = {}
    total_violations = 0

    for line in lines:
        if "Found" in line and "violation" in line:
            try:
                total_violations = int(line.split("Found")[1].split("violation")[0].strip())
            except:
                pass
        elif "violations by type:" in line.lower():
            # Parse violation types from next lines
            idx = lines.index(line) + 1
            while idx < len(lines) and lines[idx].strip().startswith('-'):
                vline = lines[idx].strip()
                if ':' in vline:
                    vtype = vline.split(':')[0].replace('- ', '').strip()
                    count = vline.split(':')[1].strip()
                    try:
                        violations_by_type[vtype] = int(count)
                    except:
                        pass
                idx += 1

    return total_violations, violations_by_type

def count_acts():
    """Count actual Acts in the project"""
    acts_dir = Path("Packages/com.tinywalnutgames.storytest/Runtime/Acts")
    if not acts_dir.exists():
        return 0

    acts = [f for f in acts_dir.glob("Act*.cs") if f.is_file()]
    return len(acts)

def check_ci_status():
    """Check CI/CD configuration"""
    workflow_file = Path(".github/workflows/story-test.yml")
    if not workflow_file.exists():
        return "❌ No workflow file"

    with open(workflow_file, 'r', encoding='utf-8') as f:
        content = f.read()

    if "UNITY_LICENSE" in content and "UNITY_EMAIL" in content:
        return "✅ Configured with Unity secrets"
    else:
        return "⚠️ Missing Unity secrets"

def check_python_validator():
    """Test Python validator functionality"""
    stdout, stderr = run_command("python scripts/story_test.py --help")
    if "usage:" in stdout.lower():
        return "✅ Working"
    else:
        return "❌ Broken"

def update_reality_check():
    """Update REALITY_CHECK.md with current state"""
    # Get current state
    violations_count, violations_by_type = get_violations()
    acts_count = count_acts()
    ci_status = check_ci_status()
    python_status = check_python_validator()

    # Read current template
    reality_file = Path("REALITY_CHECK.md")
    if not reality_file.exists():
        print("❌ REALITY_CHECK.md not found")
        return False

    with open(reality_file, 'r', encoding='utf-8') as f:
        content = f.read()

    # Update timestamp
    content = content.replace(
        "**Last Updated: 2025-10-07**",
        f"**Last Updated: {datetime.now().strftime('%Y-%m-%d')}**"
    )

    # Update violations section
    violations_text = f"```\nTotal: {violations_count} violations found\n"
    for vtype, count in violations_by_type.items():
        violations_text += f"- {vtype}: {count}\n"
    violations_text += "```"

    # Replace violations section
    import re
    violations_pattern = r'### ⚠️ \*\*CURRENT VIOLATIONS\*\*.*?```'
    content = re.sub(violations_pattern, f'### ⚠️ **CURRENT VIOLATIONS** (Real Data):\n{violations_text}', content, flags=re.DOTALL)

    # Update Acts count
    content = content.replace("11 Acts (not 9 like docs claim)", f"{acts_count} Acts (not 9 like docs claim)")

    # Write back
    with open(reality_file, 'w', encoding='utf-8') as f:
        f.write(content)

    print(f"✅ REALITY_CHECK.md updated")
    print(f"   - Violations: {violations_count}")
    print(f"   - Acts: {acts_count}")
    print(f"   - CI Status: {ci_status}")
    print(f"   - Python Validator: {python_status}")

    return True

def main():
    """Main sync function"""
    print("🧠 REALITY SYNC - Updating project state...")

    if not update_reality_check():
        sys.exit(1)

    print("\n📋 Current Project Reality:")
    print("   - Check REALITY_CHECK.md for updated state")
    print("   - Share this file with AI assistants")
    print("   - Use as truth source instead of documentation")

if __name__ == "__main__":
    main()