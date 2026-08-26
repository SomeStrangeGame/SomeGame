using System;
using System.Collections.Generic;
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

        internal struct Dependencies
        {
            internal Bundles.Entity Bundles;
            internal Bundles.IContentSource RootContentSource;
            internal PriorityLoader PriorityLoader;
            internal string ClientVersion;
            internal CancellationToken CancellationToken;
            internal Action<(LogType type, string message)> OnLog;
        }

        private readonly Dependencies _ctx;

        internal CatalogFlow(Dependencies ctx)
        {
            _ctx = ctx;
            if (ctx.Bundles == null)
                throw new ArgumentNullException(nameof(ctx.Bundles));
            if (ctx.RootContentSource == null)
                throw new ArgumentNullException(nameof(ctx.RootContentSource));
            if (ctx.PriorityLoader == null)
                throw new ArgumentNullException(nameof(ctx.PriorityLoader));
            if (string.IsNullOrWhiteSpace(ctx.ClientVersion))
            {
                throw new ArgumentException(
                    "Client version must not be empty.",
                    nameof(ctx.ClientVersion));
            }
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
                    bootstrap.ShowLoading(loading);
                    await _ctx.Bundles.LoadReleaseAsync(
                        _ctx.ClientVersion,
                        ContentAddressing.ContentCompatibility.MinimumSupportedSchemaVersion,
                        ContentAddressing.ContentCompatibility.MaximumSupportedSchemaVersion);
                    deliveryLease = await PrepareApplicationContent(bootstrap, loading);
                    var resources = await Load(deliveryLease);
                    _ctx.Bundles.ActivateRelease();
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
                return new Catalog.CatalogItem(
                    entry.ContentId,
                    text.Title,
                    text.Description,
                    ApplicationTexts.ContentAvailable,
                    cover: catalog.Covers.TryGetValue(entry.ContentId, out var cover)
                        ? cover
                        : null);
            }).ToArray();
            using var selection = CreateSelection(catalog.Screen);
            var selected = await selection.Select(
                ApplicationTexts.CatalogTitle,
                items);
            return entries.First(entry => string.Equals(
                entry.ContentId,
                selected.Id,
                StringComparison.OrdinalIgnoreCase));
        }

        internal async UniTask<Content.EpisodeDefinition> SelectEpisode(
            Content.NovelDefinition definition,
            GameObject screen,
            Catalog.CatalogAction downloadAllAction = null)
        {
            var items = definition.Episodes
                .Select(episode => new Catalog.CatalogItem(
                    episode.Id,
                    episode.Title,
                    status: ApplicationTexts.ContentAvailable))
                .ToArray();
            using var selection = CreateSelection(screen);
            var selected = await selection.Select(
                ApplicationTexts.ChooseEpisode,
                items,
                downloadAllAction);
            return definition.Episodes.First(episode => string.Equals(
                episode.Id,
                selected.Id,
                StringComparison.OrdinalIgnoreCase));
        }

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
