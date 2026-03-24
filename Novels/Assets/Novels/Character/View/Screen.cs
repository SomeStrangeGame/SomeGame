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
        [SerializeField] private Image _clothes;
        [SerializeField] private Image _emotion;
        [SerializeField] private Image _backHairs;
        [SerializeField] private Image _frontHairs;

        public void SetMainBody(Sprite sprite)
        {
            _mainBody.sprite = sprite;
            ClearImagesIfNeed();
        }

        public void SetEmotion(Sprite sprite)
        {
            _emotion.sprite = sprite;
            ClearImagesIfNeed();
        }

        public void SetClothes(Sprite sprite)
        {
            _clothes.sprite = sprite;
            ClearImagesIfNeed();
        }

        public void SetBackHairs(Sprite sprite)
        {
            _backHairs.sprite = sprite;
            ClearImagesIfNeed();
        }

        public void SetFrontHairs(Sprite sprite)
        {
            _frontHairs.sprite = sprite;
            ClearImagesIfNeed();
        }

        public void ShowImageImmediate()
        {
            ClearImagesIfNeed();

            _canvasGroup.alpha = 1f;
            _canvasGroup.gameObject.SetActive(true);
        }

        public async UniTask ShowImage(bool isLeft)
        {
            ClearImagesIfNeed();

            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(true);

            var bodyOffset = 120f;
            _mainBody.transform.position = _canvasGroup.transform.position + Vector3.right * (isLeft ? -bodyOffset : bodyOffset) * _mainBody.canvas.scaleFactor;
            _clothes.transform.position = _canvasGroup.transform.position + Vector3.right * (isLeft ? -bodyOffset : bodyOffset) * _mainBody.canvas.scaleFactor;
            _emotion.transform.position = _canvasGroup.transform.position + Vector3.right * (isLeft ? -bodyOffset : bodyOffset) * _mainBody.canvas.scaleFactor;
            _backHairs.transform.position = _canvasGroup.transform.position + Vector3.right * (isLeft ? -bodyOffset : bodyOffset) * _mainBody.canvas.scaleFactor;
            _frontHairs.transform.position = _canvasGroup.transform.position + Vector3.right * (isLeft ? -bodyOffset : bodyOffset) * _mainBody.canvas.scaleFactor;

            var startPosition = _canvasGroup.transform.localPosition + Vector3.right * (isLeft ? -100f : 100f);
            var endPosition  = _canvasGroup.transform.localPosition;

            _canvasGroup.transform.localPosition = startPosition;
            var timer = _showHideImageDuration;
            while (timer >= 0f)
            {
                _canvasGroup.transform.localPosition = Vector3.Lerp(startPosition, endPosition, 1f - (timer / _showHideImageDuration));
                _canvasGroup.alpha = 1f - (timer / _showHideImageDuration);
                timer -= Time.deltaTime;
                await UniTask.Yield();
            }
            _canvasGroup.transform.localPosition = endPosition;
            _canvasGroup.alpha = 1f;
        }

        public void HideImageImmediate()
        {
            ClearImagesIfNeed();
            
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
            _clothes.color = _clothes.sprite == null ? Color.clear : Color.white;
            _frontHairs.color = _frontHairs.sprite == null ? Color.clear : Color.white;
            _backHairs.color = _backHairs.sprite == null ? Color.clear : Color.white;
        }
    }
}

