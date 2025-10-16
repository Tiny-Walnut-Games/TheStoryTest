using UnityEngine;

namespace BubbleShooter
{
    public class BubbleShooter : MonoBehaviour
    {
        [Header("Shooter Settings")]
        public Transform shootPoint;
        public LineRenderer aimLine;
        public int aimLineSegments = 10;
        public float aimLineLength = 5f;

        [Header("Next Bubble Preview")]
        public Transform nextBubblePosition;

        private Bubble currentBubble;
        private Bubble nextBubble;
        private bool canShoot = true;
        private Vector2 aimDirection;

        private void Start()
        {
            InitializeShooter();
        }

        private void Update()
        {
            HandleInput();
            UpdateAimLine();
        }

        private void InitializeShooter()
        {
            // Create initial bubbles
            CreateNextBubble();
            LoadNextBubble();
        }

        private void HandleInput()
        {
            if (!canShoot || currentBubble == null) return;

            var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;

            // Calculate aim direction
            aimDirection = (mousePosition - shootPoint.position).normalized;

            // Limit shooting angle (can't shoot backwards)
            if (aimDirection.y < 0)
            {
                aimDirection.y = 0;
                aimDirection = aimDirection.normalized;
            }

            // Shoot on click
            if (Input.GetMouseButtonDown(0))
            {
                ShootBubble();
            }
        }

        private void UpdateAimLine()
        {
            if (aimLine == null || currentBubble == null || !canShoot) return;

            aimLine.positionCount = aimLineSegments;

            var startPosition = shootPoint.position;
            var direction = new Vector3(aimDirection.x, aimDirection.y, 0);

            for (var i = 0; i < aimLineSegments; i++)
            {
                var t = (float)i / (aimLineSegments - 1);
                var position = startPosition + direction * (t * aimLineLength);
                aimLine.SetPosition(i, position);
            }

            // Enable/disable aim line based on whether we can shoot
            aimLine.enabled = canShoot && currentBubble != null;
        }

        private void ShootBubble()
        {
            if (currentBubble == null || !canShoot) return;

            Debug.Log($"Shooting bubble in direction: {aimDirection}");
            canShoot = false;

            // Position bubble at shoot point and ensure it's visible
            currentBubble.transform.position = shootPoint.position;
            currentBubble.transform.localScale = Vector3.one;

            // Make sure the bubble is visible and has proper components
            var spriteRenderer = currentBubble.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.color = currentBubble.BubbleColor;
                Debug.Log($"Bubble sprite enabled with color: {spriteRenderer.color}");
            }

            // Enable physics for the bubble
            currentBubble.SetMoving(true);

            // Shoot the bubble with proper physics
            var shootSpeed = BubbleShooterGameManager.Instance.GetShootSpeed();
            currentBubble.Shoot(aimDirection, shootSpeed);

            Debug.Log($"Bubble shot with speed: {shootSpeed} from position: {shootPoint.position}");

            // Pause grid descent while bubble is in flight
            BubbleShooterGameManager.Instance.SetBubbleInFlight(true);

            // Add a simple trail effect to make movement more visible
            AddTrailEffect(currentBubble);

            // Load next bubble after a short delay to ensure the shot bubble is properly set up
            Invoke(nameof(LoadNextBubbleDelayed), 0.1f);
        }

        private void LoadNextBubbleDelayed()
        {
            LoadNextBubble();
        }

        private void CreateNextBubble()
        {
            var gameManager = BubbleShooterGameManager.Instance;
            var colors = gameManager.GetBubbleColors();
            var bubbleRadius = gameManager.GetBubbleRadius();

            // Create bubble object
            var bubbleObj = new GameObject("NextBubble")
            {
                transform =
                {
                    position = nextBubblePosition.position
                }
            };

            // Add components in the correct order
            var spriteRenderer = bubbleObj.AddComponent<SpriteRenderer>();
            var collider = bubbleObj.AddComponent<CircleCollider2D>();
            var rigidbody = bubbleObj.AddComponent<Rigidbody2D>();
            var bubble = bubbleObj.AddComponent<Bubble>();

            // Initialize bubble
            var randomColor = colors[Random.Range(0, colors.Length)];
            bubble.Initialize(-1, -1, randomColor, bubbleRadius);

            // Disable collider for next bubble preview (it won't be shot yet)
            collider.enabled = false;

            // Make it smaller for preview
            bubbleObj.transform.localScale = Vector3.one * 0.7f;

            nextBubble = bubble;
        }

        private void LoadNextBubble()
        {
            if (nextBubble == null)
            {
                CreateNextBubble();
            }

            currentBubble = nextBubble;
            currentBubble.transform.position = shootPoint.position;
            currentBubble.transform.localScale = Vector3.one;

            // Enable collider for current bubble now that it's being shot
            var collider = currentBubble.GetComponent<CircleCollider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }

            // Ensure the bubble is visible and properly set up
            var spriteRenderer = currentBubble.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite == null)
            {
                Debug.Log("Current bubble has no sprite, creating one");
                var gameManager = BubbleShooterGameManager.Instance;
                var radius = gameManager.GetBubbleRadius();
                spriteRenderer.sprite = currentBubble.CreateCircleSprite(radius);
            }

            // Reset physics
            var rb = currentBubble.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.isKinematic = true;
            }

            Debug.Log($"Loaded current bubble at position: {currentBubble.transform.position}, scale: {currentBubble.transform.localScale}");

            // Create new next bubble
            CreateNextBubble();

            canShoot = true;
        }

        public void OnBubbleStopped()
        {
            // Called when a bubble stops moving and attaches to the grid
            Debug.Log("OnBubbleStopped called - enabling shooting");
            canShoot = true;
        }

        private void AddTrailEffect(Bubble bubble)
        {
            // Create a simple trail by making the bubble slightly larger and more opaque while moving
            var spriteRenderer = bubble.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                var color = spriteRenderer.color;
                color.a = 0.8f; // Make it more opaque while moving
                spriteRenderer.color = color;

                // Scale up slightly for visibility
                bubble.transform.localScale = Vector3.one * 1.1f;
            }
        }

        private void OnDrawGizmos()
        {
            if (shootPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(shootPoint.position, 0.2f);
            }

            if (nextBubblePosition != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(nextBubblePosition.position, 0.2f);
            }
        }
    }
}