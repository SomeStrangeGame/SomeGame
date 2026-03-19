using System;
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

        public async UniTask Show()
        {
            await _screen.Show();
        }

        public async UniTask Hide()
        {
            await _screen.Hide();
        }

        public void SetText(string name, string header, string text)
        {
            View.Screen.BubbleType bubbleType;
            if (name == "...") bubbleType = View.Screen.BubbleType.NoCharacter;
            else if (name == _ctx.MainCharacter) bubbleType = View.Screen.BubbleType.LeftCharacter;
            else bubbleType = View.Screen.BubbleType.RightCharacter;

            _screen.SetText(bubbleType, header, text);
        }

        public void AddOrUpdateButton(int id, string name, string text, Action<int> onClick)
        {
            View.Screen.BubbleType bubbleType;
            if (name == "...") bubbleType = View.Screen.BubbleType.NoCharacter;
            else if (name == _ctx.MainCharacter) bubbleType = View.Screen.BubbleType.LeftCharacter;
            else bubbleType = View.Screen.BubbleType.RightCharacter;

            _screen.AddOrUpdateButton(id, bubbleType, text, onClick);
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

