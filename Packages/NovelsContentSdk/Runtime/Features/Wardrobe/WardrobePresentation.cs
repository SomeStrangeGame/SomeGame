using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels.WardrobeContracts
{
    public enum WardrobeCategory
    {
        Appearance,
        Hair,
        Clothes,
        Accessory,
    }

    public static class WardrobeLabels
    {
        public const string Confirm = "Выбрать";
        public const string Appearance = "Выберите внешность";
        public const string Clothes = "Выберите одежду";
        public const string Hair = "Выберите причёску";
        public const string Accessory = "Выберите украшение";
    }

    public readonly struct WardrobeOption
    {
        public WardrobeOption(int id, string text)
        {
            Id = id;
            Text = text ?? string.Empty;
        }

        public int Id { get; }
        public string Text { get; }
    }

    public sealed class WardrobeSequencePage
    {
        public WardrobeSequencePage(
            WardrobeCategory category,
            string title,
            WardrobeOption[] options,
            Func<int, UniTask<Sprite>> loadThumbnail,
            Func<int, UniTask> preview)
        {
            Category = category;
            Title = title ?? string.Empty;
            Options = options ?? Array.Empty<WardrobeOption>();
            LoadThumbnail = loadThumbnail ?? throw new ArgumentNullException(nameof(loadThumbnail));
            Preview = preview ?? throw new ArgumentNullException(nameof(preview));
        }

        public WardrobeCategory Category { get; }
        public string Title { get; }
        public WardrobeOption[] Options { get; }
        public Func<int, UniTask<Sprite>> LoadThumbnail { get; }
        public Func<int, UniTask> Preview { get; }
    }

    public sealed class WardrobePresentation
    {
        public WardrobePresentation(
            string characterName,
            WardrobeCategory category,
            string title,
            string confirmationText,
            WardrobeOption[] options,
            Func<WardrobeCategory, UniTask<string[]>> loadCategory,
            Func<WardrobeCategory, string, UniTask<Sprite>> loadCategoryThumbnail,
            Func<WardrobeCategory, string, UniTask> previewCategory,
            Func<int, UniTask<Sprite>> loadThumbnail,
            Func<int, UniTask> preview,
            Action<int> confirm,
            bool allowCategoryBrowsing = false,
            bool freeMode = false,
            WardrobeSequencePage[] sequencePages = null,
            Action<int[]> confirmSequence = null,
            WardrobeCategory[] availableCategories = null,
            Func<WardrobeCategory, string> getSelectedCategoryValue = null,
            string characterTarget = null,
            int characterCount = 1,
            Func<int, UniTask<WardrobePresentation>> loadRelativeCharacter = null,
            Action commitFreeSession = null,
            Action cancelFreeSession = null,
            int[] categoryItemCounts = null)
        {
            CharacterName = characterName ?? string.Empty;
            Category = category;
            Title = title ?? string.Empty;
            ConfirmationText = string.IsNullOrWhiteSpace(confirmationText)
                ? WardrobeLabels.Confirm
                : confirmationText;
            Options = options ?? Array.Empty<WardrobeOption>();
            LoadCategory = loadCategory
                ?? throw new ArgumentNullException(nameof(loadCategory));
            LoadCategoryThumbnail = loadCategoryThumbnail
                ?? throw new ArgumentNullException(nameof(loadCategoryThumbnail));
            PreviewCategory = previewCategory
                ?? throw new ArgumentNullException(nameof(previewCategory));
            LoadThumbnail = loadThumbnail
                ?? throw new ArgumentNullException(nameof(loadThumbnail));
            Preview = preview ?? throw new ArgumentNullException(nameof(preview));
            Confirm = confirm ?? throw new ArgumentNullException(nameof(confirm));
            AllowCategoryBrowsing = allowCategoryBrowsing;
            FreeMode = freeMode;
            SequencePages = sequencePages ?? Array.Empty<WardrobeSequencePage>();
            ConfirmSequence = confirmSequence;
            AvailableCategories = availableCategories;
            GetSelectedCategoryValue = getSelectedCategoryValue;
            CharacterTarget = characterTarget ?? string.Empty;
            CharacterCount = Math.Max(1, characterCount);
            LoadRelativeCharacter = loadRelativeCharacter;
            CommitFreeSession = commitFreeSession;
            CancelFreeSession = cancelFreeSession;
            CategoryItemCounts = categoryItemCounts;
        }

        public string Title { get; }
        public string CharacterName { get; }
        public WardrobeCategory Category { get; }
        public string ConfirmationText { get; }
        public WardrobeOption[] Options { get; }
        public Func<WardrobeCategory, UniTask<string[]>> LoadCategory { get; }
        public Func<WardrobeCategory, string, UniTask<Sprite>> LoadCategoryThumbnail { get; }
        public Func<WardrobeCategory, string, UniTask> PreviewCategory { get; }
        public Func<int, UniTask<Sprite>> LoadThumbnail { get; }
        public Func<int, UniTask> Preview { get; }
        public Action<int> Confirm { get; }
        public bool AllowCategoryBrowsing { get; }
        public bool FreeMode { get; }
        public WardrobeSequencePage[] SequencePages { get; }
        public Action<int[]> ConfirmSequence { get; }
        public WardrobeCategory[] AvailableCategories { get; }
        public Func<WardrobeCategory, string> GetSelectedCategoryValue { get; }
        public string CharacterTarget { get; }
        public int CharacterCount { get; }
        public Func<int, UniTask<WardrobePresentation>> LoadRelativeCharacter { get; }
        public Action CommitFreeSession { get; }
        public Action CancelFreeSession { get; }
        public int[] CategoryItemCounts { get; }
    }
}
