using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Novels
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField] private Content.NovelContentAsset _content;
        [SerializeField] private Logs.Entity.ShowLogs _logs;

        private Entity _entity;
        private CancellationTokenSource _sessionCancellation;

        private void OnEnable()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopHelper.Initialize(ref playerLoop);

            Application.targetFrameRate = 30;

            _sessionCancellation = new CancellationTokenSource();
            _entity = new Entity(new Entity.Ctx
            {
                Content = _content,
                CancellationToken = _sessionCancellation.Token,
                OnLog = data => 
                {
                    using (var logs = new Logs.Entity(new Logs.Entity.Ctx {Logs = _logs}))
                        logs.Log("[Novels]", data);
                },
                OnError = ReportError,
            });
            Run(_entity, _sessionCancellation.Token).Forget();
        }

        private async UniTaskVoid Run(Entity entity, CancellationToken cancellationToken)
        {
            try
            {
                await entity.Init();
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
                _entity?.Dispose();
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
                _entity = null;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && _entity != null)
                FlushSave(_entity).Forget();
        }

        private async UniTaskVoid FlushSave(Entity entity)
        {
            try
            {
                await entity.FlushSaveAsync();
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
