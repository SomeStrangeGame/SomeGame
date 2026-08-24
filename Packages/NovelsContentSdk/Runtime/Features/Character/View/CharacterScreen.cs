using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Novels.Character.View
{
    public class CharacterScreen : MonoBehaviour
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

        private readonly Dictionary<Image, ImageLayout> _imageLayouts = new();

        private void Awake()
        {
            RememberLayout(_mainBody);
            RememberLayout(_clothes);
            RememberLayout(_emotion);
            RememberLayout(_backHairs);
            RememberLayout(_frontHairs);
            RememberLayout(_backAccessories);
            RememberLayout(_middleAccessories);
            RememberLayout(_frontAccessories);
        }

        public void SetMainBody(Sprite sprite)
        {
            SetSprite(_mainBody, sprite, default);
        }

        internal void SetMainBody(Sprite sprite, CharacterSpriteTrimLayout layout) =>
            SetSprite(_mainBody, sprite, layout);

        public void SetEmotion(Sprite sprite)
        {
            SetSprite(_emotion, sprite, default);
        }

        internal void SetEmotion(Sprite sprite, CharacterSpriteTrimLayout layout) =>
            SetSprite(_emotion, sprite, layout);

        public void SetClothes(Sprite sprite)
        {
            SetSprite(_clothes, sprite, default);
        }

        internal void SetClothes(Sprite sprite, CharacterSpriteTrimLayout layout) =>
            SetSprite(_clothes, sprite, layout);

        public void SetBackHairs(Sprite sprite)
        {
            SetSprite(_backHairs, sprite, default);
        }

        internal void SetBackHairs(Sprite sprite, CharacterSpriteTrimLayout layout) =>
            SetSprite(_backHairs, sprite, layout);

        public void SetFrontHairs(Sprite sprite)
        {
            SetSprite(_frontHairs, sprite, default);
        }

        internal void SetFrontHairs(Sprite sprite, CharacterSpriteTrimLayout layout) =>
            SetSprite(_frontHairs, sprite, layout);

        public void SetBackAccessories(Sprite sprite)
        {
            SetSprite(_backAccessories, sprite, default);
        }

        internal void SetBackAccessories(Sprite sprite, CharacterSpriteTrimLayout layout) =>
            SetSprite(_backAccessories, sprite, layout);

        public void SetMiddleAccessories(Sprite sprite)
        {
            SetSprite(_middleAccessories, sprite, default);
        }

        internal void SetMiddleAccessories(Sprite sprite, CharacterSpriteTrimLayout layout) =>
            SetSprite(_middleAccessories, sprite, layout);

        public void SetFrontAccessories(Sprite sprite)
        {
            SetSprite(_frontAccessories, sprite, default);
        }

        internal void SetFrontAccessories(Sprite sprite, CharacterSpriteTrimLayout layout) =>
            SetSprite(_frontAccessories, sprite, layout);

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
            await global::UITransitions.Transition.FadeAndMove(
                _canvasGroup,
                startPosition,
                endPosition,
                _showHideImageDuration,
                cancellationToken);
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

            if (!_canvasGroup.gameObject.activeSelf || _canvasGroup.alpha <= 0f)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.gameObject.SetActive(false);
                return;
            }

            await global::UITransitions.Transition.Fade(
                _canvasGroup,
                _canvasGroup.alpha,
                0f,
                _showHideImageDuration,
                cancellationToken);
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

        private void RememberLayout(Image image)
        {
            if (image != null)
                _imageLayouts[image] = new ImageLayout(image.rectTransform);
        }

        private void SetSprite(
            Image image,
            Sprite sprite,
            CharacterSpriteTrimLayout trimLayout)
        {
            image.sprite = sprite;
            ApplyLayout(image, trimLayout);
            ClearImagesIfNeed();
        }

        private void ApplyLayout(Image image, CharacterSpriteTrimLayout trimLayout)
        {
            if (!_imageLayouts.TryGetValue(image, out var baseline))
                return;

            var rectTransform = image.rectTransform;
            var worldPosition = rectTransform.position;
            baseline.Restore(rectTransform);
            if (!trimLayout.IsValid)
            {
                rectTransform.position = worldPosition;
                return;
            }

            var available = rectTransform.rect.size;
            var scaleX = available.x / trimLayout.OriginalWidth;
            var scaleY = available.y / trimLayout.OriginalHeight;
            if (image.preserveAspect)
                scaleX = scaleY = Mathf.Min(scaleX, scaleY);

            var crop = trimLayout.Crop;
            rectTransform.anchorMin = baseline.AnchorCenter;
            rectTransform.anchorMax = baseline.AnchorCenter;
            rectTransform.pivot = new Vector2(
                (trimLayout.OriginalWidth * baseline.Pivot.x - crop.x) / crop.width,
                (trimLayout.OriginalHeight * baseline.Pivot.y - crop.y) / crop.height);
            rectTransform.sizeDelta = new Vector2(crop.width * scaleX, crop.height * scaleY);
            rectTransform.position = worldPosition;
        }

        private readonly struct ImageLayout
        {
            internal readonly Vector2 AnchorMin;
            internal readonly Vector2 AnchorMax;
            internal readonly Vector2 SizeDelta;
            internal readonly Vector2 Pivot;

            internal ImageLayout(RectTransform value)
            {
                AnchorMin = value.anchorMin;
                AnchorMax = value.anchorMax;
                SizeDelta = value.sizeDelta;
                Pivot = value.pivot;
            }

            internal Vector2 AnchorCenter => (AnchorMin + AnchorMax) * 0.5f;

            internal void Restore(RectTransform value)
            {
                value.anchorMin = AnchorMin;
                value.anchorMax = AnchorMax;
                value.sizeDelta = SizeDelta;
                value.pivot = Pivot;
            }
        }
    }
}
