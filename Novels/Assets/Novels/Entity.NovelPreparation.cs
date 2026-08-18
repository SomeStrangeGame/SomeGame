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
                ContentAddressing.ContentAddresses addresses,
                EpisodeRuntime episodeRuntime,
                Bundles.MediaScope episodeBundles,
                Loading.Entity mainLoading,
                Localization.Entity localization,
                UniTask<string> episodePreloading)
            {
                SaveSystem = saveSystem;
                Addresses = addresses;
                EpisodeRuntime = episodeRuntime;
                EpisodeScope = episodeRuntime.Scope;
                EpisodeBundles = episodeBundles;
                MainLoading = mainLoading;
                Localization = localization;
                EpisodePreloading = episodePreloading;
            }

            internal Save.Entity SaveSystem { get; }
            internal ContentAddressing.ContentAddresses Addresses { get; }
            internal EpisodeRuntime EpisodeRuntime { get; }
            internal EpisodeScope EpisodeScope { get; }
            internal CancellationToken CancellationToken =>
                EpisodeRuntime.CancellationToken;
            internal Bundles.MediaScope EpisodeBundles { get; }
            internal Loading.Entity MainLoading { get; }
            internal Localization.Entity Localization { get; }
            internal UniTask<string> EpisodePreloading { get; }
        }

        private async UniTask<NovelStartSession> PrepareApplication(
            Bundles.Scope novelBundles,
            EpisodeRuntime episodeRuntime)
        {
            var saveSystem = CreateSaveSystem();
            var addresses = new ContentAddressing.ContentAddresses(
                _definition.Id,
                _episode.Id);
            var episodeBundles = _ctx.Bundles
                .CreateMediaScope(
                    _definition.Prefix,
                    new[]
                    {
                        ContentAddressing.ContentPackageConvention.EpisodeDeliveryGroup(
                            _definition.Id,
                            _episode.Id),
                        ContentAddressing.ContentPackageConvention.SharedDeliveryGroup(
                            _definition.Id),
                    },
                    new Bundles.MediaManifest(
                        _episode.Media.AudioExtensions,
                        _episode.Media.DefaultAudioExtension,
                        _episode.Media.SilentAudioIds),
                    episodeRuntime.CancellationToken)
                .AddTo(episodeRuntime.Scope);

            await _priorityLoader.Run(() => novelBundles
                .GetAssetBundle(_definition.MainLoadingBundleName)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var mainLoadingAddress = new Bundles.BundleAssetAddress(
                _definition.MainLoadingBundleName,
                addresses.MainLoadingPrefab(BootstrapAddresses.ScreenAssetName));
            var mainLoadingScreen = await _priorityLoader.Run(() => novelBundles
                .GetBundledPrefab(mainLoadingAddress)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var mainLoading = CreateMainLoading(mainLoadingScreen);

            var episodePreloading = PreloadEpisode(
                addresses,
                episodeBundles,
                episodeRuntime.CancellationToken).Preserve();
            await mainLoading.Show().AttachExternalCancellation(_ctx.CancellationToken);

            var settingsAddress = new Bundles.BundleAssetAddress(
                _definition.BundleName,
                addresses.SettingPrefab(BootstrapAddresses.ScreenAssetName));
            var settingsScreen = await _priorityLoader.Run(() => novelBundles
                .GetBundledPrefab(settingsAddress)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var settingProcess = new SettingProcess(new SettingProcess.Ctx
            {
                BundledPrefab = settingsScreen,
                ShowLoading = mainLoading.Show,
                HideLoading = mainLoading.Hide,
                ContainAnySave = () => saveSystem.ContainAnySave,
                NovelTitle = _ctx.Content.Resolve(_ctx.Locale).Title,
                GetLocalizationValue = _localization.GetValue,
                CancellationToken = _ctx.CancellationToken,
            }).AddTo(this);
            var resources = new PreparedNovelResources(
                saveSystem,
                addresses,
                episodeRuntime,
                episodeBundles,
                mainLoading,
                _localization,
                episodePreloading);
            var selection = await settingProcess.ShowSettingProcess();
            return new NovelStartSession(
                selection,
                saveSystem.Clear,
                () => RunEpisode(resources));
        }

        private async UniTask<string> PreloadEpisode(
            ContentAddressing.ContentAddresses addresses,
            Bundles.MediaScope episodeBundles,
            CancellationToken cancellationToken)
        {
            var result = await UniTask.WhenAll(
                _ctx.Bundles.GetText(addresses.NovelText(_episode.StoryPath))
                    .AttachExternalCancellation(cancellationToken),
                episodeBundles.GetAssetBundle(_episode.BundleName));
            return result.Item1;
        }
    }
}
