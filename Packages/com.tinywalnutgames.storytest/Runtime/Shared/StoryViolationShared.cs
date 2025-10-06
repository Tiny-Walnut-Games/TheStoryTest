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
        public StoryViolationType ViolationType { get; set; }
    }

    [StoryIgnore("Enumeration for categorizing violation types in reporting")]
    public enum StoryViolationType
    {
        IncompleteImplementation,
        DebuggingCode,
        NamingConvention,
        UnusedCode,
        PrematureCelebration,
        Other
    }
}
