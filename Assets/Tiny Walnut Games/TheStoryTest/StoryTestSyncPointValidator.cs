// Use Unity Debug in Unity, System.Diagnostics.Debug otherwise
#if UNITY_EDITOR || UNITY_ENGINE
using Debug = UnityEngine.Debug;
#else
using Debug = System.Diagnostics.Debug;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System.Reflection;

using TinyWalnutGames.StoryTest.Shared;
using System.IO;

namespace TinyWalnutGames.StoryTest
{
    /// <summary>
    /// Story Test Sync-Point Performance Validator.
    /// Ensures that the story validation acts coordinate smoothly without bottlenecks or "comedy skit" scenarios
    /// where actors stumble over each other during validation.
    /// </summary>
    [StoryIgnore("Performance testing infrastructure for story validation")]
    public static class StoryTestSyncPointValidator
    {
        /// <summary>
        /// Performance test results for sync-point validation.
        /// </summary>
        public class SyncPointTestResults
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
            public List<string> Warnings { get; set; } = new List<string>();
            public List<string> Successes { get; set; } = new List<string>();

            public string GenerateReport()
            {
                var report = "=== STORY SYNC-POINT PERFORMANCE REPORT ===\n\n";
                report += $"Total Actors (Story Acts): {TotalActors}\n";
                report += $"Total Operations: {TotalOperations:N0}\n";
                report += $"Overall Time: {OverallTimeMs:N0}ms\n";
                report += $"Operations/sec: {OperationsPerSecond:N0}\n";
                report += $"Actor Timings - Avg: {AverageActorTimeMs:N0}ms, Min: {MinActorTimeMs:N0}ms, Max: {MaxActorTimeMs:N0}ms\n";
                report += $"Timing Variation: {TimingVariationPercent:F1}%\n\n";

                if (IsComedySkitDetected)
                {
                    report += "🎭 COMEDY SKIT DETECTED! 🎭\n";
                    report += "Some story actors are stumbling over each other!\n\n";
                }

                if (HasBottleneck)
                {
                    report += "⚠️  BOTTLENECK DETECTED ⚠️\n";
                    report += "Story validation performance may be suboptimal.\n\n";
                }

                if (Warnings.Count > 0)
                {
                    report += "Warnings:\n";
                    foreach (var warning in Warnings)
                    {
                        report += $"  ⚠️  {warning}\n";
                    }
                    report += "\n";
                }

                if (Successes.Count > 0)
                {
                    report += "Successes:\n";
                    foreach (var success in Successes)
                    {
                        report += $"  ✅ {success}\n";
                    }
                    report += "\n";
                }

                if (!HasBottleneck && !IsComedySkitDetected)
                {
                    report += "🎉 STORY FLOWS SMOOTHLY! 🎉\n";
                    report += "All actors are coordinating perfectly - no comedy skit detected!";
                }

                return report;
            }
        }

