using System;
using UnityEngine;
using UnityEngine.UI;

namespace Novels.Catalog.View
{
    public sealed class Card : MonoBehaviour
    {
        [SerializeField] private Text _title;
        [SerializeField] private Text _description;
        [SerializeField] private Text _status;
        [SerializeField] private Button _button;
        [SerializeField] private Image _cover;

        public void Bind(
            string title,
            string description,
            string status,
            bool isEnabled,
            Sprite cover,
            Action onClick)
        {
            _cover ??= GetComponent<Image>();
            if (_cover != null)
            {
                _cover.sprite = cover;
                _cover.preserveAspect = cover != null;
            }
            _title.text = title ?? string.Empty;
            _description.text = description ?? string.Empty;
            _description.gameObject.SetActive(
                !string.IsNullOrWhiteSpace(description));
            _status.text = status ?? string.Empty;
            _status.gameObject.SetActive(!string.IsNullOrWhiteSpace(status));
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick?.Invoke());
            _button.interactable = isEnabled;
        }
    }
}
