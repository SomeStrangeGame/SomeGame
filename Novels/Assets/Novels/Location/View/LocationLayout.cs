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

        internal LocationLayout(Image image)
        {
            _image = image ?? throw new ArgumentNullException(nameof(image));
        }

        internal void SetImage(Sprite sprite)
        {
            _image.sprite = sprite ?? throw new ArgumentNullException(nameof(sprite));
            var scaleFactor = _image.rectTransform.rect.height / sprite.texture.height;
            var imageWidth = sprite.texture.width * scaleFactor;
            var offset = (ScreenWidth - imageWidth) / 2f;
            _image.rectTransform.offsetMin = new Vector2(offset, 0f);
            _image.rectTransform.offsetMax = new Vector2(-offset, 0f);
        }

        internal Vector3 Center => new(ScreenWidth / 2f, 0f, 0f);

        internal Positions CameraPositions(float edgeInset)
        {
            if (_image.sprite == null)
                throw new InvalidOperationException("Location image is not configured.");
            var scaleFactor = _image.rectTransform.rect.height
                / _image.sprite.texture.height;
            var spriteWidth = _image.sprite.texture.width * scaleFactor;
            var delta = (spriteWidth - ScreenWidth) * 0.5f - edgeInset;
            return PositionsForDelta(delta);
        }

        internal Vector3 DialoguePosition(TextAlignment alignment, float offset)
        {
            var positions = PositionsForDelta(offset);
            return alignment switch
            {
                TextAlignment.Left => positions.Left,
                TextAlignment.Right => positions.Right,
                _ => positions.Center,
            };
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
    }
}