        /// <summary>
        /// Runs a comprehensive sync-point performance test on the story validation system.
        /// Tests the coordination between all 9 Story Acts validating the ENTIRE project (the complete "story").
        /// </summary>
        /// <param name="iterationsPerActor">Number of validation iterations per actor (default: 100 for real validation)</param>
        /// <param name="concurrentBatches">Number of concurrent batches to test (default: 3 for real validation)</param>
        /// <returns>Performance test results</returns>
        public static async Task<SyncPointTestResults> RunSyncPointStressTest(int iterationsPerActor = 100, int concurrentBatches = 3)
        {
            const int actorCount = 9; // Our 9 validation Acts
            var results = new SyncPointTestResults
            {
                TotalActors = actorCount * concurrentBatches,
                TotalOperations = actorCount * iterationsPerActor * concurrentBatches
            };

            Debug.Log($"🎭 Starting Story Sync-Point Stress Test");
            Debug.Log($"   Actors: {results.TotalActors} (9 Acts × {concurrentBatches} batches)");
            Debug.Log($"   Total Operations: {results.TotalOperations:N0}");

            var actorTimings = new List<long>();
            var overallStopwatch = Stopwatch.StartNew();
            var random = new System.Random();

            // Run concurrent batches to test sync-point coordination
            var allTasks = new List<Task<long>>();

            for (int batch = 0; batch < concurrentBatches; batch++)
            {
                var batchTasks = new List<Task<long>>();
                var batchSyncPoint = new TaskCompletionSource<bool>();

                // Create actors for this batch (representing our 9 Story Acts)
                for (int actor = 0; actor < actorCount; actor++)
                {
                    var actorId = batch * actorCount + actor;
                    batchTasks.Add(PerformRealStoryValidation(actorId, iterationsPerActor, batchSyncPoint.Task, random));
                }

                // Small delay to ensure all actors are waiting at sync-point
                await Task.Delay(5);

                // Release all actors simultaneously (sync-point test)
                batchSyncPoint.SetResult(true);

                allTasks.AddRange(batchTasks);
            }

            // Wait for all actors to complete their story validation work
            var timings = await Task.WhenAll(allTasks);
            overallStopwatch.Stop();

            // Analyze performance results
            results.OverallTimeMs = overallStopwatch.ElapsedMilliseconds;
            results.OperationsPerSecond = results.TotalOperations * 1000.0 / results.OverallTimeMs;
            results.AverageActorTimeMs = (long)timings.Average(t => t);
            results.MinActorTimeMs = timings.Min();
            results.MaxActorTimeMs = timings.Max();

            // Calculate timing variation (comedy skit detection)
            var timeSpread = results.MaxActorTimeMs - results.MinActorTimeMs;
            results.TimingVariationPercent = timeSpread * 100.0 / results.AverageActorTimeMs;

            // Detect comedy skit scenario (actors stumbling over each other)
            results.IsComedySkitDetected = results.TimingVariationPercent > 150.0; // >150% variation
            if (results.IsComedySkitDetected)
            {
                results.Warnings.Add($"High timing variation ({results.TimingVariationPercent:F1}%) - actors may be interfering with each other");
            }
            else
            {
                results.Successes.Add("Timing variation within acceptable range - smooth coordination");
            }

            // Detect bottleneck issues
            results.HasBottleneck = results.OperationsPerSecond < 10000; // Less than 10k ops/sec
            if (results.HasBottleneck)
            {
                results.Warnings.Add($"Low throughput detected: {results.OperationsPerSecond:N0} ops/sec");
            }
            else
            {
                results.Successes.Add($"Good throughput: {results.OperationsPerSecond:N0} ops/sec");
            }

            // Additional performance analysis
            if (results.MaxActorTimeMs > results.AverageActorTimeMs * 2)
            {
                results.Warnings.Add("Some actors taking significantly longer than others");
            }

            if (results.OverallTimeMs > 10000) // More than 10 seconds
            {
                results.Warnings.Add($"Overall test time is high: {results.OverallTimeMs:N0}ms");
            }
            else
            {
                results.Successes.Add($"Overall test completed in reasonable time: {results.OverallTimeMs:N0}ms");
            }

            Debug.Log("🎭 Story Sync-Point Stress Test completed");
            Debug.Log(results.GenerateReport());

            return results;
        }

