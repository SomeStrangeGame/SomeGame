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
            using (var menu = new Chapter_0.Entity(ctx))
            {
                await menu.Init();

                await _loading.Hide();

                result = await menu.WaitResult(); //wait some process...

                await _loading.Show();
            }

            switch (result)
            {
                case 1:
                    SomeBattleScene1Process().Forget();
                    break;
            }
        }
    }
}