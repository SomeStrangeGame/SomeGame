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
                public Sprite Icon;
            }

            public BubbleType Type;
            public TextCtx Text;
            public ButtonCtx[] Buttons;
            public Action OnBackgroundClick;
        }

        [Serializable]
        private struct Bubbles
        {
            private const float _choiceSpacing = 12f;

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
                    ResizeToPreferredHeight(_text);
                }

                internal readonly void PlaceButtons(
                    IReadOnlyList<Button> buttons,
                    bool placeHorizontally)
                {
                    var textRect = _text.rectTransform;
                    var nextTop = textRect.anchoredPosition.y
                        - textRect.rect.height
                        - _choiceSpacing;

                    if (placeHorizontally)
                    {
                        PlaceButtonsHorizontally(buttons, textRect, nextTop);
                        return;
                    }

                    foreach (var button in buttons)
                    {
                        if (button.transform is not RectTransform buttonRect
                            || !button.gameObject.activeSelf)
                        {
                            continue;
                        }

                        buttonRect.anchorMin = new Vector2(
                            textRect.anchorMin.x,
                            buttonRect.anchorMin.y);
                        buttonRect.anchorMax = new Vector2(
                            textRect.anchorMax.x,
                            buttonRect.anchorMax.y);
                        buttonRect.pivot = new Vector2(
                            textRect.pivot.x,
                            buttonRect.pivot.y);

                        var position = buttonRect.anchoredPosition;
                        position.x = textRect.anchoredPosition.x;
                        position.y = nextTop
                            - buttonRect.rect.height * (1f - buttonRect.pivot.y);
                        buttonRect.anchoredPosition = position;
                        nextTop -= buttonRect.rect.height + _choiceSpacing;
                    }
                }

                private static void PlaceButtonsHorizontally(
                    IReadOnlyList<Button> buttons,
                    RectTransform textRect,
                    float top)
                {
                    var activeButtons = new List<RectTransform>();
                    var totalWidth = 0f;
                    foreach (var button in buttons)
                    {
                        if (button.transform is not RectTransform buttonRect
                            || !button.gameObject.activeSelf)
                        {
                            continue;
                        }

                        activeButtons.Add(buttonRect);
                        totalWidth += buttonRect.rect.width;
                    }

                    totalWidth += Mathf.Max(0, activeButtons.Count - 1) * _choiceSpacing;
                    var nextLeft = textRect.anchoredPosition.x - totalWidth * 0.5f;
                    foreach (var buttonRect in activeButtons)
                    {
                        buttonRect.anchorMin = new Vector2(textRect.anchorMin.x, buttonRect.anchorMin.y);
                        buttonRect.anchorMax = new Vector2(textRect.anchorMax.x, buttonRect.anchorMax.y);
                        buttonRect.pivot = new Vector2(0.5f, buttonRect.pivot.y);

                        var position = buttonRect.anchoredPosition;
                        position.x = nextLeft + buttonRect.rect.width * 0.5f;
                        position.y = top - buttonRect.rect.height * (1f - buttonRect.pivot.y);
                        buttonRect.anchoredPosition = position;
                        nextLeft += buttonRect.rect.width + _choiceSpacing;
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
        [SerializeField] private bool _placeChoicesHorizontally;
        [SerializeField] private bool _hideChoiceText;

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
                {
                    var createdButton = Instantiate(
                        _bubblesView.ButtonPrefab,
                        root.transform);
                    var layoutElement = createdButton.GetComponent<LayoutElement>()
                        ?? createdButton.gameObject.AddComponent<LayoutElement>();
                    layoutElement.ignoreLayout = true;
                    _buttonPool.Add(createdButton);
                }

                var inSceneButton = _buttonPool[index];
                inSceneButton.transform.SetParent(root.transform, false);
                ConfigureChoiceButton(inSceneButton, button);
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
                foreach (var bubble in _bubblesView.BubblesPopUp)
                {
                    if (bubble.TryGetRoot(ctx.Type, out _))
                        bubble.PlaceButtons(_buttonPool, _placeChoicesHorizontally);
                }
            }

            BindBackground(ctx.OnBackgroundClick);
        }

        private void ConfigureChoiceButton(Button button, BubbleCtx.ButtonCtx context)
        {
            var text = button.GetComponentInChildren<Text>(true);
            text.text = context.Text;
            text.gameObject.SetActive(!_hideChoiceText);

            var icon = button.GetComponentInChildren<ChoiceButtonIcon>(true);
            icon?.SetSprite(context.Icon);
        }

        private void BindBackground(Action onClick)
        {
            _bubblesView.BackgroundButton.onClick.RemoveAllListeners();
            _bubblesView.BackgroundButton.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}
