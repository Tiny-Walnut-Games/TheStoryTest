using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using TinyWalnutGames.StoryTest;
using TinyWalnutGames.StoryTest.Shared;

// This script runs on load in the Unity Editor and registers all validation rules from Acts via reflection.
[InitializeOnLoad]
public static class StoryTestRuleBootstrapper
{
    static StoryTestRuleBootstrapper()
    {
        StoryIntegrityValidator.ClearRules();

        // Find the Acts assembly
        var actsAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name.Contains("TinyWalnutGames.StoryTest.Acts"));
        if (actsAssembly == null)
            return;

        // Find all public static fields of type ValidationRule
    var ruleType = typeof(ValidationRule);
        var rules = actsAssembly.GetTypes()
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(f => ruleType.IsAssignableFrom(f.FieldType))
            .Select(f => f.GetValue(null) as ValidationRule)
            .Where(r => r != null)
            .ToList();

        foreach (var rule in rules)
        {
            StoryIntegrityValidator.RegisterRule(rule);
        }
    }
}