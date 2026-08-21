using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Bubble
{
    public class BubbleController : BaseDisposable
    {
        public struct Dependencies
        {
            public GameObject BubblePrefab;
            public CancellationToken CancellationToken;
        }

        private readonly Dependencies _ctx;

        private View.BubbleScreen _screen;

        public BubbleController(Dependencies ctx)
        {
            _ctx = ctx;
        }

        public void Init()
        {
            var prefab = _ctx.BubblePrefab;
            var screenGO = GameObject.Instantiate(prefab);
            _screen = screenGO.GetComponent<View.BubbleScreen>();
            _screen.HideImmediate();
        }

        public async UniTask Show()
        {
            await _screen.Show(_ctx.CancellationToken);
        }

        public void ShowImmediate()
        {
            _screen.ShowImmediate();
        }

        public async UniTask Hide()
        {
            await _screen.Hide(_ctx.CancellationToken);
        }

        public void HideImmediate()
        {
            _screen.HideImmediate();
        }

        public void SetBubbleScreen(BubbleContracts.BubblePresentation presentation)
        {
            View.BubbleScreen.BubbleCtx.BubbleType bubbleType;
            switch (presentation.DialoguePresentation)
            {
                case StoryContracts.DialoguePresentation.Disclaimer:
                case StoryContracts.DialoguePresentation.Hint:
                    bubbleType = View.BubbleScreen.BubbleCtx.BubbleType.Hint;
                    break;

                case StoryContracts.DialoguePresentation.Thoughts:
                    bubbleType = View.BubbleScreen.BubbleCtx.BubbleType.LeftMinds;
                    break;

                case StoryContracts.DialoguePresentation.Narrator:
                    bubbleType = View.BubbleScreen.BubbleCtx.BubbleType.NoCharacter;
                    break;

                default:
                    bubbleType = presentation.SpeakerRole == StoryContracts.StorySpeakerRole.MainCharacter
                        ? View.BubbleScreen.BubbleCtx.BubbleType.LeftCharacter
                        : View.BubbleScreen.BubbleCtx.BubbleType.RightCharacter;
                    break;
            }

            var buttons = presentation.Choices.Select(choice => new View.BubbleScreen.BubbleCtx.ButtonCtx
            {
                Id = choice.Id,
                Text = choice.Text,
                OnClick = choice.OnClick
            }).ToArray();
            
            _screen.SetBubbleScreen(new View.BubbleScreen.BubbleCtx
            {
                Type = bubbleType,
                Text = new View.BubbleScreen.BubbleCtx.TextCtx
                {
                    Header = presentation.Text.Header,
                    Text = presentation.Text.Text
                },
                Buttons = buttons,
                OnBackgroundClick = buttons.Length == 0 ? presentation.OnBackgroundClick : null
            });
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            if (_screen != null)
                GameObject.Destroy(_screen.gameObject);
        }

    }
}
