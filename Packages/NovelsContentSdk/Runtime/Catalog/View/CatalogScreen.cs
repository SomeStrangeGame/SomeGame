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
        private Button _secondaryActionButton;
        private Text _secondaryActionLabel;
        private Action _secondaryAction;
        private Rect _appliedSafeArea;

        private void Awake()
        {
            _carousel.FocusChanged += OnFocusChanged;
            _actionButton.onClick.AddListener(_carousel.ActivateFocused);
            CreateSecondaryActionButton();
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
            string secondaryActionLabel,
            bool isEnabled,
            Sprite cover,
            Action onClick,
            Action onSecondaryClick)
        {
            _cardPrefab.gameObject.SetActive(false);
            if (!_cards.TryGetValue(id, out var card))
            {
                card = Instantiate(_cardPrefab, _cardPrefab.transform.parent);
                _cards.Add(id, card);
            }

            card.Bind(title, genre, description, status, cover);
            _models[card] = new ItemViewModel(
                actionLabel,
                secondaryActionLabel,
                isEnabled,
                onSecondaryClick);
            card.gameObject.SetActive(true);
            _carousel.Register(card, isEnabled, onClick);
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
            _pageIndicator.text = BuildPageIndicator(index, count);
        }

        private void CreateSecondaryActionButton()
        {
            _secondaryActionButton = Instantiate(
                _actionButton,
                _actionButton.transform.parent);
            _secondaryActionButton.name = "SecondaryActionButton";
            _secondaryActionButton.onClick.RemoveAllListeners();
            _secondaryActionButton.onClick.AddListener(() => _secondaryAction?.Invoke());
            _secondaryActionLabel = _secondaryActionButton.GetComponentInChildren<Text>();
            if (_secondaryActionLabel == null)
                throw new InvalidOperationException("Catalog action button has no Text label.");
            _secondaryActionButton.gameObject.SetActive(false);
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
