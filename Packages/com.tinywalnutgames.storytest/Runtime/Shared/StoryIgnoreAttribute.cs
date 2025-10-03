using System;

namespace TinyWalnutGames.StoryTest.Shared
{
    /// <summary>
    /// Marks code elements that should be ignored by story test validation.
    /// Use sparingly and only when absolutely necessary for technical infrastructure.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property |
                    AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Enum)]
    public sealed class StoryIgnoreAttribute : Attribute
    {
        /// <summary>
        /// Reason why this element is ignored by story tests.
        /// Must provide justification for exclusion from narrative completeness.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Creates a story ignore attribute with required justification.
        /// </summary>
        /// <param name="reason">The reason for excluding this element from story validation</param>
        public StoryIgnoreAttribute(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Story ignore reason cannot be empty", nameof(reason));

            Reason = reason;
        }
    }
}
