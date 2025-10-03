#!/usr/bin/env python3
"""
Story Test Framework - Standalone Python Validator
Analyzes C# assemblies for Story Test violations without requiring Unity runtime.

Cross-platform compatible: Windows, Linux, MacOS
Requires: Python 3.8+, pythonnet (for .NET reflection)
"""

import sys
import os
import argparse
import json
from pathlib import Path
from typing import List, Dict, Any, Tuple
from enum import Enum

try:
    # Configure pythonnet to use CoreCLR instead of Mono (cross-platform)
    from pythonnet import set_runtime
    from clr_loader import get_coreclr
    
    # Use CoreCLR runtime (works on Windows, Linux, macOS)
    rt = get_coreclr()
    set_runtime(rt)
    
    import clr
    clr.AddReference("System.Reflection")
    from System.Reflection import Assembly, BindingFlags, MethodInfo, PropertyInfo
    from System import Type, Enum as DotNetEnum
except ImportError as e:
    print(f"Error: pythonnet not installed or runtime configuration failed: {e}")
    print("Install with: pip install pythonnet")
    sys.exit(1)


class StoryViolationType(Enum):
    """Types of Story Test violations"""
    INCOMPLETE_IMPLEMENTATION = "IncompleteImplementation"
    DEBUGGING_CODE = "DebuggingCode"
    UNUSED_CODE = "UnusedCode"
    PREMATURE_CELEBRATION = "PrematureCelebration"
    OTHER = "Other"


class StoryViolation:
    """Represents a Story Test violation"""
    def __init__(self, type_name: str, member: str, violation: str, violation_type: StoryViolationType):
        self.type_name = type_name
        self.member = member
        self.violation = violation
        self.violation_type = violation_type

    def to_dict(self) -> Dict[str, str]:
        return {
            "type": self.type_name,
            "member": self.member,
            "violation": self.violation,
            "violationType": self.violation_type.value
        }

    def __str__(self) -> str:
        return f"[{self.violation_type.value}] {self.type_name}.{self.member}: {self.violation}"


class ILAnalyzer:
    """Analyzes IL bytecode for common Story Test patterns"""
    
    @staticmethod
    def contains_throw_not_implemented(il_bytes: bytes) -> bool:
        """Detects NotImplementedException pattern (opcodes 0x73 + 0x7A)"""
        if not il_bytes or len(il_bytes) == 0:
            return False
        
        for i in range(len(il_bytes) - 4):
            if il_bytes[i] == 0x73 and i + 5 < len(il_bytes) and il_bytes[i + 5] == 0x7A:
                return True
        return False
    
    @staticmethod
    def is_only_default_return(method_info, il_bytes: bytes) -> bool:
        """Detects methods that only return default values (ldc.i4.* + ret)"""
        if not hasattr(method_info, 'ReturnType'):
            return False
        
        # Skip void methods
        return_type = str(method_info.ReturnType)
        if return_type == "System.Void":
            return False
        
        if not il_bytes or len(il_bytes) == 0:
            return False
        
        # Check for very short IL with default return pattern
        if len(il_bytes) <= 8:
            for i in range(len(il_bytes) - 1):
                # ldc.i4.* opcodes (0x14-0x17) followed by ret (0x2A)
                if il_bytes[i] in [0x14, 0x16, 0x17] and il_bytes[i + 1] == 0x2A:
                    return True
        return False


