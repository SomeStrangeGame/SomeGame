using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Novels.Bubble.View
{
    public class Screen : MonoBehaviour
    {
        public struct BubbleCtx
        {
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
        private struct Bubbles
        {
            [Serializable]
            internal struct BubblePopUp
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

            [SerializeField] private GameObject _root;
            [SerializeField] private BubblePopUp[] _bubbles;
            [SerializeField] private Button _buttonPrefab;
            [SerializeField] private Button _backgroundButton;

            public readonly GameObject Root => _root;
            public readonly BubblePopUp[] BubblesPopUp => _bubbles;
            public readonly Button ButtonPrefab => _buttonPrefab;
            public readonly Button BackgroundButton => _backgroundButton;
        }

        [SerializeField] private Bubbles _bubblesView;

        [SerializeField] private float _showHideDuration;
        [SerializeField] private CanvasGroup _canvasGroup;

        private readonly List<Button> _buttonPool = new();

        public void ShowImmediate()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.gameObject.SetActive(true);
        }

        public async UniTask Show(CancellationToken cancellationToken)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(true);
            await global::UITransitions.Transition.Fade(
                _canvasGroup,
                0f,
                1f,
                _showHideDuration,
                cancellationToken);
        }

        public void HideImmediate()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(false);
        }

        public async UniTask Hide(CancellationToken cancellationToken)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.gameObject.SetActive(true);

            await global::UITransitions.Transition.Fade(
                _canvasGroup,
                1f,
                0f,
                _showHideDuration,
                cancellationToken);
            _canvasGroup.gameObject.SetActive(false);
        }

        public void SetBubbleScreen(BubbleCtx ctx)
        {
            _bubblesView.Root.SetActive(true);

            foreach (var bubble in _bubblesView.BubblesPopUp)
            {
                bubble.IsCorrectType(ctx.Type);
                bubble.SetText(ctx.Text.Header, ctx.Text.Text);
            }

            GameObject root = null;
            foreach (var bubble in _bubblesView.BubblesPopUp)
            {
                if (bubble.TryGetRoot(ctx.Type, out root))
                    break;
            }

            var buttons = ctx.Buttons ?? Array.Empty<BubbleCtx.ButtonCtx>();
            for (var index = 0; index < buttons.Length; index++)
            {
                var button = buttons[index];
                _bubblesView.ButtonPrefab.gameObject.SetActive(false);
                if (index >= _buttonPool.Count)
                    _buttonPool.Add(Instantiate(_bubblesView.ButtonPrefab, root.transform));

                var inSceneButton = _buttonPool[index];
                inSceneButton.transform.SetParent(root.transform, false);
                inSceneButton.GetComponentInChildren<Text>(true).text = button.Text;
                inSceneButton.onClick.RemoveAllListeners();
                inSceneButton.onClick.AddListener(() => button.OnClick.Invoke(button.Id));
                inSceneButton.gameObject.SetActive(true);
            }

            for (var index = buttons.Length; index < _buttonPool.Count; index++)
            {
                _buttonPool[index].onClick.RemoveAllListeners();
                _buttonPool[index].gameObject.SetActive(false);
            }
            BindBackground(ctx.OnBackgroundClick);
        }

        private void BindBackground(Action onClick)
        {
            _bubblesView.BackgroundButton.onClick.RemoveAllListeners();
            _bubblesView.BackgroundButton.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}
