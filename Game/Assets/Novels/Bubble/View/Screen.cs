using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Novels.Bubble.View
{
    public class Screen : MonoBehaviour
    {
        [SerializeField] private Text _text;
        [SerializeField] private Button _buttonPrefab;

        private readonly Dictionary<int, Button> _buttons = new();

        public void ShowImmediate()
        {
            gameObject.SetActive(true);
        }

        public void HideImmediate()
        {
            gameObject.SetActive(false);
        }

        public async UniTask Show()
        {
            gameObject.SetActive(true);
        }

        public async UniTask Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetText(string text)
        {
            _text.text = text;
        }

        public void AddOrUpdateButton(int id, string text, Action<int> onClick)
        {
            _buttonPrefab.gameObject.SetActive(false);
            if (!_buttons.TryGetValue(id, out var button))
                button = Instantiate(_buttonPrefab, _buttonPrefab.transform.parent);

            _buttons[id] = button;
            button.GetComponentInChildren<Text>(true).text = text;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick.Invoke(id));
            button.gameObject.SetActive(true);
        }

        public void RemoveAllButtons()
        {
            foreach(var buttonPair in _buttons)
                Destroy(buttonPair.Value.gameObject);

            _buttons.Clear();
        }

        public void RemoveButton(int id)
        {
            if (!_buttons.TryGetValue(id, out var button)) return;

            Destroy(button.gameObject);
            _buttons.Remove(id);
        }
    }
}

