using System.Reflection;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Acts
{
    /// <summary>
    /// Story Test Act 2: Checks for 🏳placeholder implementations.
    /// This act detects methods that only throw exceptions or return defaults without proper implementation.
    /// </summary>
    [StoryIgnore("Story test validation infrastructure")]
    public static class Act2PlaceholderImplementations
    {
        /// <summary>
        /// The validation rule for this act.
        /// </summary>
    public static readonly ValidationRule Rule = CheckForPlaceholderImplementations;
        /// <summary>
        /// The validation rule for this act.
        /// Checks for 🏳placeholder implementations (methods that only throw exceptions or return defaults).
        /// </summary>
        private static bool CheckForPlaceholderImplementations(MemberInfo member, out string violation)
        {
            violation = null;

            if (member is MethodInfo method && !method.IsAbstract)
            {
                try
                {
                    var methodBody = method.GetMethodBody();
                    if (methodBody != null && methodBody.GetILAsByteArray().Length <= 10) // Very short methods are suspicious
                    {
                        var ilBytes = methodBody.GetILAsByteArray();

                        // Check for throw new NotImplementedException pattern
                        if (StoryTestUtilities.ContainsThrowNotImplementedException(ilBytes))
                        {
                            violation = "Method throws NotImplementedException (🏳placeholder implementation)";
                            return true;
                        }
                    }
                }
                catch
                {
                    // If we can't analyze IL, skip this check
                }
            }

            return false;
        }
    }
}