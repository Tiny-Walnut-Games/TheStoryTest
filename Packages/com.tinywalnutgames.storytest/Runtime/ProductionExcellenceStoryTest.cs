using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TinyWalnutGames.StoryTest.Shared;
using UnityEngine;
using UnityEngine.Serialization;

namespace TinyWalnutGames.StoryTest
{
    /// <summary>
    /// MonoBehaviour orchestrating multiphase Story Test validation inside Unity.
    /// Keeps validation workflow agnostic to specific project architectures while
    /// still accommodating DOTS/ECS environments when present.
    /// </summary>
    [DisallowMultipleComponent]
    public class ProductionExcellenceStoryTest : MonoBehaviour
    {
        [FormerlySerializedAs("EnableStoryIntegrity")]
        [Header("Validation Phases")]
        [Tooltip("Run the 9 Acts IL validation against target assemblies.")]
        public bool enableStoryIntegrity = true;

        [FormerlySerializedAs("EnableConceptualValidation")] [Tooltip("Run conceptual validation (enums, structs, abstract sealing) using ExtendedConceptualValidator.")]
        public bool enableConceptualValidation = true;

        [FormerlySerializedAs("EnableCodeCoverage")] [Tooltip("Placeholder toggle for future coverage integration. Adds informational notes when enabled.")]
        public bool enableCodeCoverage;

        [FormerlySerializedAs("EnableArchitecturalCompliance")] [Tooltip("Perform architectural compliance checks when conceptual validation runs. Currently adds ECS awareness notes.")]
        public bool enableArchitecturalCompliance;

        [FormerlySerializedAs("EnableSyncPointPerformance")] [Tooltip("Benchmark validation sync-points to ensure smooth parallel execution.")]
        public bool enableSyncPointPerformance;

        [FormerlySerializedAs("OverrideUnityAssemblies")]
        [Header("Assembly Selection")]
        [Tooltip("Override StoryTestSettings includeUnityAssemblies value.")]
        public bool overrideUnityAssemblies;

        [FormerlySerializedAs("IncludeUnityAssemblies")] [Tooltip("Include Unity assemblies when OverrideUnityAssemblies is enabled.")]
        public bool includeUnityAssemblies;

        [FormerlySerializedAs("AssemblyFilters")] [Tooltip("Additional assembly name filters (contains). Leave empty to validate all project assemblies.")]
        public string[] assemblyFilters = Array.Empty<string>();

        [FormerlySerializedAs("ValidateAutomaticallyOnStart")]
        [Header("Execution")]
        [Tooltip("Runs validation automatically on Start in addition to StoryTestSettings.validateOnStart.")]
        public bool validateAutomaticallyOnStart;

        [FormerlySerializedAs("StopOnFirstViolation")] [Tooltip("Stop validation after the first violation is encountered. Useful for fast feedback.")]
        public bool stopOnFirstViolation;

        [FormerlySerializedAs("ExportReport")]
        [Header("Reporting")]
        [Tooltip("Export a human-readable report after validation completes.")]
        public bool exportReport = true;

        [FormerlySerializedAs("ExportPath")] [Tooltip("Report export path (relative to project root).")]
        public string exportPath = ".debug/storytest_report.txt";

        /// <summary>
        /// Raised when validation completes. Provides the generated report.
        /// </summary>
        public event Action<ValidationReport> OnValidationComplete;

        private bool isValidating;
        private Coroutine validationRoutine;
        private ValidationReport lastReport;

        /// <summary>
        /// Indicates whether a validation run is currently in progress.
        /// </summary>
        public bool IsValidating => isValidating;

        /// <summary>
        /// Gets the report from the most recent validation run.
        /// </summary>
        public ValidationReport LastReport => lastReport;

        private void Start()
        {
            var settings = SafeGetSettings();
            if (!Application.isPlaying)
            {
                return;
            }

            if ((settings?.validateOnStart ?? false) || validateAutomaticallyOnStart)
            {
                ValidateProductionExcellence();
            }
        }

        private void OnDisable()
        {
            if (validationRoutine != null)
            {
                StopCoroutine(validationRoutine);
                validationRoutine = null;
            }
            isValidating = false;
        }

        /// <summary>
        /// Public API for triggering validation. Accessible via Context Menu.
        /// </summary>
        [ContextMenu("Validate Production Excellence")]
        public void ValidateProductionExcellence()
        {
            if (!isActiveAndEnabled)
            {
                UnityEngine.Debug.LogWarning("[Story Test] ProductionExcellenceStoryTest is disabled – validation aborted.");
                return;
            }

            if (isValidating)
            {
                UnityEngine.Debug.LogWarning("[Story Test] Validation already running.");
                return;
            }

            validationRoutine = StartCoroutine(RunValidation());
        }