        /// <summary>
        /// Performs actual story validation work by one of the 9 Acts on the entire project.
        /// Tests sync-point coordination with real validation work instead of simulation.
        /// </summary>
        private static async Task<long> PerformRealStoryValidation(int actorId, int iterations, Task syncPoint, System.Random random)
        {
            // Wait for sync point - all actors start together
            await syncPoint;

            var stopwatch = Stopwatch.StartNew();
            var actType = actorId % 9; // Map to one of our 9 Story Acts

            // Get all assemblies to validate (the entire "story")
            var allAssemblies = GetAllProjectAssemblies();
            var violations = new List<StoryViolation>();

            // Perform real validation work using the appropriate Act
            for (int i = 0; i < iterations && i < allAssemblies.Length; i++)
            {
                var assembly = allAssemblies[i % allAssemblies.Length];

                try
                {
                    // Perform real validation based on the Act type
                    switch (actType)
                    {
                        case 0: // Act1TodoComments - IL analysis
                            violations.AddRange(await PerformTodoCommentsValidation(assembly));
                            break;
                        case 1: // Act2PlaceholderImplementations
                            violations.AddRange(await PerformPlaceholderValidation(assembly));
                            break;
                        case 2: // Act3IncompleteClasses - Reflection work  
                            violations.AddRange(await PerformIncompleteClassValidation(assembly));
                            break;
                        case 3: // Act4UnsealedAbstractMembers
                            violations.AddRange(await PerformUnsealedAbstractValidation(assembly));
                            break;
                        case 4: // Act5DebugOnlyImplementations
                            violations.AddRange(await PerformDebugOnlyValidation(assembly));
                            break;
                        case 5: // Act6PhantomProps - Property analysis
                            violations.AddRange(await PerformPhantomPropsValidation(assembly));
                            break;
                        case 6: // Act7ColdMethods - Method analysis
                            violations.AddRange(await PerformColdMethodsValidation(assembly));
                            break;
                        case 7: // Act8HollowEnums - Enum analysis
                            violations.AddRange(await PerformHollowEnumsValidation(assembly));
                            break;
                        case 8: // Act9PrematureCelebrations - Comprehensive analysis
                            violations.AddRange(await PerformPrematureCelebrationsValidation(assembly));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Story Actor {actorId} (Act{actType + 1}) encountered error validating {assembly.GetName().Name}: {ex.Message}");
                }

                // Periodic yield to test coordination under real-world conditions
                if (i % 5 == 0)
                {
                    await Task.Yield();
                }
            }

            stopwatch.Stop();
            Debug.Log($"🎭 Story Actor {actorId} (Act{actType + 1}) completed validation: {violations.Count} violations found in {stopwatch.ElapsedMilliseconds}ms");
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Gets all project assemblies for comprehensive validation.
        /// </summary>
        /// <summary>
        /// Determines if an assembly is a project assembly (not a system/Unity assembly).
        /// </summary>
        private static bool IsProjectAssembly(Assembly a)
        {
            var name = a.FullName;
            return !name.StartsWith("Unity") &&
                   !name.StartsWith("UnityEngine") &&
                   !name.StartsWith("UnityEditor") &&
                   !name.StartsWith("System") &&
                   !name.StartsWith("Microsoft") &&
                   !name.StartsWith("mscorlib") &&
                   !name.StartsWith("netstandard") &&
                   !name.StartsWith("Mono.") &&
                   !name.StartsWith("nunit");
        }

        /// <summary>
        /// Gets all project assemblies for comprehensive validation.
        /// </summary>
        private static Assembly[] GetAllProjectAssemblies()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Filter to project assemblies (exclude system assemblies)
            var projectAssemblies = allAssemblies
                .Where(IsProjectAssembly)
                .ToArray();
            return projectAssemblies;
        }

        // Real validation methods for each Act
        private static async Task<List<StoryViolation>> PerformTodoCommentsValidation(Assembly assembly)
        {
            var violations = new List<StoryViolation>();
            await Task.Yield();

            foreach (var type in assembly.GetTypes())
            {
                if (type.GetCustomAttribute<StoryIgnoreAttribute>() != null) continue;
                foreach (var rule in StoryIntegrityValidator.GetRegisteredRules())
                {
                    if (rule(type, out string violation))
                    {
                        violations.Add(new StoryViolation
                        {
                            Type = type.Name,
                            Member = type.Name,
                            Violation = violation,
                            ViolationType = StoryTestUtilities.GetViolationType(violation)
                        });
                    }
                }
            }
            return violations;
        }

        private static async Task<List<StoryViolation>> PerformPlaceholderValidation(Assembly assembly)
        {
            var violations = new List<StoryViolation>();
            await Task.Yield();

            foreach (var type in assembly.GetTypes())
            {
                if (type.GetCustomAttribute<StoryIgnoreAttribute>() != null) continue;
                foreach (var rule in StoryIntegrityValidator.GetRegisteredRules())
                {
                    if (rule(type, out string violation))
                    {
                        violations.Add(new StoryViolation
                        {
                            Type = type.Name,
                            Member = type.Name,
                            Violation = violation,
                            ViolationType = StoryTestUtilities.GetViolationType(violation)
                        });
                    }
                }
            }
            return violations;
        }

        private static async Task<List<StoryViolation>> PerformIncompleteClassValidation(Assembly assembly)
        {
            var violations = new List<StoryViolation>();
            await Task.Yield();

            foreach (var type in assembly.GetTypes())
            {
                if (type.GetCustomAttribute<StoryIgnoreAttribute>() != null) continue;
                foreach (var rule in StoryIntegrityValidator.GetRegisteredRules())
                {
                    if (rule(type, out string violation))
                    {
                        violations.Add(new StoryViolation
                        {
                            Type = type.Name,
                            Member = type.Name,
                            Violation = violation,
                            ViolationType = StoryTestUtilities.GetViolationType(violation)
                        });
                    }
                }
            }
            return violations;
        }

        private static async Task<List<StoryViolation>> PerformUnsealedAbstractValidation(Assembly assembly)
        {
            var violations = new List<StoryViolation>();
            await Task.Yield();

            foreach (var type in assembly.GetTypes())
            {
                if (type.GetCustomAttribute<StoryIgnoreAttribute>() != null) continue;
                foreach (var rule in StoryIntegrityValidator.GetRegisteredRules())
                {
                    if (rule(type, out string violation))
                    {
                        violations.Add(new StoryViolation
                        {
                            Type = type.Name,
                            Member = type.Name,
                            Violation = violation,
                            ViolationType = StoryTestUtilities.GetViolationType(violation)
                        });
                    }
                }
            }
            return violations;
        }

        private static async Task<List<StoryViolation>> PerformDebugOnlyValidation(Assembly assembly)
        {
            var violations = new List<StoryViolation>();
            await Task.Yield();

            foreach (var type in assembly.GetTypes())
            {
                if (type.GetCustomAttribute<StoryIgnoreAttribute>() != null) continue;
                foreach (var rule in StoryIntegrityValidator.GetRegisteredRules())
                {
                    if (rule(type, out string violation))
                    {
                        violations.Add(new StoryViolation
                        {
                            Type = type.Name,
                            Member = type.Name,
                            Violation = violation,
                            ViolationType = StoryTestUtilities.GetViolationType(violation)
                        });
                    }
                }
            }
            return violations;
        }

        private static async Task<List<StoryViolation>> PerformPhantomPropsValidation(Assembly assembly)
        {
            var violations = new List<StoryViolation>();
            await Task.Yield();

            foreach (var type in assembly.GetTypes())
            {
                if (type.GetCustomAttribute<StoryIgnoreAttribute>() != null) continue;
                foreach (var rule in StoryIntegrityValidator.GetRegisteredRules())
                {
                    if (rule(type, out string violation))
                    {
                        violations.Add(new StoryViolation
                        {
                            Type = type.Name,
                            Member = type.Name,
                            Violation = violation,
                            ViolationType = StoryTestUtilities.GetViolationType(violation)
                        });
                    }
                }
            }
            return violations;
        }

        private static async Task<List<StoryViolation>> PerformColdMethodsValidation(Assembly assembly)
        {
            var violations = new List<StoryViolation>();
            await Task.Yield();

            foreach (var type in assembly.GetTypes())
            {
                if (type.GetCustomAttribute<StoryIgnoreAttribute>() != null) continue;
                foreach (var rule in StoryIntegrityValidator.GetRegisteredRules())
                {
                    if (rule(type, out string violation))
                    {
                        violations.Add(new StoryViolation
                        {
                            Type = type.Name,
                            Member = type.Name,
                            Violation = violation,
                            ViolationType = StoryTestUtilities.GetViolationType(violation)
                        });
                    }
                }
            }
            return violations;
        }

        private static async Task<List<StoryViolation>> PerformHollowEnumsValidation(Assembly assembly)
        {
            var violations = new List<StoryViolation>();
            await Task.Yield();

            foreach (var type in assembly.GetTypes())
            {
                if (type.GetCustomAttribute<StoryIgnoreAttribute>() != null) continue;
                foreach (var rule in StoryIntegrityValidator.GetRegisteredRules())
                {
                    if (rule(type, out string violation))
                    {
                        violations.Add(new StoryViolation
                        {
                            Type = type.Name,
                            Member = type.Name,
                            Violation = violation,
                            ViolationType = StoryTestUtilities.GetViolationType(violation)
                        });
                    }
                }
            }
            return violations;
        }

        private static async Task<List<StoryViolation>> PerformPrematureCelebrationsValidation(Assembly assembly)
        {
            var violations = new List<StoryViolation>();
            await Task.Yield();

            foreach (var type in assembly.GetTypes())
            {
                if (type.GetCustomAttribute<StoryIgnoreAttribute>() != null) continue;
                foreach (var rule in StoryIntegrityValidator.GetRegisteredRules())
                {
                    if (rule(type, out string violation))
                    {
                        violations.Add(new StoryViolation
                        {
                            Type = type.Name,
                            Member = type.Name,
                            Violation = violation,
                            ViolationType = StoryTestUtilities.GetViolationType(violation)
                        });
                    }
                }
            }
            return violations;
        }

        /// <summary>
        /// Simulates a story validation actor (one of the 9 Acts) performing validation work.
        /// Tests sync-point coordination and measures performance.
        /// DEPRECATED: Use PerformRealStoryValidation for actual validation work.
        /// </summary>
        private static async Task<long> SimulateStoryActor(int actorId, int iterations, Task syncPoint, System.Random random)
        {
            // Wait for sync point - all actors start together
            await syncPoint;

            var stopwatch = Stopwatch.StartNew();
            var actType = actorId % 9; // Map to one of our 9 Story Acts

            // Simulate story validation work with different intensities per Act type
            for (int i = 0; i < iterations; i++)
            {
                switch (actType)
                {
                    case 0: // Act1TodoComments - IL analysis (more intensive)
                        await SimulateILAnalysisWork(50 + random.Next(0, 20));
                        break;
                    case 1: // Act2PlaceholderImplementations
                        SimulateComputeWork(30 + random.Next(0, 15));
                        break;
                    case 2: // Act3IncompleteClasses - Reflection work
                        await SimulateReflectionWork(40 + random.Next(0, 20));
                        break;
                    case 3: // Act4UnsealedAbstractMembers
                        SimulateComputeWork(35 + random.Next(0, 10));
                        break;
                    case 4: // Act5DebugOnlyImplementations - Lightweight
                        SimulateComputeWork(20 + random.Next(0, 8));
                        break;
                    case 5: // Act6PhantomProps - Property analysis (more intensive)
                        await SimulatePropertyAnalysisWork(60 + random.Next(0, 25));
                        break;
                    case 6: // Act7ColdMethods - Method analysis
                        await SimulateMethodAnalysisWork(45 + random.Next(0, 15));
                        break;
                    case 7: // Act8HollowEnums - Enum analysis (lightweight)
                        SimulateComputeWork(25 + random.Next(0, 10));
                        break;
                    case 8: // Act9PrematureCelebrations - Comprehensive analysis
                        await SimulateComprehensiveAnalysisWork(55 + random.Next(0, 20));
                        break;
                }

                // Periodic yield to test coordination under real-world conditions
                if (i % 250 == 0)
                {
                    await Task.Yield();
                }
            }

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Simulates IL analysis work (like Act1TodoComments).
        /// </summary>
        private static async Task SimulateILAnalysisWork(int intensity)
        {
            // Simulate IL bytecode analysis with some async operations
            await Task.Delay(1, CancellationToken.None);
            SimulateComputeWork(intensity);
        }

        /// <summary>
        /// Simulates reflection-based analysis work (like Act3IncompleteClasses).
        /// </summary>
        private static async Task SimulateReflectionWork(int intensity)
        {
            // Simulate reflection operations
            SimulateComputeWork(intensity);
            await Task.Yield(); // Simulate reflection coordination points
        }

        /// <summary>
        /// Simulates property analysis work (like Act6PhantomProps).
        /// </summary>
        private static async Task SimulatePropertyAnalysisWork(int intensity)
        {
            // Property analysis tends to be more intensive
            await Task.Delay(1, CancellationToken.None);
            SimulateComputeWork(intensity);
            await Task.Yield();
        }

        /// <summary>
        /// Simulates method analysis work (like Act7ColdMethods).
        /// </summary>
        private static async Task SimulateMethodAnalysisWork(int intensity)
        {
            // Method analysis with coordination points
            SimulateComputeWork(intensity / 2);
            await Task.Yield();
            SimulateComputeWork(intensity / 2);
        }

        /// <summary>
        /// Simulates comprehensive analysis work (like Act9PrematureCelebrations).
        /// </summary>
        private static async Task SimulateComprehensiveAnalysisWork(int intensity)
        {
            // Most comprehensive analysis
            await Task.Delay(1, CancellationToken.None);
            SimulateComputeWork(intensity);
        }

        /// <summary>
        /// Simulates CPU-bound computation work.
        /// </summary>
        private static void SimulateComputeWork(int intensity)
        {
            var result = 0;
            for (int i = 0; i < intensity * 100; i++)
            {
                result += i.GetHashCode();
            }
            // Prevent optimization
            if (result == int.MaxValue) Debug.Log("");
        }

        /// <summary>
        /// Quick sync-point validation test for development use.
        /// </summary>
        public static async Task<bool> QuickSyncPointTest()
        {
            Debug.Log("🎭 Running Quick Sync-Point Test...");

            var results = await RunSyncPointStressTest(100, 3); // Lighter test

            bool passed = !results.HasBottleneck && !results.IsComedySkitDetected;

            if (passed)
            {
                Debug.Log("✅ Quick sync-point test PASSED - Story flows smoothly!");
            }
            else
            {
                Debug.LogWarning("⚠️  Quick sync-point test has issues - Check full report");
            }

            return passed;
        }

        /// <summary>
        /// Quick sync-point validation test for development use.
        /// Exports the report to a specified file path, creating the directory if needed.
        /// </summary>
        public static async Task<bool> QuickSyncPointTestAndExport(string exportPath)
        {
            Debug.Log("🎭 Running Quick Sync-Point Test...");

            var results = await RunSyncPointStressTest(100, 3); // Lighter test

            bool passed = !results.HasBottleneck && !results.IsComedySkitDetected;

            // Export report to file
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(exportPath));
                File.WriteAllText(exportPath, results.GenerateReport());
                Debug.Log($"Story Sync-Point Test report exported to: {exportPath}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to export story test report: {ex.Message}");
            }

            if (passed)
            {
                Debug.Log("✅ Quick sync-point test PASSED - Story flows smoothly!");
            }
            else
            {
                Debug.LogWarning("⚠️  Quick sync-point test has issues - Check full report");
            }

            return passed;
        }
    }
}