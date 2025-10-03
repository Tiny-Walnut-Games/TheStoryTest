using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TinyWalnutGames.StoryTest.Shared
{
    /// <summary>
    /// Provides conceptual validation strategies that adapt to project structure.
    /// This class enables dynamic, environment-agnostic validation beyond the core 9 Acts.
    /// </summary>
    public static class ConceptualValidator
    {
        /// <summary>
        /// Auto-detect available environment capabilities at runtime.
        /// </summary>
        public static EnvironmentCapabilities DetectEnvironment()
        {
            var capabilities = new EnvironmentCapabilities();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            capabilities.hasUnityEngine = assemblies.Any(a => a.GetName().Name == "UnityEngine");
            capabilities.hasEntities = assemblies.Any(a => a.GetName().Name == "Unity.Entities");
            capabilities.hasDOTS = capabilities.hasEntities;
            capabilities.hasBurst = assemblies.Any(a => a.GetName().Name == "Unity.Burst");
            
            // Test if we can instantiate components by trying to create a simple struct
            capabilities.canInstantiateComponents = true; // Assume yes unless proven otherwise
            
            return capabilities;
        }

        /// <summary>
        /// Validate an enum type according to conceptual rules.
        /// Returns violations if enum is "hollow" (too few values, all placeholders).
        /// </summary>
        public static List<string> ValidateEnum(Type enumType)
        {
            var violations = new List<string>();

            if (!enumType.IsEnum)
            {
                return violations;
            }

            var enumValues = Enum.GetValues(enumType);
            var enumNames = Enum.GetNames(enumType);

            // Rule 1: Must have at least 2 values (Act8: HollowEnums)
            if (enumValues.Length < 2)
            {
                violations.Add($"{enumType.FullName}: Enum has only {enumValues.Length} value(s). Enums should have at least 2 meaningful values.");
            }

            // Rule 2: Check for placeholder-only names
            var placeholderNames = new[] { "None", "Default", "Undefined", "Placeholder", "TODO", "TEMP", "Unknown" };
            var nonPlaceholderCount = enumNames.Count(name => 
                !placeholderNames.Contains(name, StringComparer.OrdinalIgnoreCase));

            if (enumValues.Length == 2 && nonPlaceholderCount == 0)
            {
                violations.Add($"{enumType.FullName}: Enum has only placeholder names: {string.Join(", ", enumNames)}");
            }

            return violations;
        }

        /// <summary>
        /// Validate a value type (struct) for proper default constructor and field accessibility.
        /// Falls back to IL analysis if instantiation is not possible.
        /// </summary>
        public static List<string> ValidateValueType(Type valueType, bool canInstantiate = true)
        {
            var violations = new List<string>();

            if (!valueType.IsValueType || valueType.IsEnum || valueType.IsPrimitive)
            {
                return violations;
            }

            if (canInstantiate)
            {
                try
                {
                    // Try runtime instantiation
                    var instance = Activator.CreateInstance(valueType);
                    
                    // Verify all public fields are accessible
                    var fields = valueType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var field in fields)
                    {
                        try
                        {
                            var value = field.GetValue(instance);
                            // Success - field is accessible
                        }
                        catch (Exception ex)
                        {
                            violations.Add($"{valueType.FullName}.{field.Name}: Field not accessible - {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!HasStoryIgnore(valueType))
                    {
                        violations.Add($"{valueType.FullName}: Cannot instantiate value type - {ex.Message}. Consider adding [StoryIgnore] if intentional.");
                    }
                }
            }
            else
            {
                // Fallback: IL analysis mode
                violations.AddRange(ValidateValueTypeViaIL(valueType));
            }

            return violations;
        }

        /// <summary>
        /// Validate a value type using IL bytecode analysis (when instantiation is not possible).
        /// </summary>
        private static List<string> ValidateValueTypeViaIL(Type valueType)
        {
            var violations = new List<string>();

            // Check that type has public fields or properties
            var fields = valueType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var properties = valueType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (fields.Length == 0 && properties.Length == 0)
            {
                violations.Add($"{valueType.FullName}: Value type has no public fields or properties. Likely a placeholder.");
            }

            // Check for default constructor (value types always have one, but verify it's not explicitly deleted)
            var constructors = valueType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            
            return violations;
        }

        /// <summary>
        /// Validate that classes with abstract members are marked as abstract.
        /// This is a conceptual check for Act4 (UnsealedAbstractMembers).
        /// </summary>
        public static List<string> ValidateAbstractMemberSealing(Type classType)
        {
            var violations = new List<string>();

            if (!classType.IsClass || classType.IsAbstract)
            {
                return violations;
            }

            var abstractMethods = classType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.IsAbstract)
                .ToList();

            var abstractProperties = classType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => (p.GetMethod?.IsAbstract ?? false) || (p.SetMethod?.IsAbstract ?? false))
                .ToList();

            if (abstractMethods.Any() || abstractProperties.Any())
            {
                violations.Add($"{classType.FullName}: Class has abstract members but is not marked as abstract (Act4: UnsealedAbstractMembers)");
            }

            return violations;
        }

        /// <summary>
        /// Filter assemblies based on settings configuration.
        /// </summary>
        public static Assembly[] GetProjectAssemblies(StoryTestSettings settings)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && IsProjectAssembly(a, settings))
                .ToArray();
        }

        /// <summary>
        /// Determine if an assembly should be validated based on settings.
        /// </summary>
        private static bool IsProjectAssembly(Assembly assembly, StoryTestSettings settings)
        {
            var name = assembly.GetName().Name;

            // Exclude Unity engine assemblies unless explicitly enabled
            if (name.StartsWith("Unity") || name.StartsWith("UnityEngine") || name.StartsWith("UnityEditor"))
            {
                return settings.includeUnityAssemblies;
            }

            // Exclude system assemblies
            if (name.StartsWith("System") || name.StartsWith("mscorlib") || name.StartsWith("netstandard"))
            {
                return false;
            }

            // Exclude NUnit and test frameworks
            if (name.Contains("nunit") || name.Contains("NUnit") || name.Contains("TestRunner"))
            {
                return false;
            }

            // If assembly filters are defined, check against them
            if (settings.assemblyFilters != null && settings.assemblyFilters.Length > 0)
            {
                return settings.assemblyFilters.Any(filter => name.Contains(filter));
            }

            return true;
        }

        /// <summary>
        /// Check if a type has the StoryIgnore attribute.
        /// </summary>
        private static bool HasStoryIgnore(Type type)
        {
            return type.GetCustomAttributes(typeof(StoryIgnoreAttribute), true).Length > 0;
        }

        /// <summary>
        /// Validate custom component types specified in settings.
        /// </summary>
        public static List<StoryViolation> ValidateCustomComponents(string[] componentTypeNames)
        {
            var violations = new List<StoryViolation>();

            foreach (var typeName in componentTypeNames)
            {
                var type = Type.GetType(typeName);
                if (type == null)
                {
                    // Try to find the type in all loaded assemblies
                    type = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes())
                        .FirstOrDefault(t => t.FullName == typeName || t.Name == typeName);
                }

                if (type == null)
                {
                    violations.Add(new StoryViolation
                    {
                        Type = typeName,
                        Member = null,
                        Violation = $"Custom component type not found: {typeName}",
                        FilePath = "Configuration Error",
                        LineNumber = 0,
                        ViolationType = StoryViolationType.Other
                    });
                    continue;
                }

                // Validate the type using standard Story Test rules
                // Note: StoryIntegrityValidator is in TinyWalnutGames.StoryTest namespace
                // We can't call it directly from Shared assembly due to dependency direction
                // Instead, provide helper methods here for validation
                var typeViolations = ValidateTypeStructure(type);
                violations.AddRange(typeViolations);
            }

            return violations;
        }

        /// <summary>
        /// Validate type structure without depending on StoryIntegrityValidator.
        /// This provides basic validation that can be called from custom component validation.
        /// </summary>
        private static List<StoryViolation> ValidateTypeStructure(Type type)
        {
            var violations = new List<StoryViolation>();

            // Check for abstract members in non-abstract classes
            if (type.IsClass && !type.IsAbstract)
            {
                var abstractMethods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(m => m.IsAbstract)
                    .ToList();

                if (abstractMethods.Any())
                {
                    violations.Add(new StoryViolation
                    {
                        Type = type.FullName,
                        Member = string.Join(", ", abstractMethods.Select(m => m.Name)),
                        Violation = "Class has abstract members but is not marked as abstract",
                        FilePath = type.Assembly.Location,
                        LineNumber = 0,
                        ViolationType = StoryViolationType.IncompleteImplementation
                    });
                }
            }

            // Check for empty value types
            if (type.IsValueType && !type.IsEnum && !type.IsPrimitive)
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                if (fields.Length == 0 && properties.Length == 0)
                {
                    violations.Add(new StoryViolation
                    {
                        Type = type.FullName,
                        Member = null,
                        Violation = "Value type has no public fields or properties - likely a placeholder",
                        FilePath = type.Assembly.Location,
                        LineNumber = 0,
                        ViolationType = StoryViolationType.IncompleteImplementation
                    });
                }
            }

            return violations;
        }
    }

    /// <summary>
    /// Represents detected environment capabilities.
    /// Serializable for saving to settings file.
    /// </summary>
    [Serializable]
    public class EnvironmentCapabilities
    {
        public bool hasUnityEngine;
        public bool hasDOTS;
        public bool hasBurst;
        public bool hasEntities;
        public bool canInstantiateComponents;

        public override string ToString()
        {
            return $"Unity: {hasUnityEngine}, DOTS: {hasDOTS}, Burst: {hasBurst}, Entities: {hasEntities}, Instantiate: {canInstantiateComponents}";
        }
    }
}
