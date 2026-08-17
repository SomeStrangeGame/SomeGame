using System;
using UnityEngine;
using UnityEngine.UI;

namespace Novels.Catalog.View
{
    public sealed class Card : MonoBehaviour
    {
        [SerializeField] private Text _title;
        [SerializeField] private Text _description;
        [SerializeField] private Button _button;

        public void Bind(string title, string description, Action onClick)
        {
            _title.text = title ?? string.Empty;
            _description.text = description ?? string.Empty;
            _description.gameObject.SetActive(
                !string.IsNullOrWhiteSpace(description));
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}
