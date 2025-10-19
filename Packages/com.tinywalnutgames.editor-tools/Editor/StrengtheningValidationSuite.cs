using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TinyWalnutGames.TheStoryTest.Shared;
using TinyWalnutGames.TheStoryTest;
using Object = UnityEngine.Object;

namespace TinyWalnutGames.EditorTools
{
    /// <summary>
    /// Editor suite for running the comprehensive strengthening validation pipeline.
    /// Provides menu access to >95% validation tooling.
    /// </summary>
    [StoryIgnore("Editor tooling for story test infrastructure")]
    public static class StrengtheningValidationSuite
    {
        // MenuItem attributes require const, so we use a default that can be configured via StoryTestSettings
        private const string MenuRoot = "Tiny Walnut Games/The Story Test/";
        
        /// <summary>
        /// Gets the configured menu path from settings. Falls back to MenuRoot if not configured.
        /// Used for runtime logging, display messages, and debugging output.
        /// </summary>
        private static string GetMenuPath() => StoryTestSettings.Instance.menuPath ?? MenuRoot;

        /// <summary>
        /// Adds a Story Test component to the current scene for manual validation.
        /// </summary>
        [MenuItem(MenuRoot + "Add Story Test Component to Scene", false, 1)]
        public static void AddStoryTestComponentToScene()
        {
            if (!Application.isPlaying)
            {
                var go = new GameObject("Story Test Runner");
                var component = go.AddComponent<ProductionExcellenceStoryTest>();

                // Configure with sensible defaults
                component.enableStoryIntegrity = true;
                component.enableConceptualValidation = true;
                component.enableArchitecturalCompliance = true;
                component.enableSyncPointPerformance = true;
                component.enableCodeCoverage = false;
                component.validateAutomaticallyOnStart = false;
                component.stopOnFirstViolation = false;
                component.exportReport = true;

                Selection.activeGameObject = go;
                EditorGUIUtility.PingObject(go);

                Debug.Log("[Story Test] Added Story Test Runner to scene. Enter PlayMode, then click 'Run Validation' in the Inspector.");
            }
            else
            {
                Debug.LogWarning("[Story Test] Cannot add component during PlayMode. Exit PlayMode first.");
            }
        }

        [MenuItem(MenuRoot + "Add Story Test Component to Scene", true)]
        public static bool ValidateAddStoryTestComponentToScene()
        {
            return !Application.isPlaying;
        }

        /// <summary>
        /// Triggers validation on the Story Test component in the scene (PlayMode only).
        /// </summary>
        [MenuItem(MenuRoot + "Run Validation (PlayMode)", false, 3)]
        public static void RunValidationInPlayMode()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[Story Test] This menu option only works during PlayMode");
                return;
            }

            var runner = Object.FindFirstObjectByType<ProductionExcellenceStoryTest>();
            if (runner is null)
            {
                EditorUtility.DisplayDialog("Story Test",
                    "No Story Test Runner found in scene.\n\nUse 'Add Story Test Component to Scene' first.",
                    "OK");
                return;
            }

            if (runner.IsValidating)
            {
                Debug.LogWarning("[Story Test] Validation already running");
                return;
            }

