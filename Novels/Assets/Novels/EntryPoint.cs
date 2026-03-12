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

        private void OnEnable()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopHelper.Initialize(ref playerLoop);

            Application.targetFrameRate = 30;

            _entity = new Entity(new Entity.Ctx
            {
                Data = _data,
                OnLog = data => 
                {
                    using (var logs = new Logs.Entity(new Logs.Entity.Ctx {Logs = _logs}))
                        logs.Log("[Novels]", data);
                },
            });
            _entity.Init().Forget();
        }

        private void OnDisable()
        {
            _entity?.Dispose();
        }
    }
}
