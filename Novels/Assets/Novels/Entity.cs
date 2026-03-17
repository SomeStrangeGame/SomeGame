using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Disposable;
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

    internal partial class Entity : BaseDisposable
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
            var pathGetter = CreatePathGetter();
            var bundles = CreateBundles();
            var loading = await CreateLoading(bundles);

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

            var localization = await CreateLocalization(bundles, pathGetter);
            var storyProcessor = CreateStoryProcessor(storyText);
            var saveSystem = await CreateSaveSystem();
            var bubble = await CreateBubble(bundles, pathGetter);
            var location = await CreateLocation(bundles, pathGetter);
            var character = await CreateCharacter(bundles, pathGetter);
            var notification = await CreateNotification(bundles, pathGetter);
            var waiting = CreateWaiting();

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
                        saveSystem.TrySave();
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

                        saveSystem.TrySave((byte)id);
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

                if (!saveSystem.TryLoad(out var result))
                {
                    await bubbleDone.Task;
                }
                else
                {
                    await UniTask.Yield();
                    if (result != 255)
                    {
                        SetCharacterView(character, args, storyProcessor.GetChoices()[result]);
                        storyProcessor.SetChoice(result);
                    }
                }

                //reset content
                var resetProcess = UniTask.WhenAll(
                    character.Hide(),
                    bubble.Hide()
                );
                await resetProcess;
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