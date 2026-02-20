using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Novels.Character.View
{
    public class Screen : MonoBehaviour
    {
        [SerializeField] private float _showHideImageDuration;
        [SerializeField] private CanvasGroup _imageCanvasGroup;
        [SerializeField] private Image _image;

        public void SetImage(Sprite sprite)
        {
            _image.sprite = sprite;
        }

        public void ShowImageImmediate()
        {
            _imageCanvasGroup.alpha = 1f;
            _imageCanvasGroup.gameObject.SetActive(true);
        }

        public async UniTask ShowImage()
        {
            _image.color = _image.sprite == null ? Color.clear : Color.white;
            _imageCanvasGroup.alpha = 0f;
            _imageCanvasGroup.gameObject.SetActive(true);

            var delayMs = 50;
            var deltaTime = delayMs / 1000f;

            var timer = _showHideImageDuration;
            while (timer >= 0f)
            {
                _imageCanvasGroup.alpha = 1f - (timer / _showHideImageDuration);
                timer -= deltaTime;
                await UniTask.Delay(delayMs, true);
            }

            _imageCanvasGroup.alpha = 1f;
        }

        public void HideImageImmediate()
        {
            _imageCanvasGroup.alpha = 0f;
            _imageCanvasGroup.gameObject.SetActive(false);
        }

        public async UniTask HideImage()
        {
            _image.color = _image.sprite == null ? Color.clear : Color.white;
            _imageCanvasGroup.alpha = 1f;
            _imageCanvasGroup.gameObject.SetActive(true);

            var delayMs = 50;
            var deltaTime = delayMs / 1000f;

            var timer = _showHideImageDuration;
            while (timer >= 0f)
            {
                _imageCanvasGroup.alpha = timer / _showHideImageDuration;
                timer -= deltaTime;
                await UniTask.Delay(delayMs, true);
            }

            _imageCanvasGroup.alpha = 0f;
            _imageCanvasGroup.gameObject.SetActive(false);
        }
    }
}

