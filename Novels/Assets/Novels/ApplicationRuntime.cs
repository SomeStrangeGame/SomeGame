using System;
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
            internal Bundles.IContentSource ContentSource;
            internal Action<StoryProcessor.StorySourceLocation> OnStorySourceChanged;
        }

        private readonly ApplicationEnvironment _environment;
        private readonly Bundles.IContentSource _contentSource;
        private readonly Action<(LogType type, string message)> _onLog;
        private readonly Action<Diagnostics.NovelError> _onError;
        private readonly Action<StoryProcessor.StorySourceLocation> _onStorySourceChanged;
        private readonly Bundles.Entity _catalogBundles;
        private readonly DisposableSlot<NovelRuntime> _activeNovel;
        private readonly CatalogFlow _catalogFlow;

        internal ApplicationRuntime(Dependencies ctx)
        {
            _environment = ctx.Environment
                ?? throw new ArgumentNullException(nameof(ctx.Environment));
            _contentSource = ctx.ContentSource
                ?? throw new ArgumentNullException(nameof(ctx.ContentSource));
            _onLog = ctx.OnLog;
            _onError = ctx.OnError;
            _onStorySourceChanged = ctx.OnStorySourceChanged;
            Application.backgroundLoadingPriority = _defaultThreadPriority;
            _catalogBundles = CreateBundles(
                new Bundles.PrefixedContentSource(
                    _contentSource,
                    ContentAddressing.ContentPackageConvention.CatalogUiPrefix),
                "catalog").AddTo(this);
            _catalogFlow = new CatalogFlow(new CatalogFlow.Dependencies
            {
                Bundles = _catalogBundles,
                RootContentSource = _contentSource,
                PriorityLoader = new PriorityLoader(_defaultThreadPriority),
                ClientVersion = _environment.ClientVersion,
                CancellationToken = _environment.CancellationToken,
                OnLog = _onLog,
            });
            _activeNovel = new DisposableSlot<NovelRuntime>().AddTo(this);
        }

        internal async UniTask Run()
        {
            using var bootstrap = new Bootstrap.BootstrapController(_environment.CancellationToken);
            using var catalog = await _catalogFlow.LoadWithRetry(bootstrap);
            bootstrap.Hide();
            while (!_environment.CancellationToken.IsCancellationRequested)
            {
                var content = await _catalogFlow.SelectContent(catalog);
                if (!await RunStory(content, catalog, bootstrap))
                    return;
                bootstrap.Hide();
            }
        }

        private async UniTask<bool> RunStory(
            Catalog.NovelCatalogEntry content,
            CatalogFlow.LoadedCatalog catalog,
            Bootstrap.BootstrapController bootstrap)
        {
            using var storyBundles = CreateBundles(
                new Bundles.PrefixedContentSource(
                    _contentSource,
                    ContentAddressing.ContentPackageConvention.StoryPrefix(
                        content.ContentId)),
                $"story-{content.ContentId}");
            var contentDeliveryFlow = new ContentDeliveryFlow(
                storyBundles,
                _environment.CancellationToken);
            var novel = new NovelRuntime(new NovelRuntime.Dependencies
            {
                Bundles = storyBundles,
                Content = content,
                PersistentDataPath = _environment.PersistentDataPath,
                TargetCamera = _environment.TargetCamera,
                FallbackAssets = _environment.FallbackAssets,
                RuntimeTuning = _environment.RuntimeTuning,
                SelectEpisode = definition =>
                    _catalogFlow.SelectEpisode(definition, catalog.Screen),
                PrepareNovelContent = contentId =>
                    contentDeliveryFlow.PrepareStory(bootstrap, contentId),
                HidePreparationScreen = bootstrap.Hide,
                CancellationToken = _environment.CancellationToken,
                OnLog = _onLog,
                OnError = _onError,
                OnStorySourceChanged = _onStorySourceChanged,
            });
            _activeNovel.Replace(novel);
            var storyReleaseLoaded = false;
            try
            {
                EpisodeRunResult result;
                try
                {
                    await storyBundles.LoadReleaseAsync(
                        _environment.ClientVersion,
                        ContentAddressing.ContentCompatibility.MinimumSupportedSchemaVersion,
                        ContentAddressing.ContentCompatibility.MaximumSupportedSchemaVersion);
                    storyReleaseLoaded = true;
                    storyBundles.ActivateRelease();
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
                    return false;
                if (result.Status == EpisodeRunStatus.Failed)
                    _onError?.Invoke(result.Error.Value);
                return true;
            }
            finally
            {
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
            _activeNovel.Value?.FlushSaveSynchronously();
        }

        private Bundles.Entity CreateBundles(
            Bundles.IContentSource source,
            string cacheNamespace)
        {
            return new Bundles.Entity(new Bundles.Entity.Ctx
            {
                ContentSource = source,
                PersistentDataPath = _environment.PersistentDataPath,
                CacheNamespace = cacheNamespace,
                Platform = _environment.ContentPlatform,
                DeliveryOptions = _environment.RuntimeTuning.ContentDelivery,
                CancellationToken = _environment.CancellationToken,
                OnLog = _onLog,
            });
        }
    }
}
