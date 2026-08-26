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
    }
}
