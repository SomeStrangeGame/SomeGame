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
            }).AddTo(this);
            bubble.Init();

            return bubble;
        }
    }
}
