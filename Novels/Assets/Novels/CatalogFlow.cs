using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Novels
{
    internal sealed class CatalogFlow
    {
        private const int _maximumParallelPreviewRequests = 3;
        private const int _requiredCoverTimeoutSeconds = 30;
        private const int _optionalCoverTimeoutSeconds = 15;

        private readonly struct LoadedEntry
        {
            internal LoadedEntry(
                Catalog.NovelCatalogEntry entry,
                Catalog.Contracts.StoryCatalogPreview preview,
                IReadOnlyDictionary<string, Sprite> covers)
            {
                Entry = entry;
                Preview = preview;
                Covers = covers;
            }

            internal Catalog.NovelCatalogEntry Entry { get; }
            internal Catalog.Contracts.StoryCatalogPreview Preview { get; }
            internal IReadOnlyDictionary<string, Sprite> Covers { get; }
        }

        internal sealed class LoadedCatalog : IDisposable
        {
            internal LoadedCatalog(
                IReadOnlyList<Catalog.NovelCatalogEntry> entries,
                IReadOnlyDictionary<string, Sprite> covers,
                IReadOnlyDictionary<string, Catalog.Contracts.StoryCatalogPreview> previews,
                CatalogDownloads downloads,
                GameObject screen,
                Bundles.ContentDeliveryLease deliveryLease)
            {
                Entries = entries ?? throw new ArgumentNullException(nameof(entries));
                Covers = covers ?? throw new ArgumentNullException(nameof(covers));
                Previews = previews ?? throw new ArgumentNullException(nameof(previews));
                Downloads = downloads ?? throw new ArgumentNullException(nameof(downloads));
                Screen = screen;
                _deliveryLease = deliveryLease;
            }

            private readonly Bundles.ContentDeliveryLease _deliveryLease;

            internal IReadOnlyList<Catalog.NovelCatalogEntry> Entries { get; }
            internal IReadOnlyDictionary<string, Sprite> Covers { get; }
            internal IReadOnlyDictionary<string, Catalog.Contracts.StoryCatalogPreview> Previews { get; }
            internal CatalogDownloads Downloads { get; }
            internal GameObject Screen { get; }

            public void Dispose()
            {
                _deliveryLease?.Dispose();
                Downloads.Dispose();
                foreach (var cover in Covers.Values)
                {
                    if (cover == null)
                        continue;
                    var texture = cover.texture;
                    UnityEngine.Object.Destroy(cover);
                    if (texture != null)
                        UnityEngine.Object.Destroy(texture);
                }
            }
        }

        internal readonly struct StoryLaunchSelection
        {
            internal StoryLaunchSelection(
                Catalog.NovelCatalogEntry content,
                string episodeId,
                bool restartEpisode)
            {
                Content = content;
                EpisodeId = episodeId;
                RestartEpisode = restartEpisode;
            }

            internal Catalog.NovelCatalogEntry Content { get; }
            internal string EpisodeId { get; }
            internal bool RestartEpisode { get; }
        }

        internal struct Dependencies
        {
            internal Bundles.Entity Bundles;
            internal Bundles.IContentSource RootContentSource;
            internal IReadOnlyList<string> StoryIds;
            internal Func<string, Bundles.IContentSource> CreateStoryContentSource;
            internal PriorityLoader PriorityLoader;
            internal string PersistentDataPath;
            internal string ClientVersion;
            internal string ContentPlatform;
            internal CancellationToken CancellationToken;
            internal Action<(LogType type, string message)> OnLog;
            internal Diagnostics.SmokeTelemetry SmokeTelemetry;
            internal Analytics.ProductAnalytics ProductAnalytics;
            internal Func<string, CancellationToken, Bundles.Entity> CreateStoryBundles;
            internal Catalog.ICatalogSettings Settings;
            internal Catalog.CatalogUpdatePrompt UpdatePrompt;
        }

        private readonly Dependencies _ctx;
        private readonly Cache.Entity _progressCache;

        internal CatalogFlow(Dependencies ctx)
        {
            _ctx = ctx;
            if (ctx.Bundles == null)
                throw new ArgumentNullException(nameof(ctx.Bundles));
            if (ctx.RootContentSource == null && ctx.StoryIds == null)
                throw new ArgumentNullException(nameof(ctx.RootContentSource));
            if (ctx.PriorityLoader == null)
                throw new ArgumentNullException(nameof(ctx.PriorityLoader));
            if (string.IsNullOrWhiteSpace(ctx.PersistentDataPath))
                throw new ArgumentException(
                    "Persistent data path must not be empty.",
                    nameof(ctx.PersistentDataPath));
            if (string.IsNullOrWhiteSpace(ctx.ClientVersion))
            {
                throw new ArgumentException(
                    "Client version must not be empty.",
                    nameof(ctx.ClientVersion));
            }
            if (ctx.CreateStoryBundles == null)
                throw new ArgumentNullException(nameof(ctx.CreateStoryBundles));
            _progressCache = new Cache.Entity(ctx.PersistentDataPath);
        }

        internal async UniTask<LoadedCatalog> LoadWithRetry(
            Bootstrap.BootstrapController bootstrap)
        {
            const string loading = ApplicationTexts.CatalogLoading;
            const string failed = ApplicationTexts.CatalogLoadFailed;
            const string retry = ApplicationTexts.Retry;
            while (true)
            {
                Bundles.ContentDeliveryLease deliveryLease = null;
                try
                {
                    _ctx.SmokeTelemetry?.Emit("catalog.loading");
                    bootstrap.ShowLoading(loading);
                    await _ctx.Bundles.LoadReleaseAsync(
                        _ctx.ClientVersion,
                        ContentAddressing.ContentCompatibility.MinimumSupportedSchemaVersion,
                        ContentAddressing.ContentCompatibility.MaximumSupportedSchemaVersion);
                    deliveryLease = await PrepareApplicationContent(bootstrap, loading);
                    var resources = await Load(deliveryLease, bootstrap);
                    _ctx.Bundles.ActivateRelease();
                    _ctx.SmokeTelemetry?.Emit(
                        "catalog.ready",
                        ("storyCount", resources.Entries.Count.ToString()),
                        ("releaseId", _ctx.Bundles.ReleaseId),
                        ("deliveryMode", _ctx.Bundles.DeliveryMode.ToString()));
                    return resources;
                }
                catch (OperationCanceledException)
                    when (_ctx.CancellationToken.IsCancellationRequested)
                {
                    deliveryLease?.Dispose();
                    throw;
                }
                catch (Exception exception) when (
                    exception is Bundles.ContentSourceException
                    || exception is Bundles.ContentIntegrityException)
                {
                    deliveryLease?.Dispose();
                    _ctx.SmokeTelemetry?.Emit(
                        "catalog.load_failed",
                        ("exceptionType", exception.GetType().Name));
                    _ctx.OnLog?.Invoke((
                        LogType.Warning,
                        $"Catalog loading failed: {exception}"));
                    await bootstrap.WaitForRetry(failed, retry);
                }
                catch
                {
                    deliveryLease?.Dispose();
                    throw;
                }
            }
        }

        internal async UniTask<StoryLaunchSelection> SelectContent(
            LoadedCatalog catalog,
            string focusedStoryId = null,
            string focusedEpisodeId = null)
        {
            var entries = catalog.Entries
                .Where(entry => entry.IsEnabled)
                .ToArray();
            if (entries.Length == 0)
                throw new InvalidOperationException("Novel catalog has no enabled stories.");
            var items = entries.Select(entry =>
            {
                var text = entry.Text;
                var started = HasStarted(entry.ContentId);
                var preview = catalog.Previews[entry.ContentId];
                var progress = new NovelProgress(
                    entry.ContentId,
                    preview.contentVersion,
                    preview.episodes.Select(episode => new Content.EpisodeDefinition(
                        entry.ContentId, episode.id, episode.title, episode.description)).ToArray(),
                    _ctx.PersistentDataPath,
                    _ctx.OnLog,
                    resetIncompatible: false);
                var playableIds = new HashSet<string>(
                    progress.PlayableEpisodes.Select(episode => episode.Id),
                    StringComparer.OrdinalIgnoreCase);
                var completedIds = new HashSet<string>(
                    progress.CompletedEpisodeIds,
                    StringComparer.OrdinalIgnoreCase);
                for (var index = 0; index + 1 < entry.Episodes.Count; index++)
                {
                    if (playableIds.Contains(entry.Episodes[index + 1].Id))
                        completedIds.Add(entry.Episodes[index].Id);
                }
                return new Catalog.CatalogItem(
                    entry.ContentId,
                    text.Title,
                    genre: text.Genre,
                    description: text.Description,
                    status: started
                        ? ApplicationTexts.ContinueContent
                        : ApplicationTexts.ContentAvailable,
                    actionLabel: started
                        ? ApplicationTexts.ContinueContent
                        : ApplicationTexts.OpenContent,
                    secondaryActionLabel: started
                        ? ApplicationTexts.StartAgain
                        : null,
                    cover: catalog.Covers.TryGetValue(entry.ContentId, out var cover)
                        ? cover
                        : null,
                    episodes: entry.Episodes.Select((episode, index) =>
                        new Catalog.CatalogEpisodeItem(
                            episode.Id,
                            episode.Title,
                            playableIds.Contains(episode.Id)
                                ? episode.Description
                                : "Чтобы открыть этот эпизод, дочитайте предыдущие эпизоды.",
                            status: BuildEpisodeStatus(
                                entry,
                                episode,
                                index,
                                playableIds,
                                completedIds),
                            actionLabel: !playableIds.Contains(episode.Id)
                                ? "Прочитайте предыдущий эпизод"
                                : completedIds.Contains(episode.Id)
                                    ? string.Empty
                                    : HasEpisodeSave(entry.ContentId, episode.Id)
                                        ? ApplicationTexts.ContinueContent
                                        : ApplicationTexts.OpenContent,
                            restartLabel: completedIds.Contains(episode.Id)
                                || HasEpisodeSave(entry.ContentId, episode.Id)
                                ? ApplicationTexts.StartAgain
                                : null,
                            restartWarning: BuildRestartWarning(entry, index),
                            isEnabled: playableIds.Contains(episode.Id)
                                && !completedIds.Contains(episode.Id),
                            download: catalog.Downloads.GetState(entry.ContentId),
                            cover: GetEpisodeCover(catalog, entry.ContentId, preview.episodes[index].cover),
                            author: preview.episodes[index].author,
                            storyAuthor: entry.Author,
                            videoUrl: GetCatalogVideoUrl(catalog, entry.ContentId, preview, index),
                            readingProgress: completedIds.Contains(episode.Id) ? 1f
                                : !playableIds.Contains(episode.Id) ? null
                                : EpisodeReadingProgress.Read(_progressCache,
                                    NovelRuntime.SaveChoiceKey(entry.ContentId, episode.Id),
                                    preview.contentVersion))));
            }).ToArray();
            using var selection = CreateSelection(catalog.Screen);
            var pendingSelection = selection.SelectAction(
                ApplicationTexts.CatalogTitle,
                items,
                focusedStoryId,
                focusedEpisodeId);
            catalog.Downloads.Start();
            var selected = await pendingSelection;
            if (!catalog.Downloads.GetState(selected.Item.Id).IsReady)
                throw new InvalidOperationException("Cannot launch an episode before its content is ready.");
            var restartEpisode = selected.IsSecondaryAction;
            if (selected.Episode == null)
                throw new InvalidOperationException("Catalog selection has no episode.");
            if (restartEpisode)
                ResetEpisodeSaves(selected.Item.Id, selected.Episode.Id, entries);
            MarkStarted(selected.Item.Id);
            var content = entries.First(entry => string.Equals(
                entry.ContentId,
                selected.Item.Id,
                StringComparison.OrdinalIgnoreCase));
            _ctx.SmokeTelemetry?.Emit(
                "story.selected",
                ("contentId", content.ContentId));
            return new StoryLaunchSelection(
                content,
                selected.Episode.Id,
                restartEpisode);
        }

        private static string BuildEpisodeStatus(
            Catalog.NovelCatalogEntry entry,
            Catalog.NovelCatalogEpisodeEntry episode,
            int index,
            ISet<string> playableIds,
            ISet<string> completedIds)
        {
            if (!playableIds.Contains(episode.Id))
                return "Недоступно";
            if (completedIds.Contains(episode.Id))
                return "Завершено";
            return "Доступно";
        }

        private static string BuildRestartWarning(
            Catalog.NovelCatalogEntry entry,
            int index)
        {
            var affected = entry.Episodes.Count - index;
            return affected > 1
                ? $"Прогресс этого и {affected - 1} следующих эпизодов будет сброшен."
                : "Прогресс этого эпизода будет сброшен.";
        }

        private bool HasEpisodeSave(string contentId, string episodeId) =>
            _progressCache.Exists(NovelRuntime.SaveChoiceKey(contentId, episodeId));

        private void ResetEpisodeSaves(
            string contentId,
            string episodeId,
            IReadOnlyList<Catalog.NovelCatalogEntry> entries)
        {
            var entry = entries.First(value => string.Equals(
                value.ContentId,
                contentId,
                StringComparison.OrdinalIgnoreCase));
            var start = entry.Episodes
                .Select((episode, index) => (episode, index))
                .First(value => string.Equals(
                    value.episode.Id,
                    episodeId,
                    StringComparison.OrdinalIgnoreCase))
                .index;
            for (var index = start; index < entry.Episodes.Count; index++)
            {
                var directory = _progressCache.GetLocalPath(
                    $"Saves/{Uri.EscapeDataString(contentId)}/"
                    + Uri.EscapeDataString(entry.Episodes[index].Id),
                    false);
                if (Directory.Exists(directory))
                    Directory.Delete(directory, true);
            }
        }

        private void ResetStoryProgress(string contentId)
        {
            var directory = _progressCache.GetLocalPath(
                $"Saves/{Uri.EscapeDataString(contentId)}",
                false);
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }

        private bool HasStarted(string contentId)
        {
            if (_progressCache.Exists(StartedKey(contentId)))
                return true;
            var directory = _progressCache.GetLocalPath(
                $"Saves/{Uri.EscapeDataString(contentId)}",
                false);
            return Directory.Exists(directory)
                && Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Any();
        }

        private void MarkStarted(string contentId) =>
            _progressCache.WriteBytes(StartedKey(contentId), new byte[] { 1 });

        private static string StartedKey(string contentId) =>
            $"Saves/{Uri.EscapeDataString(contentId)}/Started";

        private async UniTask<Bundles.ContentDeliveryLease> PrepareApplicationContent(
            Bootstrap.BootstrapController bootstrap,
            string message)
        {
            if (!_ctx.Bundles.HasDeliveryGroup(
                    ContentAddressing.ContentPackageConvention.ApplicationDeliveryGroup))
            {
                return null;
            }
            return await _ctx.Bundles.PrepareDeliveryGroup(
                ContentAddressing.ContentPackageConvention.ApplicationDeliveryGroup,
                progress => ShowProgress(bootstrap, message, progress),
                _ctx.CancellationToken);
        }

        private async UniTask<LoadedCatalog> Load(
            Bundles.ContentDeliveryLease deliveryLease,
            Bootstrap.BootstrapController bootstrap)
        {
            await _ctx.PriorityLoader.Run(() => _ctx.Bundles
                .GetAssetBundle(Catalog.CatalogAddresses.BundleName)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var screen = await _ctx.PriorityLoader.Run(() => _ctx.Bundles
                .GetBundledPrefab(new Bundles.BundleAssetAddress(
                    Catalog.CatalogAddresses.BundleName,
                    Catalog.CatalogAddresses.ScreenAssetName))
                .AttachExternalCancellation(_ctx.CancellationToken));
            if (screen == null)
            {
                throw new InvalidOperationException(
                    $"Catalog assets could not be loaded from "
                    + $"AssetBundle '{Catalog.CatalogAddresses.BundleName}'.");
            }
            var loaded = await LoadEntries(bootstrap);
            return new LoadedCatalog(
                loaded.entries,
                loaded.covers,
                loaded.previews,
                new CatalogDownloads(_ctx, loaded.entries, loaded.previews),
                screen,
                deliveryLease);
        }

        private async UniTask<(
            IReadOnlyList<Catalog.NovelCatalogEntry> entries,
            IReadOnlyDictionary<string, Sprite> covers,
            IReadOnlyDictionary<string, Catalog.Contracts.StoryCatalogPreview> previews)> LoadEntries(
                Bootstrap.BootstrapController bootstrap)
        {
            IReadOnlyList<string> storyIds = _ctx.StoryIds;
            if (storyIds == null)
            {
                var registryJson = await _ctx.RootContentSource.DownloadText(
                    ContentAddressing.ContentPackageConvention.CatalogRegistryPath,
                    _ctx.CancellationToken);
                storyIds = Catalog.Contracts.CatalogContractCodec
                    .DeserializeRegistry(registryJson).stories;
            }
            var entries = new List<Catalog.NovelCatalogEntry>();
            var covers = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            var previews = new Dictionary<string, Catalog.Contracts.StoryCatalogPreview>(
                StringComparer.OrdinalIgnoreCase);
            var completed = new List<LoadedEntry>();
            using var requests = new SemaphoreSlim(
                _maximumParallelPreviewRequests,
                _maximumParallelPreviewRequests);
            try
            {
                var loadedEntries = await UniTask.WhenAll(storyIds.Select(storyId =>
                    LoadEntry(storyId, requests, completed)));
                foreach (var loaded in loadedEntries)
                {
                    entries.Add(loaded.Entry);
                    previews.Add(loaded.Entry.ContentId, loaded.Preview);
                    foreach (var cover in loaded.Covers)
                        covers.Add(cover.Key, cover.Value);
                }
            }
            catch
            {
                DestroyCovers(completed.SelectMany(value => value.Covers.Values));
                throw;
            }
            return (entries, covers, previews);
        }

        private async UniTask<LoadedEntry> LoadEntry(
            string storyId,
            SemaphoreSlim requests,
            ICollection<LoadedEntry> completed)
        {
            var storySource = CreateStoryContentSource(storyId);
            var loadedCovers = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            try
            {
                var (cardJson, previewJson) = await UniTask.WhenAll(
                    RunPreviewRequest(
                        requests,
                        () => storySource.DownloadText("card.json", _ctx.CancellationToken)),
                    RunPreviewRequest(
                        requests,
                        () => storySource.DownloadText(
                            $"Remote/{_ctx.ContentPlatform}/catalog-preview.json",
                            _ctx.CancellationToken)));
                var card = Catalog.Contracts.CatalogContractCodec.DeserializeCard(
                    cardJson,
                    storyId);
                var preview = Catalog.Contracts.CatalogContractCodec.DeserializePreview(
                    previewJson,
                    storyId);
                var coverRequests = new List<UniTask<(string key, Sprite sprite)>>
                {
                    LoadRequiredCover(
                        requests,
                        storyId,
                        preview.releaseId,
                        card.storyId,
                        card.cover,
                        storySource),
                };
                var coverKeys = new HashSet<string>(StringComparer.Ordinal)
                {
                    card.storyId,
                };
                foreach (var episode in preview.episodes)
                {
                    if (string.IsNullOrWhiteSpace(episode.cover))
                        continue;
                    var key = ContentAddressing.ContentPackageConvention.StoryEpisodeCoverPath(
                        storyId,
                        _ctx.ContentPlatform,
                        episode.cover);
                    if (!coverKeys.Add(key))
                        continue;
                    coverRequests.Add(LoadOptionalCover(
                        requests,
                        storyId,
                        preview.releaseId,
                        key,
                        $"Remote/{_ctx.ContentPlatform}/episode-covers/{episode.cover}",
                        episode.cover,
                        storySource));
                }
                foreach (var cover in await UniTask.WhenAll(coverRequests))
                {
                    if (cover.sprite != null && !loadedCovers.ContainsKey(cover.key))
                        loadedCovers.Add(cover.key, cover.sprite);
                    else if (cover.sprite != null)
                        DestroyCovers(new[] { cover.sprite });
                }
                var loaded = new LoadedEntry(
                    new Catalog.NovelCatalogEntry(
                        card.storyId,
                        card.title,
                        card.genre,
                        card.description,
                        preview.episodes.Select(episode =>
                            new Catalog.NovelCatalogEpisodeEntry(
                                episode.id,
                                episode.title,
                                episode.description)),
                        author: card.author),
                    preview,
                    loadedCovers);
                lock (completed)
                    completed.Add(loaded);
                return loaded;
            }
            catch
            {
                DestroyCovers(loadedCovers.Values);
                throw;
            }
        }

        private async UniTask<(string key, Sprite sprite)> LoadRequiredCover(
            SemaphoreSlim requests,
            string storyId,
            string releaseId,
            string key,
            string path,
            Bundles.IContentSource source) =>
            (key, await RunPreviewRequest(
                requests,
                () => LoadCover(
                    path,
                    source,
                    _ctx.CancellationToken,
                    _progressCache,
                    CoverCacheKey(storyId, releaseId, path),
                    _requiredCoverTimeoutSeconds)));

        private async UniTask<(string key, Sprite sprite)> LoadOptionalCover(
            SemaphoreSlim requests,
            string storyId,
            string releaseId,
            string key,
            string path,
            string fileName,
            Bundles.IContentSource source) =>
            (key, await RunPreviewRequest(
                requests,
                () => LoadOptionalEpisodeCoverCached(
                    path,
                    source,
                    _ctx.CancellationToken,
                    _ctx.OnLog,
                    _progressCache,
                    CoverCacheKey(storyId, releaseId, fileName))));

        private async UniTask<T> RunPreviewRequest<T>(
            SemaphoreSlim requests,
            Func<UniTask<T>> request)
        {
            await requests.WaitAsync(_ctx.CancellationToken);
            try
            {
                return await request();
            }
            finally
            {
                requests.Release();
            }
        }

        private static string CoverCacheKey(string storyId, string releaseId, string fileName) =>
            $"CatalogPreviews/{Uri.EscapeDataString(storyId)}/"
            + $"{Uri.EscapeDataString(releaseId)}/{Uri.EscapeDataString(fileName)}";

        private string GetCatalogVideoUrl(LoadedCatalog catalog, string storyId,
            Catalog.Contracts.StoryCatalogPreview preview, int index)
        {
            var video = SelectCatalogVideo(preview, index,
                GetEpisodeCover(catalog, storyId, preview.episodes[index].cover) != null);
            // Resolve only: the visible card prepares the optional stream, never blocks catalog startup.
            return string.IsNullOrWhiteSpace(video) ? null : CreateStoryContentSource(storyId).GetUrl(
                $"Remote/{_ctx.ContentPlatform}/catalog-videos/"
                + ContentAddressing.ContentPackageConvention.CatalogVideoFileName(video));
        }

        private Bundles.IContentSource CreateStoryContentSource(string storyId) =>
            _ctx.CreateStoryContentSource?.Invoke(storyId)
                ?? new Bundles.PrefixedContentSource(
                    _ctx.RootContentSource,
                    ContentAddressing.ContentPackageConvention.StoryPrefix(storyId));

        internal static string SelectCatalogVideo(Catalog.Contracts.StoryCatalogPreview preview,
            int index, bool hasEpisodeCover)
        {
            var ownVideo = preview.episodes[index].video;
            if (!string.IsNullOrWhiteSpace(ownVideo)) return ownVideo;
            // Episode art wins over inherited story media. Failed/missing own art
            // has already resolved to null during loading and permits story fallback.
            if (hasEpisodeCover) return null;
            return string.IsNullOrWhiteSpace(preview.video) ? null : preview.video;
        }

        private static void DestroyCovers(IEnumerable<Sprite> covers)
        {
            foreach (var cover in covers)
            {
                if (cover == null)
                    continue;
                var texture = cover.texture;
                UnityEngine.Object.Destroy(cover);
                if (texture != null)
                    UnityEngine.Object.Destroy(texture);
            }
        }

        private Sprite GetEpisodeCover(LoadedCatalog catalog, string storyId, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return null;
            var path = ContentAddressing.ContentPackageConvention.StoryEpisodeCoverPath(
                storyId, _ctx.ContentPlatform, fileName);
            return catalog.Covers.TryGetValue(path, out var cover) ? cover : null;
        }

        internal static async UniTask<Sprite> LoadOptionalEpisodeCover(string path,
            Bundles.IContentSource source, CancellationToken token,
            Action<(LogType type, string message)> onLog) =>
            await LoadOptionalEpisodeCoverCached(path, source, token, onLog, null, null);

        private static async UniTask<Sprite> LoadOptionalEpisodeCoverCached(
            string path,
            Bundles.IContentSource source,
            CancellationToken token,
            Action<(LogType type, string message)> onLog,
            Cache.Entity cache,
            string cacheKey)
        {
            try
            {
                return await LoadCover(
                    path,
                    source,
                    token,
                    cache,
                    cacheKey,
                    _optionalCoverTimeoutSeconds);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception exception)
            {
                token.ThrowIfCancellationRequested();
                onLog?.Invoke((LogType.Warning,
                    $"Episode cover '{path}' unavailable; using story cover. {exception.Message}"));
                return null;
            }
        }

        private static async UniTask<Sprite> LoadCover(
            string path,
            Bundles.IContentSource source,
            CancellationToken token,
            Cache.Entity cache = null,
            string cacheKey = null,
            int timeoutSeconds = 0)
        {
            token.ThrowIfCancellationRequested();
            if (cache != null && !string.IsNullOrWhiteSpace(cacheKey)
                && cache.Exists(cacheKey))
            {
                try
                {
                    var cached = CreateCover(cache.ReadBytes(cacheKey), path);
                    cache.Touch(cacheKey);
                    return cached;
                }
                catch
                {
                    cache.Delete(cacheKey);
                }
            }
            if (cache != null && !string.IsNullOrWhiteSpace(cacheKey))
            {
                var temporaryPath = cache.CreateTemporaryPath(cacheKey);
                try
                {
                    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
                    if (timeoutSeconds > 0)
                        timeout.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
                    try
                    {
                        await source.DownloadFile(path, temporaryPath, null, timeout.Token);
                    }
                    catch (OperationCanceledException exception)
                        when (!token.IsCancellationRequested)
                    {
                        throw new Bundles.ContentSourceException(
                            $"Story cover '{path}' timed out.",
                            exception);
                    }
                    var sprite = CreateCover(File.ReadAllBytes(temporaryPath), path);
                    try
                    {
                        cache.CommitTemporaryFile(temporaryPath, cacheKey);
                        return sprite;
                    }
                    catch
                    {
                        DestroyCovers(new[] { sprite });
                        throw;
                    }
                }
                finally
                {
                    if (File.Exists(temporaryPath))
                        File.Delete(temporaryPath);
                }
            }
            using var request = UnityWebRequestTexture.GetTexture(
                source.GetUrl(path),
                true);
            request.timeout = timeoutSeconds;
            await request.SendWebRequest().ToUniTask(
                cancellationToken: token);
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new Bundles.ContentSourceException(
                    $"Story cover '{path}' could not be loaded: {request.error}");
            }
            var texture = DownloadHandlerTexture.GetContent(request);
            if (texture == null)
                throw new Bundles.ContentSourceException($"Story cover '{path}' is empty.");
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
        }

        private static Sprite CreateCover(byte[] bytes, string path)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!ImageConversion.LoadImage(texture, bytes, true))
                    throw new Bundles.ContentSourceException(
                        $"Story cover '{path}' could not be decoded.");
                return Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
            }
            catch
            {
                UnityEngine.Object.Destroy(texture);
                throw;
            }
        }

        private Catalog.CatalogController CreateSelection(GameObject screen) =>
            new(
                screen,
                _ctx.CancellationToken,
                _ctx.Settings,
                _ctx.UpdatePrompt,
                (storyId, episodeId) => _ctx.ProductAnalytics?.StoryOpened(
                    storyId,
                    episodeId),
                () => _ctx.ProductAnalytics?.FeedbackOpened());

        private static void ShowProgress(
            Bootstrap.BootstrapController bootstrap,
            string message,
            Bundles.ContentDeliveryProgress progress)
        {
            bootstrap.ShowLoading(
                $"{message} {progress.CompletedItems}/{progress.TotalItems} "
                + $"({progress.Ratio:P0})");
        }
    }
}
