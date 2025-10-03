using System;
using System.Reflection;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest
{
    /// <summary>
    /// Shared utilities for story test validation including IL analysis helpers.
    /// Enhanced from original TinyWalnutGames implementation.
    /// </summary>
    public static class StoryTestUtilities
    {
        /// <summary>
        /// Analyzes IL bytes to detect NotImplementedException throws.
        /// Enhanced IL analysis from original TinyWalnutGames implementation.
        /// </summary>
        public static bool ContainsThrowNotImplementedException(byte[] ilBytes)
        {
            if (ilBytes == null || ilBytes.Length == 0) return false;

            // Look for the IL pattern of throwing NotImplementedException
            // This is a simplified check - real IL analysis would be more complex
            for (int i = 0; i < ilBytes.Length - 4; i++)
            {
                // Look for newobj instruction (0x73) followed by throw (0x7A)
                if (ilBytes[i] == 0x73 && i + 5 < ilBytes.Length && ilBytes[i + 5] == 0x7A)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a method only returns default values.
        /// </summary>
        public static bool IsOnlyDefaultReturn(MethodInfo method, byte[] ilBytes)
        {
            if (method.ReturnType == typeof(void)) return false;
            if (ilBytes == null || ilBytes.Length == 0) return false;

            // Simple check for methods that just return default values
            // This would need more sophisticated IL analysis in a real implementation
            if (ilBytes.Length <= 8) // Very short methods might just return defaults
            {
                // Look for patterns like ldnull, ret or ldc.i4.0, ret
                for (int i = 0; i < ilBytes.Length - 1; i++)
                {
                    if ((ilBytes[i] == 0x14 || // ldnull
                         ilBytes[i] == 0x16 || // ldc.i4.0
                         ilBytes[i] == 0x17) && // ldc.i4.1
                        ilBytes[i + 1] == 0x2A) // ret
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines the violation type based on the violation description.
        /// </summary>
        public static StoryViolationType GetViolationType(string violation)
        {
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