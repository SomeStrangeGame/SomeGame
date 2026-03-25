using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels
{
    internal partial class Entity
    {
        private async UniTask<Bubble.Entity> CreateBubble(Bundles.Entity bundles, PathGetter.Entity pathGetter)
        {
            var bubble = new Bubble.Entity(new Bubble.Entity.Ctx
            {
                MainCharacter = _ctx.Data.MainCharacter,
                GetBubblePrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsBubbleBundleName, pathGetter.GetBubblePrefabAssetName("Screen")),
            }).AddTo(this);
            await bubble.Init();

            return bubble;
        }
    }
}

