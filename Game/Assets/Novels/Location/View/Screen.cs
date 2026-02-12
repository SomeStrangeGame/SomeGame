using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Novels.Location.View
{
    public class Screen : MonoBehaviour
    {
        public enum Effect
        {
            Light,
            Dark,
        }

        public enum CameraEffect
        {
            LeftRight,
            RightLeft,
        }

        [Serializable]
        private struct EffectImage
        {
            [SerializeField] private Effect _effect;
            [SerializeField] private GameObject _effectRoot;

            public readonly Effect Effect => _effect;
            public readonly GameObject EffectRoot => _effectRoot;
        }

        [SerializeField] private float _showHideImageDuration;
        [SerializeField] private CanvasGroup _imageCanvasGroup;
        [SerializeField] private Image _image;

        [SerializeField] private EffectImage[] _effects;
        [SerializeField] private CanvasGroup _effectCanvasGroup;
        [SerializeField] private float _effectDuration;

        [SerializeField] private float _cameraDuration;

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

        public void ResetCamera()
        {
            _image.transform.localPosition = new Vector3(UnityEngine.Screen.width / 2f, 0f, 0f);
        }

        public async UniTask SetCamera(CameraEffect effect)
        {
            var scaleFactor = _image.rectTransform.rect.height / _image.sprite.texture.height;
            var spriteWidth = _image.sprite.texture.width * scaleFactor;
            var delta = (spriteWidth - UnityEngine.Screen.width) * 0.5f;
            
            var cameraCurrentPosition = _image.transform.localPosition;
            var cameraCenterPosition = new Vector3(UnityEngine.Screen.width / 2f, 0f, 0f);
            var cameraLeftPosition = cameraCenterPosition + Vector3.right * delta;
            var cameraRightPosition = cameraCenterPosition + Vector3.left * delta;

            switch (effect)
            {
                case CameraEffect.LeftRight:
                    await Move(_image.transform, cameraCurrentPosition, cameraLeftPosition, 1f);
                    await Move(_image.transform, cameraLeftPosition, cameraRightPosition, _cameraDuration);
                    break;
                case CameraEffect.RightLeft:
                    await Move(_image.transform, cameraCurrentPosition, cameraRightPosition, 1f);
                    await Move(_image.transform, cameraRightPosition, cameraLeftPosition, _cameraDuration);
                    break;
            }

            async UniTask Move(Transform target, Vector3 from, Vector3 to, float duration)
            {
                var delayMs = 50;
                var deltaTime = delayMs / 1000f;

                target.localPosition = from;
                var timer = duration;
                while (timer >= 0f)
                {
                    target.localPosition = Vector3.Lerp(from, to, 1f - (timer / duration));
                    timer -= deltaTime;
                    await UniTask.Delay(delayMs, true);
                }
                target.localPosition = to;
            }
        }

        public void ResetEffect()
        {
            _effectCanvasGroup.alpha = 0f;
            foreach(var effectData in _effects)
                effectData.EffectRoot.SetActive(false);
        }

        public async UniTask SetEffect(Effect effect)
        {
            foreach(var effectData in _effects)
                effectData.EffectRoot.SetActive(effectData.Effect == effect);

            var delayMs = 50;
            var deltaTime = delayMs / 1000f;

            _effectCanvasGroup.alpha = 0f;
            var timer = _effectDuration;
            while (timer >= 0f)
            {
                _effectCanvasGroup.alpha = 1f - (timer / _effectDuration);
                timer -= deltaTime;
                await UniTask.Delay(delayMs, true);
            }
            _effectCanvasGroup.alpha = 1f;
        }
    }
}

