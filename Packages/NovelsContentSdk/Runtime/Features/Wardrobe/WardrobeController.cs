using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels.Wardrobe
{
    public sealed class WardrobeController : BaseDisposable
    {
        private readonly OptionSelection.OptionListController _options;

        public WardrobeController(CancellationToken cancellationToken)
        {
            _options = new OptionSelection.OptionListController(cancellationToken);
        }

        public void Init()
        {
            _options.Init("WardrobeScreen");
        }

        public void SetScreen(WardrobeContracts.WardrobePresentation presentation)
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
                presentation.Preview,
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
