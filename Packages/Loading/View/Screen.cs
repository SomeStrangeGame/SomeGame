using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Loading.View
{
    public sealed class Screen : MonoBehaviour
    {
        [SerializeField] private float _markerSpeed = 15f;
        [SerializeField] private Transform _marker;
        [SerializeField] private float _showHideDuration;
        [SerializeField] private CanvasGroup _canvasGroup;

        private void Update()
        {
            _marker.rotation *= Quaternion.Euler(_markerSpeed * Time.unscaledDeltaTime * Vector3.up);
        }

        public async UniTask Show()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(true);

            var timer = _showHideDuration;
            while (timer >= 0f)
            {
                _canvasGroup.alpha = 1f - (timer / _showHideDuration);
                timer -= Time.unscaledDeltaTime;
                await UniTask.Yield();
            }

            _canvasGroup.alpha = 1f;
        }

        public async UniTask Hide()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.gameObject.SetActive(true);

            var timer = _showHideDuration;
            while (timer >= 0f)
            {
                _canvasGroup.alpha = timer / _showHideDuration;
                timer -= Time.unscaledDeltaTime;
                await UniTask.Yield();
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(false);
        }
    }
}

