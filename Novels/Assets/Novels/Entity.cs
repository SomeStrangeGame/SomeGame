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

            var novelProcessCtx = new NovelProcess.Ctx
            {
                StoryProcessor = storyProcessor,
                Notification = notification,
                Location = location,
                Waiting = waiting,
                Localization = localization,
                Bubble = bubble,
                SaveSystem = saveSystem,
                Character = character,

                ShowLoading = loading.Show,
                HideLoading = loading.Hide,

                OnLog = _ctx.OnLog,
            };
            var novelProcess = new NovelProcess(novelProcessCtx).AddTo(this);
            await novelProcess.ShowNovelProcess();
        }
    }
}