using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Waiting
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public CancellationToken CancellationToken;
        }

        private readonly Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask Await(float seconds)
        {
            var timer = seconds;
            while(timer > 0f)
            {
                await UniTask.Yield(_ctx.CancellationToken);
                timer -= Time.deltaTime;
            }
        }
    }
}
