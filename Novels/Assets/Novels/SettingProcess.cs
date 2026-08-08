using System;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal class SettingProcess : BaseDisposable
    {
        internal struct Ctx
        {
            internal ThreadPriority DefaultThreadPriority;
            internal Func<UniTask<GameObject>> GetBundledPrefab;
            internal Func<UniTask> ShowLoading;
            internal Func<UniTask> HideLoading;
            internal Func<bool> ContainAnySave;
            internal Action ClearSave;
        }

        private Ctx _ctx;

        internal SettingProcess(Ctx ctx)
        {
            _ctx = ctx;
        }

        internal async UniTask ShowSettingProcess()
        {
            var setting = new Setting.Entity(new Setting.Entity.Ctx
            {
                GetBundledPrefab = _ctx.GetBundledPrefab,
            }).AddTo(this);

            using (new LoadingPriority.Entity(ThreadPriority.High, _ctx.DefaultThreadPriority))
                await setting.Init();

            setting.SetDescription("Тайна затерянного мира");

            var newGame = new UniTaskCompletionSource();
            setting.AddOrUpdateButton("newGame", "<b>Новая игра</b>", () =>
            {
                newGame.TrySetResult();
            });

            var continueGame = new UniTaskCompletionSource();
            if (_ctx.ContainAnySave())
            {
                setting.AddOrUpdateButton("continueGame", "<b>Продолжить</b>", () =>
                {
                    _ctx.ClearSave();
                    continueGame.TrySetResult();
                });
            }

            setting.Show();
            await _ctx.HideLoading();
            await UniTask.WhenAny(newGame.Task, continueGame.Task);
            await _ctx.ShowLoading();
            setting.Hide();
        }
    }
}

