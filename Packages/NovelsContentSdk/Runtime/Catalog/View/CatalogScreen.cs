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

        private readonly Dictionary<string, Card> _cards = new();
        private Button _secondaryButton;
        private Text _secondaryButtonText;

        public void SetTitle(string text)
        {
            _title.text = text ?? string.Empty;
        }

        public void SetSecondaryAction(
            string text,
            bool isInteractable,
            Action onClick)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                if (_secondaryButton != null)
                    _secondaryButton.gameObject.SetActive(false);
                return;
            }
            EnsureSecondaryButton();
            _secondaryButtonText.text = text;
            _secondaryButton.interactable = isInteractable;
            _secondaryButton.onClick.RemoveAllListeners();
            _secondaryButton.onClick.AddListener(() => onClick?.Invoke());
            _secondaryButton.gameObject.SetActive(true);
        }

        public void AddOrUpdateCard(
            string id,
            string title,
            string description,
            string status,
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

            card.Bind(title, description, status, cover);
            card.gameObject.SetActive(true);
            _carousel.Register(card, isEnabled, onClick);
        }

        private void EnsureSecondaryButton()
        {
            if (_secondaryButton != null)
                return;
            var buttonObject = new GameObject(
                "Download All",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            buttonObject.transform.SetParent(transform, false);
            var rect = (RectTransform)buttonObject.transform;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 28f);
            rect.sizeDelta = new Vector2(340f, 54f);
            buttonObject.GetComponent<Image>().color = new Color(0.12f, 0.55f, 0.85f, 0.96f);
            _secondaryButton = buttonObject.GetComponent<Button>();

            var textObject = new GameObject(
                "Text",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);
            var textRect = (RectTransform)textObject.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(12f, 4f);
            textRect.offsetMax = new Vector2(-12f, -4f);
            _secondaryButtonText = textObject.GetComponent<Text>();
            _secondaryButtonText.font = _title.font;
            _secondaryButtonText.fontSize = 20;
            _secondaryButtonText.alignment = TextAnchor.MiddleCenter;
            _secondaryButtonText.color = Color.white;
            _secondaryButtonText.raycastTarget = false;
        }
    }
}
