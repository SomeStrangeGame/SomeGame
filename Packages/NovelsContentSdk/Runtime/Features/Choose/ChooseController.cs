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

        private readonly Dependencies _dependencies;
        private OptionSelection.OptionListScreen _screen;

        public ChooseController(Dependencies dependencies)
        {
            _dependencies = dependencies;
        }

        public void Init()
        {
            var screenObject = new GameObject("ChooseScreen");
            _screen = screenObject.AddComponent<OptionSelection.OptionListScreen>();
            _screen.Init();
        }

        public void SetScreen(ChooseContracts.ChoosePresentation presentation)
        {
            var items = new OptionSelection.OptionListItem[presentation.Options.Length];
            for (var index = 0; index < items.Length; index++)
            {
                var option = presentation.Options[index];
                items[index] = new OptionSelection.OptionListItem(option.Id, option.Text);
            }
            _screen.SetPresentation(new OptionSelection.OptionListPresentation(
                presentation.Title,
                presentation.ConfirmationText,
                items,
                presentation.LoadThumbnail,
                null,
                presentation.Confirm));
        }

        public UniTask Show(StoryContracts.PresentationMode mode)
        {
            _dependencies.CancellationToken.ThrowIfCancellationRequested();
            _screen.ShowImmediate();
            return UniTask.CompletedTask;
        }

        public UniTask Hide(StoryContracts.PresentationMode mode)
        {
            _dependencies.CancellationToken.ThrowIfCancellationRequested();
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
