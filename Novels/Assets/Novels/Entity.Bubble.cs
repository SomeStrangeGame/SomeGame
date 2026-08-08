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
                MainCharacter = _ctx.Data.MainCharacter,
                BubblePrefab = bubblePrefab,
            }).AddTo(this);
            bubble.Init();

            return bubble;
        }
    }
}

