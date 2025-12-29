using Cysharp.Threading.Tasks;
using Game.Disposable;

namespace Game
{
    internal sealed partial class Entity
    {
        private async UniTask Chapter_Process(int index)
        {
            var result = 0;
            var ctx = new Chapter_ScreenAndBattle.Entity.Ctx
            {
                Data = _ctx.Data.Chapters[index],
                GetBundledPrefab = data => GetBundledPrefab(data.bundleName, data.prefabName),
                GetBundledSprite = data => GetBundledSprite(data.bundleName, data.spriteName),
                GetBundledCameraData = data => GetBundledSO<Chapter_ScreenAndBattle.CameraDataSO>(data.bundleName, data.soName),
            };

            var chapter = new Chapter_ScreenAndBattle.Entity(ctx).AddTo(this);

            await chapter.InitStartScreen();

            await _loading.Hide();

            await chapter.WaitStartScreenResult();

            await _loading.Show();

            chapter.ReleaseStartScreen();

            await chapter.InitBattle();

            await _loading.Hide();

            var battleResult = await chapter.WaitBattleResult();

            await UniTask.Delay(3000);

            await _loading.Show();

            chapter.ReleaseBattle();

            if (battleResult == 0) //failed
            {
                await chapter.InitFailedScreen();

                await _loading.Hide();

                await chapter.WaitResult();

                result = 0;
            }
            else //success
            {
                await chapter.InitSuccessScreen();

                await _loading.Hide();

                await chapter.WaitResult();

                result = 1;
            }

            chapter.Dispose();

            switch (result)
            {
                case 0:
                    Chapter_Process(index).Forget();
                    break;
                case 1:
                    index++;
                    if (index >= _ctx.Data.Chapters.Length)
                        Chapter_introProcess().Forget();
                    else
                        Chapter_Process(index).Forget();
                    break;
            }
        }
    }
}