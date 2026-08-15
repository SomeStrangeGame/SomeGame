using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private Bubble.Entity CreateBubble(GameObject bubblePrefab)
        {
            var bubble = new Bubble.Entity(new Bubble.Entity.Ctx
            {
                BubblePrefab = bubblePrefab,
                CancellationToken = _ctx.CancellationToken,
            }).AddTo(this);
            bubble.Init();

            return bubble;
        }
    }
}
