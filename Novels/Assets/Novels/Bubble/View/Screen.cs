using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Novels.Bubble.View
{
    public class Screen : MonoBehaviour
    {
        public struct BubbleCtx
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
                Hint,
                LeftMinds,
            }

            public struct TextCtx
            {
                public string Header;
                public string Text;
            }

            public struct ButtonCtx
            {
                public int Id;
                public string Text;
                public Action<int> OnClick;
            }

            public BubbleType Type;
            public TextCtx Text;
            public ButtonCtx[] Buttons;
            public Action OnBackgroundClick;
        }

        [Serializable]
        private struct BubblePopUp
        {
            [SerializeField] BubbleCtx.BubbleType _type;
            [SerializeField] private GameObject _root;
            [SerializeField] private Text _header;
            [SerializeField] private Text _text;
            [SerializeField] private GameObject[] _extraObjects;

            internal readonly void SetText(string header, string description)
            {
                if (string.IsNullOrEmpty(description))
                {
                    description = header;
                    header = string.Empty;
                }

                _header.text = header;
                _text.text = description;

                _header.gameObject.SetActive(!string.IsNullOrEmpty(header));
                _text.gameObject.SetActive(!string.IsNullOrEmpty(description));
                foreach(var extraObject in _extraObjects)
                    extraObject.SetActive(!string.IsNullOrEmpty(description));
            }

            internal readonly bool IsCorrectType(BubbleCtx.BubbleType type) 
            {
                var result = _type == type;
                _root.SetActive(result);
                return result;
            }

            internal readonly bool TryGetRoot(BubbleCtx.BubbleType type, out GameObject root)
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

        public void SetBubbleScreen(BubbleCtx ctx)
        {
            foreach (var bubble in _bubbles)
            {
                bubble.IsCorrectType(ctx.Type);
                bubble.SetText(ctx.Text.Header, ctx.Text.Text);
            }

            foreach(var buttonPair in _buttons)
                Destroy(buttonPair.Value.gameObject);
            _buttons.Clear();

            foreach(var button in ctx.Buttons)
            {
                GameObject root = null;
                foreach (var bubble in _bubbles)
                    if (bubble.TryGetRoot(ctx.Type, out root)) break;
                    
                _buttonPrefab.gameObject.SetActive(false);
                if (!_buttons.TryGetValue(button.Id, out var inSceneButton))
                    inSceneButton = Instantiate(_buttonPrefab, root.transform);

                _buttons[button.Id] = inSceneButton;
                inSceneButton.GetComponentInChildren<Text>(true).text = button.Text;
                inSceneButton.onClick.RemoveAllListeners();
                inSceneButton.onClick.AddListener(() => button.OnClick.Invoke(button.Id));
                inSceneButton.gameObject.SetActive(true);
            }
            _backgroundButton.onClick.RemoveAllListeners();
            _backgroundButton.onClick.AddListener(() => ctx.OnBackgroundClick?.Invoke());
        }
    }
}

