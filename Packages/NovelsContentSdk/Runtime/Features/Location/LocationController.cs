using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Location
{
    public class LocationController : BaseDisposable
    {
        public struct Dependencies
        {
            public GameObject ScreenPrefab;
            public Camera TargetCamera;
            public Func<string, UniTask<Sprite>> GetSprite;
            public Func<string, UniTask<Sprite>> GetFullQualitySprite;
            public Func<string, UniTask<string>> ResolveVideoUrl;
            public Sprite MissingBackground;
            public CancellationToken CancellationToken;
            public int CutSceneFallbackDelayMilliseconds;

            public Action<Diagnostics.NovelError> OnError;
        }

        private readonly Dependencies _ctx;

        private View.LocationScreen _screen;
        private BackgroundPresentationController _backgrounds;

        public LocationController(Dependencies ctx)
        {
            _ctx = ctx;
        }

        public void Init()
        {
            var screenGO = GameObject.Instantiate(_ctx.ScreenPrefab);
            _screen = screenGO.GetComponent<View.LocationScreen>();
            _screen.HideImageImmediate();
            _screen.ResetCamera();
            _screen.ResetEffect();
            var videoPlayback = new VideoPlayback(new VideoPlayback.Dependencies
            {
                VideoPlayer = _screen.VideoPlayer,
                SetTexture = _screen.SetVideoTexture,
                CancellationToken = _ctx.CancellationToken,
                OnError = _ctx.OnError,
            }).AddTo(this);
            _backgrounds = new BackgroundPresentationController(
                new BackgroundPresentationController.Dependencies
                {
                    Screen = _screen,
                    VideoPlayback = videoPlayback,
                    TargetCamera = _ctx.TargetCamera,
                    GetSprite = _ctx.GetSprite,
                    GetFullQualitySprite = _ctx.GetFullQualitySprite,
                    ResolveVideoUrl = _ctx.ResolveVideoUrl,
                    MissingBackground = _ctx.MissingBackground,
                    CancellationToken = _ctx.CancellationToken,
                    CutSceneFallbackDelayMilliseconds =
                        _ctx.CutSceneFallbackDelayMilliseconds,
                    OnError = _ctx.OnError,
                });
        }

        public UniTask SetImage(
            string assetName,
            StoryContracts.StoryBackgroundPresentation presentation,
            StoryContracts.PresentationMode mode)
            => _backgrounds.Set(assetName, presentation, mode);

        public UniTask EnableFullQuality() => _backgrounds.EnableFullQuality();

        public UniTask SetCamera(
            StoryContracts.StoryCameraAction action,
            StoryContracts.PresentationMode mode) =>
            ApplyCameraAction(action, mode);

        private void ReportUnsupportedCameraAction(
            StoryContracts.StoryCameraAction action)
        {
            _ctx.OnError?.Invoke(new Diagnostics.NovelError(
                Diagnostics.NovelErrorCodes.UnsupportedCameraAction,
                Diagnostics.NovelErrorSeverity.Recoverable,
                $"Camera action '{action}' is not implemented."));
        }

        private async UniTask ApplyCameraAction(
            StoryContracts.StoryCameraAction action,
            StoryContracts.PresentationMode mode)
        {
            if (!CameraActionPlan.TryCreate(action, out var plan))
            {
                ReportUnsupportedCameraAction(action);
                return;
            }
            switch (plan.Presentation)
            {
                case CameraActionPresentation.Motion:
                    if (mode == StoryContracts.PresentationMode.Immediate)
                        _screen.SetCameraImmediate(plan.Motion);
                    else
                        await _screen.SetCamera(plan.Motion, _ctx.CancellationToken);
                    break;
                case CameraActionPresentation.PersistentEffect:
                    if (mode == StoryContracts.PresentationMode.Immediate)
                        _screen.SetEffectImmediate(plan.Effect);
                    else
                        await _screen.SetEffect(plan.Effect, _ctx.CancellationToken);
                    break;
                case CameraActionPresentation.TransientEffect:
                    if (mode == StoryContracts.PresentationMode.Immediate)
                        _screen.ResetEffect();
                    else
                        await _screen.FlashEffect(plan.Effect, _ctx.CancellationToken);
                    break;
            }
        }

        public async UniTask SetDialogue(
            StoryContracts.StoryDialogueAlignment alignment,
            StoryContracts.PresentationMode mode)
        {
            if (mode == StoryContracts.PresentationMode.Animated)
                await _screen.SetDialogue(ToViewAlignment(alignment), _ctx.CancellationToken);
            else
                _screen.SetDialogueImmediate(ToViewAlignment(alignment));
        }

        private static TextAlignment ToViewAlignment(
            StoryContracts.StoryDialogueAlignment alignment)
        {
            return alignment switch
            {
                StoryContracts.StoryDialogueAlignment.Left => TextAlignment.Left,
                StoryContracts.StoryDialogueAlignment.Right => TextAlignment.Right,
                _ => TextAlignment.Center,
            };
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            if (_screen != null)
                GameObject.Destroy(_screen.gameObject);
        }
    }
}
