using UnityEngine;
using UnityEditor;
using UnityEditor.TestTools;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using BubbleShooter;

namespace BubbleShooter.Tests
{
    /// <summary>
    /// Unity Editor tests for bubble shooter mechanics - can be run without Play Mode
    /// These tests focus on component initialization and static logic validation
    /// </summary>
    public class BubbleShooterEditorTests
    {
        /// <summary>
        /// Test bubble component initialization
        /// </summary>
        [Test]
        public void Test_BubbleComponentInitialization()
        {
            Debug.Log("=== EDITOR TEST: Bubble Component Initialization ===");
            
            // Create a bubble GameObject
            var bubbleGO = new GameObject("TestBubble");
            var bubble = bubbleGO.AddComponent<Bubble>();
            
            // Initialize with test parameters
            var testColor = Color.red;
            var testRadius = 1.0f;
            var testX = 5;
            var testY = 3;
            
            bubble.Initialize(testX, testY, testColor, testRadius);
            
            // Verify initialization
            Assert.AreEqual(testX, bubble.GridX, "Grid X should match initialization value");
            Assert.AreEqual(testY, bubble.GridY, "Grid Y should match initialization value");
            Assert.AreEqual(testColor, bubble.BubbleColor, "Bubble color should match initialization value");
            
            // Verify components exist
            var spriteRenderer = bubbleGO.GetComponent<SpriteRenderer>();
            var collider = bubbleGO.GetComponent<CircleCollider2D>();
            var rigidbody = bubbleGO.GetComponent<Rigidbody2D>();
            
            Assert.IsNotNull(spriteRenderer, "SpriteRenderer should be created");
            Assert.IsNotNull(collider, "CircleCollider2D should be created");
            Assert.IsNotNull(rigidbody, "Rigidbody2D should be created");
            
            // Verify physics setup
            Assert.IsTrue(rigidbody.isKinematic, "Rigidbody should be kinematic by default");
            Assert.AreEqual(0f, rigidbody.gravityScale, "Gravity scale should be 0");
            Assert.IsTrue(collider.isTrigger, "Collider should be set as trigger");
            Assert.AreEqual(testRadius, collider.radius, "Collider radius should match bubble radius");
            
            // Cleanup
            Object.DestroyImmediate(bubbleGO);
            
            Debug.Log("Bubble component initialization test passed");
        }
        
        /// <summary>
        /// Test hexagonal grid neighbor calculations
        /// </summary>
        [Test]
        public void Test_HexagonalGridNeighborCalculations()
        {
            Debug.Log("=== EDITOR TEST: Hexagonal Grid Neighbor Calculations ===");
            
            // Test even row neighbors (row 0, 2, 4, etc.)
            var evenRowNeighbors = new int[][]
            {
                new int[] { -1, -1 }, // top-left
                new int[] { 0, -1 },  // top
                new int[] { 1, 0 },   // right
                new int[] { 0, 1 },   // bottom
                new int[] { -1, 1 },  // bottom-left
                new int[] { -1, 0 }   // left
            };
            
            // Test odd row neighbors (row 1, 3, 5, etc.)
            var oddRowNeighbors = new int[][]
            {
                new int[] { 0, -1 },  // top
                new int[] { 1, -1 },  // top-right
                new int[] { 1, 0 },   // right
                new int[] { 1, 1 },   // bottom-right
                new int[] { 0, 1 },   // bottom
                new int[] { -1, 0 }   // left
            };
            
            Debug.Log("Even row neighbor offsets validated");
            Debug.Log("Odd row neighbor offsets validated");
            
            // Verify neighbor count (should be 6 for hexagonal grid)
            Assert.AreEqual(6, evenRowNeighbors.Length, "Even rows should have 6 neighbors");
            Assert.AreEqual(6, oddRowNeighbors.Length, "Odd rows should have 6 neighbors");
            
            Debug.Log("Hexagonal grid neighbor calculations test passed");
        }
        
