using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Novels
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField] private Logs.Entity.ShowLogs _logs;

        private ApplicationRuntime _runtime;
        private CancellationTokenSource _sessionCancellation;

        private void OnEnable()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopHelper.Initialize(ref playerLoop);

            Application.targetFrameRate = 30;

            _sessionCancellation = new CancellationTokenSource();
            _runtime = new ApplicationRuntime(new ApplicationRuntime.Ctx
            {
                CancellationToken = _sessionCancellation.Token,
                ContentSource = new Bundles.StreamingAssetsSource(
                    _sessionCancellation.Token),
                PersistentDataPath = Application.persistentDataPath,
                OnLog = data => 
                {
                    using (var logs = new Logs.Entity(new Logs.Entity.Ctx {Logs = _logs}))
                        logs.Log("[Novels]", data);
                },
                OnError = ReportError,
            });
            Run(_runtime, _sessionCancellation.Token).Forget();
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
                FlushSave(_runtime).Forget();
        }

        private async UniTaskVoid FlushSave(ApplicationRuntime runtime)
        {
            try
            {
                await runtime.FlushSaveAsync();
            }
            catch (Exception exception)
            {
                ReportError(new Diagnostics.NovelError(
                    Diagnostics.NovelErrorCodes.SaveWriteFailed,
                    Diagnostics.NovelErrorSeverity.Recoverable,
                    "Failed to flush save data while pausing.",
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