class StoryTestValidator:
    """Main validator implementing the 9 Acts"""
    
    def __init__(self, verbose: bool = False):
        self.verbose = verbose
        self.violations: List[StoryViolation] = []
    
    def log(self, message: str):
        """Log message if verbose mode enabled"""
        if self.verbose:
            print(f"[Story Test] {message}")
    
    def validate_assembly(self, assembly_path: str) -> List[StoryViolation]:
        """Validate a .NET assembly for Story Test violations"""
        self.violations = []
        
        if not os.path.exists(assembly_path):
            print(f"Error: Assembly not found: {assembly_path}")
            return self.violations
        
        try:
            self.log(f"Loading assembly: {assembly_path}")
            assembly = Assembly.LoadFrom(assembly_path)
            
            types = assembly.GetTypes()
            self.log(f"Found {len(types)} types to validate")
            
            for type_obj in types:
                self._validate_type(type_obj)
            
            self.log(f"Validation complete. Found {len(self.violations)} violations")
            return self.violations
            
        except Exception as e:
            print(f"Error loading assembly: {e}")
            return self.violations
    
    def _has_story_ignore_attribute(self, member) -> bool:
        """Check if member has [StoryIgnore] attribute"""
        try:
            attrs = member.GetCustomAttributes(False)
            for attr in attrs:
                if "StoryIgnore" in str(attr.GetType()):
                    return True
        except:
            pass
        return False
    
    def _validate_type(self, type_obj):
        """Validate a single type"""
        if self._has_story_ignore_attribute(type_obj):
            return
        
        type_name = str(type_obj.Name)
        
        # Act 3: Incomplete Classes
        self._check_incomplete_classes(type_obj, type_name)
        
        # Act 8: Hollow Enums
        if type_obj.IsEnum:
            self._check_hollow_enums(type_obj, type_name)
        
        # Validate methods
        try:
            methods = type_obj.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            for method in methods:
                self._validate_method(type_obj, method, type_name)
        except:
            pass
        
        # Validate properties
        try:
            properties = type_obj.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            for prop in properties:
                self._validate_property(type_obj, prop, type_name)
        except:
            pass
    
    def _validate_method(self, type_obj, method, type_name: str):
        """Validate a method"""
        if self._has_story_ignore_attribute(method):
            return
        
        method_name = str(method.Name)
        
        # Skip compiler-generated methods
        if method_name.startswith("<") or method.IsSpecialName:
            return
        
        # Act 1: TODO Comments (NotImplementedException)
        # Act 2: Placeholder Implementations
        self._check_todo_and_placeholders(method, type_name, method_name)
        
        # Act 4: Unsealed Abstract Members
        self._check_unsealed_abstract(method, type_obj, type_name, method_name)
        
        # Act 5: Debug-Only Implementations
        self._check_debug_only(method, type_name, method_name)
        
        # Act 7: Cold Methods
        self._check_cold_methods(method, type_name, method_name)
        
        # Act 9: Premature Celebrations
        self._check_premature_celebrations(method, type_name, method_name)
    
    def _validate_property(self, type_obj, prop, type_name: str):
        """Validate a property"""
        if self._has_story_ignore_attribute(prop):
            return
        
        prop_name = str(prop.Name)
        
        # Act 6: Phantom Props
        self._check_phantom_props(prop, type_name, prop_name)
    
    def _check_todo_and_placeholders(self, method, type_name: str, method_name: str):
        """Act 1 & 2: Check for TODO comments and placeholder implementations"""
        try:
            method_body = method.GetMethodBody()
            if method_body is None:
                return
            
            il_bytes = bytes(method_body.GetILAsByteArray())
            
            # Check for NotImplementedException
            if ILAnalyzer.contains_throw_not_implemented(il_bytes):
                self.violations.append(StoryViolation(
                    type_name, method_name,
                    "Method contains NotImplementedException indicating TODO implementation",
                    StoryViolationType.INCOMPLETE_IMPLEMENTATION
                ))
                return
            
            # Check for methods that only return default values
            if ILAnalyzer.is_only_default_return(method, il_bytes):
                self.violations.append(StoryViolation(
                    type_name, method_name,
                    "Method only returns default value without implementation",
                    StoryViolationType.INCOMPLETE_IMPLEMENTATION
                ))
        except:
            pass
    
    def _check_incomplete_classes(self, type_obj, type_name: str):
        """Act 3: Check for incomplete class implementations"""
        if type_obj.IsInterface or type_obj.IsAbstract:
            return
        
        try:
            methods = type_obj.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            abstract_methods = [m for m in methods if m.IsAbstract]
            
            if abstract_methods:
                method_names = ", ".join([str(m.Name) for m in abstract_methods])
                self.violations.append(StoryViolation(
                    type_name, type_name,
                    f"Class has unimplemented abstract methods: {method_names}",
                    StoryViolationType.INCOMPLETE_IMPLEMENTATION
                ))
        except:
            pass
    
    def _check_unsealed_abstract(self, method, type_obj, type_name: str, method_name: str):
        """Act 4: Check for unsealed abstract members"""
        try:
            if method.IsAbstract and not type_obj.IsAbstract and not type_obj.IsInterface:
                self.violations.append(StoryViolation(
                    type_name, method_name,
                    "Abstract method in non-abstract class (unsealed narrative element)",
                    StoryViolationType.INCOMPLETE_IMPLEMENTATION
                ))
        except:
            pass
    
    def _check_debug_only(self, method, type_name: str, method_name: str):
        """Act 5: Check for debug-only implementations"""
        if method_name.startswith("Debug") or method_name.startswith("Test") or "Temp" in method_name:
            # Check for Obsolete attribute
            try:
                attrs = method.GetCustomAttributes(False)
                has_obsolete = any("Obsolete" in str(attr.GetType()) for attr in attrs)
                
                if not has_obsolete:
                    self.violations.append(StoryViolation(
                        type_name, method_name,
                        "Debug/Test method without Obsolete attribute (should be temporary)",
                        StoryViolationType.DEBUGGING_CODE
                    ))
            except:
                pass
    
    def _check_phantom_props(self, prop, type_name: str, prop_name: str):
        """Act 6: Check for phantom properties"""
        # Simple heuristic: check for suspicious property names
        if "Unused" in prop_name or "Temp" in prop_name or "Placeholder" in prop_name:
            self.violations.append(StoryViolation(
                type_name, prop_name,
                "Phantom property detected - name suggests it's unused",
                StoryViolationType.UNUSED_CODE
            ))
    
    def _check_cold_methods(self, method, type_name: str, method_name: str):
        """Act 7: Check for cold (empty) methods"""
        if method.IsAbstract or method.IsVirtual:
            return
        
        try:
            method_body = method.GetMethodBody()
            if method_body is None:
                return
            
            il_bytes = bytes(method_body.GetILAsByteArray())
            
            # Check for methods with just ret instruction (≤3 bytes)
            if len(il_bytes) <= 3:
                self.violations.append(StoryViolation(
                    type_name, method_name,
                    "Cold method detected - method body is empty or minimal",
                    StoryViolationType.UNUSED_CODE
                ))
        except:
            pass
    
    def _check_hollow_enums(self, type_obj, type_name: str):
        """Act 8: Check for hollow enums"""
        try:
            enum_values = DotNetEnum.GetValues(type_obj)
            if len(enum_values) <= 1:
                self.violations.append(StoryViolation(
                    type_name, type_name,
                    "Hollow enum detected - enum has no or minimal values defined",
                    StoryViolationType.UNUSED_CODE
                ))
                return
            
            # Check for placeholder enum names
            enum_names = DotNetEnum.GetNames(type_obj)
            placeholder_names = [n for n in enum_names if "Placeholder" in n or "TODO" in n or "Temp" in n]
            
            if placeholder_names:
                self.violations.append(StoryViolation(
                    type_name, type_name,
                    f"Hollow enum detected - contains placeholder values: {', '.join(placeholder_names)}",
                    StoryViolationType.UNUSED_CODE
                ))
        except:
            pass
    
    def _check_premature_celebrations(self, method, type_name: str, method_name: str):
        """Act 9: Check for premature celebrations"""
        try:
            # Check if method is marked as "Complete" or similar
            attrs = method.GetCustomAttributes(False)
            has_completion_marker = any(
                "Complete" in str(attr.GetType()) or 
                "Finished" in str(attr.GetType()) or 
                "Done" in str(attr.GetType())
                for attr in attrs
            )
            
            if has_completion_marker:
                method_body = method.GetMethodBody()
                if method_body is not None:
                    il_bytes = bytes(method_body.GetILAsByteArray())
                    if ILAnalyzer.contains_throw_not_implemented(il_bytes):
                        self.violations.append(StoryViolation(
                            type_name, method_name,
                            "Premature celebration - marked as complete but throws NotImplementedException",
                            StoryViolationType.PREMATURE_CELEBRATION
                        ))
        except:
            pass


