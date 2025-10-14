#!/usr/bin/env python3
"""
Story Test Framework - Command Line Interface
"""

import sys
import json
import argparse
from pathlib import Path
from typing import List

from .validator import StoryTestValidator, StoryViolation, StoryViolationType, find_assemblies


def main():
    """Main CLI entry point"""
    parser = argparse.ArgumentParser(
        prog="storytest",
        description="Story Test Framework - Validate .NET assemblies for code quality",
        epilog="For more information: https://github.com/jmeyer1980/TheStoryTest"
    )
    
    subparsers = parser.add_subparsers(dest="command", help="Available commands")
    
    # Validate command
    validate_parser = subparsers.add_parser(
        "validate",
        help="Validate assemblies for Story Test violations"
    )
    validate_parser.add_argument(
        "path",
        help="Path to Unity project root, .NET project, or assembly DLL"
    )
    validate_parser.add_argument(
        "-v", "--verbose",
        action="store_true",
        help="Enable verbose output"
    )
    validate_parser.add_argument(
        "-o", "--output",
        help="Output file for JSON report"
    )
    validate_parser.add_argument(
        "--fail-on-violations",
        action="store_true",
        help="Exit with code 1 if violations found"
    )
    
    # Version command
    version_parser = subparsers.add_parser("version", help="Show version information")
    
    args = parser.parse_args()
    
    if args.command == "version":
        from . import __version__
        print(f"Story Test Framework v{__version__}")
        print("https://github.com/jmeyer1980/TheStoryTest")
        sys.exit(0)
    
    elif args.command == "validate":
        run_validation(args)
    
    else:
        parser.print_help()
        sys.exit(1)


def run_validation(args):
    """Run validation command"""
    validator = StoryTestValidator(verbose=args.verbose)
    all_violations: List[StoryViolation] = []
    
    path = Path(args.path)
    
    if path.is_file() and path.suffix == ".dll":
        # Single DLL file
        all_violations = validator.validate_assembly(str(path))
    
    elif path.is_dir():
        # Directory - check if Unity project or .NET project
        if (path / "Assets").exists() and (path / "ProjectSettings").exists():
            print("✓ Detected Unity project. Searching for non-Unity assemblies...")
            assemblies = find_assemblies(str(path))
            print(f"✓ Found {len(assemblies)} assemblies to validate\n")
            
            for assembly in assemblies:
                violations = validator.validate_assembly(assembly)
                all_violations.extend(violations)
        else:
            # Generic directory - validate all DLLs
            print(f"✓ Searching for assemblies in: {path}\n")
            dll_files = list(path.glob("*.dll"))
            
            if not dll_files:
                print(f"⚠ No DLL files found in {path}")
                sys.exit(1)
            
            for dll in dll_files:
                violations = validator.validate_assembly(str(dll))
                all_violations.extend(violations)
    
    else:
        print(f"❌ Error: Invalid path: {path}")
        sys.exit(1)
    
    # Generate report
    report = {
        "totalViolations": len(all_violations),
        "violations": [v.to_dict() for v in all_violations],
        "violationsByType": {}
    }
    
    for v_type in StoryViolationType:
        count = sum(1 for v in all_violations if v.violation_type == v_type)
        if count > 0:
            report["violationsByType"][v_type.value] = count
    
    # Output report
    if args.output:
        with open(args.output, 'w') as f:
            json.dump(report, f, indent=2)
        print(f"\n✓ Report saved to: {args.output}")
    else:
        print_report(all_violations, report)
    
    # Exit with appropriate code
    if args.fail_on_violations and all_violations:
        sys.exit(1)
    else:
        sys.exit(0)


def print_report(violations: List[StoryViolation], report: dict):
    """Print formatted validation report"""
    print("\n" + "=" * 80)
    print("STORY TEST VALIDATION REPORT")
    print("=" * 80)
    
    if violations:
        print(f"\n❌ Found {len(violations)} violation(s):\n")
        for violation in violations:
            print(f"  {violation}")
        
        print(f"\n📊 Violations by type:")
        for v_type, count in report["violationsByType"].items():
            print(f"  • {v_type}: {count}")
    else:
        print("\n✅ No Story Test violations found!")
        print("\n🎉 All assemblies pass Story Test validation.")
        print("   Every symbol tells a story. No placeholders, no TODOs, no unused code.")
    
    print("\n" + "=" * 80)


if __name__ == "__main__":
    main()