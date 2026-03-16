using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Disposable;
using Localization;
using SOData;
using UnityEngine;

namespace Novels
{
    [Serializable]
    internal struct Data
    {
        [SerializeField] private string _prefix;
        [SerializeField] private string _mainCharacter;

        [Space]
        [SerializeField] private string _storyTextPath;

        [Space]
        [SerializeField] private BundleData _loadingData;
        [SerializeField] private string _novelsSettingBundleName;
        [SerializeField] private string _novelsBubbleBundleName;
        [SerializeField] private string _novelsLocationBundleName;
        [SerializeField] private string _novelsCharacterBundleName;
        [SerializeField] private string _novelsNotificationBundleName;
        [SerializeField] private string _novelsLocalizationBundleName;

        internal readonly string Prefix => _prefix;
        internal readonly string MainCharacter => _mainCharacter;

        internal readonly string StoryTextPath => _storyTextPath;

        internal readonly BundleData LoadingData => _loadingData;
        internal readonly string NovelsSettingBundleName => _novelsSettingBundleName;
        internal readonly string NovelsBubbleBundleName => _novelsBubbleBundleName;
        internal readonly string NovelsLocationBundleName => _novelsLocationBundleName;
        internal readonly string NovelsCharacterBundleName => _novelsCharacterBundleName;
        internal readonly string NovelsNotificationBundleName => _novelsNotificationBundleName;
        internal readonly string NovelsLocalizationBundleName => _novelsLocalizationBundleName;
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

        private async void SpeedUpForSaving(List<byte> initSave)
        {
            Time.timeScale = 15f;
            while(initSave.Count != 0)
                await UniTask.Yield();
            Time.timeScale = 1f;
        }

        internal async UniTask Init()
        {
            var pathGetter = new PathGetter(new PathGetter.Ctx
            {
                Prefix = _ctx.Data.Prefix,
            }).AddTo(this);

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
            var firstPreloding = UniTask.WhenAll(
                bundles.GetAssetBundle(_ctx.Data.NovelsSettingBundleName)
            );
            var secondPreloading = UniTask.WhenAll(
                bundles.GetText(pathGetter.GetNovelTextPath(_ctx.Data.StoryTextPath)),
                bundles.GetAssetBundle(_ctx.Data.NovelsBubbleBundleName),
                bundles.GetAssetBundle(_ctx.Data.NovelsLocationBundleName),
                bundles.GetAssetBundle(_ctx.Data.NovelsCharacterBundleName),
                bundles.GetAssetBundle(_ctx.Data.NovelsNotificationBundleName),
                bundles.GetAssetBundle(_ctx.Data.NovelsLocalizationBundleName)
            );

            await loading.Show();

            //preloading loading first
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await firstPreloding;

            var settingProcessCtx = new SettingProcess.Ctx
            {
                DefaultThreadPriority = _defaultThreadPriority,
                GetBundledPrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsSettingBundleName, pathGetter.GetSettingPrefabAssetName("Screen")),
                ShowLoading = loading.Show,
                HideLoading = loading.Hide,
            };
            var settingProcess = new SettingProcess(settingProcessCtx).AddTo(this);
            await settingProcess.ShowSettingProcess();

            //preloading loading second
            var storyText = string.Empty;
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
            {
                var (storyTextTemp, _, _, _, _, _) = await secondPreloading;
                storyText = storyTextTemp;
            }

            var localization = new Localization.Entity(new Localization.Entity.Ctx
            {
                Language = LocalizationData.Language.Rus,
                GetLocalizationSO = () => bundles.GetBundledSO<LocalizationData>(_ctx.Data.NovelsLocalizationBundleName, pathGetter.GetLocalizationDataAssetName("LocalizationData")),
            }).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await localization.Init();

            var storyProcessor = new StoryProcessor.Entity(new StoryProcessor.Entity.Ctx
            {
                StoryText = storyText,
            }).AddTo(this);

            var save = new List<byte>();
            using (var cache = new Cache.Entity())
            {
                try
                {
                    save = cache.ByteArrayFromCash("Save").ToList();
                }
                catch
                {
                    _ctx.OnLog((LogType.Log, "No save file"));
                }
            }
            var initSave = save.ToList();
            SpeedUpForSaving(initSave);

            var bubble = new Bubble.Entity(new Bubble.Entity.Ctx
            {
                GetBubblePrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsBubbleBundleName, pathGetter.GetBubblePrefabAssetName("Screen")),
            }).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await bubble.Init();

