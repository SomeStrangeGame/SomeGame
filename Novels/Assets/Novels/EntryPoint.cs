using System;
using System.Threading;
using System.Globalization;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Novels
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField] private Logs.Entity.ShowLogs _logs;
        [SerializeField] private Camera _targetCamera;

        private ApplicationRuntime _runtime;
        private CancellationTokenSource _sessionCancellation;

        private void OnEnable()
        {
            try
            {
                var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
                PlayerLoopHelper.Initialize(ref playerLoop);
                var runtimeTuning = NovelRuntimeSettings.Load();
                Application.targetFrameRate = runtimeTuning.TargetFrameRate;

                _sessionCancellation = new CancellationTokenSource();
                var environment = new ApplicationEnvironment(
                    _sessionCancellation.Token,
                    Application.persistentDataPath,
                    Application.version,
                    new Locale.LocaleProvider(CultureInfo.CurrentUICulture).Code,
                    Bundles.ContentPlatform.GetCurrent(),
                    _targetCamera,
                    runtimeTuning);
                _runtime = new ApplicationRuntime(new ApplicationRuntime.Ctx
                {
                    Environment = environment,
                    ContentSource = CreateContentSource(_sessionCancellation.Token),
                    OnLog = data =>
                    {
                        using (var logs = new Logs.Entity(new Logs.Entity.Ctx {Logs = _logs}))
                            logs.Log("[Novels]", data);
                    },
                    OnError = ReportError,
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

        private Bundles.IContentSource CreateContentSource(
            CancellationToken cancellationToken)
        {
#if UNITY_EDITOR
            return new Bundles.StreamingAssetsSource(cancellationToken);
#else
            var configuration = ContentRuntimeConfiguration.Load();
            return new Bundles.HttpContentSource(
                configuration.RemoteContentBaseUrl,
                cancellationToken);
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
