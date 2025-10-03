using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using TinyWalnutGames.StoryTest.Shared;
#if UNITY_EDITOR || UNITY_ENGINE
using Debug = UnityEngine.Debug;
#else
using Debug = System.Diagnostics.Debug;
#endif

namespace TinyWalnutGames.StoryTest
{
    /// <summary>
    /// Validates code integrity according to story test methodology.
    /// Enhanced synthesis of original TinyWalnutGames IL analysis with comprehensive validation.
    /// Ensures all symbols are fully implemented with no placeholders or incomplete implementations.
    /// </summary>

    public static class StoryIntegrityValidator
    {
        public static readonly List<ValidationRule> _validationRules = new();

        /// <summary>
        /// Returns all registered validation rules.
        /// </summary>
        public static IEnumerable<ValidationRule> GetRegisteredRules()
        {
            return _validationRules;
        }

        /// <summary>
        /// Registers a validation rule at runtime.
        /// </summary>
        /// <param name="rule">The validation rule to register.</param>
        public static void RegisterRule(ValidationRule rule)
        {
            if (rule != null && !_validationRules.Contains(rule))
                _validationRules.Add(rule);
        }

        /// <summary>
        /// Clears all registered validation rules. Used for re-initialization.
        /// </summary>
        public static void ClearRules()
        {
            _validationRules.Clear();
        }

        /// <summary>
        /// Validates all types in the specified assemblies according to story test rules.
        /// Enhanced with IL analysis from original TinyWalnutGames implementation.
        /// </summary>
        /// <param name="assemblies">Assemblies to validate</param>
        /// <returns>List of violations found</returns>
        public static List<StoryViolation> ValidateAssemblies(params Assembly[] assemblies)
        {
            var violations = new List<StoryViolation>();

            foreach (var assembly in assemblies)
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        violations.AddRange(ValidateType(type));
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Debug.LogWarning($"Could not load all types from assembly {assembly.FullName}: {ex.Message}");
                    // Continue with types that did load
                    foreach (var type in ex.Types)
                    {
                        if (type != null)
                        {
                            violations.AddRange(ValidateType(type));
                        }
                    }
                }
            }

            return violations;
        }

        /// <summary>
        /// Validates a specific type according to story test rules.
        /// Enhanced with metadata token analysis from original implementation.
        /// </summary>
        /// <param name="type">Type to validate</param>
        /// <returns>List of violations found in the type</returns>
        public static List<StoryViolation> ValidateType(Type type)
        {
            var violations = new List<StoryViolation>();

            // Skip types marked with StoryIgnore
            if (type.GetCustomAttribute<StoryIgnoreAttribute>() != null)
            {
                return violations;
            }

            // Validate type itself
            violations.AddRange(ValidateMember(type));

            // Validate all members
            var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic |
                                        BindingFlags.Instance | BindingFlags.Static);

            foreach (var member in members)
            {
                if (member.GetCustomAttribute<StoryIgnoreAttribute>() != null)
                    continue;

                violations.AddRange(ValidateMember(member));
            }

            return violations;
        }

        /// <summary>
        /// Validates a specific member according to story test rules.
        /// </summary>
        /// <param name="member">Member to validate</param>
        /// <returns>List of violations found in the member</returns>
        private static List<StoryViolation> ValidateMember(MemberInfo member)
        {
            var violations = new List<StoryViolation>();

            foreach (var rule in _validationRules)
            {
                if (rule(member, out string violation))
                {
                    violations.Add(new StoryViolation
                    {
                        Type = member.DeclaringType?.Name ?? member.Name,
                        Member = member.Name,
                        Violation = violation,
                        ViolationType = StoryTestUtilities.GetViolationType(violation)
                    });
                }
            }

            return violations;
        }

        /// <summary>
        /// Gets a comprehensive production readiness score based on story test validation.
        /// Enhanced from original TinyWalnutGames implementation.
        /// </summary>
        /// <param name="assemblies">Assemblies to analyze</param>
        /// <returns>Production readiness report</returns>
        public static ProductionReadinessReport GetProductionReadinessScore(params Assembly[] assemblies)
        {
            var violations = ValidateAssemblies(assemblies);
            var report = new ProductionReadinessReport();

            report.TotalViolations = violations.Count;
            report.ViolationsByType = violations.GroupBy(v => v.ViolationType)
                                              .ToDictionary(g => g.Key, g => g.Count());

            // Calculate score based on violation severity
            var severityWeights = new Dictionary<StoryViolationType, int>
            {
                { StoryViolationType.IncompleteImplementation, 10 },
                { StoryViolationType.DebuggingCode, 5 },
                { StoryViolationType.UnusedCode, 3 },
                { StoryViolationType.PrematureCelebration, 8 },
                { StoryViolationType.Other, 2 }
            };

            int totalPenalty = 0;
            foreach (var violation in violations)
            {
                if (severityWeights.ContainsKey(violation.ViolationType))
                {
                    totalPenalty += severityWeights[violation.ViolationType];
                }
            }

            // Calculate percentage score (100% - penalties)
            report.ProductionReadinessScore = Math.Max(0, 100 - (totalPenalty * 2)); // Scale penalties
            report.IsProductionReady = report.ProductionReadinessScore >= 95;

            return report;
        }
    }



    /// <summary>
    /// Report of production readiness based on story test analysis.
    /// Enhanced from original TinyWalnutGames implementation.
    /// </summary>
    public class ProductionReadinessReport
    {
        public int ProductionReadinessScore { get; set; }
        public bool IsProductionReady { get; set; }
        public int TotalViolations { get; set; }
        public Dictionary<StoryViolationType, int> ViolationsByType { get; set; } = new Dictionary<StoryViolationType, int>();

        public string GenerateReport()
        {
            var report = "=== PRODUCTION READINESS REPORT ===\n\n";
            report += $"Overall Score: {ProductionReadinessScore}%\n";
            report += $"Production Ready: {(IsProductionReady ? "YES" : "NO")}\n";
            report += $"Total Violations: {TotalViolations}\n\n";

            if (ViolationsByType.Any())
            {
                report += "Violations by Type:\n";
                foreach (var kvp in ViolationsByType.OrderByDescending(x => x.Value))
                {
                    report += $"  {kvp.Key}: {kvp.Value}\n";
                }
            }

            if (IsProductionReady)
            {
                report += "\n🎉 CONGRATULATIONS! This codebase meets >95% production readiness standards!";
            }
            else
            {
                report += $"\n⚠️  Codebase needs improvement to reach 95% production readiness threshold.";
            }

            return report;
        }
    }
}