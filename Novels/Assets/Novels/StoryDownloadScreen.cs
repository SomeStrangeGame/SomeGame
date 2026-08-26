using UnityEngine;
using UnityEngine.UI;

namespace Novels
{
    internal sealed class StoryDownloadScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RawImage _blurredFrame;
        [SerializeField] private RectTransform _progressFill;
        [SerializeField] private Text _details;
        [SerializeField] private Text _remaining;
        private Catalog.CatalogAction _downloadAllAction;
        private Button _downloadAllButton;
        private Text _downloadAllText;

        internal void SetVisible(bool visible)
        {
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.blocksRaycasts = visible;
            _canvasGroup.interactable = visible;
        }

        internal void SetFrame(RenderTexture frame)
        {
            _blurredFrame.texture = frame;
            _blurredFrame.enabled = frame != null;
        }

        internal void SetProgress(float ratio, string details, string remaining)
        {
            var anchors = _progressFill.anchorMax;
            anchors.x = Mathf.Clamp01(ratio);
            _progressFill.anchorMax = anchors;
            _details.text = details;
            _remaining.text = remaining;
        }

        internal void BindDownloadAll(Catalog.CatalogAction action)
        {
            if (_downloadAllAction != null)
                _downloadAllAction.Changed -= RefreshDownloadAll;
            _downloadAllAction = action;
            if (_downloadAllAction == null)
                return;
            EnsureDownloadAllButton();
            _downloadAllAction.Changed += RefreshDownloadAll;
            RefreshDownloadAll();
        }

        private void RefreshDownloadAll()
        {
            if (_downloadAllAction == null || _downloadAllButton == null)
                return;
            _downloadAllText.text = _downloadAllAction.Text;
            _downloadAllButton.interactable = _downloadAllAction.IsInteractable;
        }

        private void EnsureDownloadAllButton()
        {
            if (_downloadAllButton != null)
                return;
            var panel = _details.transform.parent as RectTransform;
            if (panel == null)
                return;
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 250f);
            var buttonObject = new GameObject(
                "Download All",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            buttonObject.transform.SetParent(panel, false);
            var rect = (RectTransform)buttonObject.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(22f, -230f);
            rect.offsetMax = new Vector2(-22f, -184f);
            buttonObject.GetComponent<Image>().color = new Color(0.12f, 0.55f, 0.85f, 1f);
            _downloadAllButton = buttonObject.GetComponent<Button>();
            _downloadAllButton.onClick.AddListener(
                () => _downloadAllAction?.Invoke());

            var textObject = new GameObject(
                "Text",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);
            var textRect = (RectTransform)textObject.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8f, 3f);
            textRect.offsetMax = new Vector2(-8f, -3f);
            _downloadAllText = textObject.GetComponent<Text>();
            _downloadAllText.font = _details.font;
            _downloadAllText.fontSize = 18;
            _downloadAllText.alignment = TextAnchor.MiddleCenter;
            _downloadAllText.color = Color.white;
            _downloadAllText.raycastTarget = false;
        }

        private void OnDestroy()
        {
            if (_downloadAllAction != null)
                _downloadAllAction.Changed -= RefreshDownloadAll;
        }
    }
}
