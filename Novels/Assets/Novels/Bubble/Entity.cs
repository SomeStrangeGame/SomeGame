using System;
using System.Linq;
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
        }

        public struct WardrobeScreenCtx
        {
            // wardrobe ctx
        }

        public struct ChooseScreenCtx
        {
            // choose ctx
        }

        public struct BubbleScreenCtx
        {
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
            public string Name;
            public StoryContracts.StorySpeakerRole SpeakerRole;
            public StoryContracts.DialoguePresentation Presentation;
            public TextCtx Text;
            public ButtonCtx[] Buttons;
            public Action OnBackgroundClick;
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
            await _screen.Show();
        }

        public void ShowImmediate()
        {
            _screen.ShowImmediate();
        }

        public async UniTask Hide()
        {
            await _screen.Hide();
        }

        public void HideImmediate()
        {
            _screen.HideImmediate();
        }

        public void SetWardrobeScreen(WardrobeScreenCtx ctx)
        {
            _screen.SetWardrobeScreen(new View.Screen.WardrobeCtx
            {
                // set wardrobe ctx here...
            });
        }

        public void SetChooseScreen(ChooseScreenCtx ctx)
        {
            _screen.SetChooseScreen(new View.Screen.ChooseCtx
            {
                // set wardrobe ctx here...
            });
        }

        public void SetBubbleScreen(BubbleScreenCtx ctx)
        {
            View.Screen.BubbleCtx.BubbleType bubbleType;
            switch (ctx.Presentation)
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
                    bubbleType = ctx.SpeakerRole == StoryContracts.StorySpeakerRole.MainCharacter
                        ? View.Screen.BubbleCtx.BubbleType.LeftCharacter
                        : View.Screen.BubbleCtx.BubbleType.RightCharacter;
                    break;
            }

            var header = ctx.Text.Header;
            switch (ctx.Presentation)
            {
                case StoryContracts.DialoguePresentation.Disclaimer:
                    header = "Дисклеймер";
                    break;

                case StoryContracts.DialoguePresentation.Hint:
                    header = "Подсказка";
                    break;
            }
            var buttons = ctx.Buttons.Select(b => new View.Screen.BubbleCtx.ButtonCtx
            {
                Id = b.Id,
                Text = b.Text,
                OnClick = b.OnClick
            }).ToArray();
            
            _screen.SetBubbleScreen(new View.Screen.BubbleCtx
            {
                Type = bubbleType,
                Text = new View.Screen.BubbleCtx.TextCtx
                {
                    Header = header,
                    Text = ctx.Text.Text
                },
                Buttons = buttons,
                OnBackgroundClick = buttons.Length == 0 ? ctx.OnBackgroundClick : null
            });
        }

    }
}
