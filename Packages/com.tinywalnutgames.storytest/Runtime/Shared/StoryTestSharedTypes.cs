using System;
using System.Reflection;

namespace TinyWalnutGames.StoryTest.Shared
{
    /// <summary>
    /// Delegate for validation rules used in story test validation.
    /// </summary>
    public delegate bool ValidationRule(MemberInfo member, out string violation);

    /// <summary>
    /// Shared utilities for story test validation - including IL analysis helpers.
    /// </summary>
    [StoryIgnore("Infrastructure utilities for Story Test validation framework")]
    public static class StoryTestUtilities
    {
        public static bool ContainsThrowNotImplementedException(byte[] ilBytes)
        {
            if (ilBytes == null || ilBytes.Length == 0) return false;

            // Pattern: newobj (0x73) + 4-byte metadata token + throw (0x7A)
            // We need to verify the metadata token points to NotImplementedException constructor
            // However, metadata tokens are assembly-specific, so we'll use a more robust heuristic:
            // 1. Check for the basic pattern (newobj + throw)
            // 2. Verify the IL sequence is extremely short (typical for throw-only methods)
            // 3. Ensure there's no other meaningful logic

            bool hasNewobjThrowPattern = false;

            for (int i = 0; i < ilBytes.Length - 5; i++)
            {
                // newobj (0x73) followed by 4-byte token, then throw (0x7A)
                if (ilBytes[i] == 0x73 && ilBytes[i + 5] == 0x7A)
                {
                    hasNewobjThrowPattern = true;
                    break;
                }
            }

            if (!hasNewobjThrowPattern) return false;

            // Additional heuristic: NotImplementedException methods are typically very short
            // and contain minimal logic (usually just: newobj + throw + ret, or similar)
            // Methods with ArgumentNullException will have parameter loading (ldarg) first
            // and will be longer with null checks (ldarg, brfalse/brtrue, etc.)

            // Count meaningful operations before the throw
            int meaningfulOpsBeforeThrow = 0;
            bool foundThrow = false;

            for (int i = 0; i < ilBytes.Length; i++)
            {
                byte opCode = ilBytes[i];

                // If we hit throw, stop counting
                if (opCode == 0x7A)
                {
                    foundThrow = true;
                    break;
                }

                // Count operations that suggest actual validation logic
                // ldarg (parameter loading): 0x02-0x09, 0x0E
                if ((opCode >= 0x02 && opCode <= 0x09) || opCode == 0x0E)
                {
                    meaningfulOpsBeforeThrow++;
                }

                // Branch instructions (null checks, conditionals): 0x38-0x45
                if (opCode >= 0x38 && opCode <= 0x45)
                {
                    meaningfulOpsBeforeThrow += 2; // Branches are strong indicators of validation
                }

                // Method calls before throw (calling exception helpers, etc.)
                if (opCode == 0x28 || opCode == 0x6F)
                {
                    meaningfulOpsBeforeThrow++;
                }
            }

            // If there are multiple parameter loads and branches, it's likely ArgumentException validation
            // NotImplementedException throws typically have minimal/no logic before the throw
            // Use threshold of 3 to allow for simple parameter passing to exception constructor
            if (meaningfulOpsBeforeThrow > 3)
            {
                return false; // Likely a validation throw (ArgumentNullException, etc.)
            }

            return foundThrow;
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
            // Violation messages contain the real words without the 🏳 prefix
            if (violation.Contains("TODO") || violation.Contains("NotImplementedException"))
                return StoryViolationType.IncompleteImplementation;
            if (violation.Contains("placeholder", StringComparison.OrdinalIgnoreCase))
                return StoryViolationType.PlaceholderCode;
            if (violation.Contains("Phantom") || violation.Contains("Cold") || violation.Contains("Hollow"))
                return StoryViolationType.UnusedCode;
            if (violation.Contains("Abstract") || violation.Contains("Unsealed"))
                return StoryViolationType.IncompleteImplementation;
            if (violation.Contains("Debug") || violation.Contains("Test"))
                return StoryViolationType.DebuggingCode;
            if (violation.Contains("Premature"))
                return StoryViolationType.PrematureCelebration;
            return violation.Contains("naming", StringComparison.OrdinalIgnoreCase) ? StoryViolationType.NamingConvention : StoryViolationType.Other;
        }
    }

}