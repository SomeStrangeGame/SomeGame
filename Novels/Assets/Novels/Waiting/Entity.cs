using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Waiting
{
    public class Entity : BaseDisposable
    {
        public async UniTask Await(float seconds)
        {
            var timer = seconds;
            while(timer > 0)
            {
                await UniTask.Yield();
                timer -= Time.deltaTime;
            }
        }
    }
}

