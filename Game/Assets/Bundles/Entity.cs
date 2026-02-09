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
        private readonly Cache.Entity _cache;
        private readonly Dictionary<string, AssetBundle> _bundles = new();

        public Entity()
        {
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
            var assetBundle = await GetAssetBundle(bundleName);
            if (assetBundle == null) return null;

            var loadAsset = assetBundle.LoadAssetAsync<Sprite>(assetName);
            await loadAsset;
            return loadAsset.asset as Sprite;
        }

        public async UniTask<T> GetBundledSO<T>(string bundleName, string assetName) where T : ScriptableObject
        {
            var assetBundle = await GetAssetBundle(bundleName);
            if (assetBundle == null) return null;

            var loadAsset = assetBundle.LoadAssetAsync<T>(assetName);
            await loadAsset;
            return loadAsset.asset as T;
        }

        public async UniTask<GameObject> GetBundledPrefab(string bundleName, string assetName)
        {
            var assetBundle = await GetAssetBundle(bundleName);
            if (assetBundle == null) return null;

            var loadAsset = assetBundle.LoadAssetAsync<GameObject>(assetName);
            await loadAsset;
            return loadAsset.asset as GameObject;
        }

        public async UniTask<AssetBundle> GetAssetBundle(string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName)) return null;

            var bundlesVersion = await GetBundleVersionAsync(bundleName);
            var bundlesPath = $"Remote/{GetPlatform()}/{bundleName}/{bundlesVersion}";
            if (!_bundles.TryGetValue(bundlesPath, out _))
            {
                try
                {
                    _bundles[bundlesPath] = await _cache.BundleFromCache(bundlesPath);
                    Debug.Log($"Get local bundle from {bundlesPath}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"No local bundle {bundleName} in {bundlesPath}\nTry load from {GetRemotePath(bundlesPath)}\n---\n{e}");
                    using (var bundlesRequest = UnityWebRequest.Get(GetRemotePath(bundlesPath)))
                    {
                        SetHeaders(bundlesRequest);
                        await bundlesRequest.SendWebRequest();
                        _bundles[bundlesPath] = await _cache.BundleToCache(bundlesPath, bundlesRequest.downloadHandler.data);
                    }
                }
            }
            else
            {
                Debug.Log($"Get bundle {bundleName} from cache");
            }
            return _bundles[bundlesPath];
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
#if UNITY_EDITOR_OSX
            return "Mac";
#elif UNITY_STANDALONE_WIN
            return "Win";
#elif UNITY_WEBGL
            return "WebGL";
#else
            return string.Empty;
#endif
        }

        private string GetRemotePath(string localPath)
        {
            var result = $"{Application.streamingAssetsPath}/{localPath}";
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            result = $"file://{result}";
#endif
            return result;
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

