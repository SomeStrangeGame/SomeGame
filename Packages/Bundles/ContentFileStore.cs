using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bundles
{
    internal sealed class ContentFileStore
    {
        private readonly IContentSource _source;
        private readonly Cache.Entity _cache;
        private readonly ContentReleaseProvider _releases;
        private readonly ContentIntegrityVerifier _integrity;
        private readonly long _cacheLimit;
        private readonly CancellationToken _cancellationToken;
        private readonly Action<(LogType type, string message)> _onLog;

        internal ContentFileStore(
            IContentSource source,
            Cache.Entity cache,
            ContentReleaseProvider releases,
            ContentIntegrityVerifier integrity,
            long cacheLimit,
            CancellationToken cancellationToken,
            Action<(LogType type, string message)> onLog)
        {
            _source = source;
            _cache = cache;
            _releases = releases;
            _integrity = integrity;
            _cacheLimit = cacheLimit;
            _cancellationToken = cancellationToken;
            _onLog = onLog;
        }

        internal async UniTask<string> ResolveUrl(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;
            var release = _releases.Current;
            var descriptor = release?.FindFile(path);
            if (release != null && descriptor == null)
            {
                throw new ContentIntegrityException(
                    $"File '{path}' is absent from release '{release.ReleaseId}'.");
            }
            var releaseId = release?.ReleaseId ?? "legacy";
            var cachePath = $"RemoteFiles/{releaseId}/{path}";
            var localPath = _cache.GetLocalPath(cachePath, false);
            var verify = descriptor != null;
            var downloaded = false;
            try
            {
                await _integrity.VerifyAsync(
                    path,
                    descriptor?.Size ?? 0,
                    descriptor?.Sha256,
                    localPath,
                    verify);
            }
            catch (OperationCanceledException)
                when (_cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                var temporaryPath = _cache.CreateTemporaryPath(cachePath);
                try
                {
                    await _source.DownloadFile(path, temporaryPath);
                    _cancellationToken.ThrowIfCancellationRequested();
                    await _integrity.VerifyAsync(
                        path,
                        descriptor?.Size ?? 0,
                        descriptor?.Sha256,
                        temporaryPath,
                        verify);
                    _cache.CommitTemporaryFile(temporaryPath, cachePath);
                    _integrity.Trust(
                        localPath,
                        descriptor?.Size ?? 0,
                        descriptor?.Sha256,
                        verify);
                    downloaded = true;
                }
                finally
                {
                    if (File.Exists(temporaryPath))
                        File.Delete(temporaryPath);
                }
            }

            _cache.Touch(cachePath);
            if (downloaded)
                await PruneAsync(cachePath);
            return new Uri(localPath).AbsoluteUri;
        }

        internal async UniTask<string> GetText(string path)
        {
            var url = await ResolveUrl(path);
            var localPath = new Uri(url).LocalPath;
            string text = null;
            Exception failure = null;
            await UniTask.SwitchToThreadPool();
            try
            {
                text = File.ReadAllText(localPath);
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            await UniTask.SwitchToMainThread();
            if (failure != null)
                ExceptionDispatchInfo.Capture(failure).Throw();
            _cancellationToken.ThrowIfCancellationRequested();
            return text;
        }

        private async UniTask PruneAsync(string protectedPath)
        {
            Exception failure = null;
            await UniTask.SwitchToThreadPool();
            try
            {
                _cache.PruneBySize("RemoteFiles", _cacheLimit, protectedPath);
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            await UniTask.SwitchToMainThread();
            if (failure != null)
            {
                _onLog?.Invoke((
                    LogType.Warning,
                    $"Content cache pruning failed: {failure.Message}"));
            }
        }
    }
}
