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
            public Func<UniTask<VideosSO>> GetVideosList;
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

        public async UniTask SetImage(string assetName, bool waitVideo)
        {
            Debug.Log(assetName);
            await _screen.HideImage();

            _screen.ResetCamera();
            _screen.ResetEffect();

            var sprite = await _ctx.GetSprite(assetName);
            _screen.SetImage(sprite);

            var videos = await _ctx.GetVideosList();
            var hasVideo = videos.Videos.Contains(assetName);

            _screen.SetEnabledImage(!hasVideo);
            _screen.SetEnabledVideo(hasVideo);

            var videoReady = false;
            var videoDone = false;
            if (hasVideo)
                _screen.SetVideo(_ctx.GetVideoURL(assetName), !waitVideo, () => videoReady = true, () => videoDone = true);
            
            if (hasVideo) while (!videoReady) await UniTask.NextFrame();

            await _screen.ShowImage();
            if (hasVideo && waitVideo) while (!videoDone) await UniTask.NextFrame();
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
            //Debug.Log($"Camera: {value}");
        }
    }
}

