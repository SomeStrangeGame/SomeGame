using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels.WardrobeContracts
{
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

    public sealed class WardrobePresentation
    {
        public WardrobePresentation(
            string title,
            string confirmationText,
            WardrobeOption[] options,
            Func<int, UniTask<Sprite>> loadThumbnail,
            Func<int, UniTask> preview,
            Action<int> confirm)
        {
            Title = title ?? string.Empty;
            ConfirmationText = string.IsNullOrWhiteSpace(confirmationText)
                ? WardrobeLabels.Confirm
                : confirmationText;
            Options = options ?? Array.Empty<WardrobeOption>();
            LoadThumbnail = loadThumbnail
                ?? throw new ArgumentNullException(nameof(loadThumbnail));
            Preview = preview ?? throw new ArgumentNullException(nameof(preview));
            Confirm = confirm ?? throw new ArgumentNullException(nameof(confirm));
        }

        public string Title { get; }
        public string ConfirmationText { get; }
        public WardrobeOption[] Options { get; }
        public Func<int, UniTask<Sprite>> LoadThumbnail { get; }
        public Func<int, UniTask> Preview { get; }
        public Action<int> Confirm { get; }
    }
}
