using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Choose
{
    public sealed class ChooseController : BaseDisposable
    {
        public struct Dependencies
        {
            public CancellationToken CancellationToken;
        }

        private readonly Dependencies _ctx;
        private View.ChooseScreen _screen;

        public ChooseController(Dependencies ctx)
        {
            _ctx = ctx;
        }

        public void Init()
        {
            var screenObject = new GameObject("ChooseScreen");
            _screen = screenObject.AddComponent<View.ChooseScreen>();
            _screen.Init();
        }

        public void SetScreen(ChooseContracts.ChoosePresentation presentation)
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
