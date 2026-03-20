using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Waiting
{
    public class Entity : BaseDisposable
    {
        public async UniTask Await(bool isLoading, float seconds)
        {
            var timer = isLoading ? 0f : seconds;
            while(timer > 0f)
            {
                await UniTask.Yield();
                timer -= Time.deltaTime;
            }
        }
    }
}

