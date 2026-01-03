using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    internal sealed partial class Entity
    {
        private readonly Dictionary<string, AssetBundle> _bundles = new();

        private void ClearBundles()
        {
            foreach(var bundle in _bundles)
                bundle.Value.Unload(false);
            _bundles.Clear();
        }

        private async UniTask<Sprite> GetBundledSprite(string bundleName, string spriteName)
        {
            var assetBundle = await GetAssetBundle(bundleName);
            if (assetBundle == null) return null;

            var loadAsset = assetBundle.LoadAssetAsync<Sprite>(spriteName);
            await loadAsset;
            return loadAsset.asset as Sprite;
        }

        private async UniTask<T> GetBundledSO<T>(string bundleName, string prefabName) where T : ScriptableObject
        {
            var assetBundle = await GetAssetBundle(bundleName);
            if (assetBundle == null) return null;

            var loadAsset = assetBundle.LoadAssetAsync<T>(prefabName);
            await loadAsset;
            return loadAsset.asset as T;
        }

        private async UniTask<GameObject> GetBundledPrefab(string bundleName, string prefabName)
        {
            var assetBundle = await GetAssetBundle(bundleName);
            if (assetBundle == null) return null;

            var loadAsset = assetBundle.LoadAssetAsync<GameObject>(prefabName);
            await loadAsset;
            return loadAsset.asset as GameObject;
        }

        private async UniTask<AssetBundle> GetAssetBundle(string bundleName)
        {
            var lastBundlesVersion = string.Empty;
            try
            {
                lastBundlesVersion = TextFromCache(GetBundleVersionName(bundleName));
                Debug.Log($"{GetBundleVersionName(bundleName)} last bundle version: {lastBundlesVersion}");
            }
            catch
            {
                Debug.LogWarning($"{GetBundleVersionName(bundleName)} no last bundle version file");
            }

            var bundlesVersion = string.Empty;
            try
            {
                //bundlesVersion = await GetBundleVersionAsync(bundleName);
                bundlesVersion = TextToCache(GetBundleVersionName(bundleName), await GetBundleVersionAsync(bundleName));
                Debug.Log($"{GetRemotePath(GetBundleVersionName(bundleName))} remote bundle version: {bundlesVersion}");
            }
            catch
            {
                Debug.LogError($"{GetRemotePath(GetBundleVersionName(bundleName))} no remote bundle version file");
                return null;
            }
            
            var bundlesPath = $"Remote/{bundlesVersion}/{GetPlatform()}/{bundleName}";
            var currentBundlesPath = string.Empty;
            if (lastBundlesVersion == bundlesVersion)
            {
                Debug.Log($"Bundles version is actual {bundlesVersion}");
                currentBundlesPath = ConvertLocalPath(bundlesPath);
            }
            else
            {
                Debug.LogWarning($"Required {bundleName} bundles update {lastBundlesVersion}/{bundlesVersion}");
                currentBundlesPath = GetRemotePath(bundlesPath);
            }
            Debug.Log($"{bundleName} bundle path - {currentBundlesPath}");

            if (!_bundles.TryGetValue(bundlesPath, out _))
            {
                try
                {
                    await LoadBundle(bundlesPath, currentBundlesPath);
                }
                catch
                {
                    Debug.LogWarning($"No local bundle {bundleName} in {currentBundlesPath}\nTry load from {GetRemotePath(bundlesPath)}");
                    await LoadBundle(bundlesPath, GetRemotePath(bundlesPath));
                }
            }
            return _bundles[bundlesPath];
        }

        private async UniTask LoadBundle(string key, string path)
        {
            using (var bundlesRequest = UnityWebRequest.Get(path))
            {
                SetHeaders(bundlesRequest);
                await bundlesRequest.SendWebRequest();
                _bundles[key] = await BundleToCache(key, bundlesRequest.downloadHandler.data);
            }
        }

        private async UniTask<string> GetBundleVersionAsync(string bundleName)
        {
            var bundlesVersion = string.Empty;
            var bundlesVersionPath = GetRemotePath(GetBundleVersionName(bundleName));
            using (var bundlesVersionRequest = UnityWebRequest.Get(bundlesVersionPath))
            {
                SetHeaders(bundlesVersionRequest);
                await bundlesVersionRequest.SendWebRequest();
                bundlesVersion = bundlesVersionRequest.downloadHandler.text;
            }
            return bundlesVersion;
        }

        private string GetBundleVersionName(string bundleName)
        {
            return $"{bundleName}_BundlesVersion.json";
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

