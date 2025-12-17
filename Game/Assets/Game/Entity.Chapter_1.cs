using Cysharp.Threading.Tasks;

namespace Game
{
    internal sealed partial class Entity
    {
        private async UniTask Chapter_1Process()
        {
            var result = 0;
            var ctx = new Chapter_ScreenAndBattle.Entity.Ctx
            {
                Data = _ctx.Data.Chapter_1,
            };
            using (var chapter_1 = new Chapter_ScreenAndBattle.Entity(ctx))
            {
                await chapter_1.InitStart();

                await _loading.Hide();

                await chapter_1.WaitFirstResult();

                await _loading.Show();

                chapter_1.ReleaseScreen();

                await chapter_1.InitBattle();

                await _loading.Hide();

                var battleResult = await chapter_1.WaitBattleResult();

                await UniTask.Delay(3000);

                await _loading.Show();

                chapter_1.ReleaseBattle();

                if (battleResult == 0) //failed
                {
                    await chapter_1.InitFailed();

                    await _loading.Hide();

                    await chapter_1.WaitResult();

                    result = 1;
                }
                else //success
                {
                    await chapter_1.InitSuccess();

                    await _loading.Hide();

                    await chapter_1.WaitResult();

                    result = 2;
                }
            }

            switch (result)
            {
                case 1:
                    Chapter_1Process().Forget();
                    break;
                case 2:
                    Chapter_0Process().Forget();
                    break;
            }
        }
    }
}