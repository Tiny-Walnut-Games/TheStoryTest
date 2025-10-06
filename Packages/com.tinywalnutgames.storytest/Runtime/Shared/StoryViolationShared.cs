namespace TinyWalnutGames.StoryTest.Shared
{
    /// <summary>
    /// Types and utilities shared between StoryTest and StoryTest.Acts.
    /// </summary>
    [StoryIgnore("Data transfer object for violation reporting - auto-properties set by validation code")]
    public class StoryViolation
    {
        public string Type { get; set; }
        public string Member { get; set; }
        public string Violation { get; set; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        
        /// <summary>
        /// Category of the violation. Defaults to Other.
        /// </summary>
        public StoryViolationType ViolationType { get; set; }

        public StoryViolation()
        {
            // Ensure sensible defaults expected by tests/reporting
            ViolationType = StoryViolationType.Other;
            FilePath = FilePath ?? string.Empty;
            Member = Member ?? string.Empty;
            Type = Type ?? string.Empty;
            Violation = Violation ?? string.Empty;
        }

        public override string ToString()
        {
            // Produce an actionable, human-friendly string with file and line when available
            var typeName = string.IsNullOrWhiteSpace(Type) ? "<UnknownType>" : Type;
            var memberName = string.IsNullOrWhiteSpace(Member) ? "<UnknownMember>" : Member;
            var message = string.IsNullOrWhiteSpace(Violation) ? "<No details provided>" : Violation;
            var where = string.Empty;
            if (!string.IsNullOrWhiteSpace(FilePath) && LineNumber > 0)
            {
                where = $" ({FilePath}:{LineNumber})";
            }
            else if (!string.IsNullOrWhiteSpace(FilePath))
            {
                where = $" ({FilePath})";
            }

            return $"{ViolationType}: {typeName}.{memberName} - {message}{where}";
        }
    }

    [StoryIgnore("Enumeration for categorizing violation types in reporting")]
    public enum StoryViolationType
    {
        IncompleteImplementation,
        DebuggingCode,
        NamingConvention,
        UnusedCode,
        PrematureCelebration,
        Other,
        PlaceholderCode
    }
}
