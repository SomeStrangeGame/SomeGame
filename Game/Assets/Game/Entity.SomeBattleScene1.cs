using Cysharp.Threading.Tasks;

namespace Game
{
    internal sealed partial class Entity
    {
        private async UniTask SomeBattleScene1Process()
        {
            var result = 0;
            var ctx = new SomeBattleScene1.Entity.Ctx
            {
                Data = _ctx.Data.SomeBattleScene1,
            };
            using (var someBattle1 = new SomeBattleScene1.Entity(ctx))
            {
                await someBattle1.Init();

                await _loading.Hide();

                result = await someBattle1.WaitResult(); //wait some process...

                await _loading.Show();
            }

            switch (result)
            {
                case 2:
                    Chapter_0Process().Forget();
                    break;
            }
        }
    }
}

