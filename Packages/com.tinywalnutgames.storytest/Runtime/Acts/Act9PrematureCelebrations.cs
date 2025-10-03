using System.Linq;
using System.Reflection;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Acts
{
    /// <summary>
    /// Story Test Act 9: Checks for "Premature Celebrations" - code that claims to be complete but isn't.
    /// Enhanced from original TinyWalnutGames implementation.
    /// This act identifies code marked as complete but still containing 🏳placeholder implementations.
    /// </summary>
    [StoryIgnore("Story test validation infrastructure")]
    public static class Act9PrematureCelebrations
    {
        /// <summary>
        /// The validation rule for this act.
        /// </summary>
    public static readonly ValidationRule Rule = CheckForPrematureCelebrations;

        /// <summary>
        /// Checks for "Premature Celebrations" - code that claims to be complete but isn't.
        /// Enhanced from original TinyWalnutGames implementation.
        /// </summary>
        private static bool CheckForPrematureCelebrations(MemberInfo member, out string violation)
        {
            violation = null;

            // Check for attributes or comments that claim completion
            var attributes = member.GetCustomAttributes().ToArray();

            foreach (var attr in attributes)
            {
                var attrName = attr.GetType().Name;
                if (attrName.Contains("Complete") || attrName.Contains("Finished") || attrName.Contains("Done"))
                {
                    // If it's marked as complete, do additional validation
                    if (member is MethodInfo method)
                    {
                        try
                        {
                            var methodBody = method.GetMethodBody();
                            if (methodBody != null)
                            {
                                var ilBytes = methodBody.GetILAsByteArray();
                                if (StoryTestUtilities.ContainsThrowNotImplementedException(ilBytes))
                                {
                                    violation = "Premature celebration - marked as complete but throws NotImplementedException";
                                    return true;
                                }
                            }
                        }
                        catch
                        {
                            // If IL analysis fails, skip this check
                        }
                    }
                }
            }

            return false;
        }
    }
}