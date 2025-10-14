#!/usr/bin/env python3
"""
Story Test Framework - Core Validator
Analyzes C# assemblies for Story Test violations without Unity dependencies.
"""

import os
from pathlib import Path
from typing import List, Dict, Any
from enum import Enum

try:
    from pythonnet import set_runtime
    from clr_loader import get_coreclr

    rt = get_coreclr()
    set_runtime(rt)

    import clr
    clr.AddReference("System.Reflection")
    from System.Reflection import Assembly, BindingFlags
except ImportError as e:
    raise ImportError(
        f"pythonnet not installed: {e}\n"
        "Install with: pip install storytest"
    )


class StoryViolationType(Enum):
    """Types of Story Test violations"""
    INCOMPLETE_IMPLEMENTATION = "IncompleteImplementation"
    DEBUGGING_CODE = "DebuggingCode"
    UNUSED_CODE = "UnusedCode"
    PREMATURE_CELEBRATION = "PrematureCelebration"
    OTHER = "Other"


class StoryViolation:
    """Represents a single Story Test violation"""
    
    def __init__(
        self,
        type_name: str,
        member: str,
        violation: str,
        violation_type: StoryViolationType,
        file_path: str = "",
        line_number: int = 0
    ):
        self.type_name = type_name
        self.member = member
        self.violation = violation
        self.violation_type = violation_type
        self.file_path = file_path
        self.line_number = line_number

    def to_dict(self) -> Dict[str, Any]:
        """Convert violation to dictionary format"""
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

    def __repr__(self) -> str:
        return f"StoryViolation({self.type_name}.{self.member}, {self.violation_type.value})"


class StoryTestValidator:
    """
    Unity-safe validator that skips Unity-dependent assemblies.
    
    Example:
        validator = StoryTestValidator(verbose=True)
        violations = validator.validate_assembly("MyAssembly.dll")
        for violation in violations:
            print(violation)
    """

    def __init__(self, verbose: bool = False):
        self.verbose = verbose
        self.violations: List[StoryViolation] = []

    def log(self, message: str):
        """Log message if verbose mode is enabled"""
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

            return len(core_unity_refs) > 0
        except:
            # If we can't check, try to load it normally
            try:
                assembly = Assembly.LoadFrom(assembly_path)
                types = assembly.GetExportedTypes()
                if len(types) > 0:
                    str(types[0].FullName)
                    return False
                return False
            except Exception as e:
                error_str = str(e).lower()
                return "unityengine" in error_str or "coremodule" in error_str

    def validate_assembly(self, assembly_path: str) -> List[StoryViolation]:
        """
        Validate assembly only if it's not Unity-dependent.
        
        Args:
            assembly_path: Path to the .NET assembly DLL
            
        Returns:
            List of StoryViolation objects found in the assembly
        """
        if not os.path.exists(assembly_path):
            self.log(f"Error: Assembly not found: {assembly_path}")
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
            self.log(f"Error loading assembly: {e}")
            return self.violations

    def _validate_type(self, type_obj):
        """Basic validation without Unity dependencies"""
        type_name = str(type_obj.Name)

        # Skip compiler-generated and system types
        if type_name.startswith("<") or type_name.startswith("__"):
            return

        file_path = str(type_obj.Assembly.Location) if hasattr(type_obj, 'Assembly') else ""

        # Validate methods
        flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static
        for method in type_obj.GetMethods(flags):
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


def find_assemblies(project_path: str) -> List[str]:
    """
    Find .NET assemblies in a project directory.
    
    Args:
        project_path: Path to project root or Unity project
        
    Returns:
        List of assembly file paths
    """
    assemblies = []
    project_root = Path(project_path)

    # Look for Unity ScriptAssemblies
    script_assemblies_path = project_root / "Library" / "ScriptAssemblies"
    if script_assemblies_path.exists():
        for dll in script_assemblies_path.glob("*.dll"):
            dll_name = dll.name
            # Skip Unity's own assemblies
            if not (dll_name.startswith("Unity") or
                   dll_name.startswith("com.unity") or
                   dll_name.startswith("Assembly-CSharp") or
                   "Unity" in dll_name):
                assemblies.append(str(dll))

    # Look for bin/Debug or bin/Release folders
    for bin_path in [project_root / "bin" / "Debug", project_root / "bin" / "Release"]:
        if bin_path.exists():
            for dll in bin_path.glob("*.dll"):
                assemblies.append(str(dll))

    return assemblies