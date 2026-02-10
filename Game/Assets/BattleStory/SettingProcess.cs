using System;
using BattleStory.SOData;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace BattleStory
{
    internal class SettingProcess : BaseDisposable
    {
        internal struct Ctx
        {
            internal Action<bool> OnSkipVoice;
            internal ThreadPriority DefaultThreadPriority;
            internal BundleData SettingData;
            internal Func<string, string, UniTask<GameObject>> GetBundledPrefab;
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
                GetBundledPrefab = () => _ctx.GetBundledPrefab(_ctx.SettingData.BundleName, _ctx.SettingData.AssetName),
            }).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _ctx.DefaultThreadPriority))
                await setting.Init();
            
            var skipVoice = false;
            _ctx.OnSkipVoice.Invoke(skipVoice);
            SetVoices();
            void SetVoices()
            {
                setting.AddOrUpdateButton("voices", $"<b>Озвучка: {(skipVoice ?  "выключено" : "включено")}</b>", () => 
                {
                    skipVoice = !skipVoice;
                    _ctx.OnSkipVoice.Invoke(skipVoice);
                    SetVoices();
                });
            }

            var settingsDone = new UniTaskCompletionSource();
            SetDone();
            void SetDone()
            {
                setting.AddOrUpdateButton("done", "<b>Начать</b>", () =>
                {
                    settingsDone.TrySetResult();
                });
            }

            setting.Show();
            await _ctx.HideLoading();
            await settingsDone.Task;
            setting.Hide();
            await _ctx.ShowLoading();
        }
    }
}