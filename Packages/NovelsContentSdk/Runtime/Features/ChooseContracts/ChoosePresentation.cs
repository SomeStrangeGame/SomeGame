using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels.ChooseContracts
{
    public static class ChooseLabels
    {
        public const string Confirm = "Выбрать";
    }

    public readonly struct ChooseOption
    {
        public ChooseOption(int id, string text)
        {
            Id = id;
            Text = text ?? string.Empty;
        }

        public int Id { get; }
        public string Text { get; }
    }

    public sealed class ChoosePresentation
    {
        public ChoosePresentation(
            string title,
            string confirmationText,
            ChooseOption[] options,
            Func<int, UniTask<Sprite>> loadThumbnail,
            Action<int> confirm)
        {
            Title = title ?? string.Empty;
            ConfirmationText = string.IsNullOrWhiteSpace(confirmationText)
                ? ChooseLabels.Confirm
                : confirmationText;
            Options = options ?? Array.Empty<ChooseOption>();
            LoadThumbnail = loadThumbnail
                ?? throw new ArgumentNullException(nameof(loadThumbnail));
            Confirm = confirm ?? throw new ArgumentNullException(nameof(confirm));
        }

        public string Title { get; }
        public string ConfirmationText { get; }
        public ChooseOption[] Options { get; }
        public Func<int, UniTask<Sprite>> LoadThumbnail { get; }
        public Action<int> Confirm { get; }
    }
}