        /// <summary>
        /// Convenience alias used by editor tooling when on-demand validation is requested.
        /// </summary>
        public void ValidateOnDemand() => ValidateProductionExcellence();

        private IEnumerator RunValidation()
        {
            isValidating = true;
            var stopwatch = Stopwatch.StartNew();

            var report = new ValidationReport
            {
                StartedAtUtc = DateTime.UtcNow,
                Configuration = BuildConfigurationSnapshot()
            };

            var assemblies = ResolveAssemblies();

            yield return RunStoryIntegrityPhase(report, assemblies);
            if (report.ShouldStop(stopOnFirstViolation))
            {
                FinishValidation(report, stopwatch);
                yield break;
            }

            yield return RunConceptualPhase(report);
            if (report.ShouldStop(stopOnFirstViolation))
            {
                FinishValidation(report, stopwatch);
                yield break;
            }

            yield return RunCodeCoveragePhase(report);
            yield return RunSyncPointPhase(report);

            stopwatch.Stop();
            FinishValidation(report, stopwatch);
        }

        private IEnumerator RunStoryIntegrityPhase(ValidationReport report, Assembly[] assemblies)
        {
            if (!enableStoryIntegrity)
            {
                yield break;
            }

            LogPhaseStart("Story Integrity");
            yield return null; // allow frame update before heavy work

            List<StoryViolation> violations;
            try
            {
                violations = StoryIntegrityValidator.ValidateAssemblies(assemblies);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[Story Test] Story Integrity validation failed: {ex.Message}");
                report.AddNote("StoryIntegrity", $"Validation failed: {ex.Message}");
                yield break;
            }

            report.AddViolations("StoryIntegrity", violations);
            if (violations.Count == 0)
            {
                report.AddNote("StoryIntegrity", "No violations detected – narrative is complete.");
            }

            yield return null;
        }

        private IEnumerator RunConceptualPhase(ValidationReport report)
        {
            if (!enableConceptualValidation && !enableArchitecturalCompliance)
            {
                yield break;
            }

            LogPhaseStart("Conceptual Validation");
            yield return null;

            List<StoryViolation> conceptualViolations;
            try
            {
                conceptualViolations = ExtendedConceptualValidator.RunConceptualValidation(SafeGetSettings());
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[Story Test] Conceptual validation failed: {ex.Message}");
                report.AddNote("Conceptual", $"Validation failed: {ex.Message}");
                yield break;
            }

            report.AddViolations("Conceptual", conceptualViolations);

            var config = SafeGetSettings()?.conceptualValidation;
            if (enableArchitecturalCompliance)
            {
                report.AddNote("Conceptual",
                    config?.environmentCapabilities?.hasEntities == true
                        ? "DOTS/ECS environment detected – ensure hybrid patterns are intentional."
                        : "Architectural compliance ran in non-ECS environment.");
            }

            if (conceptualViolations.Count == 0)
            {
                report.AddNote("Conceptual", "Conceptual validation passed with no violations.");
            }

            yield return null;
        }

        private IEnumerator RunCodeCoveragePhase(ValidationReport report)
        {
            if (!enableCodeCoverage)
            {
                yield break;
            }

            LogPhaseStart("Code Coverage");
            yield return null;

            report.AddNote("CodeCoverage", "Code coverage validation not yet implemented – integrate coverage tooling to enable this phase.");
            yield return null;
        }

        private IEnumerator RunSyncPointPhase(ValidationReport report)
        {
            if (!enableSyncPointPerformance)
            {
                yield break;
            }

            LogPhaseStart("Sync-Point Performance");
            yield return null;

            Task<bool> validationTask;
            try
            {
                validationTask = StoryTestSyncPointValidator.QuickSyncPointTest();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[Story Test] Sync-point performance test failed to start: {ex.Message}");
                report.AddNote("SyncPointPerformance", $"Could not execute performance test: {ex.Message}");
                yield break;
            }

            while (!validationTask.IsCompleted)
            {
                yield return null;
            }

            if (validationTask.IsFaulted)
            {
                var message = validationTask.Exception?.GetBaseException().Message ?? "Unknown error";
                UnityEngine.Debug.LogWarning($"[Story Test] Sync-point performance test failed: {message}");
                report.Performance.SyncPointPassed = false;
                report.AddNote("SyncPointPerformance", $"Performance test failed: {message}");
            }
            else
            {
                report.Performance.SyncPointPassed = validationTask.Result;
                report.AddNote("SyncPointPerformance", validationTask.Result
                    ? "Sync-point performance passed"
                    : "Sync-point performance detected potential bottlenecks");
            }

            yield return null;
        }

