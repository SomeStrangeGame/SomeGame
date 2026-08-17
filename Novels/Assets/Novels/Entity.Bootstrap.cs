using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private async UniTask<Content.NovelDefinition> LoadContent(
            Bundles.Scope bundles,
            Catalog.NovelCatalogEntry entry)
        {
            await _priorityLoader.Run(() => bundles
                .GetAssetBundle(entry.ContentBundleName)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var content = await _priorityLoader.Run(() => bundles
                .GetBundledSO<Content.NovelContentAsset>(
                    new Bundles.BundleAssetAddress(
                        entry.ContentBundleName,
                        entry.ContentAssetName))
                .AttachExternalCancellation(_ctx.CancellationToken));
            if (content == null)
            {
                throw new System.InvalidOperationException(
                    $"Content '{entry.ContentId}' could not be loaded from "
                    + $"AssetBundle '{entry.ContentBundleName}'.");
            }
            _audioMixer = content.AudioMixer;
            return content.ToDefinition(_ctx.Locale, entry.ContentBundleName);
        }
    }
}
