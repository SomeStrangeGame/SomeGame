using Cysharp.Threading.Tasks;

namespace Game
{
    internal sealed partial class Entity
    {
        private async UniTask Chapter_introProcess()
        {
            var ctx = new Chapter_OnlyScreen.Entity.Ctx
            {
                Data = _ctx.Data.Chapter_intro,
                GetBundledPrefab = data => GetBundledPrefab(data.bundleName, data.prefabName),
                GetBundledSprite = data => GetBundledSprite(data.bundleName, data.spriteName)
            };
            using (var chapter_0 = new Chapter_OnlyScreen.Entity(ctx))
            {
                await chapter_0.Init();

                await _loading.Hide();

                await chapter_0.WaitResult();

                await _loading.Show();
            }

            Chapter_Process(0).Forget();
        }
    }
}