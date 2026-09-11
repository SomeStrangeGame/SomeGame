using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.Audio;

namespace Novels
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField] private Logs.Entity.ShowLogs _logs;
        [SerializeField] private Camera _targetCamera;
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private Sprite _missingBackground;
        [SerializeField] private Sprite _missingCharacter;
        [SerializeField] private GameObject _fallbackLoading;
        [SerializeField] private GameObject _fallbackBubble;
        [SerializeField] private GameObject _fallbackLocation;
        [SerializeField] private GameObject _fallbackCharacter;
        [SerializeField] private GameObject _fallbackNotification;

        private ApplicationRuntime _runtime;
        private Diagnostics.SmokeTelemetry _smokeTelemetry;
        private Analytics.ProductAnalytics _productAnalytics;
        private CancellationTokenSource _sessionCancellation;
        private StorySourceOverlay _storySourceOverlay;
        private Notifications.LocalNotificationCoordinator _notifications;
        private Func<CancellationToken, UniTask<string>> _downloadNotificationSchedule;
        private Notifications.NotificationRoute _pendingDeepLink;
        private bool _suppressAutomaticErrorCapture;

        private void OnEnable()
        {
            try
            {
                var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
                PlayerLoopHelper.Initialize(ref playerLoop);
                var runtimeTuning = NovelRuntimeSettings.Load();
                Application.targetFrameRate = runtimeTuning.TargetFrameRate;
                _storySourceOverlay = GetComponent<StorySourceOverlay>()
                    ?? gameObject.AddComponent<StorySourceOverlay>();
                _sessionCancellation = new CancellationTokenSource();
                var environment = new ApplicationEnvironment(
                    _sessionCancellation.Token,
                    Application.persistentDataPath,
                    Application.version,
                    Bundles.ContentPlatform.GetCurrent(),
                    _targetCamera,
                    _audioMixer,
                    new FallbackAssets(
                        _missingBackground,
                        _missingCharacter,
                        _fallbackLoading,
                        _fallbackBubble,
                        _fallbackLocation,
                        _fallbackCharacter,
                        _fallbackNotification),
                    runtimeTuning);
                Action<(LogType type, string message)> onLog = data =>
                {
                    using (var logs = new Logs.Entity(new Logs.Entity.Ctx {Logs = _logs}))
                        logs.Log("[Novels]", data);
                };
                _smokeTelemetry = new Diagnostics.SmokeTelemetry(onLog);
                var analyticsConfiguration = ContentRuntimeConfiguration.TryLoad();
                _productAnalytics = new Analytics.ProductAnalytics(
                    Application.persistentDataPath,
                    analyticsConfiguration?.AnalyticsEndpointUrl,
                    Application.version,
                    Application.platform.ToString(),
                    analyticsConfiguration?.AnalyticsEnabled == true && !Application.isEditor,
                    _sessionCancellation.Token,
                    onLog);
                Application.logMessageReceived += OnUnityLogMessage;
                _productAnalytics.StartSession();
                _notifications = new Notifications.LocalNotificationCoordinator(onLog);
                _notifications.Initialize();
                Application.deepLinkActivated += OnDeepLinkActivated;
                _pendingDeepLink = Notifications.NotificationRoute.Deserialize(
                    Application.absoluteURL);
                StartRuntime(environment, onLog, runtimeTuning.ContentDelivery).Forget();
            }
            catch (Exception exception)
            {
                ReportError(new Diagnostics.NovelError(
                    Diagnostics.NovelErrorCodes.InitializationFailed,
                    Diagnostics.NovelErrorSeverity.Fatal,
                    "Novel initialization failed.",
                    exception: exception));
                DisposeSession();
            }
        }

        private async UniTaskVoid StartRuntime(
            ApplicationEnvironment environment,
            Action<(LogType type, string message)> onLog,
            Bundles.ContentDeliveryOptions options)
        {
            using var bootstrap = new Bootstrap.BootstrapController(
                environment.CancellationToken);
            try
            {
                bootstrap.ShowLoading(ApplicationTexts.CatalogLoading);
                var dependencies = new ApplicationRuntime.Dependencies
                {
                    Environment = environment,
                    OnLog = onLog,
                    OnError = ReportError,
                    SmokeTelemetry = _smokeTelemetry,
                    ProductAnalytics = _productAnalytics,
                    OnStorySourceChanged = _storySourceOverlay.Show,
                    Notifications = _notifications,
                    InitialNotificationRoute = _pendingDeepLink ?? _notifications.OnForeground(),
                };
#if UNITY_EDITOR || NOVELS_EMBEDDED_CONTENT
                dependencies.ContentSource = CreateContentSource(
                    environment.CancellationToken, options);
#else
                var configuration = ContentRuntimeConfiguration.Load();
                var remoteSource = new Bundles.HttpContentSource(
                    configuration.RemoteContentBaseUrl,
                    environment.CancellationToken,
                    options.RemoteRequestPolicy);
                var manifestRequest = remoteSource.DownloadText(
                    ChannelManifest.FileName(configuration.ContentChannel),
                    environment.CancellationToken);
                var updateRequest = ApplicationUpdatePolicy.Download(
                    remoteSource,
                    configuration.ContentChannel,
                    Application.version,
                    environment.CancellationToken,
                    onLog);
                var (manifestJson, updatePrompt) = await UniTask.WhenAll(
                    manifestRequest,
                    updateRequest);
                var manifest = ChannelManifest.Deserialize(manifestJson);
                dependencies.UpdatePrompt = updatePrompt;
                _downloadNotificationSchedule = token => remoteSource.DownloadText(
                    Notifications.NotificationSchedule.FileName(configuration.ContentChannel),
                    token);
                RefreshNotificationSchedule(environment.CancellationToken).Forget();
                var catalogRoot = Path.Combine(
                    Application.streamingAssetsPath,
                    "NovelCatalog");
                dependencies.CatalogContentSource = new Bundles.StreamingAssetsContentSource(
                    catalogRoot,
                    environment.CancellationToken,
                    options.LocalRequestPolicy);
                dependencies.StoryIds = manifest.StoryIds;
                dependencies.CreateStoryContentSource = storyId =>
                    new Bundles.PrefixedContentSource(
                        remoteSource,
                        manifest.StoryRoot(storyId));
#endif
                environment.CancellationToken.ThrowIfCancellationRequested();
                _runtime = new ApplicationRuntime(dependencies);
                _smokeTelemetry.Emit(
                    "app.started",
                    ("appVersion", Application.version),
                    ("platform", Application.platform.ToString()),
                    ("contentPlatform", environment.ContentPlatform));
                await _runtime.Run(bootstrap);
            }
            catch (OperationCanceledException)
                when (environment.CancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                ReportError(new Diagnostics.NovelError(
                    Diagnostics.NovelErrorCodes.InitializationFailed,
                    Diagnostics.NovelErrorSeverity.Fatal,
                    "Novel initialization failed.",
                    exception: exception));
                DisposeSession();
            }
        }

        private Bundles.IContentSource CreateContentSource(
            CancellationToken cancellationToken,
            Bundles.ContentDeliveryOptions options)
        {
#if UNITY_EDITOR || NOVELS_EMBEDDED_CONTENT
#if UNITY_EDITOR
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Unity project root cannot be resolved.");
            var contentRoot = Path.Combine(projectRoot, "Build", "LocalContent");
            return new Bundles.FileSystemContentSource(
                contentRoot,
                cancellationToken,
                options.LocalRequestPolicy);
#else
            var contentRoot = Path.Combine(
                Application.streamingAssetsPath,
                "NovelContent");
#if UNITY_ANDROID
            return new Bundles.StreamingAssetsContentSource(
                contentRoot,
                cancellationToken,
                options.LocalRequestPolicy);
#else
            return new Bundles.FileSystemContentSource(
                contentRoot,
                cancellationToken,
                options.LocalRequestPolicy);
#endif
#endif
#else
            var configuration = ContentRuntimeConfiguration.Load();
            return new Bundles.HttpContentSource(
                configuration.RemoteContentBaseUrl,
                cancellationToken,
                options.RemoteRequestPolicy);
#endif
        }

        private void OnDisable()
        {
            DisposeSession();
        }

        private void DisposeSession()
        {
            _smokeTelemetry?.Emit("app.stopped");
            _productAnalytics?.FlushSynchronously();
            _sessionCancellation?.Cancel();
            try
            {
                _runtime?.Dispose();
            }
            catch (Exception exception)
            {
                using (var logs = new Logs.Entity(new Logs.Entity.Ctx {Logs = _logs}))
                    logs.Log("[Novels]", (LogType.Error, $"Disposal failed: {exception}"));
            }
            finally
            {
                _sessionCancellation?.Dispose();
                _sessionCancellation = null;
                _runtime = null;
                _downloadNotificationSchedule = null;
                _notifications = null;
                _pendingDeepLink = null;
                Application.deepLinkActivated -= OnDeepLinkActivated;
                Application.logMessageReceived -= OnUnityLogMessage;
                _smokeTelemetry = null;
                _productAnalytics = null;
                _storySourceOverlay?.Show(default);
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                if (_runtime != null)
                    FlushSaveSynchronously(_runtime, "pausing");
                _notifications?.OnBackground();
                _productAnalytics?.ApplicationPaused();
                return;
            }
            var route = _notifications?.OnForeground();
            if (route != null)
                _runtime?.NavigateToCatalog(route);
            RefreshNotificationSchedule(_sessionCancellation?.Token ?? default).Forget();
            _productAnalytics?.ApplicationResumed();
        }

        private void OnApplicationQuit()
        {
            if (_runtime != null)
                FlushSaveSynchronously(_runtime, "quitting");
            _notifications?.OnBackground();
            _productAnalytics?.FlushSynchronously();
        }

        private void OnDeepLinkActivated(string url)
        {
            var route = Notifications.NotificationRoute.Deserialize(url);
            if (route == null)
                return;
            _pendingDeepLink = route;
            _runtime?.NavigateToCatalog(route);
        }

        private async UniTask RefreshNotificationSchedule(
            CancellationToken cancellationToken)
        {
            if (_downloadNotificationSchedule == null || cancellationToken.IsCancellationRequested)
                return;
            try
            {
                var json = await _downloadNotificationSchedule(cancellationToken);
                _notifications?.ApplyServerSchedule(json);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                using (var logs = new Logs.Entity(new Logs.Entity.Ctx {Logs = _logs}))
                    logs.Log("[Novels]", (LogType.Warning,
                        $"Notification schedule unavailable: {exception.Message}"));
            }
        }

        private void FlushSaveSynchronously(
            ApplicationRuntime runtime,
            string lifecycleEvent)
        {
            try
            {
                runtime.FlushSaveSynchronously();
            }
            catch (Exception exception)
            {
                ReportError(new Diagnostics.NovelError(
                    Diagnostics.NovelErrorCodes.SaveWriteFailed,
                    Diagnostics.NovelErrorSeverity.Recoverable,
                    $"Failed to flush save data while {lifecycleEvent}.",
                    exception: exception));
            }
        }

        private void ReportError(Diagnostics.NovelError error)
        {
            _productAnalytics?.RuntimeError(
                error.Code,
                error.Context.ContentId,
                error.Context.EpisodeId,
                $"{error.Severity}:{error.Context.ReleaseId}:{error.Context.DeliveryMode}");
            _smokeTelemetry?.Emit(
                "error",
                ("code", error.Code),
                ("severity", error.Severity.ToString()),
                ("contentId", error.Context.ContentId),
                ("episodeId", error.Context.EpisodeId),
                ("releaseId", error.Context.ReleaseId),
                ("deliveryMode", error.Context.DeliveryMode));
            var logType = error.Severity == Diagnostics.NovelErrorSeverity.Warning
                ? LogType.Warning
                : LogType.Error;
            _suppressAutomaticErrorCapture = true;
            try
            {
                using (var logs = new Logs.Entity(new Logs.Entity.Ctx {Logs = _logs}))
                    logs.Log("[Novels]", (logType, error.ToString()));
            }
            finally
            {
                _suppressAutomaticErrorCapture = false;
            }
        }

        private void OnUnityLogMessage(string condition, string stackTrace, LogType type)
        {
            if (_suppressAutomaticErrorCapture)
                return;
            _productAnalytics?.CaptureUnityError(condition, stackTrace, type);
        }
    }
}
