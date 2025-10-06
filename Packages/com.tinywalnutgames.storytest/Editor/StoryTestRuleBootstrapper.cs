#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Editor
{
    /// <summary>
    /// Automatically registers all validation rules from the Acts assembly whenever the Unity Editor domain reloads.
    /// Ensures StoryIntegrityValidator always has the latest rule set without requiring manual initialization.
    /// </summary>
    // REMOVED [InitializeOnLoad] - was causing 2+ minute hangs on domain reload
    // Rules are now lazy-loaded on first validation instead of eager-loaded on startup
    [StoryIgnore("Editor bootstrapper for story test validation rules")]
    public static class StoryTestRuleBootstrapper
    {
        private const string ActsAssemblyName = "TinyWalnutGames.TheStoryTest.Acts";
        private const string LogPrefix = "[Story Test]";
        private static bool _hasBootstrapped;

        // No longer runs automatically - must be called explicitly
        public static void EnsureBootstrapped()
        {
            if (_hasBootstrapped) return;

            try
            {
                Bootstrap();
                _hasBootstrapped = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} Failed to register validation rules: {ex.Message}");
            }
        }

        private static void Bootstrap()
        {
            var actsAssembly = EnsureActsAssemblyLoaded();
            if (actsAssembly == null)
            {
                Debug.LogWarning($"{LogPrefix} Could not locate {ActsAssemblyName}. Story validation rules were not registered.");
                return;
            }

            StoryIntegrityValidator.ClearRules();

            var ruleType = typeof(ValidationRule);
            var rules = ExtractValidationRules(actsAssembly, ruleType).ToList();

            foreach (var rule in rules)
            {
                StoryIntegrityValidator.RegisterRule(rule);
            }

            var registeredCount = StoryIntegrityValidator.GetRegisteredRules().Count;
            Debug.Log($"{LogPrefix} Registered {registeredCount} story validation rules from {actsAssembly.GetName().Name}.");
        }

        private static Assembly EnsureActsAssemblyLoaded()
        {
            var actsAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(IsActsAssembly);
            if (actsAssembly != null)
            {
                return actsAssembly;
            }

            try
            {
                return Assembly.Load(ActsAssemblyName);
            }
            catch
            {
                return null;
            }
        }

        private static bool IsActsAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                return false;
            }

            var name = assembly.GetName().Name;
            return string.Equals(name, ActsAssemblyName, StringComparison.Ordinal) ||
                   name.EndsWith("StoryTest.Acts", StringComparison.Ordinal);
        }

        private static IEnumerable<ValidationRule> ExtractValidationRules(Assembly assembly, Type ruleType)
        {
            IEnumerable<Type> types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null);
                Debug.LogWarning($"{LogPrefix} Could not load all types from {assembly.FullName}: {ex.Message}");
            }

            foreach (var field in types.SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static)))
            {
                if (!ruleType.IsAssignableFrom(field.FieldType))
                {
                    continue;
                }

                if (field.GetValue(null) is ValidationRule rule)
                {
                    yield return rule;
                }
            }
        }
    }
}
#endif
