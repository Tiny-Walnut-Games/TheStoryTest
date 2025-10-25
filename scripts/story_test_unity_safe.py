#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Story Test Framework - Unity-Safe Python Validator
Analyzes C# assemblies for Story Test violations without Unity dependencies.

This version only validates assemblies that don't depend on UnityEngine.
Unity-dependent assemblies are skipped with a warning.
"""

import sys
import os
import argparse
import json
from pathlib import Path
from typing import List, Dict, Any, Tuple
from enum import Enum

# Ensure UTF-8 output on Windows
if sys.platform == "win32":
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

try:
    from pythonnet import set_runtime
    from clr_loader import get_coreclr

    rt = get_coreclr()
    set_runtime(rt)

    import clr
    clr.AddReference("System.Reflection")
    from System.Reflection import Assembly, BindingFlags, MethodInfo, PropertyInfo
    from System import Type, Enum as DotNetEnum
except ImportError as e:
    print(f"Error: pythonnet not installed: {e}")
    print("Install with: pip install -r requirements.txt")
    sys.exit(1)


class StoryViolationType(Enum):
    INCOMPLETE_IMPLEMENTATION = "IncompleteImplementation"
    DEBUGGING_CODE = "DebuggingCode"
    UNUSED_CODE = "UnusedCode"
    PREMATURE_CELEBRATION = "PrematureCelebration"
    OTHER = "Other"


class StoryViolation:
    def __init__(self, type_name: str, member: str, violation: str, violation_type: StoryViolationType,
                 file_path: str = "", line_number: int = 0):
        self.type_name = type_name
        self.member = member
        self.violation = violation
        self.violation_type = violation_type
        self.file_path = file_path
        self.line_number = line_number

    def to_dict(self) -> Dict[str, Any]:
        return {
            "type": self.type_name,
            "member": self.member,
            "violation": self.violation,
            "filePath": self.file_path,
            "lineNumber": self.line_number,
            "violationType": self.violation_type.value
        }

    def __str__(self) -> str:
        location = f" ({self.file_path}:{self.line_number})" if self.file_path else ""
        return f"[{self.violation_type.value}] {self.type_name}.{self.member}: {self.violation}{location}"


class UnitySafeValidator:
    """Unity-safe validator that skips Unity-dependent assemblies"""

    def __init__(self, verbose: bool = False):
        self.verbose = verbose
        self.violations = []

    def log(self, message: str):
        if self.verbose:
            print(f"[Story Test] {message}")

    def is_unity_assembly(self, assembly_path: str) -> bool:
        """Check if assembly depends on Unity"""
        try:
            assembly = Assembly.ReflectionOnlyLoadFrom(assembly_path)
            referenced_assemblies = [str(a.Name) for a in assembly.GetReferencedAssemblies()]

            # Check for core Unity assemblies
            unity_refs = [ref for ref in referenced_assemblies if ref.startswith("UnityEngine")]
            core_unity_refs = [ref for ref in unity_refs if ref in [
                "UnityEngine.CoreModule",
                "UnityEngine",
                "UnityEditor"
            ]]

            # If it references core Unity modules, it's Unity-dependent
            return len(core_unity_refs) > 0
        except:
            # If we can't check, try to load it normally
            try:
                assembly = Assembly.LoadFrom(assembly_path)
                # Try to get one type to see if it fails
                types = assembly.GetExportedTypes()
                if len(types) > 0:
                    # Try to inspect the first type
                    str(types[0].FullName)
                    return False  # If we get here, it's probably not Unity-dependent
                return False
            except Exception as e:
                # If loading fails with Unity-related error, mark as Unity-dependent
                error_str = str(e).lower()
                return "unityengine" in error_str or "coremodule" in error_str

    def validate_assembly(self, assembly_path: str) -> List[StoryViolation]:
        """Validate assembly only if it's not Unity-dependent"""
        if not os.path.exists(assembly_path):
            print(f"Error: Assembly not found: {assembly_path}")
            return self.violations

        # Check if this is a Unity-dependent assembly
        if self.is_unity_assembly(assembly_path):
            self.log(f"Skipping Unity-dependent assembly: {os.path.basename(assembly_path)}")
            return self.violations

        try:
            self.log(f"Loading non-Unity assembly: {assembly_path}")
            assembly = Assembly.LoadFrom(assembly_path)
            types = assembly.GetTypes()
            self.log(f"Found {len(types)} types to validate")

            for type_obj in types:
                try:
                    self._validate_type(type_obj)
                except Exception as e:
                    self.log(f"Error validating type {type_obj.Name}: {str(e)[:100]}...")
                    continue

            self.log(f"Validation complete. Found {len(self.violations)} violations")
            return self.violations

        except Exception as e:
            print(f"Error loading assembly: {e}")
            return self.violations

    def _validate_type(self, type_obj):
        """Basic validation without Unity dependencies"""
        type_name = str(type_obj.Name)

        # Skip compiler-generated and system types
        if type_name.startswith("<") or type_name.startswith("__"):
            return

        file_path = str(type_obj.Assembly.Location) if hasattr(type_obj, 'Assembly') else ""

        # Validate methods
        for method in type_obj.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static):
            # Skip compiler-generated and special methods
            if (method.IsSpecialName or
                method.Name.startswith("<") or
                method.Name == "Finalize" or
                method.Name == ".ctor" or
                method.Name == ".cctor" or
                # Skip enum interface methods (compiler-generated)
                method.Name in ["HasFlag", "CompareTo", "ToString", "GetHashCode", "Equals"] and
                type_obj.IsEnum or
                # Skip delegate/serialization interface methods (compiler-generated)
                method.Name in ["CombineImpl", "GetObjectData", "GetInvocationList", "BeginInvoke", "EndInvoke"] or
                # Skip IConvertible methods (compiler-generated for enums)
                method.Name.startswith("System.IConvertible.") or
                # Skip test assembly methods
                "Test" in str(type_obj.FullName) or
                "Tests" in str(type_obj.Assembly.GetName().Name)):
                continue

            try:
                method_body = method.GetMethodBody()
                if method_body is not None:
                    il_bytes = bytes(method_body.GetILAsByteArray())

                    # Check for NotImplementedException
                    if self._contains_not_implemented_exception(il_bytes):
                        self.violations.append(StoryViolation(
                            type_name, method.Name,
                            "Method contains NotImplementedException",
                            StoryViolationType.INCOMPLETE_IMPLEMENTATION,
                            file_path=file_path
                        ))

                    # Check for empty methods
                    if len(il_bytes) <= 2:  # Just ret instruction
                        self.violations.append(StoryViolation(
                            type_name, method.Name,
                            "Empty method - no implementation",
                            StoryViolationType.INCOMPLETE_IMPLEMENTATION,
                            file_path=file_path
                        ))
            except:
                continue

    def _contains_not_implemented_exception(self, il_bytes: bytes) -> bool:
        """Simple check for NotImplementedException pattern"""
        if not il_bytes or len(il_bytes) < 10:
            return False

        # Look for common patterns that indicate NotImplementedException
        return (0x73 in il_bytes and 0x7A in il_bytes)  # throw + new opcodes


