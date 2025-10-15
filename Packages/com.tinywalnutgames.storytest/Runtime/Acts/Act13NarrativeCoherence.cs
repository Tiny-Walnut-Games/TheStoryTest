using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Acts
{
    /// <summary>
    /// Story Test Act 13: Narrative Coherence (Architecture Alignment)
    /// 
    /// Validates that the project structure and organization aligns with its stated
    /// architectural rules and narrative. This prevents "plot holes" where claimed 
    /// architecture doesn't match reality.
    /// 
    /// Checks:
    /// - Separation of concerns (layers don't violate boundaries)
    /// - Architectural consistency (symmetric implementations)
    /// - Documentation completeness (stories match artifacts)
    /// - Quality gates (minimum standards met)
    /// </summary>
    [StoryIgnore("Story test narrative coherence validation infrastructure")]
    public static class Act13NarrativeCoherence
    {
        /// <summary>
        /// The validation rule for this act.
        /// Checks that project architecture aligns with its narrative.
        /// </summary>
        public static readonly ValidationRule Rule = ValidateNarrativeCoherence;

        /// <summary>
        /// Validates narrative coherence at assembly level.
        /// </summary>
        private static bool ValidateNarrativeCoherence(MemberInfo member, out string violation)
        {
            violation = null;

            // This is an assembly-level validation
            if (member != null)
                return false;

            try
            {
                var mentalModelPath = FindMentalModelConfig();
                if (string.IsNullOrEmpty(mentalModelPath))
                {
                    // Not a violation - Act 12 will report missing config
                    return false;
                }

                var coherenceGaps = ValidateArchitecturalCoherence(mentalModelPath);
                if (coherenceGaps.Count > 0)
                {
                    violation = $"Narrative coherence issues detected: {string.Join("; ", coherenceGaps)}";
                    return true;
                }
            }
            catch (Exception ex)
            {
                violation = $"Error validating narrative coherence: {ex.Message}";
                return true;
            }

            return false;
        }

        /// <summary>
        /// Finds the mental model configuration file.
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
        /// Validates architectural coherence against stated rules.
        /// </summary>
        private static List<string> ValidateArchitecturalCoherence(string configPath)
        {
            var gaps = new List<string>();

            try
            {
                var configJson = File.ReadAllText(configPath);

                // Check quality gates
                gaps.AddRange(ValidateQualityGates(configJson));

                // Check architectural rules
                gaps.AddRange(ValidateArchitecturalRules(configJson));

                // Check documentation coverage
                gaps.AddRange(ValidateDocumentationCoherence(configJson));
            }
            catch
            {
                gaps.Add("Unable to parse mental model configuration for coherence validation");
            }

            return gaps;
        }

        /// <summary>
        /// Validates that quality gates are met.
        /// </summary>
        private static List<string> ValidateQualityGates(string configJson)
        {
            var gaps = new List<string>();

            // Count validation acts
            if (configJson.Contains("\"minimum_acts\": 11"))
            {
                var actFiles = Directory.GetFiles("Packages/com.tinywalnutgames.storytest/Runtime/Acts", "Act*.cs")
                    .Where(f => !f.EndsWith(".meta"))
                    .Count();

                if (actFiles < 11)
                {
                    gaps.Add($"Quality gate failed: only {actFiles} Acts found (minimum 11 required)");
                }
            }

            // Check documentation exists
            if (configJson.Contains("\"minimum_docs_pages\""))
            {
                if (!Directory.Exists("docs"))
                {
                    gaps.Add("Quality gate failed: docs/ directory required but missing");
                }
                else
                {
                    var docFiles = Directory.GetFiles("docs", "*.md").Length;
                    if (docFiles < 5)
                    {
                        gaps.Add($"Quality gate failed: only {docFiles} documentation files found (minimum 5 required)");
                    }
                }
            }

            // Check for multi-platform support
            if (configJson.Contains("\"required_platforms\": 2"))
            {
                var platforms = CountPlatforms();
                if (platforms < 2)
                {
                    gaps.Add($"Quality gate failed: only {platforms} platforms implemented (minimum 2 required)");
                }
            }

            return gaps;
        }

        /// <summary>
        /// Validates separation of concerns and architectural rules.
        /// </summary>
        private static List<string> ValidateArchitecturalRules(string configJson)
        {
            var gaps = new List<string>();

            // Rule: Separation of concerns
            if (configJson.Contains("\"separation_of_concerns\""))
            {
                // Runtime should exist independently
                if (!Directory.Exists("Packages/com.tinywalnutgames.storytest/Runtime"))
                {
                    gaps.Add("Architecture violation: Runtime/ layer missing");
                }

                // Editor should be separate
                if (!Directory.Exists("Packages/com.tinywalnutgames.storytest/Editor"))
                {
                    gaps.Add("Architecture violation: Editor layer should be separate from Runtime");
                }
            }

            // Rule: Validation symmetry
            if (configJson.Contains("\"validation_symmetry\""))
            {
                var csharpActs = Directory.GetFiles("Packages/com.tinywalnutgames.storytest/Runtime/Acts", "Act*.cs")
                    .Where(f => !f.EndsWith(".meta")).Count();
                
                var pythonValidatorExists = File.Exists("storytest/validator.py");

                if (csharpActs > 0 && !pythonValidatorExists)
                {
                    gaps.Add($"Validation symmetry broken: {csharpActs} C# Acts but Python validator missing");
                }
            }

            // Rule: Zero dependencies Python
            if (configJson.Contains("\"zero_dependencies_python\""))
            {
                var projectToml = TryReadFile("pyproject.toml");
                if (!string.IsNullOrEmpty(projectToml))
                {
                    // Check that dependencies are minimal
                    var hasPythonNet = projectToml.Contains("pythonnet");
                    var hasClrLoader = projectToml.Contains("clr-loader");
                    
                    // These are expected; if there are many others it's a problem
                    // This is a simple heuristic check
                    var dependencyCount = projectToml.Split(new[] { "\"" }, StringSplitOptions.None)
                        .Where(s => s.Contains(">=") || s.Contains(">="))
                        .Count();

                    if (dependencyCount > 5) // Rough threshold
                    {
                        gaps.Add($"Zero dependencies rule violated: {dependencyCount} dependencies found");
                    }
                }
            }

            return gaps;
        }

        /// <summary>
        /// Validates that documentation supports claimed features.
        /// </summary>
        private static List<string> ValidateDocumentationCoherence(string configJson)
        {
            var gaps = new List<string>();

            if (!Directory.Exists("docs"))
            {
                gaps.Add("Documentation directory missing - narrative cannot be verified");
                return gaps;
            }

            // Check for key documentation files
            var requiredDocs = new[]
            {
                "docs/README.md",
                "docs/acts.md",
                "docs/getting-started.md"
            };

            foreach (var doc in requiredDocs)
            {
                if (!File.Exists(doc))
                {
                    gaps.Add($"Critical documentation missing: {doc}");
                }
            }

            // Verify Acts documentation matches implementation
            if (File.Exists("docs/acts.md"))
            {
                var actsDoc = File.ReadAllText("docs/acts.md");
                var actFiles = Directory.GetFiles("Packages/com.tinywalnutgames.storytest/Runtime/Acts", "Act*.cs")
                    .Where(f => !f.EndsWith(".meta"))
                    .Select(f => Path.GetFileNameWithoutExtension(f));

                foreach (var act in actFiles)
                {
                    if (!actsDoc.Contains(act))
                    {
                        gaps.Add($"Documentation gap: {act} implemented but not documented in acts.md");
                    }
                }
            }

            return gaps;
        }

        /// <summary>
        /// Counts how many platforms are implemented.
        /// </summary>
        private static int CountPlatforms()
        {
            var count = 0;
            if (Directory.Exists("Packages/com.tinywalnutgames.storytest")) count++;
            if (Directory.Exists("storytest")) count++;
            if (File.Exists("scripts/story_test.py")) count++;
            return count;
        }

        /// <summary>
        /// Safely reads a file, returning null if it doesn't exist.
        /// </summary>
        private static string TryReadFile(string path)
        {
            try
            {
                if (File.Exists(path))
                    return File.ReadAllText(path);
            }
            catch
            {
                // Ignore read errors
            }

            return null;
        }
    }
}