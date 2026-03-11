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
            public Func<UniTask<GameObject>> GetScreenPrefab;
            public Func<string, UniTask<Sprite>> GetSprite;
            public Func<string, string> GetVideoURL;
        }

        private readonly Ctx _ctx;

        private View.Screen _screen;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask Init()
        {
            var prefab = await _ctx.GetScreenPrefab();
            var screenGO = GameObject.Instantiate(prefab);
            _screen = screenGO.GetComponent<View.Screen>();
            _screen.HideImageImmediate();
            _screen.ResetCamera();
            _screen.ResetEffect();
        }

        public async UniTask SetImage(string assetName, bool cutScene, string[] args)
        {
            Camera.allCameras[0].backgroundColor = Color.black;
            if (args != null && args.Any(a => a == "белый"))
                Camera.allCameras[0].backgroundColor = Color.white;

            await _screen.HideImage();

            _screen.ResetCamera();
            _screen.ResetEffect();

            var sprite = await _ctx.GetSprite(assetName);
            _screen.SetImage(sprite);

            var videoReady = false;
            var videoDone = false;
            var videoError = false;
            _screen.SetVideo(_ctx.GetVideoURL(assetName), !cutScene, () => videoReady = true, () => videoDone = true, () => videoError = true);
            while (!videoError && !videoReady) await UniTask.NextFrame();

            _screen.SetEnabledImage(videoError);
            _screen.SetEnabledVideo(!videoError);

            await _screen.ShowImage();
            if (cutScene)
            {
                if (!videoError) while (!videoDone) await UniTask.NextFrame();
                else await UniTask.Delay(3000);
            }
        }

        public async UniTask SetCamera(string value)
        {
            if (value.ToLower() == "затемнение")
            {
                _screen.SetEffect(View.Screen.Effect.Dark).Forget();
                return;
            }
            if (value.ToLower() == "слева направо")
            {
                await _screen.SetCamera(View.Screen.CameraEffect.LeftRight);
                return;
            }
            if (value.ToLower() == "справа налево")
            {
                await _screen.SetCamera(View.Screen.CameraEffect.RightLeft);
                return;
            }
            if (value.ToLower() == "сместить в центр")
            {
                await _screen.SetCamera(View.Screen.CameraEffect.ToCenter);
                return;
            }
            Debug.LogWarning($"Camera value [{value}] not implemented");
        }
    }
}

