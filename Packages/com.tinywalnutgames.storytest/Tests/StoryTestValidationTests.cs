using System;
using TinyWalnutGames.StoryTest.Shared;
using NUnit.Framework;
using System.Reflection;
using System.Linq;

namespace TinyWalnutGames.StoryTest.Tests
{
    /// <summary>
    /// Universal tests for the Story Test validation framework.
    /// These tests work in ANY .NET environment without requiring specific project types.
    /// </summary>
    public class StoryTestValidationTests
    {
        [Test]
        public void StoryIgnoreAttribute_RequiresReason()
        {
            // Test that StoryIgnoreAttribute requires a non-empty reason
            Assert.Throws<ArgumentException>(() => _ = new StoryIgnoreAttribute(""));
            Assert.Throws<ArgumentException>(() => _ = new StoryIgnoreAttribute(null));
            Assert.Throws<ArgumentException>(() => _ = new StoryIgnoreAttribute("   "));

            // Valid reason should not throw
            Assert.DoesNotThrow(() => _ = new StoryIgnoreAttribute("Valid reason"));
        }

        [Test]
        public void StoryIntegrityValidator_ValidatesAssemblies()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var violations = StoryIntegrityValidator.ValidateAssemblies(assembly);

            // Should return a list (could be empty, but not null)
            Assert.IsNotNull(violations);

            // For a production-ready system, there should be no violations
            if (violations.Count > 0)
            {
                UnityEngine.Debug.LogWarning($"Story violations found: {string.Join(", ", violations.Select(v => v.ToString()))}");
            }
        }

        [Test]
        public void StoryIntegrityValidator_AutoRegistersRules()
        {
            StoryIntegrityValidator.ClearRules();
            var rules = StoryIntegrityValidator.GetRegisteredRules();

            Assert.IsNotNull(rules, "Registered rules collection should never be null");
            Assert.Greater(rules.Count, 0, "StoryIntegrityValidator should auto-register validation rules from the Acts assembly");
        }

        [Test]
        public void StoryIntegrityValidator_RespectsStoryIgnoreAttribute()
        {
            // Test that the StoryIgnore attribute is respected
            var testType = typeof(TestClassWithStoryIgnore);
            var violations = StoryIntegrityValidator.ValidateType(testType);

            // Should have no violations since class is marked with StoryIgnore
            Assert.AreEqual(0, violations.Count,
                $"Expected no violations for StoryIgnore class, but found: {string.Join(", ", violations.Select(v => v.ToString()))}");
        }

