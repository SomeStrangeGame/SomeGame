using UnityEngine;
using UnityEngine.UI;

namespace Novels.Bubble.View
{
    internal sealed class ChoiceButtonIcon : MonoBehaviour
    {
        private const float IconSize = 64f;
        private const float HorizontalPadding = 12f;
        private const float IllustratedButtonHeight = 88f;

        private Image _image;
        private Image _background;
        private Button _button;
        private Graphic _originalTargetGraphic;
        private Color _backgroundColor;
        private Text _text;
        private Vector2 _textOffsetMin;
        private Vector2 _buttonSize;
        private Color _textColor;

        internal static ChoiceButtonIcon Create(Button button, Text text)
        {
            var parent = button.transform;
            var originalTargetGraphic = button.targetGraphic;
            var background = originalTargetGraphic as Image
                ?? button.GetComponent<Image>();

            var root = new GameObject(
                "ChoiceIcon",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(ChoiceButtonIcon));
            root.transform.SetParent(parent, false);
            root.transform.SetAsLastSibling();
            text.transform.SetAsLastSibling();

            var rect = (RectTransform)root.transform;
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(HorizontalPadding, 0f);
            rect.sizeDelta = new Vector2(IconSize, IconSize);

            var view = root.GetComponent<ChoiceButtonIcon>();
            view._image = root.GetComponent<Image>();
            view._background = background;
            view._button = button;
            view._originalTargetGraphic = originalTargetGraphic;
            view._backgroundColor = background == null
                ? Color.white
                : background.color;
            view._image.preserveAspect = true;
            view._image.raycastTarget = false;
            view._text = text;
            view._textOffsetMin = text.rectTransform.offsetMin;
            view._buttonSize = ((RectTransform)parent).sizeDelta;
            view._textColor = text.color;
            return view;
        }

        internal void SetSprite(Sprite sprite)
        {
            _image ??= GetComponent<Image>();
            if (_text == null)
                _text = transform.parent.GetComponentInChildren<Text>(true);

            _image.sprite = sprite;
            gameObject.SetActive(sprite != null);
            if (_background != null)
                _background.color = sprite != null
                    ? new Color(1f, 0.88f, 0.55f, 0.98f)
                    : _backgroundColor;
            if (_button != null)
                _button.targetGraphic = _originalTargetGraphic;
            if (_text == null)
                return;

            var offset = _textOffsetMin;
            if (sprite != null)
                offset.x += IconSize + HorizontalPadding * 2f;
            _text.rectTransform.offsetMin = offset;
            _text.color = sprite != null
                ? new Color(0.18f, 0.2f, 0.23f, 1f)
                : _textColor;

            if (transform.parent is RectTransform buttonRect)
            {
                var size = _buttonSize;
                if (sprite != null)
                    size.y = IllustratedButtonHeight;
                buttonRect.sizeDelta = size;
            }
        }
    }
}
