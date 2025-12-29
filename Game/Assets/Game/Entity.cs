using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Disposable;
using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    [Serializable]
    internal struct Data
    {
        [SerializeField] private Loading.Data _loadingData;
        [SerializeField] private Chapter_OnlyScreen.Data _chapter_intro;
        [SerializeField] private Chapter_ScreenAndBattle.Data[] _chapters;

        internal readonly Loading.Data LoadingData => _loadingData;
        internal readonly Chapter_OnlyScreen.Data Chapter_intro => _chapter_intro;
        internal readonly Chapter_ScreenAndBattle.Data[] Chapters => _chapters;
    }

    internal sealed partial class Entity : BaseDisposable
    {
        internal struct Ctx
        {
            internal Data Data;
        }

        private Loading.Entity _loading;
        private string _bundlesVersion = string.Empty;
        private readonly Dictionary<string, AssetBundle> _bundles;

        private readonly Ctx _ctx;

        internal Entity(Ctx ctx)
        {
            _ctx = ctx;
            _bundles = new();
        }

        internal async UniTask Init()
        {
            _loading = new Loading.Entity(new Loading.Entity.Ctx
            {
                Data = _ctx.Data.LoadingData,
                GetBundledPrefab = data => GetBundledPrefab(data.bundleName, data.prefabName),
            }).AddTo(this);
            await _loading.Init();
            _loading.ShowImmediate();

            Chapter_introProcess().Forget();
        }

        private async UniTask<Sprite> GetBundledSprite(string bundleName, string spriteName)
        {
            var assetBundle = await GetAssetBundle(bundleName);

            var loadAsset = assetBundle.LoadAssetAsync<Sprite>(spriteName);
            await loadAsset;
            return loadAsset.asset as Sprite;
        }

        private async UniTask<T> GetBundledSO<T>(string bundleName, string prefabName) where T : ScriptableObject
        {
            var assetBundle = await GetAssetBundle(bundleName);

            var loadAsset = assetBundle.LoadAssetAsync<T>(prefabName);
            await loadAsset;
            return loadAsset.asset as T;
        }

        private async UniTask<GameObject> GetBundledPrefab(string bundleName, string prefabName)
        {
            var assetBundle = await GetAssetBundle(bundleName);

            var loadAsset = assetBundle.LoadAssetAsync<GameObject>(prefabName);
            await loadAsset;
            return loadAsset.asset as GameObject;
        }

        private async UniTask<AssetBundle> GetAssetBundle(string bundleName)
        {
            var bundlesVersion = await GetBundleVersionAsync();
            var bundlesPath = $"Remote/{bundlesVersion}/{GetPlatform()}/{bundleName}";
            if (!_bundles.TryGetValue(bundlesPath, out _))
            {
                using (var bundlesRequest = UnityWebRequestAssetBundle.GetAssetBundle(GetPath(bundlesPath)))
                {
                    SetHeaders(bundlesRequest);
                    await bundlesRequest.SendWebRequest();
                    _bundles[bundlesPath] = DownloadHandlerAssetBundle.GetContent(bundlesRequest);
                }
            }
            return _bundles[bundlesPath];
        }

        private async UniTask<string> GetBundleVersionAsync()
        {
            if (string.IsNullOrEmpty(_bundlesVersion))
            {
                var bundlesVersionPath = GetPath("BundlesVersion.json");
                using (var bundlesVersionRequest = UnityWebRequest.Get(bundlesVersionPath))
                {
                    SetHeaders(bundlesVersionRequest);
                    await bundlesVersionRequest.SendWebRequest();
                    _bundlesVersion = bundlesVersionRequest.downloadHandler.text;
                }
            }
            return _bundlesVersion;
        }

        private string GetPlatform()
        {
#if UNITY_EDITOR_OSX
            return "Mac";
#elif PLATFORM_WEBGL
            return "WebGL";
#else
            return string.Empty;
#endif
        }

        private string GetPath(string localPath)
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