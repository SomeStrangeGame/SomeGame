using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Waiting
{
    public class Entity : BaseDisposable
    {
        public async UniTask Await(float seconds)
        {
            await UniTask.Delay(Mathf.RoundToInt(seconds * 1000));
        }
    }
}

