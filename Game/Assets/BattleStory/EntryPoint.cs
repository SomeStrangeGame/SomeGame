using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.LowLevel;

namespace BattleStory
{
    internal sealed class EntryPoint : MonoBehaviour
    {
        [SerializeField] private Data _data;

        private Entity _entity;

        private void OnEnable()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopHelper.Initialize(ref playerLoop);

            Application.targetFrameRate = 30;

            _entity = new Entity(new Entity.Ctx
            {
                Data = _data,
            });
            _entity.Init().Forget();
        }

        private void OnDisable()
        {
            _entity?.Dispose();
        }
    }
}