            runner.ValidateOnDemand();
            Debug.Log("[Story Test] Validation triggered from menu during PlayMode");
        }

        [MenuItem(MenuRoot + "Run Validation (PlayMode)", true)]
        public static bool ValidateRunValidationInPlayMode()
        {
            return Application.isPlaying;
        }

        /// <summary>
        /// Runs story integrity validation only.
        /// </summary>
        [MenuItem(MenuRoot + "Validate Story Integrity", false, 10)]
        public static void ValidateStoryIntegrity()
        {
            Debug.Log($"[{GetMenuPath()}] Running Story Integrity Validation...");

            // Ensure validation rules are loaded (lazy-load to avoid blocking domain reload)
            StoryTestRuleBootstrapper.EnsureBootstrapped();

            // Use settings to get properly filtered assemblies - respects assemblyFilters and includeUnityAssemblies
            var settings = StoryTestSettings.Instance;
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Let StoryIntegrityValidator handle filtering according to settings
            var assemblies = allAssemblies
                .Where(a => StoryIntegrityValidator.IsProjectAssembly(a, settings))
                .ToArray();

            Debug.Log($"Running Story Integrity validation on {assemblies.Length} assemblies (filtered by settings):");
            foreach (var assembly in assemblies)
            {
                Debug.Log($"  • {assembly.GetName().Name}");
            }

            Debug.Log("\n📋 Test Naming Convention Check:");
            Debug.Log("   Tests should use descriptive names that clearly explain what is being tested.");
            Debug.Log("   Avoid using issue tracker prefixes (like 'ISSUE-2') as test names.");
            Debug.Log("   Good examples: 'TestPlayerMovement', 'ValidateInventorySystem'");
            Debug.Log("   Bad examples: 'ISSUE-2', 'BUG-123'\n");

            // Run the actual Story Integrity validation with filtered assemblies
            var violations = StoryIntegrityValidator.ValidateAssemblies(assemblies);

            // Validate test naming conventions
            ValidateTestNamingConventions(violations, assemblies);

            if (violations.Any())
            {
                Debug.LogWarning($"Story Integrity Validation found {violations.Count} violations:");
                foreach (var violation in violations)
                {
                    Debug.LogWarning($"  • {violation}");
                }

                ShowValidationWindow(violations);
            }
            else
            {
                Debug.Log("✅ Story Integrity Validation PASSED - No violations found!");
                EditorUtility.DisplayDialog("Story Integrity",
                    "Story Integrity Validation PASSED!\n\nNo violations found in the codebase.",
                    "OK");
            }
        }

        /// <summary>
        /// Validates test naming conventions, specifically checking for ISSUE-prefixes.
        /// </summary>
        private static void ValidateTestNamingConventions(List<StoryViolation> violations, System.Reflection.Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        // Check if the type name starts with ISSUE-
                        if (type.Name.StartsWith("ISSUE-", StringComparison.OrdinalIgnoreCase))
                        {
                            violations.Add(new StoryViolation
                            {
                                ViolationType = StoryViolationType.NamingConvention,
                                Type = type.FullName ?? type.Name,
                                Member = type.Name,
                                Violation = "Test class names should not start with 'ISSUE-' prefix. Use descriptive names that explain what is being tested.",
                                FilePath = string.Empty,
                                LineNumber = 0
                            });
                        }

                        // Check methods for ISSUE-prefix
                        var methods = type.GetMethods(System.Reflection.BindingFlags.Public |
                                                     System.Reflection.BindingFlags.NonPublic |
                                                     System.Reflection.BindingFlags.Instance |
                                                     System.Reflection.BindingFlags.Static);
                        foreach (var method in methods)
                        {
                            if (method.Name.StartsWith("ISSUE-", StringComparison.OrdinalIgnoreCase))
                            {
                                violations.Add(new StoryViolation
                                {
                                    ViolationType = StoryViolationType.NamingConvention,
                                    Type = type.FullName ?? type.Name,
                                    Member = method.Name,
                                    Violation = "Test method names should not start with 'ISSUE-' prefix. Use descriptive names that explain what is being tested.",
                                    FilePath = string.Empty,
                                    LineNumber = 0
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Could not validate types in assembly {assembly.GetName().Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Generates a detailed validation report.
        /// </summary>
        [MenuItem(MenuRoot + "Generate Detailed Report", false, 11)]
        public static void GenerateDetailedReport()
        {
            Debug.Log($"[{GetMenuPath()}] Generating detailed validation report...");

            // Ensure validation rules are loaded (lazy-load to avoid blocking domain reload)
            StoryTestRuleBootstrapper.EnsureBootstrapped();

            var reportPath = EditorUtility.SaveFilePanel(
                "Save Validation Report",
                // we should get the path from StoryTestSettings instead of Application.dataPath
                StoryTestSettings.Instance.exportPath,
                $"StoryTestValidationReport_{DateTime.Now:yyyyMMdd_HHmmss}",
                "txt");

            if (string.IsNullOrEmpty(reportPath))
                return;

            var report = GenerateReport();
            File.WriteAllText(reportPath, report);

            Debug.Log($"Validation report saved to: {reportPath}");
            EditorUtility.DisplayDialog("Report Generated",
                $"Detailed validation report has been saved to:\n{reportPath}",
                "OK");
        }

        /// <summary>
        /// Shows the strengthening configuration window.
        /// </summary>
        [MenuItem(MenuRoot + "Strengthening Configuration", false, 21)]
        public static void ShowStrengtheningConfiguration()
        {
            StrengtheningConfigurationWindow.ShowWindow();
        }

        /// <summary>
        /// Validates project structure and organization based on StoryTestSettings configuration.
        /// </summary>
        [MenuItem(MenuRoot + "Validate Project Structure", false, 31)]
        public static void ValidateProjectStructure()
        {
            Debug.Log($"[{GetMenuPath()}] Validating project structure...");
            
            var config = StoryTestSettings.Instance.projectStructure;
            var issues = new List<string>();

            ValidateRequiredFolders(config, issues);
            var missingOptionalFolders = ValidateOptionalFolders(config);
            ValidateAssemblyReferences(config, issues);
            ValidateBuildTarget(config, issues);

            DisplayValidationResults(issues, missingOptionalFolders);
        }

        private static void ValidateRequiredFolders(ProjectStructureConfig config, List<string> issues)
        {
            foreach (var folder in config.requiredFolders)
            {
                if (!Directory.Exists(folder))
                {
                    issues.Add($"Required folder missing: {folder}");
                }
            }
        }

        private static List<string> ValidateOptionalFolders(ProjectStructureConfig config)
        {
            var missingOptionalFolders = new List<string>();
            foreach (var folder in config.optionalFolders)
            {
                if (!Directory.Exists(folder))
                {
                    missingOptionalFolders.Add(folder);
                }
            }
            return missingOptionalFolders;
        }

        private static void ValidateAssemblyReferences(ProjectStructureConfig config, List<string> issues)
        {
            if (!config.validateAssemblyReferences || config.requiredAssemblyReferences.Length == 0)
                return;

            var assemblies = System.Reflection.Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            foreach (var requiredRef in config.requiredAssemblyReferences)
            {
                if (!assemblies.Any(a => a.Name.Contains(requiredRef)))
                {
                    issues.Add($"Required assembly reference missing: {requiredRef}");
                }
            }
        }

        private static void ValidateBuildTarget(ProjectStructureConfig config, List<string> issues)
        {
            if (string.IsNullOrEmpty(config.expectedBuildTarget))
                return;

            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            if (buildTarget.ToString() != config.expectedBuildTarget)
            {
                issues.Add($"Build target should be {config.expectedBuildTarget} for production, currently: {buildTarget}");
            }
        }

        private static void DisplayValidationResults(List<string> issues, List<string> missingOptionalFolders)
        {
            if (issues.Any())
            {
                Debug.LogWarning($"Project Structure Validation found {issues.Count} issues:");
                foreach (var issue in issues)
                {
                    Debug.LogWarning($"  • {issue}");
                }
            }
            else
            {
                Debug.Log("✅ Project Structure Validation PASSED - All required folders and references present!");
            }

            if (missingOptionalFolders.Count > 0)
            {
                Debug.Log($"ℹ️  Optional folders not found ({missingOptionalFolders.Count}):");
                foreach (var folder in missingOptionalFolders)
                {
                    Debug.Log($"  • {folder} (optional)");
                }
            }
        }

        /// <summary>
        /// Generates a validation report containing details about the project structure and validation results.
        /// </summary>
        private static string GenerateReport()
        {
            var report = new System.Text.StringBuilder();

            report.AppendLine("=== Story Test Validation Report ===");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();

            // Settings summary
            var settings = StoryTestSettings.Instance;
            report.AppendLine("--- Configuration ---");
            report.AppendLine($"Project: {settings.projectName}");
            report.AppendLine($"Menu Path: {settings.menuPath}");
            report.AppendLine($"Export Path: {settings.exportPath}");
            report.AppendLine($"Include Unity Assemblies: {settings.includeUnityAssemblies}");
            report.AppendLine($"Validate On Start: {settings.validateOnStart}");
            report.AppendLine($"Strict Mode: {settings.strictMode}");
            report.AppendLine();

            // Assemblies
            report.AppendLine("--- Loaded Assemblies ---");
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var projectAssemblies = allAssemblies
                .Where(a => StoryIntegrityValidator.IsProjectAssembly(a, settings))
                .ToArray();
            foreach (var assembly in projectAssemblies)
            {
                report.AppendLine($"  • {assembly.GetName().Name}");
            }
            report.AppendLine();

            // Project structure
            report.AppendLine("--- Project Structure ---");
            var config = settings.projectStructure;
            report.AppendLine("Required Folders:");
            foreach (var folder in config.requiredFolders)
            {
                var exists = Directory.Exists(folder);
                report.AppendLine($"  • {folder} - {(exists ? "✅ OK" : "❌ MISSING")}");
            }
            report.AppendLine();

            return report.ToString();
        }

        private static void ShowValidationWindow(List<StoryViolation> violations)
        {
            StoryValidationWindow.ShowWindow(violations);
        }
    }

    /// <summary>
    /// Window for displaying story validation results.
    /// </summary>
    public class StoryValidationWindow : EditorWindow
    {
        private List<StoryViolation> _violations;
        private Vector2 _scrollPosition;

        public static void ShowWindow(List<StoryViolation> violations)
        {
            var window = GetWindow<StoryValidationWindow>("Story Violations");
            window._violations = violations;
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        private void OnGUI()
        {
            if (_violations == null || _violations.Count == 0)
            {
                GUILayout.Label("No violations found.", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            GUILayout.Label($"Story Integrity Violations ({_violations.Count})", EditorStyles.largeLabel);
            EditorGUILayout.Space();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (var violation in _violations)
            {
                EditorGUILayout.BeginVertical("Box");

                var style = new GUIStyle(EditorStyles.label)
                {
                    fontStyle = FontStyle.Bold,
                    normal =
                    {
                        textColor = Color.red
                    }
                };

                GUILayout.Label(violation.ViolationType.ToString(), style);
                GUILayout.Label($"Member: {violation.Type}.{violation.Member}", EditorStyles.wordWrappedLabel);
                GUILayout.Label($"Issue: {violation.Violation}", EditorStyles.wordWrappedLabel);
                // Debugging requires file and violation line location for the best context
                GUILayout.Label($"File: {violation.FilePath}", EditorStyles.wordWrappedLabel);
                GUILayout.Label($"Line: {violation.LineNumber}", EditorStyles.wordWrappedLabel);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();
        }
    }
}