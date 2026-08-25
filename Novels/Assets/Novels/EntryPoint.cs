using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Novels
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField] private Logs.Entity.ShowLogs _logs;
        [SerializeField] private Camera _targetCamera;
        [SerializeField] private Sprite _missingBackground;
        [SerializeField] private Sprite _missingCharacter;
        [SerializeField] private GameObject _fallbackLoading;
        [SerializeField] private GameObject _fallbackBubble;
        [SerializeField] private GameObject _fallbackLocation;
        [SerializeField] private GameObject _fallbackCharacter;
        [SerializeField] private GameObject _fallbackNotification;

        private ApplicationRuntime _runtime;
        private CancellationTokenSource _sessionCancellation;
        private StorySourceOverlay _storySourceOverlay;
        private int _cacheGeneration;
        private bool _restartRequested;
        private bool _coldRestartRequested;

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
                _storySourceOverlay.Configure(
                    () => RequestRestart(true),
                    () => RequestRestart(false));

                _sessionCancellation = new CancellationTokenSource();
                var environment = new ApplicationEnvironment(
                    _sessionCancellation.Token,
                    Application.persistentDataPath,
                    Application.version,
                    Bundles.ContentPlatform.GetCurrent(),
                    _targetCamera,
                    new FallbackAssets(
                        _missingBackground,
                        _missingCharacter,
                        _fallbackLoading,
                        _fallbackBubble,
                        _fallbackLocation,
                        _fallbackCharacter,
                        _fallbackNotification),
                    runtimeTuning,
                    _cacheGeneration);
                _runtime = new ApplicationRuntime(new ApplicationRuntime.Dependencies
                {
                    Environment = environment,
                    ContentSource = CreateContentSource(
                        _sessionCancellation.Token,
                        runtimeTuning.ContentDelivery),
                    OnLog = data =>
                    {
                        using (var logs = new Logs.Entity(new Logs.Entity.Ctx {Logs = _logs}))
                            logs.Log("[Novels]", data);
                    },
                    OnError = ReportError,
                    OnStorySourceChanged = _storySourceOverlay.Show,
                });
                Run(_runtime, _sessionCancellation.Token).Forget();
            }
            catch (Exception exception)
            {
                DisposeSession();
                ReportError(new Diagnostics.NovelError(
                    Diagnostics.NovelErrorCodes.InitializationFailed,
                    Diagnostics.NovelErrorSeverity.Fatal,
                    "Novel initialization failed.",
                    exception: exception));
            }
        }

        private void Update()
        {
            if (!_restartRequested)
                return;
            _restartRequested = false;
            var coldRestart = _coldRestartRequested;
            _coldRestartRequested = false;
            DisposeSession();
            if (coldRestart)
            {
                ClearRemoteContentCache();
                _cacheGeneration++;
            }
            StreamingExperimentDiagnostics.Reset();
            OnEnable();
        }

        private void RequestRestart(bool cold)
        {
            _restartRequested = true;
            _coldRestartRequested |= cold;
        }

        private static void ClearRemoteContentCache()
        {
            var cache = new Cache.Entity(Application.persistentDataPath);
            cache.DeleteDirectory("RemoteContent");
            cache.DeleteDirectory("ContentStaging");
        }

        private Bundles.IContentSource CreateContentSource(
            CancellationToken cancellationToken,
            Bundles.ContentDeliveryOptions options)
        {
#if UNITY_EDITOR
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Unity project root cannot be resolved.");
            var contentRoot = Path.Combine(projectRoot, "Build", "LocalContent");
            var simulatedMegabits = Environment.GetEnvironmentVariable(
                "NOVELS_SIMULATED_MBITS");
            if (Bundles.ThrottledFileSystemContentSource.TryParseMegabits(
                    simulatedMegabits,
                    out var megabits))
            {
                return new Bundles.ThrottledFileSystemContentSource(
                    contentRoot,
                    megabits,
                    Bundles.ThrottledFileSystemContentSource.ParseMilliseconds(
                        Environment.GetEnvironmentVariable(
                            "NOVELS_SIMULATED_LATENCY_MS"),
                        120),
                    Bundles.ThrottledFileSystemContentSource.ParseMilliseconds(
                        Environment.GetEnvironmentVariable(
                            "NOVELS_SIMULATED_JITTER_MS"),
                        30));
            }
            return new Bundles.FileSystemContentSource(
                contentRoot,
                cancellationToken,
                options.LocalRequestPolicy);
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
            var logType = error.Severity == Diagnostics.NovelErrorSeverity.Warning
                ? LogType.Warning
                : LogType.Error;
            using (var logs = new Logs.Entity(new Logs.Entity.Ctx {Logs = _logs}))
                logs.Log("[Novels]", (logType, error.ToString()));
        }
    }
}
