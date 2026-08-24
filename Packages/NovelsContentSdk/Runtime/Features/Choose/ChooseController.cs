using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels.Choose
{
    public sealed class ChooseController : BaseDisposable
    {
        private readonly OptionSelection.OptionListController _options;

        public ChooseController(CancellationToken cancellationToken)
        {
            _options = new OptionSelection.OptionListController(cancellationToken);
        }

        public void Init()
        {
            _options.Init("ChooseScreen");
        }

        public void SetScreen(ChooseContracts.ChoosePresentation presentation)
        {
            var items = new OptionSelection.OptionListItem[presentation.Options.Length];
            for (var index = 0; index < items.Length; index++)
            {
                var option = presentation.Options[index];
                items[index] = new OptionSelection.OptionListItem(option.Id, option.Text);
            }
            _options.Present(new OptionSelection.OptionListPresentation(
                presentation.Title,
                presentation.ConfirmationText,
                items,
                presentation.LoadThumbnail,
                null,
                presentation.Confirm));
        }

        public UniTask Show(StoryContracts.PresentationMode mode)
        {
            return _options.Show();
        }

        public UniTask Hide(StoryContracts.PresentationMode mode)
        {
            return _options.Hide();
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            _options.Dispose();
        }
    }
}
