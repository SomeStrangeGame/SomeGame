using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Novels.Bubble.View
{
    public class BubbleScreen : MonoBehaviour
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

                internal readonly void RebuildContentLayout()
                {
                    ResizeToPreferredHeight(_header);
                    ResizeToPreferredHeight(_text);
                }

                internal readonly void SetTextSize(bool fit, int fontSize)
                {
                    _text.resizeTextForBestFit = fit;
                    _text.fontSize = fontSize;
                    if (fit)
                    {
                        _text.resizeTextMinSize = fontSize;
                        _text.resizeTextMaxSize = fontSize;
                    }
                }

                private static void ResizeToPreferredHeight(Text text)
                {
                    var rectTransform = text.rectTransform;
                    rectTransform.SetSizeWithCurrentAnchors(
                        RectTransform.Axis.Vertical,
                        text.preferredHeight);
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
        [SerializeField] private bool _forceLayoutRebuildAfterContentChange;
        [SerializeField] private int _dialogueTextSize = 32;
        [SerializeField] private int _episodeEndTextSize = 24;
        [SerializeField] private float _episodeEndButtonYOffset = -120f;

        private readonly List<Button> _buttonPool = new();
        private Button _wardrobeButton;

        public void ConfigureWardrobe(Action openWardrobe)
        {
            if (openWardrobe == null || _wardrobeButton != null)
                return;
            _wardrobeButton = Instantiate(_bubblesView.ButtonPrefab, _canvasGroup.transform);
            _wardrobeButton.name = "WardrobeButton";
            _wardrobeButton.GetComponentInChildren<Text>(true).text = "Гардероб";
            var rect = _wardrobeButton.transform as RectTransform;
            if (rect != null)
            {
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.anchoredPosition = new Vector2(-24f, -24f);
            }
            _wardrobeButton.onClick.RemoveAllListeners();
            _wardrobeButton.onClick.AddListener(() => openWardrobe());
            _wardrobeButton.gameObject.SetActive(false);
        }

        public void SetWardrobeAvailable(bool available)
        {
            if (_wardrobeButton != null)
                _wardrobeButton.gameObject.SetActive(available);
        }

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
            var episodeEnd = string.Equals(
                ctx.Text.Header,
                "КОНЕЦ СЕРИИ",
                StringComparison.OrdinalIgnoreCase);

            foreach (var bubble in _bubblesView.BubblesPopUp)
            {
                bubble.IsCorrectType(ctx.Type);
                bubble.SetText(ctx.Text.Header, ctx.Text.Text);
                bubble.SetTextSize(
                    episodeEnd,
                    episodeEnd ? _episodeEndTextSize : _dialogueTextSize);
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
                if (inSceneButton.transform is RectTransform buttonRect
                    && _bubblesView.ButtonPrefab.transform is RectTransform prefabRect)
                {
                    buttonRect.anchoredPosition = prefabRect.anchoredPosition
                        + (episodeEnd ? Vector2.up * _episodeEndButtonYOffset : Vector2.zero);
                }
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

            if (_forceLayoutRebuildAfterContentChange)
            {
                Canvas.ForceUpdateCanvases();
                foreach (var bubble in _bubblesView.BubblesPopUp)
                    bubble.RebuildContentLayout();
                if (root.transform is RectTransform rootRect)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);
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
