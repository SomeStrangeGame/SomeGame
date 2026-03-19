using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Novels.Bubble.View
{
    public class Screen : MonoBehaviour
    {
        public enum TextAlign
        {
            Left,
            Center,
            Right
        }

        public enum BubbleType
        {
            NoCharacter,
            LeftCharacter,
            RightCharacter,
        }

        [Serializable]
        private struct BubblePopUp
        {
            [SerializeField] BubbleType _type;
            [SerializeField] private GameObject _root;
            [SerializeField] private Text _header;
            [SerializeField] private Text _text;

            internal readonly void SetText(string header, string description)
            {
                _header.text = header;
                _text.text = description;

                _header.gameObject.SetActive(!string.IsNullOrEmpty(header));
                _text.gameObject.SetActive(!string.IsNullOrEmpty(description));
            }

            internal readonly bool IsCorrectType(BubbleType type) 
            {
                var result = _type == type;
                _root.SetActive(result);
                return result;
            }

            internal bool TryGetRoot(BubbleType type, out GameObject root)
            {
                root = null;
                var result = type == _type;
                if (result) root = _root;
                return result;
            }
        }

        [SerializeField] private BubblePopUp[] _bubbles;
        [SerializeField] private Button _buttonPrefab;
        [SerializeField] private Button _backgroundButton;
        [SerializeField] private float _showHideDuration;
        [SerializeField] private CanvasGroup _canvasGroup;

        private readonly Dictionary<int, Button> _buttons = new();

        public void ShowImmediate()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.gameObject.SetActive(true);
        }

        public async UniTask Show()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(true);

            var timer = _showHideDuration;
            while (timer >= 0f)
            {
                _canvasGroup.alpha = 1f - (timer / _showHideDuration);
                timer -= Time.deltaTime;
                await UniTask.Yield();
            }

            _canvasGroup.alpha = 1f;
        }

        public void HideImmediate()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(false);
        }

        public async UniTask Hide()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.gameObject.SetActive(true);

            var timer = _showHideDuration;
            while (timer >= 0f)
            {
                _canvasGroup.alpha = timer / _showHideDuration;
                timer -= Time.deltaTime;
                await UniTask.Yield();
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(false);
        }

        public void SetText(BubbleType bubbleType, string header, string text)
        {
            foreach (var bubble in _bubbles)
            {
                bubble.IsCorrectType(bubbleType);
                bubble.SetText(header, text);
            }
        }

        public void AddOrUpdateButton(int id, BubbleType bubbleType, string text, Action<int> onClick)
        {
            GameObject root = null;
            foreach (var bubble in _bubbles)
                if (bubble.TryGetRoot(bubbleType, out root)) break;
                
            _buttonPrefab.gameObject.SetActive(false);
            if (!_buttons.TryGetValue(id, out var button))
                button = Instantiate(_buttonPrefab, root.transform);

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

        public void SetBackgroundButton(Action onClick)
        {
            ResetBackgroundButton();
            _backgroundButton.onClick.AddListener(() => onClick.Invoke());
        }

        public void ResetBackgroundButton()
        {
            _backgroundButton.onClick.RemoveAllListeners();
        }
    }
}

