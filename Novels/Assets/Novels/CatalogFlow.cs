using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels
{
    internal sealed class CatalogFlow
    {
        internal sealed class Resources : IDisposable
        {
            internal Resources(
                Catalog.NovelCatalogAsset catalog,
                GameObject screen,
                Bundles.ContentDeliveryLease deliveryLease)
            {
                Catalog = catalog;
                Screen = screen;
                _deliveryLease = deliveryLease;
            }

            private readonly Bundles.ContentDeliveryLease _deliveryLease;

            internal Catalog.NovelCatalogAsset Catalog { get; }
            internal GameObject Screen { get; }

            public void Dispose()
            {
                _deliveryLease?.Dispose();
            }
        }

        internal struct Ctx
        {
            internal Bundles.Entity Bundles;
            internal PriorityLoader PriorityLoader;
            internal ApplicationLocalization Localization;
            internal string Locale;
            internal string ClientVersion;
            internal int MinimumSupportedSchemaVersion;
            internal int MaximumSupportedSchemaVersion;
            internal CancellationToken CancellationToken;
            internal Action<(LogType type, string message)> OnLog;
        }

        private readonly Ctx _ctx;

        internal CatalogFlow(Ctx ctx)
        {
            _ctx = ctx;
            if (ctx.Bundles == null)
                throw new ArgumentNullException(nameof(ctx.Bundles));
            if (ctx.PriorityLoader == null)
                throw new ArgumentNullException(nameof(ctx.PriorityLoader));
            if (ctx.Localization == null)
                throw new ArgumentNullException(nameof(ctx.Localization));
            if (string.IsNullOrWhiteSpace(ctx.Locale))
                throw new ArgumentException("Locale must not be empty.", nameof(ctx.Locale));
            if (string.IsNullOrWhiteSpace(ctx.ClientVersion))
            {
                throw new ArgumentException(
                    "Client version must not be empty.",
                    nameof(ctx.ClientVersion));
            }
        }

        internal async UniTask<Resources> LoadWithRetry(Bootstrap.Entity bootstrap)
        {
            var loading = _ctx.Localization.Get(ApplicationText.CatalogLoading);
            var failed = _ctx.Localization.Get(ApplicationText.CatalogLoadFailed);
            var retry = _ctx.Localization.Get(ApplicationText.Retry);
            while (true)
            {
                Bundles.ContentDeliveryLease deliveryLease = null;
                try
                {
                    bootstrap.ShowLoading(loading);
                    await _ctx.Bundles.LoadReleaseAsync(
                        _ctx.ClientVersion,
                        _ctx.MinimumSupportedSchemaVersion,
                        _ctx.MaximumSupportedSchemaVersion);
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
            Resources resources)
        {
            var entries = resources.Catalog.Entries.ToDictionary(
                entry => entry.ContentId,
                StringComparer.OrdinalIgnoreCase);
            var items = resources.Catalog.Entries.Select(entry =>
            {
                var text = entry.Resolve(_ctx.Locale);
                return new Catalog.CatalogItem(
                    entry.ContentId,
                    text.Title,
                    text.Description,
                    _ctx.Localization.Get(ApplicationText.ContentAvailable));
            }).ToArray();
            using var selection = CreateSelection(resources.Screen);
            var selected = await selection.Select(
                resources.Catalog.Resolve(_ctx.Locale).Title,
                items);
            return entries[selected.Id];
        }

        internal async UniTask<Content.EpisodeDefinition> SelectEpisode(
            Content.NovelDefinition definition,
            GameObject screen)
        {
            var episodes = definition.Episodes.ToDictionary(
                episode => episode.Id,
                StringComparer.OrdinalIgnoreCase);
            var items = definition.Episodes
                .Select(episode => new Catalog.CatalogItem(
                    episode.Id,
                    episode.Title,
                    status: _ctx.Localization.Get(ApplicationText.ContentAvailable)))
                .ToArray();
            using var selection = CreateSelection(screen);
            var selected = await selection.Select(
                _ctx.Localization.Get(ApplicationText.ChooseEpisode),
                items);
            return episodes[selected.Id];
        }

        private async UniTask<Bundles.ContentDeliveryLease> PrepareApplicationContent(
            Bootstrap.Entity bootstrap,
            string message)
        {
            if (_ctx.Bundles.DeliveryMode == Bundles.ContentDeliveryMode.Embedded
                || !_ctx.Bundles.HasDeliveryGroup(
                    ContentAddressing.ContentPackageConvention.ApplicationDeliveryGroup))
            {
                return null;
            }
            return await _ctx.Bundles.PrepareDeliveryGroup(
                ContentAddressing.ContentPackageConvention.ApplicationDeliveryGroup,
                progress => ShowProgress(bootstrap, message, progress),
                _ctx.CancellationToken);
        }

        private async UniTask<Resources> Load(
            Bundles.ContentDeliveryLease deliveryLease)
        {
            await _ctx.PriorityLoader.Run(() => _ctx.Bundles
                .GetAssetBundle(Catalog.CatalogAddresses.BundleName)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var catalog = await _ctx.PriorityLoader.Run(() => _ctx.Bundles
                .GetBundledSO<Catalog.NovelCatalogAsset>(
                    new Bundles.BundleAssetAddress(
                        Catalog.CatalogAddresses.BundleName,
                        Catalog.CatalogAddresses.AssetName))
                .AttachExternalCancellation(_ctx.CancellationToken));
            var screen = await _ctx.PriorityLoader.Run(() => _ctx.Bundles
                .GetBundledPrefab(new Bundles.BundleAssetAddress(
                    Catalog.CatalogAddresses.BundleName,
                    Catalog.CatalogAddresses.ScreenAssetName))
                .AttachExternalCancellation(_ctx.CancellationToken));
            if (catalog == null || screen == null)
            {
                throw new InvalidOperationException(
                    $"Catalog assets could not be loaded from "
                    + $"AssetBundle '{Catalog.CatalogAddresses.BundleName}'.");
            }
            return new Resources(catalog, screen, deliveryLease);
        }

        private Catalog.Entity CreateSelection(GameObject screen) =>
            new(new Catalog.Entity.Ctx
            {
                BundledPrefab = screen,
                CancellationToken = _ctx.CancellationToken,
            });

        private static void ShowProgress(
            Bootstrap.Entity bootstrap,
            string message,
            Bundles.ContentDeliveryProgress progress)
        {
            bootstrap.ShowLoading(
                $"{message} {progress.CompletedItems}/{progress.TotalItems} "
                + $"({progress.Ratio:P0})");
        }
    }
}
