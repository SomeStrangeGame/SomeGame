using System;
using System.Linq;
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

        public async UniTask SetImage(bool immediate, string assetName, bool cutScene, bool forceNoVideo, string[] args)
        {
            Camera.allCameras[0].backgroundColor = Color.black;
            if (args != null && args.Any(a => a == "white"))
                Camera.allCameras[0].backgroundColor = Color.white;

            if (!immediate)
                await _screen.HideImage();
            else
                _screen.HideImageImmediate();

            _screen.ResetCamera();
            _screen.ResetEffect();

            var sprite = await _ctx.GetSprite(assetName);
            _screen.SetImage(sprite);

            var url = _ctx.GetVideoURL(assetName);
#if UNITY_EDITOR_OSX
            url = $"file:///Users/iantonishin/SomeGame/Novels/{url}";
#endif
            if (!forceNoVideo && url.Split("/").Last() != "None")
            {
                var videoReady = false;
                var videoDone = false;
                var videoError = false;

                var rt = new RenderTexture(sprite.texture.width, sprite.texture.height, 16, RenderTextureFormat.ARGB32);
                rt.Create();
                var playbackSpeed = (immediate && cutScene) ? Time.timeScale * 5f : Time.timeScale;
                _screen.SetVideo(url, !cutScene, rt, playbackSpeed, () => videoReady = true, () => videoDone = true, () => videoError = true);
                while (!videoError && !videoReady) await UniTask.Yield();

                _screen.SetEnabledImage(videoError);
                _screen.SetEnabledVideo(!videoError);

                if (!immediate)
                    await _screen.ShowImage();
                else
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
                        if (!immediate)
                            await UniTask.Delay(3000);// add zoom effect in future
                        else
                            await UniTask.Yield();
                    }
                    
                    if (!args.Contains("end"))
                        await SetImage(immediate, assetName, false, true, args);
                }
            }
            else
            {
                _screen.SetEnabledImage(true);
                _screen.SetEnabledVideo(false);

                if (!immediate)
                    await _screen.ShowImage();
                else
                    _screen.ShowImageImmediate();
            }
        }

        public async UniTask SetCamera(bool immediate, string value)
        {
            if (value.ToLower() == "fadein")
            {
                if (!immediate)
                    _screen.SetEffect(View.Screen.Effect.Dark).Forget();
                else
                    _screen.SetEffectImmediate(View.Screen.Effect.Dark);
                return;
            }
            if (value.ToLower() == "leftright")
            {
                await _screen.SetCamera(immediate, View.Screen.CameraEffect.LeftRight);
                return;
            }
            if (value.ToLower() == "rightleft")
            {
                await _screen.SetCamera(immediate, View.Screen.CameraEffect.RightLeft);
                return;
            }
            if (value.ToLower() == "tocenter")
            {
                await _screen.SetCamera(immediate, View.Screen.CameraEffect.ToCenter);
                return;
            }
            if (value.ToLower() == "ToLeft")
            {
                await _screen.SetCamera(immediate, View.Screen.CameraEffect.ToLeft);
                return;
            }
            _ctx.OnLog((LogType.Error, $"Camera value [{value}] not implemented"));
        }

        public async UniTask SetDialogue(bool immediate, TextAlignment aligment)
        {
            await _screen.SetDialogue(immediate, aligment);
        }
    }
}

