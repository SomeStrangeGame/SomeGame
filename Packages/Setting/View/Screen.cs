using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Setting.View
{
    public class Screen : MonoBehaviour
    {
        [SerializeField] private Text _description;
        [SerializeField] private Button _buttonPrefab;

        private readonly Dictionary<string, Button> _buttons = new();

        public void SetDescription(string text)
        {
            _description.text = text;
        }

        public void AddOrUpdateButton(string id, string text, Action onClick)
        {
            _buttonPrefab.gameObject.SetActive(false);
            if (!_buttons.TryGetValue(id, out var button))
                button = Instantiate(_buttonPrefab, _buttonPrefab.transform.parent);

            _buttons[id] = button;
            button.GetComponentInChildren<Text>(true).text = text;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick.Invoke());
            button.gameObject.SetActive(true);
        }
    }
}

