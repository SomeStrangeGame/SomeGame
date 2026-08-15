using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;
using UnityEngine.Audio;
using ThreadPriority = UnityEngine.ThreadPriority;

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
        [SerializeField] private string _novelsLoadingBundleName;
        [SerializeField] private string _novelsSettingBundleName;
        [SerializeField] private string _novelsBubbleBundleName;
        [SerializeField] private string _novelsLocationBundleName;
        [SerializeField] private string _novelsCharacterBundleName;
        [SerializeField] private string _novelsNotificationBundleName;
        [SerializeField] private string _novelsLocalizationBundleName;

        [SerializeField] private AudioMixer _audioMixer;

        internal readonly string Prefix => _prefix;
        internal readonly string MainCharacter => _mainCharacter;
        internal readonly string StoryTextPath => _storyTextPath;
        internal readonly string NovelsLoadingBundleName => _novelsLoadingBundleName;
        internal readonly string NovelsSettingBundleName => _novelsSettingBundleName;
        internal readonly string NovelsBubbleBundleName => _novelsBubbleBundleName;
        internal readonly string NovelsLocationBundleName => _novelsLocationBundleName;
        internal readonly string NovelsCharacterBundleName => _novelsCharacterBundleName;
        internal readonly string NovelsNotificationBundleName => _novelsNotificationBundleName;
        internal readonly string NovelsLocalizationBundleName => _novelsLocalizationBundleName;
        internal readonly AudioMixer AudioMixer => _audioMixer;
    }

    internal partial class Entity : BaseDisposable
    {
        private const string _screenAssetName = "Screen";
        private const string _localizationDataAssetName = "LocalizationData";
        private const ThreadPriority _defaultThreadPriority = ThreadPriority.Low;

        internal struct Ctx
        {
            internal Data Data;
            internal CancellationToken CancellationToken;
            public Action<(LogType type, string message)> OnLog;
            internal Action<Diagnostics.NovelError> OnError;
        }

        private readonly Ctx _ctx;
        private readonly Content.NovelDefinition _definition;

        internal Entity(Ctx ctx)
        {
            _ctx = ctx;
            _definition = CreateNovelDefinition(ctx.Data);
            Application.backgroundLoadingPriority = _defaultThreadPriority;
        }

        internal async UniTask Init()
        {
            var state = new BootstrapState();
            var bootstrap = new NovelBootstrapProcess(
                new NovelBootstrapProcess.Ctx
                {
                    SelectStart = () => PrepareApplication(state),
                    ClearSave = () => state.SaveSystem.Clear(),
                    RunEpisode = () => RunEpisode(state),
                    CancellationToken = _ctx.CancellationToken,
                }).AddTo(this);

            await bootstrap.Run();
        }
    }
}
