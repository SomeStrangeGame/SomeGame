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
        private static readonly Color WardrobePanelColor = new(0.035f, 0.065f, 0.12f, 0.97f);
        private static readonly Color WardrobeAccentColor = new(0.12f, 0.52f, 0.92f, 1f);
        private static readonly string[] WardrobeTabs =
            { "Лицо", "Волосы", "Одежда", "Аксессуары" };

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
        private bool _wardrobeLayout;
        private RectTransform _panel;
        private RectTransform _collapseRect;
        private Text _wardrobeHeader;
        private Text _collapseLabel;
        private Button _previous;
        private Button _next;
        private Action<int> _selectWardrobeTab;

        public void ConfigureLayout(OptionListLayout layout, Action<int> selectWardrobeTab = null)
        {
            if (layout != OptionListLayout.Wardrobe || _wardrobeLayout)
                return;
            _wardrobeLayout = true;
            _selectWardrobeTab = selectWardrobeTab;
            BuildWardrobeLayout();
        }

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
            if (_wardrobeHeader != null)
                _wardrobeHeader.text = DisplayName(presentation.Header);
            _confirmLabel.text = presentation.ConfirmationText;
            _confirm.interactable = presentation.Items.Length > 0;
            UpdateWardrobeTabLabels(presentation);
            SetWardrobeTab(presentation.ActiveTab);
            SetWardrobeTabsInteractable(
                presentation.TabsInteractable,
                presentation.InteractableTabs);
            if (_previous != null)
                _previous.interactable = presentation.Items.Length > 1;
            if (_next != null)
                _next.interactable = presentation.Items.Length > 1;

            if (_wardrobeLayout)
            {
                if (presentation.Items.Length == 0)
                    return;
                SelectItem(
                    FindInitialItemIndex(presentation),
                    presentation.PreviewInitialItem);
                return;
            }

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
            var initialIndex = FindInitialItemIndex(presentation);
            _initialSlot = copies == 1
                ? initialIndex
                : presentation.Items.Length + initialIndex;
            _needsCentering = true;
            SelectItem(initialIndex, false);
        }

        private static int FindInitialItemIndex(OptionListPresentation presentation)
        {
            if (!presentation.InitialItemId.HasValue)
                return 0;
            for (var index = 0; index < presentation.Items.Length; index++)
            {
                if (presentation.Items[index].Id == presentation.InitialItemId.Value)
                    return index;
            }
            return 0;
        }

        public void ShowImmediate()
        {
            gameObject.SetActive(true);
            SetWardrobeExpanded(true);
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

            card.onClick.AddListener(() => SelectItem(itemIndex, true, true));
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

        private void SelectItem(
            int index,
            bool preview = true,
            bool forcePreview = false)
        {
            if (_presentation == null || index < 0 || index >= _presentation.Items.Length)
                return;
            if (_selectedIndex == index)
            {
                if (preview && forcePreview)
                    _presentation.Preview?.Invoke(_presentation.Items[index].Id).Forget();
                return;
            }
            _selectedIndex = index;
            foreach (var card in _cards)
            {
                card.Background.color = card.ItemIndex == index
                    ? SelectedColor
                    : CardColor;
            }
            var item = _presentation.Items[index];
            _selection.text = item.Text;
            if (preview)
                _presentation.Preview?.Invoke(item.Id).Forget();
        }

        private void SelectRelative(int direction)
        {
            var itemCount = _presentation?.Items.Length ?? 0;
            if (itemCount == 0)
                return;
            var index = (_selectedIndex + direction + itemCount) % itemCount;
            SelectItem(index);
            if (_cards.Count >= itemCount * 2)
                CenterCard(itemCount + index);
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

        private void BuildWardrobeLayout()
        {
            _panel = _title.transform.parent as RectTransform;
            if (_panel == null)
                return;

            var panelImage = _panel.GetComponent<Image>();
            if (panelImage != null)
                panelImage.color = WardrobePanelColor;
            _panel.anchoredPosition = new Vector2(0f, 220f);
            _panel.sizeDelta = new Vector2(-48f, 440f);

            _title.fontSize = 28;
            _title.rectTransform.anchoredPosition = new Vector2(0f, -150f);
            _title.rectTransform.sizeDelta = new Vector2(-160f, 48f);
            _selection.fontSize = 40;
            _selection.rectTransform.anchoredPosition = new Vector2(0f, 190f);
            _selection.rectTransform.sizeDelta = new Vector2(-260f, 70f);
            _viewport.gameObject.SetActive(false);
            var confirmRect = _confirm.GetComponent<RectTransform>();
            confirmRect.anchoredPosition = new Vector2(0f, 62f);
            confirmRect.sizeDelta = new Vector2(440f, 86f);

            BuildWardrobeHeader();
            BuildWardrobeTabs();
            BuildWardrobeArrows();
            BuildCollapseButton();
        }

        private void BuildWardrobeHeader()
        {
            var image = CreateImage(
                "WardrobeHeader",
                transform,
                new Color(0.035f, 0.075f, 0.14f, 0.96f));
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -92f);
            rect.sizeDelta = new Vector2(620f, 92f);
            _wardrobeHeader = CreateText(
                "CharacterName",
                image.transform,
                42,
                TextAnchor.MiddleCenter);
            SetRect(_wardrobeHeader.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(24f, 8f), new Vector2(-24f, -8f));
        }

        private void BuildWardrobeTabs()
        {
            var bar = new GameObject(
                "WardrobeTabs",
                typeof(RectTransform),
                typeof(HorizontalLayoutGroup));
            bar.transform.SetParent(_panel, false);
            var rect = bar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(0f, -64f);
            rect.sizeDelta = new Vector2(-48f, 104f);
            var layout = bar.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            for (var index = 0; index < WardrobeTabs.Length; index++)
            {
                var tabIndex = index;
                var button = CreateButton(
                    $"WardrobeTab_{index}",
                    bar.transform,
                    new Color(0.08f, 0.13f, 0.21f, 0.96f),
                    out var label);
                label.text = WardrobeTabs[index];
                label.fontSize = 21;
                label.raycastTarget = false;
                SetRect(label.rectTransform, Vector2.zero, Vector2.one,
                    new Vector2(6f, 4f), new Vector2(-6f, -4f));
                button.onClick.AddListener(() => _selectWardrobeTab?.Invoke(tabIndex));
            }
        }

        private void SetWardrobeTab(int activeTab)
        {
            if (!_wardrobeLayout || _panel == null)
                return;
            var tabs = _panel.Find("WardrobeTabs");
            if (tabs == null)
                return;
            for (var index = 0; index < tabs.childCount; index++)
            {
                var image = tabs.GetChild(index).GetComponent<Image>();
                if (image != null)
                {
                    image.color = index == activeTab
                        ? WardrobeAccentColor
                        : new Color(0.08f, 0.13f, 0.21f, 0.96f);
                }
            }
        }

        private void UpdateWardrobeTabLabels(OptionListPresentation presentation)
        {
            if (!_wardrobeLayout || _panel == null)
                return;
            var tabs = _panel.Find("WardrobeTabs");
            if (tabs == null)
                return;
            for (var index = 0; index < tabs.childCount; index++)
            {
                var label = tabs.GetChild(index).Find("Label")?.GetComponent<Text>();
                if (label == null)
                    continue;
                var count = presentation.TabItemCounts != null
                    && index < presentation.TabItemCounts.Length
                        ? presentation.TabItemCounts[index]
                        : index == presentation.ActiveTab
                            ? presentation.Items.Length
                            : -1;
                label.text = count >= 0
                    ? $"{WardrobeTabs[index]}\n{count}"
                    : WardrobeTabs[index];
            }
        }

        private void SetWardrobeTabsInteractable(
            bool interactable,
            int[] interactableTabs)
        {
            if (!_wardrobeLayout || _panel == null)
                return;
            var tabs = _panel.Find("WardrobeTabs");
            if (tabs == null)
                return;
            for (var index = 0; index < tabs.childCount; index++)
            {
                var tab = tabs.GetChild(index);
                var available = interactableTabs == null
                    || Array.IndexOf(interactableTabs, index) >= 0;
                tab.gameObject.SetActive(available);
                var button = tab.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = interactable
                        && available;
                }
            }
        }

        private void BuildWardrobeArrows()
        {
            _previous = CreateButton(
                "Previous",
                _panel,
                new Color(0.06f, 0.16f, 0.28f, 0.98f),
                out var previousLabel);
            previousLabel.text = "‹";
            previousLabel.fontSize = 64;
            PositionWardrobeArrow(_previous, -390f);
            _previous.onClick.AddListener(() => SelectRelative(-1));

            _next = CreateButton(
                "Next",
                _panel,
                new Color(0.06f, 0.16f, 0.28f, 0.98f),
                out var nextLabel);
            nextLabel.text = "›";
            nextLabel.fontSize = 64;
            PositionWardrobeArrow(_next, 390f);
            _next.onClick.AddListener(() => SelectRelative(1));
        }

        private static void PositionWardrobeArrow(Button button, float x)
        {
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, -30f);
            rect.sizeDelta = new Vector2(92f, 112f);
        }

        private void BuildCollapseButton()
        {
            var button = CreateButton(
                "Collapse",
                transform,
                new Color(0.035f, 0.075f, 0.14f, 0.98f),
                out _collapseLabel);
            _collapseLabel.text = "⌄";
            _collapseLabel.fontSize = 52;
            _collapseRect = button.GetComponent<RectTransform>();
            _collapseRect.anchorMin = _collapseRect.anchorMax = new Vector2(0.5f, 0f);
            _collapseRect.anchoredPosition = new Vector2(0f, 468f);
            _collapseRect.sizeDelta = new Vector2(150f, 76f);
            button.onClick.AddListener(() => SetWardrobeExpanded(_panel == null || !_panel.gameObject.activeSelf));
        }

        private void SetWardrobeExpanded(bool expanded)
        {
            if (!_wardrobeLayout || _panel == null)
                return;
            _panel.gameObject.SetActive(expanded);
            if (_collapseRect != null)
                _collapseRect.anchoredPosition = new Vector2(0f, expanded ? 468f : 58f);
            if (_collapseLabel != null)
                _collapseLabel.text = expanded ? "⌄" : "⌃";
        }

        private static string DisplayName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Персонаж";
            var trimmed = value.Trim();
            return char.ToUpperInvariant(trimmed[0]) + trimmed.Substring(1);
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
