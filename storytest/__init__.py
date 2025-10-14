"""
The Story Test Framework - Python Package

A comprehensive code quality validation framework for Unity and .NET projects
that enforces "Story Test Doctrine": every symbol must be fully implemented
and meaningful—no placeholders, TODOs, or unused code in production.

Usage:
    # Command line
    storytest validate ./path/to/project
    
    # Python API
    from storytest import StoryTestValidator
    validator = StoryTestValidator()
    violations = validator.validate_assembly("MyAssembly.dll")
"""

__version__ = "1.2.0"
__author__ = "Tiny Walnut Games"
__email__ = "jmeyer1980@gmail.com"
__license__ = "MIT"

from .validator import (
    StoryTestValidator,
    StoryViolation,
    StoryViolationType,
)

__all__ = [
    "StoryTestValidator",
    "StoryViolation",
    "StoryViolationType",
    "__version__",
]