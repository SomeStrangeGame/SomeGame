using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Wardrobe
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
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
            var screenObject = new GameObject("WardrobeScreen");
            _screen = screenObject.AddComponent<View.Screen>();
            _screen.Init();
        }

        public void SetScreen(WardrobeContracts.WardrobePresentation presentation)
        {
            _screen.SetPresentation(presentation);
        }

        public UniTask Show()
        {
            _ctx.CancellationToken.ThrowIfCancellationRequested();
            _screen.ShowImmediate();
            return UniTask.CompletedTask;
        }

        public void ShowImmediate() => _screen.ShowImmediate();

        public UniTask Hide()
        {
            _ctx.CancellationToken.ThrowIfCancellationRequested();
            _screen.HideImmediate();
            return UniTask.CompletedTask;
        }

        public void HideImmediate() => _screen.HideImmediate();

        protected override void OnDispose()
        {
            base.OnDispose();
            if (_screen != null)
                GameObject.Destroy(_screen.gameObject);
        }
    }
}
