using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Game
{
    internal sealed class EntryPoint : MonoBehaviour
    {
        [SerializeField] private Data _data;

        private Entity _entity;

        private void OnEnable()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopHelper.Initialize(ref playerLoop);

            _entity = new Entity(new Entity.Ctx
            {
                Data = _data,
            });
        }

        private void OnDisable()
        {
            _entity?.Dispose();
        }
    }
}