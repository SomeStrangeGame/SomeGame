using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Loading.View
{
    public sealed class Screen : MonoBehaviour
    {
        [SerializeField] private float _markerSpeed = 15f;
        [SerializeField] private Transform _marker;
        [SerializeField] private float _showHideDuration;
        [SerializeField] private CanvasGroup _canvasGroup;

        private void Update()
        {
            _marker.rotation *= Quaternion.Euler(Vector3.up * _markerSpeed * Time.deltaTime);
        }

        internal void ShowImmediate()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.gameObject.SetActive(true);
        }

        internal async UniTask Show()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(true);

            var delayMs = 50;
            var deltaTime = delayMs / 1000f;

            var timer = _showHideDuration;
            while (timer >= 0f)
            {
                _canvasGroup.alpha = 1f - (timer / _showHideDuration);
                timer -= deltaTime;
                await UniTask.Delay(delayMs, true);
            }

            _canvasGroup.alpha = 1f;
        }

        internal void HideImmediate()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(false);
        }

        internal async UniTask Hide()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.gameObject.SetActive(true);

            var delayMs = 50;
            var deltaTime = delayMs / 1000f;

            var timer = _showHideDuration;
            while (timer >= 0f)
            {
                _canvasGroup.alpha = timer / _showHideDuration;
                timer -= deltaTime;
                await UniTask.Delay(delayMs, true);
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(false);
        }
    }
}

