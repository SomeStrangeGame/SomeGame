using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels.OptionSelection
{
    public readonly struct OptionListItem
    {
        public OptionListItem(int id, string text)
        {
            Id = id;
            Text = text ?? string.Empty;
        }

        public int Id { get; }
        public string Text { get; }
    }

    public sealed class OptionListPresentation
    {
        public OptionListPresentation(
            string title,
            string confirmationText,
            OptionListItem[] items,
            Func<int, UniTask<Sprite>> loadThumbnail,
            Func<int, UniTask> preview,
            Action<int> confirm)
        {
            Title = title ?? string.Empty;
            ConfirmationText = confirmationText ?? string.Empty;
            Items = items ?? Array.Empty<OptionListItem>();
            LoadThumbnail = loadThumbnail
                ?? throw new ArgumentNullException(nameof(loadThumbnail));
            Preview = preview;
            Confirm = confirm ?? throw new ArgumentNullException(nameof(confirm));
        }

        public string Title { get; }
        public string ConfirmationText { get; }
        public OptionListItem[] Items { get; }
        public Func<int, UniTask<Sprite>> LoadThumbnail { get; }
        public Func<int, UniTask> Preview { get; }
        public Action<int> Confirm { get; }
    }
}
