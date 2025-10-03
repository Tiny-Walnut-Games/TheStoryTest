using System;
using System.Reflection;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Acts
{
    /// <summary>
    /// Story Test Act 5: Checks for debug-only implementations.
    /// This act ensures debug/test methods are properly marked as temporary for production builds.
    /// </summary>
    [StoryIgnore("Story test validation infrastructure")]
    public static class Act5DebugOnlyImplementations
    {
        /// <summary>
        /// The validation rule for this act.
        /// </summary>
    public static readonly ValidationRule Rule = CheckForDebugOnlyImplementations;

        /// <summary>
        /// Checks for debug-only implementations that shouldn't be in production.
        /// </summary>
        private static bool CheckForDebugOnlyImplementations(MemberInfo member, out string violation)
        {
            violation = null;

            if (member.Name.StartsWith("Debug") || member.Name.StartsWith("Test") || member.Name.Contains("Temp"))
            {
                var obsoleteAttr = member.GetCustomAttribute<ObsoleteAttribute>();
                if (obsoleteAttr == null)
                {
                    violation = "Debug/Test method without Obsolete attribute (should be temporary)";
                    return true;
                }
            }

            return false;
        }
    }
}