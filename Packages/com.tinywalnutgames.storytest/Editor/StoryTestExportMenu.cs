#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Editor
{
    public static class StoryTestExportMenu
    {
        private const string MenuPath = "Tiny Walnut Games/The Story Test/Run Story Test and Export Report";

        private static MenuValidationRun _currentRun;

        [MenuItem(MenuPath, false, 0)]
        public static void RunAndExportStoryTest()
        {
            if (_currentRun is { IsActive: true })
            {
                EditorUtility.DisplayDialog("Story Test Validation", "Validation is already running.", "OK");
                return;
            }

            _currentRun = new MenuValidationRun();
            _currentRun.Begin();
        }

        [MenuItem(MenuPath, true)]
        public static bool ValidateRunAndExportStoryTest()
        {
            // DISABLED: This menu option is unreliable due to PlayMode timing issues
            // Use the in-scene component + manual trigger button instead
            // See: "Add Story Test Component to Scene" menu
            return false;
        }

        private sealed class MenuValidationRun
        {
            private const string ProgressTitle = "Story Test Validation";

            private readonly string exportPathSetting;
            private readonly string exportPathAbsolute;

            private ProductionExcellenceStoryTest runner;
            private GameObject runnerGameObject;

            private bool createdRunner;
            private bool startedPlayMode;
            private bool waitingForPlayMode;
            private bool awaitingExit;
            private bool completed;

            private bool runnerSubscribed;
            private bool playModeSubscribed;
            private bool editorUpdateSubscribed;

            private bool previousExportReport;
            private string previousExportPath;
            private bool previousStopOnFirstViolation;
            private bool previousValidateAutomaticallyOnStart;

            private ValidationReport pendingReport;
            private string failureMessage;

            public bool IsActive { get; private set; }

            public MenuValidationRun()
            {
                var settings1 = SafeGetSettings();
                exportPathSetting = !string.IsNullOrWhiteSpace(settings1?.exportPath)
                    ? settings1.exportPath
                    : ".debug/storytest_report.txt";
                exportPathAbsolute = ResolveAbsolutePath(exportPathSetting);
            }

            public void Begin()
            {
                IsActive = true;
                EditorUtility.DisplayProgressBar(ProgressTitle, "Preparing production validation…", 0.05f);

                if (!EditorApplication.isPlaying)
                {
                    const string promptTitle = "Story Test Validation";
                    const string promptMessage =
                        "Production validation needs Play Mode. Enter Play Mode now and run the Story Test?";
                    if (!EditorUtility.DisplayDialog(promptTitle, promptMessage, "Enter Play Mode", "Cancel"))
                    {
                        Cleanup();
                        return;
                    }

                    startedPlayMode = true;
                    waitingForPlayMode = true;
                    SubscribePlayModeEvents();
                    SubscribeEditorUpdate(); // Start progress updates immediately
                    EditorUtility.DisplayProgressBar(ProgressTitle, "Entering Play Mode…", 0.1f);
                    Debug.Log("[Story Test] Requesting Play Mode entry for validation...");
                    EditorApplication.isPlaying = true;
                }
                else
                {
                    EditorUtility.DisplayProgressBar(ProgressTitle, "Starting validation…", 0.2f);
                    EditorApplication.delayCall += StartValidation;
                }
            }

            private void StartValidation()
            {
                Debug.Log("[Story Test] StartValidation() called");

                if (!IsActive)
                {
                    Debug.LogWarning("[Story Test] StartValidation aborted: IsActive = false");
                    return;
                }

                if (!EditorApplication.isPlaying)
                {
                    Debug.LogWarning("[Story Test] StartValidation aborted: Not in Play Mode");
                    Fail("Play Mode ended before validation could start.");
                    return;
                }

                Debug.Log("[Story Test] Setting up validation runner...");

                try
                {
                    if (runner == null)
                    {
                        Debug.Log("[Story Test] Locating existing runner...");
                        runner = LocateRunner();
                    }

                    if (runner == null)
                    {
                        Debug.Log("[Story Test] Creating temporary runner GameObject...");
                        runnerGameObject = new GameObject("Story Test Runner (Temp)")
                        {
                            hideFlags = HideFlags.DontSave
                        };
                        runner = runnerGameObject.AddComponent<ProductionExcellenceStoryTest>();
                        ConfigureTempRunner(runner);
                        createdRunner = true;
                        Debug.Log("[Story Test] Temporary runner created");
                    }
                    else
                    {
                        Debug.Log($"[Story Test] Using existing runner: {runner.gameObject.name}");
                    }

                    CaptureRunnerState(runner);
                    ConfigureRunnerForMenu(runner);

                    if (!runnerSubscribed)
                    {
                        runner.OnValidationComplete += OnValidationComplete;
                        runnerSubscribed = true;
                    }

                    EditorUtility.DisplayProgressBar(ProgressTitle, "Running validation…", 0.4f);
                    Debug.Log("[Story Test] Play Mode validation triggered from menu.");

                    // Subscribe to editor updates to show progress
                    SubscribeEditorUpdate();

                    Debug.Log("[Story Test] Calling runner.ValidateOnDemand()...");
                    runner.ValidateOnDemand();
                    Debug.Log("[Story Test] ValidateOnDemand() returned");
                }
                catch (Exception ex)
                {
                    Fail($"Failed to start validation: {ex.Message}");
                }
            }

            private void OnValidationComplete(ValidationReport report)
            {
                if (!IsActive || completed)
                {
                    return;
                }

                completed = true;
                pendingReport = report;

                Debug.Log("[Story Test] Play Mode validation completed. Preparing report feedback…");

                UnsubscribeEditorUpdate();

                RestoreRunnerState();
                if (createdRunner && runnerGameObject is not null)
                {
                    UnityEngine.Object.Destroy(runnerGameObject);
                }

                EditorUtility.ClearProgressBar();

                if (runnerSubscribed && runner is not null)
                {
                    runner.OnValidationComplete -= OnValidationComplete;
                    runnerSubscribed = false;
                }

                if (createdRunner)
                {
                    runner = null;
                    runnerGameObject = null;
                }

                if (startedPlayMode)
                {
                    awaitingExit = true;
                    SubscribePlayModeEvents();
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.isPlaying = false;
                    }
                    else
                    {
                        awaitingExit = false;
                        ShowSummaryAndFinish();
                    }
                }
                else
                {
                    ShowSummaryAndFinish();
                }
            }

            private void OnPlayModeStateChanged(PlayModeStateChange change)
            {
                if (!IsActive)
                {
                    return;
                }

                if (waitingForPlayMode)
                {
                    switch (change)
                    {
                        case PlayModeStateChange.EnteredPlayMode:
                            Debug.Log("[Story Test] ✓ Play Mode entered successfully. Waiting for stability...");
                            waitingForPlayMode = false;
                            EditorUtility.DisplayProgressBar(ProgressTitle, "Waiting for PlayMode to stabilize…", 0.2f);

                            // Wait multiple frames after PlayMode entry to ensure all domain reload/import is complete
                            EditorApplication.delayCall += () =>
                            {
                                EditorApplication.delayCall += () =>
                                {
                                    EditorApplication.delayCall += () =>
                                    {
                                        EditorApplication.delayCall += () =>
                                        {
                                            Debug.Log("[Story Test] PlayMode stabilized. Starting validation...");
                                            StartValidation();
                                        };
                                    };
                                };
                            };
                            break;
                        case PlayModeStateChange.ExitingPlayMode:
                            Debug.LogWarning("[Story Test] Play Mode exited unexpectedly before validation started.");
                            waitingForPlayMode = false;
                            Fail("Play Mode exited before validation could start.");
                            break;
                        case PlayModeStateChange.EnteredEditMode:
                        case PlayModeStateChange.ExitingEditMode:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(change), change, null);
                    }

                    return;
                }

                if (awaitingExit)
                {
                    if (change != PlayModeStateChange.EnteredEditMode) return;
                    Debug.Log("[Story Test] ✓ Exited Play Mode. Showing validation results...");
                    awaitingExit = false;
                    ShowSummaryAndFinish();

                    return;
                }

                if (!completed && change == PlayModeStateChange.ExitingPlayMode)
                {
                    Debug.LogWarning("[Story Test] Play Mode exited before validation completed.");
                    Fail("Play Mode exited before validation completed.");
                }
            }

            private void ShowSummaryAndFinish()
            {
                UnsubscribePlayModeEvents();

                EditorUtility.ClearProgressBar();

                var reportExists = File.Exists(exportPathAbsolute);
                var title = "Story Test Validation Complete";
                string message;

                if (pendingReport != null)
                {
                    var status = pendingReport.IsFullyCompliant ? "✓ PASSED" : "✗ FAILED";
                    var violationCount = pendingReport.StoryViolations.Count;

                    message = pendingReport.IsFullyCompliant
                        ? "Production validation passed with no violations."
                        : $"Validation detected {violationCount} violation(s).";
                    message += "\n\n";
                    message += reportExists
                        ? $"Report saved to:\n{exportPathAbsolute}"
                        : $"⚠️ Report file was not found at:\n{exportPathAbsolute}";

                    Debug.Log($"[Story Test] {status} - Validation completed with {violationCount} violation(s). Report: {exportPathAbsolute}");
                }
                else
                {
                    message = string.IsNullOrEmpty(failureMessage)
                        ? "Validation did not complete."
                        : failureMessage;

                    Debug.LogWarning($"[Story Test] Validation did not complete. {failureMessage}");
                }

                if (reportExists)
                {
                    var option = EditorUtility.DisplayDialogComplex(
                        title,
                        message,
                        "Reveal Report",
                        "View Console",
                        "Close");

                    if (option == 0)
                    {
                        EditorUtility.RevealInFinder(exportPathAbsolute);
                    }
                    else if (option == 1)
                    {
                        EditorApplication.ExecuteMenuItem("Window/General/Console");
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog(title, message, "OK");
                }

                Cleanup();
            }

            private void Fail(string reason)
            {
                failureMessage = reason;
                Debug.LogWarning($"[Story Test] Menu validation aborted: {reason}");

                EditorUtility.ClearProgressBar();

                RestoreRunnerState();
                if (createdRunner && runnerGameObject != null)
                {
                    UnityEngine.Object.Destroy(runnerGameObject);
                    runner = null;
                    runnerGameObject = null;
                }

                if (runnerSubscribed && runner != null)
                {
                    runner.OnValidationComplete -= OnValidationComplete;
                    runnerSubscribed = false;
                }

                UnsubscribePlayModeEvents();

                if (startedPlayMode && EditorApplication.isPlaying)
                {
                    EditorApplication.isPlaying = false;
                }

                EditorUtility.DisplayDialog("Story Test Validation", reason, "OK");
                Cleanup();
            }

            private void Cleanup()
            {
                UnsubscribePlayModeEvents();
                UnsubscribeEditorUpdate();
                EditorUtility.ClearProgressBar();
                runner = null;
                runnerGameObject = null;
                IsActive = false;
                _currentRun = null;
            }

            private void SubscribePlayModeEvents()
            {
                if (playModeSubscribed)
                {
                    return;
                }

                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                playModeSubscribed = true;
            }

            private void UnsubscribePlayModeEvents()
            {
                if (!playModeSubscribed)
                {
                    return;
                }

                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                playModeSubscribed = false;
            }

            private void SubscribeEditorUpdate()
            {
                if (editorUpdateSubscribed)
                {
                    return;
                }

                EditorApplication.update += OnEditorUpdate;
                editorUpdateSubscribed = true;
            }

            private void UnsubscribeEditorUpdate()
            {
                if (!editorUpdateSubscribed)
                {
                    return;
                }

                EditorApplication.update -= OnEditorUpdate;
                editorUpdateSubscribed = false;
            }

            private void OnEditorUpdate()
            {
                if (!IsActive || completed)
                {
                    return;
                }

                // Animate progress bar during wait states
                if (waitingForPlayMode)
                {
                    // Waiting for PlayMode to start
                    var progress = 0.1f + (float)(EditorApplication.timeSinceStartup % 1.0) * 0.05f;
                    EditorUtility.DisplayProgressBar(ProgressTitle, "Entering Play Mode…", progress);
                    return;
                }

                // Update progress bar to show validation is running
                if (runner != null && runner.IsValidating)
                {
                    // Animate the progress bar to show activity
                    var progress = 0.4f + (float)(EditorApplication.timeSinceStartup % 1.0) * 0.2f;
                    EditorUtility.DisplayProgressBar(ProgressTitle, "Validating code integrity…", progress);
                }
            }

            private void CaptureRunnerState(ProductionExcellenceStoryTest instance)
            {
                previousExportReport = instance.exportReport;
                previousExportPath = instance.exportPath;
                previousStopOnFirstViolation = instance.stopOnFirstViolation;
                previousValidateAutomaticallyOnStart = instance.validateAutomaticallyOnStart;
            }

            private void ConfigureRunnerForMenu(ProductionExcellenceStoryTest instance)
            {
                instance.exportReport = true;
                instance.exportPath = exportPathSetting;
                instance.stopOnFirstViolation = false;
                instance.validateAutomaticallyOnStart = false;

                EnsureReportDirectory(exportPathAbsolute);
            }

            private static void ConfigureTempRunner(ProductionExcellenceStoryTest instance)
            {
                // Disable Story Integrity in PlayMode - it's too heavy and causes frame budget overruns
                // Use the non-PlayMode "Validate Story Integrity" or "Generate Detailed Report" menus instead
                instance.enableStoryIntegrity = false;
                instance.enableConceptualValidation = true;
                instance.enableArchitecturalCompliance = true;
                instance.enableCodeCoverage = false;
                instance.enableSyncPointPerformance = true;
                instance.overrideUnityAssemblies = false;
                instance.validateAutomaticallyOnStart = false;
                instance.stopOnFirstViolation = false;
            }

            private void RestoreRunnerState()
            {
                if (runner is null)
                {
                    return;
                }

                try
                {
                    runner.exportReport = previousExportReport;
                    runner.exportPath = previousExportPath;
                    runner.stopOnFirstViolation = previousStopOnFirstViolation;
                    runner.validateAutomaticallyOnStart = previousValidateAutomaticallyOnStart;
                }
                catch
                {
                    // ignored
                }
            }

            private static ProductionExcellenceStoryTest LocateRunner()
            {
#if UNITY_2023_1_OR_NEWER
                return UnityEngine.Object.FindFirstObjectByType<ProductionExcellenceStoryTest>(FindObjectsInactive.Exclude);
#elif UNITY_2022_2_OR_NEWER
                return UnityEngine.Object.FindFirstObjectByType<ProductionExcellenceStoryTest>();
#else
                return UnityEngine.Object.FindObjectOfType<ProductionExcellenceStoryTest>();
#endif
            }

            private static void EnsureReportDirectory(string absolutePath)
            {
                try
                {
                    var directory = Path.GetDirectoryName(absolutePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Story Test] Could not ensure report directory: {ex.Message}");
                }
            }

            private static StoryTestSettings SafeGetSettings()
            {
                try
                {
                    return StoryTestSettings.Instance;
                }
                catch
                {
                    return new StoryTestSettings();
                }
            }

            private static string ResolveAbsolutePath(string targetPath)
            {
                if (string.IsNullOrWhiteSpace(targetPath))
                {
                    targetPath = ".debug/storytest_report.txt";
                }

                if (Path.IsPathRooted(targetPath))
                {
                    return Path.GetFullPath(targetPath);
                }

                var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                var normalized = targetPath.Replace('\\', '/');
                var fullPath = Path.GetFullPath(Path.Combine(projectRoot, normalized));

                // Strip ISSUE-prefix from filename if present (e.g., ISSUE-2 -> 2)
                var directory = Path.GetDirectoryName(fullPath) ?? string.Empty;
                var filename = Path.GetFileName(fullPath);
                var extension = Path.GetExtension(filename);
                var nameWithoutExt = Path.GetFileNameWithoutExtension(filename);

                if (!nameWithoutExt.StartsWith("ISSUE-", StringComparison.OrdinalIgnoreCase)) return fullPath;
                nameWithoutExt = nameWithoutExt[6..]; // Remove "ISSUE-" prefix
                filename = nameWithoutExt + extension;
                fullPath = Path.Combine(directory, filename);

                return fullPath;
            }
        }
    }
}
#endif
