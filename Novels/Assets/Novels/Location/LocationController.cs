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
                    ResolveVideoUrl = _ctx.ResolveVideoUrl,
                    MissingBackground = _ctx.MissingBackground,
                    CancellationToken = _ctx.CancellationToken,
                    CutSceneFallbackDelayMilliseconds =
                        _ctx.CutSceneFallbackDelayMilliseconds,
                });
        }

        public UniTask SetImage(
            string assetName,
            StoryContracts.StoryBackgroundPresentation presentation)
            => _backgrounds.Set(assetName, presentation);

        public UniTask SetImageImmediate(
            string assetName,
            StoryContracts.StoryBackgroundPresentation presentation)
            => _backgrounds.SetImmediate(assetName, presentation);

        public UniTask SetCamera(StoryContracts.StoryCameraAction action) =>
            ApplyCameraAction(action, false);

        public UniTask SetCameraImmediate(StoryContracts.StoryCameraAction action) =>
            ApplyCameraAction(action, true);

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
            bool immediate)
        {
            if (!CameraActionPlan.TryCreate(action, out var plan))
            {
                ReportUnsupportedCameraAction(action);
                return;
            }
            switch (plan.Presentation)
            {
                case CameraActionPresentation.Motion:
                    if (immediate)
                        _screen.SetCameraImmediate(plan.Motion);
                    else
                        await _screen.SetCamera(plan.Motion, _ctx.CancellationToken);
                    break;
                case CameraActionPresentation.PersistentEffect:
                    if (immediate)
                        _screen.SetEffectImmediate(plan.Effect);
                    else
                        await _screen.SetEffect(plan.Effect, _ctx.CancellationToken);
                    break;
                case CameraActionPresentation.TransientEffect:
                    if (immediate)
                        _screen.ResetEffect();
                    else
                        await _screen.FlashEffect(plan.Effect, _ctx.CancellationToken);
                    break;
            }
        }

        public async UniTask SetDialogue(StoryContracts.StoryDialogueAlignment alignment)
        {
            await _screen.SetDialogue(ToViewAlignment(alignment), _ctx.CancellationToken);
        }

        public UniTask SetDialogueImmediate(StoryContracts.StoryDialogueAlignment alignment)
        {
            _screen.SetDialogueImmediate(ToViewAlignment(alignment));
            return UniTask.CompletedTask;
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
