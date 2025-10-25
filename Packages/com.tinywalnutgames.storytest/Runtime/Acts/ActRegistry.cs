using System;
using System.Collections.Generic;
using System.Reflection;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Acts
{
    /// <summary>
    /// Central registry for all Story Test Acts.
    /// Provides a unified way to access and invoke validation rules.
    /// </summary>
    [StoryIgnore("Story Test framework infrastructure")]
    public static class ActRegistry
    {
        /// <summary>
        /// Core acts (1-11) that are always active.
        /// </summary>
        public static readonly Dictionary<int, string> CoreActNames = new()
        {
            { 1, nameof(Act1TodoComments) },
            { 2, nameof(Act2PlaceholderImplementations) },
            { 3, nameof(Act3IncompleteClasses) },
            { 4, nameof(Act4UnsealedAbstractMembers) },
            { 5, nameof(Act5DebugOnlyImplementations) },
            { 6, nameof(Act6PhantomProps) },
            { 7, nameof(Act7ColdMethods) },
            { 8, nameof(Act8HollowEnums) },
            { 9, nameof(Act9PrematureCelebrations) },
            { 10, nameof(Act10SuspiciouslySimple) },
            { 11, nameof(Act11DeadCode) }
        };

        /// <summary>
        /// Extended acts (12-13) that are optional and require explicit opt-in.
        /// </summary>
        public static readonly Dictionary<int, string> ExtendedActNames = new()
        {
            { 12, nameof(Act12MentalModelClaims) },
            { 13, nameof(Act13NarrativeCoherence) }
        };

        /// <summary>
        /// Get a validation rule by act number.
        /// Returns null if the act doesn't exist.
        /// </summary>
        public static ValidationRule GetActRule(int actNumber)
        {
            return actNumber switch
            {
                1 => Act1TodoComments.Rule,
                2 => Act2PlaceholderImplementations.Rule,
                3 => Act3IncompleteClasses.Rule,
                4 => Act4UnsealedAbstractMembers.Rule,
                5 => Act5DebugOnlyImplementations.Rule,
                6 => Act6PhantomProps.Rule,
                7 => Act7ColdMethods.Rule,
                8 => Act8HollowEnums.Rule,
                9 => Act9PrematureCelebrations.Rule,
                10 => Act10SuspiciouslySimple.Rule,
                11 => Act11DeadCode.Rule,
                12 => Act12MentalModelClaims.Rule,
                13 => Act13NarrativeCoherence.Rule,
                _ => null
            };
        }

        /// <summary>
        /// Get the name of an act by number.
        /// </summary>
        public static string GetActName(int actNumber)
        {
            if (CoreActNames.TryGetValue(actNumber, out var coreName))
                return coreName;
            
            if (ExtendedActNames.TryGetValue(actNumber, out var extName))
                return extName;

            return $"Act{actNumber}Unknown";
        }

        /// <summary>
        /// Validate a member against a specific act.
        /// </summary>
        public static bool ValidateMember(int actNumber, MemberInfo member, out string violation)
        {
            violation = null;
            var rule = GetActRule(actNumber);
            
            if (rule == null)
                return false; // Act not found

            try
            {
                return rule(member, out violation);
            }
            catch (Exception ex)
            {
                violation = $"Error in {GetActName(actNumber)}: {ex.Message}";
                return true;
            }
        }

        /// <summary>
        /// Get all available act numbers (core + extended).
        /// </summary>
        public static int[] GetAllActNumbers()
        {
            var all = new List<int>();
            all.AddRange(CoreActNames.Keys);
            all.AddRange(ExtendedActNames.Keys);
            all.Sort();
            return all.ToArray();
        }

        /// <summary>
        /// Get core act numbers only (1-11).
        /// </summary>
        public static int[] GetCoreActNumbers()
        {
            var core = new List<int>(CoreActNames.Keys);
            core.Sort();
            return core.ToArray();
        }

        /// <summary>
        /// Get extended act numbers only (12-13).
        /// </summary>
        public static int[] GetExtendedActNumbers()
        {
            var ext = new List<int>(ExtendedActNames.Keys);
            ext.Sort();
            return ext.ToArray();
        }
    }
}