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
        private enum ApplicationFlowState
        {
            LoadingCatalog,
            SelectingStory,
            RunningStory,
            ReturningToCatalog,
            Completed,
        }

        private const ThreadPriority _defaultThreadPriority = ThreadPriority.Low;
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
        private readonly CatalogFlow _catalogFlow;
        private readonly ContentDeliveryFlow _contentDeliveryFlow;
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
            _catalogFlow = new CatalogFlow(new CatalogFlow.Ctx
            {
                Bundles = _bundles,
                PriorityLoader = _priorityLoader,
                Localization = _localization,
                Locale = _locale,
                ClientVersion = _environment.ClientVersion,
                MinimumSupportedSchemaVersion =
                    ContentAddressing.ContentCompatibility.MinimumSupportedSchemaVersion,
                MaximumSupportedSchemaVersion =
                    ContentAddressing.ContentCompatibility.MaximumSupportedSchemaVersion,
                CancellationToken = _environment.CancellationToken,
                OnLog = _ctx.OnLog,
            });
            _contentDeliveryFlow = new ContentDeliveryFlow(
                _bundles,
                _localization,
                _environment.CancellationToken);
            _activeNovel = new DisposableSlot<Entity>().AddTo(this);
        }

        internal async UniTask Run()
        {
            using var bootstrap = new Bootstrap.Entity(_environment.CancellationToken)
                .AddTo(this);
            CatalogFlow.Resources catalog = null;
            Catalog.NovelCatalogEntry content = null;
            var state = ApplicationFlowState.LoadingCatalog;
            try
            {
                while (state != ApplicationFlowState.Completed)
                {
                    _environment.CancellationToken.ThrowIfCancellationRequested();
                    switch (state)
                    {
                        case ApplicationFlowState.LoadingCatalog:
                            catalog = await _catalogFlow.LoadWithRetry(bootstrap);
                            bootstrap.Hide();
                            state = ApplicationFlowState.SelectingStory;
                            break;

                        case ApplicationFlowState.SelectingStory:
                            content = await _catalogFlow.SelectContent(catalog);
                            state = ApplicationFlowState.RunningStory;
                            break;

                        case ApplicationFlowState.RunningStory:
                            state = await RunStory(content, catalog, bootstrap);
                            break;

                        case ApplicationFlowState.ReturningToCatalog:
                            bootstrap.Hide();
                            content = null;
                            state = ApplicationFlowState.SelectingStory;
                            break;

                        default:
                            state = ApplicationFlowState.Completed;
                            break;
                    }
                }
            }
            finally
            {
                catalog?.Dispose();
            }
        }

        private async UniTask<ApplicationFlowState> RunStory(
            Catalog.NovelCatalogEntry content,
            CatalogFlow.Resources catalog,
            Bootstrap.Entity bootstrap)
        {
            var novel = new Entity(new Entity.Ctx
            {
                Bundles = _bundles,
                Content = content,
                Locale = _locale,
                PersistentDataPath = _environment.PersistentDataPath,
                TargetCamera = _environment.TargetCamera,
                SelectEpisode = definition =>
                    _catalogFlow.SelectEpisode(definition, catalog.Screen),
                PrepareNovelContent = contentId =>
                    _contentDeliveryFlow.PrepareNovel(bootstrap, contentId),
                PrepareEpisodeContent = (definition, episode) =>
                    _contentDeliveryFlow.PrepareEpisode(
                        bootstrap,
                        definition,
                        episode),
                CancellationToken = _environment.CancellationToken,
                OnLog = _ctx.OnLog,
                OnError = _ctx.OnError,
            });
            _activeNovel.Replace(novel);
            try
            {
                EpisodeRunResult result;
                try
                {
                    result = await novel.Init();
                }
                catch (Exception exception) when (
                    exception is Bundles.ContentSourceException
                    || exception is Bundles.ContentIntegrityException
                    || exception is Bundles.ContentStorageException
                    || exception is Bundles.ContentConfigurationException)
                {
                    _ctx.OnError?.Invoke(new Diagnostics.NovelError(
                        Diagnostics.NovelErrorCodes.ContentPreparationFailed,
                        Diagnostics.NovelErrorSeverity.Recoverable,
                        "Story content could not be prepared.",
                        exception: exception,
                        context: new Diagnostics.NovelErrorContext(
                            _bundles.ReleaseId,
                            content.ContentId,
                            deliveryMode: _bundles.DeliveryMode.ToString())));
                    return ApplicationFlowState.ReturningToCatalog;
                }
                if (result.Status == EpisodeRunStatus.Cancelled)
                    return ApplicationFlowState.Completed;
                if (result.Status == EpisodeRunStatus.Failed)
                    _ctx.OnError?.Invoke(result.Error.Value);
                return ApplicationFlowState.ReturningToCatalog;
            }
            finally
            {
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
    }
}
