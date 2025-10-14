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
                Debug.LogWarning($"Story Integrity Validation found {violations.Count()} violations:");
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
        /// Validates project structure and organization.
        /// </summary>
        [MenuItem(MenuRoot + "Validate Project Structure", false, 31)]
        public static void ValidateProjectStructure()
        {
            Debug.Log($"[{GetMenuPath()}] Validating project structure...");
            
            var issues = new List<string>();

            // Check for proper assembly organization
            var assemblies = System.Reflection.Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            if (!assemblies.Any(a => a.Name.Contains("Unity.Entities")))
            {
                issues.Add("DOTS/ECS assemblies not properly referenced");
            }

            // Check for the proper folder structure?            
            // 👀 - A better approach would be to load a config file defining the expected structure.
            /* I am not sure of the string over-ride syntax here, but as an example:
            story-settings.json
            {
                "projectName": "MyGame",
                "requiredFolders": [
                    $"Assets/{projectName}/Scripts",
                    $"Assets/{projectName}/Scenes",
                    $"Assets/{projectName}/Prefabs",
                    $"Assets/{projectName}/Art",
                    $"Assets/{projectName}/Audio",
                    $"Assets/Editor",
                    $"Assets/Tests"
                ],
                "optionalFolders": [
                    $"Assets/{projectName}/UI",
                    $"Assets/{projectName}/Resources",
                    $"Assets/{projectName}/Animations"
                ],
                "rules": {
                    "enforcePrefabUsage": true,
                    "allowResourcesFolder": false,
                    "requireTests": true
                }
            }
            */
            var requiredFolders = new[]
            {
                "Assets/Scripts",
                "Assets/Scenes",
                "Assets/Prefabs",
                "Assets/Art",
                "Assets/Audio",
                "Assets/Editor",
                "Assets/Tests"
            };

            issues.AddRange(from folder in requiredFolders where !Directory.Exists(folder) select $"Required folder missing: {folder}");

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
            if (state != PlayModeStateChange.EnteredPlayMode) return;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            Debug.LogWarning("🏳Complete validation pipeline not yet implemented");
        }
        
        private static void RunValidation()
        {
            var testObject = Object.FindFirstObjectByType<ProductionExcellenceStoryTest>();
            if (testObject is null)
            {
                // Create a temporary test object
                var tempGo = new GameObject("TempValidationRunner");
                testObject = tempGo.AddComponent<ProductionExcellenceStoryTest>();

                testObject.OnValidationComplete += (report) =>
                {
                    ShowValidationResults(report);
                    Object.DestroyImmediate(tempGo);
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
            var report = "THE STORY-TEST FRAMEWORK STRENGTHENING VALIDATION REPORT\n";
            report += new string('=', 50) + "\n\n";
            report += $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
            report += $"Unity Version: {Application.unityVersion}\n";
            report += $"Target Platform: {EditorUserBuildSettings.activeBuildTarget}\n\n";

            // Story integrity analysis - use proper settings-based filtering
            var settings = StoryTestSettings.Instance;
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Let StoryIntegrityValidator handle filtering according to settings
            var assemblies = allAssemblies
                .Where(a => StoryIntegrityValidator.IsProjectAssembly(a, settings))
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
                    var file = string.IsNullOrWhiteSpace(violation.FilePath) ? "<unknown>" : violation.FilePath;
                    var line = violation.LineNumber > 0 ? violation.LineNumber.ToString() : "?";
                    var typeName = string.IsNullOrWhiteSpace(violation.Type) ? "<UnknownType>" : violation.Type;
                    var member = string.IsNullOrWhiteSpace(violation.Member) ? "<UnknownMember>" : violation.Member;
                    var message = string.IsNullOrWhiteSpace(violation.Violation) ? "<No details provided>" : violation.Violation;
                    report += $"  • [{violation.ViolationType}] {typeName}.{member} - {message} (File: {file}, Line: {line})\n";
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
                // Guard GetTypes; some assemblies may throw
                int typeCount;
                try { typeCount = assembly.GetTypes().Length; } catch { typeCount = -1; }
                report += $"Types: {typeCount}\n";

                // Dynamic assemblies don't have a Location property
                try
                {
                    if (!assembly.IsDynamic)
                    {
                        report += $"Location: {assembly.Location}\n";
                    }
                    else
                    {
                        report += "Location: (dynamic assembly)\n";
                    }
                }
                catch (NotSupportedException)
                {
                    report += "Location: (not supported)\n";
                }
                catch
                {
                    report += "Location: (unavailable)\n";
                }

                report += "\n";
            }

            // Project structure analysis
            report += "PROJECT STRUCTURE\n";
            report += new string('-', 17) + "\n";
            report += "Folder Structure:\n";
            string[] directories;
            try { directories = Directory.GetDirectories("Assets", "*", SearchOption.AllDirectories); }
            catch { directories = Array.Empty<string>(); }
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
        private const string SettingsRelativePath = "Assets/Tiny Walnut Games/TheStoryTest/Resources/StoryTestSettings.json";

        private Vector2 scrollPosition;
        private StoryTestSettings editableSettings;
        private bool settingsFileExists;
        private bool showConceptualSection = true;
        private bool showEnvironmentSection;
        private bool showCustomComponentTypes;
        private bool showEnumPatterns;

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
            editableSettings = CloneSettings(StoryTestSettings.Instance);
            EnsureDefaults(editableSettings);
            settingsFileExists = File.Exists(GetSettingsAbsolutePath());
        }

        private void OnGUI()
        {
            if (editableSettings == null)
            {
                ReloadSettings();
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Strengthening Validation Configuration", EditorStyles.largeLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox($"Editing Story Test settings at: {SettingsRelativePath}", MessageType.Info);
            if (!settingsFileExists)
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

            editableSettings.projectName = EditorGUILayout.TextField("Project Name", editableSettings.projectName);
            editableSettings.menuPath = EditorGUILayout.TextField("Menu Path", editableSettings.menuPath);
            editableSettings.exportPath = EditorGUILayout.TextField("Export Path", editableSettings.exportPath);

            editableSettings.validateOnStart = EditorGUILayout.Toggle("Validate on Start", editableSettings.validateOnStart);
            editableSettings.strictMode = EditorGUILayout.Toggle("Strict Mode", editableSettings.strictMode);
            editableSettings.includeUnityAssemblies = EditorGUILayout.Toggle("Include Unity Assemblies", editableSettings.includeUnityAssemblies);

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        private void DrawAssemblyFilters()
        {
            EditorGUILayout.LabelField("Assembly Include Filters", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Select the assemblies to INCLUDE in validation. Assemblies not selected will be ignored.", MessageType.Info);

            if (editableSettings == null)
            {
                return;
            }

            // Build list of available assemblies
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var includeUnity = editableSettings.includeUnityAssemblies;
            var names = allAssemblies
                .Where(a => includeUnity || (
                    !a.FullName.StartsWith("Unity") &&
                    !a.FullName.StartsWith("UnityEngine") &&
                    !a.FullName.StartsWith("UnityEditor")))
                .Select(a => a.GetName().Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var selected = new HashSet<string>(editableSettings.assemblyFilters ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);

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

            editableSettings.assemblyFilters = selected.ToArray();
            EditorGUILayout.Space();
        }

        private void DrawConceptualValidation()
        {
            var config = editableSettings.conceptualValidation;
            showConceptualSection = EditorGUILayout.Foldout(showConceptualSection, "Conceptual Validation", true);
            if (!showConceptualSection)
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

            showCustomComponentTypes = EditorGUILayout.Foldout(showCustomComponentTypes, "Custom Component Types", true);
            if (showCustomComponentTypes)
            {
                EditorGUI.indentLevel++;
                DrawStringList(ref config.customComponentTypes, "Component", "Component");
                EditorGUI.indentLevel--;
            }

            showEnumPatterns = EditorGUILayout.Foldout(showEnumPatterns, "Enum Validation Patterns", true);
            if (showEnumPatterns)
            {
                EditorGUI.indentLevel++;
                DrawStringList(ref config.enumValidationPatterns, "Pattern", "Pattern");
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            showEnvironmentSection = EditorGUILayout.Foldout(showEnvironmentSection, "Environment Capabilities Overrides", true);
            if (showEnvironmentSection)
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
            if (editableSettings == null)
            {
                return;
            }

            var runtimeSettings = StoryTestSettings.Instance;
            CopySettings(editableSettings, runtimeSettings);
            runtimeSettings.SaveSettings();
            StoryTestSettings.ReloadSettings();
            settingsFileExists = true;
            AssetDatabase.Refresh();

            Debug.Log($"[Story Test] Settings saved to {SettingsRelativePath}");
            ReloadSettings();
        }

        private void ResetToDefaults()
        {
            editableSettings = new StoryTestSettings();
            EnsureDefaults(editableSettings);
        }

        private void OpenSettingsLocation()
        {
            var absolutePath = GetSettingsAbsolutePath();
            if (File.Exists(absolutePath))
            {
                EditorUtility.RevealInFinder(absolutePath);
            }
            else
            {
                EditorUtility.DisplayDialog("Story Test Settings", $"No settings file found at: {absolutePath}\nApply the configuration to create it.", "OK");
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