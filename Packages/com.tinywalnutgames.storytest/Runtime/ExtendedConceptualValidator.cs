using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest
{
    /// <summary>
    /// Bridges conceptual validation utilities (shared assembly) with the main Story Test runtime.
    /// Provides high-level orchestration for enum/value-type/abstract sealing checks alongside
    /// custom project validation designated in StoryTestSettings.
    /// </summary>
    [StoryIgnore("Conceptual validation orchestrator for Story Test framework")]
    public static class ExtendedConceptualValidator
    {
        /// <summary>
        /// Runs conceptual validation according to StoryTestSettings configuration.
        /// </summary>
        public static List<StoryViolation> RunConceptualValidation(StoryTestSettings settings = null, bool log = true)
        {
            settings ??= SafeGetSettings();
            var config = settings?.conceptualValidation ?? new ConceptualValidationConfig();

            if (!config.enableConceptTests)
            {
                return new List<StoryViolation>();
            }

            if (config.autoDetectEnvironment)
            {
                try
                {
                    config.environmentCapabilities = ConceptualValidator.DetectEnvironment();
                    if (log)
                    {
                        LogInfo($"[Story Test] Environment detected: {config.environmentCapabilities}");
                    }
                }
                catch (Exception ex)
                {
                    LogWarning($"[Story Test] Environment detection failed: {ex.Message}");
                }
            }

            var assemblies = GetProjectAssemblies(settings);
            var violations = new List<StoryViolation>();
            var tiers = config.validationTiers ?? new ValidationTiers();

            IEnumerable<Assembly> enumerable = assemblies as Assembly[] ?? assemblies.ToArray();
            if (tiers.universal)
            {
                if (log) LogInfo("[Story Test] Running conceptual enum validation...");
                violations.AddRange(ValidateProjectEnums(settings, enumerable));

                if (log) LogInfo("[Story Test] Running conceptual value type validation...");
                violations.AddRange(ValidateProjectValueTypes(settings, enumerable, config.environmentCapabilities));
            }

            if (tiers.unityAware)
            {
                if (log) LogInfo("[Story Test] Running abstract member sealing validation...");
                violations.AddRange(ValidateAbstractMemberSealing(settings, enumerable));
            }

            if (tiers.projectSpecific && config.customComponentTypes is { Length: > 0 })
            {
                if (log) LogInfo("[Story Test] Running project-specific conceptual validation...");
                violations.AddRange(ConceptualValidator.ValidateCustomComponents(config.customComponentTypes));
            }

            return violations;
        }

        /// <summary>
        /// Validates enums across provided assemblies.
        /// </summary>
        public static List<StoryViolation> ValidateProjectEnums(StoryTestSettings settings, IEnumerable<Assembly> assemblies = null)
        {
            assemblies ??= GetProjectAssemblies(settings);
            var violations = new List<StoryViolation>();

            foreach (var assembly in assemblies)
            {
                foreach (var type in GetTypesSafe(assembly).Where(t => t.IsEnum))
                {
                    if (HasStoryIgnore(type)) continue;

                    var messages = ConceptualValidator.ValidateEnum(type);
                    violations.AddRange(messages.Select(message => CreateViolation(type, message)));
                }
            }

            return violations;
        }

        /// <summary>
        /// Validates value-type structures for completeness.
        /// </summary>
        public static List<StoryViolation> ValidateProjectValueTypes(
            StoryTestSettings settings,
            IEnumerable<Assembly> assemblies = null,
            EnvironmentCapabilities capabilities = null)
        {
            assemblies ??= GetProjectAssemblies(settings);
            var violations = new List<StoryViolation>();
            var canInstantiate = capabilities?.canInstantiateComponents ?? true;

            foreach (var assembly in assemblies)
            {
                foreach (var type in GetTypesSafe(assembly)
                             .Where(t => t.IsValueType && !t.IsEnum && !t.IsPrimitive))
                {
                    if (HasStoryIgnore(type)) continue;

                    var messages = ConceptualValidator.ValidateValueType(type, canInstantiate);
                    violations.AddRange(messages.Select(message => CreateViolation(type, message)));
                }
            }

            return violations;
        }

        /// <summary>
        /// Validates that classes with abstract members are marked abstract.
        /// </summary>
        public static List<StoryViolation> ValidateAbstractMemberSealing(StoryTestSettings settings, IEnumerable<Assembly> assemblies = null)
        {
            assemblies ??= GetProjectAssemblies(settings);
            var violations = new List<StoryViolation>();

            foreach (var assembly in assemblies)
            {
                foreach (var type in GetTypesSafe(assembly).Where(t => t.IsClass))
                {
                    if (HasStoryIgnore(type)) continue;

                    var messages = ConceptualValidator.ValidateAbstractMemberSealing(type);
                    violations.AddRange(messages.Select(message => CreateViolation(type, message)));
                }
            }

            return violations;
        }

        private static StoryTestSettings SafeGetSettings()
        {
            try
            {
                return StoryTestSettings.Instance;
            }
            catch
            {
                return new StoryTestSettings();
            }
        }

        private static IEnumerable<Assembly> GetProjectAssemblies(StoryTestSettings settings)
        {
            try
            {
                return ConceptualValidator.GetProjectAssemblies(settings);
            }
            catch (Exception ex)
            {
                LogWarning($"[Story Test] Failed to load project assemblies: {ex.Message}");
                return Array.Empty<Assembly>();
            }
        }

        private static IEnumerable<Type> GetTypesSafe(Assembly assembly)
        {
            if (assembly == null)
            {
                return Enumerable.Empty<Type>();
            }

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                LogWarning($"[Story Test] Partial type load for {assembly.FullName}: {ex.Message}");
                return ex.Types.Where(t => t != null);
            }
            catch (Exception ex)
            {
                LogWarning($"[Story Test] Failed to enumerate types for {assembly.FullName}: {ex.Message}");
                return Enumerable.Empty<Type>();
            }
        }

        private static StoryViolation CreateViolation(Type type, string message)
        {
            return new StoryViolation
            {
                Type = type.FullName,
                Member = type.Name,
                Violation = message,
                FilePath = type.Assembly.Location,
                LineNumber = 0,
                ViolationType = StoryTestUtilities.GetViolationType(message)
            };
        }

        private static bool HasStoryIgnore(MemberInfo member)
        {
            return member?.GetCustomAttributes(typeof(StoryIgnoreAttribute), true).Any() == true;
        }

        private static void LogInfo(string message)
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL || UNITY_SERVER
            UnityEngine.Debug.Log(message);
#else
            Debug.WriteLine(message);
#endif
        }

        private static void LogWarning(string message)
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL || UNITY_SERVER
            UnityEngine.Debug.LogWarning(message);
#else
            Debug.WriteLine(message);
#endif
        }
    }
}
