using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Novels.OptionSelection
{
    public sealed class OptionListScreen : MonoBehaviour
    {
        private static readonly Color PanelColor = new(0.12f, 0.14f, 0.17f, 0.96f);
        private static readonly Color CardColor = new(0.24f, 0.27f, 0.32f, 0.96f);
        private static readonly Color SelectedColor = new(0.20f, 0.55f, 0.78f, 1f);

        private readonly List<Image> _cardBackgrounds = new();
        private readonly List<RectTransform> _cardRects = new();
        private readonly List<Image> _cardThumbnails = new();
        private readonly List<int> _cardItemIndices = new();
        private CanvasGroup _canvasGroup;
        private RectTransform _content;
        private RectTransform _viewport;
        private ScrollRect _scroll;
        private Text _title;
        private Text _selection;
        private Button _confirm;
        private Text _confirmLabel;
        private OptionListPresentation _presentation;
        private int _selectedIndex = -1;
        private int _presentationVersion;
        private int _initialSlot;
        private bool _needsCentering;

        public void Init()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 150;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            gameObject.AddComponent<GraphicRaycaster>();
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            var panel = CreateImage("Panel", transform, PanelColor);
            SetRect(panel.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(24f, 28f), new Vector2(-24f, 560f));

            _title = CreateText("Title", panel.transform, 42, TextAnchor.MiddleCenter);
            SetRect(_title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(24f, -82f), new Vector2(-24f, -20f));

            var viewport = CreateImage("Viewport", panel.transform, new Color(0f, 0f, 0f, 0.22f));
            _viewport = viewport.rectTransform;
            SetRect(_viewport, new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(80f, 170f), new Vector2(-80f, -96f));
            viewport.gameObject.AddComponent<RectMask2D>();

            var contentObject = new GameObject("Content", typeof(RectTransform));
            _content = contentObject.GetComponent<RectTransform>();
            _content.SetParent(viewport.transform, false);
            _content.anchorMin = new Vector2(0f, 0f);
            _content.anchorMax = new Vector2(0f, 1f);
            _content.pivot = new Vector2(0f, 0.5f);
            _content.anchoredPosition = Vector2.zero;
            var layout = contentObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            var fitter = contentObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            _scroll = viewport.gameObject.AddComponent<ScrollRect>();
            _scroll.viewport = _viewport;
            _scroll.content = _content;
            _scroll.horizontal = true;
            _scroll.vertical = false;
            _scroll.movementType = ScrollRect.MovementType.Unrestricted;
            _scroll.scrollSensitivity = 40f;
            _scroll.onValueChanged.AddListener(_ => SelectClosestCard());

            _selection = CreateText("Selection", panel.transform, 34, TextAnchor.MiddleCenter);
            SetRect(_selection.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(40f, 104f), new Vector2(-40f, 164f));

            _confirm = CreateButton("Confirm", panel.transform, SelectedColor, out _confirmLabel);
            SetRect(_confirm.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-260f, 28f), new Vector2(260f, 100f));
            _confirm.onClick.AddListener(Confirm);
            HideImmediate();
        }

        public void SetPresentation(OptionListPresentation presentation)
        {
            _presentation = presentation
                ?? throw new ArgumentNullException(nameof(presentation));
            _presentationVersion++;
            ClearCards();
            _title.text = presentation.Title;
            _confirmLabel.text = presentation.ConfirmationText;
            _confirm.interactable = presentation.Items.Length > 0;

            var copies = presentation.Items.Length > 1 ? 3 : 1;
            for (var copy = 0; copy < copies; copy++)
            {
                for (var index = 0; index < presentation.Items.Length; index++)
                    CreateCard(index, presentation.Items[index]);
            }
            for (var index = 0; index < presentation.Items.Length; index++)
                LoadThumbnail(index, presentation.Items[index].Id, _presentationVersion).Forget();

            if (presentation.Items.Length == 0)
                return;
            _initialSlot = copies == 1 ? 0 : presentation.Items.Length;
            _needsCentering = true;
            SelectItem(0);
        }

        public void ShowImmediate()
        {
            gameObject.SetActive(true);
            if (_needsCentering)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
                Canvas.ForceUpdateCanvases();
                CenterCard(_initialSlot);
                _needsCentering = false;
            }
            _canvasGroup.alpha = 1f;
        }

        public void HideImmediate()
        {
            if (_canvasGroup == null)
                return;
            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private void CreateCard(int itemIndex, OptionListItem item)
        {
            var card = CreateButton($"Option_{item.Id}", _content, CardColor, out var label);
            var rect = card.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(240f, 270f);
            var layout = card.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = 240f;
            layout.preferredHeight = 270f;
            label.text = item.Text;
            label.fontSize = 28;
            label.alignment = TextAnchor.LowerCenter;
            SetRect(label.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(12f, 12f), new Vector2(-12f, -190f));

            var thumbnail = CreateImage("Thumbnail", card.transform, new Color(0f, 0f, 0f, 0.18f));
            thumbnail.preserveAspect = true;
            thumbnail.raycastTarget = false;
            SetRect(thumbnail.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(12f, 72f), new Vector2(-12f, -12f));

            card.onClick.AddListener(() => SelectItem(itemIndex));
            _cardBackgrounds.Add(card.GetComponent<Image>());
            _cardRects.Add(rect);
            _cardThumbnails.Add(thumbnail);
            _cardItemIndices.Add(itemIndex);
        }

        private async UniTaskVoid LoadThumbnail(int itemIndex, int id, int version)
        {
            try
            {
                var sprite = await _presentation.LoadThumbnail(id);
                if (version != _presentationVersion)
                    return;
                for (var index = 0; index < _cardThumbnails.Count; index++)
                {
                    if (_cardItemIndices[index] == itemIndex && _cardThumbnails[index] != null)
                        _cardThumbnails[index].sprite = sprite;
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void SelectItem(int index)
        {
            if (_presentation == null || index < 0 || index >= _presentation.Items.Length
                || _selectedIndex == index)
                return;
            _selectedIndex = index;
            for (var cardIndex = 0; cardIndex < _cardBackgrounds.Count; cardIndex++)
            {
                _cardBackgrounds[cardIndex].color = _cardItemIndices[cardIndex] == index
                    ? SelectedColor
                    : CardColor;
            }
            var item = _presentation.Items[index];
            _selection.text = item.Text;
            _presentation.Preview?.Invoke(item.Id).Forget();
        }

        private void Confirm()
        {
            if (_presentation == null || _selectedIndex < 0
                || _selectedIndex >= _presentation.Items.Length)
                return;
            _confirm.interactable = false;
            _presentation.Confirm(_presentation.Items[_selectedIndex].Id);
        }

        private void ClearCards()
        {
            _selectedIndex = -1;
            _cardBackgrounds.Clear();
            _cardRects.Clear();
            _cardThumbnails.Clear();
            _cardItemIndices.Clear();
            _scroll.velocity = Vector2.zero;
            _needsCentering = false;
            for (var index = _content.childCount - 1; index >= 0; index--)
            {
                var child = _content.GetChild(index).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        }

        private void SelectClosestCard()
        {
            if (_cardRects.Count == 0)
                return;
            var closestIndex = 0;
            var closestDistance = float.MaxValue;
            for (var index = 0; index < _cardRects.Count; index++)
            {
                var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
                    _viewport, _cardRects[index]);
                var distance = Mathf.Abs(bounds.center.x);
                if (distance >= closestDistance)
                    continue;
                closestDistance = distance;
                closestIndex = index;
            }
            SelectItem(_cardItemIndices[closestIndex]);
            WrapCarousel(closestIndex);
        }

        private void WrapCarousel(int closestSlot)
        {
            var itemCount = _presentation?.Items.Length ?? 0;
            if (itemCount <= 1)
                return;
            if (closestSlot < itemCount)
                ShiftContent(-GetCycleWidth(itemCount));
            else if (closestSlot >= itemCount * 2)
                ShiftContent(GetCycleWidth(itemCount));
        }

        private float GetCycleWidth(int itemCount) =>
            _cardRects[itemCount].anchoredPosition.x - _cardRects[0].anchoredPosition.x;

        private void ShiftContent(float offset)
        {
            var position = _content.anchoredPosition;
            position.x += offset;
            _content.anchoredPosition = position;
        }

        private void CenterCard(int slot)
        {
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
                _viewport, _cardRects[slot]);
            ShiftContent(-bounds.center.x);
        }

        private static Button CreateButton(
            string name, Transform parent, Color color, out Text label)
        {
            var image = CreateImage(name, parent, color);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            label = CreateText("Label", image.transform, 32, TextAnchor.MiddleCenter);
            SetRect(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var value = new GameObject(
                name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            value.transform.SetParent(parent, false);
            var image = value.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(
            string name, Transform parent, int size, TextAnchor alignment)
        {
            var value = new GameObject(
                name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            value.transform.SetParent(parent, false);
            var text = value.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static void SetRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
