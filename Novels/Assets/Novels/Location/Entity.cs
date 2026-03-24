using System;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;
using UnityEngine.Networking;

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
            try
            {
                using (var req = UnityWebRequest.Get(url))
                {
                    await req.SendWebRequest();
                    var videoData = req.downloadHandler.data;
                    ByteArrayToCash(videoData, $"Videos/{assetName}.mp4");
                    url = ConvertLocalPath($"Videos/{assetName}.mp4");
                }
            }
            catch
            {

            }
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

        public async UniTask SetImageImmediate(string assetName, bool cutScene, string[] args)
        {
            Camera.allCameras[0].backgroundColor = Color.black;
            if (args != null && args.Any(a => a == "white"))
                Camera.allCameras[0].backgroundColor = Color.white;

            _screen.HideImageImmediate();

            _screen.ResetCamera();
            _screen.ResetEffect();

            Debug.Log(assetName);
            var sprite = _ctx.GetSprite(assetName);
            _screen.SetImage(sprite);

            var videoReady = false;
            var videoDone = false;
            var videoError = false;

            var url = _ctx.GetVideoURL(assetName);
            try
            {
                using (var req = UnityWebRequest.Get(url))
                {
                    await req.SendWebRequest();
                    var videoData = req.downloadHandler.data;
                    ByteArrayToCash(videoData, $"Videos/{assetName}.mp4");
                    url = ConvertLocalPath($"Videos/{assetName}.mp4");
                }
            }
            catch
            {

            }
            _screen.SetVideo(url, !cutScene, () => videoReady = true, () => videoDone = true, () => videoError = true);
            while (!videoError && !videoReady) await UniTask.Yield();

            _screen.SetEnabledImage(videoError);
            _screen.SetEnabledVideo(!videoError);

            _screen.ShowImageImmediate();

            await UniTask.Yield();
        }

        private void ByteArrayToCash(byte[] data, string path)
        {
            var file = ConvertLocalPath(path);
            if (File.Exists(file))
                File.Delete(file);
            using (var fs = File.Create(file))
            {
                fs.Write(data, 0, data.Length);
            }
        }

        private string ConvertLocalPath(string path)
        {
            var localFilesPath = GetLocalPath();

            if (!Directory.Exists(localFilesPath))
                Directory.CreateDirectory(localFilesPath);

            var localExtraPath = path.Split('/');
            for (var i = 0; i < localExtraPath.Length - 1; i++)
            {
                localFilesPath += "/" + localExtraPath[i];
                if (!Directory.Exists(localFilesPath))
                    Directory.CreateDirectory(localFilesPath);
            }

            return $"{localFilesPath}/{localExtraPath.Last()}";
        }
        
        private string GetLocalPath() 
        {
            return $"{Application.persistentDataPath}/CachedFiles";
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

        public void SetCameraImmediate(string value)
        {
            if (value.ToLower() == "fadein")
            {
                _screen.SetEffect(View.Screen.Effect.Dark).Forget();
                return;
            }
            if (value.ToLower() == "leftright")
            {
                _screen.SetCameraImmediate(View.Screen.CameraEffect.LeftRight);
                return;
            }
            if (value.ToLower() == "rightleft")
            {
                _screen.SetCameraImmediate(View.Screen.CameraEffect.RightLeft);
                return;
            }
            if (value.ToLower() == "tocenter")
            {
                _screen.SetCameraImmediate(View.Screen.CameraEffect.ToCenter);
                return;
            }
            if (value.ToLower() == "ToLeft")
            {
                _screen.SetCameraImmediate(View.Screen.CameraEffect.ToLeft);
                return;
            }
            _ctx.OnLog((LogType.Error, $"Camera value [{value}] not implemented"));
        }

        public async UniTask SetDialog(TextAlignment aligment)
        {
            await _screen.SetDialog(aligment);
        }

        public void SetDialogImmediate(TextAlignment aligment)
        {
            _screen.SetDialogImmediate(aligment);
        }
    }
}

