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
            public string MainCharacter;
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
            public string[] Args;
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
            if (HasArgument(ctx.Args, StoryContracts.StoryArguments.Disclaimer)) bubbleType = View.Screen.BubbleCtx.BubbleType.Hint;
            else if (HasArgument(ctx.Args, StoryContracts.StoryArguments.Hint)) bubbleType = View.Screen.BubbleCtx.BubbleType.Hint;
            else if (HasArgument(ctx.Args, StoryContracts.StoryArguments.Thoughts)) bubbleType = View.Screen.BubbleCtx.BubbleType.LeftMinds;
            else if (ctx.Name == StoryContracts.StorySpeakers.Narrator) bubbleType = View.Screen.BubbleCtx.BubbleType.NoCharacter;
            else if (ctx.Name == _ctx.MainCharacter) bubbleType = View.Screen.BubbleCtx.BubbleType.LeftCharacter;
            else bubbleType = View.Screen.BubbleCtx.BubbleType.RightCharacter;

            var header = ctx.Text.Header;
            if (bubbleType == View.Screen.BubbleCtx.BubbleType.Hint)
            {
                if (HasArgument(ctx.Args, StoryContracts.StoryArguments.Disclaimer)) header = "Дисклеймер";
                if (HasArgument(ctx.Args, StoryContracts.StoryArguments.Hint)) header = "Подсказка";
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

        private static bool HasArgument(string[] arguments, string expected)
        {
            return arguments != null && arguments.Any(argument =>
                string.Equals(argument, expected, StringComparison.OrdinalIgnoreCase));
        }
    }
}
