using System;
using System.Collections.Generic;
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

        private readonly Dictionary<string, Card> _cards = new();
        private readonly Dictionary<Card, ItemViewModel> _models = new();
        private Rect _appliedSafeArea;

        private void Awake()
        {
            _carousel.FocusChanged += OnFocusChanged;
            _actionButton.onClick.AddListener(_carousel.ActivateFocused);
            ApplySafeArea();
        }

        private void OnDestroy()
        {
            if (_carousel != null)
                _carousel.FocusChanged -= OnFocusChanged;
        }

        private void LateUpdate()
        {
            if (_appliedSafeArea != Screen.safeArea)
                ApplySafeArea();
        }
        public void SetTitle(string text)
        {
            _title.text = text ?? string.Empty;
        }

        public void AddOrUpdateCard(
            string id,
            string title,
            string genre,
            string description,
            string status,
            string actionLabel,
            bool isEnabled,
            Sprite cover,
            Action onClick)
        {
            _cardPrefab.gameObject.SetActive(false);
            if (!_cards.TryGetValue(id, out var card))
            {
                card = Instantiate(_cardPrefab, _cardPrefab.transform.parent);
                _cards.Add(id, card);
            }

            card.Bind(title, genre, description, status, cover);
            _models[card] = new ItemViewModel(actionLabel, isEnabled);
            card.gameObject.SetActive(true);
            _carousel.Register(card, isEnabled, onClick);
        }

        private void OnFocusChanged(Card card, int index, int count)
        {
            if (!_models.TryGetValue(card, out var model))
                return;
            _actionLabel.text = model.ActionLabel ?? string.Empty;
            _actionButton.interactable = model.CanOpen;
            _pageIndicator.text = BuildPageIndicator(index, count);
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
            internal ItemViewModel(string actionLabel, bool canOpen)
            {
                ActionLabel = actionLabel;
                CanOpen = canOpen;
            }

            internal string ActionLabel { get; }
            internal bool CanOpen { get; }
        }

    }
}
