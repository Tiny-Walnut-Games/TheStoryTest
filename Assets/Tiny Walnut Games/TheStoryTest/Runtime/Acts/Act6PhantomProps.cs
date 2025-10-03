using System.Reflection;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Acts
{
    /// <summary>
    /// Story Test Act 6: Checks for "Phantom Props" - properties that are defined but never used meaningfully.
    /// Enhanced from original TinyWalnutGames implementation.
    /// This act identifies properties that exist but serve no real business purpose.
    /// </summary>
    [StoryIgnore("Story test validation infrastructure")]
    public static class Act6PhantomProps
    {
        /// <summary>
        /// The validation rule for this act.
        /// </summary>
    public static readonly ValidationRule Rule = CheckForPhantomProps;

        /// <summary>
        /// Checks for "Phantom Props" - properties that are defined but never actually used meaningfully.
        /// Enhanced from original TinyWalnutGames implementation.
        /// </summary>
        private static bool CheckForPhantomProps(MemberInfo member, out string violation)
        {
            violation = null;

            if (member is PropertyInfo property)
            {
                // Check if property has both getter and setter but no real logic
                var getter = property.GetGetMethod();
                var setter = property.GetSetMethod();

                if (getter != null && setter != null)
                {
                    try
                    {
                        var getterBody = getter.GetMethodBody();
                        var setterBody = setter.GetMethodBody();

                        if (getterBody != null && setterBody != null)
                        {
                            var getterIL = getterBody.GetILAsByteArray();
                            var setterIL = setterBody.GetILAsByteArray();

                            // Very basic auto-property pattern detection
                            if (getterIL.Length <= 10 && setterIL.Length <= 10)
                            {
                                // This might be an auto-property without business logic
                                // Check if it's used anywhere meaningful (simplified check)
                                if (property.Name.Contains("Unused") || property.Name.Contains("Temp"))
                                {
                                    violation = "Phantom property detected - defined but likely unused";
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
            }

            return false;
        }
    }
}