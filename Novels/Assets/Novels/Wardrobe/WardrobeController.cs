using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Wardrobe
{
    public sealed class WardrobeController : BaseDisposable
    {
        public struct Dependencies
        {
            public CancellationToken CancellationToken;
        }

        private readonly Dependencies _ctx;
        private View.WardrobeScreen _screen;

        public WardrobeController(Dependencies ctx)
        {
            _ctx = ctx;
        }

        public void Init()
        {
            var screenObject = new GameObject("WardrobeScreen");
            _screen = screenObject.AddComponent<View.WardrobeScreen>();
            _screen.Init();
        }

        public void SetScreen(WardrobeContracts.WardrobePresentation presentation)
        {
            _screen.SetPresentation(presentation);
        }

        public UniTask Show(StoryContracts.PresentationMode mode)
        {
            _ctx.CancellationToken.ThrowIfCancellationRequested();
            _screen.ShowImmediate();
            return UniTask.CompletedTask;
        }

        public UniTask Hide(StoryContracts.PresentationMode mode)
        {
            _ctx.CancellationToken.ThrowIfCancellationRequested();
            _screen.HideImmediate();
            return UniTask.CompletedTask;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            if (_screen != null)
                GameObject.Destroy(_screen.gameObject);
        }
    }
}
