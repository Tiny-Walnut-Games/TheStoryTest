using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest
{
    /// <summary>
    /// Central orchestrator for Story Test validation. Executes registered validation rules
    /// (Acts) against project assemblies and produces structured violation reports.
    /// </summary>
    public static class StoryIntegrityValidator
    {
        private static readonly object RuleLock = new object();
        private static readonly List<ValidationRule> Rules = new List<ValidationRule>();
        private static bool _rulesInitialized;

        /// <summary>
        /// Registers a validation rule at runtime. Duplicate registrations are ignored.
        /// </summary>
        public static void RegisterRule(ValidationRule rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            EnsureRulesInitialized();

            lock (RuleLock)
            {
                if (!Rules.Contains(rule))
                {
                    Rules.Add(rule);
                }
            }
        }

        /// <summary>
        /// Clears all registered rules. Typically used before re-registering via bootstrapper.
        /// </summary>
        public static void ClearRules()
        {
            lock (RuleLock)
            {
                Rules.Clear();
                _rulesInitialized = false;
            }
        }

        /// <summary>
        /// Returns a snapshot of all registered rules.
        /// </summary>
        public static IReadOnlyList<ValidationRule> GetRegisteredRules()
        {
            EnsureRulesInitialized();
            lock (RuleLock)
            {
                return Rules.ToArray();
            }
        }

        /// <summary>
        /// Validates all members across the provided assemblies.
        /// When no assemblies supplied, auto-discovers project assemblies using StoryTestSettings.
        /// </summary>
        public static List<StoryViolation> ValidateAssemblies(params Assembly[] assemblies)
        {
            EnsureRulesInitialized();
            var violations = new List<StoryViolation>();

            var targetAssemblies = (assemblies != null && assemblies.Length > 0)
                ? assemblies.Where(a => a != null)
                : GetDefaultAssemblies();

            foreach (var assembly in targetAssemblies.Distinct())
            {
                violations.AddRange(ValidateAssemblyInternal(assembly));
            }

            return violations;
        }

        /// <summary>
        /// Validates a specific type and all of its members.
        /// </summary>
        public static List<StoryViolation> ValidateType(Type type)
        {
            EnsureRulesInitialized();

            if (type == null || HasStoryIgnore(type))
            {
                return new List<StoryViolation>();
            }

            return ValidateMembersForType(type).ToList();
        }

        /// <summary>
        /// Allows explicit registration of rules found in an assembly (used by bootstrapper/tests).
        /// </summary>
        internal static void RegisterRulesFromAssembly(Assembly assembly)
        {
            if (assembly == null) return;

            EnsureRulesInitialized();
            RegisterRulesFromAssemblyInternal(assembly, typeof(ValidationRule));
        }

        private static IEnumerable<Assembly> GetDefaultAssemblies()
        {
            StoryTestSettings settings = null;
            try
            {
                settings = StoryTestSettings.Instance;
            }
            catch
            {
                // Running outside Unity (e.g., standalone validator) – settings may not be available.
            }

            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => IsProjectAssembly(a, settings));
        }

        private static IEnumerable<StoryViolation> ValidateAssemblyInternal(Assembly assembly)
        {
            var violations = new List<StoryViolation>();
            if (assembly == null) return violations;

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray();
                LogWarning($"[Story Test] Could not load all types from {assembly.FullName}: {ex.Message}");
            }

            foreach (var type in types)
            {
                if (type == null || HasStoryIgnore(type))
                {
                    continue;
                }

                try
                {
                    violations.AddRange(ValidateMembersForType(type));
                }
                catch (Exception ex)
                {
                    LogWarning($"[Story Test] Failed to validate {type.FullName}: {ex.Message}");
                }
            }

            return violations;
        }

        private static IEnumerable<StoryViolation> ValidateMembersForType(Type type)
        {
            foreach (var member in EnumerateMembers(type))
            {
                if (member == null || HasStoryIgnore(member))
                {
                    continue;
                }

                foreach (var rule in GetRulesSnapshot())
                {
                    bool hasViolation;
                    string violationMessage;

                    try
                    {
                        hasViolation = rule(member, out violationMessage);
                    }
                    catch (Exception ex)
                    {
                        LogWarning($"[Story Test] Rule {rule.Method.DeclaringType?.FullName}.{rule.Method.Name} threw for {member.Name}: {ex.Message}");
                        continue;
                    }

                    if (hasViolation && !string.IsNullOrWhiteSpace(violationMessage))
                    {
                        yield return CreateViolation(member, violationMessage);
                    }
                }
            }
        }

        private static StoryViolation CreateViolation(MemberInfo member, string violation)
        {
            var declaringType = member as Type ?? member.DeclaringType;

            return new StoryViolation
            {
                Type = declaringType?.FullName ?? member.Name,
                Member = member.Name,
                Violation = violation,
                FilePath = declaringType?.Module?.FullyQualifiedName ?? string.Empty,
                LineNumber = 0,
                ViolationType = StoryTestUtilities.GetViolationType(violation)
            };
        }

        private static IEnumerable<MemberInfo> EnumerateMembers(Type type)
        {
            yield return type;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                       BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            foreach (var ctor in type.GetConstructors(flags))
            {
                yield return ctor;
            }

            foreach (var method in type.GetMethods(flags))
            {
                yield return method;
            }

            foreach (var property in type.GetProperties(flags))
            {
                yield return property;
            }

            foreach (var field in type.GetFields(flags))
            {
                yield return field;
            }

            foreach (var evt in type.GetEvents(flags))
            {
                yield return evt;
            }

            foreach (var nested in type.GetNestedTypes(flags))
            {
                foreach (var nestedMember in EnumerateMembers(nested))
                {
                    yield return nestedMember;
                }
            }
        }

        private static bool HasStoryIgnore(MemberInfo member)
        {
            return member.GetCustomAttributes(typeof(StoryIgnoreAttribute), true).Any();
        }

        private static IReadOnlyList<ValidationRule> GetRulesSnapshot()
        {
            lock (RuleLock)
            {
                return Rules.ToArray();
            }
        }

        private static void EnsureRulesInitialized()
        {
            lock (RuleLock)
            {
                if (_rulesInitialized)
                {
                    return;
                }

                TryAutoRegisterRules();
                _rulesInitialized = true;
            }
        }

        private static void TryAutoRegisterRules()
        {
            var ruleType = typeof(ValidationRule);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = assembly.GetName().Name;
                if (name == "TinyWalnutGames.TheStoryTest.Acts" || name.EndsWith("StoryTest.Acts", StringComparison.Ordinal))
                {
                    RegisterRulesFromAssemblyInternal(assembly, ruleType);
                }
            }
        }

        private static void RegisterRulesFromAssemblyInternal(Assembly assembly, Type ruleType)
        {
            foreach (var field in assembly.GetTypes()
                         .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static)))
            {
                if (!ruleType.IsAssignableFrom(field.FieldType))
                {
                    continue;
                }

                if (field.GetValue(null) is ValidationRule rule && !Rules.Contains(rule))
                {
                    Rules.Add(rule);
                }
            }
        }

        private static bool IsProjectAssembly(Assembly assembly, StoryTestSettings settings)
        {
            if (assembly == null)
            {
                return false;
            }

            var name = assembly.GetName().Name;

            if (name.StartsWith("System", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Newtonsoft", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("nunit", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (name.StartsWith("Unity", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("UnityEngine", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("UnityEditor", StringComparison.OrdinalIgnoreCase))
            {
                return settings != null && settings.includeUnityAssemblies;
            }

            if (settings != null && settings.assemblyFilters != null && settings.assemblyFilters.Length > 0)
            {
                return settings.assemblyFilters.Any(filter =>
                    name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            return true;
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
