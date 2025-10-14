#!/usr/bin/env python3
"""Quick test to verify the storytest package works"""

print("Testing Story Test Package...")
print("=" * 60)

# Test imports
print("\n1. Testing imports...")
try:
    from storytest import __version__, StoryTestValidator, StoryViolationType, StoryViolation
    print(f"   ✓ Package version: {__version__}")
    print(f"   ✓ StoryTestValidator imported")
    print(f"   ✓ StoryViolationType imported")
    print(f"   ✓ StoryViolation imported")
except ImportError as e:
    print(f"   ✗ Import failed: {e}")
    exit(1)

# Test validator creation
print("\n2. Testing validator creation...")
try:
    validator = StoryTestValidator(verbose=False)
    print(f"   ✓ Validator created: {validator}")
except Exception as e:
    print(f"   ✗ Validator creation failed: {e}")
    exit(1)

# Test violation types
print("\n3. Testing violation types...")
try:
    for vtype in StoryViolationType:
        print(f"   ✓ {vtype.name}: {vtype.value}")
except Exception as e:
    print(f"   ✗ Violation types failed: {e}")
    exit(1)

# Test violation creation
print("\n4. Testing violation creation...")
try:
    violation = StoryViolation(
        type_name="TestClass",
        member="TestMethod",
        violation="Test violation",
        violation_type=StoryViolationType.INCOMPLETE_IMPLEMENTATION,
        file_path="test.cs",
        line_number=42
    )
    print(f"   ✓ Violation created: {violation}")
    print(f"   ✓ Violation dict: {violation.to_dict()}")
except Exception as e:
    print(f"   ✗ Violation creation failed: {e}")
    exit(1)

print("\n" + "=" * 60)
print("✅ All tests passed! Package is working correctly.")
print("\nNext steps:")
print("  • Install locally: pip install -e .")
print("  • Test CLI: storytest version")
print("  • Build package: .\\scripts\\build_pypi.ps1")