using System;
using System.Linq;
using System.Reflection;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Acts
{
    /// <summary>
    /// Story Test Act 8: Checks for "Hollow Enums" - enums with no meaningful values or implementations.
    /// Enhanced from original TinyWalnutGames implementation.
    /// This act identifies enums that are declared but not properly populated.
    /// </summary>
    [StoryIgnore("Story test validation infrastructure")]
    public static class Act8HollowEnums
    {
        /// <summary>
        /// The validation rule for this act.
        /// </summary>
    public static readonly ValidationRule Rule = CheckForHollowEnums;

        /// <summary>
        /// Checks for "Hollow Enums" - enums with no meaningful values or implementations.
        /// Enhanced from original TinyWalnutGames implementation.
        /// </summary>
        private static bool CheckForHollowEnums(MemberInfo member, out string violation)
        {
            violation = null;

            if (member is Type type && type.IsEnum)
            {
                var enumValues = Enum.GetValues(type);
                if (enumValues.Length <= 1)
                {
                    violation = "Hollow enum detected - enum has no or minimal values defined";
                    return true;
                }

                // Check for 🏳placeholder enum values
                var enumNames = Enum.GetNames(type);
                if (enumNames.Any(name => name.Contains("Placeholder") || name.Contains("TODO") || name.Contains("Temp")))
                {
                    violation = "Hollow enum detected - contains 🏳placeholder values";
                    return true;
                }
            }

            return false;
        }
    }
}