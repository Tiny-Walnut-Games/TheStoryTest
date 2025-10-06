using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest
{
    /// <summary>
    /// Validates that Story Test validation acts coordinate efficiently when executed in parallel.
    /// Provides performance telemetry to highlight bottlenecks or excessive skew between workers.
    /// </summary>
    [StoryIgnore("Performance testing infrastructure for story validation")]
    public static class StoryTestSyncPointValidator
    {
        private const int ActorCount = 9; // Mirrors the 9 validation acts

        /// <summary>
        /// Container for performance metrics captured during sync-point validation.
        /// </summary>
        private class SyncPointTestResults
        {
            public int TotalActors { get; set; }
            public int TotalOperations { get; set; }
            public long OverallTimeMs { get; set; }
            public double OperationsPerSecond { get; set; }
            public long AverageActorTimeMs { get; set; }
            public long MinActorTimeMs { get; set; }
            public long MaxActorTimeMs { get; set; }
            public double TimingVariationPercent { get; set; }
            public bool HasBottleneck { get; set; }
            public bool IsComedySkitDetected { get; set; }
            public List<string> Warnings { get; } = new List<string>();
            public List<string> Successes { get; } = new List<string>();

            public string GenerateReport()
            {
                var builder = new StringBuilder();
                builder.AppendLine("=== Story Test Sync-Point Report ===");
                builder.AppendLine($"Actors: {TotalActors}");
                builder.AppendLine($"Total Operations: {TotalOperations}");
                builder.AppendLine($"Overall Time: {OverallTimeMs} ms");
                builder.AppendLine($"Ops/sec: {OperationsPerSecond:F0}");
                builder.AppendLine($"Actor Times (ms) -> Avg: {AverageActorTimeMs}, Min: {MinActorTimeMs}, Max: {MaxActorTimeMs}");
                builder.AppendLine($"Timing Variation: {TimingVariationPercent:F1}%");
                builder.AppendLine();

                if (Warnings.Count > 0)
                {
                    builder.AppendLine("Warnings:");
                    foreach (var warning in Warnings)
                    {
                        builder.AppendLine($"  ⚠️  {warning}");
                    }
                    builder.AppendLine();
                }

                if (Successes.Count > 0)
                {
                    builder.AppendLine("Successes:");
                    foreach (var success in Successes)
                    {
                        builder.AppendLine($"  ✅ {success}");
                    }
                    builder.AppendLine();
                }

                if (!HasBottleneck && !IsComedySkitDetected && Warnings.Count == 0)
                {
                    builder.AppendLine("🎉 Story validation sync-points are flowing smoothly! 🎉");
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// Runs a sync-point stress test with configurable iterations and concurrency.
        /// </summary>
        private static async Task<SyncPointTestResults> RunSyncPointStressTest(int iterationsPerActor = 25, int concurrentBatches = 2)
        {
            var assemblies = GetProjectAssemblies();
            if (assemblies.Length == 0)
            {
                UnityDebugLogWarning("[Story Test] No project assemblies found for sync-point validation.");
                return new SyncPointTestResults
                {
                    TotalActors = ActorCount * concurrentBatches,
                    TotalOperations = 0,
                    OverallTimeMs = 0,
                    OperationsPerSecond = 0
                };
            }

            var rules = StoryIntegrityValidator.GetRegisteredRules();
            if (rules.Count == 0)
            {
                UnityDebugLogWarning("[Story Test] No validation rules registered – sync-point test skipped.");
                return new SyncPointTestResults();
            }

            var allTasks = new List<Task<long>>();
            var stopwatch = Stopwatch.StartNew();

            for (int batch = 0; batch < concurrentBatches; batch++)
            {
                var syncPoint = new TaskCompletionSource<bool>();
                var batchTasks = new List<Task<long>>();

                for (int actor = 0; actor < ActorCount; actor++)
                {
                    var actorId = batch * ActorCount + actor;
                    batchTasks.Add(PerformRealStoryValidation(actorId, iterationsPerActor, syncPoint.Task, assemblies, rules));
                }

                // Give actors time to reach the sync point before release
                await Task.Delay(5);
                syncPoint.SetResult(true);
                allTasks.AddRange(batchTasks);
            }

            var actorTimings = await Task.WhenAll(allTasks);
            stopwatch.Stop();

            var results = new SyncPointTestResults
            {
                TotalActors = ActorCount * concurrentBatches,
                TotalOperations = ActorCount * concurrentBatches * iterationsPerActor,
                OverallTimeMs = Math.Max(1, stopwatch.ElapsedMilliseconds)
            };

            results.OperationsPerSecond = results.TotalOperations * 1000.0 / results.OverallTimeMs;
            results.AverageActorTimeMs = (long)actorTimings.Average();
            results.MinActorTimeMs = actorTimings.Min();
            results.MaxActorTimeMs = actorTimings.Max();
            var spread = results.MaxActorTimeMs - results.MinActorTimeMs;
            results.TimingVariationPercent = results.AverageActorTimeMs > 0
                ? spread * 100.0 / results.AverageActorTimeMs
                : 0.0;

            results.IsComedySkitDetected = results.TimingVariationPercent > 150.0;
            if (results.IsComedySkitDetected)
            {
                results.Warnings.Add($"High timing variation detected ({results.TimingVariationPercent:F1}%). Actors may be blocking each other.");
            }
            else
            {
                results.Successes.Add("Timing variation within acceptable range.");
            }

            results.HasBottleneck = results.OperationsPerSecond < 1000.0;
            if (results.HasBottleneck)
            {
                results.Warnings.Add($"Throughput is low ({results.OperationsPerSecond:F0} ops/sec). Consider profiling validation rules.");
            }
            else
            {
                results.Successes.Add($"Throughput healthy at {results.OperationsPerSecond:F0} ops/sec.");
            }

            UnityDebugLog("[Story Test] Sync-point stress test completed.\n" + results.GenerateReport());
            return results;
        }

        /// <summary>
        /// Lightweight helper that runs the stress test with default parameters.
        /// </summary>
        public static async Task<bool> QuickSyncPointTest()
        {
            var results = await RunSyncPointStressTest(16);
            return !results.HasBottleneck && !results.IsComedySkitDetected;
        }

        /// <summary>
        /// Runs the stress test and exports the summary report to disk.
        /// </summary>
        public static async Task<bool> QuickSyncPointTestAndExport(string exportPath)
        {
            var results = await RunSyncPointStressTest(16);
            try
            {
                if (!string.IsNullOrEmpty(exportPath))
                {
                    var directory = Path.GetDirectoryName(exportPath);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    await File.WriteAllTextAsync(exportPath, results.GenerateReport());
                    UnityDebugLog($"[Story Test] Sync-point report exported to: {exportPath}");
                }
            }
            catch (Exception ex)
            {
                UnityDebugLogWarning($"[Story Test] Failed to export sync-point report: {ex.Message}");
            }

            return !results.HasBottleneck && !results.IsComedySkitDetected;
        }

        private static async Task<long> PerformRealStoryValidation(int actorId, int iterations, Task syncPoint, Assembly[] assemblies, IReadOnlyList<ValidationRule> rules)
        {
            await syncPoint.ConfigureAwait(false);
            var stopwatch = Stopwatch.StartNew();

            var rule = rules.Count > 0 ? rules[actorId % rules.Count] : null;
            if (rule == null)
            {
                stopwatch.Stop();
                return stopwatch.ElapsedMilliseconds;
            }

            for (int i = 0; i < iterations; i++)
            {
                var assembly = assemblies[i % assemblies.Length];
                foreach (var type in GetTypesSafe(assembly).Take(16))
                {
                    if (HasStoryIgnore(type))
                    {
                        continue;
                    }

                    foreach (var member in EnumerateMembers(type).Take(32))
                    {
                        try
                        {
                            rule(member, out _);
                        }
                        catch
                        {
                            // Intentionally ignore rule exceptions during performance pass
                        }
                    }
                }

                if (i % 3 == 0)
                {
                    await Task.Yield();
                }
            }

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        private static Assembly[] GetProjectAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(IsProjectAssembly)
                .ToArray();
        }

        private static bool IsProjectAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                return false;
            }

            var name = assembly.GetName().Name;
            if (name.StartsWith("System", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Mono.", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("nunit", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private static IEnumerable<Type> GetTypesSafe(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null);
            }
            catch
            {
                return Enumerable.Empty<Type>();
            }
        }

        private static IEnumerable<MemberInfo> EnumerateMembers(Type type)
        {
            yield return type;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                       BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            foreach (var method in type.GetMethods(flags))
            {
                yield return method;
            }

            foreach (var property in type.GetProperties(flags))
            {
                yield return property;
            }

            foreach (var field in type.GetFields(flags))
            {
                yield return field;
            }
        }

        private static bool HasStoryIgnore(MemberInfo member)
        {
            return member?.GetCustomAttributes(typeof(StoryIgnoreAttribute), true).Any() == true;
        }

        private static void UnityDebugLog(string message)
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL || UNITY_SERVER
            UnityEngine.Debug.Log(message);
#else
            Debug.WriteLine(message);
#endif
        }

        private static void UnityDebugLogWarning(string message)
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL || UNITY_SERVER
            UnityEngine.Debug.LogWarning(message);
#else
            Debug.WriteLine(message);
#endif
        }
    }
}
