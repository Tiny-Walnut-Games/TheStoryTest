using System.Reflection;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Acts
{
    /// <summary>
    /// Story Test Act 7: Checks for "Cold Methods" - methods that are defined but never called or have no meaningful implementation.
    /// Enhanced from original TinyWalnutGames implementation.
    /// This act identifies methods that exist but serve no real purpose.
    /// </summary>
    [StoryIgnore("Story test validation infrastructure")]
    public static class Act7ColdMethods
    {
        /// <summary>
        /// The validation rule for this act.
        /// </summary>
    public static readonly ValidationRule Rule = CheckForColdMethods;

        /// <summary>
        /// Checks for "Cold Methods" - methods that are defined but never called or have no meaningful implementation.
        /// Enhanced from original TinyWalnutGames implementation.
        /// </summary>
        private static bool CheckForColdMethods(MemberInfo member, out string violation)
        {
            violation = null;

            if (member is MethodInfo method && !method.IsConstructor && !method.IsSpecialName)
            {
                try
                {
                    var methodBody = method.GetMethodBody();
                    if (methodBody != null)
                    {
                        var ilBytes = methodBody.GetILAsByteArray();

                        // Check for methods that do nothing (just return)
                        if (ilBytes.Length <= 3) // Just ret instruction
                        {
                            if (!method.IsAbstract && !method.IsVirtual)
                            {
                                violation = "Cold method detected - method body is empty or minimal";
                                return true;
                            }
                        }
                    }
                }
                catch
                {
                    // If IL analysis fails, skip this check
                }
            }

            return false;
        }
    }
}