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
        }

        public void ShowImmediate()
        {
            _screen.ShowImmediate();
        }

        public void HideImmediate()
        {
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

        public void SetText(string text)
        {
            _screen.SetText(text);
        }

        public void AddOrUpdateButton(int id, string text, Action<int> onClick)
        {
            _screen.AddOrUpdateButton(id, text, onClick);
        }

        public void RemoveAllButtons()
        {
            _screen.RemoveAllButtons();
        }

        public void RemoveButton(int id)
        {
            _screen.RemoveButton(id);
        }
    }
}

