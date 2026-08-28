using System;
using UnityEngine;
using UnityEngine.UI;

namespace Novels.Location.View
{
    internal sealed class LocationLayout
    {
        internal readonly struct Positions
        {
            internal Positions(Vector3 current, Vector3 center, Vector3 left, Vector3 right)
            {
                Current = current;
                Center = center;
                Left = left;
                Right = right;
            }

            internal Vector3 Current { get; }
            internal Vector3 Center { get; }
            internal Vector3 Left { get; }
            internal Vector3 Right { get; }
        }

        private readonly Image _image;
        private bool _videoConfigured;

        internal LocationLayout(Image image)
        {
            _image = image ?? throw new ArgumentNullException(nameof(image));
        }

        internal void SetImage(Sprite sprite)
        {
            _image.sprite = sprite ?? throw new ArgumentNullException(nameof(sprite));
            SetVisualSize(sprite.texture.width, sprite.texture.height);
        }

        internal void ClearImage()
        {
            _image.sprite = null;
        }

        internal void SetVideoTexture(Texture texture)
        {
            _videoConfigured = texture != null;
            if (texture != null)
                SetVisualSize(texture.width, texture.height);
        }

        internal bool HasVisual => _image.sprite != null || _videoConfigured;

        internal Vector3 Center => new(ScreenWidth / 2f, 0f, 0f);

        internal Positions CameraPositions(float edgeInset)
        {
            EnsureVisualConfigured();
            var delta = Mathf.Max(0f, AvailableHorizontalTravel - edgeInset);
            return PositionsForDelta(delta);
        }

        internal Vector3 DialoguePosition(TextAlignment alignment, float offset)
        {
            EnsureVisualConfigured();
            var positions = PositionsForDelta(
                Mathf.Min(offset, AvailableHorizontalTravel));
            return alignment switch
            {
                TextAlignment.Left => positions.Left,
                TextAlignment.Right => positions.Right,
                _ => positions.Center,
            };
        }

        private void SetVisualSize(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));
            var scaleFactor = _image.rectTransform.rect.height / height;
            var visualWidth = width * scaleFactor;
            var offset = (ScreenWidth - visualWidth) / 2f;
            _image.rectTransform.offsetMin = new Vector2(offset, 0f);
            _image.rectTransform.offsetMax = new Vector2(-offset, 0f);
        }

        private void EnsureVisualConfigured()
        {
            if (!HasVisual)
                throw new InvalidOperationException("Location visual is not configured.");
        }

        private Positions PositionsForDelta(float delta)
        {
            var center = Center;
            return new Positions(
                _image.transform.localPosition,
                center,
                center + Vector3.right * delta,
                center + Vector3.left * delta);
        }

        private float ScreenWidth =>
            UnityEngine.Screen.width / _image.canvas.scaleFactor;

        private float AvailableHorizontalTravel
        {
            get => Mathf.Max(
                0f,
                (_image.rectTransform.rect.width - ScreenWidth) * 0.5f);
        }
    }
}
