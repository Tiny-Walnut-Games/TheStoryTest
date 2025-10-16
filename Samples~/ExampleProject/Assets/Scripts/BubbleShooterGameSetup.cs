using UnityEngine;
using UnityEngine.UIElements;

namespace BubbleShooter
{
    public class BubbleShooterGameSetup : MonoBehaviour
    {
        [Header("Scene Setup")]
        public Camera gameCamera;
        public GameObject gameManagerPrefab; // purposeful story-test violation
        public GameObject shooterPrefab; // purposeful story-test violation
        public UIDocument uiDocument;

        [Header("Game Settings")]
        public int gridWidth = 11;
        public int gridHeight = 12;
        public float bubbleRadius = 0.5f;

        // Calculated positioning values
        public float gridTopY;
        private float gridBottomY;
        private float shooterY;

        private void Start()
        {
            SetupGame();
        }

        private void SetupGame()
        {
            // Setup camera
            SetupCamera();

            // Setup UI
            SetupUI();

            // Create game manager
            CreateGameManager();

            // Create shooter
            CreateShooter();
        }

        private void SetupCamera()
        {
            if (gameCamera == null)
            {
                gameCamera = Camera.main;
                if (gameCamera == null)
                {
                    var cameraObj = new GameObject("Main Camera");
                    gameCamera = cameraObj.AddComponent<Camera>();
                    cameraObj.tag = "MainCamera";
                }
            }

            // Set orthographic camera for 2D game
            gameCamera.orthographic = true;
            // Calculate proper play area (70% grid, 30% shooter)
            var gridActualHeight = gridHeight * bubbleRadius * 1.732f; // Actual hex grid height
            var shooterArea = gridActualHeight * 0.4f; // 40% for shooter area
            var totalHeight = gridActualHeight + shooterArea;

            // Position grid to start at top of screen
            gridTopY = totalHeight * 0.7f; // Top of grid at 70% from center (near top of screen)
            gridBottomY = gridTopY - gridActualHeight; // Bottom of grid
            shooterY = gridBottomY - shooterArea * 0.75f; // Shooter position (move up with more cushion)

            gameCamera.orthographicSize = totalHeight / 2f;
            gameCamera.backgroundColor = new Color(0.1f, 0.1f, 0.2f, 1f);
            gameCamera.transform.position = new Vector3(0, 0, -10f); // Center camera

            Debug.Log($"Camera setup: gridTop={gridTopY}, gridBottom={gridBottomY}, shooterY={shooterY}, orthoSize={gameCamera.orthographicSize}");
        }

        private void SetupUI()
        {
            if (uiDocument == null)
            {
                // Create UI Document if it doesn't exist
                var uiObj = new GameObject("UI Document");
                uiObj.transform.SetParent(transform);
                uiDocument = uiObj.AddComponent<UIDocument>();
            }

            // Load the UI from UXML
            var uiAsset = Resources.Load<VisualTreeAsset>("UI/BubbleShooterUI");
            if (uiAsset != null)
            {
                uiDocument.visualTreeAsset = uiAsset;
            }
            else
            {
                Debug.LogWarning("BubbleShooterUI.uxml not found in Resources/UI/. Creating basic UI programmatically.");
                CreateBasicUI();
            }
        }

        private void CreateBasicUI()
        {
            var root = uiDocument.rootVisualElement;

            // Create basic UI elements
            var topBar = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    top = 20,
                    left = 20,
                    right = 20,
                    height = 60,
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceBetween,
                    alignItems = Align.Center,
                    backgroundColor = new Color(0, 0, 0, 0.3f),
                    borderTopLeftRadius = 10,
                    borderTopRightRadius = 10
                }
            };

            var scoreLabel = new Label("Score: 0")
            {
                name = "score-label",
                style =
                {
                    fontSize = 24,
                    color = Color.white,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };

            var highScoreLabel = new Label("High Score: 0")
            {
                name = "high-score-label",
                style =
                {
                    fontSize = 24,
                    color = Color.yellow,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };

            topBar.Add(scoreLabel);
            topBar.Add(highScoreLabel);
            root.Add(topBar);

            // Game over panel
            var gameOverPanel = new VisualElement
            {
                name = "game-over-panel",
                style =
                {
                    position = Position.Absolute,
                    top = 0,
                    left = 0,
                    width = Length.Percent(100),
                    height = Length.Percent(100),
                    backgroundColor = new Color(0, 0, 0, 0.8f),
                    justifyContent = Justify.Center,
                    alignItems = Align.Center,
                    display = DisplayStyle.None
                }
            };

            var gameOverContent = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    justifyContent = Justify.Center,
                    alignItems = Align.Center,
                    backgroundColor = new Color(0.16f, 0.16f, 0.16f, 0.9f),
                    borderTopLeftRadius = 20,
                    borderTopRightRadius = 20,
                    borderBottomLeftRadius = 20,
                    borderBottomRightRadius = 20
                }
            };

            var gameOverTitle = new Label("Game Over!")
            {
                style =
                {
                    fontSize = 48,
                    color = Color.red,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 20
                }
            };

            var finalScoreLabel = new Label("Final Score: 0")
            {
                name = "final-score-label",
                style =
                {
                    fontSize = 32,
                    color = Color.white,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 30
                }
            };

