using Cysharp.Threading.Tasks;

namespace Game
{
    internal sealed partial class Entity
    {
        private async UniTask Chapter_0Process()
        {
            var result = 0;
            var ctx = new Chapter_0.Entity.Ctx
            {
                Data = _ctx.Data.Chapter_0,
            };
            using (var chapter_0 = new Chapter_0.Entity(ctx))
            {
                await chapter_0.Init();

                await _loading.Hide();

                result = await chapter_0.WaitResult();

                await _loading.Show();
            }

            switch (result)
            {
                case 0:
                    Chapter_0Process().Forget();
                    break;
                case 1:
                    Chapter_1Process().Forget();
                    break;
            }
        }
    }
}