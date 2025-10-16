using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BubbleShooter
{
    public class Bubble : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private CircleCollider2D circleCollider;
        [SerializeField] private Rigidbody2D rigidbody2D;

        private int gridX;
        private int gridY;
        private Color bubbleColor;
        private float radius;
        private bool isMoving = false;

        // Collider management for shot bubbles
        private Vector3 shotStartPosition;
        private float shotTime;
        private const float COLLIDER_ENABLE_MIN_TIME = 2.0f; // Minimum time before enabling collider
        private const float COLLIDER_ENABLE_MIN_DISTANCE = 8.0f; // Minimum distance traveled before enabling collider

        public int GridX => gridX;
        public int GridY => gridY;
        public Color BubbleColor => bubbleColor;
        public bool IsMoving => isMoving;
        public GameObject GameObject => gameObject;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            circleCollider = GetComponent<CircleCollider2D>();
            rigidbody2D = GetComponent<Rigidbody2D>();

            // Create components if they don't exist
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            if (circleCollider == null)
                circleCollider = gameObject.AddComponent<CircleCollider2D>();

            if (rigidbody2D == null)
                rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
        }

        public void Initialize(int x, int y, Color color, float bubbleRadius)
        {
            gridX = x;
            gridY = y;
            bubbleColor = color;
            radius = bubbleRadius;

            // Debug.Log($"Initializing bubble at ({x}, {y}) with color {color}");

            // Setup visual - use a simple colored circle
            if (spriteRenderer != null)
            {
                spriteRenderer.color = bubbleColor;

                // Create a simple circle sprite if none exists
                if (spriteRenderer.sprite == null)
                {
                    // Debug.Log("Creating circle sprite for bubble");
                    spriteRenderer.sprite = CreateSimpleCircleSprite();
                }
                else
                {
                    // Debug.Log("Bubble already has a sprite");
                }

                // Ensure the sprite is visible
                spriteRenderer.enabled = true;
                spriteRenderer.drawMode = SpriteDrawMode.Simple;
            }
            else
            {
                Debug.LogError("SpriteRenderer is null!");
            }

            // Setup physics
            if (circleCollider != null)
            {
                circleCollider.radius = bubbleRadius;
                circleCollider.isTrigger = true;
            }
            else
            {
                Debug.LogError("CircleCollider2D is null!");
            }

            if (rigidbody2D != null)
            {
                rigidbody2D.isKinematic = true;
                rigidbody2D.gravityScale = 0f;
                rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
            else
            {
                Debug.LogError("Rigidbody2D is null!");
            }

            isMoving = false;
            // Debug.Log($"Bubble initialization complete for ({x}, {y})");
        }

        private Sprite CreateSimpleCircleSprite()
        {
            // Create a simple white circle texture
            var size = 64; // Fixed size for simplicity
            var texture = new Texture2D(size, size);

            var center = new Vector2(size / 2f, size / 2f);
            var radius = size / 2f - 2; // Slightly smaller to avoid edge artifacts

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var pixel = new Vector2(x, y);
                    var distance = Vector2.Distance(pixel, center);

                    if (distance <= radius)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();
            texture.filterMode = FilterMode.Point;

            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64);
            // Debug.Log($"Created simple circle sprite: bounds={sprite.bounds}");
            return sprite;
        }

        public Sprite CreateCircleSprite(float radius)
        {
            return CreateSimpleCircleSprite(); // Use the simple method for now
        }

        public void SetMoving(bool moving)
        {
            isMoving = moving;

            Debug.Log($"Bubble.SetMoving({moving}) called");

            if (rigidbody2D != null)
            {
                rigidbody2D.isKinematic = !moving;
                rigidbody2D.gravityScale = 0f;

                if (!moving)
                {
                    rigidbody2D.linearVelocity = Vector2.zero;
                }

                Debug.Log($"Rigidbody2D set - isKinematic: {rigidbody2D.isKinematic}, velocity: {rigidbody2D.linearVelocity}");
            }
            else
            {
                Debug.LogError("Rigidbody2D is null in SetMoving!");
            }
        }

        public void Shoot(Vector2 direction, float speed)
        {
            Debug.Log($"========== [BARREL FIRED] SHOT {System.Guid.NewGuid().ToString().Substring(0, 8)} ==========");
            
            SetMoving(true);

            Debug.Log($"Bubble.Shoot called with direction: {direction}, speed: {speed}");

            // Store shot parameters for distance-based collider enabling
            shotStartPosition = transform.position;
            shotTime = Time.time;

            if (rigidbody2D != null)
            {
                // Ensure the bubble is not kinematic while moving
                rigidbody2D.isKinematic = false;
                rigidbody2D.gravityScale = 0f;
                rigidbody2D.linearVelocity = direction.normalized * speed;

                Debug.Log($"Bubble velocity set to: {rigidbody2D.linearVelocity}");

                // Disable collider to avoid colliding with starting grid
                // Will re-enable when: time >= 2s OR distance traveled >= 8 units
                if (circleCollider != null)
                {
                    circleCollider.enabled = false;
                    Debug.Log("Shot bubble collider disabled - will re-enable based on time/distance");
                }
            }
            else
            {
                Debug.LogError("Rigidbody2D is null in Shoot method!");
            }
        }

        private void Update()
        {
            // Check if collider should be re-enabled for moving bubbles
            if (isMoving && !circleCollider.enabled)
            {
                if (ShouldEnableCollider())
                {
                    circleCollider.enabled = true;
                    Debug.Log("Shot bubble collider re-enabled (time/distance check passed)");
                }
            }
        }

        private bool ShouldEnableCollider()
        {
            // Check if minimum time has elapsed
            float elapsedTime = Time.time - shotTime;
            if (elapsedTime >= COLLIDER_ENABLE_MIN_TIME)
            {
                Debug.Log($"Enabling collider: time check passed ({elapsedTime:F2}s >= {COLLIDER_ENABLE_MIN_TIME}s)");
                return true;
            }

            // Check if minimum distance has been traveled
            float distanceTraveled = Vector3.Distance(transform.position, shotStartPosition);
            if (distanceTraveled >= COLLIDER_ENABLE_MIN_DISTANCE)
            {
                Debug.Log($"Enabling collider: distance check passed ({distanceTraveled:F2} units >= {COLLIDER_ENABLE_MIN_DISTANCE} units)");
                return true;
            }

            return false;
        }

        public void StopMoving()
        {
            SetMoving(false);

            if (rigidbody2D != null)
            {
                rigidbody2D.linearVelocity = Vector2.zero;
            }

            // Resume grid descent now that bubble has stopped moving
            BubbleShooterGameManager.Instance.SetBubbleInFlight(false);
        }

        public void SetGridPosition(int x, int y)
        {
            gridX = x;
            gridY = y;
        }

        public List<Bubble> GetNeighbors()
        {
            var neighbors = new List<Bubble>();
            var gameManager = BubbleShooterGameManager.Instance;

            // Hexagonal grid neighbors
            var isOddRow = gridY % 2 == 1;

            // Six directions in hexagonal grid
            int[][] neighborOffsets = isOddRow ?
                new int[][]
                {
                    new int[] { 0, -1 },  // top
                    new int[] { 1, -1 },  // top-right
                    new int[] { 1, 0 },   // right
                    new int[] { 1, 1 },   // bottom-right
                    new int[] { 0, 1 },   // bottom
                    new int[] { -1, 0 }   // left (corrected)
                } :
                new int[][]
                {
                    new int[] { -1, -1 }, // top-left
                    new int[] { 0, -1 },  // top
                    new int[] { 1, 0 },   // right
                    new int[] { 0, 1 },   // bottom
                    new int[] { -1, 1 },  // bottom-left
                    new int[] { -1, 0 }   // left
                };

            foreach (var offset in neighborOffsets)
            {
                var neighborX = gridX + offset[0];
                var neighborY = gridY + offset[1];

                var neighbor = gameManager.GetGridBubble(neighborX, neighborY);
                if (neighbor != null && !neighbor.IsMoving)
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        public List<Bubble> GetMatchingNeighbors()
        {
            var matchingBubbles = new List<Bubble>();
            var visited = new HashSet<Bubble>();

            FindMatchingBubbles(this, matchingBubbles, visited);

            return matchingBubbles;
        }

        private void FindMatchingBubbles(Bubble current, List<Bubble> matchingBubbles, HashSet<Bubble> visited)
        {
            if (visited.Contains(current) || current.BubbleColor != bubbleColor)
            {
                return;
            }

            visited.Add(current);
            matchingBubbles.Add(current);

            var neighbors = current.GetNeighbors();
            foreach (var neighbor in neighbors)
            {
                FindMatchingBubbles(neighbor, matchingBubbles, visited);
            }
        }

        public void Pop()
        {
            // Add particle effect or animation here if desired
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isMoving) return;

            Debug.Log($"*** COLLISION at position {transform.position} with: {other.gameObject.name}, collider enabled: {other.enabled}, my velocity: {rigidbody2D.linearVelocity}");

            var otherBubble = other.GetComponent<Bubble>();
            if (otherBubble != null && !otherBubble.IsMoving)
            {
                Debug.Log($"  -> Collision with STATIONARY BUBBLE at position: {otherBubble.transform.position}");
                // Collision with stationary bubble
                OnBubbleCollision(otherBubble);
            }
            else if (otherBubble != null && otherBubble.IsMoving)
            {
                Debug.Log($"  -> Ignoring collision with MOVING BUBBLE (probably another shot bubble)");
                return;
            }
            else
            {
                // Check for collision with boundaries
                if (other.gameObject.tag == "Boundary")
                {
                    Debug.Log($"  -> Boundary collision with: {other.gameObject.name}");

                    if (other.gameObject.name.Contains("Wall"))
                    {
                        // Bounce off walls
                        Debug.Log("Wall bounce");
                        var rb = GetComponent<Rigidbody2D>();
                        if (rb != null)
                        {
                            var velocity = rb.linearVelocity;
                            velocity.x = -velocity.x; // Reverse X velocity
                            rb.linearVelocity = velocity;
                            Debug.Log($"Wall bounce - new velocity: {rb.linearVelocity}");
                        }
                    }
                    else if (other.gameObject.name.Contains("Ceiling"))
                    {
                        // Stop at ceiling
                        Debug.Log("Ceiling collision");
                        OnBubbleCollision(null); // Treat as collision with ceiling
                    }
                }
            }
        }

        private void OnBubbleCollision(Bubble targetBubble)
        {
            StopMoving();

            int gridX, gridY;
            Debug.Log($"=== COLLISION START === Bubble at world pos: {transform.position}");

            try
            {
                if (targetBubble != null)
                {
                    // Collision with another bubble - snap to nearest grid position
                    Debug.Log($"[SNAP] About to call FindNearestGridCoordinates()");
                    var nearestCoords = FindNearestGridCoordinates();
                    Debug.Log($"[SNAP] FindNearestGridCoordinates returned: ({nearestCoords.x}, {nearestCoords.y})");
                    gridX = nearestCoords.x;
                    gridY = nearestCoords.y;
                    Debug.Log($"Bubble collision: nearest grid coords ({gridX}, {gridY})");
                }
                else
                {
                    // Collision with ceiling - find the nearest valid grid position at the top
                    var gameManager = BubbleShooterGameManager.Instance;
                    var radius = gameManager.GetBubbleRadius();
                    var gridWidth = 11; // gameManager.gridWidth;

                    // Find the column closest to current position
                    var currentX = transform.position.x;
                    gridX = Mathf.RoundToInt((currentX + gridWidth * radius) / (radius * 2f));
                    gridX = Mathf.Clamp(gridX, 0, gridWidth - 1);
                    gridY = 0;

                    Debug.Log($"Ceiling collision: currentX={currentX}, calculated gridX={gridX}");
                }

                // Snap to the calculated grid position
                Debug.Log($"[SNAP] About to snap. gridX={gridX}, gridY={gridY}");
                var snapPosition = GetGridPosition(gridX, gridY);
                Debug.Log($"Snapping to grid ({gridX}, {gridY}), world pos: {snapPosition}");
                transform.position = snapPosition;

                Debug.Log($"Final grid coords: ({gridX}, {gridY}), Final world pos: {transform.position}");
                SnapToGrid(gridX, gridY);

                // Check for matches
                CheckForMatches();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SNAP] EXCEPTION in OnBubbleCollision during snap/match logic: {ex.Message}\n{ex.StackTrace}");
                throw;
            }

            // Notify the shooter that this bubble has stopped
            var shooter = FindObjectOfType<BubbleShooter>();
            if (shooter != null)
            {
                shooter.OnBubbleStopped();
            }

            // Reset visual effects
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                var color = spriteRenderer.color;
                color.a = 1f; // Reset to full opacity
                spriteRenderer.color = color;
            }
            transform.localScale = Vector3.one; // Reset scale
        }

        private Vector2Int FindNearestGridCoordinates()
        {
            var gameManager = BubbleShooterGameManager.Instance;
            var bubbleRadius = gameManager.GetBubbleRadius();

            // Use multi-point Y-shape collision geometry to determine snap cell
            // Find all bubbles currently touching this shot bubble
            var overlapCircles = Physics2D.OverlapCircleAll(
                new Vector2(transform.position.x, transform.position.y),
                bubbleRadius * 2.5f  // Large enough to detect all neighboring bubbles
            );

            // Collect candidate cells and their vote counts
            Dictionary<Vector2Int, float> candidateCells = new Dictionary<Vector2Int, float>();
            
            Debug.Log($"[SNAP Y-SHAPE] Processing {overlapCircles.Length} overlapping colliders");
            foreach (var collider in overlapCircles)
            {
                try
                {
                    var contactBubble = collider.GetComponent<Bubble>();
                    if (contactBubble == null || contactBubble == this)
                    {
                        Debug.Log($"[SNAP Y-SHAPE] Skipping: contactBubble null or self");
                        continue;
                    }

                    // For each bubble we're touching, analyze the Y-shape geometry
                    var contactGridX = contactBubble.gridX;
                    var contactGridY = contactBubble.gridY;
                    
                    // Skip bubbles with invalid grid coordinates (likely destroyed or transitional)
                    if (contactGridX < 0 || contactGridY < 0)
                    {
                        Debug.Log($"[SNAP Y-SHAPE] Skipping contact bubble with invalid grid coords ({contactGridX},{contactGridY})");
                        continue;
                    }
                    
                    Debug.Log($"[SNAP Y-SHAPE] Analyzing contact at ({contactGridX},{contactGridY})...");
                    var bestCell = FindBestCellForContact(contactGridX, contactGridY);
                    Debug.Log($"[SNAP Y-SHAPE] FindBestCellForContact returned ({bestCell.x},{bestCell.y})");
                    
                    if (candidateCells.ContainsKey(bestCell))
                        candidateCells[bestCell] += 1.0f;
                    else
                        candidateCells[bestCell] = 1.0f;
                        
                    Debug.Log($"[SNAP Y-SHAPE] Contact with bubble at ({contactGridX},{contactGridY}) → candidate cell ({bestCell.x},{bestCell.y})");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[SNAP Y-SHAPE] EXCEPTION processing contact: {ex.Message}\n{ex.StackTrace}");
                }
            }
            Debug.Log($"[SNAP Y-SHAPE] Loop complete. Total candidate cells: {candidateCells.Count}");

            // If we detected contacts, use the highest-voted cell
            Debug.Log($"[SNAP] candidateCells.Count = {candidateCells.Count}");
            if (candidateCells.Count > 0)
            {
                try
                {
                    var sortedCells = candidateCells.OrderByDescending(x => x.Value).ToList();
                    Debug.Log($"[SNAP] After sort: {sortedCells.Count} cells");
                    
                    if (sortedCells.Count > 0)
                    {
                        var bestSnap = sortedCells.First().Key;
                        var voteCount = candidateCells[bestSnap];
                        Debug.Log($"[SNAP] Multi-point analysis selected grid ({bestSnap.x}, {bestSnap.y}) with {voteCount} contact votes");
                        
                        // Log all votes for debugging
                        Debug.Log($"[SNAP] Vote breakdown: {string.Join(", ", sortedCells.Select(x => $"({x.Key.x},{x.Key.y}):{x.Value}"))}");
                        
                        return bestSnap;
                    }
                    else
                    {
                        Debug.LogError("[SNAP] ERROR: sortedCells is empty after OrderByDescending!");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[SNAP] EXCEPTION during vote analysis: {ex.Message}\n{ex.StackTrace}");
                }
            }

            // Fallback: no contacts detected, use simple nearest empty cell
            Debug.Log($"[SNAP] No contacts detected or error occurred, falling back to nearest empty cell");
            return FindNearestEmptyGridCell();
        }

        private Vector2Int FindBestCellForContact(int contactX, int contactY)
        {
            var gameManager = BubbleShooterGameManager.Instance;
            var contactBubbleWorldPos = GetGridPosition(contactX, contactY);
            var shotBubbleWorldPos = new Vector2(transform.position.x, transform.position.y);
            
            // For hex grid: analyze which side of the Y-shape the shot bubble is on
            // The Y-shape connects: the contact bubble (green) and two neighbors (blue/red)
            
            bool isOddRow = contactY % 2 == 1;
            
            // Get the 6 neighboring cells for hex grid
            List<Vector2Int> neighborCells = new List<Vector2Int>();
            
            if (isOddRow)
            {
                // Odd rows are offset RIGHT, so neighbors in adjacent even rows are shifted LEFT
                neighborCells.Add(new Vector2Int(contactX - 1, contactY - 1));  // Upper-left
                neighborCells.Add(new Vector2Int(contactX, contactY - 1));      // Upper-right
                neighborCells.Add(new Vector2Int(contactX - 1, contactY));      // Left
                neighborCells.Add(new Vector2Int(contactX + 1, contactY));      // Right
                neighborCells.Add(new Vector2Int(contactX - 1, contactY + 1));  // Lower-left
                neighborCells.Add(new Vector2Int(contactX, contactY + 1));      // Lower-right
            }
            else
            {
                // Even rows are not offset, so neighbors in adjacent odd rows are shifted RIGHT
                neighborCells.Add(new Vector2Int(contactX, contactY - 1));      // Upper-left
                neighborCells.Add(new Vector2Int(contactX + 1, contactY - 1));  // Upper-right
                neighborCells.Add(new Vector2Int(contactX - 1, contactY));      // Left
                neighborCells.Add(new Vector2Int(contactX + 1, contactY));      // Right
                neighborCells.Add(new Vector2Int(contactX, contactY + 1));      // Lower-left
                neighborCells.Add(new Vector2Int(contactX + 1, contactY + 1));  // Lower-right
            }
            
            // Find the closest EMPTY cell: either the contact cell or one of its neighbors
            float closestDistance = float.MaxValue;
            Vector2Int bestCell = new Vector2Int(contactX, contactY);
            
            // Check if contact cell itself is empty
            if (gameManager.GetGridBubble(contactX, contactY) == null)
            {
                closestDistance = Vector2.Distance(shotBubbleWorldPos, (Vector2)contactBubbleWorldPos);
                bestCell = new Vector2Int(contactX, contactY);
                Debug.Log($"[SNAP GEOMETRY] Contact cell ({contactX},{contactY}) is empty, distance {closestDistance:F2}");
            }
            
            // Check all neighbors
            foreach (var neighbor in neighborCells)
            {
                // Bounds check
                if (neighbor.x < 0 || neighbor.x >= 11 || neighbor.y < 0 || neighbor.y >= 12)
                    continue;
                
                // Skip occupied cells
                if (gameManager.GetGridBubble(neighbor.x, neighbor.y) != null)
                    continue;
                
                var neighborWorldPos = GetGridPosition(neighbor.x, neighbor.y);
                var distance = Vector2.Distance(shotBubbleWorldPos, (Vector2)neighborWorldPos);
                
                Debug.Log($"[SNAP GEOMETRY] Neighbor ({neighbor.x},{neighbor.y}) empty, distance {distance:F2}");
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestCell = neighbor;
                    Debug.Log($"[SNAP GEOMETRY] → New best: ({bestCell.x},{bestCell.y}) distance {closestDistance:F2}");
                }
            }
            
            Debug.Log($"[SNAP GEOMETRY] Final choice for contact ({contactX},{contactY}): ({bestCell.x},{bestCell.y})");
            return bestCell;
        }

        private Vector2Int FindNearestEmptyGridCell()
        {
            var gameManager = BubbleShooterGameManager.Instance;
            var closestDistance = float.MaxValue;
            var closestCoords = Vector2Int.zero;

            for (var y = 0; y < 12; y++)
            {
                for (var x = 0; x < 11; x++)
                {
                    if (gameManager.GetGridBubble(x, y) != null)
                        continue;

                    var gridPos = GetGridPosition(x, y);
                    var distance = Vector2.Distance(transform.position, gridPos);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestCoords = new Vector2Int(x, y);
                    }
                }
            }

            Debug.Log($"[SNAP] Fallback nearest empty: ({closestCoords.x}, {closestCoords.y})");
            return closestCoords;
        }

        private Vector3 GetGridPosition(int x, int y)
        {
            var gameManager = BubbleShooterGameManager.Instance;
            
            // Use the same calculation as GameManager.GetBubblePosition for consistency
            var offsetX = (y % 2 == 1) ? gameManager.GetBubbleRadius() : 0f;
            var xPos = x * gameManager.GetBubbleRadius() * 2f + offsetX - (11 * gameManager.GetBubbleRadius());
            
            // Get the game setup to position grid correctly (same as in GameManager)
            var gameSetup = FindObjectOfType<BubbleShooterGameSetup>();
            var yPos = gameSetup != null 
                ? gameSetup.gridTopY - (y * gameManager.GetBubbleRadius() * 1.732f)
                : y * gameManager.GetBubbleRadius() * 1.732f;
            
            // Apply grid descent offset (this ensures all bubbles stay synchronized)
            yPos += gameManager.GetGridOffsetY();

            return new Vector3(xPos, yPos, 0);
        }

        private void SnapToGrid(int x, int y)
        {
            var gameManager = BubbleShooterGameManager.Instance;

            // Safety check: ensure grid position is empty
            var existingBubble = gameManager.GetGridBubble(x, y);
            if (existingBubble != null && existingBubble != this)
            {
                Debug.LogWarning($"[SYNC] WARNING: Grid position ({x}, {y}) already occupied! Existing bubble will be replaced!");
            }

            // Update grid position
            SetGridPosition(x, y);
            gameManager.SetGridBubble(x, y, this);

            // Snap visual position
            var gridPos = GetGridPosition(x, y);
            transform.position = gridPos;
            
            Debug.Log($"[SYNC] Bubble snapped to grid ({x}, {y}) at world pos: {gridPos}, offset: {gameManager.GetGridOffsetY()}");
            
            // CRITICAL: Verify bubble is actually in grid (for descent sync)
            var verifyBubble = gameManager.GetGridBubble(x, y);
            if (verifyBubble != this)
            {
                Debug.LogError($"[SYNC ERROR] CRITICAL: Bubble NOT properly added to grid[{x},{y}]! This bubble will not descend with grid!");
            }
        }

        private void CheckForMatches()
        {
            var matchingBubbles = GetMatchingNeighbors();

            if (matchingBubbles.Count >= 3)
            {
                // Pop all matching bubbles
                var score = matchingBubbles.Count * 10;
                BubbleShooterGameManager.Instance.AddScore(score);

                foreach (var bubble in matchingBubbles)
                {
                    var gameManager = BubbleShooterGameManager.Instance;
                    gameManager.SetGridBubble(bubble.GridX, bubble.GridY, null);
                    bubble.Pop();
                }

                // Check for floating bubbles
                CheckForFloatingBubbles();
            }
        }

        private void CheckForFloatingBubbles()
        {
            var gameManager = BubbleShooterGameManager.Instance;
            var gridHeight = 12; // gameManager.gridHeight
            var gridWidth = 11; // gameManager.gridWidth

            var connectedBubbles = new HashSet<Bubble>();

            // Find all bubbles connected to the top row
            for (var x = 0; x < gridWidth; x++)
            {
                var topBubble = gameManager.GetGridBubble(x, 0);
                if (topBubble != null)
                {
                    FindConnectedBubbles(topBubble, connectedBubbles);
                }
            }

            // Remove floating bubbles
            for (var y = 0; y < gridHeight; y++)
            {
                for (var x = 0; x < gridWidth; x++)
                {
                    var bubble = gameManager.GetGridBubble(x, y);
                    if (bubble != null && !connectedBubbles.Contains(bubble))
                    {
                        gameManager.SetGridBubble(x, y, null);
                        bubble.Pop();
                        BubbleShooterGameManager.Instance.AddScore(20); // Bonus for floating bubbles
                    }
                }
            }
        }

        private void FindConnectedBubbles(Bubble current, HashSet<Bubble> connectedBubbles)
        {
            if (connectedBubbles.Contains(current))
            {
                return;
            }

            connectedBubbles.Add(current);

            var neighbors = current.GetNeighbors();
            foreach (var neighbor in neighbors)
            {
                FindConnectedBubbles(neighbor, connectedBubbles);
            }
        }
    }
}