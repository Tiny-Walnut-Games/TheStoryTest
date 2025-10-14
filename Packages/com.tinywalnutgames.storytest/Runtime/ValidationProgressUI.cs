using UnityEngine;
using UnityEngine.UI;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest
{
    /// <summary>
    /// Simple UI overlay that shows validation progress during runtime.
    /// Auto-creates a Canvas with progress text.
    /// </summary>
    [StoryIgnore("UI overlay for validation progress tracking")]
    public class ValidationProgressUI : MonoBehaviour
    {
        private Canvas canvas;
        private Text statusText;
        private Text progressText;
        private GameObject panel;

        private void Awake()
        {
            CreateUI();
        }

        private void CreateUI()
        {
            // Create Canvas
            var canvasGo = new GameObject("Story Test Progress Canvas");
            canvasGo.transform.SetParent(transform);
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            // Create Panel (semi-transparent background)
            panel = new GameObject("Panel");
            panel.transform.SetParent(canvasGo.transform, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0);
            panelRect.anchorMax = new Vector2(0.5f, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.anchoredPosition = new Vector2(0, 20);
            panelRect.sizeDelta = new Vector2(400, 100);

            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);

            // Create Status Text (bold header)
            var statusGo = new GameObject("Status Text");
            statusGo.transform.SetParent(panel.transform, false);
            var statusRect = statusGo.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0, 0.5f);
            statusRect.anchorMax = new Vector2(1, 1);
            statusRect.pivot = new Vector2(0.5f, 0.5f);
            statusRect.anchoredPosition = Vector2.zero;
            statusRect.sizeDelta = new Vector2(-20, 0);

            statusText = statusGo.AddComponent<Text>();
            statusText.text = "Validation Running...";
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.fontSize = 18;
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.color = Color.white;
            statusText.fontStyle = FontStyle.Bold;

            // Create Progress Text (detail)
            var progressGo = new GameObject("Progress Text");
            progressGo.transform.SetParent(panel.transform, false);
            var progressRect = progressGo.AddComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(0, 0);
            progressRect.anchorMax = new Vector2(1, 0.5f);
            progressRect.pivot = new Vector2(0.5f, 0.5f);
            progressRect.anchoredPosition = Vector2.zero;
            progressRect.sizeDelta = new Vector2(-20, 0);

            progressText = progressGo.AddComponent<Text>();
            progressText.text = "Initializing...";
            progressText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            progressText.fontSize = 14;
            progressText.alignment = TextAnchor.MiddleCenter;
            progressText.color = new Color(0.8f, 0.8f, 0.8f);

            Hide();
        }

        public void Show()
        {
            panel?.SetActive(true);
        }

        private void Hide()
        {
            panel?.SetActive(false);
        }

        public void UpdateStatus(string status)
        {
            if (statusText is not null)
            {
                statusText.text = status;
            }
        }

        public void UpdateProgress(string progress)
        {
            if (progressText is not null)
            {
                progressText.text = progress;
            }
        }

        public void SetComplete(int violationCount)
        {
            if (statusText is not null)
            {
                statusText.text = violationCount == 0 ? "✓ Validation Passed" : $"✗ {violationCount} Violations Found";
                statusText.color = violationCount == 0 ? Color.green : new Color(1f, 0.6f, 0);
            }

            if (progressText is not null)
            {
                progressText.text = "Check Console for details";
            }
        }

        private void OnDestroy()
        {
            if (canvas?.gameObject is not null)
            {
                Destroy(canvas.gameObject);
            }
        }
    }
}
