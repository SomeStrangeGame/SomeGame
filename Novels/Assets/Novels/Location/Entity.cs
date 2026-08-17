using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Location
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public GameObject ScreenPrefab;
            public Camera TargetCamera;
            public Func<string, UniTask<Sprite>> GetSprite;
            public Func<string, UniTask<string>> ResolveVideoUrl;
            public CancellationToken CancellationToken;

            public Action<Diagnostics.NovelError> OnError;
        }

        private readonly Ctx _ctx;

        private View.Screen _screen;
        private BackgroundPresentationController _backgrounds;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void Init()
        {
            var screenGO = GameObject.Instantiate(_ctx.ScreenPrefab);
            _screen = screenGO.GetComponent<View.Screen>();
            _screen.HideImageImmediate();
            _screen.ResetCamera();
            _screen.ResetEffect();
            var videoPlayback = new VideoPlayback(new VideoPlayback.Ctx
            {
                VideoPlayer = _screen.VideoPlayer,
                SetTexture = _screen.SetVideoTexture,
                CancellationToken = _ctx.CancellationToken,
                OnError = _ctx.OnError,
            }).AddTo(this);
            _backgrounds = new BackgroundPresentationController(
                new BackgroundPresentationController.Ctx
                {
                    Screen = _screen,
                    VideoPlayback = videoPlayback,
                    TargetCamera = _ctx.TargetCamera,
                    GetSprite = _ctx.GetSprite,
                    ResolveVideoUrl = _ctx.ResolveVideoUrl,
                    CancellationToken = _ctx.CancellationToken,
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

        public async UniTask SetCamera(StoryContracts.StoryCameraAction action)
        {
            if (action == StoryContracts.StoryCameraAction.FadeIn)
            {
                await _screen.SetEffect(View.Screen.Effect.Dark, _ctx.CancellationToken);
                return;
            }

            if (TryGetCameraEffect(action, out var effect))
            {
                await _screen.SetCamera(effect, _ctx.CancellationToken);
                return;
            }

            ReportUnsupportedCameraAction(action);
        }

        public UniTask SetCameraImmediate(StoryContracts.StoryCameraAction action)
        {
            if (action == StoryContracts.StoryCameraAction.FadeIn)
            {
                _screen.SetEffectImmediate(View.Screen.Effect.Dark);
                return UniTask.CompletedTask;
            }

            if (TryGetCameraEffect(action, out var effect))
            {
                _screen.SetCameraImmediate(effect);
                return UniTask.CompletedTask;
            }

            ReportUnsupportedCameraAction(action);
            return UniTask.CompletedTask;
        }

        private void ReportUnsupportedCameraAction(
            StoryContracts.StoryCameraAction action)
        {
            _ctx.OnError?.Invoke(new Diagnostics.NovelError(
                Diagnostics.NovelErrorCodes.UnsupportedCameraAction,
                Diagnostics.NovelErrorSeverity.Recoverable,
                $"Camera action '{action}' is not implemented."));
        }

        private static bool TryGetCameraEffect(
            StoryContracts.StoryCameraAction action,
            out View.Screen.CameraEffect effect)
        {
            switch (action)
            {
                case StoryContracts.StoryCameraAction.PanLeftToRight:
                    effect = View.Screen.CameraEffect.LeftRight;
                    return true;

                case StoryContracts.StoryCameraAction.PanRightToLeft:
                    effect = View.Screen.CameraEffect.RightLeft;
                    return true;

                case StoryContracts.StoryCameraAction.MoveToCenter:
                    effect = View.Screen.CameraEffect.ToCenter;
                    return true;

                case StoryContracts.StoryCameraAction.MoveToLeft:
                    effect = View.Screen.CameraEffect.ToLeft;
                    return true;

                case StoryContracts.StoryCameraAction.Shake:
                    effect = View.Screen.CameraEffect.Shaking;
                    return true;

                default:
                    effect = default;
                    return false;
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
