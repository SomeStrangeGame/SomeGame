using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bundles
{
    internal sealed class BundlePayloadLoader
    {
        private readonly Cache.Entity _cache;
        private readonly ContentPayloadMaterializer _materializer;
        private readonly string _platform;
        private readonly CancellationToken _cancellationToken;

        internal BundlePayloadLoader(
            Cache.Entity cache,
            ContentPayloadMaterializer materializer,
            string platform,
            CancellationToken cancellationToken)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _materializer = materializer
                ?? throw new ArgumentNullException(nameof(materializer));
            _platform = string.IsNullOrWhiteSpace(platform)
                ? throw new ArgumentException("Platform must not be empty.", nameof(platform))
                : platform;
            _cancellationToken = cancellationToken;
        }

        internal async UniTask<AssetBundle> Load(
            ContentReleaseSession session,
            string bundleName)
        {
            var descriptor = RequireDescriptor(session, bundleName);
            await Prepare(session, descriptor);
            return await Open(ContentStoragePlanner.BundlePath(
                session,
                _platform,
                descriptor));
        }

        internal async UniTask Prepare(
            ContentReleaseSession session,
            BundleReleaseDescriptor descriptor,
            Action<long> onDownloadedBytes = null)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));
            await _materializer.Materialize(
                GetPayload(session, descriptor),
                onDownloadedBytes);
        }

        internal ContentCachePayload GetCachePayload(
            ContentReleaseSession session,
            BundleReleaseDescriptor descriptor) =>
            GetPayload(session, descriptor).CachePayload;

        private ContentPayloadRequest GetPayload(
            ContentReleaseSession session,
            BundleReleaseDescriptor descriptor) =>
            new(
                descriptor.Name,
                $"Remote/{_platform}/{descriptor.Name}/{descriptor.Version}",
                ContentStoragePlanner.BundlePath(session, _platform, descriptor),
                descriptor.Size,
                descriptor.Sha256);

        private static BundleReleaseDescriptor RequireDescriptor(
            ContentReleaseSession session,
            string bundleName)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            return session.FindBundle(bundleName) ?? throw new ContentIntegrityException(
                $"Bundle '{bundleName}' is absent from release '{session.ReleaseId}'.");
        }

        private async UniTask<AssetBundle> Open(string cachePath)
        {
            var bundle = await AssetBundle.LoadFromFileAsync(
                    _cache.GetLocalPath(cachePath, false))
                .ToUniTask(cancellationToken: _cancellationToken);
            if (bundle == null)
            {
                throw new ContentIntegrityException(
                    $"Bundle payload '{cachePath}' is invalid.");
            }
            return bundle;
        }
    }
}
