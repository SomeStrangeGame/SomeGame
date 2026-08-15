using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Novels
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField] private Data _data;
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
                Data = _data,
                CancellationToken = _sessionCancellation.Token,
                OnLog = data => 
                {
                    using (var logs = new Logs.Entity(new Logs.Entity.Ctx {Logs = _logs}))
                        logs.Log("[Novels]", data);
                },
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
                using (var logs = new Logs.Entity(new Logs.Entity.Ctx {Logs = _logs}))
                    logs.Log("[Novels]", (LogType.Error, $"Initialization failed: {exception}"));
            }
        }

        private void OnDisable()
        {
            _sessionCancellation?.Cancel();
            _entity?.Dispose();
            _sessionCancellation?.Dispose();
            _sessionCancellation = null;
            _entity = null;
        }
    }
}
