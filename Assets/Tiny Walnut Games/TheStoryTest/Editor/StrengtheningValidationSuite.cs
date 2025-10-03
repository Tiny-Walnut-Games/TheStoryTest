using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TinyWalnutGames.StoryTest;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Editor
{
    /// <summary>
    /// Editor suite for running comprehensive strengthening validation pipeline.
    /// Provides menu access to >95% validation tooling.
    /// </summary>
    [StoryIgnore("Editor tooling for story test infrastructure")]
    public static class StrengtheningValidationSuite
    {
        private const string MenuRoot = "Toxicity/Story Test/";

        /// <summary>
        /// Runs the complete strengthening validation pipeline.
        /// </summary>
        [MenuItem(MenuRoot + "Run Complete Validation Pipeline", false, 1)]
        public static void RunCompleteValidationPipeline()
        {
            if (!EditorApplication.isPlaying)
            {
                if (EditorUtility.DisplayDialog("Validation Pipeline",
                    "The validation pipeline requires Play Mode to run properly. Enter Play Mode now?",
                    "Yes", "Cancel"))
                {
                    EditorApplication.isPlaying = true;
                    EditorApplication.playModeStateChanged += OnPlayModeChanged;
                }
                return;
            }

            RunValidation();
        }

        /// <summary>
        /// Runs story integrity validation only.
        /// </summary>
        [MenuItem(MenuRoot + "Validate Story Integrity", false, 2)]
        public static void ValidateStoryIntegrity()
        {
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.Contains("Toxicity"))
                .ToArray();

            var violations = StoryIntegrityValidator.ValidateAssemblies(assemblies);

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
        /// Generates a detailed validation report.
        /// </summary>
        [MenuItem(MenuRoot + "Generate Detailed Report", false, 11)]
        public static void GenerateDetailedReport()
        {
            var reportPath = EditorUtility.SaveFilePanel(
                "Save Validation Report",
                Application.dataPath,
                $"ToxicityValidationReport_{System.DateTime.Now:yyyyMMdd_HHmmss}",
                "txt");

            if (string.IsNullOrEmpty(reportPath))
                return;

            var report = GenerateReport();
            System.IO.File.WriteAllText(reportPath, report);

            Debug.Log($"Validation report saved to: {reportPath}");
            EditorUtility.DisplayDialog("Report Generated",
                $"Detailed validation report has been saved to:\n{reportPath}",
                "OK");
        }

        /// <summary>
        /// Shows strengthening configuration window.
        /// </summary>
        [MenuItem(MenuRoot + "Strengthening Configuration", false, 21)]
        public static void ShowStrengtheningConfiguration()
        {
            StrengtheningConfigurationWindow.ShowWindow();
        }

        /// <summary>
        /// Validates project structure and organization.
        /// </summary>
        [MenuItem(MenuRoot + "Validate Project Structure", false, 31)]
        public static void ValidateProjectStructure()
        {
            var issues = new List<string>();

            // Check for proper assembly organization
            var assemblies = System.Reflection.Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            if (!assemblies.Any(a => a.Name.Contains("Unity.Entities")))
            {
                issues.Add("DOTS/ECS assemblies not properly referenced");
            }

            // Check for proper folder structure
            var requiredFolders = new[]
            {
                "Assets/Scripts",
                "Assets/Resources",
                "Assets/Scenes"
            };

            foreach (var folder in requiredFolders)
            {
                if (!System.IO.Directory.Exists(folder))
                {
                    issues.Add($"Required folder missing: {folder}");
                }
            }

            // Check for WebGL compatibility
            var buildSettings = EditorUserBuildSettings.activeBuildTarget;
            if (buildSettings != BuildTarget.WebGL)
            {
                issues.Add($"Build target should be WebGL for production, currently: {buildSettings}");
            }

            if (issues.Any())
            {
                Debug.LogWarning("Project Structure Issues:");
                foreach (var issue in issues)
                {
                    Debug.LogWarning($"  • {issue}");
                }
            }
            else
            {
                Debug.Log("✅ Project Structure Validation PASSED");
            }
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;
                // Delay to allow scene to initialize
                EditorApplication.delayCall += () => RunValidation();
            }
        }

        private static void RunValidation()
        {
            var testObject = Object.FindFirstObjectByType<ProductionExcellenceStoryTest>();
            if (testObject == null)
            {
                // Create temporary test object
                var tempGO = new GameObject("TempValidationRunner");
                testObject = tempGO.AddComponent<ProductionExcellenceStoryTest>();

                testObject.OnValidationComplete += (report) =>
                {
                    ShowValidationResults(report);
                    Object.DestroyImmediate(tempGO);
                };
            }

            testObject.ValidateOnDemand();
        }

        private static void ShowValidationResults(ValidationReport report)
        {
            var message = report.GenerateSummary();
            var title = report.IsFullyCompliant ? "Validation Passed!" : "Validation Issues Found";

            Debug.Log(message);

            if (report.IsFullyCompliant)
            {
                EditorUtility.DisplayDialog(title,
                    $"🎉 Production Excellence Validation PASSED!\n\nScore: {report.ProductionReadinessScore:F1}%\nDuration: {report.Duration.TotalSeconds:F2}s",
                    "Excellent!");
            }
            else
            {
                EditorUtility.DisplayDialog(title,
                    $"⚠️ Validation found issues that need attention.\n\nScore: {report.ProductionReadinessScore:F1}%\nSee console for details.",
                    "Fix Issues");
            }
        }

        private static void ShowValidationWindow(List<StoryViolation> violations)
        {
            StoryValidationWindow.ShowWindow(violations);
        }

        private static string GenerateReport()
        {
            var report = "TOXICITY FRAMEWORK STRENGTHENING VALIDATION REPORT\n";
            report += new string('=', 50) + "\n\n";
            report += $"Generated: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
            report += $"Unity Version: {Application.unityVersion}\n";
            report += $"Target Platform: {EditorUserBuildSettings.activeBuildTarget}\n\n";

            // Story integrity analysis
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.Contains("Toxicity"))
                .ToArray();

            var violations = StoryIntegrityValidator.ValidateAssemblies(assemblies);

            report += "STORY INTEGRITY ANALYSIS\n";
            report += new string('-', 25) + "\n";
            if (violations.Any())
            {
                report += $"Status: FAILED ({violations.Count} violations)\n\n";
                report += "Violations:\n";
                foreach (var violation in violations)
                {
                    report += $"  • {violation}\n";
                }
            }
            else
            {
                report += "Status: PASSED\n";
                report += "No story integrity violations found.\n";
            }
            report += "\n";

            // Assembly analysis
            report += "ASSEMBLY ANALYSIS\n";
            report += new string('-', 17) + "\n";
            foreach (var assembly in assemblies)
            {
                report += $"Assembly: {assembly.FullName}\n";
                report += $"Types: {assembly.GetTypes().Length}\n";
                report += $"Location: {assembly.Location}\n\n";
            }

            // Project structure analysis
            report += "PROJECT STRUCTURE\n";
            report += new string('-', 17) + "\n";
            report += "Folder Structure:\n";
            var directories = System.IO.Directory.GetDirectories("Assets", "*", System.IO.SearchOption.AllDirectories);
            foreach (var dir in directories.Take(20))
            {
                report += $"  {dir}\n";
            }
            if (directories.Length > 20)
            {
                report += $"  ... and {directories.Length - 20} more directories\n";
            }

            return report;
        }
    }

    /// <summary>
    /// Window for configuring strengthening validation parameters.
    /// </summary>
    public class StrengtheningConfigurationWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private bool validateOnBuild = true;
        private bool strictMode = false;
        private string[] assemblyFilters = { "Toxicity" };

        public static void ShowWindow()
        {
            var window = GetWindow<StrengtheningConfigurationWindow>("Strengthening Config");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Strengthening Validation Configuration", EditorStyles.largeLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Validation Settings", EditorStyles.boldLabel);
            validateOnBuild = EditorGUILayout.Toggle("Validate on Build", validateOnBuild);
            strictMode = EditorGUILayout.Toggle("Strict Mode", strictMode);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Assembly Filters", EditorStyles.boldLabel);

            if (assemblyFilters != null)
            {
                for (int i = 0; i < assemblyFilters.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    assemblyFilters[i] = EditorGUILayout.TextField($"Filter {i + 1}", assemblyFilters[i]);
                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        var newFilters = new string[assemblyFilters.Length - 1];
                        System.Array.Copy(assemblyFilters, 0, newFilters, 0, i);
                        System.Array.Copy(assemblyFilters, i + 1, newFilters, i, assemblyFilters.Length - i - 1);
                        assemblyFilters = newFilters;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (GUILayout.Button("Add Filter"))
            {
                var newFilters = new string[assemblyFilters?.Length + 1 ?? 1];
                if (assemblyFilters != null)
                {
                    System.Array.Copy(assemblyFilters, newFilters, assemblyFilters.Length);
                }
                newFilters[newFilters.Length - 1] = "";
                assemblyFilters = newFilters;
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Apply Configuration"))
            {
                ApplyConfiguration();
            }

            if (GUILayout.Button("Reset to Defaults"))
            {
                ResetToDefaults();
            }

            EditorGUILayout.EndScrollView();
        }

        private void ApplyConfiguration()
        {
            EditorPrefs.SetBool("Toxicity.ValidateOnBuild", validateOnBuild);
            EditorPrefs.SetBool("Toxicity.StrictMode", strictMode);

            if (assemblyFilters != null)
            {
                for (int i = 0; i < assemblyFilters.Length; i++)
                {
                    EditorPrefs.SetString($"Toxicity.AssemblyFilter.{i}", assemblyFilters[i]);
                }
                EditorPrefs.SetInt("Toxicity.AssemblyFilters.Count", assemblyFilters.Length);
            }

            Debug.Log("Strengthening configuration applied successfully.");
        }

        private void ResetToDefaults()
        {
            validateOnBuild = true;
            strictMode = false;
            assemblyFilters = new[] { "Toxicity" };
        }

        private void OnEnable()
        {
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            validateOnBuild = EditorPrefs.GetBool("Toxicity.ValidateOnBuild", true);
            strictMode = EditorPrefs.GetBool("Toxicity.StrictMode", false);

            var filterCount = EditorPrefs.GetInt("Toxicity.AssemblyFilters.Count", 1);
            assemblyFilters = new string[filterCount];
            for (int i = 0; i < filterCount; i++)
            {
                assemblyFilters[i] = EditorPrefs.GetString($"Toxicity.AssemblyFilter.{i}", i == 0 ? "Toxicity" : "");
            }
        }
    }

    /// <summary>
    /// Window for displaying story validation results.
    /// </summary>
    public class StoryValidationWindow : EditorWindow
    {
        private List<StoryViolation> violations;
        private Vector2 scrollPosition;

        public static void ShowWindow(List<StoryViolation> violations)
        {
            var window = GetWindow<StoryValidationWindow>("Story Violations");
            window.violations = violations;
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        private void OnGUI()
        {
            if (violations == null || violations.Count == 0)
            {
                GUILayout.Label("No violations found.", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            GUILayout.Label($"Story Integrity Violations ({violations.Count})", EditorStyles.largeLabel);
            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (var violation in violations)
            {
                EditorGUILayout.BeginVertical("Box");

                var style = new GUIStyle(EditorStyles.label);
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = Color.red;

                GUILayout.Label(violation.ViolationType.ToString(), style);
                GUILayout.Label($"Member: {violation.Type}.{violation.Member}", EditorStyles.wordWrappedLabel);
                GUILayout.Label($"Issue: {violation.Violation}", EditorStyles.wordWrappedLabel);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();
        }
    }
}