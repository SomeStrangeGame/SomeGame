using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Bubble
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public GameObject BubblePrefab;
            public CancellationToken CancellationToken;
        }

        private readonly Ctx _ctx;

        private View.Screen _screen;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void Init()
        {
            var prefab = _ctx.BubblePrefab;
            var screenGO = GameObject.Instantiate(prefab);
            _screen = screenGO.GetComponent<View.Screen>();
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

        public void SetWardrobeScreen(BubbleContracts.WardrobePresentation presentation)
        {
            _screen.SetWardrobeScreen(new View.Screen.WardrobeCtx
            {
                // set wardrobe ctx here...
            });
        }

        public void SetChooseScreen(BubbleContracts.ChoosePresentation presentation)
        {
            _screen.SetChooseScreen(new View.Screen.ChooseCtx
            {
                // set wardrobe ctx here...
            });
        }

        public void SetBubbleScreen(BubbleContracts.BubblePresentation presentation)
        {
            View.Screen.BubbleCtx.BubbleType bubbleType;
            switch (presentation.DialoguePresentation)
            {
                case StoryContracts.DialoguePresentation.Disclaimer:
                case StoryContracts.DialoguePresentation.Hint:
                    bubbleType = View.Screen.BubbleCtx.BubbleType.Hint;
                    break;

                case StoryContracts.DialoguePresentation.Thoughts:
                    bubbleType = View.Screen.BubbleCtx.BubbleType.LeftMinds;
                    break;

                case StoryContracts.DialoguePresentation.Narrator:
                    bubbleType = View.Screen.BubbleCtx.BubbleType.NoCharacter;
                    break;

                default:
                    bubbleType = presentation.SpeakerRole == StoryContracts.StorySpeakerRole.MainCharacter
                        ? View.Screen.BubbleCtx.BubbleType.LeftCharacter
                        : View.Screen.BubbleCtx.BubbleType.RightCharacter;
                    break;
            }

            var buttons = presentation.Choices.Select(choice => new View.Screen.BubbleCtx.ButtonCtx
            {
                Id = choice.Id,
                Text = choice.Text,
                OnClick = choice.OnClick
            }).ToArray();
            
            _screen.SetBubbleScreen(new View.Screen.BubbleCtx
            {
                Type = bubbleType,
                Text = new View.Screen.BubbleCtx.TextCtx
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
