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
            await setting.Init();
                
            var settingDone = new UniTaskCompletionSource();
            SetDoneButton(setting, settingDone);

            setting.Show();
            await _ctx.HideLoading();
            await settingDone.Task;
            await _ctx.ShowLoading();
            setting.Hide();
        }

        private void SetDoneButton(Setting.Entity setting, UniTaskCompletionSource settingDone)
        {
            SetDone();
            void SetDone()
            {
                setting.AddOrUpdateButton("done", "<b>Начать</b>", () =>
                {
                    settingDone.TrySetResult();
                });
            }
        }
    }
}

