using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TinyWalnutGames.StoryTest.Shared;
using Object = UnityEngine.Object;

namespace TinyWalnutGames.StoryTest.Editor
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
                Debug.Log($"ℹ️ Optional folders not present ({missingOptionalFolders.Count}):");
                foreach (var folder in missingOptionalFolders)
                {
                    Debug.Log($"  • {folder}");
                }
            }
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode) return;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            Debug.LogWarning("🏳Complete validation pipeline not yet implemented");
        }

        public static void ShowValidationResults(ValidationReport report)
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
            var report = BuildReportHeader();
            
            var assemblies = GetFilteredAssemblies();
            var violations = StoryIntegrityValidator.ValidateAssemblies(assemblies);

            report += BuildStoryIntegritySection(violations);
            report += BuildAssemblyAnalysisSection(assemblies);
            report += BuildProjectStructureSection();

            return report;
        }

        private static string BuildReportHeader()
        {
            var header = "THE STORY-TEST FRAMEWORK STRENGTHENING VALIDATION REPORT\n";
            header += new string('=', 50) + "\n\n";
            header += $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
            header += $"Unity Version: {Application.unityVersion}\n";
            header += $"Target Platform: {EditorUserBuildSettings.activeBuildTarget}\n\n";
            return header;
        }

        private static System.Reflection.Assembly[] GetFilteredAssemblies()
        {
            var settings = StoryTestSettings.Instance;
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            return allAssemblies
                .Where(a => StoryIntegrityValidator.IsProjectAssembly(a, settings))
                .ToArray();
        }

        private static string BuildStoryIntegritySection(List<StoryViolation> violations)
        {
            var section = "STORY INTEGRITY ANALYSIS\n";
            section += new string('-', 25) + "\n";
            
            if (violations.Any())
            {
                section += $"Status: FAILED ({violations.Count} violations)\n\n";
                section += "Violations:\n";
                section += BuildViolationsList(violations);
            }
            else
            {
                section += "Status: PASSED\n";
                section += "No story integrity violations found.\n";
            }
            
            section += "\n";
            return section;
        }

        private static string BuildViolationsList(List<StoryViolation> violations)
        {
            var list = "";
            foreach (var violation in violations)
            {
                var file = string.IsNullOrWhiteSpace(violation.FilePath) ? "<unknown>" : violation.FilePath;
                var line = violation.LineNumber > 0 ? violation.LineNumber.ToString() : "?";
                var typeName = string.IsNullOrWhiteSpace(violation.Type) ? "<UnknownType>" : violation.Type;
                var member = string.IsNullOrWhiteSpace(violation.Member) ? "<UnknownMember>" : violation.Member;
                var message = string.IsNullOrWhiteSpace(violation.Violation) ? "<No details provided>" : violation.Violation;
                list += $"  • [{violation.ViolationType}] {typeName}.{member} - {message} (File: {file}, Line: {line})\n";
            }
            return list;
        }

        private static string BuildAssemblyAnalysisSection(System.Reflection.Assembly[] assemblies)
        {
            var section = "ASSEMBLY ANALYSIS\n";
            section += new string('-', 17) + "\n";
            
            foreach (var assembly in assemblies)
            {
                section += BuildAssemblyInfo(assembly);
            }
            
            return section;
        }

        private static string BuildAssemblyInfo(System.Reflection.Assembly assembly)
        {
            var info = $"Assembly: {assembly.FullName}\n";
            info += $"Types: {GetAssemblyTypeCount(assembly)}\n";
            info += $"Location: {GetAssemblyLocation(assembly)}\n\n";
            return info;
        }

        private static int GetAssemblyTypeCount(System.Reflection.Assembly assembly)
        {
            try 
            { 
                return assembly.GetTypes().Length; 
            } 
            catch 
            { 
                return -1; 
            }
        }

        private static string GetAssemblyLocation(System.Reflection.Assembly assembly)
        {
            try
            {
                if (!assembly.IsDynamic)
                {
                    return assembly.Location;
                }
                return "(dynamic assembly)";
            }
            catch (NotSupportedException)
            {
                return "(not supported)";
            }
            catch
            {
                return "(unavailable)";
            }
        }

        private static string BuildProjectStructureSection()
        {
            var section = "PROJECT STRUCTURE\n";
            section += new string('-', 17) + "\n";
            section += "Folder Structure:\n";
            
            var directories = GetProjectDirectories();
            section += FormatDirectoryList(directories);
            
            return section;
        }

        private static string[] GetProjectDirectories()
        {
            try 
            { 
                return Directory.GetDirectories("Assets", "*", SearchOption.AllDirectories); 
            }
            catch 
            { 
                return Array.Empty<string>(); 
            }
        }

        private static string FormatDirectoryList(string[] directories)
        {
            var list = "";
            foreach (var dir in directories.Take(20))
            {
                list += $"  {dir}\n";
            }
            
            if (directories.Length > 20)
            {
                list += $"  ... and {directories.Length - 20} more directories\n";
            }
            
            return list;
        }
    }

    /// <summary>
    /// Window for configuring strengthening validation parameters.
    /// </summary>
    public class StrengtheningConfigurationWindow : EditorWindow
    {
        private const string SettingsRelativePath = "Assets/Tiny Walnut Games/TheStoryTest/Resources/StoryTestSettings.json";

        private Vector2 _scrollPosition;
        private StoryTestSettings _editableSettings;
        private bool _settingsFileExists;
        private bool _showConceptualSection = true;
        private bool _showEnvironmentSection;
        private bool _showCustomComponentTypes;
        private bool _showEnumPatterns;

        public static void ShowWindow()
        {
            var window = GetWindow<StrengtheningConfigurationWindow>("Strengthening Config");
            window.minSize = new Vector2(480, 420);
            window.Show();
        }

        private void OnEnable()
        {
            ReloadSettings();
        }

        private void ReloadSettings()
        {
            StoryTestSettings.ReloadSettings();
            _editableSettings = CloneSettings(StoryTestSettings.Instance);
            EnsureDefaults(_editableSettings);
            _settingsFileExists = File.Exists(GetSettingsAbsolutePath());
        }

        private void OnGUI()
        {
            if (_editableSettings == null)
            {
                ReloadSettings();
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUILayout.Label("Strengthening Validation Configuration", EditorStyles.largeLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox($"Editing Story Test settings at: {SettingsRelativePath}", MessageType.Info);
            if (!_settingsFileExists)
            {
                EditorGUILayout.HelpBox("Settings file not found. A new StoryTestSettings.json will be created when you apply changes.", MessageType.Warning);
            }

            DrawGeneralSettings();
            DrawAssemblyFilters();
            DrawConceptualValidation();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reload From Disk"))
            {
                ReloadSettings();
            }

            if (GUILayout.Button("Open Settings File"))
            {
                OpenSettingsLocation();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset to Defaults"))
            {
                ResetToDefaults();
            }

            if (GUILayout.Button("Apply Configuration"))
            {
                ApplyConfiguration();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
        }

        private void DrawGeneralSettings()
        {
            EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            _editableSettings.projectName = EditorGUILayout.TextField("Project Name", _editableSettings.projectName);
            _editableSettings.menuPath = EditorGUILayout.TextField("Menu Path", _editableSettings.menuPath);
            _editableSettings.exportPath = EditorGUILayout.TextField("Export Path", _editableSettings.exportPath);

            _editableSettings.validateOnStart = EditorGUILayout.Toggle("Validate on Start", _editableSettings.validateOnStart);
            _editableSettings.strictMode = EditorGUILayout.Toggle("Strict Mode", _editableSettings.strictMode);
            _editableSettings.includeUnityAssemblies = EditorGUILayout.Toggle("Include Unity Assemblies", _editableSettings.includeUnityAssemblies);

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        private void DrawAssemblyFilters()
        {
            EditorGUILayout.LabelField("Assembly Include Filters", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Select the assemblies to INCLUDE in validation. Assemblies not selected will be ignored.", MessageType.Info);

            if (_editableSettings == null)
            {
                return;
            }

            // Build list of available assemblies
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var includeUnity = _editableSettings.includeUnityAssemblies;
            var names = allAssemblies
                .Where(a => includeUnity || (
                    !a.FullName.StartsWith("Unity") &&
                    !a.FullName.StartsWith("UnityEngine") &&
                    !a.FullName.StartsWith("UnityEditor")))
                .Select(a => a.GetName().Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var selected = new HashSet<string>(_editableSettings.assemblyFilters ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All", GUILayout.Width(100)))
            {
                selected = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
            }
            if (GUILayout.Button("Clear All", GUILayout.Width(100)))
            {
                selected.Clear();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            foreach (var label in names)
            {
                var isSelected = selected.Contains(label);
                var toggled = EditorGUILayout.ToggleLeft(label, isSelected);
                if (toggled == isSelected) continue;
                if (toggled) selected.Add(label); else selected.Remove(label);
            }
            EditorGUI.indentLevel--;

            _editableSettings.assemblyFilters = selected.ToArray();
            EditorGUILayout.Space();
        }

        private void DrawConceptualValidation()
        {
            var config = _editableSettings.conceptualValidation;
            _showConceptualSection = EditorGUILayout.Foldout(_showConceptualSection, "Conceptual Validation", true);
            if (!_showConceptualSection)
            {
                return;
            }

            EditorGUI.indentLevel++;
            config.enableConceptTests = EditorGUILayout.Toggle("Enable Concept Tests", config.enableConceptTests);
            config.autoDetectEnvironment = EditorGUILayout.Toggle("Auto Detect Environment", config.autoDetectEnvironment);

            EditorGUILayout.LabelField("Validation Tiers", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            config.validationTiers.universal = EditorGUILayout.Toggle("Universal", config.validationTiers.universal);
            config.validationTiers.unityAware = EditorGUILayout.Toggle("Unity-Aware", config.validationTiers.unityAware);
            config.validationTiers.projectSpecific = EditorGUILayout.Toggle("Project-Specific", config.validationTiers.projectSpecific);
            EditorGUI.indentLevel--;

            var fallbackModes = new[] { "ilAnalysis", "skip" };
            var fallbackIndex = Array.IndexOf(fallbackModes, config.fallbackMode);
            if (fallbackIndex < 0)
            {
                fallbackIndex = 0;
            }
            fallbackIndex = EditorGUILayout.Popup("Fallback Mode", fallbackIndex, fallbackModes);
            config.fallbackMode = fallbackModes[fallbackIndex];

            EditorGUILayout.Space();

            _showCustomComponentTypes = EditorGUILayout.Foldout(_showCustomComponentTypes, "Custom Component Types", true);
            if (_showCustomComponentTypes)
            {
                EditorGUI.indentLevel++;
                DrawStringList(ref config.customComponentTypes, "Component", "Component");
                EditorGUI.indentLevel--;
            }

            _showEnumPatterns = EditorGUILayout.Foldout(_showEnumPatterns, "Enum Validation Patterns", true);
            if (_showEnumPatterns)
            {
                EditorGUI.indentLevel++;
                DrawStringList(ref config.enumValidationPatterns, "Pattern", "Pattern");
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            _showEnvironmentSection = EditorGUILayout.Foldout(_showEnvironmentSection, "Environment Capabilities Overrides", true);
            if (_showEnvironmentSection)
            {
                EditorGUI.indentLevel++;
                var environment = config.environmentCapabilities;
                environment.hasUnityEngine = EditorGUILayout.Toggle("Has UnityEngine", environment.hasUnityEngine);
                environment.hasDots = EditorGUILayout.Toggle("Has DOTS", environment.hasDots);
                environment.hasBurst = EditorGUILayout.Toggle("Has Burst", environment.hasBurst);
                environment.hasEntities = EditorGUILayout.Toggle("Has Entities", environment.hasEntities);
                environment.canInstantiateComponents = EditorGUILayout.Toggle("Can Instantiate Components", environment.canInstantiateComponents);
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        private void ApplyConfiguration()
        {
            if (_editableSettings == null)
            {
                return;
            }

            var runtimeSettings = StoryTestSettings.Instance;
            CopySettings(_editableSettings, runtimeSettings);
            runtimeSettings.SaveSettings();
            StoryTestSettings.ReloadSettings();
            _settingsFileExists = true;
            AssetDatabase.Refresh();

            Debug.Log($"[Story Test] Settings saved to {SettingsRelativePath}");
            ReloadSettings();
        }

        private void ResetToDefaults()
        {
            _editableSettings = new StoryTestSettings();
            EnsureDefaults(_editableSettings);
        }

        private static void OpenSettingsLocation()
        {
            var settingsPath = GetSettingsAbsolutePath();
            
            if (File.Exists(settingsPath))
            {
                EditorUtility.RevealInFinder(settingsPath);
            }
            else
            {
                var directory = Path.GetDirectoryName(settingsPath);
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                {
                    EditorUtility.RevealInFinder(directory);
                }
                else
                {
                    EditorUtility.DisplayDialog("Settings File Not Found",
                        $"The settings file does not exist yet:\n{settingsPath}\n\nClick 'Apply Configuration' to create it.",
                        "OK");
                }
            }
        }

        private void DrawStringList(ref string[] items, string label, string addLabel)
        {
            items ??= Array.Empty<string>();

            EditorGUI.indentLevel++;
            var list = new List<string>(items);
            var removed = false;

            for (var i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                list[i] = EditorGUILayout.TextField($"{label} {i + 1}", list[i]);
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    list.RemoveAt(i);
                    removed = true;
                    EditorGUILayout.EndHorizontal();
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (!removed && GUILayout.Button($"Add {addLabel}"))
            {
                list.Add(string.Empty);
            }

            items = list.ToArray();
            EditorGUI.indentLevel--;

            if (removed)
            {
                // Trigger a repaint so the removed element disappears immediately.
                Repaint();
            }
        }

        private static StoryTestSettings CloneSettings(StoryTestSettings source)
        {
            if (source == null)
            {
                return new StoryTestSettings();
            }

            var json = JsonUtility.ToJson(source);
            var clone = JsonUtility.FromJson<StoryTestSettings>(json) ?? new StoryTestSettings();
            return clone;
        }

        private static void EnsureDefaults(StoryTestSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            settings.projectName ??= "YourProjectName";
            settings.menuPath ??= "Tiny Walnut Games/The Story Test/";
            settings.exportPath ??= ".debug/storytest_report.txt";
            settings.assemblyFilters = settings.assemblyFilters ?? Array.Empty<string>();

            settings.conceptualValidation ??= new ConceptualValidationConfig();
            var concept = settings.conceptualValidation;
            concept.validationTiers ??= new ValidationTiers();
            concept.environmentCapabilities ??= new EnvironmentCapabilities();
            concept.customComponentTypes = concept.customComponentTypes ?? Array.Empty<string>();
            concept.enumValidationPatterns = concept.enumValidationPatterns ?? Array.Empty<string>();
            concept.fallbackMode ??= "ilAnalysis";
        }

        private static void CopySettings(StoryTestSettings source, StoryTestSettings target)
        {
            if (source == null || target == null)
            {
                return;
            }

            target.projectName = source.projectName;
            target.menuPath = source.menuPath;
            target.exportPath = source.exportPath;
            target.includeUnityAssemblies = source.includeUnityAssemblies;
            target.validateOnStart = source.validateOnStart;
            target.strictMode = source.strictMode;
            target.assemblyFilters = CloneArray(source.assemblyFilters);

            target.conceptualValidation ??= new ConceptualValidationConfig();
            var targetConcept = target.conceptualValidation;
            var sourceConcept = source.conceptualValidation ?? new ConceptualValidationConfig();

            targetConcept.enableConceptTests = sourceConcept.enableConceptTests;
            targetConcept.autoDetectEnvironment = sourceConcept.autoDetectEnvironment;
            targetConcept.fallbackMode = string.IsNullOrWhiteSpace(sourceConcept.fallbackMode) ? "ilAnalysis" : sourceConcept.fallbackMode;

            targetConcept.validationTiers ??= new ValidationTiers();
            var sourceTiers = sourceConcept.validationTiers ?? new ValidationTiers();
            targetConcept.validationTiers.universal = sourceTiers.universal;
            targetConcept.validationTiers.unityAware = sourceTiers.unityAware;
            targetConcept.validationTiers.projectSpecific = sourceTiers.projectSpecific;

            targetConcept.environmentCapabilities ??= new EnvironmentCapabilities();
            var sourceEnv = sourceConcept.environmentCapabilities ?? new EnvironmentCapabilities();
            targetConcept.environmentCapabilities.hasUnityEngine = sourceEnv.hasUnityEngine;
            targetConcept.environmentCapabilities.hasDots = sourceEnv.hasDots;
            targetConcept.environmentCapabilities.hasBurst = sourceEnv.hasBurst;
            targetConcept.environmentCapabilities.hasEntities = sourceEnv.hasEntities;
            targetConcept.environmentCapabilities.canInstantiateComponents = sourceEnv.canInstantiateComponents;

            targetConcept.customComponentTypes = CloneArray(sourceConcept.customComponentTypes);
            targetConcept.enumValidationPatterns = CloneArray(sourceConcept.enumValidationPatterns);
        }

        private static string[] CloneArray(string[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<string>();
            }

            var clone = new string[source.Length];
            Array.Copy(source, clone, source.Length);
            return clone;
        }

        private static string GetSettingsAbsolutePath()
        {
            return Path.Combine(Application.dataPath, "Tiny Walnut Games", "TheStoryTest", "Resources", "StoryTestSettings.json");
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