using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Novels.Location.View
{
    public class LocationScreen : MonoBehaviour
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
            Shaking,
            ToRight
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
        private CanvasGroup _videoCanvasGroup;

        [Space]
        [SerializeField] private EffectImage[] _effects;
        [SerializeField] private CanvasGroup _effectCanvasGroup;
        [SerializeField] private float _effectDuration;

        [Space]
        [SerializeField] private float _cameraDuration;
        [SerializeField] private float _dialogDuration;

        [SerializeField] private AnimationCurve _moveCurve;
        private LocationLayout _layout;

        public VideoPlayer VideoPlayer => _video;
        private LocationLayout Layout => _layout ??= new LocationLayout(_image);

        public void SetImage(Sprite sprite)
        {
            Layout.SetImage(sprite);
        }

        public async UniTask CrossfadeImage(
            Sprite sprite,
            CancellationToken cancellationToken)
        {
            if (sprite == null)
                throw new ArgumentNullException(nameof(sprite));
            if (_image.sprite == null || !_image.enabled || !_image.gameObject.activeInHierarchy)
            {
                SetImage(sprite);
                return;
            }

            var previous = Instantiate(_image, _image.transform.parent);
            previous.name = $"{_image.name} (Quality Crossfade)";
            previous.raycastTarget = false;
            previous.transform.SetSiblingIndex(_image.transform.GetSiblingIndex() + 1);
            var fade = previous.GetComponent<CanvasGroup>()
                ?? previous.gameObject.AddComponent<CanvasGroup>();
            SetImage(sprite);
            try
            {
                await global::UITransitions.Transition.Fade(
                    fade,
                    1f,
                    0f,
                    _showHideImageDuration,
                    cancellationToken);
            }
            finally
            {
                if (previous != null)
                    Destroy(previous.gameObject);
            }
        }

        public void ShowImageImmediate()
        {
            _imageCanvasGroup.alpha = 1f;
            _imageCanvasGroup.gameObject.SetActive(true);
        }

        public async UniTask ShowImage(CancellationToken cancellationToken)
        {
            _image.color = _image.sprite == null ? Color.clear : Color.white;
            _imageCanvasGroup.alpha = 0f;
            _imageCanvasGroup.gameObject.SetActive(true);

            await global::UITransitions.Transition.Fade(
                _imageCanvasGroup,
                0f,
                1f,
                _showHideImageDuration,
                cancellationToken);
        }

        public void HideImageImmediate()
        {
            _imageCanvasGroup.alpha = 0f;
            _imageCanvasGroup.gameObject.SetActive(false);
        }

        public async UniTask HideImage(CancellationToken cancellationToken)
        {
            _image.color = _image.sprite == null ? Color.clear : Color.white;
            _imageCanvasGroup.alpha = 1f;
            _imageCanvasGroup.gameObject.SetActive(true);

            await global::UITransitions.Transition.Fade(
                _imageCanvasGroup,
                1f,
                0f,
                _showHideImageDuration,
                cancellationToken);
            _imageCanvasGroup.gameObject.SetActive(false);
        }

        public void SetEnabledImage(bool state)
        {
            _image.enabled = state;
        }

        public void SetVideoTexture(RenderTexture renderTexture)
        {
            _videoImage.texture = renderTexture;
            _video.targetTexture = renderTexture;
        }

        public void SetEnabledVideo(bool state)
        {
            _videoImage.enabled = state;
            if (!state && _videoCanvasGroup != null)
                _videoCanvasGroup.alpha = 0f;
        }

        public async UniTask CrossfadeToVideo(CancellationToken cancellationToken)
        {
            _videoCanvasGroup ??= _videoImage.GetComponent<CanvasGroup>()
                ?? _videoImage.gameObject.AddComponent<CanvasGroup>();
            _videoCanvasGroup.alpha = 0f;
            _videoImage.enabled = true;
            await global::UITransitions.Transition.Fade(
                _videoCanvasGroup,
                0f,
                1f,
                _showHideImageDuration,
                cancellationToken);
            _image.enabled = false;
        }

        public void ResetCamera()
        {
            _image.transform.localPosition = Layout.Center;
        }

        public async UniTask SetCamera(CameraEffect effect, CancellationToken cancellationToken)
        {
            var positions = Layout.CameraPositions(_dialogOffset);

            switch (effect)
            {
                case CameraEffect.LeftRight:
                    await Move(_image.transform, positions.Current, positions.Left, _cameraDuration, cancellationToken);
                    await Move(_image.transform, positions.Left, positions.Right, _cameraDuration, cancellationToken);
                    break;
                case CameraEffect.RightLeft:
                    await Move(_image.transform, positions.Current, positions.Right, _cameraDuration, cancellationToken);
                    await Move(_image.transform, positions.Right, positions.Left, _cameraDuration, cancellationToken);
                    break;
                case CameraEffect.ToCenter:
                    await Move(_image.transform, positions.Current, positions.Center, _cameraDuration, cancellationToken);
                    break;
                case CameraEffect.ToLeft:
                    await Move(_image.transform, positions.Current, positions.Left, _cameraDuration, cancellationToken);
                    break;
                case CameraEffect.ToRight:
                    await Move(_image.transform, positions.Current, positions.Right, _cameraDuration, cancellationToken);
                    break;
                case CameraEffect.Shaking:
                    await Move(_image.transform, positions.Current, positions.Left, _cameraDuration / 10f, cancellationToken);
                    await Move(_image.transform, positions.Left, positions.Right, _cameraDuration / 10f, cancellationToken);
                    await Move(_image.transform, positions.Right, positions.Left, _cameraDuration / 10f, cancellationToken);
                    await Move(_image.transform, positions.Left, positions.Right, _cameraDuration / 10f, cancellationToken);
                    await Move(_image.transform, positions.Right, positions.Left, _cameraDuration / 10f, cancellationToken);
                    await Move(_image.transform, positions.Current, positions.Center, _cameraDuration / 10f, cancellationToken);
                    break;
            }
        }

        public void SetCameraImmediate(CameraEffect effect)
        {
            var positions = Layout.CameraPositions(_dialogOffset);

            switch (effect)
            {
                case CameraEffect.LeftRight:
                    MoveImmediate(_image.transform, positions.Right);
                    break;
                case CameraEffect.RightLeft:
                    MoveImmediate(_image.transform, positions.Left);
                    break;
                case CameraEffect.ToCenter:
                    MoveImmediate(_image.transform, positions.Center);
                    break;
                case CameraEffect.ToLeft:
                    MoveImmediate(_image.transform, positions.Left);
                    break;
                case CameraEffect.ToRight:
                    MoveImmediate(_image.transform, positions.Right);
                    break;
                case CameraEffect.Shaking:
                    MoveImmediate(_image.transform, positions.Center);
                    break;
            }
        }

        public async UniTask SetDialogue(TextAlignment aligment, CancellationToken cancellationToken)
        {
            if (_image.sprite == null) return;

            var current = _image.transform.localPosition;
            var target = Layout.DialoguePosition(aligment, _dialogOffset);
            await Move(_image.transform, current, target, _dialogDuration, cancellationToken);
        }

        public void SetDialogueImmediate(TextAlignment aligment)
        {
            if (_image.sprite == null) return;

            MoveImmediate(
                _image.transform,
                Layout.DialoguePosition(aligment, _dialogOffset));
        }

        private UniTask Move(
            Transform target,
            Vector3 from,
            Vector3 to,
            float duration,
            CancellationToken cancellationToken) =>
            global::UITransitions.Transition.Move(
                target,
                from,
                to,
                duration,
                _moveCurve,
                cancellationToken);

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

        public async UniTask SetEffect(Effect effect, CancellationToken cancellationToken)
        {
            ActivateEffect(effect);
            await global::UITransitions.Transition.Fade(
                _effectCanvasGroup,
                0f,
                1f,
                _effectDuration,
                cancellationToken);
        }

        public async UniTask FlashEffect(
            Effect effect,
            CancellationToken cancellationToken)
        {
            ActivateEffect(effect);
            var halfDuration = _effectDuration / 2f;
            await global::UITransitions.Transition.Fade(
                _effectCanvasGroup,
                0f,
                1f,
                halfDuration,
                cancellationToken);
            await global::UITransitions.Transition.Fade(
                _effectCanvasGroup,
                1f,
                0f,
                halfDuration,
                cancellationToken);
            ResetEffect();
        }

        public void SetEffectImmediate(Effect effect)
        {
            ActivateEffect(effect);
            _effectCanvasGroup.alpha = 1f;
        }

        private void ActivateEffect(Effect effect)
        {
            foreach(var effectData in _effects)
                effectData.EffectRoot.SetActive(effectData.Effect == effect);
        }
    }
}
