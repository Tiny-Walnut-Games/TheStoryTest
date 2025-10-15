"""
Quick test to verify that IEnumerator.Reset is properly filtered as a compiler-generated member.
"""

# Test the member name filtering logic
def test_is_compiler_generated_member_name(name):
    """Simulates the IsCompilerGeneratedMemberName logic"""
    # Names that indicate compiler generated members
    if "<" in name or ">" in name or "$" in name:
        return True
    if "b__" in name:  # lambda/closure methods
        return True
    if "k__BackingField" in name:  # auto-property backing field
        return True
    
    # Explicit interface implementations (e.g., "System.Collections.IEnumerator.Reset")
    # These often appear in compiler-generated iterator/async state machines
    if "." in name and ("IEnumerator" in name or "IAsyncStateMachine" in name):
        return True
    
    return False

# Test cases
test_cases = [
    ("System.Collections.IEnumerator.Reset", True, "IEnumerator.Reset should be skipped"),
    ("System.Collections.IEnumerator.MoveNext", True, "IEnumerator.MoveNext should be skipped"),
    ("System.Collections.Generic.IEnumerator<T>.get_Current", True, "IEnumerator.get_Current should be skipped"),
    ("MyMethod", False, "Regular method should not be skipped"),
    ("<GetBusinessLogicMethods>d__13", True, "Iterator state machine should be skipped"),
    ("b__0", True, "Lambda method should be skipped"),
    ("<>k__BackingField", True, "Backing field should be skipped"),
]

print("Testing compiler-generated member name detection:")
print("=" * 70)

all_passed = True
for name, expected, description in test_cases:
    result = test_is_compiler_generated_member_name(name)
    status = "✅ PASS" if result == expected else "❌ FAIL"
    if result != expected:
        all_passed = False
    print(f"{status}: {description}")
    print(f"  Name: {name}")
    print(f"  Expected: {expected}, Got: {result}")
    print()

print("=" * 70)
if all_passed:
    print("✅ All tests passed!")
else:
    print("❌ Some tests failed!")