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
        private readonly ContentIntegrityVerifier _integrity;
        private readonly ContentStoragePlanner _storage;
        private readonly CancellationToken _cancellationToken;

        internal ContentFileStore(
            IContentSource source,
            Cache.Entity cache,
            ContentIntegrityVerifier integrity,
            ContentStoragePlanner storage,
            CancellationToken cancellationToken)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _integrity = integrity ?? throw new ArgumentNullException(nameof(integrity));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _cancellationToken = cancellationToken;
        }

        internal async UniTask<string> ResolveUrl(
            ContentReleaseSession session,
            string path,
            Action<long> onDownloadedBytes = null)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (string.IsNullOrWhiteSpace(path))
                return null;
            var descriptor = session.FindFile(path) ?? throw new ContentIntegrityException(
                $"File '{path}' is absent from release '{session.ReleaseId}'.");
            var cachePath = ContentStoragePlanner.FilePath(session, path);
            var localPath = _cache.GetLocalPath(cachePath, false);
            var downloaded = false;
            try
            {
                await _integrity.VerifyAsync(
                    path,
                    descriptor.Size,
                    descriptor.Sha256,
                    localPath,
                    true);
                onDownloadedBytes?.Invoke(descriptor.Size);
            }
            catch (OperationCanceledException)
                when (_cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                var temporaryPath = _cache.CreateTemporaryPath(cachePath);
                try
                {
                    await _source.DownloadFile(
                        path,
                        temporaryPath,
                        bytes => onDownloadedBytes?.Invoke(
                            Math.Min(bytes, descriptor.Size)));
                    _cancellationToken.ThrowIfCancellationRequested();
                    await _integrity.VerifyAsync(
                        path,
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
            return new Uri(localPath).AbsoluteUri;
        }

        internal async UniTask<string> GetText(
            ContentReleaseSession session,
            string path)
        {
            var url = await ResolveUrl(session, path);
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
    }
}
