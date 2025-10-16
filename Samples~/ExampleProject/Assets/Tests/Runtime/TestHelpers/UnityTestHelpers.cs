using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using BubbleShooter;

namespace BubbleShooter.Tests
{
    /// <summary>
    /// Helper utilities for Unity NUnit tests
    /// Provides common functionality for test setup, logging, and analysis
    /// </summary>
    public static class UnityTestHelpers
    {
        /// <summary>
        /// Log capture system for analyzing Unity console output during tests
        /// </summary>
        public class LogCapture
        {
            public struct LogEntry
            {
                public LogType Type;
                public string Message;
                public string StackTrace;
                public float Timestamp;
                public string Category;
            }
            
            private readonly List<LogEntry> _capturedLogs = new List<LogEntry>();
            private bool _isCapturing = false;
            
            public void StartCapture()
            {
                if (!_isCapturing)
                {
                    Application.logMessageReceived += OnLogMessageReceived;
                    _isCapturing = true;
                    Debug.Log("[TEST LOG CAPTURE] Started capturing Unity logs");
                }
            }
            
            public void StopCapture()
            {
                if (_isCapturing)
                {
                    Application.logMessageReceived -= OnLogMessageReceived;
                    _isCapturing = false;
                    Debug.Log($"[TEST LOG CAPTURE] Stopped capturing Unity logs. Captured {_capturedLogs.Count} messages");
                }
            }
            
            public void ClearLogs()
            {
                _capturedLogs.Clear();
                Debug.Log("[TEST LOG CAPTURE] Cleared captured logs");
            }
            
            private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
            {
                var category = DetermineLogCategory(logString);
                _capturedLogs.Add(new LogEntry
                {
                    Type = type,
                    Message = logString,
                    StackTrace = stackTrace,
                    Timestamp = Time.realtimeSinceStartup,
                    Category = category
                });
            }
            
            private string DetermineLogCategory(string logMessage)
            {
                if (logMessage.Contains("COLLISION")) return "Collision";
                if (logMessage.Contains("SNAP GEOMETRY")) return "Positioning";
                if (logMessage.Contains("BARREL FIRED")) return "Shooting";
                if (logMessage.Contains("[SYNC]")) return "Synchronization";
                if (logMessage.Contains("TEST")) return "Testing";
                return "General";
            }
            
            public List<LogEntry> GetLogsByCategory(string category)
            {
                var filteredLogs = new List<LogEntry>();
                foreach (var log in _capturedLogs)
                {
                    if (log.Category.Equals(category, System.StringComparison.OrdinalIgnoreCase))
                    {
                        filteredLogs.Add(log);
                    }
                }
                return filteredLogs;
            }
            
            public List<LogEntry> GetLogsByKeyword(string keyword)
            {
                var filteredLogs = new List<LogEntry>();
                foreach (var log in _capturedLogs)
                {
                    if (log.Message.Contains(keyword))
                    {
                        filteredLogs.Add(log);
                    }
                }
                return filteredLogs;
            }
            
            public List<LogEntry> GetAllLogs()
            {
                return new List<LogEntry>(_capturedLogs);
            }
            
            public void DumpLogsToConsole(string prefix = "")
            {
                Debug.Log($"{prefix}=== CAPTURED LOGS DUMP ({_capturedLogs.Count} messages) ===");
                foreach (var log in _capturedLogs)
                {
                    Debug.Log($"{prefix}[{log.Timestamp:F2}s] [{log.Category}] {log.Message}");
                }
            }
            
            public PositioningAnalysis AnalyzePositioning()
            {
                var analysis = new PositioningAnalysis();
                var collisionLogs = GetLogsByKeyword("COLLISION at position");
                var snapLogs = GetLogsByKeyword("SNAP GEOMETRY");
                
                foreach (var collisionLog in collisionLogs)
                {
                    var collision = ParseCollisionData(collisionLog);
                    if (collision.HasValue)
                    {
                        analysis.Collisions.Add(collision.Value);
                    }
                }
                
                foreach (var snapLog in snapLogs)
                {
                    if (snapLog.Message.Contains("Final choice"))
                    {
                        var attachment = ParseAttachmentData(snapLog);
                        if (attachment.HasValue)
                        {
                            analysis.Attachments.Add(attachment.Value);
                        }
                    }
                }
                
                analysis.CalculateErrors();
                return analysis;
            }
            
