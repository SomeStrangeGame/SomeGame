using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
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

        private static readonly Color CardColor = new(0.20f, 0.22f, 0.25f, 0.90f);
        private static readonly Color SelectedColor = new(0.38f, 0.40f, 0.43f, 0.96f);
        private static readonly string[] WardrobeTabs =
            { "Лицо", "Волосы", "Одежда", "Аксессуары" };
        private static readonly string[] WardrobeTabIcons =
            { "◉", "≋", "♙", "◇" };

        private readonly List<CardView> _cards = new();
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _content;
        [SerializeField] private RectTransform _viewport;
        [SerializeField] private ScrollRect _scroll;
        [SerializeField] private Text _title;
        [SerializeField] private Text _selection;
        [SerializeField] private Button _confirm;
        [SerializeField] private Text _confirmLabel;
        [SerializeField] private float _snapSpeed = 12f;
        [Header("Authored wardrobe fallback")]
        [SerializeField] private GameObject _wardrobeRoot;
        [SerializeField] private RectTransform _wardrobePanel;
        [SerializeField] private Text _wardrobeTitle;
        [SerializeField] private Text _wardrobeSelection;
        [SerializeField] private Button _wardrobeConfirm;
        [SerializeField] private Text _wardrobeConfirmLabel;
        [SerializeField] private Text _wardrobeHeader;
        [SerializeField] private Button[] _wardrobeTabs;
        [SerializeField] private Text[] _wardrobeTabLabels;
        [SerializeField] private Button _previous;
        [SerializeField] private Button _next;
        [SerializeField] private Button _previousCharacter;
        [SerializeField] private Button _nextCharacter;
        [SerializeField] private Button _cancel;
        [SerializeField] private Button _collapse;
        [SerializeField] private RectTransform _collapseRect;
        [SerializeField] private Text _collapseLabel;
        [SerializeField] private float _wardrobeExpandedCollapseY = 635f;
        [Header("Optional wardrobe theme")]
        [SerializeField] private bool _wardrobeReferenceLayout;
        [SerializeField] private Font _wardrobeFont;
        [SerializeField] private Image[] _wardrobeTabIcons;
        [SerializeField] private Sprite _wardrobeFaceIcon;
        [SerializeField] private Sprite _wardrobeHairIcon;
        [SerializeField] private Sprite _wardrobeClothesIcon;
        [SerializeField] private Sprite _wardrobeAccessoryIcon;
        [SerializeField] private Sprite _wardrobePanelSprite;
        [SerializeField] private Sprite _wardrobeHeaderSprite;
        [SerializeField] private Sprite _wardrobeConfirmSprite;
        [SerializeField] private Sprite _wardrobeCancelSprite;
        [SerializeField] private Sprite _wardrobeCharacterArrowSprite;
        [SerializeField] private Sprite _wardrobeItemArrowSprite;
        [SerializeField] private Sprite _wardrobeCollapseSprite;
        [SerializeField] private Sprite _wardrobeTabLeftActiveSprite;
        [SerializeField] private Sprite _wardrobeTabLeftInactiveSprite;
        [SerializeField] private Sprite _wardrobeTabMiddleActiveSprite;
        [SerializeField] private Sprite _wardrobeTabMiddleInactiveSprite;
        [SerializeField] private Sprite _wardrobeTabRightActiveSprite;
        [SerializeField] private Sprite _wardrobeTabRightInactiveSprite;
        [SerializeField] private Color _wardrobePanelColor = Color.white;
        [SerializeField] private Color _wardrobeHeaderColor = Color.white;
        [SerializeField] private Color _wardrobePanelTextColor = Color.black;
        [SerializeField] private Color _wardrobeAccentColor =
            new(0.04f, 0.58f, 0.92f, 1f);
        [SerializeField] private Color _wardrobeInactiveColor =
            new(0.02f, 0.28f, 0.65f, 1f);
        private OptionListPresentation _presentation;
        private int _selectedIndex = -1;
        private int _presentationVersion;
        private int _initialSlot;
        private bool _needsCentering;
        private bool _snapping;
        private int _snapSlot = -1;
        private bool _wardrobeLayout;
        private GameObject _defaultPanel;
        private UnityEngine.Events.UnityAction[] _wardrobeTabActions;
        private Action<int> _selectWardrobeTab;
        private int _activeWardrobeTab;
        private EventTrigger _dragTrigger;

        public void ConfigureLayout(OptionListLayout layout, Action<int> selectWardrobeTab = null)
        {
            if (layout != OptionListLayout.Wardrobe || _wardrobeLayout)
                return;
            _wardrobeLayout = true;
            _selectWardrobeTab = selectWardrobeTab;
            BindWardrobeLayout();
        }

        private void Awake()
        {
            _scroll.onValueChanged.AddListener(OnScrollChanged);
            _confirm.onClick.AddListener(Confirm);
            BindCarouselDrag();
            HideImmediate();
        }

        private void OnDestroy()
        {
            _scroll.onValueChanged.RemoveListener(OnScrollChanged);
            _confirm.onClick.RemoveListener(Confirm);
            UnbindWardrobeLayout();
        }

        private void OnScrollChanged(Vector2 _) => SelectClosestCard();

        private void BindCarouselDrag()
        {
            _dragTrigger = _scroll.gameObject.AddComponent<EventTrigger>();
            AddDragTrigger(EventTriggerType.BeginDrag, BeginCarouselDrag);
            AddDragTrigger(EventTriggerType.EndDrag, EndCarouselDrag);
        }

        private void AddDragTrigger(
            EventTriggerType eventType,
            UnityEngine.Events.UnityAction<BaseEventData> callback)
        {
            var entry = new EventTrigger.Entry { eventID = eventType };
            entry.callback.AddListener(callback);
            _dragTrigger.triggers.Add(entry);
        }

        private void BeginCarouselDrag(BaseEventData eventData)
        {
            if (_wardrobeLayout)
                return;
            _snapping = false;
            _snapSlot = -1;
        }

        private void EndCarouselDrag(BaseEventData eventData)
        {
            if (_wardrobeLayout || _cards.Count == 0)
                return;
            FocusCard(FindClosestCardSlot());
        }

        private void LateUpdate()
        {
            if (_snapping)
                MoveFocusedCardToCenter();
        }

        public void SetPresentation(OptionListPresentation presentation)
        {
            _presentation = presentation
                ?? throw new ArgumentNullException(nameof(presentation));
            _presentationVersion++;
            ClearCards();
            ActiveTitle.text = presentation.Title;
            if (_wardrobeHeader != null)
                _wardrobeHeader.text = DisplayName(presentation.Header);
            ActiveConfirmLabel.text = presentation.ConfirmationText;
            ActiveConfirm.interactable = presentation.Items.Length > 0;
            UpdateWardrobeTabLabels(presentation);
            SetWardrobeTab(presentation.ActiveTab);
            SetWardrobeTabsInteractable(
                presentation.TabsInteractable,
                presentation.InteractableTabs);
            if (_previous != null)
                _previous.interactable = presentation.Items.Length > 1;
            if (_next != null)
                _next.interactable = presentation.Items.Length > 1;
            UpdateWardrobeActions(presentation);

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
            rect.sizeDelta = new Vector2(640f, 700f);
            var layout = card.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = 640f;
            layout.preferredHeight = 700f;
            label.text = item.Text;
            label.gameObject.SetActive(false);

            var thumbnail = CreateImage("Thumbnail", card.transform, Color.white);
            thumbnail.preserveAspect = true;
            thumbnail.raycastTarget = false;
            SetRect(thumbnail.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(28f, 28f), new Vector2(-28f, -28f));

            var cardSlot = _cards.Count;
            card.onClick.AddListener(() => FocusCard(cardSlot, true));
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
            ActiveSelection.text = item.Text;
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
            ActiveConfirm.interactable = false;
            _presentation.Confirm(_presentation.Items[_selectedIndex].Id);
        }

        private void ClearCards()
        {
            _selectedIndex = -1;
            _cards.Clear();
            _scroll.velocity = Vector2.zero;
            _needsCentering = false;
            _snapping = false;
            _snapSlot = -1;
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
            var closestIndex = FindClosestCardSlot();
            SelectItem(_cards[closestIndex].ItemIndex);
            WrapCarousel(closestIndex);
        }

        private int FindClosestCardSlot()
        {
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
            return closestIndex;
        }

        private void FocusCard(int slot, bool forcePreview = false)
        {
            if (slot < 0 || slot >= _cards.Count)
                return;
            _scroll.StopMovement();
            slot = NormalizeCarouselSlot(slot);
            SelectItem(_cards[slot].ItemIndex, true, forcePreview);
            _snapSlot = slot;
            _snapping = true;
        }

        private void MoveFocusedCardToCenter()
        {
            if (_snapSlot < 0 || _snapSlot >= _cards.Count)
            {
                _snapping = false;
                return;
            }
            var offset = CardCenter(_snapSlot);
            if (Mathf.Abs(offset) < 0.5f)
            {
                ShiftContent(-offset);
                _snapping = false;
                return;
            }
            var amount = 1f - Mathf.Exp(-_snapSpeed * Time.unscaledDeltaTime);
            ShiftContent(-offset * amount);
        }

        private void WrapCarousel(int closestSlot)
        {
            NormalizeCarouselSlot(closestSlot);
        }

        private int NormalizeCarouselSlot(int slot)
        {
            var itemCount = _presentation?.Items.Length ?? 0;
            if (itemCount <= 1)
                return slot;
            if (slot < itemCount)
            {
                ShiftContent(-GetCycleWidth(itemCount));
                return slot + itemCount;
            }
            if (slot >= itemCount * 2)
            {
                ShiftContent(GetCycleWidth(itemCount));
                return slot - itemCount;
            }
            return slot;
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
            ShiftContent(-CardCenter(slot));
        }

        private float CardCenter(int slot) =>
            RectTransformUtility.CalculateRelativeRectTransformBounds(
                _viewport, _cards[slot].Rect).center.x;

        private Text ActiveTitle => _wardrobeLayout ? _wardrobeTitle : _title;
        private Text ActiveSelection => _wardrobeLayout ? _wardrobeSelection : _selection;
        private Button ActiveConfirm => _wardrobeLayout ? _wardrobeConfirm : _confirm;
        private Text ActiveConfirmLabel => _wardrobeLayout ? _wardrobeConfirmLabel : _confirmLabel;

        private void BindWardrobeLayout()
        {
            ValidateWardrobeLayout();
            ApplyWardrobeTheme();
            _defaultPanel = _title.transform.parent.gameObject;
            _defaultPanel.SetActive(false);
            _wardrobeRoot.SetActive(true);

            _wardrobeConfirm.onClick.AddListener(Confirm);
            _previous.onClick.AddListener(SelectPrevious);
            _next.onClick.AddListener(SelectNext);
            _previousCharacter.onClick.AddListener(SelectPreviousCharacter);
            _nextCharacter.onClick.AddListener(SelectNextCharacter);
            _cancel.onClick.AddListener(CancelWardrobe);
            _collapse.onClick.AddListener(ToggleWardrobePanel);

            _wardrobeTabActions = new UnityEngine.Events.UnityAction[_wardrobeTabs.Length];
            for (var index = 0; index < _wardrobeTabs.Length; index++)
            {
                var tabIndex = index;
                _wardrobeTabActions[index] = () => _selectWardrobeTab?.Invoke(tabIndex);
                _wardrobeTabs[index].onClick.AddListener(_wardrobeTabActions[index]);
            }
        }

        private void UnbindWardrobeLayout()
        {
            if (!_wardrobeLayout || _wardrobeRoot == null)
                return;
            _wardrobeConfirm.onClick.RemoveListener(Confirm);
            _previous.onClick.RemoveListener(SelectPrevious);
            _next.onClick.RemoveListener(SelectNext);
            _previousCharacter.onClick.RemoveListener(SelectPreviousCharacter);
            _nextCharacter.onClick.RemoveListener(SelectNextCharacter);
            _cancel.onClick.RemoveListener(CancelWardrobe);
            _collapse.onClick.RemoveListener(ToggleWardrobePanel);
            if (_wardrobeTabActions == null)
                return;
            for (var index = 0; index < _wardrobeTabActions.Length; index++)
                _wardrobeTabs[index].onClick.RemoveListener(_wardrobeTabActions[index]);
        }

        private void ValidateWardrobeLayout()
        {
            if (_wardrobeRoot == null || _wardrobePanel == null
                || _wardrobeTitle == null || _wardrobeSelection == null
                || _wardrobeConfirm == null || _wardrobeConfirmLabel == null
                || _wardrobeHeader == null || _previous == null || _next == null
                || _previousCharacter == null || _nextCharacter == null
                || _cancel == null || _collapse == null || _collapseRect == null
                || _collapseLabel == null
                || _wardrobeTabs == null || _wardrobeTabLabels == null
                || _wardrobeTabs.Length != WardrobeTabs.Length
                || _wardrobeTabLabels.Length != WardrobeTabs.Length)
            {
                throw new InvalidOperationException(
                    "OptionListScreen fallback wardrobe prefab is not fully authored.");
            }
        }

        private void SelectPrevious() => SelectRelative(-1);
        private void SelectNext() => SelectRelative(1);
        private void SelectPreviousCharacter() => _presentation?.PreviousCharacter?.Invoke();
        private void SelectNextCharacter() => _presentation?.NextCharacter?.Invoke();
        private void CancelWardrobe() => _presentation?.Cancel?.Invoke();
        private void ToggleWardrobePanel() =>
            SetWardrobeExpanded(_wardrobePanel == null || !_wardrobePanel.gameObject.activeSelf);

        private void SetWardrobeTab(int activeTab)
        {
            if (!_wardrobeLayout || _wardrobeTabs == null)
                return;
            _activeWardrobeTab = activeTab;
            RefreshWardrobeTabs();
        }

        private void RefreshWardrobeTabs()
        {
            if (_wardrobeTabs == null)
                return;
            var visibleTabs = new List<int>(_wardrobeTabs.Length);
            for (var index = 0; index < _wardrobeTabs.Length; index++)
            {
                if (_wardrobeTabs[index].gameObject.activeSelf)
                    visibleTabs.Add(index);
            }
            for (var index = 0; index < _wardrobeTabs.Length; index++)
            {
                var image = _wardrobeTabs[index].GetComponent<Image>();
                if (image == null)
                    continue;
                var visibleIndex = visibleTabs.IndexOf(index);
                var customSprite = visibleIndex >= 0
                    ? WardrobeTabSprite(
                        visibleIndex,
                        visibleTabs.Count,
                        index == _activeWardrobeTab)
                    : null;
                if (customSprite != null)
                {
                    image.sprite = customSprite;
                    image.type = Image.Type.Simple;
                    image.color = Color.white;
                }
                else
                {
                    image.color = index == _activeWardrobeTab
                        ? _wardrobeAccentColor
                        : _wardrobeInactiveColor;
                }
            }
        }

        private Sprite WardrobeTabSprite(int visibleIndex, int visibleCount, bool active)
        {
            if (!_wardrobeReferenceLayout || visibleCount <= 0)
                return null;
            if (visibleCount > 1 && visibleIndex == 0)
                return active ? _wardrobeTabLeftActiveSprite : _wardrobeTabLeftInactiveSprite;
            if (visibleCount > 1 && visibleIndex == visibleCount - 1)
                return active ? _wardrobeTabRightActiveSprite : _wardrobeTabRightInactiveSprite;
            return active ? _wardrobeTabMiddleActiveSprite : _wardrobeTabMiddleInactiveSprite;
        }

        private void UpdateWardrobeTabLabels(OptionListPresentation presentation)
        {
            if (!_wardrobeLayout || _wardrobeTabLabels == null)
                return;
            for (var index = 0; index < _wardrobeTabLabels.Length; index++)
            {
                var label = _wardrobeTabLabels[index];
                if (label == null)
                    continue;
                var count = presentation.TabItemCounts != null
                    && index < presentation.TabItemCounts.Length
                        ? presentation.TabItemCounts[index]
                        : index == presentation.ActiveTab
                            ? presentation.Items.Length
                            : -1;
                var themedIcon = _wardrobeReferenceLayout
                    && _wardrobeTabIcons != null
                    && index < _wardrobeTabIcons.Length
                    && _wardrobeTabIcons[index] != null
                    && _wardrobeTabIcons[index].sprite != null;
                label.text = themedIcon
                    ? count >= 0 ? count.ToString() : string.Empty
                    : count >= 0
                        ? $"{WardrobeTabIcons[index]}  {count}\n{WardrobeTabs[index]}"
                        : $"{WardrobeTabIcons[index]}\n{WardrobeTabs[index]}";
            }
        }

        private void SetWardrobeTabsInteractable(
            bool interactable,
            int[] interactableTabs)
        {
            if (!_wardrobeLayout || _wardrobeTabs == null)
                return;
            for (var index = 0; index < _wardrobeTabs.Length; index++)
            {
                var available = interactableTabs == null
                    || Array.IndexOf(interactableTabs, index) >= 0;
                _wardrobeTabs[index].gameObject.SetActive(available);
                _wardrobeTabs[index].interactable = interactable && available;
            }
            RefreshWardrobeTabs();
        }

        private void ApplyWardrobeTheme()
        {
            if (!_wardrobeReferenceLayout)
                return;

            if (_wardrobeFont != null)
            {
                foreach (var text in _wardrobeRoot.GetComponentsInChildren<Text>(true))
                    text.font = _wardrobeFont;
            }

            SetImageTheme(_wardrobePanel.GetComponent<Image>(), _wardrobePanelSprite, true);
            SetImageTheme(
                _wardrobeHeader.transform.parent.GetComponent<Image>(),
                _wardrobeHeaderSprite,
                true);
            SetImageTheme(_wardrobeConfirm.GetComponent<Image>(), _wardrobeConfirmSprite, true);
            SetImageTheme(_cancel.GetComponent<Image>(), _wardrobeCancelSprite, true);
            SetImageTheme(
                _previousCharacter.GetComponent<Image>(),
                _wardrobeCharacterArrowSprite,
                false);
            SetImageTheme(
                _nextCharacter.GetComponent<Image>(),
                _wardrobeCharacterArrowSprite,
                false);
            SetImageTheme(_previous.GetComponent<Image>(), _wardrobeItemArrowSprite, false);
            SetImageTheme(_next.GetComponent<Image>(), _wardrobeItemArrowSprite, false);
            SetImageTheme(_collapse.GetComponent<Image>(), _wardrobeCollapseSprite, false);

            _wardrobePanel.GetComponent<Image>().color = _wardrobePanelColor;
            _wardrobeHeader.transform.parent.GetComponent<Image>().color = _wardrobeHeaderColor;
            _wardrobeTitle.color = _wardrobePanelTextColor;
            _wardrobeSelection.color = _wardrobePanelTextColor;

            var iconSprites = new[]
            {
                _wardrobeFaceIcon,
                _wardrobeHairIcon,
                _wardrobeClothesIcon,
                _wardrobeAccessoryIcon
            };
            if (_wardrobeTabIcons != null)
            {
                for (var index = 0; index < _wardrobeTabIcons.Length; index++)
                {
                    var icon = _wardrobeTabIcons[index];
                    var sprite = index < iconSprites.Length ? iconSprites[index] : null;
                    if (icon == null)
                        continue;
                    icon.sprite = sprite;
                    icon.color = Color.white;
                    icon.preserveAspect = true;
                    icon.gameObject.SetActive(sprite != null);
                    if (sprite != null && index < _wardrobeTabLabels.Length)
                    {
                        var label = _wardrobeTabLabels[index];
                        label.fontSize = 30;
                        label.alignment = TextAnchor.MiddleCenter;
                        label.color = Color.white;
                        label.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                        label.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                        label.rectTransform.anchoredPosition = new Vector2(40f, -12f);
                        label.rectTransform.sizeDelta = new Vector2(64f, 56f);
                    }
                }
            }

            if (_wardrobeCharacterArrowSprite != null)
            {
                _previousCharacter.GetComponentInChildren<Text>(true).text = string.Empty;
                _nextCharacter.GetComponentInChildren<Text>(true).text = string.Empty;
                _nextCharacter.transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            if (_wardrobeItemArrowSprite != null)
            {
                _previous.GetComponentInChildren<Text>(true).text = string.Empty;
                _next.GetComponentInChildren<Text>(true).text = string.Empty;
                _previous.transform.localScale = Vector3.one;
                _next.transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            if (_wardrobeCollapseSprite != null)
                _collapseLabel.text = string.Empty;
        }

        private static void SetImageTheme(Image image, Sprite sprite, bool sliced)
        {
            if (image == null || sprite == null)
                return;
            image.sprite = sprite;
            image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            image.color = Color.white;
            image.preserveAspect = !sliced;
        }

        private void UpdateWardrobeActions(OptionListPresentation presentation)
        {
            if (!_wardrobeLayout)
                return;
            var canSwitchCharacter = presentation.PreviousCharacter != null
                && presentation.NextCharacter != null;
            if (_previousCharacter != null)
                _previousCharacter.gameObject.SetActive(canSwitchCharacter);
            if (_nextCharacter != null)
                _nextCharacter.gameObject.SetActive(canSwitchCharacter);

            var canCancel = presentation.Cancel != null;
            if (_cancel != null)
                _cancel.gameObject.SetActive(canCancel);
            var confirmRect = _wardrobeConfirm.GetComponent<RectTransform>();
            confirmRect.anchoredPosition = new Vector2(
                canCancel ? -130f : 0f,
                125f);
            confirmRect.sizeDelta = new Vector2(
                380f,
                130f);
        }

        private void SetWardrobeExpanded(bool expanded)
        {
            if (!_wardrobeLayout || _wardrobePanel == null)
                return;
            _wardrobePanel.gameObject.SetActive(expanded);
            if (_collapseRect != null)
                _collapseRect.anchoredPosition = new Vector2(
                    0f,
                    expanded ? _wardrobeExpandedCollapseY : 58f);
            if (_collapseLabel != null)
            {
                if (_wardrobeCollapseSprite != null)
                {
                    _collapseLabel.text = string.Empty;
                    _collapse.transform.localScale = new Vector3(
                        1f,
                        expanded ? 1f : -1f,
                        1f);
                }
                else
                {
                    _collapseLabel.text = expanded ? "⌄" : "⌃";
                }
            }
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
