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
            var options = new View.CarouselOption[presentation.Options.Length];
            for (var index = 0; index < options.Length; index++)
            {
                var option = presentation.Options[index];
                options[index] = new View.CarouselOption(option.Id, option.Text);
            }
            _screen.SetPresentation(new View.CarouselPresentation
            {
                Title = presentation.Title,
                ConfirmationText = presentation.ConfirmationText,
                Options = options,
                LoadThumbnail = presentation.LoadThumbnail,
                Preview = _ => UniTask.CompletedTask,
                Confirm = presentation.Confirm,
            });
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
