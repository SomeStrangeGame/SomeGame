using Cysharp.Threading.Tasks;
using Game.Disposable;

namespace Game
{
    internal sealed partial class Entity
    {
        private async UniTask ChapterIntroProcess()
        {
            var ctx = new Chapter_OnlyScreen.Entity.Ctx
            {
                Data = _ctx.Data.Chapter_intro,
                GetBundledPrefab = data => _bundles.GetBundledPrefab(data.bundleName, data.prefabName),
                GetBundledSprite = data => _bundles.GetBundledSprite(data.bundleName, data.spriteName)
            };
            
            var chapter_0 = new Chapter_OnlyScreen.Entity(ctx).AddTo(this);
            
            await chapter_0.Init();
            await _loading.Hide();
            await chapter_0.WaitResult();
            await _loading.Show();

            chapter_0.Dispose();

            ChapterBattleProcess(0).Forget();
        }
    }
}