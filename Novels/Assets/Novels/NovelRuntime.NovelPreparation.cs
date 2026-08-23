using Cysharp.Threading.Tasks;
using Disposable;
using System.Threading;
using UnityEngine;

namespace Novels
{
    internal partial class NovelRuntime
    {
        private readonly struct EpisodeStoryData
        {
            internal EpisodeStoryData(string storyText, string sourceMapText)
            {
                StoryText = storyText;
                SourceMapText = sourceMapText;
            }

            internal string StoryText { get; }
            internal string SourceMapText { get; }
        }

        private sealed class PreparedNovelResources
        {
            internal PreparedNovelResources(
                Save.SaveSystem saveSystem,
                ContentAddressing.ContentAddresses addresses,
                EpisodeRuntime episodeRuntime,
                Bundles.Scope storyAssets,
                Bundles.MediaScope storyMedia,
                Loading.Entity mainLoading,
                UniTask<EpisodeStoryData> episodePreloading)
            {
                SaveSystem = saveSystem;
                Addresses = addresses;
                EpisodeRuntime = episodeRuntime;
                EpisodeScope = episodeRuntime.Scope;
                StoryAssets = storyAssets;
                StoryMedia = storyMedia;
                MainLoading = mainLoading;
                EpisodePreloading = episodePreloading;
            }

            internal Save.SaveSystem SaveSystem { get; }
            internal ContentAddressing.ContentAddresses Addresses { get; }
            internal EpisodeRuntime EpisodeRuntime { get; }
            internal EpisodeScope EpisodeScope { get; }
            internal CancellationToken CancellationToken =>
                EpisodeRuntime.CancellationToken;
            internal Bundles.Scope StoryAssets { get; }
            internal Bundles.MediaScope StoryMedia { get; }
            internal Loading.Entity MainLoading { get; }
            internal UniTask<EpisodeStoryData> EpisodePreloading { get; }
        }

        private async UniTask<NovelStartSession> PrepareApplication(
            Bundles.Scope storyAssets,
            EpisodeRuntime episodeRuntime)
        {
            var saveSystem = CreateSaveSystem();
            var addresses = new ContentAddressing.ContentAddresses(
                _definition.Id,
                _episode.Id);
            var storyMedia = _ctx.Bundles
                .CreateMediaScope(
                    _definition.Prefix,
                    new[]
                    {
                        ContentAddressing.ContentPackageConvention.StoryDeliveryGroup(
                            _definition.Id),
                    },
                    new Bundles.MediaManifest(_episode.Media.SilentAudioIds),
                    episodeRuntime.CancellationToken)
                .AddTo(episodeRuntime.Scope);

            await _priorityLoader.Run(() => storyAssets
                .GetAssetBundle(_definition.BundleName)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var mainLoadingAddress = new Bundles.BundleAssetAddress(
                _definition.BundleName,
                addresses.SharedLoadingPrefab(
                    ContentAddressing.ContentAssetNames.EpisodeScreen));
            var bundledMainLoadingScreen = await _priorityLoader.Run(() => storyAssets
                .TryGetBundledPrefab(mainLoadingAddress)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var mainLoadingScreen = bundledMainLoadingScreen
                != null
                ? bundledMainLoadingScreen
                : _ctx.FallbackAssets.Loading;
            var mainLoading = CreateMainLoading(mainLoadingScreen);

            var episodePreloading = PreloadEpisode(
                addresses,
                episodeRuntime.CancellationToken).Preserve();
            await mainLoading.Show().AttachExternalCancellation(_ctx.CancellationToken);

            var settingsAddress = new Bundles.BundleAssetAddress(
                _definition.BundleName,
                addresses.SettingPrefab(BootstrapAddresses.ScreenAssetName));
            var settingsScreen = await _priorityLoader.Run(() => storyAssets
                .GetBundledPrefab(settingsAddress)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var settingProcess = new SettingProcess(new SettingProcess.Dependencies
            {
                BundledPrefab = settingsScreen,
                ShowLoading = mainLoading.Show,
                HideLoading = mainLoading.Hide,
                ContainAnySave = () => saveSystem.ContainAnySave,
                NovelTitle = _ctx.Content.Text.Title,
                CancellationToken = _ctx.CancellationToken,
            }).AddTo(this);
            var resources = new PreparedNovelResources(
                saveSystem,
                addresses,
                episodeRuntime,
                storyAssets,
                storyMedia,
                mainLoading,
                episodePreloading);
            var selection = await settingProcess.ShowSettingProcess();
            return new NovelStartSession(
                selection,
                saveSystem.Clear,
                () => RunEpisode(resources));
        }

        private async UniTask<EpisodeStoryData> PreloadEpisode(
            ContentAddressing.ContentAddresses addresses,
            CancellationToken cancellationToken)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var result = await UniTask.WhenAll(
                _ctx.Bundles.GetText(addresses.NovelText(_episode.StoryPath))
                    .AttachExternalCancellation(cancellationToken),
                TryLoadStorySourceMap(addresses, cancellationToken));
            return new EpisodeStoryData(result.Item1, result.Item2);
#else
            var storyText = await _ctx.Bundles.GetText(
                    addresses.NovelText(_episode.StoryPath))
                .AttachExternalCancellation(cancellationToken);
            return new EpisodeStoryData(storyText, string.Empty);
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private async UniTask<string> TryLoadStorySourceMap(
            ContentAddressing.ContentAddresses addresses,
            CancellationToken cancellationToken)
        {
            try
            {
                return await _ctx.Bundles
                    .GetText(addresses.NovelSourceMap(_episode.StoryPath))
                    .AttachExternalCancellation(cancellationToken);
            }
            catch (System.OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (System.Exception exception)
            {
                _ctx.OnLog?.Invoke((
                    LogType.Warning,
                    $"Story source overlay is unavailable: {exception.Message}"));
                return string.Empty;
            }
        }
#endif
    }
}