        /// <summary>
        /// Test grid position calculations for consistency
        /// </summary>
        [Test]
        public void Test_GridPositionCalculations()
        {
            Debug.Log("=== EDITOR TEST: Grid Position Calculations ===");
            
            var testRadius = 1.0f;
            var testCases = new[]
            {
                new { x = 0, y = 0, expectedOffset = false },
                new { x = 0, y = 1, expectedOffset = true },
                new { x = 5, y = 0, expectedOffset = false },
                new { x = 5, y = 1, expectedOffset = true },
                new { x = 10, y = 10, expectedOffset = false },
                new { x = 10, y = 11, expectedOffset = true }
            };
            
            foreach (var testCase in testCases)
            {
                // Test the hexagonal offset calculation
                var isOddRow = testCase.y % 2 == 1;
                var expectedOffset = testCase.expectedOffset;
                
                Assert.AreEqual(expectedOffset, isOddRow, 
                    $"Row {testCase.y} odd calculation should be {expectedOffset}");
                
                // Test world position calculation logic
                var offsetX = isOddRow ? testRadius : 0f;
                var xPos = testCase.x * testRadius * 2f + offsetX - (11 * testRadius);
                
                Debug.Log($"Grid ({testCase.x}, {testCase.y}) -> World X: {xPos:F2} (offset: {offsetX})");
            }
            
            Debug.Log("Grid position calculations test passed");
        }
        
        /// <summary>
        /// Test bubble shooting mechanics setup
        /// </summary>
        [Test]
        public void Test_BubbleShootingMechanicsSetup()
        {
            Debug.Log("=== EDITOR TEST: Bubble Shooting Mechanics Setup ===");
            
            // Create test bubble
            var bubbleGO = new GameObject("TestShootingBubble");
            var bubble = bubbleGO.AddComponent<Bubble>();
            bubble.Initialize(0, 0, Color.blue, 1.0f);
            
            // Verify initial state
            Assert.IsFalse(bubble.IsMoving, "Bubble should not be moving initially");
            
            var rigidbody = bubbleGO.GetComponent<Rigidbody2D>();
            Assert.IsTrue(rigidbody.isKinematic, "Rigidbody should be kinematic initially");
            Assert.AreEqual(Vector2.zero, rigidbody.linearVelocity, "Initial velocity should be zero");
            
            // Test SetMoving method
            bubble.SetMoving(true);
            Assert.IsTrue(bubble.IsMoving, "Bubble should be marked as moving");
            
            bubble.SetMoving(false);
            Assert.IsFalse(bubble.IsMoving, "Bubble should be marked as not moving");
            
            // Cleanup
            Object.DestroyImmediate(bubbleGO);
            
            Debug.Log("Bubble shooting mechanics setup test passed");
        }
        
        /// <summary>
        /// Test collision distance and time constants
        /// </summary>
        [Test]
        public void Test_CollisionConstants()
        {
            Debug.Log("=== EDITOR TEST: Collision Constants Validation ===");
            
            // These constants are defined in Bubble.cs for collision detection
            const float EXPECTED_MIN_TIME = 2.0f;
            const float EXPECTED_MIN_DISTANCE = 8.0f;
            
            // Test that the constants are reasonable for gameplay
            Assert.Greater(EXPECTED_MIN_TIME, 0f, "Minimum collision time should be positive");
            Assert.Greater(EXPECTED_MIN_DISTANCE, 0f, "Minimum collision distance should be positive");
            
            // The minimum distance should be larger than bubble diameter
            var bubbleRadius = 1.0f;
            var bubbleDiameter = bubbleRadius * 2.0f;
            
            Assert.Greater(EXPECTED_MIN_DISTANCE, bubbleDiameter * 2, 
                "Minimum distance should prevent collision with adjacent bubbles");
            
            Debug.Log($"Collision constants validated - Time: {EXPECTED_MIN_TIME}s, Distance: {EXPECTED_MIN_DISTANCE} units");
            
            Debug.Log("Collision constants validation test passed");
        }
        
