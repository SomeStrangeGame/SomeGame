using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Novels.Choose.View
{
    public readonly struct CarouselOption
    {
        public CarouselOption(int id, string text)
        {
            Id = id;
            Text = text ?? string.Empty;
        }

        public int Id { get; }
        public string Text { get; }
    }

    public sealed class CarouselPresentation
    {
        public string Title;
        public string ConfirmationText;
        public CarouselOption[] Options;
        public Func<int, UniTask<Sprite>> LoadThumbnail;
        public Func<int, UniTask> Preview;
        public Action<int> Confirm;
    }

    public class ChooseScreen : MonoBehaviour
    {
        private static readonly Color PanelColor = new(0.12f, 0.14f, 0.17f, 0.96f);
        private static readonly Color CardColor = new(0.24f, 0.27f, 0.32f, 0.96f);
        private static readonly Color SelectedColor = new(0.20f, 0.55f, 0.78f, 1f);

        private readonly List<Image> _cardBackgrounds = new();
        private readonly List<RectTransform> _cardRects = new();
        private readonly List<Image> _cardThumbnails = new();
        private readonly List<int> _cardOptionIndices = new();
        private CanvasGroup _canvasGroup;
        private RectTransform _content;
        private RectTransform _viewport;
        private ScrollRect _scroll;
        private Text _title;
        private Text _selection;
        private Button _confirm;
        private Text _confirmLabel;
        private CarouselPresentation _presentation;
        private int _selectedIndex = -1;
        private int _presentationVersion;
        private int _initialSlot;
        private bool _needsCentering;

        public void Init()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 150;
            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080f, 1920f);
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
            SetRect(viewport.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f),
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
            _scroll.viewport = viewport.rectTransform;
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
            SetRect(_confirm.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-260f, 28f), new Vector2(260f, 100f));
            _confirm.onClick.AddListener(Confirm);
            HideImmediate();
        }

        public void SetPresentation(CarouselPresentation presentation)
        {
            _presentation = presentation ?? throw new ArgumentNullException(nameof(presentation));
            _presentationVersion++;
            ClearCards();
            _title.text = presentation.Title;
            _confirmLabel.text = presentation.ConfirmationText;
            _confirm.interactable = presentation.Options.Length > 0;

            var copies = presentation.Options.Length > 1 ? 3 : 1;
            for (var copy = 0; copy < copies; copy++)
            {
                for (var index = 0; index < presentation.Options.Length; index++)
                    CreateCard(index, presentation.Options[index]);
            }

            for (var index = 0; index < presentation.Options.Length; index++)
            {
                var option = presentation.Options[index];
                LoadThumbnail(index, option.Id, _presentationVersion).Forget();
            }

            if (presentation.Options.Length > 0)
            {
                _initialSlot = copies == 1 ? 0 : presentation.Options.Length;
                _needsCentering = true;
                SelectOption(0);
            }
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

        private void CreateCard(int optionIndex, CarouselOption option)
        {
            var card = CreateButton($"Option_{option.Id}", _content, CardColor, out var label);
            var rect = card.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(240f, 270f);
            var layout = card.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = 240f;
            layout.preferredHeight = 270f;
            label.text = option.Text;
            label.fontSize = 28;
            label.alignment = TextAnchor.LowerCenter;
            SetRect(label.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(12f, 12f), new Vector2(-12f, -190f));

            var thumbnail = CreateImage("Thumbnail", card.transform, new Color(0f, 0f, 0f, 0.18f));
            thumbnail.preserveAspect = true;
            thumbnail.raycastTarget = false;
            SetRect(thumbnail.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(12f, 72f), new Vector2(-12f, -12f));

            var capturedIndex = optionIndex;
            card.onClick.AddListener(() => SelectOption(capturedIndex));
            _cardBackgrounds.Add(card.GetComponent<Image>());
            _cardRects.Add(rect);
            _cardThumbnails.Add(thumbnail);
            _cardOptionIndices.Add(optionIndex);
        }

        private async UniTaskVoid LoadThumbnail(int optionIndex, int id, int version)
        {
            try
            {
                var sprite = await _presentation.LoadThumbnail(id);
                if (version != _presentationVersion)
                    return;
                for (var index = 0; index < _cardThumbnails.Count; index++)
                {
                    if (_cardOptionIndices[index] == optionIndex
                        && _cardThumbnails[index] != null)
                    {
                        _cardThumbnails[index].sprite = sprite;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void SelectOption(int index)
        {
            if (_presentation == null || index < 0 || index >= _presentation.Options.Length)
                return;
            if (_selectedIndex == index)
                return;
            _selectedIndex = index;
            for (var cardIndex = 0; cardIndex < _cardBackgrounds.Count; cardIndex++)
            {
                _cardBackgrounds[cardIndex].color =
                    _cardOptionIndices[cardIndex] == index ? SelectedColor : CardColor;
            }
            var option = _presentation.Options[index];
            _selection.text = option.Text;
            _presentation.Preview(option.Id).Forget();
        }

        private void Confirm()
        {
            if (_presentation == null
                || _selectedIndex < 0
                || _selectedIndex >= _presentation.Options.Length)
            {
                return;
            }
            _confirm.interactable = false;
            _presentation.Confirm(_presentation.Options[_selectedIndex].Id);
        }

        private void ClearCards()
        {
            _selectedIndex = -1;
            _cardBackgrounds.Clear();
            _cardRects.Clear();
            _cardThumbnails.Clear();
            _cardOptionIndices.Clear();
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
                    _viewport,
                    _cardRects[index]);
                var distance = Mathf.Abs(bounds.center.x);
                if (distance >= closestDistance)
                    continue;
                closestDistance = distance;
                closestIndex = index;
            }
            SelectOption(_cardOptionIndices[closestIndex]);
            WrapCarousel(closestIndex);
        }

        private void WrapCarousel(int closestSlot)
        {
            var optionCount = _presentation?.Options.Length ?? 0;
            if (optionCount <= 1)
                return;
            if (closestSlot < optionCount)
            {
                ShiftContent(-GetCycleWidth(optionCount));
            }
            else if (closestSlot >= optionCount * 2)
            {
                ShiftContent(GetCycleWidth(optionCount));
            }
        }

        private float GetCycleWidth(int optionCount)
        {
            var first = _cardRects[0];
            var nextCycle = _cardRects[optionCount];
            return nextCycle.anchoredPosition.x - first.anchoredPosition.x;
        }

        private void ShiftContent(float offset)
        {
            var position = _content.anchoredPosition;
            position.x += offset;
            _content.anchoredPosition = position;
        }

        private void CenterCard(int slot)
        {
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
                _viewport,
                _cardRects[slot]);
            ShiftContent(-bounds.center.x);
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            Color color,
            out Text label)
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
            var value = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            value.transform.SetParent(parent, false);
            var image = value.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(string name, Transform parent, int size, TextAnchor alignment)
        {
            var value = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
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
