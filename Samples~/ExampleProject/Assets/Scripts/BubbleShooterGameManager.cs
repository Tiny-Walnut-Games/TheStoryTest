using UnityEngine;
using UnityEngine.UIElements;

namespace BubbleShooter
{
    public class BubbleShooterGameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        public int gridWidth = 11;
        public int gridHeight = 12;
        public float bubbleRadius = 0.5f;
        public float shootSpeed = 10f;
        public float descendInterval = 10f; // Time between grid descents
        public float descendAmount = 0.5f; // How much the grid descends each time

        [Header("Colors")]
        public Color[] bubbleColors = new Color[]
        {
            Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan
        };

        [Header("UI")]
        public UIDocument uiDocument;

        // Game state
        private int score = 0;
        private int highScore = 0;
        private bool isGameOver = false;
        private Bubble[,] grid;
        private Bubble currentBubble;
        private Vector2 shootDirection;
        private float descendTimer = 0f;
        private float gridOffsetY = 0f; // Current vertical offset of the grid
        private bool isBubbleInFlight = false; // Pause descent while a bubble is moving

        // UI Elements
        private Label scoreLabel;
        private Label highScoreLabel;
        private Button restartButton;
        private VisualElement gameOverPanel;

        public static BubbleShooterGameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializeGame();
            LoadHighScore();
            SetupUI();
        }

        public void InitializeGame()
        {
            grid = new Bubble[gridWidth, gridHeight];
            score = 0;
            isGameOver = false;

            // Initialize grid with some bubbles
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            // Fill bottom half of grid with random bubbles
            for (var y = 0; y < gridHeight / 2; y++)
            {
                for (var x = 0; x < gridWidth; x++)
                {
                    if (ShouldPlaceBubble(x, y))
                    {
                        CreateBubble(x, y);
                    }
                }
            }
        }

        private bool ShouldPlaceBubble(int x, int y)
        {
            // Create a hexagonal pattern
            if (y % 2 == 0)
            {
                return x < gridWidth;
            }
            else
            {
                return x < gridWidth - 1;
            }
        }

        public Bubble CreateBubble(int x, int y, Color color, float radius)
        {
            var position = GetBubblePosition(x, y);
            var bubbleObj = new GameObject($"Bubble_{x}_{y}")
            {
                transform =
                {
                    position = position
                }
            };

            var bubble = bubbleObj.AddComponent<Bubble>();
            bubble.Initialize(x, y, color, radius);

            if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            {
                grid[x, y] = bubble;
            }

            return bubble;
        }

        private void CreateBubble(int x, int y)
        {
            CreateBubble(x, y, bubbleColors[Random.Range(0, bubbleColors.Length)], bubbleRadius);
        }

        private Vector3 GetBubblePosition(int x, int y)
        {
            var offsetX = (y % 2 == 1) ? bubbleRadius : 0f;
            var xPos = x * bubbleRadius * 2f + offsetX - (gridWidth * bubbleRadius);
            
            // Get the game setup to position grid correctly
            var gameSetup = FindObjectOfType<BubbleShooterGameSetup>();
            var yPos = gameSetup != null 
                ? gameSetup.gridTopY - (y * bubbleRadius * 1.732f)
                : y * bubbleRadius * 1.732f;
            
            // Apply grid descent offset
            yPos += gridOffsetY;

            return new Vector3(xPos, yPos, 0);
        }

        private void SetupUI()
        {
            if (uiDocument == null) return;

            var root = uiDocument.rootVisualElement;

            scoreLabel = root.Q<Label>("score-label");
            highScoreLabel = root.Q<Label>("high-score-label");
            restartButton = root.Q<Button>("restart-button");
            gameOverPanel = root.Q<VisualElement>("game-over-panel");

            if (restartButton != null)
            {
                restartButton.clicked += RestartGame;
            }

            UpdateUI();
        }

        private void Update()
        {
            if (isGameOver) return;

            // Handle grid descent - pause while a bubble is in flight
            if (!isBubbleInFlight)
            {
                descendTimer += Time.deltaTime;
                if (descendTimer >= descendInterval)
                {
                    DescendGrid();
                    descendTimer = 0f;
                }
            }

            // Check for game over condition
            CheckGameOver();
        }

        private void DescendGrid()
        {
            Debug.Log("Descending grid");
            gridOffsetY -= descendAmount;

            // Update all bubbles to their new descended positions using the offset
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (grid[x, y] != null)
                    {
                        var bubble = grid[x, y];
                        // Recalculate position based on new gridOffsetY
                        bubble.transform.position = GetBubblePosition(x, y);
                        Debug.Log($"  Bubble at ({x}, {y}) moved to {bubble.transform.position}");
                    }
                }
            }
        }

        private void CheckGameOver()
        {
            // Check if any bubble has reached the danger zone (bottom 20% of screen)
            var dangerLine = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height * 0.2f, 0)).y;

            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (grid[x, y] != null)
                    {
                        if (grid[x, y].transform.position.y <= dangerLine)
                        {
                            Debug.Log("Game Over! Bubble reached danger line");
                            GameOver();
                            return;
                        }
                    }
                }
            }
        }

        public void AddScore(int points)
        {
            score += points;
            if (score > highScore)
            {
                highScore = score;
                SaveHighScore();
            }
            UpdateUI();
        }

        public void GameOver()
        {
            isGameOver = true;
            UpdateUI();
        }

        public void RestartGame()
        {
            // Clear existing bubbles
            for (var y = 0; y < gridHeight; y++)
            {
                for (var x = 0; x < gridWidth; x++)
                {
                    if (grid[x, y] != null)
                    {
                        Destroy(grid[x, y].gameObject);
                        grid[x, y] = null;
                    }
                }
            }

            // Reset game state
            score = 0;
            isGameOver = false;
            descendTimer = 0f;
            gridOffsetY = 0f;

            InitializeGame();
            UpdateUI();
        }

        private void LoadHighScore()
        {
            highScore = PlayerPrefs.GetInt("BubbleShooterHighScore", 0);
        }

        private void SaveHighScore()
        {
            PlayerPrefs.SetInt("BubbleShooterHighScore", highScore);
            PlayerPrefs.Save();
        }

        public Bubble GetGridBubble(int x, int y)
        {
            if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            {
                return grid[x, y];
            }
            return null;
        }

        public void SetGridBubble(int x, int y, Bubble bubble)
        {
            if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            {
                grid[x, y] = bubble;
            }
        }

        public bool IsGameOver()
        {
            return isGameOver;
        }

        public Color[] GetBubbleColors()
        {
            return bubbleColors;
        }

        public float GetBubbleRadius()
        {
            return bubbleRadius;
        }

        public float GetShootSpeed()
        {
            return shootSpeed;
        }

        public float GetGridOffsetY()
        {
            return gridOffsetY;
        }

        public void SetBubbleInFlight(bool inFlight)
        {
            isBubbleInFlight = inFlight;
            Debug.Log($"Bubble in flight: {inFlight} - Grid descent {(inFlight ? "PAUSED" : "RESUMED")}");
        }

        /// <summary>
        /// Validates that all bubbles in the grid are at their expected positions.
        /// Used for debugging sync issues.
        /// </summary>
        public void ValidateGridSync()
        {
            int syncErrors = 0;
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    var bubble = grid[x, y];
                    if (bubble != null)
                    {
                        var expectedPos = GetBubblePosition(x, y);
                        var distance = Vector3.Distance(bubble.transform.position, expectedPos);
                        
                        if (distance > 0.1f) // Allow small floating point error
                        {
                            Debug.LogError($"[SYNC ERROR] Bubble at ({x}, {y}): expected {expectedPos}, got {bubble.transform.position}, distance: {distance}");
                            syncErrors++;
                        }
                    }
                }
            }
            
            if (syncErrors == 0)
            {
                Debug.Log($"[SYNC OK] Grid validation passed - all {CountGridBubbles()} bubbles are synchronized");
            }
            else
            {
                Debug.LogError($"[SYNC FAILED] Found {syncErrors} sync errors in grid!");
            }
        }

        private int CountGridBubbles()
        {
            int count = 0;
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (grid[x, y] != null)
                        count++;
                }
            }
            return count;
        }

        private void UpdateUI()
        {
            if (scoreLabel != null)
                scoreLabel.text = $"Score: {score}";

            if (highScoreLabel != null)
                highScoreLabel.text = $"High Score: {highScore}";

            if (gameOverPanel != null)
                gameOverPanel.style.display = isGameOver ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}