def find_unity_assemblies(project_path: str) -> List[str]:
    """Find compiled Unity assemblies in Library/ScriptAssemblies"""
    assemblies = []
    
    # Check Library/ScriptAssemblies
    script_assemblies_path = Path(project_path) / "Library" / "ScriptAssemblies"
    if script_assemblies_path.exists():
        for dll in script_assemblies_path.glob("*.dll"):
            # Skip Unity's own assemblies
            dll_name = dll.name
            if not dll_name.startswith("Unity") and not dll_name.startswith("com.unity"):
                assemblies.append(str(dll))
    
    return assemblies


def main():
    parser = argparse.ArgumentParser(
        description="Story Test Framework - Standalone C# Validator"
    )
    parser.add_argument(
        "path",
        help="Path to Unity project root, assembly DLL, or directory of DLLs"
    )
    parser.add_argument(
        "-v", "--verbose",
        action="store_true",
        help="Enable verbose output"
    )
    parser.add_argument(
        "-o", "--output",
        help="Output file for JSON report (default: print to stdout)"
    )
    parser.add_argument(
        "--fail-on-violations",
        action="store_true",
        help="Exit with code 1 if violations found (useful for CI/CD)"
    )
    
    args = parser.parse_args()
    
    validator = StoryTestValidator(verbose=args.verbose)
    all_violations = []
    
    path = Path(args.path)
    
    # Determine what type of path we have
    if path.is_file() and path.suffix == ".dll":
        # Single assembly
        all_violations = validator.validate_assembly(str(path))
    elif path.is_dir():
        # Check if it's a Unity project
        if (path / "Assets").exists() and (path / "ProjectSettings").exists():
            print("Detected Unity project. Searching for compiled assemblies...")
            assemblies = find_unity_assemblies(str(path))
            
            if not assemblies:
                print("No assemblies found. Make sure the project is compiled.")
                print("In Unity: Menu > Assets > Reimport All")
                sys.exit(1)
            
            print(f"Found {len(assemblies)} assemblies to validate")
            for assembly in assemblies:
                violations = validator.validate_assembly(assembly)
                all_violations.extend(violations)
        else:
            # Directory of DLLs
            assemblies = list(path.glob("*.dll"))
            if not assemblies:
                print(f"No DLL files found in: {path}")
                sys.exit(1)
            
            for assembly in assemblies:
                violations = validator.validate_assembly(str(assembly))
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
    
    # Count by type
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
        print("STORY TEST VALIDATION REPORT")
        print("="*80)
        
        if all_violations:
            print(f"\n❌ Found {len(all_violations)} violation(s):\n")
            for violation in all_violations:
                print(f"  {violation}")
            
            print(f"\nViolations by type:")
            for v_type, count in report["violationsByType"].items():
                print(f"  - {v_type}: {count}")
        else:
            print("\n✅ No Story Test violations found! Code narrative is complete.")
        
        print("\n" + "="*80)
    
    # Exit with appropriate code
    if args.fail_on_violations and all_violations:
        sys.exit(1)
    else:
        sys.exit(0)


if __name__ == "__main__":
    main()