        private void FinishValidation(ValidationReport report, Stopwatch stopwatch)
        {
            isValidating = false;
            validationRoutine = null;

            report.CompletedAtUtc = DateTime.UtcNow;
            report.Duration = stopwatch.Elapsed;
            lastReport = report;

            LogReport(report);
            if (exportReport)
            {
                ExportReportToDisk(report);
            }

            OnValidationComplete?.Invoke(report);
        }

        private ValidationConfigurationSnapshot BuildConfigurationSnapshot()
        {
            var settings = SafeGetSettings();
            var filters = new List<string>();
            if (settings?.assemblyFilters != null)
            {
                filters.AddRange(settings.assemblyFilters);
            }
            if (assemblyFilters is { Length: > 0 })
            {
                filters.AddRange(assemblyFilters);
            }

            return new ValidationConfigurationSnapshot
            {
                storyIntegrity = enableStoryIntegrity,
                conceptualValidation = enableConceptualValidation,
                codeCoverage = enableCodeCoverage,
                architecturalCompliance = enableArchitecturalCompliance,
                syncPointPerformance = enableSyncPointPerformance,
                stopOnFirstViolation = stopOnFirstViolation,
                includeUnityAssemblies = overrideUnityAssemblies ? includeUnityAssemblies : settings?.includeUnityAssemblies ?? false,
                assemblyFilters = filters.Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
            };
        }

        private Assembly[] ResolveAssemblies()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var settings = SafeGetSettings();
            var includeUnity = settings?.includeUnityAssemblies ?? false;
            if (overrideUnityAssemblies)
            {
                includeUnity = includeUnityAssemblies;
            }

            var filters = new List<string>();
            if (settings?.assemblyFilters != null)
            {
                filters.AddRange(settings.assemblyFilters);
            }
            if (assemblyFilters is { Length: > 0 })
            {
                filters.AddRange(assemblyFilters);
            }

            var usingFilters = filters.Count > 0;

            return allAssemblies
                .Where(a => ShouldIncludeAssembly(a, includeUnity, filters, usingFilters))
                .ToArray();
        }

