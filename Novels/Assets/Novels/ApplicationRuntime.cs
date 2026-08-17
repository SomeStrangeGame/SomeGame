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
        private Entity _activeNovel;

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
            _bundles = CreateBundles().AddTo(this);
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
                using var novel = new Entity(new Entity.Ctx
                {
                    Bundles = _bundles,
                    Content = content,
                    SelectEpisode = definition =>
                        SelectEpisode(definition, catalog.screen),
                    CancellationToken = _ctx.CancellationToken,
                    OnLog = _ctx.OnLog,
                    OnError = _ctx.OnError,
                }).AddTo(this);
                _activeNovel = novel;
                try
                {
                    await novel.Init();
                }
                finally
                {
                    _activeNovel = null;
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
                    return await LoadCatalog();
                }
                catch (OperationCanceledException)
                    when (_ctx.CancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
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
            return _activeNovel?.FlushSaveAsync() ?? UniTask.CompletedTask;
        }

        private Bundles.Entity CreateBundles()
        {
            return new Bundles.Entity(new Bundles.Entity.Ctx
            {
                ContentSource = _ctx.ContentSource,
                PersistentDataPath = _ctx.PersistentDataPath,
                CancellationToken = _ctx.CancellationToken,
                OnLog = _ctx.OnLog,
                OnFailure = failure => _ctx.OnError?.Invoke(
                    new Diagnostics.NovelError(
                        Diagnostics.NovelErrorCodes.BundleFailure,
                        Diagnostics.NovelErrorSeverity.Recoverable,
                        $"[{failure.Code}] {failure.Message}",
                        exception: failure.Exception)),
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
                    Catalog.CatalogAddresses.BundleName,
                    Catalog.CatalogAddresses.AssetName)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var screen = await _priorityLoader.Run(() => _bundles
                .GetBundledPrefab(
                    Catalog.CatalogAddresses.BundleName,
                    Catalog.CatalogAddresses.ScreenAssetName)
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
            var entries = catalog.Entries.ToDictionary(entry => entry.ContentId);
            var items = catalog.Entries.Select(entry =>
            {
                var text = entry.Resolve();
                return new Catalog.CatalogItem(
                    entry.ContentId,
                    text.Title,
                    text.Description);
            }).ToArray();
            using var selection = new Catalog.Entity(new Catalog.Entity.Ctx
            {
                BundledPrefab = screen,
                CancellationToken = _ctx.CancellationToken,
            });
            var selected = await selection.Select(catalog.Resolve().Title, items);
            return entries[selected.Id];
        }

        private async UniTask<Content.EpisodeDefinition> SelectEpisode(
            Content.NovelDefinition definition,
            GameObject screen)
        {
            var episodes = definition.Episodes.ToDictionary(episode => episode.Id);
            var items = definition.Episodes
                .Select(episode => new Catalog.CatalogItem(
                    episode.Id,
                    episode.Title))
                .ToArray();
            using var selection = new Catalog.Entity(new Catalog.Entity.Ctx
            {
                BundledPrefab = screen,
                CancellationToken = _ctx.CancellationToken,
            });
            var title = string.Equals(
                CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
                "ru",
                StringComparison.OrdinalIgnoreCase)
                ? "Выберите эпизод"
                : "Choose an episode";
            var selected = await selection.Select(title, items);
            return episodes[selected.Id];
        }

        private static BootstrapStrings GetBootstrapStrings()
        {
            return string.Equals(
                CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
                "ru",
                StringComparison.OrdinalIgnoreCase)
                ? new BootstrapStrings(
                    "Загрузка каталога историй…",
                    "Не удалось загрузить каталог. Проверьте подключение.",
                    "Повторить")
                : new BootstrapStrings(
                    "Loading story catalog…",
                    "Could not load the catalog. Check your connection.",
                    "Retry");
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
