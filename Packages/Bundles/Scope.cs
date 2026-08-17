using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Bundles
{
    public sealed class Scope : BaseDisposable
    {
        private readonly Entity _owner;
        private readonly HashSet<string> _bundleNames = new(
            StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, UniTask<AssetBundle>> _bundleLoads = new(
            StringComparer.OrdinalIgnoreCase);
        private MediaResolver _media;

        internal Scope(Entity owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public async UniTask<AssetBundle> GetAssetBundle(string bundleName)
        {
            if (_bundleNames.Contains(bundleName))
                return _owner.GetOwnedAssetBundle(bundleName);
            if (!_bundleLoads.TryGetValue(bundleName, out var loading))
            {
                loading = Acquire(bundleName).Preserve();
                _bundleLoads.Add(bundleName, loading);
            }
            return await loading;
        }

        public void ConfigureMedia(string prefix, MediaManifest manifest)
        {
            _media = _owner.CreateMediaResolver(prefix, manifest);
        }

        public UniTask<Sprite> GetBundledSprite(
            string bundleName,
            string assetName)
        {
            EnsureOwned(bundleName);
            return _owner.GetBundledSprite(bundleName, assetName);
        }

        public UniTask<Sprite> TryGetBundledSprite(
            string bundleName,
            string assetName)
        {
            EnsureOwned(bundleName);
            return _owner.TryGetBundledSprite(bundleName, assetName);
        }

        public UniTask<T> GetBundledSO<T>(string bundleName, string assetName)
            where T : ScriptableObject
        {
            EnsureOwned(bundleName);
            return _owner.GetBundledSO<T>(bundleName, assetName);
        }

        public UniTask<GameObject> GetBundledPrefab(
            string bundleName,
            string assetName)
        {
            EnsureOwned(bundleName);
            return _owner.GetBundledPrefab(bundleName, assetName);
        }

        public UniTask<string> ResolveVideoUrl(string assetName) =>
            RequireMedia().ResolveVideoUrl(assetName);

        public UniTask<string> ResolveAudioUrl(string assetName) =>
            RequireMedia().ResolveAudioUrl(assetName);

        public string ResolveAssetName(string bundleName, string requestedName)
        {
            EnsureOwned(bundleName);
            return _owner.ResolveAssetName(bundleName, requestedName);
        }

        protected override void OnDispose()
        {
            _owner.ReleaseBundles(_bundleNames);
            _bundleNames.Clear();
            _bundleLoads.Clear();
            _media = null;
            base.OnDispose();
        }

        private async UniTask<AssetBundle> Acquire(string bundleName)
        {
            try
            {
                var bundle = await _owner.AcquireAssetBundle(bundleName);
                _bundleNames.Add(bundleName);
                return bundle;
            }
            finally
            {
                _bundleLoads.Remove(bundleName);
            }
        }

        private MediaResolver RequireMedia()
        {
            return _media ?? throw new InvalidOperationException(
                "Media resolver is not configured for this scope.");
        }

        private void EnsureOwned(string bundleName)
        {
            if (!_bundleNames.Contains(bundleName))
            {
                throw new InvalidOperationException(
                    $"AssetBundle '{bundleName}' is not loaded by this scope.");
            }
        }
    }
}
