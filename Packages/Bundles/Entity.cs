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
            var storage = new ContentStoragePlanner(
                cache,
                ctx.ContentFileCacheLimit > 0
                    ? ctx.ContentFileCacheLimit
                    : _defaultContentFileCacheLimit,
                ctx.CancellationToken,
                ctx.OnLog);
            _releases = new ContentReleaseProvider(
                source,
                cache,
                platform,
                ctx.CancellationToken,
                ctx.OnLog);
            _contentFiles = new ContentFileStore(
                source,
                cache,
                integrity,
                storage,
                ctx.CancellationToken);
            var payloads = new BundlePayloadLoader(
                source,
                cache,
                integrity,
                storage,
                platform,
                ctx.CancellationToken,
                ctx.OnLog);
            _delivery = new ContentDeliveryCoordinator(
                _contentFiles,
                payloads,
                storage);
            _bundles = new BundleStore(payloads, ctx.CancellationToken);
        }

        public Scope CreateScope() => new(
            this,
            RequireSession(),
            _releases.CancellationToken);

        public Scope CreateScope(CancellationToken cancellationToken) =>
            new(this, RequireSession(), cancellationToken);

        public UniTask<ContentReleaseSnapshot> LoadReleaseAsync(
            string clientVersion,
            int minimumSupportedSchemaVersion,
            int maximumSupportedSchemaVersion) =>
            LoadRelease(
                clientVersion,
                minimumSupportedSchemaVersion,
                maximumSupportedSchemaVersion);

        public ContentDeliveryMode DeliveryMode =>
            RequireSession().DeliveryMode;

        public string ReleaseId =>
            RequireSession().ReleaseId;

        public void ActivateRelease() => _releases.ActivateCurrent();

        public bool HasDeliveryGroup(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return false;
            var release = RequireSession().Release;
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
            _delivery.Prepare(
                RequireSession(),
                groupId,
                onProgress,
                cancellationToken);

        public UniTask<AssetBundle> GetAssetBundle(string bundleName) =>
            _bundles.GetPersistent(RequireSession(), bundleName);

        public UniTask<Sprite> GetBundledSprite(
            string bundleName,
            string assetName) =>
            GetBundledSprite(new BundleAssetAddress(bundleName, assetName));

        public UniTask<Sprite> GetBundledSprite(BundleAssetAddress address) =>
            GetBundledSprite(RequireSession(), address);

        public UniTask<Sprite> TryGetBundledSprite(
            string bundleName,
            string assetName) =>
            TryGetBundledSprite(new BundleAssetAddress(bundleName, assetName));

        public UniTask<Sprite> TryGetBundledSprite(BundleAssetAddress address) =>
            TryGetBundledSprite(RequireSession(), address);

        public UniTask<T> GetBundledSO<T>(string bundleName, string assetName)
            where T : ScriptableObject =>
            GetBundledSO<T>(new BundleAssetAddress(bundleName, assetName));

        public UniTask<T> GetBundledSO<T>(BundleAssetAddress address)
            where T : ScriptableObject =>
            GetBundledSO<T>(RequireSession(), address);

        public UniTask<GameObject> GetBundledPrefab(
            string bundleName,
            string assetName) =>
            GetBundledPrefab(new BundleAssetAddress(bundleName, assetName));

        public UniTask<GameObject> GetBundledPrefab(BundleAssetAddress address) =>
            GetBundledPrefab(RequireSession(), address);

        public string ResolveAssetName(string bundleName, string requestedName) =>
            ResolveAssetName(RequireSession(), bundleName, requestedName);

        public UniTask<string> GetText(string path) =>
            _contentFiles.GetText(RequireSession(), path);

        internal UniTask<AssetBundle> AcquireAssetBundle(
            ContentReleaseSession session,
            string bundleName) =>
            _bundles.Acquire(session, bundleName);

        internal AssetBundle GetOwnedAssetBundle(
            ContentReleaseSession session,
            string bundleName) =>
            _bundles.GetOwned(session, bundleName);

        internal void ReleaseBundles(
            ContentReleaseSession session,
            System.Collections.Generic.IEnumerable<string> names) =>
            _bundles.Release(session, names);

        internal UniTask<Sprite> GetBundledSprite(
            ContentReleaseSession session,
            BundleAssetAddress address) =>
            _bundles.GetSprite(session, address.BundleName, address.AssetName);

        internal UniTask<Sprite> TryGetBundledSprite(
            ContentReleaseSession session,
            BundleAssetAddress address) =>
            _bundles.TryGetSprite(session, address.BundleName, address.AssetName);

        internal UniTask<T> GetBundledSO<T>(
            ContentReleaseSession session,
            BundleAssetAddress address)
            where T : ScriptableObject =>
            _bundles.GetScriptableObject<T>(
                session,
                address.BundleName,
                address.AssetName);

        internal UniTask<GameObject> GetBundledPrefab(
            ContentReleaseSession session,
            BundleAssetAddress address) =>
            _bundles.GetPrefab(session, address.BundleName, address.AssetName);

        internal string ResolveAssetName(
            ContentReleaseSession session,
            string bundleName,
            string requestedName) =>
            _bundles.ResolveAssetName(session, bundleName, requestedName);

        internal MediaResolver CreateMediaResolver(
            ContentReleaseSession session,
            string prefix,
            MediaManifest manifest) =>
            new(prefix, manifest, path => _contentFiles.ResolveUrl(session, path));

        private async UniTask<ContentReleaseSnapshot> LoadRelease(
            string clientVersion,
            int minimumSupportedSchemaVersion,
            int maximumSupportedSchemaVersion)
        {
            var previous = _releases.Current;
            var session = await _releases.LoadAsync(
                clientVersion,
                minimumSupportedSchemaVersion,
                maximumSupportedSchemaVersion);
            if (previous != null && !string.Equals(
                    previous.ReleaseId,
                    session.ReleaseId,
                    StringComparison.OrdinalIgnoreCase))
            {
                _bundles.Discard(previous);
            }
            return session.Release;
        }

        private ContentReleaseSession RequireSession() =>
            _releases.Current ?? throw new ContentConfigurationException(
                "Content release is not loaded.");

        protected override void OnDispose()
        {
            _bundles.Clear();
            base.OnDispose();
        }
    }
}