            var location = new Location.Entity(new Location.Entity.Ctx
            {
                GetScreenPrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsLocationBundleName, pathGetter.GetLocationPrefabAssetName("Screen")),
                GetSprite = assetName => bundles.GetBundledSprite(_ctx.Data.NovelsLocationBundleName, pathGetter.GetLocationImagePath(assetName)),
                GetVideoURL = pathGetter.GetVideoPath,

                OnLog = _ctx.OnLog,
            }).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await location.Init();

            var character = new Character.Entity(new Character.Entity.Ctx
            {
                MainCharacterName = _ctx.Data.MainCharacter,
                GetScreenPrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsCharacterBundleName, pathGetter.GetCharacterPrefabAssetName("Screen")),
                GetSprite = assetName => bundles.GetBundledSprite(_ctx.Data.NovelsCharacterBundleName, assetName),
                GetMainBodyPath = pathGetter.GetCharacterMainBodyPath,
                GetEmotionPath = pathGetter.GetCharacterEmotionPath,
                GetClothesPath = pathGetter.GetCharacterClothesPath
            }).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await character.Init();

            var notification = new Notification.Entity(new Notification.Entity.Ctx
            {
                GetNotificationPrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsNotificationBundleName, pathGetter.GetNotificationPrefabAssetName("Screen")),
            }).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await notification.Init();

            var waiting = new Waiting.Entity().AddTo(this);

            await loading.Hide();

            while (!IsDisposed)
            {
                var bubbleDone = new UniTaskCompletionSource();

                storyProcessor.TryGetNextText(out var text);

                var data = text.Split(":");
                var prefix = data.FirstOrDefault().Trim();
                var value = data.LastOrDefault().Trim();

                if (prefix.ToLower() == "title") continue;
                if (prefix.ToLower() == "series") continue;
                if (prefix.ToLower() == "genres") continue;
                if (prefix.ToLower() == "annotation") continue;
                if (prefix.ToLower() == "stats") continue;

                if (prefix.ToLower().Contains("keyboard")) continue;

                if (prefix.ToLower() == "music") continue;
                if (prefix.ToLower() == "sound") continue;
                if (prefix.ToLower() == "ambient") continue;

                if (prefix.ToLower() == "notification")
                {
                    notification.Show(value).Forget();
                    continue;
                }

                if (prefix.ToLower().Contains("location"))
                {
                    var locationRawArgsData = prefix.Split("(");
                    var locationArgs = locationRawArgsData.Length <= 1
                    ? new string[0]
                    : locationRawArgsData.LastOrDefault().Split(")").FirstOrDefault().Split(",").Select(a => a.Trim()).ToArray();

                    await location.SetImage(value, false, locationArgs);
                    continue;
                }
                if (prefix.ToLower() == "cut-scene")
                {
                    await location.SetImage(value, true, null);
                    continue;
                }
                if (prefix.ToLower() == "camera")
                {
                    await location.SetCamera(value);
                    continue;
                }
                if (prefix.ToLower() == "await")
                {
                    if (int.TryParse(value, out var seconds))
                        await waiting.Await(seconds);
                    continue;
                }

                var rawPrefixData = prefix.Split("(");
                var name = rawPrefixData.FirstOrDefault().Trim();
                var args = rawPrefixData.Length <= 1
                    ? new string[0]
                    : rawPrefixData.LastOrDefault().Split(")").FirstOrDefault().Split(",").Select(a => a.Trim()).ToArray();

                var characterName = string.Empty;
                if (!localization.TryGetValue(name, out characterName))
                    _ctx.OnLog.Invoke((LogType.Warning, $"No localized character name [{name}]"));

                bubble.SetText(text);
                bubble.RemoveAllButtons();
                var choices = storyProcessor.GetChoices();
                if (choices.Count > 0)
                    bubble.ResetBackgroundButton();
                else if (string.IsNullOrEmpty(text))
                    bubbleDone.TrySetResult();
                else
                    bubble.SetBackgroundButton(() => 
                    {
                        if (initSave.Count == 0)
                            save.Add(255);
                        bubbleDone.TrySetResult();
                    });
                foreach (var choice in choices)
                {
                    var choiceText = choice.text;
                    if (!localization.TryGetValue(choice.text, out choiceText))
                        _ctx.OnLog.Invoke((LogType.Warning, $"No localized choice [{choice.text}]"));
                    bubble.AddOrUpdateButton(choice.index, choiceText, id =>
                    {
                        SetCharacterView(character, args, choice);

                        if (initSave.Count == 0)
                            save.Add((byte)id);
                        storyProcessor.SetChoice(id);
                        bubbleDone.TrySetResult();
                    });
                }

                //show content
                var showProcess = UniTask.WhenAll(
                    character.SetImageAndShow(name, args),
                    bubble.Show()
                );
                await showProcess;

                if (initSave.Count == 0)
                {
                    await bubbleDone.Task;
                }
                else
                {
                    await UniTask.Yield();
                    var saveResult = initSave.First();
                    if (saveResult != 255)
                    {
                        SetCharacterView(character, args, storyProcessor.GetChoices()[saveResult]);
                        storyProcessor.SetChoice(saveResult);
                    }
                        
                    initSave.RemoveAt(0);
                }

                //reset content
                var resetProcess = UniTask.WhenAll(
                    character.Hide(),
                    bubble.Hide()
                );
                await resetProcess;

                if (initSave.Count == 0)
                {
                    using( var cache = new Cache.Entity())
                    {
                        cache.ByteArrayToCash(save.ToArray(), "Save");
                    }
                }
            }
        }

        private void SetCharacterView(Character.Entity character, string[] args, Ink.Runtime.Choice choice)
        {
            if (args.Any(a => a == "Выбери внешность"))
                character.SetMainCharacterView(choice.text);
            if (args.Any(a => a == "Выбери одежду"))
                character.SetMainCharacterWeather(choice.text);
        }
    }
}