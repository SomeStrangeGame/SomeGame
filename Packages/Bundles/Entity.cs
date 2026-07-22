using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;
using UnityEngine.Networking;

namespace Bundles
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public string Prefix;
            public Action<(LogType type, string message)> OnLog;
        }

        private readonly Dictionary<string, Sprite> _sprites = new();
        private readonly Dictionary<string, ScriptableObject> _scriptableObjects = new();
        private readonly Dictionary<string, GameObject> _prefabs = new();

        private readonly Cache.Entity _cache;
        private readonly Dictionary<string, AssetBundle> _bundles = new();

        private readonly Dictionary<string, string> _videos = new();
        private readonly Dictionary<string, string> _audio = new();

        private Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
            _cache = new Cache.Entity().AddTo(this);
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            ClearBundles();
        }

        private void ClearBundles()
        {
            foreach(var bundle in _bundles)
                bundle.Value.Unload(false);
            _bundles.Clear();
        }

        public async UniTask<Sprite> GetBundledSprite(string bundleName, string assetName)
        {
            var assetBundle = _bundles[GetBundleKey(bundleName)];
            if (assetBundle == null) return null;
            if (string.IsNullOrEmpty(assetName)) return null;
            if (!_sprites.ContainsKey(assetName))
                _sprites[assetName] = await assetBundle.LoadAssetAsync<Sprite>(assetName) as Sprite;
            return _sprites[assetName];
        }

        public async UniTask<T> GetBundledSO<T>(string bundleName, string assetName) where T : ScriptableObject
        {
            var assetBundle = _bundles[GetBundleKey(bundleName)];
            if (assetBundle == null) return null;
            if (string.IsNullOrEmpty(assetName)) return null;
            if (!_scriptableObjects.ContainsKey(assetName))
                _scriptableObjects[assetName] = await assetBundle.LoadAssetAsync<T>(assetName) as T;
            return _scriptableObjects[assetName] as T;
        }

        public async UniTask<GameObject> GetBundledPrefab(string bundleName, string assetName)
        {
            var assetBundle = _bundles[GetBundleKey(bundleName)];
            if (assetBundle == null) return null;
            if (string.IsNullOrEmpty(assetName)) return null;
            if (!_prefabs.ContainsKey(assetName))
                _prefabs[assetName] = await assetBundle.LoadAssetAsync<GameObject>(assetName) as GameObject;
            return _prefabs[assetName];
        }

        public string GetVideoURL(string assetName)
        {
            if (!_videos.ContainsKey(assetName.ToLower())) return "None";
            return _videos[assetName.ToLower()];
        }

        public async UniTask<AssetBundle> GetAssetBundle(string bundleName)
        {
            var log = (LogType.Warning, "bundle name is empty");
            if (string.IsNullOrEmpty(bundleName)) 
            {
                _ctx.OnLog.Invoke(log);
                return null;
            }

            var bundlesVersion = await GetBundleVersionAsync(bundleName);
            var bundlesKey = GetBundleKey(bundleName);
            var bundlesPath = $"{bundlesKey}/{bundlesVersion}";
            if (!_bundles.TryGetValue(bundlesKey, out _))
            {
                try
                {
                    _bundles[bundlesKey] = await _cache.BundleFromCache(bundlesKey);
                    log = (LogType.Log, $"Get local bundle from {bundlesKey}");
                }
                catch (Exception e)
                {
                    log = (LogType.Warning, $"No local bundle {bundleName} in {bundlesKey}\nTry load from {GetRemotePath(bundlesPath)}\n---\n{e}");
                    using (var bundlesRequest = UnityWebRequest.Get(GetRemotePath(bundlesPath)))
                    {
                        SetHeaders(bundlesRequest);
                        await bundlesRequest.SendWebRequest();
                        _bundles[bundlesKey] = await _cache.BundleToCache(bundlesKey, bundlesRequest.downloadHandler.data);
                    }
                }
            }
            else
            {
                log = (LogType.Log, $"Get bundle {bundleName} from cache");
            }
            _ctx.OnLog.Invoke(log);
            return _bundles[bundlesKey];
        }

        public async UniTask LoadVideosToDict()
        {
            var allVideos = _bundles["Remote/Android/novels_location"].GetAllAssetNames().Where(a => a.Contains(".png")).Select(a => a.Replace(".png", "")).ToArray();
            List<UniTask> cacheVideoProcesses = new();
            foreach (var video in allVideos)
            {
                cacheVideoProcesses.Add(CacheVideo(video));
            }
            await UniTask.WhenAll(cacheVideoProcesses);
        }

        private async UniTask CacheVideo(string video)
        {
            var videoName = video.Split("/").Last().ToLower();

            var log = (LogType.Warning, $"No video for {video}");
            var path = $"NovelsVideos/{_ctx.Prefix}/{videoName}.mp4";
            try
            {
                var videoFile = _cache.ByteArrayFromCash(path);
                _videos[videoName.ToLower()] = _cache.ConvertLocalPath(path);
                log = (LogType.Log, $"Get video local from: {videoName} - {_videos[videoName.ToLower()]}");
            }
            catch
            {
                try
                {
                    var url = GetRemotePath(path);
                    using (var videoRequest = UnityWebRequest.Get(url))
                    {
                        SetHeaders(videoRequest);
                        await videoRequest.SendWebRequest();
                        _cache.ByteArrayToCash(videoRequest.downloadHandler.data, path);
                        _videos[videoName.ToLower()] = _cache.ConvertLocalPath(path);
                        log = (LogType.Warning, $"Load video remote: {videoName} - {_videos[videoName.ToLower()]}");
                    }
                }
                catch
                {
                    // ignore
                }
            }
            _ctx.OnLog.Invoke(log);
        }

        public void LoadAudioToDict(string audio)
        {
            var audioName = audio.Split("/").Last().ToLower();
            var path = $"NovelsAudio/{_ctx.Prefix}/{audioName}.wav";
            _audio[audioName.ToLower()] = GetRemotePath(path);
        }

        public string GetAudioURL(string assetName)
        {
            if (!_audio.ContainsKey(assetName.ToLower())) return "None";
            return _audio[assetName.ToLower()];
        }

        private string GetBundleKey(string bundleName)
        {
            return $"Remote/{GetPlatform()}/{bundleName}";
        }

        private async UniTask<string> GetBundleVersionAsync(string bundleName)
        {
            var bundlesVersion = string.Empty;
            var bundlesVersionPath = GetRemotePath($"Remote/{GetPlatform()}/{bundleName}/version.txt");
            using (var bundlesVersionRequest = UnityWebRequest.Get(bundlesVersionPath))
            {
                SetHeaders(bundlesVersionRequest);
                await bundlesVersionRequest.SendWebRequest();
                bundlesVersion = bundlesVersionRequest.downloadHandler.text;
            }
            return bundlesVersion;
        }

        public async UniTask<string> GetText(string path)
        {
            var result = string.Empty;
            var textPath = GetRemotePath(path);
            using (var request = UnityWebRequest.Get(textPath))
            {
                SetHeaders(request);
                await request.SendWebRequest();
                result = request.downloadHandler.text;
            }
            return result;
        }

        private string GetPlatform()
        {
#if UNITY_STANDALONE_OSX
            return "Mac";
#elif UNITY_STANDALONE_WIN
            return "Win";
#elif UNITY_WEBGL
            return "WebGL";
#elif UNITY_ANDROID
            return "Android";
#else
            return string.Empty;
#endif
        }

        private string GetRemotePath(string localPath)
        {
            var localResult = $"{Application.streamingAssetsPath}/{localPath}";
#if UNITY_EDITOR_OSX
            localResult = $"file://{localResult}";
#endif
            return localResult;
        }

        private void SetHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("Access-Control-Allow-Credentials", "true");
            request.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
            request.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            request.SetRequestHeader("Access-Control-Allow-Origin", "*");
        }
    }
}

