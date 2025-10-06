#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using TinyWalnutGames.StoryTest;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Editor
{
    public static class StoryTestExportMenu
    {
        private const string MenuPath = "Tiny Walnut Games/The Story Test/Run Story Test and Export Report";

        private static MenuValidationRun currentRun;

        [MenuItem(MenuPath, false, 0)]
        public static void RunAndExportStoryTest()
        {
            if (currentRun != null && currentRun.IsActive)
            {
                EditorUtility.DisplayDialog("Story Test Validation", "Validation is already running.", "OK");
                return;
            }

            currentRun = new MenuValidationRun();
            currentRun.Begin();
        }

        [MenuItem(MenuPath, true)]
        public static bool ValidateRunAndExportStoryTest()
        {
            return currentRun == null || !currentRun.IsActive;
        }

        private sealed class MenuValidationRun
        {
            private const string ProgressTitle = "Story Test Validation";

            private readonly StoryTestSettings settings;
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

            private bool previousExportReport;
            private string previousExportPath;
            private bool previousStopOnFirstViolation;
            private bool previousValidateAutomaticallyOnStart;

            private ValidationReport pendingReport;
            private string failureMessage;

            public bool IsActive { get; private set; }

            public MenuValidationRun()
            {
                settings = SafeGetSettings();
                exportPathSetting = !string.IsNullOrWhiteSpace(settings?.exportPath)
                    ? settings.exportPath
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
                    EditorUtility.DisplayProgressBar(ProgressTitle, "Entering Play Mode…", 0.1f);
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
                if (!IsActive)
                {
                    return;
                }

                if (!EditorApplication.isPlaying)
                {
                    Fail("Play Mode ended before validation could start.");
                    return;
                }

                try
                {
                    if (runner == null)
                    {
                        runner = LocateRunner();
                    }

                    if (runner == null)
                    {
                        runnerGameObject = new GameObject("Story Test Runner (Temp)");
                        runnerGameObject.hideFlags = HideFlags.DontSave;
                        runner = runnerGameObject.AddComponent<ProductionExcellenceStoryTest>();
                        ConfigureTempRunner(runner);
                        createdRunner = true;
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
                    runner.ValidateOnDemand();
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

                RestoreRunnerState();
                if (createdRunner && runnerGameObject != null)
                {
                    UnityEngine.Object.Destroy(runnerGameObject);
                }

                EditorUtility.ClearProgressBar();

                if (runnerSubscribed && runner != null)
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
                    if (change == PlayModeStateChange.EnteredPlayMode)
                    {
                        waitingForPlayMode = false;
                        EditorUtility.DisplayProgressBar(ProgressTitle, "Starting validation…", 0.2f);
                        EditorApplication.delayCall += StartValidation;
                    }
                    else if (change == PlayModeStateChange.ExitingPlayMode)
                    {
                        waitingForPlayMode = false;
                        Fail("Play Mode exited before validation could start.");
                    }

                    return;
                }

                if (awaitingExit)
                {
                    if (change == PlayModeStateChange.EnteredEditMode)
                    {
                        awaitingExit = false;
                        ShowSummaryAndFinish();
                    }

                    return;
                }

                if (!completed && change == PlayModeStateChange.ExitingPlayMode)
                {
                    Fail("Play Mode exited before validation completed.");
                }
            }

            private void ShowSummaryAndFinish()
            {
                UnsubscribePlayModeEvents();

                EditorUtility.ClearProgressBar();

                bool reportExists = File.Exists(exportPathAbsolute);
                string title = "Story Test Validation Complete";
                string message;

                if (pendingReport != null)
                {
                    message = pendingReport.IsFullyCompliant
                        ? "Production validation passed with no violations."
                        : $"Validation detected {pendingReport.StoryViolations.Count} violation(s).";
                    message += "\n\n";
                    message += reportExists
                        ? $"Report saved to:\n{exportPathAbsolute}"
                        : $"⚠️ Report file was not found at:\n{exportPathAbsolute}";
                }
                else
                {
                    message = string.IsNullOrEmpty(failureMessage)
                        ? "Validation did not complete."
                        : failureMessage;
                }

                Debug.Log($"[Story Test] Validation summary ready. Report path: {exportPathAbsolute}");

                if (reportExists)
                {
                    int option = EditorUtility.DisplayDialogComplex(
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
                EditorUtility.ClearProgressBar();
                runner = null;
                runnerGameObject = null;
                IsActive = false;
                currentRun = null;
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

            private void CaptureRunnerState(ProductionExcellenceStoryTest instance)
            {
                previousExportReport = instance.ExportReport;
                previousExportPath = instance.ExportPath;
                previousStopOnFirstViolation = instance.StopOnFirstViolation;
                previousValidateAutomaticallyOnStart = instance.ValidateAutomaticallyOnStart;
            }

            private void ConfigureRunnerForMenu(ProductionExcellenceStoryTest instance)
            {
                instance.ExportReport = true;
                instance.ExportPath = exportPathSetting;
                instance.StopOnFirstViolation = false;
                instance.ValidateAutomaticallyOnStart = false;

                EnsureReportDirectory(exportPathAbsolute);
            }

            private void ConfigureTempRunner(ProductionExcellenceStoryTest instance)
            {
                instance.EnableStoryIntegrity = true;
                instance.EnableConceptualValidation = true;
                instance.EnableArchitecturalCompliance = true;
                instance.EnableCodeCoverage = false;
                instance.EnableSyncPointPerformance = true;
                instance.OverrideUnityAssemblies = false;
                instance.ValidateAutomaticallyOnStart = false;
                instance.StopOnFirstViolation = false;
            }

            private void RestoreRunnerState()
            {
                if (runner == null)
                {
                    return;
                }

                try
                {
                    runner.ExportReport = previousExportReport;
                    runner.ExportPath = previousExportPath;
                    runner.StopOnFirstViolation = previousStopOnFirstViolation;
                    runner.ValidateAutomaticallyOnStart = previousValidateAutomaticallyOnStart;
                }
                catch
                {
                    // ignored
                }
            }

            private static ProductionExcellenceStoryTest LocateRunner()
            {
#if UNITY_2023_1_OR_NEWER
                return UnityEngine.Object.FindFirstObjectByType<ProductionExcellenceStoryTest>(UnityEngine.FindObjectsInactive.Exclude);
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

                string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                string normalized = targetPath.Replace('\\', '/');
                return Path.GetFullPath(Path.Combine(projectRoot, normalized));
            }
        }
    }
}
#endif
