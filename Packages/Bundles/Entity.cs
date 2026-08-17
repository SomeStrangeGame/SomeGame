using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Bundles
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public string Prefix;
            public CancellationToken CancellationToken;
            public Action<(LogType type, string message)> OnLog;
            public Action<BundleFailure> OnFailure;
        }

        private readonly Dictionary<string, Sprite> _sprites = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ScriptableObject> _scriptableObjects = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, GameObject> _prefabs = new(StringComparer.OrdinalIgnoreCase);

        private readonly Cache.Entity _cache;
        private readonly Dictionary<string, AssetBundle> _bundles = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Dictionary<string, string>> _assetNames = new(
            StringComparer.OrdinalIgnoreCase);

        private readonly StreamingAssetsSource _source;
        private readonly MediaResolver _media;

        private Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
            _cache = new Cache.Entity(Application.persistentDataPath).AddTo(this);
            _source = new StreamingAssetsSource(ctx.CancellationToken);
            _media = new MediaResolver(ctx.Prefix, _cache, _source, ctx.CancellationToken);
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            ClearBundles();
        }

        public Scope CreateScope()
        {
            return new Scope(this);
        }

        private void ClearBundles()
        {
            foreach(var bundle in _bundles)
                bundle.Value.Unload(false);
            _bundles.Clear();
            _assetNames.Clear();
        }

        public async UniTask<Sprite> GetBundledSprite(string bundleName, string assetName)
        {
            var assetBundle = GetLoadedBundle(bundleName);
            if (assetBundle == null) return null;
            if (string.IsNullOrEmpty(assetName)) return null;
            assetName = ResolveAssetName(bundleName, assetName);
            if (assetName == null) return null;
            var assetKey = GetAssetKey(bundleName, assetName);
            if (!_sprites.ContainsKey(assetKey))
            {
                _sprites[assetKey] = await assetBundle
                    .LoadAssetAsync<Sprite>(assetName)
                    .WithCancellation(_ctx.CancellationToken) as Sprite;
            }
            return _sprites[assetKey];
        }

        public async UniTask<T> GetBundledSO<T>(string bundleName, string assetName) where T : ScriptableObject
        {
            var assetBundle = GetLoadedBundle(bundleName);
            if (assetBundle == null) return null;
            if (string.IsNullOrEmpty(assetName)) return null;
            var requestedName = assetName;
            assetName = ResolveAssetName(bundleName, assetName);
            if (assetName == null)
            {
                ReportMissingAsset(bundleName, requestedName);
                return null;
            }
            var assetKey = GetAssetKey(bundleName, assetName);
            if (!_scriptableObjects.ContainsKey(assetKey))
            {
                _scriptableObjects[assetKey] = await assetBundle
                    .LoadAssetAsync<T>(assetName)
                    .WithCancellation(_ctx.CancellationToken) as T;
            }
            return _scriptableObjects[assetKey] as T;
        }

        public async UniTask<GameObject> GetBundledPrefab(string bundleName, string assetName)
        {
            var assetBundle = GetLoadedBundle(bundleName);
            if (assetBundle == null) return null;
            if (string.IsNullOrEmpty(assetName)) return null;
            var requestedName = assetName;
            assetName = ResolveAssetName(bundleName, assetName);
            if (assetName == null)
            {
                ReportMissingAsset(bundleName, requestedName);
                return null;
            }
            var assetKey = GetAssetKey(bundleName, assetName);
            if (!_prefabs.ContainsKey(assetKey))
            {
                _prefabs[assetKey] = await assetBundle
                    .LoadAssetAsync<GameObject>(assetName)
                    .WithCancellation(_ctx.CancellationToken) as GameObject;
            }
            return _prefabs[assetKey];
        }

        public void ConfigureMedia(MediaManifest manifest)
        {
            _media.Configure(manifest);
        }

        public UniTask<string> ResolveVideoUrl(string assetName) =>
            _media.ResolveVideoUrl(assetName);

        public UniTask<string> ResolveAudioUrl(string assetName) =>
            _media.ResolveAudioUrl(assetName);

        public async UniTask<AssetBundle> GetAssetBundle(string bundleName)
        {
            var log = (LogType.Warning, "bundle name is empty");
            if (string.IsNullOrEmpty(bundleName)) 
            {
                _ctx.OnLog.Invoke(log);
                _ctx.OnFailure?.Invoke(new BundleFailure(
                    BundleFailureCodes.InvalidBundleName,
                    "Bundle name is empty."));
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
                RegisterAssetNames(bundlesKey, cachedBundle);
                _cache.PruneDirectory(bundlesKey, bundlesVersion);
                log = (LogType.Log, $"Get local bundle from {cachePath}");
            }
            catch (OperationCanceledException) when (_ctx.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception e)
            {
                log = (LogType.Warning, $"No local bundle {bundleName} in {bundlesKey}\nTry load from {_source.GetUrl(bundlesPath)}\n---\n{e}");
                var data = await _source.DownloadBytes(bundlesPath);
                _ctx.CancellationToken.ThrowIfCancellationRequested();
                var downloadedBundle = await _cache
                    .BundleToCache(cachePath, data)
                    .AttachExternalCancellation(_ctx.CancellationToken);
                if (downloadedBundle == null)
                    throw new InvalidDataException($"Downloaded bundle '{bundlesPath}' is invalid.");
                _bundles[bundlesKey] = downloadedBundle;
                RegisterAssetNames(bundlesKey, downloadedBundle);
                _cache.PruneDirectory(bundlesKey, bundlesVersion);
            }
            _ctx.OnLog.Invoke(log);
            return _bundles[bundlesKey];
        }

        private string GetBundleKey(string bundleName)
        {
            return $"Remote/{GetPlatform()}/{bundleName}";
        }

        internal void ReleaseBundles(IEnumerable<string> bundleNames)
        {
            foreach (var bundleName in bundleNames)
            {
                var bundleKey = GetBundleKey(bundleName);
                if (_bundles.Remove(bundleKey, out var bundle) && bundle != null)
                    bundle.Unload(false);

                RemoveAssets(_sprites, bundleKey);
                RemoveAssets(_scriptableObjects, bundleKey);
                RemoveAssets(_prefabs, bundleKey);
                _assetNames.Remove(bundleKey);
            }

            _media.Clear();
        }

        private string GetAssetKey(string bundleName, string assetName)
        {
            return $"{GetBundleKey(bundleName)}|{assetName}";
        }

        public string ResolveAssetName(string bundleName, string requestedName)
        {
            if (string.IsNullOrWhiteSpace(requestedName))
                return null;
            var bundleKey = GetBundleKey(bundleName);
            if (_assetNames.TryGetValue(bundleKey, out var names)
                && names.TryGetValue(requestedName, out var actualName))
            {
                return actualName;
            }

            return null;
        }

        private void ReportMissingAsset(string bundleName, string requestedName)
        {
            _ctx.OnFailure?.Invoke(new BundleFailure(
                BundleFailureCodes.AssetNotFound,
                $"Asset '{requestedName}' is absent from bundle '{bundleName}'."));
        }

        private void RegisterAssetNames(string bundleKey, AssetBundle bundle)
        {
            var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var assetName in bundle.GetAllAssetNames())
                names[assetName] = assetName;
            _assetNames[bundleKey] = names;
        }

        private static void RemoveAssets<T>(
            IDictionary<string, T> assets,
            string bundleKey)
        {
            var prefix = $"{bundleKey}|";
            var keys = assets.Keys
                .Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            foreach (var key in keys)
                assets.Remove(key);
        }

        private async UniTask<string> GetBundleVersionAsync(string bundleName)
        {
            var path = $"Remote/{GetPlatform()}/{bundleName}/version.txt";
            var bundlesVersion = (await _source.DownloadText(path)).Trim();
            if (bundlesVersion.Length == 0)
                throw new InvalidDataException($"Bundle version is empty for '{bundleName}'.");
            return bundlesVersion;
        }

        public async UniTask<string> GetText(string path)
        {
            return await _source.DownloadText(path);
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

        private AssetBundle GetLoadedBundle(string bundleName)
        {
            var key = GetBundleKey(bundleName);
            if (!_bundles.TryGetValue(key, out var bundle) || bundle == null)
                throw new InvalidOperationException($"AssetBundle '{bundleName}' is not loaded.");
            return bundle;
        }

    }
}
