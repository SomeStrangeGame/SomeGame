using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Novels.Notification.View
{
    public class NotificationScreen : MonoBehaviour
    {
        [SerializeField] private Text _text;
        [SerializeField] private float _showHideDuration;
        [SerializeField] private CanvasGroup _canvasGroup;

        public void ShowImmediate()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.gameObject.SetActive(true);
        }

        public async UniTask Show(CancellationToken cancellationToken)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(true);

            await global::UITransitions.Transition.Fade(
                _canvasGroup,
                0f,
                1f,
                _showHideDuration,
                cancellationToken);
        }

        public void HideImmediate()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(false);
        }

        public async UniTask Hide(CancellationToken cancellationToken)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.gameObject.SetActive(true);

            await global::UITransitions.Transition.Fade(
                _canvasGroup,
                1f,
                0f,
                _showHideDuration,
                cancellationToken);
            _canvasGroup.gameObject.SetActive(false);
        }

        public void SetText(string text)
        {
            _text.text = text;
        }
    }
}
