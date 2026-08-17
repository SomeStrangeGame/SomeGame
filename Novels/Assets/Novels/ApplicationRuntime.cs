using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;
using ThreadPriority = UnityEngine.ThreadPriority;

namespace Novels
{
    internal sealed class ApplicationRuntime : BaseDisposable
    {
        private const ThreadPriority _defaultThreadPriority = ThreadPriority.Low;
        private const int _supportedContentSchemaVersion = 2;

        internal struct Ctx
        {
            internal ApplicationEnvironment Environment;
            internal Action<(LogType type, string message)> OnLog;
            internal Action<Diagnostics.NovelError> OnError;
            internal Bundles.IContentSource ContentSource;
        }

        private readonly Ctx _ctx;
        private readonly ApplicationEnvironment _environment;
        private readonly PriorityLoader _priorityLoader;
        private readonly Bundles.Entity _bundles;
        private readonly DisposableSlot<Entity> _activeNovel;
        private readonly ApplicationLocalization _localization;
        private readonly string _locale;

        internal ApplicationRuntime(Ctx ctx)
        {
            _ctx = ctx;
            _environment = ctx.Environment
                ?? throw new ArgumentNullException(nameof(ctx.Environment));
            if (ctx.ContentSource == null)
                throw new ArgumentNullException(nameof(ctx.ContentSource));
            Application.backgroundLoadingPriority = _defaultThreadPriority;
            _priorityLoader = new PriorityLoader(_defaultThreadPriority);
            _locale = _environment.Locale;
            _localization = new ApplicationLocalization(_locale);
            _bundles = CreateBundles().AddTo(this);
            _activeNovel = new DisposableSlot<Entity>().AddTo(this);
        }

        internal async UniTask Run()
        {
            using var bootstrap = new Bootstrap.Entity(_environment.CancellationToken)
                .AddTo(this);
            var strings = GetBootstrapStrings();
            var catalog = await LoadCatalogWithRetry(bootstrap, strings);
            bootstrap.Hide();
            while (!_environment.CancellationToken.IsCancellationRequested)
            {
                var content = await SelectContent(catalog.catalog, catalog.screen);
                try
                {
                    await PrepareContent(bootstrap, content.ContentId);
                }
                catch (Exception exception) when (
                    exception is Bundles.ContentSourceException
                    || exception is Bundles.ContentIntegrityException
                    || exception is Bundles.ContentStorageException)
                {
                    _ctx.OnError?.Invoke(new Diagnostics.NovelError(
                        Diagnostics.NovelErrorCodes.ContentPreparationFailed,
                        Diagnostics.NovelErrorSeverity.Recoverable,
                        "Story content could not be prepared.",
                        exception: exception));
                    continue;
                }
                var novel = new Entity(new Entity.Ctx
                {
                    Bundles = _bundles,
                    Content = content,
                    Locale = _locale,
                    PersistentDataPath = _environment.PersistentDataPath,
                    TargetCamera = _environment.TargetCamera,
                    SelectEpisode = definition =>
                        SelectEpisode(definition, catalog.screen),
                    CancellationToken = _environment.CancellationToken,
                    OnLog = _ctx.OnLog,
                    OnError = _ctx.OnError,
                });
                _activeNovel.Replace(novel);
                try
                {
                    var result = await novel.Init();
                    if (result.Status == EpisodeRunStatus.Cancelled)
                        return;
                    if (result.Status == EpisodeRunStatus.Failed)
                        _ctx.OnError?.Invoke(result.Error.Value);
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
                        _environment.ClientVersion,
                        _supportedContentSchemaVersion);
                    return await LoadCatalog();
                }
                catch (OperationCanceledException)
                    when (_environment.CancellationToken.IsCancellationRequested)
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
                PersistentDataPath = _environment.PersistentDataPath,
                Platform = _environment.ContentPlatform,
                CancellationToken = _environment.CancellationToken,
                OnLog = _ctx.OnLog,
            });
        }

        private async UniTask<(
            Catalog.NovelCatalogAsset catalog,
            GameObject screen)> LoadCatalog()
        {
            await _priorityLoader.Run(() => _bundles
                .GetAssetBundle(Catalog.CatalogAddresses.BundleName)
                .AttachExternalCancellation(_environment.CancellationToken));

            var catalog = await _priorityLoader.Run(() => _bundles
                .GetBundledSO<Catalog.NovelCatalogAsset>(
                    new Bundles.BundleAssetAddress(
                        Catalog.CatalogAddresses.BundleName,
                        Catalog.CatalogAddresses.AssetName))
                .AttachExternalCancellation(_environment.CancellationToken));
            var screen = await _priorityLoader.Run(() => _bundles
                .GetBundledPrefab(
                    new Bundles.BundleAssetAddress(
                        Catalog.CatalogAddresses.BundleName,
                        Catalog.CatalogAddresses.ScreenAssetName))
                .AttachExternalCancellation(_environment.CancellationToken));
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
                var text = entry.Resolve(_locale);
                return new Catalog.CatalogItem(
                    entry.ContentId,
                    text.Title,
                    text.Description,
                    _localization.Get(ApplicationText.ContentAvailable));
            }).ToArray();
            using var selection = new Catalog.Entity(new Catalog.Entity.Ctx
            {
                BundledPrefab = screen,
                CancellationToken = _environment.CancellationToken,
            });
            var selected = await selection.Select(
                catalog.Resolve(_locale).Title,
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
                CancellationToken = _environment.CancellationToken,
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

        private async UniTask PrepareContent(
            Bootstrap.Entity bootstrap,
            string deliveryGroup)
        {
            if (_bundles.DeliveryMode == Bundles.ContentDeliveryMode.Embedded)
                return;
            var message = _localization.Get(ApplicationText.PreparingContent);
            bootstrap.ShowLoading(message);
            try
            {
                await _bundles.PrepareDeliveryGroup(
                    deliveryGroup,
                    progress => bootstrap.ShowLoading(
                        $"{message} {progress.CompletedFiles}/{progress.TotalFiles}"),
                    _environment.CancellationToken);
            }
            finally
            {
                bootstrap.Hide();
            }
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
