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
        private Func<WardrobeContracts.WardrobePresentation> _createFreePresentation;
        private Func<string, UniTask> _beforeOpenFree;
        private Func<UniTask> _afterCloseFree;
        private bool _freeOpen;
        private readonly System.Collections.Generic.Dictionary<
            WardrobeContracts.WardrobeCategory, int> _sequenceSelections = new();

        public WardrobeController(
            CancellationToken cancellationToken,
            UnityEngine.GameObject screenPrefab = null)
        {
            _options = new OptionSelection.OptionListController(
                cancellationToken,
                screenPrefab == null
                    ? null
                    : screenPrefab.GetComponent<OptionSelection.OptionListScreen>());
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
            _sequenceSelections.Clear();
            foreach (var page in presentation.SequencePages)
            {
                if (page.Options.Length > 0)
                    _sequenceSelections[page.Category] = page.Options[0].Id;
            }
            PresentStoryCategory();
        }

        public void ConfigureFree(
            Func<WardrobeContracts.WardrobePresentation> createPresentation,
            Func<string, UniTask> beforeOpen,
            Func<UniTask> afterClose)
        {
            _createFreePresentation = createPresentation;
            _beforeOpenFree = beforeOpen;
            _afterCloseFree = afterClose;
        }

        public void OpenFree()
        {
            if (_createFreePresentation == null || _freeOpen)
                return;
            OpenFreeAsync().Forget();
        }

        private async UniTaskVoid OpenFreeAsync()
        {
            _freeOpen = true;
            _presentation = _createFreePresentation();
            if (_beforeOpenFree != null)
                await _beforeOpenFree(_presentation.CharacterTarget);
            var version = ++_categoryVersion;
            LoadCategory(_presentation.Category, version).Forget();
            await Show(StoryContracts.PresentationMode.Immediate);
        }

        private void PresentStoryCategory()
        {
            _categoryVersion++;
            var presentation = _presentation;
            if (presentation.SequencePages.Length > 1)
            {
                PresentSequencePage(presentation.SequencePages[0], true);
                return;
            }
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
                (int)presentation.Category,
                presentation.AllowCategoryBrowsing,
                interactableTabs: presentation.AllowCategoryBrowsing
                    ? GetAvailableTabIndices(presentation)
                    : new[] { (int)presentation.Category },
                previewInitialItem: true));
        }

        private void SelectCategory(int index)
        {
            if (_presentation == null
                || !_presentation.AllowCategoryBrowsing
                || index < 0
                || index > (int)WardrobeContracts.WardrobeCategory.Accessory)
            {
                return;
            }
            var category = (WardrobeContracts.WardrobeCategory)index;
            if (!IsCategoryAvailable(category))
                return;
            if (_presentation.SequencePages.Length > 1)
            {
                foreach (var page in _presentation.SequencePages)
                {
                    if (page.Category == category)
                    {
                        PresentSequencePage(page, false);
                        return;
                    }
                }
                return;
            }
            if (category == _presentation.Category)
            {
                _categoryVersion++;
                PresentStoryCategory();
                return;
            }
            LoadCategory(category, ++_categoryVersion).Forget();
        }

        private void PresentSequencePage(
            WardrobeContracts.WardrobeSequencePage page,
            bool previewInitialItem)
        {
            var items = new OptionSelection.OptionListItem[page.Options.Length];
            for (var index = 0; index < items.Length; index++)
                items[index] = new OptionSelection.OptionListItem(
                    page.Options[index].Id,
                    page.Options[index].Text);
            _options.Present(new OptionSelection.OptionListPresentation(
                page.Title,
                _presentation.ConfirmationText,
                items,
                page.LoadThumbnail,
                id =>
                {
                    _sequenceSelections[page.Category] = id;
                    return page.Preview(id);
                },
                _ =>
                {
                    var selected = new int[_presentation.SequencePages.Length];
                    for (var index = 0; index < selected.Length; index++)
                    {
                        var sequencePage = _presentation.SequencePages[index];
                        selected[index] = _sequenceSelections[sequencePage.Category];
                    }
                    _presentation.ConfirmSequence?.Invoke(selected);
                },
                _presentation.CharacterName,
                (int)page.Category,
                true,
                GetSequenceTabIndices(),
                _sequenceSelections[page.Category],
                GetSequenceTabItemCounts(),
                previewInitialItem));
        }

        private int[] GetSequenceTabIndices()
        {
            var pages = _presentation.SequencePages;
            var indices = new int[pages.Length];
            for (var index = 0; index < pages.Length; index++)
                indices[index] = (int)pages[index].Category;
            return indices;
        }

        private int[] GetSequenceTabItemCounts()
        {
            var counts = new[] { -1, -1, -1, -1 };
            foreach (var page in _presentation.SequencePages)
                counts[(int)page.Category] = page.Options.Length;
            return counts;
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
                int? initialItemId = null;
                var selectedValue = source.GetSelectedCategoryValue?.Invoke(category);
                if (!string.IsNullOrWhiteSpace(selectedValue))
                {
                    for (var index = 0; index < values.Length; index++)
                    {
                        if (string.Equals(
                                values[index],
                                selectedValue,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            initialItemId = index;
                            break;
                        }
                    }
                }
                _options.Present(new OptionSelection.OptionListPresentation(
                    CategoryTitle(category),
                    source.FreeMode ? "Готово" : "Применить",
                    items,
                    id => source.LoadCategoryThumbnail(category, values[id]),
                    id => source.PreviewCategory(category, values[id]),
                    _ =>
                    {
                        if (source.FreeMode)
                        {
                            source.CommitFreeSession?.Invoke();
                            CloseFree().Forget();
                        }
                        else
                            PresentStoryCategory();
                    },
                    source.CharacterName,
                    (int)category,
                    source.AllowCategoryBrowsing,
                    GetAvailableTabIndices(source),
                    initialItemId,
                    source.CategoryItemCounts,
                    previousCharacter: source.CharacterCount > 1
                        ? () => SelectRelativeCharacter(-1)
                        : null,
                    nextCharacter: source.CharacterCount > 1
                        ? () => SelectRelativeCharacter(1)
                        : null,
                    cancel: source.FreeMode
                        ? () =>
                        {
                            source.CancelFreeSession?.Invoke();
                            CloseFree().Forget();
                        }
                        : null));
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void SelectRelativeCharacter(int direction)
        {
            var source = _presentation;
            if (source?.LoadRelativeCharacter == null || source.CharacterCount <= 1)
                return;
            SelectRelativeCharacterAsync(source, direction).Forget();
        }

        private async UniTaskVoid SelectRelativeCharacterAsync(
            WardrobeContracts.WardrobePresentation source,
            int direction)
        {
            var version = ++_categoryVersion;
            try
            {
                var presentation = await source.LoadRelativeCharacter(direction);
                if (version != _categoryVersion || source != _presentation
                    || presentation == null)
                {
                    return;
                }
                _presentation = presentation;
                LoadCategory(presentation.Category, ++_categoryVersion).Forget();
            }
            catch (OperationCanceledException)
            {
            }
        }

        private bool IsCategoryAvailable(
            WardrobeContracts.WardrobeCategory category)
        {
            var categories = _presentation.AvailableCategories;
            return categories == null || Array.IndexOf(categories, category) >= 0;
        }

        private static int[] GetAvailableTabIndices(
            WardrobeContracts.WardrobePresentation presentation)
        {
            var categories = presentation.AvailableCategories;
            if (categories == null)
                return null;
            var indices = new int[categories.Length];
            for (var index = 0; index < categories.Length; index++)
                indices[index] = (int)categories[index];
            return indices;
        }

        private async UniTaskVoid CloseFree()
        {
            await Hide(StoryContracts.PresentationMode.Immediate);
            if (_afterCloseFree != null)
                await _afterCloseFree();
            _freeOpen = false;
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
