using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Bundles
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public IContentSource ContentSource;
            public string PersistentDataPath;
            public string Platform;
            public long ContentFileCacheLimit;
            public CancellationToken CancellationToken;
            public Action<(LogType type, string message)> OnLog;
        }

        private const long _defaultContentFileCacheLimit =
            512L * 1024L * 1024L;

        private readonly ContentReleaseProvider _releases;
        private readonly ContentFileStore _contentFiles;
        private readonly ContentDeliveryCoordinator _delivery;
        private readonly BundleStore _bundles;

        public Entity(Ctx ctx)
        {
            var source = ctx.ContentSource
                ?? throw new ArgumentNullException(nameof(ctx.ContentSource));
            var cache = new Cache.Entity(ctx.PersistentDataPath).AddTo(this);
            var platform = string.IsNullOrWhiteSpace(ctx.Platform)
                ? ContentPlatform.GetCurrent()
                : ctx.Platform;
            var integrity = new ContentIntegrityVerifier(ctx.CancellationToken);
            _releases = new ContentReleaseProvider(
                source,
                cache,
                platform,
                ctx.CancellationToken,
                ctx.OnLog);
            _contentFiles = new ContentFileStore(
                source,
                cache,
                _releases,
                integrity,
                ctx.ContentFileCacheLimit > 0
                    ? ctx.ContentFileCacheLimit
                    : _defaultContentFileCacheLimit,
                ctx.CancellationToken,
                ctx.OnLog);
            var payloads = new BundlePayloadLoader(
                source,
                cache,
                _releases,
                integrity,
                ctx.CancellationToken,
                ctx.OnLog);
            _delivery = new ContentDeliveryCoordinator(
                _releases,
                _contentFiles,
                payloads,
                platform);
            _bundles = new BundleStore(payloads, platform, ctx.CancellationToken);
        }

        public Scope CreateScope() => new(this, _releases.CancellationToken);

        public Scope CreateScope(CancellationToken cancellationToken) =>
            new(this, cancellationToken);

        public UniTask<ContentReleaseSnapshot> LoadReleaseAsync(
            string clientVersion,
            int minimumSupportedSchemaVersion,
            int maximumSupportedSchemaVersion) =>
            _releases.LoadAsync(
                clientVersion,
                minimumSupportedSchemaVersion,
                maximumSupportedSchemaVersion);

        public ContentDeliveryMode DeliveryMode =>
            (_releases.Current ?? throw new ContentConfigurationException(
                "Content release is not loaded.")).DeliveryMode;

        public string ReleaseId =>
            (_releases.Current ?? throw new ContentConfigurationException(
                "Content release is not loaded.")).ReleaseId;

        public void ActivateRelease() => _releases.ActivateCurrent();

        public bool HasDeliveryGroup(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return false;
            var release = _releases.Current ?? throw new ContentConfigurationException(
                "Content release is not loaded.");
            return System.Linq.Enumerable.Any(
                release.DeliveryGroups,
                group => string.Equals(
                    group.Id,
                    groupId,
                    StringComparison.OrdinalIgnoreCase));
        }

        public UniTask<ContentDeliveryLease> PrepareDeliveryGroup(
            string groupId,
            Action<ContentDeliveryProgress> onProgress,
            CancellationToken cancellationToken) =>
            _delivery.Prepare(groupId, onProgress, cancellationToken);

        public UniTask<AssetBundle> GetAssetBundle(string bundleName) =>
            _bundles.GetPersistent(bundleName);

        public UniTask<Sprite> GetBundledSprite(
            string bundleName,
            string assetName) =>
            GetBundledSprite(new BundleAssetAddress(bundleName, assetName));

        public UniTask<Sprite> GetBundledSprite(BundleAssetAddress address) =>
            _bundles.GetSprite(address.BundleName, address.AssetName);

        public UniTask<Sprite> TryGetBundledSprite(
            string bundleName,
            string assetName) =>
            TryGetBundledSprite(new BundleAssetAddress(bundleName, assetName));

        public UniTask<Sprite> TryGetBundledSprite(BundleAssetAddress address) =>
            _bundles.TryGetSprite(address.BundleName, address.AssetName);

        public UniTask<T> GetBundledSO<T>(string bundleName, string assetName)
            where T : ScriptableObject =>
            GetBundledSO<T>(new BundleAssetAddress(bundleName, assetName));

        public UniTask<T> GetBundledSO<T>(BundleAssetAddress address)
            where T : ScriptableObject =>
            _bundles.GetScriptableObject<T>(address.BundleName, address.AssetName);

        public UniTask<GameObject> GetBundledPrefab(
            string bundleName,
            string assetName) =>
            GetBundledPrefab(new BundleAssetAddress(bundleName, assetName));

        public UniTask<GameObject> GetBundledPrefab(BundleAssetAddress address) =>
            _bundles.GetPrefab(address.BundleName, address.AssetName);

        public string ResolveAssetName(string bundleName, string requestedName) =>
            _bundles.ResolveAssetName(bundleName, requestedName);

        public UniTask<string> GetText(string path) => _contentFiles.GetText(path);

        internal UniTask<AssetBundle> AcquireAssetBundle(string bundleName) =>
            _bundles.Acquire(bundleName);

        internal AssetBundle GetOwnedAssetBundle(string bundleName) =>
            _bundles.GetOwned(bundleName);

        internal void ReleaseBundles(System.Collections.Generic.IEnumerable<string> names) =>
            _bundles.Release(names);

        internal MediaResolver CreateMediaResolver(
            string prefix,
            MediaManifest manifest) =>
            new(prefix, manifest, path => _contentFiles.ResolveUrl(path));

        protected override void OnDispose()
        {
            _bundles.Clear();
            base.OnDispose();
        }
    }
}
