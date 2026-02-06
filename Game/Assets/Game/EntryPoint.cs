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

            Application.targetFrameRate = 30;

            _entity = new Entity(new Entity.Ctx
            {
                Data = _data,
            });
            _entity.Init().Forget();

            Story().Forget();
        }

        private async UniTask Story()
        {
            var path = $"file://{Application.streamingAssetsPath}/Texts/Intro.json";
            Debug.Log(path);
            var request = UnityEngine.Networking.UnityWebRequest.Get(path);
            await request.SendWebRequest();
            var story = new Ink.Runtime.Story(request.downloadHandler.text);
            while (story.canContinue)
            {
                await UniTask.Delay(1000);
                Debug.Log(story.Continue());
            }
        }

        private void OnDisable()
        {
            _entity?.Dispose();
        }
    }
}