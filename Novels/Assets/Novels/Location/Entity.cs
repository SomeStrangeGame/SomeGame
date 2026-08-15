using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Location
{
    public class Entity : BaseDisposable
    {
        private enum BackgroundPlaybackMode
        {
            Live,
            Immediate,
        }

        private const string _noVideo = "None";
        private const int _cutSceneFallbackDelayMilliseconds = 3000;

        public struct Ctx
        {
            public GameObject ScreenPrefab;
            public Camera TargetCamera;
            public Func<string, UniTask<Sprite>> GetSprite;
            public Func<string, string> GetVideoURL;
            public CancellationToken CancellationToken;

            public Action<(LogType type, string message)> OnLog;
        }

        private readonly Ctx _ctx;

        private View.Screen _screen;
        private VideoPlayback _videoPlayback;

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
            _videoPlayback = new VideoPlayback(new VideoPlayback.Ctx
            {
                VideoPlayer = _screen.VideoPlayer,
                SetTexture = _screen.SetVideoTexture,
                CancellationToken = _ctx.CancellationToken,
                OnLog = _ctx.OnLog,
            }).AddTo(this);
        }

        public UniTask SetImage(
            string assetName,
            StoryContracts.StoryBackgroundPresentation presentation)
        {
            return SetImage(
                assetName,
                presentation,
                BackgroundPlaybackMode.Live,
                false,
                presentation.Type == StoryContracts.StoryBackgroundType.CutScene);
        }

        private async UniTask SetImage(
            string assetName,
            StoryContracts.StoryBackgroundPresentation presentation,
            BackgroundPlaybackMode mode,
            bool forceNoVideo,
            bool cutScene)
        {
            if (_ctx.TargetCamera == null)
                throw new InvalidOperationException("Location target Camera is not configured.");

            _ctx.TargetCamera.backgroundColor = presentation.BackgroundColor
                == StoryContracts.StoryBackgroundColor.White
                ? Color.white
                : Color.black;

            if (mode == BackgroundPlaybackMode.Live)
                await _screen.HideImage(_ctx.CancellationToken);
            else
                _screen.HideImageImmediate();

            _screen.ResetCamera();
            _screen.ResetEffect();

            var sprite = await _ctx.GetSprite(assetName).AttachExternalCancellation(_ctx.CancellationToken);
            _screen.SetImage(sprite);

            var url = _ctx.GetVideoURL(assetName);
            if (!forceNoVideo && HasVideo(url))
            {
                var playbackSpeed = mode == BackgroundPlaybackMode.Immediate && cutScene
                    ? Time.timeScale * 5f
                    : Time.timeScale;
                var playbackStatus = await _videoPlayback.Play(new VideoPlaybackRequest(
                    url,
                    sprite.texture.width,
                    sprite.texture.height,
                    !cutScene,
                    playbackSpeed));
                var videoReady = playbackStatus == VideoPlaybackStatus.Ready;

                _screen.SetEnabledImage(!videoReady);
                _screen.SetEnabledVideo(videoReady);
                await ShowImage(mode);

                if (cutScene)
                {
                    if (videoReady)
                        playbackStatus = await _videoPlayback.WaitForCompletion();

                    if (playbackStatus == VideoPlaybackStatus.Failed)
                        await WaitForCutSceneFallback(mode);

                    if (!presentation.KeepFinalVideoFrame)
                        await SetImage(assetName, presentation, mode, true, false);
                }
            }
            else
            {
                _videoPlayback.Stop();
                _screen.SetEnabledImage(true);
                _screen.SetEnabledVideo(false);
                await ShowImage(mode);
            }
        }

        private UniTask ShowImage(BackgroundPlaybackMode mode)
        {
            if (mode == BackgroundPlaybackMode.Live)
                return _screen.ShowImage(_ctx.CancellationToken);

            _screen.ShowImageImmediate();
            return UniTask.CompletedTask;
        }

        private async UniTask WaitForCutSceneFallback(BackgroundPlaybackMode mode)
        {
            if (mode == BackgroundPlaybackMode.Live)
            {
                await UniTask.Delay(
                    _cutSceneFallbackDelayMilliseconds,
                    cancellationToken: _ctx.CancellationToken);
            }
            else
            {
                await UniTask.Yield(_ctx.CancellationToken);
            }
        }

        private static bool HasVideo(string url)
        {
            return url.Split("/").Last() != _noVideo;
        }

        public UniTask SetImageImmediate(
            string assetName,
            StoryContracts.StoryBackgroundPresentation presentation)
        {
            return SetImage(
                assetName,
                presentation,
                BackgroundPlaybackMode.Immediate,
                false,
                presentation.Type == StoryContracts.StoryBackgroundType.CutScene);
        }

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

            _ctx.OnLog((LogType.Error, $"Camera action [{action}] not implemented"));
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

            _ctx.OnLog((LogType.Error, $"Camera action [{action}] not implemented"));
            return UniTask.CompletedTask;
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
