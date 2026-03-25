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
            public Func<GameObject> GetScreenPrefab;
            public Func<string, Sprite> GetSprite;
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
            var prefab = _ctx.GetScreenPrefab();
            var screenGO = GameObject.Instantiate(prefab);
            _screen = screenGO.GetComponent<View.Screen>();
            _screen.HideImageImmediate();
            _screen.ResetCamera();
            _screen.ResetEffect();
        }

        public async UniTask SetImage(string assetName, bool cutScene, string[] args)
        {
            Camera.allCameras[0].backgroundColor = Color.black;
            if (args != null && args.Any(a => a == "white"))
                Camera.allCameras[0].backgroundColor = Color.white;

            await _screen.HideImage();

            _screen.ResetCamera();
            _screen.ResetEffect();

            var sprite = _ctx.GetSprite(assetName);
            _screen.SetImage(sprite);

            var videoReady = false;
            var videoDone = false;
            var videoError = false;

            var url = _ctx.GetVideoURL(assetName);
            _screen.SetVideo(url, !cutScene, () => videoReady = true, () => videoDone = true, () => videoError = true);
            while (!videoError && !videoReady) await UniTask.Yield();

            _screen.SetEnabledImage(videoError);
            _screen.SetEnabledVideo(!videoError);

            await _screen.ShowImage();
            if (cutScene)
            {
                if (!videoError)
                    while (!videoDone) await UniTask.Yield();
                else 
                    await UniTask.Delay(3000);
            }
        }

        public async UniTask SetCamera(string value)
        {
            if (value.ToLower() == "fadein")
            {
                _screen.SetEffect(View.Screen.Effect.Dark).Forget();
                return;
            }
            if (value.ToLower() == "leftright")
            {
                await _screen.SetCamera(View.Screen.CameraEffect.LeftRight);
                return;
            }
            if (value.ToLower() == "rightleft")
            {
                await _screen.SetCamera(View.Screen.CameraEffect.RightLeft);
                return;
            }
            if (value.ToLower() == "tocenter")
            {
                await _screen.SetCamera(View.Screen.CameraEffect.ToCenter);
                return;
            }
            if (value.ToLower() == "ToLeft")
            {
                await _screen.SetCamera(View.Screen.CameraEffect.ToLeft);
                return;
            }
            _ctx.OnLog((LogType.Error, $"Camera value [{value}] not implemented"));
        }

        public async UniTask SetDialog(TextAlignment aligment)
        {
            await _screen.SetDialog(aligment);
        }
    }
}

