using System;
using System.Collections.Generic;
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
        internal struct Dependencies
        {
            internal ApplicationEnvironment Environment;
            internal Action<(LogType type, string message)> OnLog;
            internal Action<Diagnostics.NovelError> OnError;
            internal Diagnostics.SmokeTelemetry SmokeTelemetry;
            internal Analytics.ProductAnalytics ProductAnalytics;
            internal Bundles.IContentSource ContentSource;
            internal Bundles.IContentSource CatalogContentSource;
            internal IReadOnlyList<string> StoryIds;
            internal Func<string, Bundles.IContentSource> CreateStoryContentSource;
            internal Action<StoryProcessor.StorySourceLocation> OnStorySourceChanged;
            internal Notifications.LocalNotificationCoordinator Notifications;
            internal Notifications.NotificationRoute InitialNotificationRoute;
            internal Catalog.CatalogUpdatePrompt UpdatePrompt;
        }

        private readonly ApplicationEnvironment _environment;
        private readonly Bundles.IContentSource _contentSource;
        private readonly Func<string, Bundles.IContentSource> _createStoryContentSource;
        private readonly Action<(LogType type, string message)> _onLog;
        private readonly Action<Diagnostics.NovelError> _onError;
        private readonly Diagnostics.SmokeTelemetry _smokeTelemetry;
        private readonly Analytics.ProductAnalytics _productAnalytics;
        private readonly Action<StoryProcessor.StorySourceLocation> _onStorySourceChanged;
        private readonly Bundles.Entity _catalogBundles;
        private readonly DisposableSlot<NovelRuntime> _activeNovel;
        private readonly CatalogFlow _catalogFlow;
        private readonly ApplicationAudioSettings _audioSettings;
        private readonly Notifications.LocalNotificationCoordinator _notifications;
        private Notifications.NotificationRoute _pendingNotificationRoute;
        private CatalogFlow.LoadedCatalog _activeCatalog;

        internal ApplicationRuntime(Dependencies ctx)
        {
            _environment = ctx.Environment
                ?? throw new ArgumentNullException(nameof(ctx.Environment));
            _contentSource = ctx.ContentSource;
            _createStoryContentSource = ctx.CreateStoryContentSource;
            var catalogContentSource = ctx.CatalogContentSource;
            if (catalogContentSource == null)
            {
                if (_contentSource == null)
                    throw new ArgumentNullException(nameof(ctx.ContentSource));
                catalogContentSource = new Bundles.PrefixedContentSource(
                    _contentSource,
                    ContentAddressing.ContentPackageConvention.CatalogUiPrefix);
            }
            _onLog = ctx.OnLog;
            _onError = ctx.OnError;
            _smokeTelemetry = ctx.SmokeTelemetry;
            _productAnalytics = ctx.ProductAnalytics;
            _onStorySourceChanged = ctx.OnStorySourceChanged;
            _notifications = ctx.Notifications;
            _pendingNotificationRoute = ctx.InitialNotificationRoute;
            _audioSettings = new ApplicationAudioSettings().AddTo(this);
            Application.backgroundLoadingPriority = _defaultThreadPriority;
            _catalogBundles = CreateBundles(
                catalogContentSource,
                "catalog").AddTo(this);
            _catalogFlow = new CatalogFlow(new CatalogFlow.Dependencies
            {
                Bundles = _catalogBundles,
                RootContentSource = _contentSource,
                StoryIds = ctx.StoryIds,
                CreateStoryContentSource = ctx.CreateStoryContentSource,
                PriorityLoader = new PriorityLoader(_defaultThreadPriority),
                PersistentDataPath = _environment.PersistentDataPath,
                ClientVersion = _environment.ClientVersion,
                ContentPlatform = string.IsNullOrWhiteSpace(_environment.ContentPlatform)
                    ? Bundles.ContentPlatform.GetCurrent() : _environment.ContentPlatform,
                CancellationToken = _environment.CancellationToken,
                OnLog = _onLog,
                SmokeTelemetry = _smokeTelemetry,
                ProductAnalytics = _productAnalytics,
                CreateStoryBundles = CreateStoryBundles,
                Settings = _audioSettings,
                UpdatePrompt = ctx.UpdatePrompt,
            });
            _activeNovel = new DisposableSlot<NovelRuntime>().AddTo(this);
        }

        internal async UniTask Run(Bootstrap.BootstrapController bootstrap)
        {
            if (bootstrap == null)
                throw new ArgumentNullException(nameof(bootstrap));
            _audioSettings.Apply();
            using var catalog = await _catalogFlow.LoadWithRetry(bootstrap);
            _activeCatalog = catalog;
            _notifications?.SetCatalog(catalog.Entries);
            bootstrap.Hide();
            _productAnalytics?.CatalogOpened();
            try
            {
                while (!_environment.CancellationToken.IsCancellationRequested)
                {
                    var route = _pendingNotificationRoute;
                    _pendingNotificationRoute = null;
                    var launch = await _catalogFlow.SelectContent(
                        catalog,
                        route?.storyId,
                        route?.episodeId);
                    _notifications?.SetReadingTarget(
                        launch.Content.ContentId,
                        launch.EpisodeId,
                        launch.Content.Text.Title);
                    if (!await RunStory(
                            launch.Content,
                            launch.EpisodeId,
                            launch.RestartEpisode,
                            bootstrap,
                            catalog.Downloads.GetReadyBundles(launch.Content.ContentId)))
                        return;
                    _smokeTelemetry?.Emit(
                        "catalog.returned",
                        ("contentId", launch.Content.ContentId));
                    _productAnalytics?.CatalogOpened();
                    bootstrap.Hide();
                }
            }
            finally
            {
                _activeCatalog = null;
            }
        }

        internal void NavigateToCatalog(Notifications.NotificationRoute route)
        {
            if (route?.IsValid != true)
                return;
            _pendingNotificationRoute = route;
            if (_activeNovel.Value != null)
            {
                _activeNovel.Clear();
                return;
            }
            _activeCatalog?.Screen
                ?.GetComponent<Catalog.View.CatalogScreen>()
                ?.Focus(route.storyId, route.episodeId);
        }

        private async UniTask<bool> RunStory(
            Catalog.NovelCatalogEntry content,
            string episodeId,
            bool restartEpisode,
            Bootstrap.BootstrapController bootstrap,
            Bundles.Entity storyBundles)
        {
            // Borrow the verified/pinned release prepared by the catalog queue.
            // Loading a fresh release here could launch a different, not-yet-downloaded version.
            var contentDeliveryFlow = new ContentDeliveryFlow(
                storyBundles,
                _environment.CancellationToken);
            var novel = new NovelRuntime(new NovelRuntime.Dependencies
            {
                Bundles = storyBundles,
                Content = content,
                PersistentDataPath = _environment.PersistentDataPath,
                TargetCamera = _environment.TargetCamera,
                AudioMixer = _environment.AudioMixer,
                FallbackAssets = _environment.FallbackAssets,
                RuntimeTuning = _environment.RuntimeTuning,
                SelectedEpisodeId = episodeId,
                RestartSelectedEpisode = restartEpisode,
                PrepareNovelContent = contentId =>
                    contentDeliveryFlow.PrepareStoryInitial(bootstrap, contentId),
                HidePreparationScreen = bootstrap.Hide,
                CancellationToken = _environment.CancellationToken,
                OnLog = _onLog,
                OnError = _onError,
                SmokeTelemetry = _smokeTelemetry,
                ProductAnalytics = _productAnalytics,
                OnStorySourceChanged = _onStorySourceChanged,
            });
            _activeNovel.Replace(novel);
            var storyReleaseLoaded = false;
            try
            {
                EpisodeRunResult result;
                try
                {
                    storyReleaseLoaded = true;
                    storyBundles.ActivateRelease();
                    _smokeTelemetry?.Emit(
                        "release.activated",
                        ("scope", "story"),
                        ("contentId", content.ContentId),
                        ("releaseId", storyBundles.ReleaseId),
                        ("deliveryMode", storyBundles.DeliveryMode.ToString()));
                    result = await novel.Init();
                }
                catch (Exception exception) when (
                    exception is Bundles.ContentSourceException
                    || exception is Bundles.ContentIntegrityException
                    || exception is Bundles.ContentStorageException
                    || exception is Bundles.ContentConfigurationException)
                {
                    _onError?.Invoke(new Diagnostics.NovelError(
                        Diagnostics.NovelErrorCodes.ContentPreparationFailed,
                        Diagnostics.NovelErrorSeverity.Recoverable,
                        "Story content could not be prepared.",
                        exception: exception,
                        context: new Diagnostics.NovelErrorContext(
                            storyReleaseLoaded ? storyBundles.ReleaseId : string.Empty,
                            content.ContentId,
                            deliveryMode: storyReleaseLoaded
                                ? storyBundles.DeliveryMode.ToString()
                                : Bundles.ContentDeliveryMode.Remote.ToString())));
                    return true;
                }
                if (result.Status == EpisodeRunStatus.Cancelled)
                    return _pendingNotificationRoute != null;
                if (result.Status == EpisodeRunStatus.Failed)
                    _onError?.Invoke(result.Error.Value);
                if (result.Status == EpisodeRunStatus.Completed)
                {
                    _notifications?.ClearReadingTarget(
                        content.ContentId,
                        episodeId);
                }
                return true;
            }
            finally
            {
                _productAnalytics?.StopReading();
                _onStorySourceChanged?.Invoke(default);
                _activeNovel.Clear(novel);
            }
        }

        internal UniTask FlushSaveAsync()
        {
            return _activeNovel.Value?.FlushSaveAsync() ?? UniTask.CompletedTask;
        }

        internal void FlushSaveSynchronously()
        {
            _audioSettings.Save();
            _activeNovel.Value?.FlushSaveSynchronously();
        }

        private Bundles.Entity CreateBundles(
            Bundles.IContentSource source,
            string cacheNamespace,
            CancellationToken? cancellationToken = null)
        {
            return new Bundles.Entity(new Bundles.Entity.Ctx
            {
                ContentSource = source,
                PersistentDataPath = _environment.PersistentDataPath,
                CacheNamespace = cacheNamespace,
                Platform = _environment.ContentPlatform,
                DeliveryOptions = _environment.RuntimeTuning.ContentDelivery,
                CancellationToken = cancellationToken ?? _environment.CancellationToken,
                OnLog = _onLog,
            });
        }

        private Bundles.Entity CreateStoryBundles(string contentId, CancellationToken cancellationToken) =>
            CreateBundles(
                _createStoryContentSource?.Invoke(contentId)
                    ?? new Bundles.PrefixedContentSource(
                        _contentSource,
                        ContentAddressing.ContentPackageConvention.StoryPrefix(contentId)),
                $"story-{contentId}", cancellationToken);
    }
}