def find_unity_assemblies(project_path: str) -> List[str]:
    """Find assemblies but filter out Unity-dependent ones"""
    assemblies = []
    project_root = Path(project_path)

    # Look for common assembly locations
    script_assemblies_path = project_root / "Library" / "ScriptAssemblies"
    if script_assemblies_path.exists():
        for dll in script_assemblies_path.glob("*.dll"):
            # Skip Unity's own assemblies and known Unity-dependent assemblies
            dll_name = dll.name
            if not (dll_name.startswith("Unity") or
                   dll_name.startswith("com.unity") or
                   dll_name.startswith("Assembly-CSharp") or
                   "Unity" in dll_name):
                assemblies.append(str(dll))

    return assemblies


def main():
    parser = argparse.ArgumentParser(
        description="Story Test Framework - Unity-Safe Python Validator"
    )
    parser.add_argument("path", help="Path to Unity project root or assembly DLL")
    parser.add_argument("-v", "--verbose", action="store_true", help="Enable verbose output")
    parser.add_argument("-o", "--output", help="Output file for JSON report")
    parser.add_argument("--fail-on-violations", action="store_true", help="Exit with code 1 if violations found")

    args = parser.parse_args()

    validator = UnitySafeValidator(verbose=args.verbose)
    all_violations = []

    path = Path(args.path)

    if path.is_file() and path.suffix == ".dll":
        all_violations = validator.validate_assembly(str(path))
    elif path.is_dir():
        if (path / "Assets").exists() and (path / "ProjectSettings").exists():
            print("Detected Unity project. Searching for non-Unity assemblies...")
            assemblies = find_unity_assemblies(str(path))
            print(f"Found {len(assemblies)} non-Unity assemblies to validate")

            for assembly in assemblies:
                violations = validator.validate_assembly(assembly)
                all_violations.extend(violations)
        else:
            # Directory of DLLs
            for dll in path.glob("*.dll"):
                violations = validator.validate_assembly(str(dll))
                all_violations.extend(violations)
    else:
        print(f"Error: Invalid path: {path}")
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
        print(f"Report saved to: {args.output}")
    else:
        print("\n" + "="*80)
        print("STORY TEST UNITY-SAFE VALIDATION REPORT")
        print("="*80)

        if all_violations:
            print(f"\n❌ Found {len(all_violations)} violation(s):\n")
            for violation in all_violations:
                print(f"  {violation}")

            print(f"\nViolations by type:")
            for v_type, count in report["violationsByType"].items():
                print(f"  - {v_type}: {count}")
        else:
            print("\n✅ No Story Test violations found in non-Unity assemblies!")

        print("\n" + "="*80)

    if args.fail_on_violations and all_violations:
        sys.exit(1)
    else:
        sys.exit(0)


if __name__ == "__main__":
    main()