            var restartButton = new Button(() => {
                BubbleShooterGameManager.Instance.RestartGame();
            })
            {
                text = "Restart",
                name = "restart-button",
                style =
                {
                    fontSize = 24,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    paddingTop = 15,
                    paddingBottom = 15,
                    paddingLeft = 30,
                    paddingRight = 30,
                    backgroundColor = new Color(0.3f, 0.69f, 0.31f, 1f),
                    color = Color.white,
                    borderTopLeftRadius = 10,
                    borderTopRightRadius = 10,
                    borderBottomLeftRadius = 10,
                    borderBottomRightRadius = 10
                }
            };

            gameOverContent.Add(gameOverTitle);
            gameOverContent.Add(finalScoreLabel);
            gameOverContent.Add(restartButton);
            gameOverPanel.Add(gameOverContent);
            root.Add(gameOverPanel);
        }

        private void CreateGameManager()
        {
            if (BubbleShooterGameManager.Instance != null)
            {
                Debug.Log("BubbleShooterGameManager already exists");
                return;
            }

            Debug.Log("Creating BubbleShooterGameManager");
            var gameManagerObj = new GameObject("Bubble Shooter Game Manager");
            var gameManager = gameManagerObj.AddComponent<BubbleShooterGameManager>();

            // Configure game manager
            gameManager.gridWidth = gridWidth;
            gameManager.gridHeight = gridHeight;
            gameManager.bubbleRadius = bubbleRadius;
            gameManager.uiDocument = uiDocument;

            // Set colors
            gameManager.bubbleColors = new[]
            {
                Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan
            };

            Debug.Log("BubbleShooterGameManager created and configured");
        }

        private void CreateShooter()
        {
            Debug.Log("Creating BubbleShooter");

            // Position shooter at the calculated position
            var shooterObj = new GameObject("Bubble Shooter")
            {
                transform =
                {
                    position = new Vector3(0, shooterY, 0)
                }
            };

            var shooter = shooterObj.AddComponent<BubbleShooter>();

            // Create shoot point
            var shootPointObj = new GameObject("Shoot Point");
            shootPointObj.transform.SetParent(shooterObj.transform);
            shootPointObj.transform.localPosition = Vector3.zero;

            // Create next bubble position (lower right of shooter)
            var nextBubblePosObj = new GameObject("Next Bubble Position");
            nextBubblePosObj.transform.SetParent(shooterObj.transform);
            nextBubblePosObj.transform.localPosition = new Vector3(1.5f, -1f, 0);

            // Create aim line
            var aimLineObj = new GameObject("Aim Line");
            aimLineObj.transform.SetParent(shooterObj.transform);
            var aimLine = aimLineObj.AddComponent<LineRenderer>();

            // Configure aim line
            aimLine.material = new Material(Shader.Find("Sprites/Default"));
            aimLine.startColor = Color.white;
            aimLine.endColor = Color.white;
            aimLine.startWidth = 0.1f;
            aimLine.endWidth = 0.1f;
            aimLine.positionCount = 0;
            aimLine.useWorldSpace = false;

            // Configure shooter
            shooter.shootPoint = shootPointObj.transform;
            shooter.nextBubblePosition = nextBubblePosObj.transform;
            shooter.aimLine = aimLine;

            // Create danger line indicator
            CreateDangerLine();

            // Create boundary colliders
            CreateBoundaryColliders();

            Debug.Log($"BubbleShooter created and positioned at Y: {shooterY}");
        }

        private void CreateBoundaryColliders()
        {
            // Create invisible walls for bubble collision based on actual play area
            var radius = bubbleRadius;

            // Calculate boundaries based on camera view
            var camHeight = gameCamera.orthographicSize * 2f;
            var camWidth = camHeight * gameCamera.aspect;

            var leftBound = -camWidth / 2f + radius;
            var rightBound = camWidth / 2f - radius;
            var topBound = gridTopY + radius;
            var bottomBound = shooterY - radius;

            // Create left wall
            CreateWallCollider("LeftWall", new Vector2(leftBound - 0.5f, (topBound + bottomBound) / 2f), new Vector2(1f, topBound - bottomBound));

            // Create right wall
            CreateWallCollider("RightWall", new Vector2(rightBound + 0.5f, (topBound + bottomBound) / 2f), new Vector2(1f, topBound - bottomBound));

            // Create ceiling
            CreateWallCollider("Ceiling", new Vector2(0, topBound + 0.5f), new Vector2(rightBound - leftBound, 1f));

            Debug.Log($"Boundary colliders created: left={leftBound}, right={rightBound}, top={topBound}, bottom={bottomBound}");
        }

        private void CreateWallCollider(string name, Vector2 position, Vector2 size)
        {
            var wallObj = new GameObject(name);
            wallObj.transform.position = position;

            var collider = wallObj.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = size;

            // Add a tag or component to identify as wall
            wallObj.tag = "Boundary";
        }

        private void CreateDangerLine()
        {
            // Create a visual line to show the danger zone
            var dangerLineObj = new GameObject("DangerLine");
            var dangerLineRenderer = dangerLineObj.AddComponent<LineRenderer>();

            // Calculate danger line position (20% from bottom of screen)
            var dangerY = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height * 0.2f, 0)).y;
            var leftX = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
            var rightX = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;

            dangerLineRenderer.startColor = Color.red;
            dangerLineRenderer.endColor = Color.red;
            dangerLineRenderer.startWidth = 0.1f;
            dangerLineRenderer.endWidth = 0.1f;
            dangerLineRenderer.positionCount = 2;
            dangerLineRenderer.useWorldSpace = true;
            dangerLineRenderer.SetPosition(0, new Vector3(leftX, dangerY, 0));
            dangerLineRenderer.SetPosition(1, new Vector3(rightX, dangerY, 0));

            Debug.Log($"Danger line created at Y: {dangerY}");
        }
    }
}