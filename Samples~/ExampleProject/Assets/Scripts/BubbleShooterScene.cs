using UnityEngine;

namespace BubbleShooter
{
    public class BubbleShooterScene : MonoBehaviour
    {
        [Header("Scene Configuration")]
        public bool autoSetup = true;
        public int gridWidth = 11;
        public int gridHeight = 12;
        public float bubbleRadius = 0.5f;

        private void Start()
        {
            if (autoSetup)
            {
                SetupScene();
            }
        }

        private void SetupScene()
        {
            // Add the game setup component
            var gameSetup = gameObject.AddComponent<BubbleShooterGameSetup>();

            // Configure the setup
            gameSetup.gridWidth = gridWidth;
            gameSetup.gridHeight = gridHeight;
            gameSetup.bubbleRadius = bubbleRadius;
        }

        private void OnDrawGizmos()
        {
            // Draw game area bounds
            Gizmos.color = Color.yellow;
            var center = new Vector3(0, gridHeight * bubbleRadius * 0.433f, 0);
            var size = new Vector3(gridWidth * bubbleRadius * 2, gridHeight * bubbleRadius * 1.732f, 1);
            Gizmos.DrawWireCube(center, size);

            // Draw shooter position
            Gizmos.color = Color.green;
            var shooterPos = new Vector3(0, -gridHeight * bubbleRadius * 0.433f + 1f, 0);
            Gizmos.DrawWireSphere(shooterPos, bubbleRadius);
        }
    }
}