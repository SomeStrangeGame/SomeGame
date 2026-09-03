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
        internal sealed class LoadedCatalog : IDisposable
        {
            internal LoadedCatalog(
                IReadOnlyList<Catalog.NovelCatalogEntry> entries,
                IReadOnlyDictionary<string, Sprite> covers,
                GameObject screen,
                Bundles.ContentDeliveryLease deliveryLease)
            {
                Entries = entries ?? throw new ArgumentNullException(nameof(entries));
                Covers = covers ?? throw new ArgumentNullException(nameof(covers));
                Screen = screen;
                _deliveryLease = deliveryLease;
            }

            private readonly Bundles.ContentDeliveryLease _deliveryLease;

            internal IReadOnlyList<Catalog.NovelCatalogEntry> Entries { get; }
            internal IReadOnlyDictionary<string, Sprite> Covers { get; }
            internal GameObject Screen { get; }

            public void Dispose()
            {
                _deliveryLease?.Dispose();
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

        internal readonly struct EpisodeLaunchSelection
        {
            internal EpisodeLaunchSelection(
                Content.EpisodeDefinition episode,
                bool startNew)
            {
                Episode = episode;
                StartNew = startNew;
            }

            internal Content.EpisodeDefinition Episode { get; }
            internal bool StartNew { get; }
        }

        internal struct Dependencies
        {
            internal Bundles.Entity Bundles;
            internal Bundles.IContentSource RootContentSource;
            internal PriorityLoader PriorityLoader;
            internal string PersistentDataPath;
            internal string ClientVersion;
            internal CancellationToken CancellationToken;
            internal Action<(LogType type, string message)> OnLog;
            internal Diagnostics.SmokeTelemetry SmokeTelemetry;
        }

        private readonly Dependencies _ctx;
        private readonly Cache.Entity _progressCache;

        internal CatalogFlow(Dependencies ctx)
        {
            _ctx = ctx;
            if (ctx.Bundles == null)
                throw new ArgumentNullException(nameof(ctx.Bundles));
            if (ctx.RootContentSource == null)
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
                    var resources = await Load(deliveryLease);
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

        internal async UniTask<Catalog.NovelCatalogEntry> SelectContent(
            LoadedCatalog catalog)
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
                    cover: catalog.Covers.TryGetValue(entry.ContentId, out var cover)
                        ? cover
                        : null);
            }).ToArray();
            using var selection = CreateSelection(catalog.Screen);
            var selected = await selection.Select(
                ApplicationTexts.CatalogTitle,
                items);
            MarkStarted(selected.Id);
            var content = entries.First(entry => string.Equals(
                entry.ContentId,
                selected.Id,
                StringComparison.OrdinalIgnoreCase));
            _ctx.SmokeTelemetry?.Emit(
                "story.selected",
                ("contentId", content.ContentId));
            return content;
        }

        internal async UniTask<EpisodeLaunchSelection> SelectEpisode(
            Content.NovelDefinition definition,
            GameObject screen)
        {
            var items = definition.Episodes
                .Select(episode =>
                {
                    var hasSave = _progressCache.Exists(
                        NovelRuntime.SaveChoiceKey(definition.Id, episode.Id));
                    return new Catalog.CatalogItem(
                        episode.Id,
                        episode.Title,
                        description: episode.Description,
                        status: hasSave
                            ? ApplicationTexts.ContinueContent
                            : ApplicationTexts.ContentAvailable,
                        actionLabel: hasSave
                            ? ApplicationTexts.ContinueContent
                            : ApplicationTexts.NewGame,
                        secondaryActionLabel: hasSave
                            ? ApplicationTexts.StartAgain
                            : null);
                })
                .ToArray();
            using var selection = CreateSelection(screen);
            var selected = await selection.SelectAction(
                ApplicationTexts.ChooseEpisode,
                items);
            var episode = definition.Episodes.First(candidate => string.Equals(
                candidate.Id,
                selected.Item.Id,
                StringComparison.OrdinalIgnoreCase));
            _ctx.SmokeTelemetry?.Emit(
                "episode.selected",
                ("contentId", definition.Id),
                ("episodeId", episode.Id));
            var hasSave = _progressCache.Exists(
                NovelRuntime.SaveChoiceKey(definition.Id, episode.Id));
            return new EpisodeLaunchSelection(
                episode,
                startNew: selected.IsSecondaryAction || !hasSave);
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
            Bundles.ContentDeliveryLease deliveryLease)
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
            var loaded = await LoadEntries();
            return new LoadedCatalog(loaded.entries, loaded.covers, screen, deliveryLease);
        }

        private async UniTask<(
            IReadOnlyList<Catalog.NovelCatalogEntry> entries,
            IReadOnlyDictionary<string, Sprite> covers)> LoadEntries()
        {
            var registryJson = await _ctx.RootContentSource.DownloadText(
                ContentAddressing.ContentPackageConvention.CatalogRegistryPath,
                _ctx.CancellationToken);
            var registry = Catalog.Contracts.CatalogContractCodec
                .DeserializeRegistry(registryJson);
            var entries = new List<Catalog.NovelCatalogEntry>();
            var covers = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var storyId in registry.stories)
                {
                    var cardJson = await _ctx.RootContentSource.DownloadText(
                        ContentAddressing.ContentPackageConvention.StoryCardPath(
                            storyId),
                        _ctx.CancellationToken);
                    var card = Catalog.Contracts.CatalogContractCodec.DeserializeCard(
                        cardJson,
                        storyId);
                    covers.Add(card.storyId, await LoadCover(card));
                    entries.Add(new Catalog.NovelCatalogEntry(
                        card.storyId,
                        card.title,
                        card.genre,
                        card.description));
                }
            }
            catch
            {
                DestroyCovers(covers.Values);
                throw;
            }
            return (entries, covers);
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

        private async UniTask<Sprite> LoadCover(Catalog.Contracts.StoryCard card)
        {
            var path = ContentAddressing.ContentPackageConvention.StoryCoverPath(
                card.storyId,
                card.cover);
            using var request = UnityWebRequestTexture.GetTexture(
                _ctx.RootContentSource.GetUrl(path),
                true);
            await request.SendWebRequest().ToUniTask(
                cancellationToken: _ctx.CancellationToken);
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

        private Catalog.CatalogController CreateSelection(GameObject screen) =>
            new(screen, _ctx.CancellationToken);

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
