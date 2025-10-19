"""
The Story Test Framework - Python Package

A comprehensive code quality validation framework for Unity and .NET projects
that enforces "Story Test Doctrine": every symbol must be fully implemented
and meaningful—no placeholders, TODOs, or unused code in production.

Usage:
    # Command line (C#/.NET assemblies)
    storytest validate ./path/to/assemblies

    # Command line (Python projects)
    storytest validate-py ./path/to/python/project
    
    # Python API (C#/.NET)
    from storytest import StoryTestValidator
    validator = StoryTestValidator()
    violations = validator.validate_assembly("MyAssembly.dll")

    # Python API (Python source)
    from storytest import validate_python_path
    py_violations = validate_python_path("./src")
"""

__version__ = "1.3.0"
__author__ = "Tiny Walnut Games"
__email__ = "jmeyer1980@gmail.com"
__license__ = "MIT"

from .validator import (
    StoryTestValidator,
    StoryViolation,
    StoryViolationType,
)
from .python_validator import validate_python_path

__all__ = [
    "StoryTestValidator",
    "StoryViolation",
    "StoryViolationType",
    "validate_python_path",
    "__version__",
]