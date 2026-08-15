using System.Threading;
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
        [SerializeField] private Image _backAccessories;
        [SerializeField] private Image _middleAccessories;
        [SerializeField] private Image _frontAccessories;

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

        public void SetBackAccessories(Sprite sprite)
        {
            _backAccessories.sprite = sprite;
            ClearImagesIfNeed();
        }

        public void SetMiddleAccessories(Sprite sprite)
        {
            _middleAccessories.sprite = sprite;
            ClearImagesIfNeed();
        }

        public void SetFrontAccessories(Sprite sprite)
        {
            _frontAccessories.sprite = sprite;
            ClearImagesIfNeed();
        }

        public void ShowImageImmediate(bool? isLeft)
        {
            ClearImagesIfNeed();

            _canvasGroup.gameObject.SetActive(true);

            var bodyOffset = 120f;
            if (isLeft.HasValue)
            {
                if (isLeft.Value)
                    bodyOffset = -bodyOffset;
            }
            else
            {
                bodyOffset = 0f;
            }
            _mainBody.transform.position = _canvasGroup.transform.position + Vector3.right * bodyOffset * _mainBody.canvas.scaleFactor;
            _clothes.transform.position = _canvasGroup.transform.position + Vector3.right * bodyOffset * _mainBody.canvas.scaleFactor;
            _emotion.transform.position = _canvasGroup.transform.position + Vector3.right * bodyOffset * _mainBody.canvas.scaleFactor;
            _backHairs.transform.position = _canvasGroup.transform.position + Vector3.right * bodyOffset * _mainBody.canvas.scaleFactor;
            _frontHairs.transform.position = _canvasGroup.transform.position + Vector3.right * bodyOffset * _mainBody.canvas.scaleFactor;
            _backAccessories.transform.position = _canvasGroup.transform.position + Vector3.right * bodyOffset * _mainBody.canvas.scaleFactor;
            _middleAccessories.transform.position = _canvasGroup.transform.position + Vector3.right * bodyOffset * _mainBody.canvas.scaleFactor;
            _frontAccessories.transform.position = _canvasGroup.transform.position + Vector3.right * bodyOffset * _mainBody.canvas.scaleFactor;

            var endPosition  = _canvasGroup.transform.localPosition;

            _canvasGroup.transform.localPosition = endPosition;
            _canvasGroup.alpha = 1f;
        }

        public async UniTask ShowImage(bool? isLeft, CancellationToken cancellationToken)
        {
            ClearImagesIfNeed();

            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(true);

            var bodyOffset = 120f;
            if (isLeft.HasValue)
            {
                if (isLeft.Value)
                    bodyOffset = -bodyOffset;
            }
            else
            {
                bodyOffset = 0f;
            }
            _mainBody.transform.position = _canvasGroup.transform.position + Vector3.right * bodyOffset * _mainBody.canvas.scaleFactor;
            _clothes.transform.position = _canvasGroup.transform.position + Vector3.right * bodyOffset * _mainBody.canvas.scaleFactor;
            _emotion.transform.position = _canvasGroup.transform.position + Vector3.right * bodyOffset * _mainBody.canvas.scaleFactor;
            _backHairs.transform.position = _canvasGroup.transform.position + Vector3.right * bodyOffset * _mainBody.canvas.scaleFactor;
            _frontHairs.transform.position = _canvasGroup.transform.position + Vector3.right * bodyOffset * _mainBody.canvas.scaleFactor;
            _backAccessories.transform.position = _canvasGroup.transform.position + Vector3.right * bodyOffset * _mainBody.canvas.scaleFactor;
            _middleAccessories.transform.position = _canvasGroup.transform.position + Vector3.right * bodyOffset * _mainBody.canvas.scaleFactor;
            _frontAccessories.transform.position = _canvasGroup.transform.position + Vector3.right * bodyOffset * _mainBody.canvas.scaleFactor;

            var startPosition = _canvasGroup.transform.localPosition + Vector3.right * (isLeft.HasValue ? isLeft.Value ? -100f : 100f : 0f);
            var endPosition  = _canvasGroup.transform.localPosition;

            _canvasGroup.transform.localPosition = startPosition;
            var timer = _showHideImageDuration;
            while (timer >= 0f)
            {
                _canvasGroup.transform.localPosition = Vector3.Lerp(startPosition, endPosition, 1f - (timer / _showHideImageDuration));
                _canvasGroup.alpha = 1f - (timer / _showHideImageDuration);
                timer -= Time.deltaTime;
                await UniTask.Yield(cancellationToken);
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

        public async UniTask HideImage(CancellationToken cancellationToken)
        {
            ClearImagesIfNeed();

            _canvasGroup.alpha = 1f;
            _canvasGroup.gameObject.SetActive(true);

            var timer = _showHideImageDuration;
            while (timer >= 0f)
            {
                _canvasGroup.alpha = timer / _showHideImageDuration;
                timer -= Time.deltaTime;
                await UniTask.Yield(cancellationToken);
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
            _backAccessories.color = _backAccessories.sprite == null ? Color.clear : Color.white;
            _middleAccessories.color = _middleAccessories.sprite == null ? Color.clear : Color.white;
            _frontAccessories.color = _frontAccessories.sprite == null ? Color.clear : Color.white;
        }
    }
}
