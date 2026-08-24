using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Novels.OptionSelection
{
    public sealed class OptionListScreen : MonoBehaviour
    {
        private readonly struct CardView
        {
            internal CardView(
                int itemIndex,
                Image background,
                RectTransform rect,
                Image thumbnail)
            {
                ItemIndex = itemIndex;
                Background = background;
                Rect = rect;
                Thumbnail = thumbnail;
            }

            internal int ItemIndex { get; }
            internal Image Background { get; }
            internal RectTransform Rect { get; }
            internal Image Thumbnail { get; }
        }

        private static readonly Color CardColor = new(0.24f, 0.27f, 0.32f, 0.96f);
        private static readonly Color SelectedColor = new(0.20f, 0.55f, 0.78f, 1f);

        private readonly List<CardView> _cards = new();
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _content;
        [SerializeField] private RectTransform _viewport;
        [SerializeField] private ScrollRect _scroll;
        [SerializeField] private Text _title;
        [SerializeField] private Text _selection;
        [SerializeField] private Button _confirm;
        [SerializeField] private Text _confirmLabel;
        private OptionListPresentation _presentation;
        private int _selectedIndex = -1;
        private int _presentationVersion;
        private int _initialSlot;
        private bool _needsCentering;

        private void Awake()
        {
            _scroll.onValueChanged.AddListener(OnScrollChanged);
            _confirm.onClick.AddListener(Confirm);
            HideImmediate();
        }

        private void OnDestroy()
        {
            _scroll.onValueChanged.RemoveListener(OnScrollChanged);
            _confirm.onClick.RemoveListener(Confirm);
        }

        private void OnScrollChanged(Vector2 _) => SelectClosestCard();

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
            _cards.Add(new CardView(
                itemIndex,
                card.GetComponent<Image>(),
                rect,
                thumbnail));
        }

        private async UniTaskVoid LoadThumbnail(int itemIndex, int id, int version)
        {
            try
            {
                var sprite = await _presentation.LoadThumbnail(id);
                if (version != _presentationVersion)
                    return;
                foreach (var card in _cards)
                {
                    if (card.ItemIndex == itemIndex && card.Thumbnail != null)
                        card.Thumbnail.sprite = sprite;
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
            foreach (var card in _cards)
            {
                card.Background.color = card.ItemIndex == index
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
            _cards.Clear();
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
            if (_cards.Count == 0)
                return;
            var closestIndex = 0;
            var closestDistance = float.MaxValue;
            for (var index = 0; index < _cards.Count; index++)
            {
                var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
                    _viewport, _cards[index].Rect);
                var distance = Mathf.Abs(bounds.center.x);
                if (distance >= closestDistance)
                    continue;
                closestDistance = distance;
                closestIndex = index;
            }
            SelectItem(_cards[closestIndex].ItemIndex);
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
            _cards[itemCount].Rect.anchoredPosition.x
            - _cards[0].Rect.anchoredPosition.x;

        private void ShiftContent(float offset)
        {
            var position = _content.anchoredPosition;
            position.x += offset;
            _content.anchoredPosition = position;
        }

        private void CenterCard(int slot)
        {
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
                _viewport, _cards[slot].Rect);
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
