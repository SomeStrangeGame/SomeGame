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
            SetVoiceButton(setting);
            var settingDone = new UniTaskCompletionSource();
            SetDoneButton(setting, settingDone);

            setting.Show();
            await _ctx.HideLoading();
            await settingDone.Task;
            setting.Hide();
            await _ctx.ShowLoading();
        }

        private void SetVoiceButton(Setting.Entity setting)
        {
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