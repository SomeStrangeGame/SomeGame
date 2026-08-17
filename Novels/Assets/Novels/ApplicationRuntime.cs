using System;
using System.Linq;
using System.Threading;
using System.Globalization;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;
using ThreadPriority = UnityEngine.ThreadPriority;

namespace Novels
{
    internal sealed class ApplicationRuntime : BaseDisposable
    {
        private const ThreadPriority _defaultThreadPriority = ThreadPriority.Low;
        private const int _supportedContentSchemaVersion = 1;

        internal struct Ctx
        {
            internal CancellationToken CancellationToken;
            internal Action<(LogType type, string message)> OnLog;
            internal Action<Diagnostics.NovelError> OnError;
            internal Bundles.IContentSource ContentSource;
            internal string PersistentDataPath;
        }

        private readonly Ctx _ctx;
        private readonly PriorityLoader _priorityLoader;
        private readonly Bundles.Entity _bundles;
        private readonly DisposableSlot<Entity> _activeNovel;
        private readonly ApplicationLocalization _localization;
        private readonly Locale.LocaleProvider _locale;

        internal ApplicationRuntime(Ctx ctx)
        {
            _ctx = ctx;
            if (ctx.ContentSource == null)
                throw new ArgumentNullException(nameof(ctx.ContentSource));
            if (string.IsNullOrWhiteSpace(ctx.PersistentDataPath))
                throw new ArgumentException(
                    "Persistent data path must not be empty.",
                    nameof(ctx.PersistentDataPath));
            Application.backgroundLoadingPriority = _defaultThreadPriority;
            _priorityLoader = new PriorityLoader(_defaultThreadPriority);
            _locale = new Locale.LocaleProvider(CultureInfo.CurrentUICulture);
            _localization = new ApplicationLocalization(_locale.Code);
            _bundles = CreateBundles().AddTo(this);
            _activeNovel = new DisposableSlot<Entity>().AddTo(this);
        }

        internal async UniTask Run()
        {
            using var bootstrap = new Bootstrap.Entity(_ctx.CancellationToken)
                .AddTo(this);
            var strings = GetBootstrapStrings();
            var catalog = await LoadCatalogWithRetry(bootstrap, strings);
            bootstrap.Hide();
            while (!_ctx.CancellationToken.IsCancellationRequested)
            {
                var content = await SelectContent(catalog.catalog, catalog.screen);
                var novel = new Entity(new Entity.Ctx
                {
                    Bundles = _bundles,
                    Content = content,
                    Locale = _locale.Code,
                    PersistentDataPath = _ctx.PersistentDataPath,
                    SelectEpisode = definition =>
                        SelectEpisode(definition, catalog.screen),
                    CancellationToken = _ctx.CancellationToken,
                    OnLog = _ctx.OnLog,
                    OnError = _ctx.OnError,
                });
                _activeNovel.Replace(novel);
                try
                {
                    await novel.Init();
                }
                finally
                {
                    _activeNovel.Clear(novel);
                }
            }
        }

        private async UniTask<(
            Catalog.NovelCatalogAsset catalog,
            GameObject screen)> LoadCatalogWithRetry(
                Bootstrap.Entity bootstrap,
                BootstrapStrings strings)
        {
            while (true)
            {
                try
                {
                    bootstrap.ShowLoading(strings.Loading);
                    await _bundles.LoadReleaseAsync(
                        Application.version,
                        _supportedContentSchemaVersion);
                    return await LoadCatalog();
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
                    _ctx.OnLog?.Invoke((
                        LogType.Warning,
                        $"Catalog loading failed: {exception}"));
                    await bootstrap.WaitForRetry(strings.Failed, strings.Retry);
                }
            }
        }

        internal UniTask FlushSaveAsync()
        {
            return _activeNovel.Value?.FlushSaveAsync() ?? UniTask.CompletedTask;
        }

        private Bundles.Entity CreateBundles()
        {
            return new Bundles.Entity(new Bundles.Entity.Ctx
            {
                ContentSource = _ctx.ContentSource,
                PersistentDataPath = _ctx.PersistentDataPath,
                CancellationToken = _ctx.CancellationToken,
                OnLog = _ctx.OnLog,
            });
        }

        private async UniTask<(
            Catalog.NovelCatalogAsset catalog,
            GameObject screen)> LoadCatalog()
        {
            await _priorityLoader.Run(() => _bundles
                .GetAssetBundle(Catalog.CatalogAddresses.BundleName)
                .AttachExternalCancellation(_ctx.CancellationToken));

            var catalog = await _priorityLoader.Run(() => _bundles
                .GetBundledSO<Catalog.NovelCatalogAsset>(
                    new Bundles.BundleAssetAddress(
                        Catalog.CatalogAddresses.BundleName,
                        Catalog.CatalogAddresses.AssetName))
                .AttachExternalCancellation(_ctx.CancellationToken));
            var screen = await _priorityLoader.Run(() => _bundles
                .GetBundledPrefab(
                    new Bundles.BundleAssetAddress(
                        Catalog.CatalogAddresses.BundleName,
                        Catalog.CatalogAddresses.ScreenAssetName))
                .AttachExternalCancellation(_ctx.CancellationToken));
            if (catalog == null || screen == null)
            {
                throw new InvalidOperationException(
                    $"Catalog assets could not be loaded from "
                    + $"AssetBundle '{Catalog.CatalogAddresses.BundleName}'.");
            }

            return (catalog, screen);
        }

        private async UniTask<Catalog.NovelCatalogEntry> SelectContent(
            Catalog.NovelCatalogAsset catalog,
            GameObject screen)
        {
            var entries = catalog.Entries.ToDictionary(
                entry => entry.ContentId,
                StringComparer.OrdinalIgnoreCase);
            var items = catalog.Entries.Select(entry =>
            {
                var text = entry.Resolve(_locale.Code);
                return new Catalog.CatalogItem(
                    entry.ContentId,
                    text.Title,
                    text.Description,
                    _localization.Get(ApplicationText.ContentAvailable));
            }).ToArray();
            using var selection = new Catalog.Entity(new Catalog.Entity.Ctx
            {
                BundledPrefab = screen,
                CancellationToken = _ctx.CancellationToken,
            });
            var selected = await selection.Select(
                catalog.Resolve(_locale.Code).Title,
                items);
            return entries[selected.Id];
        }

        private async UniTask<Content.EpisodeDefinition> SelectEpisode(
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
                    status: _localization.Get(ApplicationText.ContentAvailable)))
                .ToArray();
            using var selection = new Catalog.Entity(new Catalog.Entity.Ctx
            {
                BundledPrefab = screen,
                CancellationToken = _ctx.CancellationToken,
            });
            var title = _localization.Get(ApplicationText.ChooseEpisode);
            var selected = await selection.Select(title, items);
            return episodes[selected.Id];
        }

        private BootstrapStrings GetBootstrapStrings()
        {
            return new BootstrapStrings(
                _localization.Get(ApplicationText.CatalogLoading),
                _localization.Get(ApplicationText.CatalogLoadFailed),
                _localization.Get(ApplicationText.Retry));
        }

        private readonly struct BootstrapStrings
        {
            internal BootstrapStrings(string loading, string failed, string retry)
            {
                Loading = loading;
                Failed = failed;
                Retry = retry;
            }

            internal string Loading { get; }
            internal string Failed { get; }
            internal string Retry { get; }
        }
    }
}
