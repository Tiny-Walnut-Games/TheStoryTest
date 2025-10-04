using System;
using System.Reflection;

namespace TinyWalnutGames.StoryTest.Shared
{
    /// <summary>
    /// Delegate for validation rules used in story test validation.
    /// </summary>
    public delegate bool ValidationRule(MemberInfo member, out string violation);

    /// <summary>
    /// Shared utilities for story test validation including IL analysis helpers.
    /// </summary>
    [StoryIgnore("Infrastructure utilities for Story Test validation framework")]
    public static class StoryTestUtilities
    {
        public static bool ContainsThrowNotImplementedException(byte[] ilBytes)
        {
            if (ilBytes == null || ilBytes.Length == 0) return false;
            for (int i = 0; i < ilBytes.Length - 4; i++)
            {
                if (ilBytes[i] == 0x73 && i + 5 < ilBytes.Length && ilBytes[i + 5] == 0x7A)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsOnlyDefaultReturn(MethodInfo method, byte[] ilBytes)
        {
            if (method.ReturnType == typeof(void)) return false;
            if (ilBytes == null || ilBytes.Length == 0) return false;
            if (ilBytes.Length <= 8)
            {
                for (int i = 0; i < ilBytes.Length - 1; i++)
                {
                    if ((ilBytes[i] == 0x14 || ilBytes[i] == 0x16 || ilBytes[i] == 0x17) && ilBytes[i + 1] == 0x2A)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static StoryViolationType GetViolationType(string violation)
        {
            // Note: This method categorizes ACTUAL violation messages (not comments)
            // Violation messages contain the real words without 🏳 prefix
            if (violation.Contains("TODO") || violation.Contains("NotImplementedException"))
                return StoryViolationType.IncompleteImplementation;
            if (violation.Contains("Phantom") || violation.Contains("Cold") || violation.Contains("Hollow"))
                return StoryViolationType.UnusedCode;
            if (violation.Contains("Abstract") || violation.Contains("Unsealed"))
                return StoryViolationType.IncompleteImplementation;
            if (violation.Contains("Debug") || violation.Contains("Test"))
                return StoryViolationType.DebuggingCode;
            if (violation.Contains("Premature"))
                return StoryViolationType.PrematureCelebration;
            return StoryViolationType.Other;
        }
    }

}