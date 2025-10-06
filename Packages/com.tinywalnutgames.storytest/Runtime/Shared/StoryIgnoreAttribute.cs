using System;

namespace TinyWalnutGames.StoryTest.Shared
{
    /// <summary>
    /// Marks code elements that should be ignored by story test validation.
    /// Use sparingly and only when necessary for technical infrastructure.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property |
                    AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Enum)]
    [StoryIgnore("Story Test framework attribute infrastructure - cannot self-validate")]
    public sealed class StoryIgnoreAttribute : Attribute
    {
        /// <summary>
        /// Reason why this element is ignored by story tests.
        /// Must provide justification for exclusion from narrative completeness.
        /// </summary>
        private string Reason { get; }

        /// <summary>
        /// Creates a story ignore attribute with required justification.
        /// </summary>
        /// <param name="reason">The reason for excluding this element from story validation</param>
        /// <exception cref="ArgumentException">Thrown when reason is null, empty or whitespace</exception>
        public StoryIgnoreAttribute(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Reason must not be null, empty or whitespace", nameof(reason));

            Reason = reason;
        }
    }
}
