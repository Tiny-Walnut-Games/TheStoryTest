using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest
{
    /// <summary>
    /// Extended conceptual validation that works with StoryIntegrityValidator.
    /// This class bridges the gap between Shared assembly (ConceptualValidator) and Main assembly.
    /// </summary>
    public static class ExtendedConceptualValidator
    {
        /// <summary>
        /// Validate custom component types using the full Story Test rule set.
        /// This is the Main assembly version that can call StoryIntegrityValidator.
        /// </summary>
        public static List<StoryViolation> ValidateCustomComponents(string[] componentTypeNames)
        {
            var violations = new List<StoryViolation>();

            if (componentTypeNames == null || componentTypeNames.Length == 0)
            {
                return violations;
            }

            foreach (var typeName in componentTypeNames)
            {
                var type = Type.GetType(typeName);
                if (type == null)
                {
                    // Try to find the type in all loaded assemblies
                    type = AppDomain.CurrentDomain.GetAssemblies()
                        .Where(a => !a.IsDynamic)
                        .SelectMany(a => 
                        {
                            try { return a.GetTypes(); }
                            catch { return new Type[0]; }
                        })
                        .FirstOrDefault(t => t.FullName == typeName || t.Name == typeName);
                }

                if (type == null)
                {
                    violations.Add(new StoryViolation
                    {
                        Type = typeName,
                        Member = null,
                        Violation = $"Custom component type '{typeName}' not found in loaded assemblies. Check configuration.",
                        FilePath = "StoryTestSettings.json",
                        LineNumber = 0,
                        ViolationType = StoryViolationType.Other
                    });
                    continue;
                }

                // Validate the type using full Story Test rules
                var typeViolations = StoryIntegrityValidator.ValidateType(type);
                violations.AddRange(typeViolations);
            }

            return violations;
        }

        /// <summary>
        /// Validate all enums across project assemblies using conceptual rules.
        /// </summary>
        public static List<StoryViolation> ValidateProjectEnums(StoryTestSettings settings)
        {
            var violations = new List<StoryViolation>();
            var assemblies = ConceptualValidator.GetProjectAssemblies(settings);

            foreach (var assembly in assemblies)
            {
                try
                {
                    var enumTypes = assembly.GetTypes()
                        .Where(t => t.IsEnum && !HasStoryIgnore(t));

                    foreach (var enumType in enumTypes)
                    {
                        var enumViolations = ConceptualValidator.ValidateEnum(enumType);
                        foreach (var violation in enumViolations)
                        {
                            violations.Add(new StoryViolation
                            {
                                Type = enumType.FullName,
                                Member = null,
                                Violation = violation,
                                FilePath = enumType.Assembly.Location,
                                LineNumber = 0,
                                ViolationType = StoryViolationType.IncompleteImplementation
                            });
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // Log and continue - some assemblies may have loading issues
                    UnityEngine.Debug.LogWarning($"[Story Test] Could not load types from {assembly.FullName}: {ex.Message}");
                }
            }

            return violations;
        }

        /// <summary>
        /// Validate all value types (structs) across project assemblies.
        /// </summary>
        public static List<StoryViolation> ValidateProjectValueTypes(StoryTestSettings settings)
        {
            var violations = new List<StoryViolation>();
            var assemblies = ConceptualValidator.GetProjectAssemblies(settings);
            var capabilities = ConceptualValidator.DetectEnvironment();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var valueTypes = assembly.GetTypes()
                        .Where(t => t.IsValueType && !t.IsEnum && !t.IsPrimitive && !HasStoryIgnore(t));

                    foreach (var type in valueTypes)
                    {
                        var typeViolations = ConceptualValidator.ValidateValueType(
                            type, 
                            capabilities.canInstantiateComponents
                        );

                        foreach (var violation in typeViolations)
                        {
                            violations.Add(new StoryViolation
                            {
                                Type = type.FullName,
                                Member = null,
                                Violation = violation,
                                FilePath = type.Assembly.Location,
                                LineNumber = 0,
                                ViolationType = StoryViolationType.IncompleteImplementation
                            });
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    UnityEngine.Debug.LogWarning($"[Story Test] Could not load types from {assembly.FullName}: {ex.Message}");
                }
            }

            return violations;
        }

        /// <summary>
        /// Validate abstract member sealing across project assemblies.
        /// </summary>
        public static List<StoryViolation> ValidateAbstractMemberSealing(StoryTestSettings settings)
        {
            var violations = new List<StoryViolation>();
            var assemblies = ConceptualValidator.GetProjectAssemblies(settings);

            foreach (var assembly in assemblies)
            {
                try
                {
                    var classTypes = assembly.GetTypes()
                        .Where(t => t.IsClass && !HasStoryIgnore(t));

                    foreach (var type in classTypes)
                    {
                        var typeViolations = ConceptualValidator.ValidateAbstractMemberSealing(type);

                        foreach (var violation in typeViolations)
                        {
                            violations.Add(new StoryViolation
                            {
                                Type = type.FullName,
                                Member = null,
                                Violation = violation,
                                FilePath = type.Assembly.Location,
                                LineNumber = 0,
                                ViolationType = StoryViolationType.IncompleteImplementation
                            });
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    UnityEngine.Debug.LogWarning($"[Story Test] Could not load types from {assembly.FullName}: {ex.Message}");
                }
            }

            return violations;
        }

        /// <summary>
        /// Run all conceptual validation tiers based on settings configuration.
        /// </summary>
        public static List<StoryViolation> RunConceptualValidation(StoryTestSettings settings)
        {
            var allViolations = new List<StoryViolation>();

            if (!settings.conceptualValidation.enableConceptTests)
            {
                UnityEngine.Debug.Log("[Story Test] Conceptual validation disabled in settings");
                return allViolations;
            }

            // Tier 1: Universal validation (Acts 1-9) is always run via StoryIntegrityValidator

            // Tier 2: Conceptual validation
            if (settings.conceptualValidation.validationTiers.unityAware || 
                settings.conceptualValidation.validationTiers.universal)
            {
                UnityEngine.Debug.Log("[Story Test] Running conceptual enum validation...");
                allViolations.AddRange(ValidateProjectEnums(settings));

                UnityEngine.Debug.Log("[Story Test] Running conceptual value type validation...");
                allViolations.AddRange(ValidateProjectValueTypes(settings));

                UnityEngine.Debug.Log("[Story Test] Running abstract member sealing validation...");
                allViolations.AddRange(ValidateAbstractMemberSealing(settings));
            }

            // Tier 3: Project-specific validation
            if (settings.conceptualValidation.validationTiers.projectSpecific)
            {
                if (settings.conceptualValidation.customComponentTypes != null && 
                    settings.conceptualValidation.customComponentTypes.Length > 0)
                {
                    UnityEngine.Debug.Log($"[Story Test] Running project-specific validation for {settings.conceptualValidation.customComponentTypes.Length} custom types...");
                    allViolations.AddRange(ValidateCustomComponents(settings.conceptualValidation.customComponentTypes));
                }
            }

            return allViolations;
        }

        /// <summary>
        /// Check if a type has the StoryIgnore attribute.
        /// </summary>
        private static bool HasStoryIgnore(Type type)
        {
            return type.GetCustomAttributes(typeof(StoryIgnoreAttribute), true).Length > 0;
        }
    }
}