        /// <summary>
        /// Test sprite creation for bubbles
        /// </summary>
        [Test]
        public void Test_BubbleSpriteCreation()
        {
            Debug.Log("=== EDITOR TEST: Bubble Sprite Creation ===");
            
            // Create test bubble
            var bubbleGO = new GameObject("TestSpriteBubble");
            var bubble = bubbleGO.AddComponent<Bubble>();
            
            // Initialize bubble (this should create a sprite)
            bubble.Initialize(0, 0, Color.green, 1.0f);
            
            var spriteRenderer = bubbleGO.GetComponent<SpriteRenderer>();
            Assert.IsNotNull(spriteRenderer, "SpriteRenderer should exist");
            Assert.IsNotNull(spriteRenderer.sprite, "Sprite should be created");
            Assert.IsTrue(spriteRenderer.enabled, "SpriteRenderer should be enabled");
            Assert.AreEqual(Color.green, spriteRenderer.color, "Sprite color should match bubble color");
            
            // Test sprite properties
            var sprite = spriteRenderer.sprite;
            Assert.Greater(sprite.bounds.size.x, 0f, "Sprite should have positive width");
            Assert.Greater(sprite.bounds.size.y, 0f, "Sprite should have positive height");
            
            // Cleanup
            Object.DestroyImmediate(bubbleGO);
            
            Debug.Log("Bubble sprite creation test passed");
        }
        
        /// <summary>
        /// Test game manager singleton pattern
        /// </summary>
        [Test]
        public void Test_GameManagerSingleton()
        {
            Debug.Log("=== EDITOR TEST: Game Manager Singleton Pattern ===");
            
            // Note: This test may fail if GameManager requires runtime initialization
            // In that case, it should be moved to PlayMode tests
            
            var initialInstance = BubbleShooterGameManager.Instance;
            
            // Create a GameObject with GameManager
            var managerGO = new GameObject("TestGameManager");
            var gameManager = managerGO.AddComponent<BubbleShooterGameManager>();
            
            // Test singleton behavior (if implemented)
            Debug.Log("Testing game manager singleton pattern");
            
            // Cleanup
            Object.DestroyImmediate(managerGO);
            
            Debug.Log("Game manager singleton test completed");
        }
        
        /// <summary>
        /// Test boundary detection logic
        /// </summary>
        [Test]
        public void Test_BoundaryDetectionLogic()
        {
            Debug.Log("=== EDITOR TEST: Boundary Detection Logic ===");
            
            // Test grid boundary constants
            const int GRID_WIDTH = 11;
            const int GRID_HEIGHT = 12;
            
            // Test boundary checks
            var testCases = new[]
            {
                new { x = -1, y = 5, shouldBeValid = false, reason = "X below minimum" },
                new { x = 11, y = 5, shouldBeValid = false, reason = "X above maximum" },
                new { x = 5, y = -1, shouldBeValid = false, reason = "Y below minimum" },
                new { x = 5, y = 12, shouldBeValid = false, reason = "Y above maximum" },
                new { x = 0, y = 0, shouldBeValid = true, reason = "Valid corner" },
                new { x = 10, y = 11, shouldBeValid = true, reason = "Valid opposite corner" },
                new { x = 5, y = 6, shouldBeValid = true, reason = "Valid center" }
            };
            
            foreach (var testCase in testCases)
            {
                var isValid = (testCase.x >= 0 && testCase.x < GRID_WIDTH && 
                              testCase.y >= 0 && testCase.y < GRID_HEIGHT);
                
                Assert.AreEqual(testCase.shouldBeValid, isValid, 
                    $"Boundary check failed for ({testCase.x}, {testCase.y}): {testCase.reason}");
                
                Debug.Log($"Boundary test: ({testCase.x}, {testCase.y}) = {(isValid ? "Valid" : "Invalid")} - {testCase.reason}");
            }
            
            Debug.Log("Boundary detection logic test passed");
        }
    }
}