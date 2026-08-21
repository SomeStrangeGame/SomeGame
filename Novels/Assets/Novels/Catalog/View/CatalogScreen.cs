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

        private readonly Dictionary<string, Card> _cards = new();

        public void SetTitle(string text)
        {
            _title.text = text ?? string.Empty;
        }

        public void AddOrUpdateCard(
            string id,
            string title,
            string description,
            string status,
            bool isEnabled,
            Action onClick)
        {
            _cardPrefab.gameObject.SetActive(false);
            if (!_cards.TryGetValue(id, out var card))
            {
                card = Instantiate(_cardPrefab, _cardPrefab.transform.parent);
                _cards.Add(id, card);
            }

            card.Bind(title, description, status, isEnabled, onClick);
            card.gameObject.SetActive(true);
        }
    }
}
