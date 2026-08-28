using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Novels
{
    internal sealed class StoryStreamingProgressScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _progressFill;
        [SerializeField] private Text _label;

        private Coroutine _hideRoutine;

        internal void SetProgress(float ratio)
        {
            CancelHide();
            SetVisible(true);
            ratio = Mathf.Clamp01(ratio);
            var anchors = _progressFill.anchorMax;
            anchors.x = ratio;
            _progressFill.anchorMax = anchors;
            _label.text = $"Загрузка истории · {ratio:P0}";
        }

        internal void SetComplete()
        {
            CancelHide();
            var anchors = _progressFill.anchorMax;
            anchors.x = 1f;
            _progressFill.anchorMax = anchors;
            _label.text = "История доступна офлайн";
            SetVisible(true);
            _hideRoutine = StartCoroutine(HideAfterDelay());
        }

        internal void SetInterrupted(float ratio)
        {
            SetProgress(ratio);
            _label.text = $"Загрузка истории приостановлена · {ratio:P0}";
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSecondsRealtime(2f);
            SetVisible(false);
            _hideRoutine = null;
        }

        private void SetVisible(bool visible)
        {
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        private void CancelHide()
        {
            if (_hideRoutine == null)
                return;
            StopCoroutine(_hideRoutine);
            _hideRoutine = null;
        }
    }
}
