using System;
using Cysharp.Threading.Tasks;
using Disposable;
using SOData;
using UnityEngine;

namespace Novels
{
    [Serializable]
    internal struct Data
    {
        [SerializeField] private BundleData _loadingData;
        [SerializeField] private BundleData _settingData;
        [SerializeField] private BundleData _bubbleData;

        internal readonly BundleData LoadingData => _loadingData;
        internal readonly BundleData SettingData => _settingData;
        internal readonly BundleData BubbleData => _bubbleData;
    }

    internal class Entity : BaseDisposable
    {
        internal struct Ctx
        {
            internal Data Data;
            public Action<(LogType type, string message)> OnLog;
        }

        private const ThreadPriority _defaultThreadPriority = ThreadPriority.Low;

        private readonly Ctx _ctx;

        internal Entity(Ctx ctx)
        {
            _ctx = ctx;

            Application.backgroundLoadingPriority = _defaultThreadPriority;
        }

        internal async UniTask Init()
        {
            var bundles = new Bundles.Entity(new Bundles.Entity.Ctx
            {
                OnLog = _ctx.OnLog,
            }).AddTo(this);

            var loadingCtx = new Loading.Entity.Ctx
            {
                GetBundledPrefab = () => bundles.GetBundledPrefab(_ctx.Data.LoadingData.BundleName, _ctx.Data.LoadingData.AssetName),
            };
            var loading = new Loading.Entity(loadingCtx).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await loading.Init();

            //preloading init
            var storyTextLoading = bundles.GetText($"NovelTexts/s01e01.ink.json");

            await loading.Show();

            var settingProcessCtx = new SettingProcess.Ctx
            {
                DefaultThreadPriority = _defaultThreadPriority,
                SettingData = _ctx.Data.SettingData,
                GetBundledPrefab = bundles.GetBundledPrefab,
                ShowLoading = loading.Show,
                HideLoading = loading.Hide,
            };
            var settingProcess = new SettingProcess(settingProcessCtx).AddTo(this);
            await settingProcess.ShowSettingProcess();

            await loading.Show();

            //preloading loading
            var storyText = string.Empty;
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                storyText = await storyTextLoading;

            var storyProcessor = new StoryProcessor.Entity(new StoryProcessor.Entity.Ctx
            {
                StoryText = storyText,
            }).AddTo(this);

            var bubble = new Bubble.Entity(new Bubble.Entity.Ctx
            {
                GetBubblePrefab = () => bundles.GetBundledPrefab(_ctx.Data.BubbleData.BundleName, _ctx.Data.BubbleData.AssetName),
            }).AddTo(this);
            await bubble.Init();

            await loading.Hide();

            while (!IsDisposed)
            {
                if (storyProcessor.TryGetNextText(out var text))
                {
                    bubble.SetText(text);
                    bubble.RemoveAllButtons();
                    await UniTask.Delay(100);
                }
                else
                {
                    var bubbleDone = new UniTaskCompletionSource();
                    var choices = storyProcessor.GetChoices();
                    foreach(var choice in choices)
                    {
                        bubble.AddOrUpdateButton(choice.index, choice.text, id =>
                        {
                            storyProcessor.SetChoice(id);
                            bubbleDone.TrySetResult();
                        });
                    }
                    await bubbleDone.Task;
                }
            }
        }
    }
}