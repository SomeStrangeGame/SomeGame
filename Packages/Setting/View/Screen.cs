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
        private Font _runtimeFont;

        private void Awake()
        {
            _runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ApplyRuntimeFont(_description);
            ApplyRuntimeFont(_buttonPrefab.GetComponentInChildren<Text>(true));
        }

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
            var label = button.GetComponentInChildren<Text>(true);
            ApplyRuntimeFont(label);
            label.text = text;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick.Invoke());
            button.gameObject.SetActive(true);
        }

        private void ApplyRuntimeFont(Text label)
        {
            if (label != null && _runtimeFont != null)
                label.font = _runtimeFont;
        }
    }
}
