using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

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
            ToCenter,
            ToLeft,
            Shaking
        }

        [Serializable]
        private struct EffectImage
        {
            [SerializeField] private Effect _effect;
            [SerializeField] private GameObject _effectRoot;

            public readonly Effect Effect => _effect;
            public readonly GameObject EffectRoot => _effectRoot;
        }

        private const float _dialogOffset = 100f;

        [SerializeField] private float _showHideImageDuration;
        [SerializeField] private CanvasGroup _imageCanvasGroup;
        [SerializeField] private Image _image;

        [Space]
        [SerializeField] private VideoPlayer _video;
        [SerializeField] private RawImage _videoImage;

        [Space]
        [SerializeField] private EffectImage[] _effects;
        [SerializeField] private CanvasGroup _effectCanvasGroup;
        [SerializeField] private float _effectDuration;

        [Space]
        [SerializeField] private float _cameraDuration;
        [SerializeField] private float _dialogDuration;

        [SerializeField] private AnimationCurve _moveCurve;

        private Action _onVideoReady;
        private Action _onVideoDone;
        private Action _onVideoFailed;

        public void SetImage(Sprite sprite)
        {
            _image.sprite = sprite;

            var scaleFactor = _image.rectTransform.rect.height / _image.sprite.texture.height;
            var imageWidth = _image.sprite.texture.width * scaleFactor;
            _image.rectTransform.offsetMin = new Vector2(((UnityEngine.Screen.width / _image.canvas.scaleFactor) - imageWidth) / 2f, 0f);
            _image.rectTransform.offsetMax = new Vector2(-((UnityEngine.Screen.width / _image.canvas.scaleFactor) - imageWidth) / 2f, 0f);
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

            var timer = _showHideImageDuration;
            while (timer >= 0f)
            {
                _imageCanvasGroup.alpha = 1f - (timer / _showHideImageDuration);
                timer -= Time.deltaTime;
                await UniTask.Yield();
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

            var timer = _showHideImageDuration;
            while (timer >= 0f)
            {
                _imageCanvasGroup.alpha = timer / _showHideImageDuration;
                timer -= Time.deltaTime;
                await UniTask.Yield();
            }

            _imageCanvasGroup.alpha = 0f;
            _imageCanvasGroup.gameObject.SetActive(false);
        }

        public void SetEnabledImage(bool state)
        {
            _image.enabled = state;
        }

        public void SetVideo(string url, bool loop, RenderTexture rt, float playbackSpeed, Action onVideoReady, Action onVideoDone, Action onVideoFailed)
        {
            _video.GetComponentInChildren<RawImage>(true).texture = rt;
            _video.targetTexture = rt;
            _video.url = url;
            _video.isLooping = loop;
            _video.Play();
            _video.playbackSpeed = playbackSpeed;

            _onVideoReady = onVideoReady;
            _onVideoDone = onVideoDone;
            _onVideoFailed = onVideoFailed;
            _video.prepareCompleted += OnVideoReady;
            _video.loopPointReached += OnVideoDone;
            _video.errorReceived += OnVideoFailed;
        }

        private void OnVideoReady(VideoPlayer source)
        {
            _onVideoReady?.Invoke();
            _video.loopPointReached -= OnVideoReady;
        }

        private void OnVideoDone(VideoPlayer source)
        {
            _onVideoDone?.Invoke();
            _video.loopPointReached -= OnVideoDone;
        }

        private void OnVideoFailed(VideoPlayer source, string message)
        {
            _onVideoFailed?.Invoke();
            _video.errorReceived -= OnVideoFailed;
        }

        public void SetEnabledVideo(bool state)
        {
            _videoImage.enabled = state;
        }

        public void ResetCamera()
        {
            _image.transform.localPosition = new Vector3((UnityEngine.Screen.width / _image.canvas.scaleFactor) / 2f, 0f, 0f);
        }

        public async UniTask SetCamera(bool immediate, CameraEffect effect)
        {
            var scaleFactor = _image.rectTransform.rect.height / _image.sprite.texture.height;
            var spriteWidth = _image.sprite.texture.width * scaleFactor;
            var delta = (spriteWidth - (UnityEngine.Screen.width / _image.canvas.scaleFactor)) * 0.5f;
            delta -= _dialogOffset;
            
            var cameraCurrentPosition = _image.transform.localPosition;
            var cameraCenterPosition = new Vector3((UnityEngine.Screen.width / _image.canvas.scaleFactor) / 2f, 0f, 0f);
            var cameraLeftPosition = cameraCenterPosition + Vector3.right * delta;
            var cameraRightPosition = cameraCenterPosition + Vector3.left * delta;

            switch (effect)
            {
                case CameraEffect.LeftRight:
                    if (!immediate)
                    {
                        await Move(_image.transform, cameraCurrentPosition, cameraLeftPosition, _cameraDuration);
                        await Move(_image.transform, cameraLeftPosition, cameraRightPosition, _cameraDuration);
                    }
                    else
                    {
                        MoveImmediate(_image.transform, cameraRightPosition);
                    }
                    break;
                case CameraEffect.RightLeft:
                    if (!immediate)
                    {
                        await Move(_image.transform, cameraCurrentPosition, cameraRightPosition, _cameraDuration);
                        await Move(_image.transform, cameraRightPosition, cameraLeftPosition, _cameraDuration);
                    }
                    else
                    {
                        MoveImmediate(_image.transform, cameraLeftPosition);
                    }
                    break;
                case CameraEffect.ToCenter:
                    if (!immediate)
                    {
                        await Move(_image.transform, cameraCurrentPosition, cameraCenterPosition, _cameraDuration);
                    }
                    else
                    {
                        MoveImmediate(_image.transform, cameraCenterPosition);
                    }
                    break;
                case CameraEffect.ToLeft:
                    if (!immediate)
                    {
                        await Move(_image.transform, cameraCurrentPosition, cameraLeftPosition, _cameraDuration);
                    }
                    else
                    {
                        MoveImmediate(_image.transform, cameraLeftPosition);
                    }
                    break;
                case CameraEffect.Shaking:
                    if (!immediate)
                    {
                        await Move(_image.transform, cameraCurrentPosition, cameraLeftPosition, _cameraDuration / 10f);
                        await Move(_image.transform, cameraLeftPosition, cameraRightPosition, _cameraDuration / 10f);
                        await Move(_image.transform, cameraRightPosition, cameraLeftPosition, _cameraDuration / 10f);
                        await Move(_image.transform, cameraLeftPosition, cameraRightPosition, _cameraDuration / 10f);
                        await Move(_image.transform, cameraRightPosition, cameraLeftPosition, _cameraDuration / 10f);
                        await Move(_image.transform, cameraCurrentPosition, cameraCenterPosition, _cameraDuration / 10f);
                    }
                    else
                    {
                        MoveImmediate(_image.transform, cameraCenterPosition);
                    }
                    break;
            }
        }

        public void SetCameraImmediate(CameraEffect effect)
        {
            var scaleFactor = _image.rectTransform.rect.height / _image.sprite.texture.height;
            var spriteWidth = _image.sprite.texture.width * scaleFactor;
            var delta = (spriteWidth - (UnityEngine.Screen.width / _image.canvas.scaleFactor)) * 0.5f;
            delta -= _dialogOffset;
            
            var cameraCurrentPosition = _image.transform.localPosition;
            var cameraCenterPosition = new Vector3((UnityEngine.Screen.width / _image.canvas.scaleFactor) / 2f, 0f, 0f);
            var cameraLeftPosition = cameraCenterPosition + Vector3.right * delta;
            var cameraRightPosition = cameraCenterPosition + Vector3.left * delta;

            switch (effect)
            {
                case CameraEffect.LeftRight:
                    MoveImmediate(_image.transform, cameraRightPosition);
                    break;
                case CameraEffect.RightLeft:
                    MoveImmediate(_image.transform, cameraLeftPosition);
                    break;
                case CameraEffect.ToCenter:
                    MoveImmediate(_image.transform, cameraCenterPosition);
                    break;
                case CameraEffect.ToLeft:
                    MoveImmediate(_image.transform, cameraLeftPosition);
                    break;
                case CameraEffect.Shaking:
                    MoveImmediate(_image.transform, cameraCenterPosition);
                    break;
            }
        }

        public async UniTask SetDialogue(bool immediate, TextAlignment aligment)
        {
            if (_image.sprite == null) return;

            var delta = _dialogOffset;
            var cameraCurrentPosition = _image.transform.localPosition;
            var cameraCenterPosition = new Vector3((UnityEngine.Screen.width / _image.canvas.scaleFactor) / 2f, 0f, 0f);
            var cameraLeftPosition = cameraCenterPosition + Vector3.right * delta;
            var cameraRightPosition = cameraCenterPosition + Vector3.left * delta;

            var targetPosition = aligment switch
            {
                TextAlignment.Left => cameraLeftPosition,
                TextAlignment.Right => cameraRightPosition,
                _ => cameraCenterPosition,
            };
            if (!immediate)
                await Move(_image.transform, cameraCurrentPosition, targetPosition, _dialogDuration);
            else
                MoveImmediate(_image.transform, targetPosition);
        }

        public void SetDialogueImmediate(TextAlignment aligment)
        {
            if (_image.sprite == null) return;

            var delta = _dialogOffset;
            var cameraCurrentPosition = _image.transform.localPosition;
            var cameraCenterPosition = new Vector3((UnityEngine.Screen.width / _image.canvas.scaleFactor) / 2f, 0f, 0f);
            var cameraLeftPosition = cameraCenterPosition + Vector3.right * delta;
            var cameraRightPosition = cameraCenterPosition + Vector3.left * delta;

            var targetPosition = aligment switch
            {
                TextAlignment.Left => cameraLeftPosition,
                TextAlignment.Right => cameraRightPosition,
                _ => cameraCenterPosition,
            };
            MoveImmediate(_image.transform, targetPosition);
        }

        private async UniTask Move(Transform target, Vector3 from, Vector3 to, float duration)
        {
            if (from == to) return;
            target.localPosition = from;
            var timer = duration;
            while (timer >= 0f)
            {
                target.localPosition = Vector3.Lerp(from, to, _moveCurve.Evaluate(1f - (timer / duration)));
                timer -= Time.deltaTime;
                await UniTask.Yield();
            }
            target.localPosition = to;
        }

        private void MoveImmediate(Transform target, Vector3 to)
        {
            target.localPosition = to;
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

            _effectCanvasGroup.alpha = 0f;
            var timer = _effectDuration;
            while (timer >= 0f)
            {
                _effectCanvasGroup.alpha = 1f - (timer / _effectDuration);
                timer -= Time.deltaTime;
                await UniTask.Yield();
            }
            _effectCanvasGroup.alpha = 1f;
        }

        public void SetEffectImmediate(Effect effect)
        {
            foreach(var effectData in _effects)
                effectData.EffectRoot.SetActive(effectData.Effect == effect);

            _effectCanvasGroup.alpha = 1f;
        }
    }
}

