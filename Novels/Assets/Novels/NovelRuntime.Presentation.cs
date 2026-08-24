using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class NovelRuntime
    {
        private EpisodePresentation CreateEpisodePresentation(
            PreparedEpisode state,
            EpisodeAssetSet assets,
            Loading.Entity loading)
        {
            var cancellationToken = state.CancellationToken;
            return new EpisodePresentation
            {
                Loading = loading,
                Audio = CreateAudio(
                    state.EpisodeScope,
                    state.StoryMedia.ResolveAudioUrl,
                    cancellationToken),
                Bubble = CreateBubble(
                    state.EpisodeScope,
                    assets.Bubble,
                    cancellationToken),
                Character = CreateCharacter(
                    state.EpisodeScope,
                    assets.Character,
                    assetName => GetCharacterSprite(state, assetName),
                    _ctx.FallbackAssets.Character,
                    cancellationToken),
                Choose = CreateChoose(state.EpisodeScope, cancellationToken),
                Location = CreateLocation(
                    state.EpisodeScope,
                    assets.Location,
                    assetName => _priorityLoader.Run(() => state.StoryAssets
                        .TryGetBundledSprite(new Bundles.BundleAssetAddress(
                            _definition.BundleName,
                            state.Addresses.LocationImage(assetName)))
                        .AttachExternalCancellation(cancellationToken)),
                    assetName => state.StoryMedia.ResolveVideoUrl(
                        _definition.ResolveVideoId(assetName)),
                    _ctx.FallbackAssets.Background,
                    cancellationToken),
                Notification = CreateNotification(
                    state.EpisodeScope,
                    assets.Notification,
                    cancellationToken),
                Wardrobe = CreateWardrobe(state.EpisodeScope, cancellationToken),
            };
        }

        private Audio.AudioController CreateAudio(
            IBaseDisposable owner,
            Func<string, UniTask<string>> resolveAudioUrl,
            CancellationToken cancellationToken)
        {
            return new Audio.AudioController(new Audio.AudioController.Dependencies
            {
                ResolveAudioUrl = resolveAudioUrl,
                AudioMixer = _audioMixer,
                CancellationToken = cancellationToken,

                OnLog = _ctx.OnLog,
                OnError = ReportError,
            }).AddTo(owner);
        }

        private Bubble.BubbleController CreateBubble(
            IBaseDisposable owner,
            GameObject bubblePrefab,
            CancellationToken cancellationToken)
        {
            var bubble = new Bubble.BubbleController(new Bubble.BubbleController.Dependencies
            {
                BubblePrefab = bubblePrefab,
                CancellationToken = cancellationToken,
            }).AddTo(owner);
            bubble.Init();
            return bubble;
        }

        private Character.CharacterController CreateCharacter(
            IBaseDisposable owner,
            GameObject screenPrefab,
            Func<string, UniTask<Sprite>> getSprite,
            Sprite missingCharacter,
            CancellationToken cancellationToken)
        {
            var character = new Character.CharacterController(
                new Character.CharacterController.Dependencies
                {
                    ScreenPrefab = screenPrefab,
                    ContentPrefix = _definition.Prefix,
                    AssetProfile = _definition.CharacterAssets,
                    GetSprite = getSprite,
                    MissingCharacter = missingCharacter,
                    CancellationToken = cancellationToken,
                }).AddTo(owner);
            character.Init();
            return character;
        }

        private static Choose.ChooseController CreateChoose(
            IBaseDisposable owner,
            CancellationToken cancellationToken)
        {
            var choose = new Choose.ChooseController(new Choose.ChooseController.Dependencies
            {
                CancellationToken = cancellationToken,
            }).AddTo(owner);
            choose.Init();
            return choose;
        }

        private async UniTask<Sprite> GetChooseSprite(
            PreparedEpisode state,
            string assetName)
        {
            var cancellationToken = state.CancellationToken;
            var sprite = await _priorityLoader.Run(() => state.StoryAssets
                .TryGetBundledSprite(new Bundles.BundleAssetAddress(
                    _definition.BundleName,
                    state.Addresses.ChooseItem(assetName)))
                .AttachExternalCancellation(cancellationToken));
            return sprite != null ? sprite : _ctx.FallbackAssets.Background;
        }

        private Loading.Entity CreateMainLoading(GameObject bundledPrefab) =>
            CreateLoading(this, bundledPrefab, _ctx.CancellationToken);

        private static Loading.Entity CreateLoading(
            IBaseDisposable owner,
            GameObject bundledPrefab,
            CancellationToken cancellationToken)
        {
            var loading = new Loading.Entity(new Loading.Entity.Ctx
            {
                BundledPrefab = bundledPrefab,
                CancellationToken = cancellationToken,
            }).AddTo(owner);
            loading.Init();
            return loading;
        }

        private Location.LocationController CreateLocation(
            IBaseDisposable owner,
            GameObject screenPrefab,
            Func<string, UniTask<Sprite>> getSprite,
            Func<string, UniTask<string>> resolveVideoUrl,
            Sprite missingBackground,
            CancellationToken cancellationToken)
        {
            var location = new Location.LocationController(
                new Location.LocationController.Dependencies
                {
                    ScreenPrefab = screenPrefab,
                    TargetCamera = _ctx.TargetCamera,
                    GetSprite = getSprite,
                    ResolveVideoUrl = resolveVideoUrl,
                    MissingBackground = missingBackground,
                    CancellationToken = cancellationToken,
                    CutSceneFallbackDelayMilliseconds =
                        _ctx.RuntimeTuning.CutSceneFallbackDelayMilliseconds,
                    OnError = ReportError,
                }).AddTo(owner);
            location.Init();
            return location;
        }

        private Notification.NotificationController CreateNotification(
            IBaseDisposable owner,
            GameObject notificationPrefab,
            CancellationToken cancellationToken)
        {
            var notification = new Notification.NotificationController(
                new Notification.NotificationController.Dependencies
                {
                    NotificationPrefab = notificationPrefab,
                    CancellationToken = cancellationToken,
                    DisplayDuration = _ctx.RuntimeTuning.NotificationDuration,
                    OnError = ReportError,
                }).AddTo(owner);
            notification.Init();
            return notification;
        }

        private static Wardrobe.WardrobeController CreateWardrobe(
            IBaseDisposable owner,
            CancellationToken cancellationToken)
        {
            var wardrobe = new Wardrobe.WardrobeController(
                new Wardrobe.WardrobeController.Dependencies
                {
                    CancellationToken = cancellationToken,
                }).AddTo(owner);
            wardrobe.Init();
            return wardrobe;
        }
    }
}
