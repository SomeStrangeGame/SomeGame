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
            public Func<UniTask<GameObject>> GetBubblePrefab;
        }

        private readonly Ctx _ctx;

        private View.Screen _screen;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask Init()
        {
            var prefab = await _ctx.GetBubblePrefab();
            var screenGO = GameObject.Instantiate(prefab);
            _screen = screenGO.GetComponent<View.Screen>();
            _screen.HideImmediate();
        }

        public async UniTask Show(bool isLoading)
        {
            if (isLoading)
                _screen.ShowImmediate();
            else
                await _screen.Show();
        }

        public async UniTask Hide(bool isLoading)
        {
            if (isLoading)
                _screen.HideImmediate();
            else
                await _screen.Hide();
        }

        public void SetText(string name, string header, string text, string[] args)
        {
            var bubbleType = GetBubbleType(name, args);
            if (bubbleType == View.Screen.BubbleType.Hint)
            {
                if (args != null && args.Any(arg => arg.ToLower() == "дисклеймер")) header = "Дисклеймер";
                if (args != null && args.Any(arg => arg.ToLower() == "подсказка")) header = "Подсказка";
            }
            _screen.SetText(bubbleType, header, text);
        }

        public void AddOrUpdateButton(int id, string name, string text, string[] args, Action<int> onClick)
        {
            _screen.AddOrUpdateButton(id, GetBubbleType(name, args), text, onClick);
        }

        private View.Screen.BubbleType GetBubbleType(string name, string[] args)
        {
            View.Screen.BubbleType bubbleType;
            if (args != null && args.Any(arg => arg.ToLower() == "дисклеймер")) bubbleType = View.Screen.BubbleType.Hint;
            else if (args != null && args.Any(arg => arg.ToLower() == "подсказка")) bubbleType = View.Screen.BubbleType.Hint;
            else if (args != null && args.Any(arg => arg.ToLower() == "мысли")) bubbleType = View.Screen.BubbleType.LeftMinds;
            else if (name == "..." || name == "Wardrobe") bubbleType = View.Screen.BubbleType.NoCharacter;
            else if (name == _ctx.MainCharacter) bubbleType = View.Screen.BubbleType.LeftCharacter;
            else bubbleType = View.Screen.BubbleType.RightCharacter;
            return bubbleType;
        }

        public void RemoveAllButtons()
        {
            _screen.RemoveAllButtons();
        }

        public void RemoveButton(int id)
        {
            _screen.RemoveButton(id);
        }

        public void SetBackgroundButton(Action onClick)
        {
            _screen.SetBackgroundButton(onClick);
        }

        public void ResetBackgroundButton()
        {
            _screen.ResetBackgroundButton();
        }
    }
}

