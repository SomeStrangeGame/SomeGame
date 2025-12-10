using Cysharp.Threading.Tasks;

namespace Game
{
    internal sealed partial class Entity
    {
        private async UniTask SomeMenu1Process()
        {
            var result = 0;
            var ctx = new SomeMenu1.Entity.Ctx
            {
                Data = _ctx.Data.SomeMenu1Data,
            };
            using (var someMenu1 = new SomeMenu1.Entity(ctx))
            {
                await someMenu1.Init();

                await _loading.Hide();

                result = await someMenu1.WaitResult(); //wait some process...

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