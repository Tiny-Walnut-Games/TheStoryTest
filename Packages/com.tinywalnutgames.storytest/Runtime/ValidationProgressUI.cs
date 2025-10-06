using UnityEngine;
using UnityEngine.UI;

namespace TinyWalnutGames.StoryTest
{
    /// <summary>
    /// Simple UI overlay that shows validation progress during runtime.
    /// Auto-creates a Canvas with progress text.
    /// </summary>
    [StoryIgnore("UI overlay for validation progress tracking")]
    public class ValidationProgressUI : MonoBehaviour
    {
        private Canvas _canvas;
        private Text _statusText;
        private Text _progressText;
        private GameObject _panel;

        private void Awake()
        {
            CreateUI();
        }

        private void CreateUI()
        {
            // Create Canvas
            var canvasGo = new GameObject("Story Test Progress Canvas");
            canvasGo.transform.SetParent(transform);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 9999;

            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            // Create Panel (semi-transparent background)
            _panel = new GameObject("Panel");
            _panel.transform.SetParent(canvasGo.transform, false);
            var panelRect = _panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0);
            panelRect.anchorMax = new Vector2(0.5f, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.anchoredPosition = new Vector2(0, 20);
            panelRect.sizeDelta = new Vector2(400, 100);

            var panelImage = _panel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);

            // Create Status Text (bold header)
            var statusGo = new GameObject("Status Text");
            statusGo.transform.SetParent(_panel.transform, false);
            var statusRect = statusGo.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0, 0.5f);
            statusRect.anchorMax = new Vector2(1, 1);
            statusRect.pivot = new Vector2(0.5f, 0.5f);
            statusRect.anchoredPosition = Vector2.zero;
            statusRect.sizeDelta = new Vector2(-20, 0);

            _statusText = statusGo.AddComponent<Text>();
            _statusText.text = "Validation Running...";
            _statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _statusText.fontSize = 18;
            _statusText.alignment = TextAnchor.MiddleCenter;
            _statusText.color = Color.white;
            _statusText.fontStyle = FontStyle.Bold;

            // Create Progress Text (detail)
            var progressGo = new GameObject("Progress Text");
            progressGo.transform.SetParent(_panel.transform, false);
            var progressRect = progressGo.AddComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(0, 0);
            progressRect.anchorMax = new Vector2(1, 0.5f);
            progressRect.pivot = new Vector2(0.5f, 0.5f);
            progressRect.anchoredPosition = Vector2.zero;
            progressRect.sizeDelta = new Vector2(-20, 0);

            _progressText = progressGo.AddComponent<Text>();
            _progressText.text = "Initializing...";
            _progressText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _progressText.fontSize = 14;
            _progressText.alignment = TextAnchor.MiddleCenter;
            _progressText.color = new Color(0.8f, 0.8f, 0.8f);

            Hide();
        }

        public void Show()
        {
            if (_panel != null)
            {
                _panel.SetActive(true);
            }
        }

        public void Hide()
        {
            if (_panel != null)
            {
                _panel.SetActive(false);
            }
        }

        public void UpdateStatus(string status)
        {
            if (_statusText != null)
            {
                _statusText.text = status;
            }
        }

        public void UpdateProgress(string progress)
        {
            if (_progressText != null)
            {
                _progressText.text = progress;
            }
        }

        public void SetComplete(int violationCount)
        {
            if (_statusText != null)
            {
                _statusText.text = violationCount == 0 ? "✓ Validation Passed" : $"✗ {violationCount} Violations Found";
                _statusText.color = violationCount == 0 ? Color.green : new Color(1f, 0.6f, 0);
            }

            if (_progressText != null)
            {
                _progressText.text = "Check Console for details";
            }
        }

        private void OnDestroy()
        {
            if (_canvas != null && _canvas.gameObject != null)
            {
                Destroy(_canvas.gameObject);
            }
        }
    }
}
