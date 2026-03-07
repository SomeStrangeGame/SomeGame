using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Novels.Character.View
{
    public class Screen : MonoBehaviour
    {
        [SerializeField] private float _showHideImageDuration;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _mainBody;
        [SerializeField] private Image _emotion;

        public void SetMainBody(Sprite sprite)
        {
            _mainBody.sprite = sprite;
        }

        public void SetEmotion(Sprite sprite)
        {
            _emotion.sprite = sprite;
        }

        public void ShowImageImmediate()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.gameObject.SetActive(true);
        }

        public async UniTask ShowImage()
        {
            ClearImagesIfNeed();

            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(true);

            var timer = _showHideImageDuration;
            while (timer >= 0f)
            {
                _canvasGroup.alpha = 1f - (timer / _showHideImageDuration);
                timer -= Time.deltaTime;
                await UniTask.Yield();
            }

            _canvasGroup.alpha = 1f;
        }

        public void HideImageImmediate()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(false);
        }

        public async UniTask HideImage()
        {
            ClearImagesIfNeed();

            _canvasGroup.alpha = 1f;
            _canvasGroup.gameObject.SetActive(true);

            var timer = _showHideImageDuration;
            while (timer >= 0f)
            {
                _canvasGroup.alpha = timer / _showHideImageDuration;
                timer -= Time.deltaTime;
                await UniTask.Yield();
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(false);
        }

        private void ClearImagesIfNeed()
        {
            _mainBody.color = _mainBody.sprite == null ? Color.clear : Color.white;
            _emotion.color = _emotion.sprite == null ? Color.clear : Color.white;
        }
    }
}

