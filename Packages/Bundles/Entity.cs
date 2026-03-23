using System;
using System.Collections.Generic;
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
            public Action<(LogType type, string message)> OnLog;
        }

        private readonly Cache.Entity _cache;
        private readonly Dictionary<string, AssetBundle> _bundles = new();

        private readonly Dictionary<string, GameObject> _bundledPrefabs = new();
        private readonly Dictionary<string, ScriptableObject> _bundledSOs = new();
        private readonly Dictionary<string, Sprite> _bundledSprites = new();

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

        public Sprite GetBundledSprite(string bundleName, string assetName)
        {
            var assetBundle = _bundles[GetBundleKey(bundleName)];
            if (assetBundle == null) return null;
            if (!_bundledSprites.ContainsKey(assetName.ToLower())) return null;
            return _bundledSprites[assetName.ToLower()];
        }

        public T GetBundledSO<T>(string bundleName, string assetName) where T : ScriptableObject
        {
            var assetBundle = _bundles[GetBundleKey(bundleName)];
            if (assetBundle == null) return null;
            if (!_bundledSOs.ContainsKey(assetName.ToLower())) return null;
            return _bundledSOs[assetName.ToLower()] as T;
        }

        public GameObject GetBundledPrefab(string bundleName, string assetName)
        {
            var assetBundle = _bundles[GetBundleKey(bundleName)];
            if (assetBundle == null) return null;
            if (!_bundledPrefabs.ContainsKey(assetName.ToLower())) return null;
            return _bundledPrefabs[assetName.ToLower()];
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

        public async UniTask LoadAssetsToDict(string bundleName = null)
        {
            List<UniTask> addToDict = new ();
            if (!string.IsNullOrEmpty(bundleName))
            {
                var bundleKey = GetBundleKey(bundleName);
                foreach(var asset in _bundles[bundleKey].GetAllAssetNames())
                        addToDict.Add(AddAssetToDict(asset, bundleKey));
            }
            else
            {
                foreach(var assetBundle in _bundles)
                {
                    foreach(var asset in assetBundle.Value.GetAllAssetNames())
                        addToDict.Add(AddAssetToDict(asset, assetBundle.Key));
                }
            }
            
            await UniTask.WhenAll(addToDict);
        }

        private async UniTask AddAssetToDict(string asset, string bundlesKey)
        {
            if (asset.Contains(".prefab"))
            {
                if (!_bundledPrefabs.ContainsKey(asset.ToLower()))
                    _bundledPrefabs[asset.ToLower()] = await _bundles[bundlesKey].LoadAssetAsync<GameObject>(asset) as GameObject;
            }
            else if (asset.Contains(".asset"))
            {
                if (!_bundledSOs.ContainsKey(asset.ToLower()))
                    _bundledSOs[asset.ToLower()] = await _bundles[bundlesKey].LoadAssetAsync<ScriptableObject>(asset) as ScriptableObject;
            }
            else if (asset.Contains(".png"))
            {
                if (!_bundledSprites.ContainsKey(asset.ToLower()))
                    _bundledSprites[asset.ToLower()] = await _bundles[bundlesKey].LoadAssetAsync<Sprite>(asset) as Sprite;
            }
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
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
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

