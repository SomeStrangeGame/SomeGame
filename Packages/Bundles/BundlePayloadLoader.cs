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
        private readonly ContentReleaseProvider _releases;
        private readonly ContentIntegrityVerifier _integrity;
        private readonly CancellationToken _cancellationToken;
        private readonly Action<(LogType type, string message)> _onLog;

        internal BundlePayloadLoader(
            IContentSource source,
            Cache.Entity cache,
            ContentReleaseProvider releases,
            ContentIntegrityVerifier integrity,
            CancellationToken cancellationToken,
            Action<(LogType type, string message)> onLog)
        {
            _source = source;
            _cache = cache;
            _releases = releases;
            _integrity = integrity;
            _cancellationToken = cancellationToken;
            _onLog = onLog;
        }

        internal async UniTask<AssetBundle> Load(string bundleName, string bundleKey)
        {
            await Prepare(bundleName, bundleKey);
            var descriptor = (_releases.Current ?? throw new ContentConfigurationException(
                    "Content release must be loaded before AssetBundles."))
                .FindBundle(bundleName) ?? throw new ContentIntegrityException(
                    $"Bundle '{bundleName}' is absent from the active release.");
            return await Open($"{bundleKey}/{descriptor.Version}");
        }

        internal async UniTask Prepare(
            string bundleName,
            string bundleKey,
            Action<long> onDownloadedBytes = null)
        {
            var release = _releases.Current ?? throw new ContentConfigurationException(
                "Content release must be loaded before AssetBundles.");
            var descriptor = release.FindBundle(bundleName)
                ?? throw new ContentIntegrityException(
                    $"Bundle '{bundleName}' is absent from release '{release.ReleaseId}'.");
            var sourcePath = $"{bundleKey}/{descriptor.Version}";
            var localPath = _cache.GetLocalPath(sourcePath, false);
            try
            {
                await _integrity.VerifyAsync(
                    bundleName,
                    descriptor.Size,
                    descriptor.Sha256,
                    localPath,
                    true);
                onDownloadedBytes?.Invoke(descriptor.Size);
                _onLog?.Invoke((LogType.Log, $"Get local bundle from {sourcePath}"));
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
                    $"Download bundle '{bundleName}' because its cache is invalid: "
                    + exception.Message));
                var temporaryPath = _cache.CreateTemporaryPath(sourcePath);
                try
                {
                    await _source.DownloadFile(
                        sourcePath,
                        temporaryPath,
                        bytes => onDownloadedBytes?.Invoke(
                            Math.Min(bytes, descriptor.Size)));
                    _cancellationToken.ThrowIfCancellationRequested();
                    await _integrity.VerifyAsync(
                        bundleName,
                        descriptor.Size,
                        descriptor.Sha256,
                        temporaryPath,
                        true);
                    _cache.CommitTemporaryFile(temporaryPath, sourcePath);
                    _integrity.Trust(
                        localPath,
                        descriptor.Size,
                        descriptor.Sha256,
                        true);
                    onDownloadedBytes?.Invoke(descriptor.Size);
                }
                finally
                {
                    if (File.Exists(temporaryPath))
                        File.Delete(temporaryPath);
                }
            }

            _cache.PruneDirectory(bundleKey, descriptor.Version);
        }

        internal long GetMissingBytes(
            BundleReleaseDescriptor descriptor,
            string bundleKey)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));
            var localPath = _cache.GetLocalPath(
                $"{bundleKey}/{descriptor.Version}",
                false);
            return !File.Exists(localPath)
                || new FileInfo(localPath).Length != descriptor.Size
                    ? descriptor.Size
                    : 0L;
        }

        private async UniTask<AssetBundle> Open(string sourcePath)
        {
            var bundle = await _cache.BundleFromCache(sourcePath)
                .AttachExternalCancellation(_cancellationToken);
            if (bundle == null)
            {
                throw new ContentIntegrityException(
                    $"Bundle payload '{sourcePath}' is invalid.");
            }
            return bundle;
        }
    }
}