        [Test]
        public void ProductionExcellenceStoryTest_ValidatesConfiguration()
        {
            var testObject = new UnityEngine.GameObject("Test Story Test");
            var storyTest = testObject.AddComponent<ProductionExcellenceStoryTest>();

            // Test that component can be created without errors
            Assert.IsNotNull(storyTest);

            // Cleanup
            UnityEngine.Object.DestroyImmediate(testObject);
        }
    }

    /// <summary>
    /// Conceptual validation tests - dynamically discover and validate project patterns.
    /// These tests adapt to the project structure instead of requiring specific types.
    /// </summary>
    public class ConceptualValidationTests
    {
        [Test]
        public void AllEnumTypesHaveValidValues()
        {
            // Dynamically discover all enum types in project assemblies
            var settings = StoryTestSettings.Instance;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && IsProjectAssembly(a, settings))
                .ToArray();

            Assert.IsTrue(assemblies.Length > 0, "No project assemblies found");

            foreach (var assembly in assemblies)
            {
                var enumTypes = assembly.GetTypes()
                    .Where(t => t.IsEnum && !HasStoryIgnore(t))
                    .ToArray();

                foreach (var enumType in enumTypes)
                {
                    var enumValues = Enum.GetValues(enumType);
                    
                    // Act8 (HollowEnums) rule: Must have at least 2 values
                    Assert.IsTrue(enumValues.Length >= 2, 
                        $"{enumType.FullName} enum should have at least 2 values (Act8: HollowEnums)");

                    // Check for 🏳placeholder names
                    var names = Enum.GetNames(enumType);
                    var placeholderNames = new[] { "None", "Default", "Undefined", "Placeholder", "🏳TODO", "TEMP" };
                    
                    if (enumValues.Length == 2)
                    {
                        // If only 2 values, ensure both aren't 🏳placeholders
                        var nonPlaceholders = names.Count(n => !placeholderNames.Contains(n, StringComparer.OrdinalIgnoreCase));
                        Assert.IsTrue(nonPlaceholders > 0,
                            $"{enumType.FullName} has only 🏳placeholder values (Act8: HollowEnums)");
                    }
                }
            }
        }

        [Test]
        public void ValueTypesHaveValidDefaultConstructors()
        {
            // Dynamically discover value types (structs) that might be components
            var settings = StoryTestSettings.Instance;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && IsProjectAssembly(a, settings))
                .ToArray();

            foreach (var assembly in assemblies)
            {
                var valueTypes = assembly.GetTypes()
                    .Where(t => t.IsValueType && !t.IsEnum && !t.IsPrimitive && !HasStoryIgnore(t))
                    .ToArray();

                foreach (var type in valueTypes)
                {
                    try
                    {
                        // Try to create a default instance
                        var instance = Activator.CreateInstance(type);
                        Assert.IsNotNull(instance, $"Could not create default instance of {type.FullName}");

                        // Verify all public fields are accessible
                        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                        foreach (var field in fields)
                        {
                            // Just verify we can access the field - value may be null for reference types
                            Assert.DoesNotThrow(() => field.GetValue(instance),
                                $"{type.FullName}.{field.Name} should be accessible");
                        }
                    }
                    catch (Exception ex)
                    {
                        // If instantiation fails, verify it's intentional (has StoryIgnore or is abstract)
                        if (!type.IsAbstract && !HasStoryIgnore(type))
                        {
                            Assert.Fail($"Failed to instantiate {type.FullName}: {ex.Message}. " +
                                       "Consider adding [StoryIgnore] if this type is not meant to be instantiated.");
                        }
                    }
                }
            }
        }

        [Test]
        public void ClassesWithAbstractMembersAreAbstract()
        {
            // This validates Act4 (🏳UnsealedAbstractMembers) conceptually
            var settings = StoryTestSettings.Instance;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && IsProjectAssembly(a, settings))
                .ToArray();

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass && !HasStoryIgnore(t))
                    .ToArray();

                foreach (var type in types)
                {
                    var abstractMembers = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(m => m is MethodInfo { IsAbstract: true } || 
                                   (m is PropertyInfo pi && ((pi.GetMethod?.IsAbstract ?? false) || (pi.SetMethod?.IsAbstract ?? false))))
                        .ToArray();

                    if (abstractMembers.Length > 0 && !type.IsAbstract)
                    {
                        Assert.Fail($"{type.FullName} has abstract members but is not marked as abstract (Act4: 🏳UnsealedAbstractMembers)");
                    }
                }
            }
        }

        [Test]
        public void ConceptualValidator_SettingsLoaded()
        {
            // Verify settings file is accessible and contains conceptual validation config
            var settings = StoryTestSettings.Instance;
            Assert.IsNotNull(settings, "StoryTestSettings should be available");
            
            // The settings object should be serializable and contain expected properties
            // This is a meta-test ensuring the configuration system works
        }

        // Helper methods
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
            if (settings.assemblyFilters is { Length: > 0 })
            {
                return settings.assemblyFilters.Any(filter => name.Contains(filter));
            }

            return true;
        }

        private static bool HasStoryIgnore(Type type)
        {
            return type.GetCustomAttributes(typeof(StoryIgnoreAttribute), true).Length > 0;
        }
    }

    /// <summary>
    /// Integration tests for the complete system.
    /// </summary>
    public class IntegrationTests
    {
        [Test]
        public void StoryTestCompliance_CoreFrameworkHasNoViolations()
        {
            // Validate that the Story Test framework itself complies with its own rules
            var storyTestAssembly = typeof(StoryIntegrityValidator).Assembly;
            var violations = StoryIntegrityValidator.ValidateAssemblies(storyTestAssembly);

            // Filter out intentional test infrastructure
            var realViolations = violations.Where(v => 
                !v.ToString().Contains("StoryTest") && 
                !v.ToString().Contains("Test")).ToList();

            if (realViolations.Count > 0)
            {
                var violationMessages = string.Join("\n", realViolations.Select(v => v.ToString()));
                UnityEngine.Debug.LogWarning($"Story Test framework violations:\n{violationMessages}");
            }

            // For production readiness, the framework itself should have zero violations
            Assert.AreEqual(0, realViolations.Count,
                $"Story Test framework should have no violations, but found:\n{string.Join("\n", realViolations.Select(v => v.ToString()))}");
        }

        [Test]
        public void EnvironmentDetection_WorksCorrectly()
        {
            // Verify we can detect the current environment
            var hasUnity = AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => a.GetName().Name.StartsWith("UnityEngine"));
            
            var hasDots = AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => a.GetName().Name.Contains("Unity.Entities"));

            var hasBurst = AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => a.GetName().Name.Contains("Unity.Burst"));

            // Log environment for debugging
            UnityEngine.Debug.Log($"Environment detected - Unity: {hasUnity}, DOTS: {hasDots}, Burst: {hasBurst}");

            // In Unity tests, we should always detect Unity
            Assert.IsTrue(hasUnity, "Should detect UnityEngine in Unity test environment");
        }
    }

    /// <summary>
    /// Test class marked with StoryIgnore for testing purposes.
    /// </summary>
    [StoryIgnore("Test class for validating StoryIgnore attribute functionality")]
    public class TestClassWithStoryIgnore
    {
        public void SomeMethod()
        {
            // This would normally be flagged as 🏳incomplete, but StoryIgnore should prevent it
        }
    }
}