        private static bool ShouldIncludeAssembly(Assembly assembly, bool includeUnity, List<string> filters, bool usingFilters)
        {
            if (assembly == null)
            {
                return false;
            }

            var name = assembly.GetName().Name;

            if (!includeUnity && (name.StartsWith("Unity", StringComparison.OrdinalIgnoreCase) ||
                                  name.StartsWith("UnityEngine", StringComparison.OrdinalIgnoreCase) ||
                                  name.StartsWith("UnityEditor", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            if (IsSystemAssembly(name))
            {
                return false;
            }

            if (usingFilters)
            {
                return filters.Any(filter =>
                    name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            return true;
        }

        private static bool IsSystemAssembly(string assemblyName)
        {
            return assemblyName.StartsWith("System", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("Mono.", StringComparison.OrdinalIgnoreCase) ||
                   assemblyName.StartsWith("nunit", StringComparison.OrdinalIgnoreCase);
        }

        private static StoryTestSettings SafeGetSettings()
        {
            try
            {
                return StoryTestSettings.Instance;
            }
            catch
            {
                return null;
            }
        }

        private void LogReport(ValidationReport report)
        {
            var summary = report.GenerateSummary();
            if (report.IsFullyCompliant)
            {
                UnityEngine.Debug.Log($"[Story Test] Production Excellence PASSED!\n{summary}");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[Story Test] Production Excellence found {report.StoryViolations.Count} violation(s).\n{summary}");
            }
        }

        private void ExportReportToDisk(ValidationReport report)
        {
            try
            {
                var path = exportPath;
                if (!Path.IsPathRooted(path))
                {
                    path = Path.Combine(Application.dataPath, "..", path);
                }

                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(path, report.GenerateSummary());
                report.Performance.syncPointReportPath = path;
                UnityEngine.Debug.Log($"[Story Test] Validation report exported to: {Path.GetFullPath(path)}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[Story Test] Failed to export validation report: {ex.Message}");
            }
        }

        private static void LogPhaseStart(string phase)
        {
            UnityEngine.Debug.Log($"[Story Test] === {phase} ===");
        }
    }

    /// <summary>
    /// Structured validation report for ProductionExcellenceStoryTest runs.
    /// </summary>
    [Serializable]
    public class ValidationReport
    {
        private readonly Dictionary<string, List<StoryViolation>> phaseViolations = new Dictionary<string, List<StoryViolation>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<string>> phaseNotes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        public DateTime StartedAtUtc { get; set; }
        public DateTime CompletedAtUtc { get; set; }
        public TimeSpan Duration { get; set; }
        public ValidationConfigurationSnapshot Configuration { get; set; } = new ValidationConfigurationSnapshot();
        public ValidationPerformanceSnapshot Performance { get; } = new ValidationPerformanceSnapshot();
        public List<StoryViolation> StoryViolations { get; } = new List<StoryViolation>();

        public IReadOnlyDictionary<string, List<StoryViolation>> PhaseViolations => phaseViolations;
        public IReadOnlyDictionary<string, List<string>> PhaseNotes => phaseNotes;

        public bool IsFullyCompliant => StoryViolations.Count == 0;
        public float ProductionReadinessScore => Mathf.Clamp(100f - StoryViolations.Count * 5f, 0f, 100f);

        public void AddViolations(string phase, IEnumerable<StoryViolation> violations)
        {
            if (string.IsNullOrEmpty(phase) || violations == null)
            {
                return;
            }

            if (!phaseViolations.TryGetValue(phase, out var list))
            {
                list = new List<StoryViolation>();
                phaseViolations[phase] = list;
            }

            foreach (var violation in violations)
            {
                if (violation == null)
                {
                    continue;
                }

                list.Add(violation);
                StoryViolations.Add(violation);
            }
        }

        public void AddNote(string phase, string note)
        {
            if (string.IsNullOrWhiteSpace(phase) || string.IsNullOrWhiteSpace(note))
            {
                return;
            }

            if (!phaseNotes.TryGetValue(phase, out var list))
            {
                list = new List<string>();
                phaseNotes[phase] = list;
            }

            list.Add(note);
        }

        public bool ShouldStop(bool stopOnFirstViolation) => stopOnFirstViolation && StoryViolations.Count > 0;

        public string GenerateSummary()
        {
            var builder = new StringBuilder();
            builder.AppendLine("=== Story Test Production Excellence Report ===");
            builder.AppendLine($"Started:  {StartedAtUtc:u}");
            builder.AppendLine($"Finished: {CompletedAtUtc:u}");
            builder.AppendLine($"Duration: {Duration.TotalSeconds:F2}s");
            builder.AppendLine($"Violations: {StoryViolations.Count}");
            builder.AppendLine($"Production Readiness Score: {ProductionReadinessScore:F1}%");
            builder.AppendLine();

            foreach (var phase in phaseViolations.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
            {
                builder.AppendLine($"[{phase}] {phaseViolations[phase].Count} violation(s)");
                foreach (var violation in phaseViolations[phase])
                {
                    builder.AppendLine($"  - {violation.Type}.{violation.Member}: {violation.Violation}");
                }

                if (phaseNotes.TryGetValue(phase, out var notes) && notes.Count > 0)
                {
                    foreach (var note in notes)
                    {
                        builder.AppendLine($"    Note: {note}");
                    }
                }

                builder.AppendLine();
            }

            var extraNotes = phaseNotes.Keys
                .Where(k => !phaseViolations.ContainsKey(k))
                .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (extraNotes.Count > 0)
            {
                builder.AppendLine("Additional Notes:");
                foreach (var phase in extraNotes)
                {
                    foreach (var note in phaseNotes[phase])
                    {
                        builder.AppendLine($"[{phase}] {note}");
                    }
                }
                builder.AppendLine();
            }

            return builder.ToString();
        }
    }

    /// <summary>
    /// Snapshot of the configuration used during validation.
    /// </summary>
    [Serializable]
    public class ValidationConfigurationSnapshot
    {
        [FormerlySerializedAs("StoryIntegrity")] public bool storyIntegrity;
        [FormerlySerializedAs("ConceptualValidation")] public bool conceptualValidation;
        [FormerlySerializedAs("CodeCoverage")] public bool codeCoverage;
        [FormerlySerializedAs("ArchitecturalCompliance")] public bool architecturalCompliance;
        [FormerlySerializedAs("SyncPointPerformance")] public bool syncPointPerformance;
        [FormerlySerializedAs("StopOnFirstViolation")] public bool stopOnFirstViolation;
        [FormerlySerializedAs("IncludeUnityAssemblies")] public bool includeUnityAssemblies;
        [FormerlySerializedAs("AssemblyFilters")] public string[] assemblyFilters = Array.Empty<string>();
    }

    /// <summary>
    /// Performance-related results captured during validation.
    /// </summary>
    [Serializable]
    public class ValidationPerformanceSnapshot
    {
        public bool? SyncPointPassed;
        [FormerlySerializedAs("SyncPointReportPath")] public string syncPointReportPath;
    }
}
