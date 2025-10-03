using System;
using System.IO;
using UnityEngine;

namespace TinyWalnutGames.StoryTest.Shared
{
    /// <summary>
    /// Configuration settings for Story Test framework.
    /// Loaded from Resources/StoryTestSettings.json or uses defaults for project-agnostic validation.
    /// </summary>
    [Serializable]
    [StoryIgnore("Configuration infrastructure for Story Test framework")]
    public class StoryTestSettings
    {
        public string projectName = "YourProjectName";
        public string menuPath = "Tiny Walnut Games/The Story Test/";
        public string[] assemblyFilters = new string[0];
        public bool includeUnityAssemblies = false;
        public bool validateOnStart = false;
        public bool strictMode = false;
        public string exportPath = ".debug/storytest_report.txt";

        // Conceptual validation configuration
        public ConceptualValidationConfig conceptualValidation = new ConceptualValidationConfig();

        private static StoryTestSettings _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the singleton instance of Story Test settings.
        /// Loads from Resources/StoryTestSettings.json or creates default settings.
        /// </summary>
        public static StoryTestSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = LoadSettings();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Loads settings from Resources/StoryTestSettings.json.
        /// Falls back to default settings if file doesn't exist.
        /// </summary>
        private static StoryTestSettings LoadSettings()
        {
            try
            {
#if UNITY_EDITOR || UNITY_ENGINE
                var settingsAsset = Resources.Load<TextAsset>("StoryTestSettings");
                if (settingsAsset != null)
                {
                    var settings = JsonUtility.FromJson<StoryTestSettings>(settingsAsset.text);
                    if (settings != null)
                    {
                        Debug.Log($"[Story Test] Loaded settings for project: {settings.projectName}");
                        return settings;
                    }
                }
#else
                // For standalone Python validator or non-Unity contexts
                string settingsPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Assets",
                    "Tiny Walnut Games",
                    "TheStoryTest",
                    "Resources",
                    "StoryTestSettings.json"
                );

                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    var settings = JsonUtility.FromJson<StoryTestSettings>(json);
                    if (settings != null)
                    {
                        return settings;
                    }
                }
#endif
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || UNITY_ENGINE
                Debug.LogWarning($"[Story Test] Could not load settings: {ex.Message}. Using defaults.");
#else
                Console.WriteLine($"[Story Test] Could not load settings: {ex.Message}. Using defaults.");
#endif
            }

            // Return default settings for project-agnostic validation
            return new StoryTestSettings();
        }

        /// <summary>
        /// Saves current settings to Resources/StoryTestSettings.json (Editor only).
        /// </summary>
        public void SaveSettings()
        {
#if UNITY_EDITOR
            try
            {
                string json = JsonUtility.ToJson(this, true);
                string settingsPath = Path.Combine(
                    Application.dataPath,
                    "Tiny Walnut Games",
                    "TheStoryTest",
                    "Resources",
                    "StoryTestSettings.json"
                );

                Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
                File.WriteAllText(settingsPath, json);

                UnityEditor.AssetDatabase.Refresh();
                Debug.Log($"[Story Test] Settings saved for project: {projectName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Story Test] Failed to save settings: {ex.Message}");
            }
#endif
        }

        /// <summary>
        /// Reloads settings from Resources/StoryTestSettings.json.
        /// </summary>
        public static void ReloadSettings()
        {
            lock (_lock)
            {
                _instance = null;
            }
        }
    }

    /// <summary>
    /// Configuration for conceptual validation features.
    /// </summary>
    [Serializable]
    [StoryIgnore("Configuration infrastructure for conceptual validation")]
    public class ConceptualValidationConfig
    {
        public bool enableConceptTests = true;
        public bool autoDetectEnvironment = true;
        public ValidationTiers validationTiers = new ValidationTiers();
        public EnvironmentCapabilities environmentCapabilities = new EnvironmentCapabilities();
        public string[] customComponentTypes = new string[0];
        public string[] enumValidationPatterns = new string[0];
        public string fallbackMode = "ilAnalysis"; // or "skip"
    }

    /// <summary>
    /// Validation tier configuration.
    /// </summary>
    [Serializable]
    [StoryIgnore("Configuration infrastructure for validation tiers")]
    public class ValidationTiers
    {
        public bool universal = true;
        public bool unityAware = true;
        public bool projectSpecific = false;
    }
}
