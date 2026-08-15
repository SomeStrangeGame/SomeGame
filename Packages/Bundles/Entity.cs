using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
            public CancellationToken CancellationToken;
            public Action<(LogType type, string message)> OnLog;
        }

        private readonly Dictionary<string, Sprite> _sprites = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ScriptableObject> _scriptableObjects = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, GameObject> _prefabs = new(StringComparer.OrdinalIgnoreCase);

        private readonly Cache.Entity _cache;
        private readonly Dictionary<string, AssetBundle> _bundles = new(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, string> _videos = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _audio = new(StringComparer.OrdinalIgnoreCase);

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
            var assetBundle = GetLoadedBundle(bundleName);
            if (assetBundle == null) return null;
            if (string.IsNullOrEmpty(assetName)) return null;
            if (!_sprites.ContainsKey(assetName))
            {
                _sprites[assetName] = await assetBundle
                    .LoadAssetAsync<Sprite>(assetName)
                    .WithCancellation(_ctx.CancellationToken) as Sprite;
            }
            return _sprites[assetName];
        }

        public async UniTask<T> GetBundledSO<T>(string bundleName, string assetName) where T : ScriptableObject
        {
            var assetBundle = GetLoadedBundle(bundleName);
            if (assetBundle == null) return null;
            if (string.IsNullOrEmpty(assetName)) return null;
            if (!_scriptableObjects.ContainsKey(assetName))
            {
                _scriptableObjects[assetName] = await assetBundle
                    .LoadAssetAsync<T>(assetName)
                    .WithCancellation(_ctx.CancellationToken) as T;
            }
            return _scriptableObjects[assetName] as T;
        }

        public async UniTask<GameObject> GetBundledPrefab(string bundleName, string assetName)
        {
            var assetBundle = GetLoadedBundle(bundleName);
            if (assetBundle == null) return null;
            if (string.IsNullOrEmpty(assetName)) return null;
            if (!_prefabs.ContainsKey(assetName))
            {
                _prefabs[assetName] = await assetBundle
                    .LoadAssetAsync<GameObject>(assetName)
                    .WithCancellation(_ctx.CancellationToken) as GameObject;
            }
            return _prefabs[assetName];
        }

        public string GetVideoURL(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return "None";
            return _videos.TryGetValue(assetName, out var url) ? url : "None";
        }

        public async UniTask<AssetBundle> GetAssetBundle(string bundleName)
        {
            var log = (LogType.Warning, "bundle name is empty");
            if (string.IsNullOrEmpty(bundleName)) 
            {
                _ctx.OnLog.Invoke(log);
                return null;
            }

            var bundlesKey = GetBundleKey(bundleName);
            if (_bundles.TryGetValue(bundlesKey, out var loadedBundle))
            {
                _ctx.OnLog.Invoke((LogType.Log, $"Get bundle {bundleName} from memory"));
                return loadedBundle;
            }

            var bundlesVersion = await GetBundleVersionAsync(bundleName);
            var bundlesPath = $"{bundlesKey}/{bundlesVersion}";
            var cachePath = bundlesPath;
            try
            {
                var cachedBundle = await _cache
                    .BundleFromCache(cachePath)
                    .AttachExternalCancellation(_ctx.CancellationToken);
                if (cachedBundle == null)
                    throw new InvalidDataException($"Cached bundle '{cachePath}' is invalid.");
                _bundles[bundlesKey] = cachedBundle;
                log = (LogType.Log, $"Get local bundle from {cachePath}");
            }
            catch (OperationCanceledException) when (_ctx.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception e)
            {
                log = (LogType.Warning, $"No local bundle {bundleName} in {bundlesKey}\nTry load from {GetRemotePath(bundlesPath)}\n---\n{e}");
                var data = await DownloadBytes(bundlesPath);
                _ctx.CancellationToken.ThrowIfCancellationRequested();
                var downloadedBundle = await _cache
                    .BundleToCache(cachePath, data)
                    .AttachExternalCancellation(_ctx.CancellationToken);
                if (downloadedBundle == null)
                    throw new InvalidDataException($"Downloaded bundle '{bundlesPath}' is invalid.");
                _bundles[bundlesKey] = downloadedBundle;
            }
            _ctx.OnLog.Invoke(log);
            return _bundles[bundlesKey];
        }

        public async UniTask LoadVideosToDict(string locationBundleName)
        {
            var locationBundle = GetLoadedBundle(locationBundleName);
            var allVideos = locationBundle.GetAllAssetNames()
                .Where(a => a.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                .Select(a => a.Substring(0, a.Length - ".png".Length))
                .ToArray();
            List<UniTask> cacheVideoProcesses = new();
            foreach (var video in allVideos)
            {
                cacheVideoProcesses.Add(CacheVideo(video));
            }
            await UniTask.WhenAll(cacheVideoProcesses);
        }

        private async UniTask CacheVideo(string video)
        {
            var videoName = video.Split('/').Last();

            var log = (LogType.Warning, $"No video for {video}");
            var path = $"NovelsVideos/{_ctx.Prefix}/{videoName}.mp4";
            try
            {
                if (!_cache.Exists(path))
                    throw new FileNotFoundException("Cached video not found.", path);

                _videos[videoName] = ToFileUrl(_cache.ConvertLocalPath(path));
                log = (LogType.Log, $"Get video local from: {videoName} - {_videos[videoName]}");
            }
            catch (OperationCanceledException) when (_ctx.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception cacheException)
            {
                try
                {
                    var data = await DownloadBytes(path);
                    _ctx.CancellationToken.ThrowIfCancellationRequested();
                    _cache.WriteBytes(path, data);
                    _videos[videoName] = ToFileUrl(_cache.ConvertLocalPath(path));
                    log = (LogType.Log, $"Load video remote: {videoName} - {_videos[videoName]}");
                }
                catch (OperationCanceledException) when (_ctx.CancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception downloadException)
                {
                    log = (
                        LogType.Warning,
                        $"No video for {video}\nCache: {cacheException.Message}\nSource: {downloadException.Message}");
                }
            }
            _ctx.OnLog.Invoke(log);
        }

        private static string ToFileUrl(string path)
        {
            return new Uri(path).AbsoluteUri;
        }

        public void LoadAudioToDict(string audio)
        {
            if (string.IsNullOrEmpty(audio)) return;

            var audioName = audio.Split('/').Last();
            var path = $"NovelsAudio/{_ctx.Prefix}/{audioName}.wav";
            _audio[audioName] = GetRemotePath(path);
        }

        public string GetAudioURL(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return "None";
            return _audio.TryGetValue(assetName, out var url) ? url : "None";
        }

        private string GetBundleKey(string bundleName)
        {
            return $"Remote/{GetPlatform()}/{bundleName}";
        }

        private async UniTask<string> GetBundleVersionAsync(string bundleName)
        {
            var path = $"Remote/{GetPlatform()}/{bundleName}/version.txt";
            var bundlesVersion = (await DownloadText(path)).Trim();
            if (bundlesVersion.Length == 0)
                throw new InvalidDataException($"Bundle version is empty for '{bundleName}'.");
            return bundlesVersion;
        }

        public async UniTask<string> GetText(string path)
        {
            return await DownloadText(path);
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
            throw new PlatformNotSupportedException(
                "AssetBundle platform is not configured for the active build target.");
#endif
        }

        private string GetRemotePath(string localPath)
        {
            var localResult = $"{Application.streamingAssetsPath}/{localPath}";
#if UNITY_EDITOR_OSX
            localResult = new Uri(localResult).AbsoluteUri;
#endif
            return localResult;
        }

        private AssetBundle GetLoadedBundle(string bundleName)
        {
            var key = GetBundleKey(bundleName);
            if (!_bundles.TryGetValue(key, out var bundle) || bundle == null)
                throw new InvalidOperationException($"AssetBundle '{bundleName}' is not loaded.");
            return bundle;
        }

        private async UniTask<byte[]> DownloadBytes(string path)
        {
            using (var request = UnityWebRequest.Get(GetRemotePath(path)))
            {
                await Send(request);
                return request.downloadHandler.data;
            }
        }

        private async UniTask<string> DownloadText(string path)
        {
            using (var request = UnityWebRequest.Get(GetRemotePath(path)))
            {
                await Send(request);
                return request.downloadHandler.text;
            }
        }

        private async UniTask Send(UnityWebRequest request)
        {
            await request.SendWebRequest().WithCancellation(_ctx.CancellationToken);
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException(
                    $"Request failed [{request.responseCode}] {request.url}: {request.error}");
            }
        }
    }
}
