using System;
using System.Linq;
using System.Reflection;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Acts
{
    /// <summary>
    /// Story Test Act 3: Checks for incomplete class implementations.
    /// This act ensures that non-abstract classes don't have unimplemented abstract methods.
    /// </summary>
    [StoryIgnore("Story test validation infrastructure")]
    public static class Act3IncompleteClasses
    {
        /// <summary>
        /// The validation rule for this act.
        /// </summary>
    public static readonly ValidationRule Rule = CheckForIncompleteClasses;

        /// <summary>
        /// The validation rule for this act.
        /// Checks for incomplete class implementations.
        /// </summary>
        private static bool CheckForIncompleteClasses(MemberInfo member, out string violation)
        {
            violation = null;

            if (member is Type type && !type.IsInterface && !type.IsAbstract)
            {
                var abstractMethods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                         .Where(m => m.IsAbstract).ToArray();

                if (abstractMethods.Length > 0)
                {
                    violation = $"Class has unimplemented abstract methods: {string.Join(", ", abstractMethods.Select(m => m.Name))}";
                    return true;
                }
            }

            return false;
        }
    }
}