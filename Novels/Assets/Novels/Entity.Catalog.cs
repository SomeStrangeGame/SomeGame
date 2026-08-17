using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private async UniTask<(Catalog.NovelCatalogAsset catalog, GameObject screen)>
            LoadCatalog(Bundles.Entity bundles)
        {
            await _priorityLoader.Run(() => bundles
                .GetAssetBundle(_catalogBundleName)
                .AttachExternalCancellation(_ctx.CancellationToken));

            var catalog = await _priorityLoader.Run(() => bundles
                .GetBundledSO<Catalog.NovelCatalogAsset>(
                    _catalogBundleName,
                    _catalogAssetName)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var screen = await _priorityLoader.Run(() => bundles
                .GetBundledPrefab(_catalogBundleName, _catalogScreenAssetName)
                .AttachExternalCancellation(_ctx.CancellationToken));
            if (catalog == null || screen == null)
            {
                throw new System.InvalidOperationException(
                    $"Catalog assets could not be loaded from "
                    + $"AssetBundle '{_catalogBundleName}'.");
            }

            return (catalog, screen);
        }

        private async UniTask<string> SelectContent(
            Catalog.NovelCatalogAsset catalog,
            GameObject screen)
        {
            using var selection = new CatalogSelectionProcess(
                new CatalogSelectionProcess.Ctx
                {
                    BundledPrefab = screen,
                    Catalog = catalog,
                    CancellationToken = _ctx.CancellationToken,
                });
            return await selection.SelectContent();
        }
    }
}
