using Cysharp.Threading.Tasks;
using Disposable;
using System;
using UnityEngine;

namespace Novels
{
    internal partial class NovelRuntime
    {
        private const string _saveChoiceFileName = "SaveChoice";

        private async UniTask<Content.NovelDefinition> LoadContent(
            Bundles.Scope bundles,
            Catalog.NovelCatalogEntry entry,
            string bundleName)
        {
            await _priorityLoader.Run(() => bundles
                .GetAssetBundle(bundleName)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var content = await _priorityLoader.Run(() =>
                    bundles.GetBundledSO<Content.NovelContentAsset>(
                        new Bundles.BundleAssetAddress(
                            bundleName,
                            entry.ContentAssetName))
                .AttachExternalCancellation(_ctx.CancellationToken));
            if (content == null)
            {
                throw new System.InvalidOperationException(
                    $"Content definition for '{entry.ContentId}' could not be loaded from "
                    + $"AssetBundle '{bundleName}'.");
            }
            var definition = content.ToDefinition();
            return definition;
        }

        private Save.SaveSystem CreateSaveSystem()
        {
            var cache = new Cache.Entity(_ctx.PersistentDataPath);
            var saveKey = $"Saves/{Uri.EscapeDataString(_definition.Id)}/"
                + $"{Uri.EscapeDataString(_episode.Id)}/{_saveChoiceFileName}";
            var saveSystem = new Save.SaveSystem(new Save.SaveSystem.Dependencies
            {
                SaveChoiceFileName = saveKey,
                ContentId = $"{_definition.Id}/{_episode.Id}",
                ContentVersion = _definition.ContentVersion,
                ReadBytes = cache.ReadBytes,
                WriteBytes = cache.WriteBytes,
                Delete = cache.Delete,
                OnLog = _ctx.OnLog,
                OnError = ReportError,
            }).AddTo(this);
            saveSystem.Init();
            _saveSystem = saveSystem;
            return saveSystem;
        }
    }
}
