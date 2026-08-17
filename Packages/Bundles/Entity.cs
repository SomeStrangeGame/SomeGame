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
            public long ContentFileCacheLimit;
            public CancellationToken CancellationToken;
            public Action<(LogType type, string message)> OnLog;
        }

        private const long _defaultContentFileCacheLimit =
            512L * 1024L * 1024L;

        private readonly ContentReleaseProvider _releases;
        private readonly ContentFileStore _contentFiles;
        private readonly BundleStore _bundles;

        public Entity(Ctx ctx)
        {
            var source = ctx.ContentSource
                ?? throw new ArgumentNullException(nameof(ctx.ContentSource));
            var cache = new Cache.Entity(ctx.PersistentDataPath).AddTo(this);
            var platform = ContentPlatform.GetCurrent();
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
            _bundles = new BundleStore(
                source,
                cache,
                _releases,
                integrity,
                platform,
                ctx.CancellationToken,
                ctx.OnLog);
        }

        public Scope CreateScope() => new(this);

        public UniTask<ContentRelease> LoadReleaseAsync(
            string clientVersion,
            int supportedSchemaVersion) =>
            _releases.LoadAsync(clientVersion, supportedSchemaVersion);

        public UniTask<AssetBundle> GetAssetBundle(string bundleName) =>
            _bundles.GetPersistent(bundleName);

        public UniTask<Sprite> GetBundledSprite(
            string bundleName,
            string assetName) =>
            _bundles.GetSprite(bundleName, assetName);

        public UniTask<Sprite> TryGetBundledSprite(
            string bundleName,
            string assetName) =>
            _bundles.TryGetSprite(bundleName, assetName);

        public UniTask<T> GetBundledSO<T>(string bundleName, string assetName)
            where T : ScriptableObject =>
            _bundles.GetScriptableObject<T>(bundleName, assetName);

        public UniTask<GameObject> GetBundledPrefab(
            string bundleName,
            string assetName) =>
            _bundles.GetPrefab(bundleName, assetName);

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
            new(prefix, manifest, _contentFiles.ResolveUrl);

        protected override void OnDispose()
        {
            _bundles.Clear();
            base.OnDispose();
        }
    }
}
