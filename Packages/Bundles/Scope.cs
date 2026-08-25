using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Bundles
{
    public class Scope : BaseDisposable
    {
        private readonly Entity _owner;
        private readonly ContentReleaseSession _session;
        private readonly CancellationToken _cancellationToken;
        private readonly HashSet<string> _bundleNames = new(
            StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, UniTask<AssetBundle>> _bundleLoads = new(
            StringComparer.OrdinalIgnoreCase);

        internal Scope(
            Entity owner,
            ContentReleaseSession session,
            CancellationToken cancellationToken)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _cancellationToken = cancellationToken;
        }

        public async UniTask<AssetBundle> GetAssetBundle(string bundleName)
        {
            EnsureActive();
            if (_bundleNames.Contains(bundleName))
                return _owner.GetOwnedAssetBundle(_session, bundleName);
            if (!_bundleLoads.TryGetValue(bundleName, out var loading))
            {
                loading = Acquire(bundleName).Preserve();
                _bundleLoads.Add(bundleName, loading);
            }
            return await loading;
        }

        public void ReleaseAssetBundle(string bundleName)
        {
            EnsureActive();
            if (!_bundleNames.Remove(bundleName))
                return;
            _owner.ReleaseBundles(_session, new[] { bundleName });
        }

        public UniTask<Sprite> GetBundledSprite(
            string bundleName,
            string assetName)
        {
            return GetBundledSprite(new BundleAssetAddress(bundleName, assetName));
        }

        public UniTask<Sprite> GetBundledSprite(BundleAssetAddress address)
        {
            EnsureOwned(address.BundleName);
            return _owner.GetBundledSprite(_session, address);
        }

        public UniTask<Sprite> TryGetBundledSprite(
            string bundleName,
            string assetName)
        {
            return TryGetBundledSprite(new BundleAssetAddress(bundleName, assetName));
        }

        public UniTask<Sprite> TryGetBundledSprite(BundleAssetAddress address)
        {
            EnsureOwned(address.BundleName);
            return _owner.TryGetBundledSprite(_session, address);
        }

        public UniTask<T> GetBundledSO<T>(string bundleName, string assetName)
            where T : ScriptableObject
        {
            return GetBundledSO<T>(new BundleAssetAddress(bundleName, assetName));
        }

        public UniTask<T> GetBundledSO<T>(BundleAssetAddress address)
            where T : ScriptableObject
        {
            EnsureOwned(address.BundleName);
            return _owner.GetBundledSO<T>(_session, address);
        }

        public UniTask<GameObject> GetBundledPrefab(
            string bundleName,
            string assetName)
        {
            return GetBundledPrefab(new BundleAssetAddress(bundleName, assetName));
        }

        public UniTask<GameObject> GetBundledPrefab(BundleAssetAddress address)
        {
            EnsureOwned(address.BundleName);
            return _owner.GetBundledPrefab(_session, address);
        }

        public UniTask<GameObject> TryGetBundledPrefab(BundleAssetAddress address)
        {
            EnsureOwned(address.BundleName);
            return _owner.TryGetBundledPrefab(_session, address);
        }

        public string ResolveAssetName(string bundleName, string requestedName)
        {
            EnsureOwned(bundleName);
            return _owner.ResolveAssetName(_session, bundleName, requestedName);
        }

        protected override void OnDispose()
        {
            _owner.ReleaseBundles(_session, _bundleNames);
            _bundleNames.Clear();
            _bundleLoads.Clear();
            base.OnDispose();
        }

        private async UniTask<AssetBundle> Acquire(string bundleName)
        {
            try
            {
                var bundle = await _owner.AcquireAssetBundle(_session, bundleName);
                if (IsDisposed || _cancellationToken.IsCancellationRequested)
                {
                    _owner.ReleaseBundles(_session, new[] { bundleName });
                    _cancellationToken.ThrowIfCancellationRequested();
                    throw new ObjectDisposedException(nameof(Scope));
                }
                _bundleNames.Add(bundleName);
                return bundle;
            }
            finally
            {
                _bundleLoads.Remove(bundleName);
            }
        }

        private void EnsureOwned(string bundleName)
        {
            EnsureActive();
            if (!_bundleNames.Contains(bundleName))
            {
                throw new InvalidOperationException(
                    $"AssetBundle '{bundleName}' is not loaded by this scope.");
            }
        }

        protected void EnsureActive()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(Scope));
            _cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
