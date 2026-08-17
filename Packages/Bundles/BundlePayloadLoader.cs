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
            var release = _releases.Current ?? throw new ContentConfigurationException(
                "Content release must be loaded before AssetBundles.");
            var descriptor = release.FindBundle(bundleName)
                ?? throw new ContentIntegrityException(
                    $"Bundle '{bundleName}' is absent from release '{release.ReleaseId}'.");
            var sourcePath = $"{bundleKey}/{descriptor.Version}";
            var localPath = _cache.GetLocalPath(sourcePath, false);
            AssetBundle bundle;
            try
            {
                await _integrity.VerifyAsync(
                    bundleName,
                    descriptor.Size,
                    descriptor.Sha256,
                    localPath,
                    true);
                bundle = await Open(sourcePath);
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
                    await _source.DownloadFile(sourcePath, temporaryPath);
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
                }
                finally
                {
                    if (File.Exists(temporaryPath))
                        File.Delete(temporaryPath);
                }
                bundle = await Open(sourcePath);
            }

            _cache.PruneDirectory(bundleKey, descriptor.Version);
            return bundle;
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
