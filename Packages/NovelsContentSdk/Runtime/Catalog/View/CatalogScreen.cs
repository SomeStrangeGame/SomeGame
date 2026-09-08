using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Novels.Catalog.View
{
    public sealed class CatalogScreen : MonoBehaviour
    {
        [SerializeField] private Text _title;
        [SerializeField] private Card _cardPrefab;
        [SerializeField] private CatalogCarousel _carousel;
        [SerializeField] private RectTransform _safeArea;
        [SerializeField] private Text _pageIndicator;
        [SerializeField] private Button _actionButton;
        [SerializeField] private Text _actionLabel;
        [SerializeField] private Button _secondaryActionButton;
        [SerializeField] private Text _secondaryActionLabel;
        [SerializeField] private GameObject _restartConfirmation;
        [SerializeField] private Button _restartCancelButton;
        [SerializeField] private Button _restartConfirmButton;
        [SerializeField] private GameObject _verticalRoot;

        private readonly Dictionary<string, Card> _cards = new();
        private readonly Dictionary<Card, ItemViewModel> _models = new();
        private Action _secondaryAction;
        private Rect _appliedSafeArea;
        private Vector2 _verticalViewportSize;
        private ScrollRect _verticalScroll;
        private CatalogSettingsPopup _settingsPopup;
        private CatalogVideoPlayback _videoPlayback;
        private readonly Vector3[] _videoCorners = new Vector3[4];
        private bool _applicationPaused;
        private bool _applicationFocused = true;

        private void Awake()
        {
            _verticalScroll = _verticalRoot != null ? _verticalRoot.GetComponentInChildren<ScrollRect>() : null;
            _settingsPopup = GetComponent<CatalogSettingsPopup>();
            _videoPlayback = new CatalogVideoPlayback(gameObject);
            if (_carousel != null)
                _carousel.FocusChanged += OnFocusChanged;
            if (_actionButton != null && _carousel != null)
                _actionButton.onClick.AddListener(_carousel.ActivateFocused);
            if (_secondaryActionButton != null)
                _secondaryActionButton.onClick.AddListener(ShowRestartConfirmation);
            if (_restartCancelButton != null)
                _restartCancelButton.onClick.AddListener(HideRestartConfirmation);
            if (_restartConfirmButton != null)
                _restartConfirmButton.onClick.AddListener(ConfirmRestart);
            HideRestartConfirmation();
            ApplySafeArea();
        }

        private void OnDestroy()
        {
            _videoPlayback?.Dispose();
            if (_carousel != null)
                _carousel.FocusChanged -= OnFocusChanged;
        }

        private void LateUpdate()
        {
            RefreshVideoPlayback();
            if (_appliedSafeArea != Screen.safeArea)
                ApplySafeArea();
            if (_verticalRoot == null || !_verticalRoot.activeSelf)
                return;
            var scroll = _verticalRoot.GetComponentInChildren<ScrollRect>();
            if (scroll == null || scroll.viewport == null || _verticalViewportSize == scroll.viewport.rect.size)
                return;
            _verticalViewportSize = scroll.viewport.rect.size;
            foreach (var card in _cards.Values)
                ConfigureVerticalCardHeight(card);
            RefreshStorySnapping(scroll);
        }

        private void RefreshVideoPlayback()
        {
            Card visible = null;
            if (!_applicationPaused && _applicationFocused && _verticalRoot != null && _verticalRoot.activeInHierarchy
                && _verticalScroll != null && _verticalScroll.viewport != null
                && (_settingsPopup == null || !_settingsPopup.IsOpen))
            {
                var area = 0f;
                var viewport = CatalogVideoPlayback.WorldRect(_verticalScroll.viewport, _videoCorners);
                foreach (var story in _cards.Values)
                    story.FindVisibleEpisode(viewport, _videoCorners, ref visible, ref area);
                if (visible != null && visible.RestartIsOpen) visible = null;
            }
            _videoPlayback?.Tick(visible);
        }

        private void OnEnable() => _applicationFocused = Application.isFocused;
        private void OnDisable() => _videoPlayback?.Stop();
        private void OnApplicationPause(bool paused)
        {
            _applicationPaused = paused;
            if (paused) _videoPlayback?.Stop();
        }
        private void OnApplicationFocus(bool focused)
        {
            _applicationFocused = focused;
            if (!focused) _videoPlayback?.Stop();
        }
        public void SetTitle(string text)
        {
            if (_title != null)
                _title.text = text ?? string.Empty;
        }

        public void AddOrUpdateCard(
            string id,
            string title,
            string genre,
            string description,
            string status,
            string actionLabel,
            string secondaryActionLabel,
            bool isEnabled,
            Sprite cover,
            IReadOnlyList<CatalogEpisodeItem> episodes,
            Action<CatalogEpisodeItem> onClick,
            Action<CatalogEpisodeItem> onSecondaryClick)
        {
            _cardPrefab.gameObject.SetActive(false);
            if (!_cards.TryGetValue(id, out var card))
            {
                card = Instantiate(_cardPrefab, _cardPrefab.transform.parent);
                _cards.Add(id, card);
            }

            var item = new CatalogItem(
                id,
                title,
                genre,
                description,
                status,
                actionLabel,
                secondaryActionLabel,
                isEnabled,
                cover,
                episodes);
            card.Bind(title, genre, description, status, cover);
            _models[card] = new ItemViewModel(
                actionLabel,
                secondaryActionLabel,
                isEnabled,
                null);
            card.gameObject.SetActive(true);
            if (_verticalRoot != null && _verticalRoot.activeSelf)
            {
                ConfigureVerticalCardHeight(card);
                card.BindStory(item, onClick, onSecondaryClick);
                RefreshStorySnapping(_verticalRoot.GetComponentInChildren<ScrollRect>());
                return;
            }
            _carousel.Register(
                card,
                isEnabled,
                () => onClick?.Invoke(item.Episodes.FirstOrDefault()));
        }

        private void RefreshStorySnapping(ScrollRect scroll)
        {
            if (scroll == null || scroll.viewport == null || _cards.Count == 0) return;
            var last = _cards.Values.OrderBy(card => card.transform.GetSiblingIndex()).Last();
            var layout = scroll.content.GetComponent<VerticalLayoutGroup>();
            if (layout != null)
                layout.padding.bottom = Mathf.CeilToInt(Mathf.Max(32f,
                    scroll.viewport.rect.height - last.RectTransform.rect.height - CatalogScrollSnap.StoryInset));
            LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
            CatalogScrollSnap.Ensure(scroll).SetItems(_cards.Values.Select(card => card.RectTransform));
        }

        private void ConfigureVerticalCardHeight(Card card)
        {
            var scrollRect = _verticalRoot.GetComponentInChildren<ScrollRect>();
            var viewport = scrollRect != null ? scrollRect.viewport : null;
            if (viewport == null || viewport.rect.height <= 0f)
                return;

            var layout = card.GetComponent<LayoutElement>();
            if (layout == null)
                return;

            // Preserve the episode's size while reserving the authored gap below its carousel.
            var episodeViewport = card.GetComponentInChildren<ScrollRect>(true)?.viewport;
            var bottomMargin = episodeViewport != null ? Mathf.Max(0f, episodeViewport.offsetMin.y) : 0f;
            layout.preferredHeight = Mathf.Max(540f, viewport.rect.height - 132f) + bottomMargin;
            card.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, layout.preferredHeight);
        }

        private void OnFocusChanged(Card card, int index, int count)
        {
            if (!_models.TryGetValue(card, out var model))
                return;
            _actionLabel.text = model.ActionLabel ?? string.Empty;
            _actionButton.interactable = model.CanOpen;
            _secondaryAction = model.SecondaryAction;
            _secondaryActionLabel.text = model.SecondaryActionLabel ?? string.Empty;
            _secondaryActionButton.gameObject.SetActive(
                model.CanOpen && !string.IsNullOrWhiteSpace(model.SecondaryActionLabel));
            HideRestartConfirmation();
            _pageIndicator.text = BuildPageIndicator(index, count);
        }

        private void ShowRestartConfirmation()
        {
            if (_secondaryAction != null)
                _restartConfirmation.SetActive(true);
        }

        private void HideRestartConfirmation() =>
            _restartConfirmation.SetActive(false);

        private void ConfirmRestart()
        {
            var action = _secondaryAction;
            HideRestartConfirmation();
            action?.Invoke();
        }

        private static string BuildPageIndicator(int focusedIndex, int count)
        {
            var indicators = new string[count];
            for (var index = 0; index < count; index++)
                indicators[index] = index == focusedIndex ? "●" : "○";
            return string.Join("  ", indicators);
        }

        private void ApplySafeArea()
        {
            var area = Screen.safeArea;
            _appliedSafeArea = area;
            var size = new Vector2(Screen.width, Screen.height);
            if (size.x <= 0f || size.y <= 0f)
                return;
            _safeArea.anchorMin = Vector2.Scale(area.position, new Vector2(1f / size.x, 1f / size.y));
            _safeArea.anchorMax = Vector2.Scale(area.position + area.size, new Vector2(1f / size.x, 1f / size.y));
            _safeArea.offsetMin = Vector2.zero;
            _safeArea.offsetMax = Vector2.zero;
        }

        private readonly struct ItemViewModel
        {
            internal ItemViewModel(
                string actionLabel,
                string secondaryActionLabel,
                bool canOpen,
                Action secondaryAction)
            {
                ActionLabel = actionLabel;
                SecondaryActionLabel = secondaryActionLabel;
                CanOpen = canOpen;
                SecondaryAction = secondaryAction;
            }

            internal string ActionLabel { get; }
            internal string SecondaryActionLabel { get; }
            internal bool CanOpen { get; }
            internal Action SecondaryAction { get; }
        }

    }
}
