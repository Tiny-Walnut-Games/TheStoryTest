using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TinyWalnutGames.StoryTest.Shared;
using UnityEngine;

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
        /// When no assemblies are supplied, auto-discovers project assemblies using StoryTestSettings.
        /// </summary>
        public static List<StoryViolation> ValidateAssemblies(params Assembly[] assemblies)
        {
            EnsureRulesInitialized();
            var violations = new List<StoryViolation>();

            var targetAssemblies = assemblies is { Length: > 0 }
                ? assemblies.Where(a => a != null)
                : GetDefaultAssemblies();

            foreach (var assembly in targetAssemblies.Distinct())
            {
                // CRITICAL SAFETY CHECK: Never validate test assemblies
                // They contain async state machines, lambda closures, and test fixtures
                // that create massive false positives and performance issues
                var name = assembly.GetName().Name;

                // Only skip assemblies that END with .Tests or .Test (actual test assemblies)
                // Don't skip assemblies that just contain "Test" in the name (like "TheStoryTest")
                if (name.EndsWith(".Tests") || name.EndsWith(".Test") ||
                    name.EndsWith("Tests") && !name.Contains("StoryTest"))
                {
                    Debug.Log($"[Story Test] Skipping test assembly: {name}");
                    continue;
                }

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

            // First pass: validate all members in all types
            foreach (var type in types)
            {
                if (type == null || HasStoryIgnore(type))
                {
                    continue;
                }

                // Skip compiler-generated types (async state machines, lambdas, Unity source gen, etc.)
                if (Shared.AdvancedILAnalysis.ShouldSkipType(type))
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

            // Second pass: assembly-level validation (Acts 12-13, etc.)
            // These acts expect null for member parameter and validate the entire assembly
            violations.AddRange(ValidateAssemblyLevel(assembly));

            return violations;
        }

        /// <summary>
        /// Validates at assembly level by calling all rules with null member.
        /// This is where assembly-level Acts (12-13, etc.) execute their logic.
        /// Member-level Acts will return false when given null and won't produce violations.
        /// </summary>
        private static IEnumerable<StoryViolation> ValidateAssemblyLevel(Assembly assembly)
        {
            foreach (var rule in GetRulesSnapshot())
            {
                bool hasViolation;
                string violationMessage;

                try
                {
                    // Call rule with null member to trigger assembly-level validation
                    hasViolation = rule(null, out violationMessage);
                }
                catch (Exception ex)
                {
                    LogWarning($"[Story Test] Assembly-level rule {rule.Method.DeclaringType?.FullName}.{rule.Method.Name} threw: {ex.Message}");
                    continue;
                }

                if (hasViolation && !string.IsNullOrWhiteSpace(violationMessage))
                {
                    yield return new StoryViolation
                    {
                        Type = assembly.GetName().Name,
                        Member = "[Assembly]",
                        Violation = violationMessage,
                        FilePath = assembly.Location,
                        LineNumber = 0,
                        ViolationType = StoryTestUtilities.GetViolationType(violationMessage)
                    };
                }
            }
        }

        private static IEnumerable<StoryViolation> ValidateMembersForType(Type type)
        {
            foreach (var member in EnumerateMembers(type))
            {
                if (member == null || HasStoryIgnore(member) || Shared.AdvancedILAnalysis.ShouldSkipMember(member))
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
                FilePath = declaringType?.Module.FullyQualifiedName ?? string.Empty,
                LineNumber = 0,
                ViolationType = StoryTestUtilities.GetViolationType(violation)
            };
        }

        private static IEnumerable<MemberInfo> EnumerateMembers(Type type)
        {
            // Use iterative approach with a stack to avoid recursion
            var typeStack = new Stack<Type>();
            typeStack.Push(type);

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                       BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            while (typeStack.Count > 0)
            {
                var currentType = typeStack.Pop();

                // Yield the type itself unless it's flagged as generated
                if (!Shared.AdvancedILAnalysis.ShouldSkipType(currentType))
                {
                    yield return currentType;
                }

                // Enumerate all members of the current type
                foreach (var member in EnumerateTypeMembers(currentType, flags))
                {
                    yield return member;
                }

                // Queue nested types for processing
                QueueNestedTypes(currentType, flags, typeStack);
            }
        }

        private static IEnumerable<MemberInfo> EnumerateTypeMembers(Type type, BindingFlags flags)
        {
            foreach (var ctor in FilterMembers(type.GetConstructors(flags)))
                yield return ctor;

            foreach (var method in FilterMembers(type.GetMethods(flags)))
                yield return method;

            foreach (var property in FilterMembers(type.GetProperties(flags)))
                yield return property;

            foreach (var field in FilterMembers(type.GetFields(flags)))
                yield return field;

            foreach (var evt in FilterMembers(type.GetEvents(flags)))
                yield return evt;
        }

        private static IEnumerable<T> FilterMembers<T>(T[] members) where T : MemberInfo
        {
            foreach (var member in members)
            {
                if (!Shared.AdvancedILAnalysis.ShouldSkipMember(member))
                {
                    yield return member;
                }
            }
        }

        private static void QueueNestedTypes(Type type, BindingFlags flags, Stack<Type> typeStack)
        {
            foreach (var nested in type.GetNestedTypes(flags))
            {
                if (!Shared.AdvancedILAnalysis.ShouldSkipType(nested))
                {
                    typeStack.Push(nested);
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

        public static bool IsProjectAssembly(Assembly assembly, StoryTestSettings settings)
        {
            if (assembly == null)
            {
                return false;
            }

            var name = assembly.GetName().Name;

            // Always exclude core/BCL and common third-party libs
            if (name.StartsWith("System", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Newtonsoft", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("nunit", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Unity assemblies are optional based on settings
            if (name.StartsWith("Unity", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("UnityEngine", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("UnityEditor", StringComparison.OrdinalIgnoreCase))
            {
                if (settings is { includeUnityAssemblies: true })
                {
                    // Will still be subject to include-only list below if provided
                }
                else
                {
                    return false;
                }
            }

            // Include-only semantics: when filters provided, only include exact name matches
            if (settings is { assemblyFilters: { Length: > 0 } })
            {
                var includeSet = new HashSet<string>(settings.assemblyFilters.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()), StringComparer.OrdinalIgnoreCase);
                return includeSet.Contains(name);
            }

            // No filters specified: include by default (subject to exclusions above)
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
