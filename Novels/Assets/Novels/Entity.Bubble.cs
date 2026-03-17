using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private async UniTask<Bubble.Entity> CreateBubble(Bundles.Entity bundles, PathGetter.Entity pathGetter)
        {
            var bubble = new Bubble.Entity(new Bubble.Entity.Ctx
            {
                GetBubblePrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsBubbleBundleName, pathGetter.GetBubblePrefabAssetName("Screen")),
            }).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await bubble.Init();

            return bubble;
        }
    }
}

