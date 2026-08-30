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
        private CancellationTokenSource _sessionCancellation;
        private StorySourceOverlay _storySourceOverlay;

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
                _runtime = new ApplicationRuntime(new ApplicationRuntime.Dependencies
                {
                    Environment = environment,
                    ContentSource = CreateContentSource(
                        _sessionCancellation.Token,
                        runtimeTuning.ContentDelivery),
                    OnLog = onLog,
                    OnError = ReportError,
                    SmokeTelemetry = _smokeTelemetry,
                    OnStorySourceChanged = _storySourceOverlay.Show,
                });
                _smokeTelemetry.Emit(
                    "app.started",
                    ("appVersion", Application.version),
                    ("platform", Application.platform.ToString()),
                    ("contentPlatform", environment.ContentPlatform));
                Run(_runtime, _sessionCancellation.Token).Forget();
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

        private async UniTaskVoid Run(
            ApplicationRuntime runtime,
            CancellationToken cancellationToken)
        {
            try
            {
                await runtime.Run();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                ReportError(new Diagnostics.NovelError(
                    Diagnostics.NovelErrorCodes.InitializationFailed,
                    Diagnostics.NovelErrorSeverity.Fatal,
                    "Novel initialization failed.",
                    exception: exception));
            }
        }

        private void OnDisable()
        {
            DisposeSession();
        }

        private void DisposeSession()
        {
            _smokeTelemetry?.Emit("app.stopped");
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
                _smokeTelemetry = null;
                _storySourceOverlay?.Show(default);
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && _runtime != null)
                FlushSaveSynchronously(_runtime, "pausing");
        }

        private void OnApplicationQuit()
        {
            if (_runtime != null)
                FlushSaveSynchronously(_runtime, "quitting");
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
            using (var logs = new Logs.Entity(new Logs.Entity.Ctx {Logs = _logs}))
                logs.Log("[Novels]", (logType, error.ToString()));
        }
    }
}
