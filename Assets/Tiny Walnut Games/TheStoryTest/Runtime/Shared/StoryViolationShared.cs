using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace TinyWalnutGames.StoryTest.Shared
{
    /// <summary>
    /// Types and utilities shared between StoryTest and StoryTest.Acts.
    /// </summary>
    public class StoryViolation
    {
        public string Type { get; set; }
        public string Member { get; set; }
        public string Violation { get; set; }
        public StoryViolationType ViolationType { get; set; }
    }

    public enum StoryViolationType
    {
        IncompleteImplementation,
        DebuggingCode,
        UnusedCode,
        PrematureCelebration,
        Other
    }
}
