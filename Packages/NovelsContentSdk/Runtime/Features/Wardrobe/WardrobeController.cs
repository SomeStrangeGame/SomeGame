using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels.Wardrobe
{
    public sealed class WardrobeController : BaseDisposable
    {
        private readonly OptionSelection.OptionListController _options;
        private WardrobeContracts.WardrobePresentation _presentation;
        private int _categoryVersion;

        public WardrobeController(CancellationToken cancellationToken)
        {
            _options = new OptionSelection.OptionListController(cancellationToken);
        }

        public void Init()
        {
            _options.Init(
                "WardrobeScreen",
                OptionSelection.OptionListLayout.Wardrobe,
                SelectCategory);
        }

        public void SetScreen(WardrobeContracts.WardrobePresentation presentation)
        {
            _presentation = presentation
                ?? throw new System.ArgumentNullException(nameof(presentation));
            _categoryVersion++;
            PresentStoryCategory();
        }

        private void PresentStoryCategory()
        {
            _categoryVersion++;
            var presentation = _presentation;
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
                presentation.Confirm,
                presentation.CharacterName,
                (int)presentation.Category));
        }

        private void SelectCategory(int index)
        {
            if (_presentation == null
                || index < 0
                || index > (int)WardrobeContracts.WardrobeCategory.Accessory)
            {
                return;
            }
            var category = (WardrobeContracts.WardrobeCategory)index;
            if (category == _presentation.Category)
            {
                _categoryVersion++;
                PresentStoryCategory();
                return;
            }
            LoadCategory(category, ++_categoryVersion).Forget();
        }

        private async UniTaskVoid LoadCategory(
            WardrobeContracts.WardrobeCategory category,
            int version)
        {
            var source = _presentation;
            if (source == null)
                return;
            try
            {
                var values = await source.LoadCategory(category);
                if (version != _categoryVersion || source != _presentation)
                    return;
                var items = new OptionSelection.OptionListItem[values.Length];
                for (var index = 0; index < values.Length; index++)
                    items[index] = new OptionSelection.OptionListItem(index, values[index]);
                _options.Present(new OptionSelection.OptionListPresentation(
                    CategoryTitle(category),
                    "Применить",
                    items,
                    id => source.LoadCategoryThumbnail(category, values[id]),
                    id => source.PreviewCategory(category, values[id]),
                    _ => PresentStoryCategory(),
                    source.CharacterName,
                    (int)category));
            }
            catch (OperationCanceledException)
            {
            }
        }

        private static string CategoryTitle(
            WardrobeContracts.WardrobeCategory category) => category switch
            {
                WardrobeContracts.WardrobeCategory.Appearance =>
                    WardrobeContracts.WardrobeLabels.Appearance,
                WardrobeContracts.WardrobeCategory.Hair =>
                    WardrobeContracts.WardrobeLabels.Hair,
                WardrobeContracts.WardrobeCategory.Accessory =>
                    WardrobeContracts.WardrobeLabels.Accessory,
                _ => WardrobeContracts.WardrobeLabels.Clothes,
            };

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
