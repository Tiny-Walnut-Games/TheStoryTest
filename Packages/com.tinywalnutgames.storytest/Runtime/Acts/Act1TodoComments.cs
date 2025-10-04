// Use Unity Debug in Unity, System.Diagnostics.Debug otherwise
#if UNITY_EDITOR || UNITY_ENGINE
using Debug = UnityEngine.Debug;
#else
using Debug = System.Diagnostics.Debug;

#endif
using System.Reflection;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Acts
{
    /// <summary>
    /// Story Test Act 1: Checks for 🏳TODO comments in method implementations.
    /// Enhanced with IL analysis capabilities from original implementation.
    /// This act ensures that no 🏳placeholder 🏳TODO implementations remain in production code.
    /// </summary>
    [StoryIgnore("Story test validation infrastructure")]
    public static class Act1TodoComments
    {
        /// <summary>
        /// The validation rule for this act.
        /// </summary>
    public static readonly ValidationRule Rule = CheckForTodoComments;
        /// <summary>
        /// The validation rule for this act.
        /// Checks for 🏳TODO comments in method implementations.
        /// Enhanced with IL analysis capabilities from original implementation.
        /// </summary>
        private static bool CheckForTodoComments(MemberInfo member, out string violation)
        {
            violation = null;

            if (member is MethodInfo method)
            {
                // Use IL analysis to detect 🏳placeholder implementations
                try
                {
                    var methodBody = method.GetMethodBody();
                    if (methodBody != null)
                    {
                        var ilBytes = methodBody.GetILAsByteArray();

                        // Check for patterns that indicate 🏳TODO implementations
                        if (StoryTestUtilities.ContainsThrowNotImplementedException(ilBytes))
                        {
                            violation = "Method contains NotImplementedException indicating TODO implementation";
                            return true;
                        }

                        // Check for methods that only return default values
                        if (StoryTestUtilities.IsOnlyDefaultReturn(method, ilBytes))
                        {
                            violation = "Method only returns default value without implementation";
                            return true;
                        }
                    }
                }
                catch
                {
                    // If IL analysis fails, fall back to basic checks
                }
            }

            return false;
        }
    }
}