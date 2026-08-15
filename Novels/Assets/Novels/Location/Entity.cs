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
        private const string _noVideo = "None";
        private const int _cutSceneFallbackDelayMilliseconds = 3000;

        public struct Ctx
        {
            public GameObject ScreenPrefab;
            public Func<string, UniTask<Sprite>> GetSprite;
            public Func<string, string> GetVideoURL;
            public CancellationToken CancellationToken;

            public Action<(LogType type, string message)> OnLog;
        }

        private readonly Ctx _ctx;

        private View.Screen _screen;

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
        }

        public UniTask SetImage(
            string assetName,
            StoryContracts.StoryBackgroundPresentation presentation)
        {
            return SetImage(
                assetName,
                presentation,
                false,
                presentation.Type == StoryContracts.StoryBackgroundType.CutScene);
        }

        private async UniTask SetImage(
            string assetName,
            StoryContracts.StoryBackgroundPresentation presentation,
            bool forceNoVideo,
            bool cutScene)
        {
            Camera.allCameras[0].backgroundColor = presentation.BackgroundColor
                == StoryContracts.StoryBackgroundColor.White
                ? Color.white
                : Color.black;

            await _screen.HideImage(_ctx.CancellationToken);

            _screen.ResetCamera();
            _screen.ResetEffect();

            var sprite = await _ctx.GetSprite(assetName).AttachExternalCancellation(_ctx.CancellationToken);
            _screen.SetImage(sprite);

            var url = _ctx.GetVideoURL(assetName);
            if (!forceNoVideo && url.Split("/").Last() != _noVideo)
            {
                var videoReady = false;
                var videoDone = false;
                var videoError = false;

                var rt = new RenderTexture(sprite.texture.width, sprite.texture.height, 16, RenderTextureFormat.ARGB32);
                rt.Create();
                var playbackSpeed = Time.timeScale;
                _screen.SetVideo(url, !cutScene, rt, playbackSpeed, () => videoReady = true, () => videoDone = true, () => videoError = true);
                while (!videoError && !videoReady) await UniTask.Yield(_ctx.CancellationToken);

                _screen.SetEnabledImage(videoError);
                _screen.SetEnabledVideo(!videoError);

                await _screen.ShowImage(_ctx.CancellationToken);

                if (cutScene)
                {
                    if (!videoError)
                    {
                        while (!videoDone)
                            await UniTask.Yield(_ctx.CancellationToken);
                    }
                    else
                    {
                        await UniTask.Delay(_cutSceneFallbackDelayMilliseconds, cancellationToken: _ctx.CancellationToken);// add zoom effect in future
                    }
                    
                    if (!presentation.KeepFinalVideoFrame)
                        await SetImage(assetName, presentation, true, false);
                }
            }
            else
            {
                _screen.SetEnabledImage(true);
                _screen.SetEnabledVideo(false);

                await _screen.ShowImage(_ctx.CancellationToken);
            }
        }

        public UniTask SetImageImmediate(
            string assetName,
            StoryContracts.StoryBackgroundPresentation presentation)
        {
            return SetImageImmediate(
                assetName,
                presentation,
                false,
                presentation.Type == StoryContracts.StoryBackgroundType.CutScene);
        }

        private async UniTask SetImageImmediate(
            string assetName,
            StoryContracts.StoryBackgroundPresentation presentation,
            bool forceNoVideo,
            bool cutScene)
        {
            Camera.allCameras[0].backgroundColor = presentation.BackgroundColor
                == StoryContracts.StoryBackgroundColor.White
                ? Color.white
                : Color.black;

            _screen.HideImageImmediate();

            _screen.ResetCamera();
            _screen.ResetEffect();

            var sprite = await _ctx.GetSprite(assetName).AttachExternalCancellation(_ctx.CancellationToken);
            _screen.SetImage(sprite);

            var url = _ctx.GetVideoURL(assetName);
            if (!forceNoVideo && url.Split("/").Last() != _noVideo)
            {
                var videoReady = false;
                var videoDone = false;
                var videoError = false;

                var rt = new RenderTexture(sprite.texture.width, sprite.texture.height, 16, RenderTextureFormat.ARGB32);
                rt.Create();
                var playbackSpeed = cutScene ? Time.timeScale * 5f : Time.timeScale;
                _screen.SetVideo(url, !cutScene, rt, playbackSpeed, () => videoReady = true, () => videoDone = true, () => videoError = true);
                while (!videoError && !videoReady) await UniTask.Yield(_ctx.CancellationToken);

                _screen.SetEnabledImage(videoError);
                _screen.SetEnabledVideo(!videoError);

                _screen.ShowImageImmediate();

                if (cutScene)
                {
                    if (!videoError)
                    {
                        while (!videoDone)
                            await UniTask.Yield(_ctx.CancellationToken);
                    }
                    else
                    {
                        await UniTask.Yield(_ctx.CancellationToken);
                    }
                    
                    if (!presentation.KeepFinalVideoFrame)
                        await SetImageImmediate(assetName, presentation, true, false);
                }
            }
            else
            {
                _screen.SetEnabledImage(true);
                _screen.SetEnabledVideo(false);

                _screen.ShowImageImmediate();
            }
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

        public async UniTask SetCameraImmediate(StoryContracts.StoryCameraAction action)
        {
            if (action == StoryContracts.StoryCameraAction.FadeIn)
            {
                _screen.SetEffectImmediate(View.Screen.Effect.Dark);
                return;
            }

            if (TryGetCameraEffect(action, out var effect))
            {
                _screen.SetCameraImmediate(effect);
                return;
            }

            _ctx.OnLog((LogType.Error, $"Camera action [{action}] not implemented"));
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

        public async UniTask SetDialogueImmediate(StoryContracts.StoryDialogueAlignment alignment)
        {
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
