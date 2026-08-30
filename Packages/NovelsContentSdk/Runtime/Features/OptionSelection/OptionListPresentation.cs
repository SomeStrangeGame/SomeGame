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
            Action<int> confirm,
            string header = null,
            int activeTab = -1,
            bool tabsInteractable = true,
            int[] interactableTabs = null,
            int? initialItemId = null,
            int[] tabItemCounts = null,
            bool previewInitialItem = false)
        {
            Title = title ?? string.Empty;
            ConfirmationText = confirmationText ?? string.Empty;
            Items = items ?? Array.Empty<OptionListItem>();
            LoadThumbnail = loadThumbnail
                ?? throw new ArgumentNullException(nameof(loadThumbnail));
            Preview = preview;
            Confirm = confirm ?? throw new ArgumentNullException(nameof(confirm));
            Header = header ?? string.Empty;
            ActiveTab = activeTab;
            TabsInteractable = tabsInteractable;
            InteractableTabs = interactableTabs;
            InitialItemId = initialItemId;
            TabItemCounts = tabItemCounts;
            PreviewInitialItem = previewInitialItem;
        }

        public string Title { get; }
        public string ConfirmationText { get; }
        public OptionListItem[] Items { get; }
        public Func<int, UniTask<Sprite>> LoadThumbnail { get; }
        public Func<int, UniTask> Preview { get; }
        public Action<int> Confirm { get; }
        public string Header { get; }
        public int ActiveTab { get; }
        public bool TabsInteractable { get; }
        public int[] InteractableTabs { get; }
        public int? InitialItemId { get; }
        public int[] TabItemCounts { get; }
        public bool PreviewInitialItem { get; }
    }
}
