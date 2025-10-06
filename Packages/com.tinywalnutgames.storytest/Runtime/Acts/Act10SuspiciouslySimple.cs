using System.Reflection;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Acts
{
    /// <summary>
    /// Story Test Act 10: Detects suspiciously simple implementations.
    /// This catches the "sneaky bits" - code that exists but doesn't do enough.
    ///
    /// Examples caught:
    /// - Methods that only return constants
    /// - Methods with no actual logic
    /// - Implementations that are technically "complete" but trivial
    /// </summary>
    [StoryIgnore("Story test validation infrastructure")]
    public static class Act10SuspiciouslySimple
    {
        /// <summary>
        /// The validation rule for this act.
        /// </summary>
        public static readonly ValidationRule Rule = CheckForSuspiciousSimplicity;

        private const float SUSPICIOUS_THRESHOLD = 0.6f; // 60% incompleteness score triggers warning

        private static bool CheckForSuspiciousSimplicity(MemberInfo member, out string violation)
        {
            violation = null;

            if (member is MethodInfo method)
            {
                // Skip special methods (constructors, property accessors, event handlers)
                if (method.IsSpecialName) return false;
                if (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")) return false;
                if (AdvancedILAnalysis.IsLikelyEventHandler(method)) return false;

                // Skip methods with StoryIgnore
                if (method.GetCustomAttributes(typeof(StoryIgnoreAttribute), true).Length > 0)
                    return false;

                try
                {
                    // Calculate incompleteness score
                    var score = AdvancedILAnalysis.GetIncompletenessScore(method);

                    if (score >= SUSPICIOUS_THRESHOLD)
                    {
                        var body = method.GetMethodBody();
                        var ilBytes = body?.GetILAsByteArray();

                        // Provide specific details about what's suspicious
                        if (AdvancedILAnalysis.IsEmptyMethod(method))
                        {
                            violation = $"Method '{method.Name}' is empty - likely incomplete (incompleteness: {score:P0})";
                            return true;
                        }

                        if (AdvancedILAnalysis.ReturnsConstantValue(ilBytes))
                        {
                            violation = $"Method '{method.Name}' only returns a constant value - likely placeholder (incompleteness: {score:P0})";
                            return true;
                        }

                        if (AdvancedILAnalysis.HasNoLogic(ilBytes))
                        {
                            violation = $"Method '{method.Name}' contains no actual logic - likely incomplete (incompleteness: {score:P0})";
                            return true;
                        }

                        violation = $"Method '{method.Name}' is suspiciously simple - may be incomplete (incompleteness: {score:P0})";
                        return true;
                    }
                }
                catch
                {
                    // If analysis fails, don't report a violation
                    return false;
                }
            }

            return false;
        }
    }
}
