using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace TinyWalnutGames.StoryTest.Shared
{
    /// <summary>
    /// Advanced IL analysis utilities for detecting incomplete/suspicious code patterns.
    /// These tools help achieve >95% detection of sneaky incomplete implementations.
    /// </summary>
    [StoryIgnore("Advanced IL analysis infrastructure for Story Test")]
    public static class AdvancedILAnalysis
    {
        #region IL Pattern Detection

        /// <summary>
        /// Detects if a method body is suspiciously simple (likely incomplete).
        /// Returns a confidence score (0.0 = definitely complete, 1.0 = definitely incomplete).
        /// </summary>
        public static float GetIncompletenessScore(MethodInfo method)
        {
            if (method == null) return 0f;

            var score = 0f;
            var body = method.GetMethodBody();
            if (body == null) return 0f; // Abstract/interface methods

            var ilBytes = body.GetILAsByteArray();
            if (ilBytes == null || ilBytes.Length == 0) return 0.8f; // Empty method body

            // Factor 1: Method length (very short = suspicious)
            if (ilBytes.Length <= 2) score += 0.4f;  // Just 'ret'
            else if (ilBytes.Length <= 5) score += 0.2f;  // Load constant + ret

            // Factor 2: Only returns default/null
            if (ReturnsOnlyDefaultValue(method, ilBytes)) score += 0.3f;

            // Factor 3: Contains no actual logic (no calls, no branches)
            if (HasNoLogic(ilBytes)) score += 0.2f;

            // Factor 4: Always returns same constant
            if (ReturnsConstantValue(ilBytes)) score += 0.15f;

            return Math.Min(1.0f, score);
        }

        /// <summary>
        /// Detects methods that return hardcoded constants (suspicious).
        /// Example: public int GetCount() => 0;  // Always returns 0
        /// </summary>
        public static bool ReturnsConstantValue(byte[] ilBytes)
        {
            if (ilBytes == null || ilBytes.Length < 2) return false;

            // Pattern: ldc.i4.0/1/2/... followed by ret
            // OpCodes: 0x16 (ldc.i4.0), 0x17 (ldc.i4.1), etc.
            for (var i = 0; i < ilBytes.Length - 1; i++)
            {
                if (ilBytes[i] >= 0x16 && ilBytes[i] <= 0x1E && ilBytes[i + 1] == 0x2A)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Detects methods with no actual logic (no method calls, no branches).
        /// </summary>
        public static bool HasNoLogic(byte[] ilBytes)
        {
            if (ilBytes == null || ilBytes.Length == 0) return true;

            // Check for presence of call/branch instructions
            for (var i = 0; i < ilBytes.Length; i++)
            {
                var opCode = ilBytes[i];

                // Method calls: 0x28 (call), 0x6F (callvirt), 0x29 (calli)
                if (opCode == 0x28 || opCode == 0x6F || opCode == 0x29)
                    return false;

                // Branch instructions: 0x38-0x45
                if (opCode >= 0x38 && opCode <= 0x45)
                    return false;

                // Switch: 0x45
                if (opCode == 0x45)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Detects methods that only return default(T) or null.
        /// </summary>
        public static bool ReturnsOnlyDefaultValue(MethodInfo method, byte[] ilBytes)
        {
            if (method.ReturnType == typeof(void)) return false;
            if (ilBytes == null || ilBytes.Length == 0) return true;

            // For very short methods, check if they just return a default value
            if (ilBytes.Length <= 8)
            {
                for (var i = 0; i < ilBytes.Length - 1; i++)
                {
                    // ldc.i4.0 (0x16), ldc.i4.1 (0x17), ldnull (0x14), followed by ret (0x2A)
                    if ((ilBytes[i] == 0x14 || ilBytes[i] == 0x16 || ilBytes[i] == 0x17) && ilBytes[i + 1] == 0x2A)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Detects empty method bodies (suspicious).
        /// </summary>
        public static bool IsEmptyMethod(MethodInfo method)
        {
            var body = method.GetMethodBody();
            if (body == null) return false; // Abstract methods are not "empty" in this sense

            var ilBytes = body.GetILAsByteArray();
            if (ilBytes == null || ilBytes.Length == 0) return true;

            // A method with only 'ret' (0x2A) is effectively empty
            if (ilBytes.Length == 1 && ilBytes[0] == 0x2A)
                return true;

            // nop followed by ret is also effectively empty
            if (ilBytes.Length == 2 && ilBytes[0] == 0x00 && ilBytes[1] == 0x2A)
                return true;

            return false;
        }

        #endregion

        #region Usage Analysis

        /// <summary>
        /// Attempts to determine if a field is ever READ (not just written).
        /// This helps detect "phantom fields" - fields that are set but never used.
        /// </summary>
        public static bool IsFieldRead(FieldInfo field, Type containingType)
        {
            if (field == null || containingType == null) return false;

            try
            {
                // Check all methods in the type for field reads
                var methods = containingType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                                       BindingFlags.Instance | BindingFlags.Static);

                foreach (var method in methods)
                {
                    var body = method.GetMethodBody();
                    if (body == null) continue;

                    var ilBytes = body.GetILAsByteArray();
                    if (ilBytes == null) continue;

                    // Look for ldfld (0x7B) or ldsfld (0x7E) opcodes
                    // These load field values onto the stack (reading)
                    for (var i = 0; i < ilBytes.Length - 4; i++)
                    {
                        if (ilBytes[i] == 0x7B || ilBytes[i] == 0x7E)
                        {
                            // This is a field load - could be our field
                            // Full verification would require parsing the metadata token
                            return true; // Conservative: assume it might be read
                        }
                    }
                }
            }
            catch
            {
                // On any error, assume it's being used (conservative approach)
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a property getter is ever called anywhere.
        /// </summary>
        public static bool IsPropertyGetterUsed(PropertyInfo property, Assembly assembly)
        {
            if (property?.GetMethod == null) return false;

            try
            {
                var getterToken = property.GetMethod.MetadataToken;

                // Search all methods in the assembly for calls to this getter
                var types = assembly.GetTypes();
                if (types.Select(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                                         BindingFlags.Instance | BindingFlags.Static)).Any(methods => (from method in methods where method != property.GetMethod select method.GetMethodBody() into body select body?.GetILAsByteArray() into ilBytes where ilBytes != null select ilBytes).Any(ilBytes => ContainsCallTo(ilBytes, getterToken))))
                {
                    return true;
                }
            }
            catch
            {
                return true; // Conservative: assume it's used
            }

            return false;
        }

        /// <summary>
        /// Checks if a method is ever called by any other method.
        /// </summary>
        public static bool IsMethodCalled(MethodInfo method, Assembly assembly)
        {
            if (method == null || assembly == null) return false;

            try
            {
                var methodToken = method.MetadataToken;

                var types = assembly.GetTypes();
                if (types.Select(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                                         BindingFlags.Instance | BindingFlags.Static)).Any(methods => (from caller in methods where caller != method select caller.GetMethodBody() into body where body != null select body.GetILAsByteArray() into ilBytes where ilBytes != null select ilBytes).Any(ilBytes => ContainsCallTo(ilBytes, methodToken))))
                {
                    return true;
                }
            }
            catch
            {
                return true; // Conservative
            }

            return false;
        }

        private static bool ContainsCallTo(byte[] ilBytes, int targetToken)
        {
            // Look for call (0x28) or callvirt (0x6F) followed by metadata token
            for (var i = 0; i < ilBytes.Length - 4; i++)
            {
                if (ilBytes[i] != 0x28 && ilBytes[i] != 0x6F) continue;
                // The next 4 bytes are the metadata token
                // Target token is encoded as big-endian uint32
                var token = BitConverter.ToUInt32(ilBytes, i + 1);
                if (token == targetToken) return true;
            }

            return false;
        }

        #endregion

        #region Complexity Analysis

        /// <summary>
        /// Calculates cyclomatic complexity from IL bytes.
        /// Low complexity with high line count = suspicious.
        /// </summary>
        public static int CalculateCyclomaticComplexity(byte[] ilBytes)
        {
            if (ilBytes == null || ilBytes.Length == 0) return 1;

            var branchCount = 0;

            for (var i = 0; i < ilBytes.Length; i++)
            {
                var opCode = ilBytes[i];

                // Branch instructions: br, brtrue, brfalse, beq, bne, etc.
                if (opCode >= 0x38 && opCode <= 0x45) branchCount++;

                // Switch
                if (opCode == 0x45) branchCount++;
            }

            // Cyclomatic complexity = branches + 1
            return branchCount + 1;
        }

        /// <summary>
        /// Estimates "implementation depth" - how much actual work a method does.
        /// </summary>
        public static int GetImplementationDepth(byte[] ilBytes)
        {
            if (ilBytes == null || ilBytes.Length == 0) return 0;

            var depth = 0;

            // Count meaningful operations
            for (var i = 0; i < ilBytes.Length; i++)
            {
                var opCode = ilBytes[i];

                // Method calls
                if (opCode == 0x28 || opCode == 0x6F || opCode == 0x29) depth += 2;

                // Field access
                if (opCode >= 0x7B && opCode <= 0x7E) depth += 1;

                // Array operations
                if (opCode >= 0x46 && opCode <= 0x65) depth += 1;

                // Arithmetic
                if (opCode >= 0x58 && opCode <= 0x65) depth += 1;
            }

            return depth;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Determines if a type should be skipped entirely from validation (test/compiler-generated/source-generated code).
        /// </summary>
        public static bool ShouldSkipType(Type type)
        {
            if (type == null) return true;

            var typeName = type.Name ?? string.Empty;
            var fullName = type.FullName ?? string.Empty;

            return IsCompilerGeneratedType(typeName, fullName) ||
                   IsStateMachineType(typeName, fullName) ||
                   IsDisplayClassType(typeName, fullName) ||
                   IsUnitySourceGeneratedType(typeName, fullName) ||
                   IsIteratorType(typeName, fullName) ||
                   HasCompilerGeneratedAttribute(type) ||
                   IsTestFixtureType(type);
        }

        private static bool IsCompilerGeneratedType(string typeName, string fullName)
        {
            // Skip compiler-generated types (lambdas, closures, state machines)
            // Check both name and fullName to catch nested compiler-generated types (Outer+<Inner>)
            return typeName.Contains("<") || typeName.Contains(">") || typeName.Contains("$") ||
                   fullName.Contains("<") || fullName.Contains(">");
        }

        private static bool IsStateMachineType(string typeName, string fullName)
        {
            // Skip async/iterator state machines (d__0, d__1, etc.)
            return typeName.Contains("d__") || fullName.Contains("d__");
        }

        private static bool IsDisplayClassType(string typeName, string fullName)
        {
            // Skip display classes (closures/lambdas)
            return typeName.Contains("DisplayClass") || fullName.Contains("DisplayClass") ||
                   typeName.Contains("<>c") || fullName.Contains("<>c");
        }

        private static bool IsUnitySourceGeneratedType(string typeName, string fullName)
        {
            // Skip Unity source-generated types (and their nested types)
            return typeName.StartsWith("UnitySourceGeneratedAssemblyMonoScriptTypes", StringComparison.Ordinal) ||
                   fullName.Contains("UnitySourceGeneratedAssemblyMonoScriptTypes");
        }

        private static bool IsIteratorType(string typeName, string fullName)
        {
            // Skip generic iterator helpers
            return typeName.Contains("Iterator") || fullName.Contains("Iterator");
        }

        private static bool HasCompilerGeneratedAttribute(Type type)
        {
            // Skip types with CompilerGenerated attribute
            return type.GetCustomAttributes(false).Any(a => a.GetType().Name.Contains("CompilerGenerated"));
        }

        private static bool IsTestFixtureType(Type type)
        {
            // Skip test fixtures (but not assemblies that contain "Test" in their name like "TheStoryTest")
            return !string.IsNullOrEmpty(type.Namespace) && 
                   (type.Namespace.EndsWith(".Tests") || type.Namespace.EndsWith(".Test"));
        }

        /// <summary>
        /// Determines if a member should be skipped (compiler or tool generated, or belongs to a skipped type).
        /// </summary>
        public static bool ShouldSkipMember(MemberInfo member)
        {
            if (member == null) return true;

            // If the declaring type is to be skipped, skip the member
            var declaring = (member as Type) ?? member.DeclaringType;
            if (ShouldSkipType(declaring)) return true;

            var name = member.Name ?? string.Empty;

            return IsCompilerGeneratedMemberName(name) || 
                   HasCompilerGeneratedMemberAttribute(member);
        }

        private static bool IsCompilerGeneratedMemberName(string name)
        {
            // Names that indicate compiler generated members (closures, lambdas, backing fields, local functions)
            if (name.Contains("<") || name.Contains(">") || name.Contains("$")) return true;
            if (name.Contains("b__")) return true; // lambda/closure methods
            if (name.Contains("k__BackingField")) return true; // auto-property backing field

            return false;
        }

        private static bool HasCompilerGeneratedMemberAttribute(MemberInfo member)
        {
            // Explicit attribute checks without taking a hard dependency on specific assemblies
            try
            {
                return member.GetCustomAttributes(false).Any(a =>
                {
                    var n = a.GetType().Name;
                    return n.Contains("CompilerGenerated") ||
                           n.Contains("AsyncStateMachine") ||
                           n.Contains("IteratorStateMachine");
                });
            }
            catch
            {
                // If reflection fails, be conservative and skip
                return true;
            }
        }

        /// <summary>
        /// Gets all non-obvious methods from a type (excludes constructors, getters/setters, etc.).
        /// </summary>
        public static IEnumerable<MethodInfo> GetBusinessLogicMethods(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                         BindingFlags.Instance | BindingFlags.Static);

            foreach (var method in methods)
            {
                // Skip special methods
                if (method.IsSpecialName) continue; // Skips property accessors, operators, etc.
                if (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")) continue;
                if (method.IsConstructor) continue;

                // Skip compiler-generated methods
                if (method.GetCustomAttributes(false).Any(a => a.GetType().Name.Contains("CompilerGenerated")))
                    continue;

                yield return method;
            }
        }

        /// <summary>
        /// Checks if a method is likely an event handler or callback (different validation rules).
        /// </summary>
        public static bool IsLikelyEventHandler(MethodInfo method)
        {
            if (method == null) return false;

            // Common event handler patterns
            var name = method.Name;
            if (name.StartsWith("On") || name.EndsWith("Handler") || name.EndsWith("Callback"))
                return true;

            // Check parameters
            var parameters = method.GetParameters();
            if (parameters.Length == 2 && parameters[1].ParameterType.Name.EndsWith("EventArgs"))
                return true;

            return false;
        }

        #endregion
    }
}
