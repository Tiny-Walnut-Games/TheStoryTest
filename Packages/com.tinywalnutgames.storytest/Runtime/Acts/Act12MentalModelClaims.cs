using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Acts
{
    /// <summary>
    /// Story Test Act 12: Mental Model Claims Validation
    /// 
    /// Validates that the project's claimed capabilities (from mental model config) 
    /// are supported by actual code artifacts. This act ensures the narrative is not 
    /// "foreshadowing" features that don't exist.
    /// 
    /// This act inspects:
    /// - storytest-mental-model.json configuration
    /// - Required artifacts listed in the mental model
    /// - Claimed capabilities against implementation evidence
    /// </summary>
    [StoryIgnore("Story test mental model validation infrastructure")]
    public static class Act12MentalModelClaims
    {
        /// <summary>
        /// The validation rule for this act.
        /// Checks that mental model claims have concrete evidence in the codebase.
        /// </summary>
        public static readonly ValidationRule Rule = ValidateMentalModelClaims;

        /// <summary>
        /// Validates mental model claims against project evidence.
        /// </summary>
        private static bool ValidateMentalModelClaims(MemberInfo member, out string violation)
        {
            violation = null;

            // This act validates at the assembly level, not individual members
            // It should be called by the validation orchestrator with null for member
            if (member != null)
                return false;

            try
            {
                var mentalModelPath = FindMentalModelConfig();
                if (string.IsNullOrEmpty(mentalModelPath))
                {
                    violation = "Mental model configuration file (storytest-mental-model.json) not found. " +
                               "Create it to document your project's claimed capabilities.";
                    return true; // Warning: not critical, but should be addressed
                }

                var claimsGaps = ValidateClaimedCapabilities(mentalModelPath);
                if (claimsGaps.Count > 0)
                {
                    violation = $"Mental model claims not fully supported: {string.Join("; ", claimsGaps)}";
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Gracefully handle parsing errors
                violation = $"Error validating mental model: {ex.Message}";
                return true;
            }

            return false;
        }

        /// <summary>
        /// Finds the mental model configuration file in the project root.
        /// </summary>
        private static string FindMentalModelConfig()
        {
            var candidates = new[]
            {
                "storytest-mental-model.json",
                "../../../storytest-mental-model.json",
                "../../storytest-mental-model.json"
            };

            foreach (var candidate in candidates)
            {
                if (File.Exists(candidate))
                    return Path.GetFullPath(candidate);
            }

            return null;
        }

        /// <summary>
        /// Validates that claimed capabilities have corresponding evidence.
        /// </summary>
        private static List<string> ValidateClaimedCapabilities(string configPath)
        {
            var gaps = new List<string>();

            try
            {
                var configJson = File.ReadAllText(configPath);
                
                // Simple JSON parsing - look for required_artifacts entries
                if (configJson.Contains("\"required\": true"))
                {
                    // Extract required artifacts
                    var requiredArtifacts = ExtractRequiredArtifacts(configJson);
                    
                    foreach (var artifact in requiredArtifacts)
                    {
                        if (!File.Exists(artifact) && !Directory.Exists(artifact))
                        {
                            gaps.Add($"Missing required artifact: {artifact}");
                        }
                    }
                }

                // Check for claimed capabilities
                var platformClaims = ExtractPlatformClaims(configJson);
                var implementedPlatforms = CountImplementedPlatforms();
                
                if (platformClaims.Count > implementedPlatforms)
                {
                    gaps.Add($"Claimed {platformClaims.Count} platforms but only {implementedPlatforms} are implemented");
                }
            }
            catch
            {
                // If we can't parse the config, that's a gap
                gaps.Add("Mental model configuration is malformed or unreadable");
            }

            return gaps;
        }

        /// <summary>
        /// Extracts required artifact paths from the config.
        /// </summary>
        private static List<string> ExtractRequiredArtifacts(string configJson)
        {
            var artifacts = new List<string>();
            
            // Simple pattern matching for "path" fields with required: true
            var lines = configJson.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("\"required\": true") || 
                    (i + 1 < lines.Length && lines[i + 1].Contains("\"required\": true")))
                {
                    // Look backwards for the path
                    for (int j = i; j >= Math.Max(0, i - 3); j--)
                    {
                        if (lines[j].Contains("\"path\""))
                        {
                            var pathValue = ExtractJsonValue(lines[j]);
                            if (!string.IsNullOrEmpty(pathValue))
                            {
                                artifacts.Add(pathValue);
                                break;
                            }
                        }
                    }
                }
            }

            return artifacts;
        }

        /// <summary>
        /// Counts how many platform implementations exist.
        /// </summary>
        private static int CountImplementedPlatforms()
        {
            var platforms = 0;
            
            if (Directory.Exists("Packages/com.tinywalnutgames.storytest"))
                platforms++; // C# / Unity
            
            if (Directory.Exists("storytest") && File.Exists("storytest/cli.py"))
                platforms++; // Python
            
            if (File.Exists("scripts/story_test.py"))
                platforms++; // CLI scripts

            return platforms;
        }

        /// <summary>
        /// Extracts platform claims from the config.
        /// </summary>
        private static List<string> ExtractPlatformClaims(string configJson)
        {
            var platforms = new List<string>();
            
            if (configJson.Contains("\"platforms\"") && configJson.Contains("\"Unity\""))
                platforms.Add("Unity");
            if (configJson.Contains(".NET"))
                platforms.Add(".NET");
            if (configJson.Contains("Python"))
                platforms.Add("Python");

            return platforms;
        }

        /// <summary>
        /// Extracts a string value from a JSON line.
        /// Simple utility for basic JSON parsing.
        /// </summary>
        private static string ExtractJsonValue(string jsonLine)
        {
            var start = jsonLine.IndexOf('"');
            if (start == -1) return null;
            
            start = jsonLine.IndexOf('"', start + 1);
            if (start == -1) return null;
            
            var end = jsonLine.IndexOf('"', start + 1);
            if (end == -1) return null;
            
            return jsonLine.Substring(start + 1, end - start - 1);
        }
    }
}