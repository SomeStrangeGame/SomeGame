using Cysharp.Threading.Tasks;
using Localization;
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
            var localizationAddress = ContentAddressing.ContentAddressConvention.LocalizationAsset(
                entry.ContentId,
                BootstrapAddresses.LocalizationDataAssetName);
            var (content, localizationData) = await _priorityLoader.Run(() => UniTask.WhenAll(
                    bundles.GetBundledSO<Content.NovelContentAsset>(
                        new Bundles.BundleAssetAddress(
                            entry.ContentBundleName,
                            entry.ContentAssetName)),
                    bundles.GetBundledSO<LocalizationData>(
                        new Bundles.BundleAssetAddress(
                            entry.ContentBundleName,
                            localizationAddress)))
                .AttachExternalCancellation(_ctx.CancellationToken));
            if (content == null || localizationData == null)
            {
                throw new System.InvalidOperationException(
                    $"Content definition or localization for '{entry.ContentId}' "
                    + "could not be loaded from "
                    + $"AssetBundle '{entry.ContentBundleName}'.");
            }
            var localization = CreateLocalization(localizationData);
            var definition = content.ToDefinition(localization.GetRequiredValue);
            _audioMixer = content.AudioMixer;
            _localization = localization;
            return definition;
        }
    }
}
