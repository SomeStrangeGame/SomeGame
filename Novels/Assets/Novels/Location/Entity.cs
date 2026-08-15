using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Location
{
    public class Entity : BaseDisposable
    {
        private const string _noVideo = "None";
        private const int _cutSceneFallbackDelayMilliseconds = 3000;

        private static class CameraCommands
        {
            internal const string FadeIn = "fadein";
            internal const string LeftRight = "leftright";
            internal const string RightLeft = "rightleft";
            internal const string ToCenter = "tocenter";
            internal const string ToLeft = "toleft";
        }

        public struct Ctx
        {
            public GameObject ScreenPrefab;
            public Func<string, UniTask<Sprite>> GetSprite;
            public Func<string, string> GetVideoURL;

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

        public async UniTask SetImage(string assetName, bool cutScene, bool forceNoVideo, string[] args)
        {
            Camera.allCameras[0].backgroundColor = Color.black;
            if (args != null && args.Any(a => a == StoryContracts.StoryArguments.WhiteBackground))
                Camera.allCameras[0].backgroundColor = Color.white;

            await _screen.HideImage();

            _screen.ResetCamera();
            _screen.ResetEffect();

            var sprite = await _ctx.GetSprite(assetName);
            _screen.SetImage(sprite);

            var url = _ctx.GetVideoURL(assetName);
#if UNITY_EDITOR_OSX
            url = $"file:///Users/iantonishin/SomeGame/Novels/{url}";
#endif
            if (!forceNoVideo && url.Split("/").Last() != _noVideo)
            {
                var videoReady = false;
                var videoDone = false;
                var videoError = false;

                var rt = new RenderTexture(sprite.texture.width, sprite.texture.height, 16, RenderTextureFormat.ARGB32);
                rt.Create();
                var playbackSpeed = Time.timeScale;
                _screen.SetVideo(url, !cutScene, rt, playbackSpeed, () => videoReady = true, () => videoDone = true, () => videoError = true);
                while (!videoError && !videoReady) await UniTask.Yield();

                _screen.SetEnabledImage(videoError);
                _screen.SetEnabledVideo(!videoError);

                await _screen.ShowImage();

                if (cutScene)
                {
                    if (!videoError)
                    {
                        while (!videoDone)
                            await UniTask.Yield();
                    }
                    else
                    {
                        await UniTask.Delay(_cutSceneFallbackDelayMilliseconds);// add zoom effect in future
                    }
                    
                    if (!args.Contains(StoryContracts.StoryArguments.EndCutScene))
                        await SetImage(assetName, false, true, args);
                }
            }
            else
            {
                _screen.SetEnabledImage(true);
                _screen.SetEnabledVideo(false);

                await _screen.ShowImage();
            }
        }

        public async UniTask SetImageImmediate(string assetName, bool cutScene, bool forceNoVideo, string[] args)
        {
            Camera.allCameras[0].backgroundColor = Color.black;
            if (args != null && args.Any(a => a == StoryContracts.StoryArguments.WhiteBackground))
                Camera.allCameras[0].backgroundColor = Color.white;

            _screen.HideImageImmediate();

            _screen.ResetCamera();
            _screen.ResetEffect();

            var sprite = await _ctx.GetSprite(assetName);
            _screen.SetImage(sprite);

            var url = _ctx.GetVideoURL(assetName);
#if UNITY_EDITOR_OSX
            url = $"file:///Users/iantonishin/SomeGame/Novels/{url}";
#endif
            if (!forceNoVideo && url.Split("/").Last() != _noVideo)
            {
                var videoReady = false;
                var videoDone = false;
                var videoError = false;

                var rt = new RenderTexture(sprite.texture.width, sprite.texture.height, 16, RenderTextureFormat.ARGB32);
                rt.Create();
                var playbackSpeed = cutScene ? Time.timeScale * 5f : Time.timeScale;
                _screen.SetVideo(url, !cutScene, rt, playbackSpeed, () => videoReady = true, () => videoDone = true, () => videoError = true);
                while (!videoError && !videoReady) await UniTask.Yield();

                _screen.SetEnabledImage(videoError);
                _screen.SetEnabledVideo(!videoError);

                _screen.ShowImageImmediate();

                if (cutScene)
                {
                    if (!videoError)
                    {
                        while (!videoDone)
                            await UniTask.Yield();
                    }
                    else
                    {
                        await UniTask.Yield();
                    }
                    
                    if (!args.Contains(StoryContracts.StoryArguments.EndCutScene))
                        await SetImageImmediate(assetName, false, true, args);
                }
            }
            else
            {
                _screen.SetEnabledImage(true);
                _screen.SetEnabledVideo(false);

                _screen.ShowImageImmediate();
            }
        }

        public async UniTask SetCamera(string value)
        {
            if (string.Equals(value, CameraCommands.FadeIn, StringComparison.OrdinalIgnoreCase))
            {
                _screen.SetEffect(View.Screen.Effect.Dark).Forget();
                return;
            }
            if (string.Equals(value, CameraCommands.LeftRight, StringComparison.OrdinalIgnoreCase))
            {
                await _screen.SetCamera(View.Screen.CameraEffect.LeftRight);
                return;
            }
            if (string.Equals(value, CameraCommands.RightLeft, StringComparison.OrdinalIgnoreCase))
            {
                await _screen.SetCamera(View.Screen.CameraEffect.RightLeft);
                return;
            }
            if (string.Equals(value, CameraCommands.ToCenter, StringComparison.OrdinalIgnoreCase))
            {
                await _screen.SetCamera(View.Screen.CameraEffect.ToCenter);
                return;
            }
            if (string.Equals(value, CameraCommands.ToLeft, StringComparison.OrdinalIgnoreCase))
            {
                await _screen.SetCamera(View.Screen.CameraEffect.ToLeft);
                return;
            }
            _ctx.OnLog((LogType.Error, $"Camera value [{value}] not implemented"));
        }

        public async UniTask SetCameraImmediate(string value)
        {
            if (string.Equals(value, CameraCommands.FadeIn, StringComparison.OrdinalIgnoreCase))
            {
                _screen.SetEffectImmediate(View.Screen.Effect.Dark);
                return;
            }
            if (string.Equals(value, CameraCommands.LeftRight, StringComparison.OrdinalIgnoreCase))
            {
                _screen.SetCameraImmediate(View.Screen.CameraEffect.LeftRight);
                return;
            }
            if (string.Equals(value, CameraCommands.RightLeft, StringComparison.OrdinalIgnoreCase))
            {
                _screen.SetCameraImmediate(View.Screen.CameraEffect.RightLeft);
                return;
            }
            if (string.Equals(value, CameraCommands.ToCenter, StringComparison.OrdinalIgnoreCase))
            {
                _screen.SetCameraImmediate(View.Screen.CameraEffect.ToCenter);
                return;
            }
            if (string.Equals(value, CameraCommands.ToLeft, StringComparison.OrdinalIgnoreCase))
            {
                _screen.SetCameraImmediate(View.Screen.CameraEffect.ToLeft);
                return;
            }
            _ctx.OnLog((LogType.Error, $"Camera value [{value}] not implemented"));
        }

        public async UniTask SetDialogue(TextAlignment aligment)
        {
            await _screen.SetDialogue(aligment);
        }

        public async UniTask SetDialogueImmediate(TextAlignment aligment)
        {
            _screen.SetDialogueImmediate(aligment);
        }
    }
}
