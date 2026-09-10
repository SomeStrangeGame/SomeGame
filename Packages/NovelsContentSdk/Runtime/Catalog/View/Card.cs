using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Novels.Catalog.View
{
    public sealed class Card : MonoBehaviour
    {
        [SerializeField] private Text _title;
        [SerializeField] private Text _description;
        [SerializeField] private Text _status;
        [SerializeField] private Button _button;
        [SerializeField] private Text _buttonLabel;
        [SerializeField] private Image _cover;
        [SerializeField] private Text _author;
        [SerializeField] private RawImage _video;
        [Header("Episode reading progress")]
        [SerializeField] private GameObject _readingProgress;
        [SerializeField] private Image _readingProgressFill;
        [SerializeField] private Text _readingProgressPercent;
        [Header("Story section")]
        [SerializeField] private RectTransform _episodesContainer;
        [SerializeField] private Card _episodePrefab;
        [SerializeField] private Image _storyProgressFill;
        [SerializeField] private Text _episodeLabel;
        [SerializeField] private Image _stateIcon;
        [SerializeField] private Sprite _lockedIcon;
        [SerializeField] private Sprite _completedIcon;
        [Header("Episode restart")]
        [SerializeField] private Button _restartButton;
        [SerializeField] private GameObject _restartConfirmation;
        [SerializeField] private Text _restartWarning;
        [SerializeField] private Button _restartCancelButton;
        [SerializeField] private Button _restartConfirmButton;

        private CanvasGroup _canvasGroup;
        private readonly List<Card> _episodeCards = new();
        private float _episodeViewportWidth = -1f;
        private CatalogEpisodeItem _boundEpisode;
        private ScrollRect _episodeScroll;

        internal string VideoUrl => _boundEpisode?.VideoUrl;
        internal bool RestartIsOpen => _restartConfirmation != null && _restartConfirmation.activeSelf;
        internal bool HasVideoSurface => _video != null;
        internal string EpisodeId => _boundEpisode?.Id;

        internal void SetVideoTexture(Texture texture)
        {
            if (_video == null) return;
            _video.texture = texture;
            _video.enabled = texture != null;
            if (texture == null) return;
            var size = _video.rectTransform.rect.size;
            var aspect = (float)texture.width / Mathf.Max(1, texture.height);
            var targetAspect = size.x / Mathf.Max(1, size.y);
            // Cover crop: preserve proportions without resizing/rebuilding the authored UI.
            _video.uvRect = aspect > targetAspect
                ? new Rect((1f - targetAspect / aspect) / 2f, 0f, targetAspect / aspect, 1f)
                : new Rect(0f, (1f - aspect / targetAspect) / 2f, 1f, aspect / targetAspect);
        }

        internal void FindVisibleEpisode(Rect outer, Vector3[] corners, ref Card best, ref float bestArea)
        {
            if (_episodeScroll == null || !gameObject.activeInHierarchy) return;
            var viewport = CatalogVideoPlayback.Intersection(outer,
                CatalogVideoPlayback.WorldRect(_episodeScroll.viewport, corners));
            foreach (var episode in _episodeCards)
            {
                if (!episode.isActiveAndEnabled) continue;
                var rect = CatalogVideoPlayback.WorldRect(episode.RectTransform, corners);
                var visible = CatalogVideoPlayback.Intersection(viewport, rect);
                var area = visible.width * visible.height;
                if (area > bestArea && area >= rect.width * rect.height * .5f)
                {
                    best = episode;
                    bestArea = area;
                }
            }
        }

        private void OnDestroy()
        {
            if (_boundEpisode?.Download != null) _boundEpisode.Download.Changed -= RefreshDownload;
        }

        private void RefreshDownload()
        {
            if (_boundEpisode == null) return;
            var download = _boundEpisode.Download;
            var ready = download == null || download.IsReady;
            _button.gameObject.SetActive(!ready || !string.IsNullOrWhiteSpace(_boundEpisode.ActionLabel));
            _button.interactable = ready ? _boundEpisode.IsEnabled
                : download.Status == CatalogDownloadStatus.Failed;
            if (_buttonLabel != null)
                _buttonLabel.text = ready ? _boundEpisode.ActionLabel : download.Status switch
                {
                    CatalogDownloadStatus.Downloading => $"Загрузка · {Mathf.Min(99, Mathf.FloorToInt(download.Progress * 100f))}%",
                    CatalogDownloadStatus.Failed => "Ошибка загрузки · Повторить",
                    _ => "Ожидает загрузки",
                };
            if (_restartButton != null)
                _restartButton.interactable = ready && !string.IsNullOrWhiteSpace(_boundEpisode.RestartLabel);
        }

        public RectTransform RectTransform => (RectTransform)transform;

        public void Bind(
            string title,
            string genre,
            string description,
            string status,
            Sprite cover)
        {
            _cover ??= GetComponent<Image>();
            if (_cover != null)
            {
                if (cover != null)
                    _cover.sprite = cover;
                var crop = _cover.GetComponent<AspectRatioFitter>();
                _cover.preserveAspect = crop == null;
                if (crop != null && _cover.sprite != null)
                    crop.aspectRatio = _cover.sprite.rect.width / _cover.sprite.rect.height;
            }
            _title.text = title ?? string.Empty;
            _description.text = description ?? string.Empty;
            _description.gameObject.SetActive(
                !string.IsNullOrWhiteSpace(description));
            _status.text = JoinMetadata(genre, status);
            _status.gameObject.SetActive(!string.IsNullOrWhiteSpace(_status.text));
        }

        public void SetClick(Action onClick)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick?.Invoke());
            _button.interactable = true;
        }

        public void BindStory(
            CatalogItem story,
            Action<CatalogEpisodeItem> onOpen,
            Action<CatalogEpisodeItem> onRestart)
        {
            if (story == null)
                throw new ArgumentNullException(nameof(story));
            Bind(
                story.Title,
                story.Genre,
                BuildStoryProgress(story.Episodes),
                story.Status,
                story.Cover);
            if (_storyProgressFill != null)
                _storyProgressFill.fillAmount = story.Episodes.Count == 0 ? 0f
                    : (float)story.Episodes.Count(IsCompleted) / story.Episodes.Count;
            if (_episodesContainer == null || _episodePrefab == null)
            {
                var episode = story.Episodes.FirstOrDefault();
                SetClick(() =>
                {
                    if (episode != null && episode.IsEnabled)
                        onOpen?.Invoke(episode);
                });
                return;
            }

            foreach (var card in _episodeCards)
            {
                if (card != null)
                {
                    card.gameObject.SetActive(false);
                    Destroy(card.gameObject);
                }
            }
            _episodeCards.Clear();
            _episodePrefab.gameObject.SetActive(false);
            var nestedScroll = _episodesContainer.GetComponentInParent<ScrollRect>();
            _episodeScroll = nestedScroll;
            var outerScroll = FindOuterVerticalScroll(nestedScroll);
            foreach (var episode in story.Episodes)
            {
                var card = Instantiate(_episodePrefab, _episodesContainer);
                card.BindEpisode(story.Cover, episode, onOpen, onRestart);
                if (card._episodeLabel != null)
                    card._episodeLabel.text = $"ЭПИЗОД {_episodeCards.Count + 1:00}";
                card.ConfigureNestedScrolling(nestedScroll, outerScroll);
                card.gameObject.SetActive(true);
                _episodeCards.Add(card);
            }
            _episodeViewportWidth = -1f;
            RefreshEpisodeLayout();
        }

        private void LateUpdate() => RefreshEpisodeLayout();

        internal void RefreshEpisodeLayout()
        {
            if (_episodesContainer == null || _episodeCards.Count == 0)
                return;
            var scroll = _episodesContainer.GetComponentInParent<ScrollRect>();
            if (scroll == null || scroll.viewport == null)
                return;
            var width = scroll.viewport.rect.width;
            if (width <= 0f || Mathf.Abs(width - _episodeViewportWidth) < 0.1f)
                return;
            var normalized = scroll.horizontalNormalizedPosition;
            _episodeViewportWidth = width;
            // Leave a real next-card peek after the inter-card gap, on every aspect ratio.
            var cardWidth = Mathf.Max(1f, width - 32f);
            var layout = _episodesContainer.GetComponent<HorizontalLayoutGroup>();
            // The last episode needs the same left alignment as all preceding cards.
            if (layout != null) layout.padding.right = Mathf.CeilToInt(width - cardWidth);
            var spacing = layout != null ? layout.spacing : 12f;
            var padding = layout != null ? layout.padding.horizontal : 0f;
            foreach (var card in _episodeCards)
            {
                card.GetComponent<LayoutElement>().preferredWidth = cardWidth;
                card.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cardWidth);
            }
            _episodesContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                _episodeCards.Count * cardWidth + (_episodeCards.Count - 1) * spacing + padding);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_episodesContainer);
            scroll.horizontalNormalizedPosition = Mathf.Clamp01(normalized);
            CatalogScrollSnap.Ensure(scroll).SetItems(_episodeCards.Select(card => card.RectTransform));
        }

        internal void FocusEpisode(string episodeId)
        {
            if (string.IsNullOrWhiteSpace(episodeId) || _episodeScroll == null)
                return;
            RefreshEpisodeLayout();
            var episode = _episodeCards.FirstOrDefault(card => string.Equals(
                card.EpisodeId,
                episodeId,
                StringComparison.OrdinalIgnoreCase));
            if (episode != null)
                CatalogScrollSnap.Ensure(_episodeScroll).Focus(episode.RectTransform);
        }

        private void ConfigureNestedScrolling(
            ScrollRect horizontalScroll,
            ScrollRect verticalScroll)
        {
            var router = GetComponent<AxisRoutedScrollDrag>();
            if (router == null)
                router = gameObject.AddComponent<AxisRoutedScrollDrag>();
            router.Configure(horizontalScroll, verticalScroll);
        }

        private static ScrollRect FindOuterVerticalScroll(ScrollRect nestedScroll)
        {
            var parent = nestedScroll != null ? nestedScroll.transform.parent : null;
            while (parent != null)
            {
                var candidate = parent.GetComponent<ScrollRect>();
                if (candidate != null && candidate.vertical)
                    return candidate;
                parent = parent.parent;
            }
            return null;
        }

        private void BindEpisode(
            Sprite cover,
            CatalogEpisodeItem episode,
            Action<CatalogEpisodeItem> onOpen,
            Action<CatalogEpisodeItem> onRestart)
        {
            if (_boundEpisode?.Download != null) _boundEpisode.Download.Changed -= RefreshDownload;
            _boundEpisode = episode;
            if (_readingProgress != null)
                _readingProgress.SetActive(episode.IsEnabled || IsCompleted(episode));
            var reading = IsCompleted(episode) ? 1f : episode.ReadingProgress;
            if (_readingProgressFill != null)
                _readingProgressFill.fillAmount = IsCompleted(episode) ? 1f
                    : Mathf.Min(.99f, reading ?? 0f);
            if (_readingProgressPercent != null)
                _readingProgressPercent.text = IsCompleted(episode) ? "100%"
                    : !reading.HasValue ? "—"
                    : reading.Value <= 0f ? "0%"
                    : $"≈{Mathf.Min(99, Mathf.FloorToInt(reading.Value * 100f))}%";
            SetVideoTexture(null);
            if (episode.Download != null) episode.Download.Changed += RefreshDownload;
            Bind(episode.Title, string.Empty, episode.Description, episode.Status, episode.Cover != null ? episode.Cover : cover);
            if (_author != null)
            {
                _author.supportRichText = false;
                _author.text = episode.Author;
                _author.gameObject.SetActive(!string.IsNullOrWhiteSpace(episode.Author));
            }
            if (_stateIcon != null)
            {
                var stateIcon = IsCompleted(episode) ? _completedIcon : _lockedIcon;
                _stateIcon.sprite = stateIcon;
                _stateIcon.gameObject.SetActive(stateIcon != null &&
                    (IsCompleted(episode) || !episode.IsEnabled));
            }
            _button.onClick.RemoveAllListeners();
            _button.gameObject.SetActive(!string.IsNullOrWhiteSpace(episode.ActionLabel));
            _button.interactable = episode.IsEnabled;
            if (_buttonLabel != null)
                _buttonLabel.text = episode.ActionLabel;
            _button.onClick.AddListener(() =>
            {
                if (episode.Download != null && !episode.Download.IsReady) episode.Download.Retry();
                else if (episode.IsEnabled) onOpen?.Invoke(episode);
            });
            if (_restartConfirmation != null)
                _restartConfirmation.SetActive(false);
            if (_restartButton == null)
            {
                RefreshDownload();
                return;
            }
            var canRestart = !string.IsNullOrWhiteSpace(episode.RestartLabel);
            // Keep the labelled control discoverable on unread episodes, but do
            // not offer a destructive action when there is nothing to reset.
            _restartButton.gameObject.SetActive(episode.IsEnabled || IsCompleted(episode));
            _restartButton.interactable = canRestart;
            var restartGroup = _restartButton.GetComponent<CanvasGroup>();
            if (restartGroup != null)
                restartGroup.alpha = canRestart ? 1f : 0.4f;
            _restartButton.onClick.RemoveAllListeners();
            _restartButton.onClick.AddListener(() =>
            {
                if (canRestart && (episode.Download == null || episode.Download.IsReady)
                    && _restartConfirmation != null)
                {
                    foreach (var snap in GetComponentsInParent<CatalogScrollSnap>()) snap.Cancel();
                    _restartConfirmation.SetActive(true);
                }
            });
            if (_restartWarning != null)
                _restartWarning.text = "Начать заново?\n\n" + episode.RestartWarning;
            if (_restartCancelButton != null)
            {
                _restartCancelButton.onClick.RemoveAllListeners();
                _restartCancelButton.onClick.AddListener(() =>
                    _restartConfirmation.SetActive(false));
            }
            if (_restartConfirmButton != null)
            {
                _restartConfirmButton.onClick.RemoveAllListeners();
                // Fail closed when an outdated catalog has no hold control.
                _restartConfirmButton.GetComponent<HoldToConfirm>()?.Configure(() =>
                {
                    if (!canRestart || (episode.Download != null && !episode.Download.IsReady)) return;
                    _restartConfirmation.SetActive(false);
                    onRestart?.Invoke(episode);
                });
            }
            RefreshDownload();
        }

        private static string BuildStoryProgress(
            IReadOnlyList<CatalogEpisodeItem> episodes)
        {
            var complete = episodes.Count(episode => string.Equals(
                episode.Status,
                "Завершено",
                StringComparison.OrdinalIgnoreCase));
            return $"{complete} из {episodes.Count} эпизодов";
        }

        private static bool IsCompleted(CatalogEpisodeItem episode) =>
            string.Equals(episode.Status, "Завершено", StringComparison.OrdinalIgnoreCase);

        public void SetFocus(float scale, float opacity)
        {
            transform.localScale = Vector3.one * scale;
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = opacity;
        }

        private static string JoinMetadata(string genre, string status)
        {
            if (string.IsNullOrWhiteSpace(genre))
                return status ?? string.Empty;
            if (string.IsNullOrWhiteSpace(status))
                return genre;
            return $"{genre} · {status}";
        }
    }

    internal sealed class AxisRoutedScrollDrag : MonoBehaviour,
        IInitializePotentialDragHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IScrollHandler
    {
        private ScrollRect _horizontal;
        private ScrollRect _vertical;
        private ScrollRect _active;

        internal void Configure(ScrollRect horizontal, ScrollRect vertical)
        {
            _horizontal = horizontal;
            _vertical = vertical;
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            _horizontal?.OnInitializePotentialDrag(eventData);
            _vertical?.OnInitializePotentialDrag(eventData);
            _horizontal?.GetComponent<CatalogScrollSnap>()?.OnInitializePotentialDrag(eventData);
            _vertical?.GetComponent<CatalogScrollSnap>()?.OnInitializePotentialDrag(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _active = Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y)
                ? _horizontal
                : _vertical;
            _active?.OnBeginDrag(eventData);
            _active?.GetComponent<CatalogScrollSnap>()?.OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData) =>
            _active?.OnDrag(eventData);

        public void OnEndDrag(PointerEventData eventData)
        {
            _active?.OnEndDrag(eventData);
            _active?.GetComponent<CatalogScrollSnap>()?.OnEndDrag(eventData);
            _active = null;
        }

        public void OnScroll(PointerEventData eventData)
        {
            var target = Mathf.Abs(eventData.scrollDelta.x)
                > Mathf.Abs(eventData.scrollDelta.y)
                ? _horizontal
                : _vertical;
            target?.OnScroll(eventData);
            target?.GetComponent<CatalogScrollSnap>()?.OnScroll(eventData);
        }
    }
}