            private CollisionData? ParseCollisionData(LogEntry log)
            {
                try
                {
                    var message = log.Message;
                    var positionStart = message.IndexOf("position ") + 9;
                    var positionEnd = message.IndexOf(" with:");
                    if (positionEnd == -1) positionEnd = message.Length;
                    
                    var positionStr = message.Substring(positionStart, positionEnd - positionStart);
                    positionStr = positionStr.Replace("(", "").Replace(")", "").Trim();
                    var parts = positionStr.Split(',');
                    
                    if (parts.Length >= 2 &&
                        float.TryParse(parts[0].Trim(), out float x) &&
                        float.TryParse(parts[1].Trim(), out float y))
                    {
                        return new CollisionData
                        {
                            WorldPosition = new Vector2(x, y),
                            Timestamp = log.Timestamp
                        };
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Failed to parse collision data: {e.Message}");
                }
                
                return null;
            }
            
            private AttachmentData? ParseAttachmentData(LogEntry log)
            {
                try
                {
                    var message = log.Message;
                    var choiceStart = message.LastIndexOf("(");
                    var choiceEnd = message.LastIndexOf(")");
                    
                    if (choiceStart != -1 && choiceEnd != -1)
                    {
                        var coordStr = message.Substring(choiceStart + 1, choiceEnd - choiceStart - 1);
                        var parts = coordStr.Split(',');
                        
                        if (parts.Length >= 2 &&
                            int.TryParse(parts[0].Trim(), out int x) &&
                            int.TryParse(parts[1].Trim(), out int y))
                        {
                            return new AttachmentData
                            {
                                GridPosition = new Vector2Int(x, y),
                                Timestamp = log.Timestamp
                            };
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Failed to parse attachment data: {e.Message}");
                }
                
                return null;
            }
        }
        
        public struct CollisionData
        {
            public Vector2 WorldPosition;
            public float Timestamp;
        }
        
        public struct AttachmentData
        {
            public Vector2Int GridPosition;
            public float Timestamp;
        }
        
        public class PositioningAnalysis
        {
            public List<CollisionData> Collisions = new List<CollisionData>();
            public List<AttachmentData> Attachments = new List<AttachmentData>();
            public List<float> PositioningErrors = new List<float>();
            
            public float AverageError;
            public float MaxError;
            public float MinError;
            public int SignificantErrorCount;
            
            public void CalculateErrors()
            {
                PositioningErrors.Clear();
                
                // Match collisions with attachments by timestamp proximity
                foreach (var collision in Collisions)
                {
                    var closestAttachment = Attachments
                        .OrderBy(a => Mathf.Abs(a.Timestamp - collision.Timestamp))
                        .FirstOrDefault();
                    
                    if (closestAttachment.GridPosition != Vector2Int.zero) // Valid attachment found
                    {
                        // Convert grid position to world position for comparison
                        var worldAttachment = GridToWorldPosition(closestAttachment.GridPosition);
                        var error = Vector2.Distance(collision.WorldPosition, worldAttachment);
                        PositioningErrors.Add(error);
                    }
                }
                
                if (PositioningErrors.Count > 0)
                {
                    AverageError = PositioningErrors.Average();
                    MaxError = PositioningErrors.Max();
                    MinError = PositioningErrors.Min();
                    SignificantErrorCount = PositioningErrors.Count(e => e > 2.0f);
                }
            }
            
            private Vector2 GridToWorldPosition(Vector2Int gridPos)
            {
                // Simplified world position calculation for error analysis
                var bubbleRadius = 1.0f; // Default radius
                var offsetX = (gridPos.y % 2 == 1) ? bubbleRadius : 0f;
                var xPos = gridPos.x * bubbleRadius * 2f + offsetX - (11 * bubbleRadius);
                var yPos = -gridPos.y * bubbleRadius * 1.732f; // Hexagonal spacing
                
                return new Vector2(xPos, yPos);
            }
            
            public void LogAnalysis(string prefix = "")
            {
                Debug.Log($"{prefix}=== POSITIONING ANALYSIS RESULTS ===");
                Debug.Log($"{prefix}Collisions detected: {Collisions.Count}");
                Debug.Log($"{prefix}Attachments detected: {Attachments.Count}");
                Debug.Log($"{prefix}Positioning errors calculated: {PositioningErrors.Count}");
                
                if (PositioningErrors.Count > 0)
                {
                    Debug.Log($"{prefix}Average error: {AverageError:F2} units");
                    Debug.Log($"{prefix}Maximum error: {MaxError:F2} units");
                    Debug.Log($"{prefix}Minimum error: {MinError:F2} units");
                    Debug.Log($"{prefix}Significant errors (>2 units): {SignificantErrorCount}/{PositioningErrors.Count}");
                    
                    var errorPercentage = (float)SignificantErrorCount / PositioningErrors.Count * 100f;
                    Debug.Log($"{prefix}Error percentage: {errorPercentage:F1}%");
                    
                    if (errorPercentage > 50f)
                    {
                        Debug.LogError($"{prefix}CRITICAL: High positioning error rate detected!");
                    }
                }
                else
                {
                    Debug.LogWarning($"{prefix}No positioning errors could be calculated (insufficient data)");
                }
            }
        }
        
        /// <summary>
        /// Create a test game environment with all necessary components
        /// </summary>
        public static IEnumerator SetupTestGameEnvironment()
        {
            Debug.Log("[TEST HELPER] Setting up test game environment...");
            
            // Clean up existing objects
            var existingManagers = Object.FindObjectsOfType<BubbleShooterGameManager>();
            foreach (var manager in existingManagers)
            {
                Object.DestroyImmediate(manager.gameObject);
            }
            
            // Create camera
            var cameraGo = new GameObject("Test Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 15f;
            
            // Create game setup
            var setupGo = new GameObject("Test BubbleShooterGameSetup");
            var gameSetup = setupGo.AddComponent<BubbleShooterGameSetup>();
            
            // Create game manager
            var managerGo = new GameObject("Test BubbleShooterGameManager");
            var gameManager = managerGo.AddComponent<BubbleShooterGameManager>();
            
            yield return new WaitForFixedUpdate();
            
            // Initialize game - Let Start() handle initialization automatically
            yield return new WaitForSeconds(0.5f);
            
            try
            {
                Debug.Log("[TEST HELPER] Game environment setup complete");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TEST HELPER] Failed to initialize game: {e.Message}");
            }
        }
        
        /// <summary>
        /// Clean up test game environment
        /// </summary>
        public static void CleanupTestGameEnvironment()
        {
            Debug.Log("[TEST HELPER] Cleaning up test game environment...");
            
            var testObjects = new[]
            {
                "Test Main Camera",
                "Test BubbleShooterGameSetup", 
                "Test BubbleShooterGameManager"
            };
            
            foreach (var objectName in testObjects)
            {
                var obj = GameObject.Find(objectName);
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }
            
            // Also clean up any remaining bubbles
            var bubbles = Object.FindObjectsOfType<Bubble>();
            foreach (var bubble in bubbles)
            {
                Object.DestroyImmediate(bubble.gameObject);
            }
            
            Debug.Log("[TEST HELPER] Test environment cleanup complete");
        }
        
        /// <summary>
        /// Wait for a bubble to finish moving
        /// </summary>
        public static IEnumerator WaitForBubbleToSettle(Bubble bubble, float maxWaitTime = 10f)
        {
            var waitTime = 0f;
            
            while (waitTime < maxWaitTime && bubble != null && bubble.IsMoving)
            {
                yield return new WaitForFixedUpdate();
                waitTime += Time.fixedDeltaTime;
            }
            
            if (waitTime >= maxWaitTime)
            {
                Debug.LogWarning($"[TEST HELPER] Bubble did not settle within {maxWaitTime}s");
            }
            else
            {
                Debug.Log($"[TEST HELPER] Bubble settled after {waitTime:F2}s");
            }
        }
        
        /// <summary>
        /// Create a test bubble with specified parameters
        /// </summary>
        public static Bubble CreateTestBubble(int gridX, int gridY, Color color, float radius = 1.0f)
        {
            var bubbleGo = new GameObject($"TestBubble_({gridX},{gridY})");
            var bubble = bubbleGo.AddComponent<Bubble>();
            bubble.Initialize(gridX, gridY, color, radius);
            
            Debug.Log($"[TEST HELPER] Created test bubble at grid ({gridX}, {gridY}) with color {color}");
            return bubble;
        }
        
        /// <summary>
        /// Simulate a bubble shot with logging
        /// </summary>
        public static IEnumerator SimulateBubbleShot(Bubble bubble, Vector2 direction, float speed = 15f)
        {
            var shotId = System.Guid.NewGuid().ToString().Substring(0, 8);
            Debug.Log($"[TEST HELPER] Simulating bubble shot {shotId} with direction {direction} and speed {speed}");
            
            bubble.Shoot(direction.normalized, speed);
            yield return WaitForBubbleToSettle(bubble);
            
            Debug.Log($"[TEST HELPER] Shot {shotId} simulation complete");
        }
    }
}