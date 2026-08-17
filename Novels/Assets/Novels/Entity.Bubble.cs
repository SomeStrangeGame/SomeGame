using Cysharp.Threading.Tasks;
using Disposable;
using System.Threading;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private Bubble.Entity CreateBubble(
            IBaseDisposable owner,
            GameObject bubblePrefab,
            CancellationToken cancellationToken)
        {
            var bubble = new Bubble.Entity(new Bubble.Entity.Ctx
            {
                BubblePrefab = bubblePrefab,
                CancellationToken = cancellationToken,
            }).AddTo(owner);
            bubble.Init();

            return bubble;
        }
    }
}
