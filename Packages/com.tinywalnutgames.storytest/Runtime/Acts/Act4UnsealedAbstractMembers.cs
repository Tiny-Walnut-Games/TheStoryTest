using System.Reflection;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Acts
{
    /// <summary>
    /// Story Test Act 4: Checks for unsealed abstract members.
    /// This act identifies abstract methods in non-abstract classes which violates narrative completion.
    /// </summary>
    [StoryIgnore("Story test validation infrastructure")]
    public static class Act4UnsealedAbstractMembers
    {
        /// <summary>
        /// The validation rule for this act.
        /// </summary>
    public static readonly ValidationRule Rule = CheckForUnsealedAbstractMembers;

        /// <summary>
        /// Checks for unsealed abstract members that should be implemented.
        /// </summary>
        private static bool CheckForUnsealedAbstractMembers(MemberInfo member, out string violation)
        {
            violation = null;

            if (member is MethodInfo method && method.IsAbstract && method.DeclaringType != null)
            {
                if (!method.DeclaringType.IsAbstract && !method.DeclaringType.IsInterface)
                {
                    violation = "Abstract method in non-abstract class (unsealed narrative element)";
                    return true;
                }
            }

            return false;
        }
    }
}