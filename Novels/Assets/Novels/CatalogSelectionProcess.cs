using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal sealed class CatalogSelectionProcess : BaseDisposable
    {
        internal struct Ctx
        {
            internal GameObject BundledPrefab;
            internal Catalog.NovelCatalogAsset Catalog;
            internal CancellationToken CancellationToken;
        }

        private readonly Ctx _ctx;

        internal CatalogSelectionProcess(Ctx ctx)
        {
            _ctx = ctx;
        }

        internal async UniTask<string> SelectContent()
        {
            if (_ctx.Catalog == null)
                throw new ArgumentNullException(nameof(_ctx.Catalog));
            if (_ctx.Catalog.Entries.Count == 0)
                throw new InvalidOperationException("Novel catalog is empty.");

            var setting = new Setting.Entity(new Setting.Entity.Ctx
            {
                BundledPrefab = _ctx.BundledPrefab,
            }).AddTo(this);
            setting.Init();
            setting.SetDescription(_ctx.Catalog.Title);

            var selection = new UniTaskCompletionSource<string>();
            foreach (var entry in _ctx.Catalog.Entries)
            {
                var contentId = entry.ContentId;
                var text = string.IsNullOrWhiteSpace(entry.Description)
                    ? $"<b>{entry.Title}</b>"
                    : $"<b>{entry.Title}</b>\n{entry.Description}";
                setting.AddOrUpdateButton(
                    contentId,
                    text,
                    () => selection.TrySetResult(contentId));
            }

            try
            {
                setting.Show();
                return await selection.Task.AttachExternalCancellation(
                    _ctx.CancellationToken);
            }
            finally
            {
                if (!setting.IsDisposed)
                    setting.Hide();
            }
        }
    }
}
