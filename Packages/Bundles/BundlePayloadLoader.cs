using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bundles
{
    internal sealed class BundlePayloadLoader
    {
        private readonly IContentSource _source;
        private readonly Cache.Entity _cache;
        private readonly ContentIntegrityVerifier _integrity;
        private readonly ContentStoragePlanner _storage;
        private readonly string _platform;
        private readonly CancellationToken _cancellationToken;
        private readonly Action<(LogType type, string message)> _onLog;

        internal BundlePayloadLoader(
            IContentSource source,
            Cache.Entity cache,
            ContentIntegrityVerifier integrity,
            ContentStoragePlanner storage,
            string platform,
            CancellationToken cancellationToken,
            Action<(LogType type, string message)> onLog)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _integrity = integrity ?? throw new ArgumentNullException(nameof(integrity));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _platform = string.IsNullOrWhiteSpace(platform)
                ? throw new ArgumentException("Platform must not be empty.", nameof(platform))
                : platform;
            _cancellationToken = cancellationToken;
            _onLog = onLog;
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
            var sourcePath = $"Remote/{_platform}/{descriptor.Name}/{descriptor.Version}";
            var cachePath = ContentStoragePlanner.BundlePath(
                session,
                _platform,
                descriptor);
            var localPath = _cache.GetLocalPath(cachePath, false);
            var downloaded = false;
            try
            {
                await _integrity.VerifyAsync(
                    descriptor.Name,
                    descriptor.Size,
                    descriptor.Sha256,
                    localPath,
                    true);
                onDownloadedBytes?.Invoke(descriptor.Size);
                _onLog?.Invoke((LogType.Log, $"Get local bundle from {cachePath}"));
            }
            catch (OperationCanceledException)
                when (_cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _onLog?.Invoke((
                    LogType.Warning,
                    $"Download bundle '{descriptor.Name}' because its cache is invalid: "
                    + exception.Message));
                var temporaryPath = _cache.CreateTemporaryPath(cachePath);
                try
                {
                    await _source.DownloadFile(
                        sourcePath,
                        temporaryPath,
                        bytes => onDownloadedBytes?.Invoke(
                            Math.Min(bytes, descriptor.Size)));
                    _cancellationToken.ThrowIfCancellationRequested();
                    await _integrity.VerifyAsync(
                        descriptor.Name,
                        descriptor.Size,
                        descriptor.Sha256,
                        temporaryPath,
                        true);
                    _cache.CommitTemporaryFile(temporaryPath, cachePath);
                    _integrity.Trust(
                        localPath,
                        descriptor.Size,
                        descriptor.Sha256,
                        true);
                    downloaded = true;
                    onDownloadedBytes?.Invoke(descriptor.Size);
                }
                finally
                {
                    if (File.Exists(temporaryPath))
                        File.Delete(temporaryPath);
                }
            }
            _cache.Touch(cachePath);
            if (downloaded)
                _storage.SchedulePrune(cachePath);
        }

        internal ContentCachePayload GetCachePayload(
            ContentReleaseSession session,
            BundleReleaseDescriptor descriptor) =>
            new(
                ContentStoragePlanner.BundlePath(session, _platform, descriptor),
                descriptor.Size);

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
            var bundle = await _cache.BundleFromCache(cachePath)
                .AttachExternalCancellation(_cancellationToken);
            if (bundle == null)
            {
                throw new ContentIntegrityException(
                    $"Bundle payload '{cachePath}' is invalid.");
            }
            return bundle;
        }
    }
}
