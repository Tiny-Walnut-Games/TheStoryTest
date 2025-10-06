using System.Linq;
using System.Reflection;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Acts
{
    /// <summary>
    /// Story Test Act 11: Detects dead code - fields, properties, and methods that are never used.
    /// This is the "phantom detection" act - finds code that exists but serves no purpose.
    ///
    /// Examples caught:
    /// - Fields that are set but never read
    /// - Properties with getters that are never called
    /// - Methods that are defined but never invoked
    /// </summary>
    [StoryIgnore("Story test validation infrastructure")]
    public static class Act11DeadCode
    {
        /// <summary>
        /// The validation rule for this act.
        /// </summary>
        public static readonly ValidationRule Rule = CheckForDeadCode;

        private static bool CheckForDeadCode(MemberInfo member, out string violation)
        {
            violation = null;

            // Skip compiler-generated members
            if (member.GetCustomAttributes(false).Any(a => a.GetType().Name.Contains("CompilerGenerated")))
                return false;

            // Skip members with StoryIgnore
            if (member.GetCustomAttributes(typeof(StoryIgnoreAttribute), true).Length > 0)
                return false;

            try
            {
                // Check fields
                if (member is FieldInfo field)
                {
                    // Skip public fields (might be used by serialization/Unity)
                    if (field.IsPublic) return false;

                    // Skip const/readonly fields (configuration values)
                    if (field.IsLiteral || field.IsInitOnly) return false;

                    // Check if field is read anywhere
                    if (!AdvancedILAnalysis.IsFieldRead(field, field.DeclaringType))
                    {
                        violation = $"Field '{field.Name}' is never read - consider removing dead code";
                        return true;
                    }
                }

                // Check properties
                if (member is PropertyInfo property)
                {
                    // Skip properties without getters (write-only might be intentional)
                    if (property.GetMethod == null) return false;

                    // Skip public properties (might be used externally/serialization)
                    if (property.GetMethod.IsPublic) return false;

                    // Check if getter is ever called
                    var assembly = property.DeclaringType?.Assembly;
                    if (assembly != null && !AdvancedILAnalysis.IsPropertyGetterUsed(property, assembly))
                    {
                        violation = $"Property '{property.Name}' getter is never called - consider removing phantom property";
                        return true;
                    }
                }

                // Check methods
                if (member is MethodInfo method)
                {
                    // Skip special methods
                    if (method.IsSpecialName) return false;
                    if (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")) return false;

                    // Skip public methods (might be API surface)
                    if (method.IsPublic) return false;

                    // Skip virtual/override methods (part of inheritance contract)
                    if (method.IsVirtual || method.GetBaseDefinition() != method) return false;

                    // Skip entry points and Unity lifecycle methods
                    if (IsEntryPointOrLifecycleMethod(method)) return false;

                    // Check if method is ever called
                    var assembly = method.DeclaringType?.Assembly;
                    if (assembly != null && !AdvancedILAnalysis.IsMethodCalled(method, assembly))
                    {
                        violation = $"Method '{method.Name}' is never called - consider removing cold method";
                        return true;
                    }
                }
            }
            catch
            {
                // If analysis fails, don't report violation (conservative approach)
                return false;
            }

            return false;
        }

        private static bool IsEntryPointOrLifecycleMethod(MethodInfo method)
        {
            // Unity lifecycle methods
            var unityLifecycleMethods = new[]
            {
                "Awake", "Start", "Update", "FixedUpdate", "LateUpdate",
                "OnEnable", "OnDisable", "OnDestroy", "OnApplicationQuit",
                "OnGUI", "OnDrawGizmos", "OnDrawGizmosSelected",
                "OnCollisionEnter", "OnCollisionExit", "OnTriggerEnter", "OnTriggerExit"
            };

            if (unityLifecycleMethods.Contains(method.Name))
                return true;

            // Event handlers
            if (AdvancedILAnalysis.IsLikelyEventHandler(method))
                return true;

            // Main entry point
            if (method.Name == "Main" && method.IsStatic)
                return true;

            return false;
        }
    }
}
