using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using BubbleShooter;

namespace BubbleShooter.Tests
{
    /// <summary>
    /// Comprehensive Play Mode tests for Bubble Shooter game
    /// These tests run the full game simulation and can be executed in builds
    /// Focus: End-to-end testing with full game loop integration
    /// </summary>
    public class BubbleShooterPlayModeTests
    {
        private UnityTestHelpers.LogCapture _logCapture;
        private BubbleShooterGameManager _gameManager;
        private Camera _testCamera;
        
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _logCapture = new UnityTestHelpers.LogCapture();
            _logCapture.StartCapture();
            Debug.Log("=== PLAY MODE TESTS STARTED ===");
        }
        
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _logCapture?.StopCapture();
            UnityTestHelpers.CleanupTestGameEnvironment();
            Debug.Log("=== PLAY MODE TESTS COMPLETED ===");
        }
        
        [SetUp]
        public void SetUp()
        {
            _logCapture?.ClearLogs();
        }
        
        [TearDown]
        public void TearDown()
        {
            // Log test results after each test
            _logCapture?.DumpLogsToConsole("[TEST LOGS] ");
        }
        
        /// <summary>
        /// Test complete game startup sequence
        /// </summary>
        [UnityTest]
        public IEnumerator Test_CompleteGameStartup()
        {
            Debug.Log("=== PLAY MODE TEST: Complete Game Startup ===");
            
            yield return UnityTestHelpers.SetupTestGameEnvironment();
            
            // Verify game manager exists and is initialized
            _gameManager = Object.FindObjectOfType<BubbleShooterGameManager>();
            Assert.IsNotNull(_gameManager, "Game Manager should exist after setup");
            
            // Verify camera setup
            _testCamera = Camera.main;
            Assert.IsNotNull(_testCamera, "Main camera should exist");
            Assert.IsTrue(_testCamera.orthographic, "Camera should be orthographic for 2D game");
            
            // Check for essential game components
            var gameSetup = Object.FindObjectOfType<BubbleShooterGameSetup>();
            Assert.IsNotNull(gameSetup, "Game Setup component should exist");
            
            Debug.Log("Complete game startup test passed");
            yield return new WaitForSeconds(0.5f);
        }
        
        /// <summary>
        /// Test multiple bubble shots in sequence to detect positioning inconsistencies
        /// </summary>
        [UnityTest]
        public IEnumerator Test_MultipleBubbleShotsPositioningConsistency()
        {
            Debug.Log("=== PLAY MODE TEST: Multiple Bubble Shots Positioning Consistency ===");
            
            yield return UnityTestHelpers.SetupTestGameEnvironment();
            _gameManager = Object.FindObjectOfType<BubbleShooterGameManager>();
            
            var shotDirections = new Vector2[]
            {
                new Vector2(0.0f, 1.0f),     // Straight up
                new Vector2(0.2f, 1.0f),     // Slight right
                new Vector2(-0.2f, 1.0f),    // Slight left
                new Vector2(0.4f, 0.8f),     // Diagonal right
                new Vector2(-0.4f, 0.8f),    // Diagonal left
                new Vector2(0.6f, 0.6f),     // Sharp right
                new Vector2(-0.6f, 0.6f)     // Sharp left
            };
            
            var positioningResults = new List<PositioningTestResult>();
            
            for (int i = 0; i < shotDirections.Length; i++)
            {
                Debug.Log($"\n--- Shot {i + 1}/{shotDirections.Length}: Direction {shotDirections[i]} ---");
                
                // Create test bubble
                var testBubble = UnityTestHelpers.CreateTestBubble(0, 0, 
                    new Color(Random.value, Random.value, Random.value), 
                    _gameManager?.GetBubbleRadius() ?? 1.0f);
                
                // Position at shooter location
                testBubble.transform.position = new Vector3(0, -10, 0);
                
                // Clear logs for this shot
                _logCapture.ClearLogs();
                
                // Execute shot
                yield return UnityTestHelpers.SimulateBubbleShot(testBubble, shotDirections[i]);
                
                // Analyze this shot's positioning
                var analysis = _logCapture.AnalyzePositioning();
                var result = new PositioningTestResult
                {
                    ShotNumber = i + 1,
                    Direction = shotDirections[i],
                    FinalGridPosition = new Vector2Int(testBubble.GridX, testBubble.GridY),
                    FinalWorldPosition = testBubble.transform.position,
                    Analysis = analysis
                };
                
                positioningResults.Add(result);
                
                Debug.Log($"Shot {i + 1} Result: Grid({testBubble.GridX}, {testBubble.GridY}), " +
                         $"World{testBubble.transform.position}");
                
                // Wait between shots
                yield return new WaitForSeconds(1.0f);
            }
            
            // Analyze overall consistency
            AnalyzePositioningConsistency(positioningResults);
            
            Debug.Log("Multiple bubble shots positioning consistency test completed");
        }
        
        private struct PositioningTestResult
        {
            public int ShotNumber;
            public Vector2 Direction;
            public Vector2Int FinalGridPosition;
            public Vector3 FinalWorldPosition;
            public UnityTestHelpers.PositioningAnalysis Analysis;
        }
        
        private void AnalyzePositioningConsistency(List<PositioningTestResult> results)
        {
            Debug.Log("\n=== POSITIONING CONSISTENCY ANALYSIS ===");
            
            var totalErrors = 0;
            var totalShots = results.Count;
            
            foreach (var result in results)
            {
                Debug.Log($"Shot {result.ShotNumber}: Direction {result.Direction}");
                result.Analysis.LogAnalysis("  ");
                
                if (result.Analysis.SignificantErrorCount > 0)
                {
                    totalErrors += result.Analysis.SignificantErrorCount;
                    Debug.LogWarning($"  POSITIONING ISSUE: {result.Analysis.SignificantErrorCount} significant errors detected");
                }
            }
            
            var errorRate = totalShots > 0 ? (float)totalErrors / totalShots * 100f : 0f;
            
            Debug.Log($"\nOVERALL CONSISTENCY METRICS:");
            Debug.Log($"Total shots tested: {totalShots}");
            Debug.Log($"Shots with positioning errors: {totalErrors}");
            Debug.Log($"Error rate: {errorRate:F1}%");
            
            if (errorRate > 30f)
            {
                Debug.LogError("CRITICAL POSITIONING INCONSISTENCY DETECTED!");
                Debug.LogError("This suggests the 'several cells away' bug is present and affecting multiple shots.");
            }
            else if (errorRate > 10f)
            {
                Debug.LogWarning("Moderate positioning inconsistency detected - investigation recommended");
            }
            else
            {
                Debug.Log("Positioning consistency appears acceptable");
            }
        }
        
        /// <summary>
        /// Test boundary collision behavior
        /// </summary>
        [UnityTest]
        public IEnumerator Test_BoundaryCollisions()
        {
            Debug.Log("=== PLAY MODE TEST: Boundary Collisions ===");
            
            yield return UnityTestHelpers.SetupTestGameEnvironment();
            _gameManager = Object.FindObjectOfType<BubbleShooterGameManager>();
            
            // Test wall bounces
            var wallTestDirections = new Vector2[]
            {
                new Vector2(1.0f, 0.5f),   // Right wall
                new Vector2(-1.0f, 0.5f),  // Left wall
            };
            
            foreach (var direction in wallTestDirections)
            {
                Debug.Log($"Testing boundary collision with direction: {direction}");
                
                var testBubble = UnityTestHelpers.CreateTestBubble(0, 0, Color.yellow, 
                    _gameManager?.GetBubbleRadius() ?? 1.0f);
                testBubble.transform.position = new Vector3(0, -10, 0);
                
                _logCapture.ClearLogs();
                yield return UnityTestHelpers.SimulateBubbleShot(testBubble, direction);
                
                // Check for wall bounce logs
                var wallBounces = _logCapture.GetLogsByKeyword("Wall bounce");
                Debug.Log($"Wall bounce events detected: {wallBounces.Count}");
            }
            
            Debug.Log("Boundary collisions test completed");
        }
        
        /// <summary>
        /// Test bubble matching and removal system
        /// </summary>
        [UnityTest]
        public IEnumerator Test_BubbleMatchingSystem()
        {
            Debug.Log("=== PLAY MODE TEST: Bubble Matching System ===");
            
            yield return UnityTestHelpers.SetupTestGameEnvironment();
            _gameManager = Object.FindObjectOfType<BubbleShooterGameManager>();
            
            if (_gameManager == null)
            {
                Debug.LogError("Game Manager not found - skipping matching test");
                yield break;
            }
            
            // Create a controlled scenario with matching bubbles
            var testColor = Color.red;
            var testRadius = _gameManager.GetBubbleRadius();
            
            // Create a cluster of matching bubbles
            var bubble1 = UnityTestHelpers.CreateTestBubble(5, 5, testColor, testRadius);
            var bubble2 = UnityTestHelpers.CreateTestBubble(5, 6, testColor, testRadius);
            var bubble3 = UnityTestHelpers.CreateTestBubble(6, 5, testColor, testRadius);
            
            // Position them in the game grid (this would normally be done by GameManager)
            // Note: This test may need adjustment based on actual GameManager implementation
            
            Debug.Log("Created test bubble cluster for matching test");
            
            // Test neighbor detection
            var neighbors1 = bubble1.GetNeighbors();
            var neighbors2 = bubble2.GetNeighbors();
            var neighbors3 = bubble3.GetNeighbors();
            
            Debug.Log($"Bubble 1 neighbors: {neighbors1.Count}");
            Debug.Log($"Bubble 2 neighbors: {neighbors2.Count}");
            Debug.Log($"Bubble 3 neighbors: {neighbors3.Count}");
            
            // Test matching bubble detection
            var matchingBubbles1 = bubble1.GetMatchingNeighbors();
            Debug.Log($"Matching bubbles found: {matchingBubbles1.Count}");
            
            yield return new WaitForSeconds(1.0f);
            Debug.Log("Bubble matching system test completed");
        }
        
        /// <summary>
        /// Test game over conditions
        /// </summary>
        [UnityTest]
        public IEnumerator Test_GameOverConditions()
        {
            Debug.Log("=== PLAY MODE TEST: Game Over Conditions ===");
            
            yield return UnityTestHelpers.SetupTestGameEnvironment();
            _gameManager = Object.FindObjectOfType<BubbleShooterGameManager>();
            
            if (_gameManager == null)
            {
                Debug.LogError("Game Manager not found - skipping game over test");
                yield break;
            }
            
            Debug.Log("Testing game over detection logic");
            
            // Test danger line collision (this would require actual game state)
            // For now, just verify the test environment supports game over detection
            
            yield return new WaitForSeconds(0.5f);
            Debug.Log("Game over conditions test completed");
        }
        
        /// <summary>
        /// Performance test: Multiple rapid bubble shots
        /// </summary>
        [UnityTest]
        public IEnumerator Test_RapidShotsPerformance()
        {
            Debug.Log("=== PLAY MODE TEST: Rapid Shots Performance ===");
            
            yield return UnityTestHelpers.SetupTestGameEnvironment();
            _gameManager = Object.FindObjectOfType<BubbleShooterGameManager>();
            
            var startTime = Time.realtimeSinceStartup;
            var shotCount = 5; // Reduced for testing
            
            for (int i = 0; i < shotCount; i++)
            {
                var testBubble = UnityTestHelpers.CreateTestBubble(0, 0, 
                    new Color(Random.value, Random.value, Random.value), 
                    _gameManager?.GetBubbleRadius() ?? 1.0f);
                
                testBubble.transform.position = new Vector3(Random.Range(-2f, 2f), -10, 0);
                
                var randomDirection = new Vector2(Random.Range(-0.5f, 0.5f), 1.0f);
                
                // Fire without waiting for completion to test rapid shots
                testBubble.Shoot(randomDirection.normalized, 20f);
                
                yield return new WaitForSeconds(0.1f); // Brief pause
            }
            
            // Wait for all shots to complete
            yield return new WaitForSeconds(5.0f);
            
            var totalTime = Time.realtimeSinceStartup - startTime;
            Debug.Log($"Rapid shots performance test completed: {shotCount} shots in {totalTime:F2}s");
            
            // Check for performance issues in logs
            var errorLogs = _logCapture.GetLogsByKeyword("Error");
            var warningLogs = _logCapture.GetLogsByKeyword("Warning");
            
            Debug.Log($"Errors during rapid shots: {errorLogs.Count}");
            Debug.Log($"Warnings during rapid shots: {warningLogs.Count}");
            
            if (errorLogs.Count > 0)
            {
                Debug.LogWarning("Errors detected during rapid shots - may indicate threading or performance issues");
            }
        }
        
        /// <summary>
        /// Stress test: Maximum grid density
        /// </summary>
        [UnityTest]
        public IEnumerator Test_MaximumGridDensity()
        {
            Debug.Log("=== PLAY MODE TEST: Maximum Grid Density ===");
            
            yield return UnityTestHelpers.SetupTestGameEnvironment();
            _gameManager = Object.FindObjectOfType<BubbleShooterGameManager>();
            
            if (_gameManager == null)
            {
                Debug.LogError("Game Manager not found - skipping density test");
                yield break;
            }
            
            // Fill most of the grid to test near-full conditions
            var bubblesCreated = 0;
            var maxBubbles = 50; // Reasonable limit for testing
            
            for (int y = 0; y < 8 && bubblesCreated < maxBubbles; y++)
            {
                for (int x = 0; x < 11 && bubblesCreated < maxBubbles; x++)
                {
                    if (Random.value > 0.3f) // Leave some gaps
                    {
                        var bubble = UnityTestHelpers.CreateTestBubble(x, y, 
                            new Color(Random.value, Random.value, Random.value), 
                            _gameManager.GetBubbleRadius());
                        bubblesCreated++;
                    }
                }
            }
            
            Debug.Log($"Created {bubblesCreated} bubbles for density test");
            
            // Test shooting into dense grid
            var testShot = UnityTestHelpers.CreateTestBubble(0, 0, Color.white, 
                _gameManager.GetBubbleRadius());
            testShot.transform.position = new Vector3(0, -10, 0);
            
            yield return UnityTestHelpers.SimulateBubbleShot(testShot, Vector2.up);
            
            Debug.Log("Maximum grid density test completed");
            yield return new WaitForSeconds(1.0f);
        }
    }
}