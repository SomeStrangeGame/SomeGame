using Cysharp.Threading.Tasks;
using Disposable;
using Localization;
using System.Threading;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private sealed class PreparedNovelResources
        {
            internal PreparedNovelResources(
                Save.Entity saveSystem,
                PathGetter.Entity pathGetter,
                EpisodeRuntime episodeRuntime,
                Bundles.Scope episodeBundles,
                Loading.Entity mainLoading,
                Localization.Entity localization,
                UniTask<string> episodePreloading)
            {
                SaveSystem = saveSystem;
                PathGetter = pathGetter;
                EpisodeRuntime = episodeRuntime;
                EpisodeScope = episodeRuntime.Scope;
                EpisodeBundles = episodeBundles;
                MainLoading = mainLoading;
                Localization = localization;
                EpisodePreloading = episodePreloading;
            }

            internal Save.Entity SaveSystem { get; }
            internal PathGetter.Entity PathGetter { get; }
            internal EpisodeRuntime EpisodeRuntime { get; }
            internal EpisodeScope EpisodeScope { get; }
            internal CancellationToken CancellationToken =>
                EpisodeRuntime.CancellationToken;
            internal Bundles.Scope EpisodeBundles { get; }
            internal Loading.Entity MainLoading { get; }
            internal Localization.Entity Localization { get; }
            internal UniTask<string> EpisodePreloading { get; }
        }

        private async UniTask<NovelStartSession> PrepareApplication(
            Bundles.Scope novelBundles)
        {
            var saveSystem = CreateSaveSystem();
            var pathGetter = CreatePathGetter();
            var episodeRuntime = CreateEpisodeRuntime().AddTo(this);
            var episodeBundles = _ctx.Bundles
                .CreateScope(episodeRuntime.CancellationToken)
                .AddTo(episodeRuntime.Scope);
            ConfigureMedia(episodeBundles);

            await _priorityLoader.Run(() => novelBundles
                .GetAssetBundle(_definition.MainLoadingBundleName)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var mainLoadingAddress = new Bundles.BundleAssetAddress(
                _definition.MainLoadingBundleName,
                pathGetter.GetMainLoadingPrefabAssetName(BootstrapAddresses.ScreenAssetName));
            var mainLoadingScreen = await _priorityLoader.Run(() => novelBundles
                .GetBundledPrefab(mainLoadingAddress)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var mainLoading = CreateMainLoading(mainLoadingScreen);

            var episodePreloading = PreloadEpisode(
                pathGetter,
                episodeBundles,
                episodeRuntime.CancellationToken).Preserve();
            await mainLoading.Show().AttachExternalCancellation(_ctx.CancellationToken);

            var localizationAddress = new Bundles.BundleAssetAddress(
                _definition.BundleName,
                pathGetter.GetLocalizationDataAssetName(
                    BootstrapAddresses.LocalizationDataAssetName));
            LocalizationData localizationData;
            localizationData = await _priorityLoader.Run(() => novelBundles
                .GetBundledSO<LocalizationData>(localizationAddress)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var localization = CreateLocalization(localizationData);

            var settingsAddress = new Bundles.BundleAssetAddress(
                _definition.BundleName,
                pathGetter.GetSettingPrefabAssetName(BootstrapAddresses.ScreenAssetName));
            var settingsScreen = await _priorityLoader.Run(() => novelBundles
                .GetBundledPrefab(settingsAddress)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var settingProcess = new SettingProcess(new SettingProcess.Ctx
            {
                BundledPrefab = settingsScreen,
                ShowLoading = mainLoading.Show,
                HideLoading = mainLoading.Hide,
                ContainAnySave = () => saveSystem.ContainAnySave,
                GetLocalizationValue = localization.GetValue,
                CancellationToken = _ctx.CancellationToken,
            }).AddTo(this);
            var resources = new PreparedNovelResources(
                saveSystem,
                pathGetter,
                episodeRuntime,
                episodeBundles,
                mainLoading,
                localization,
                episodePreloading);
            var selection = await settingProcess.ShowSettingProcess();
            return new NovelStartSession(
                selection,
                saveSystem.Clear,
                () => RunEpisode(resources));
        }

        private async UniTask<string> PreloadEpisode(
            PathGetter.Entity pathGetter,
            Bundles.Scope episodeBundles,
            CancellationToken cancellationToken)
        {
            var result = await UniTask.WhenAll(
                _ctx.Bundles.GetText(pathGetter.GetNovelTextPath(_episode.StoryPath))
                    .AttachExternalCancellation(cancellationToken),
                episodeBundles.GetAssetBundle(_episode.BundleName));
            return result.Item1;
        }
    }
}
