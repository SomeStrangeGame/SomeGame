using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Collections.Generic;
using System.Linq;
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
        private sealed class Pin
        {
            internal Pin(long size)
            {
                Size = size;
                Count = 1;
            }

            internal long Size { get; }
            internal int Count { get; set; }
        }

        private readonly Dictionary<string, Pin> _pinnedFiles = new(
            StringComparer.OrdinalIgnoreCase);
        private readonly object _pinnedFilesGate = new();
        private readonly object _pruneGate = new();

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

        internal async UniTask<string> ResolveUrl(
            string path,
            Action<long> onDownloadedBytes = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;
            var release = _releases.Current ?? throw new ContentConfigurationException(
                "Content release must be loaded before external files.");
            var descriptor = release.FindFile(path) ?? throw new ContentIntegrityException(
                $"File '{path}' is absent from release '{release.ReleaseId}'.");
            var releaseId = release.ReleaseId;
            var cachePath = CachePath(releaseId, path);
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
            catch (Exception)
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
                await PruneAsync(cachePath);
            return new Uri(localPath).AbsoluteUri;
        }

        internal ContentDeliveryLease ReserveGroup(
            IReadOnlyCollection<ContentFileDescriptor> files,
            long additionalMissingBytes = 0)
        {
            var release = _releases.Current ?? throw new ContentConfigurationException(
                "Content release must be loaded before delivery reservation.");
            var missingBytes = Math.Max(0L, additionalMissingBytes);
            foreach (var file in files)
            {
                var cachePath = CachePath(release.ReleaseId, file.Path);
                var localPath = _cache.GetLocalPath(cachePath, false);
                if (!File.Exists(localPath) || new FileInfo(localPath).Length != file.Size)
                    missingBytes += file.Size;
            }
            var available = _cache.GetAvailableFreeSpace();
            if (available.HasValue && available.Value < missingBytes)
            {
                throw new ContentStorageException(
                    $"Content requires {missingBytes} free bytes, but only "
                    + $"{available.Value} bytes are available.");
            }
            lock (_pinnedFilesGate)
            {
                foreach (var file in files)
                {
                    var path = CachePath(release.ReleaseId, file.Path);
                    if (_pinnedFiles.TryGetValue(path, out var pin))
                        pin.Count++;
                    else
                        _pinnedFiles[path] = new Pin(file.Size);
                }
            }
            var releaseId = release.ReleaseId;
            return new ContentDeliveryLease(() => ReleaseGroupReservation(
                releaseId,
                files));
        }

        private void ReleaseGroupReservation(
            string releaseId,
            IReadOnlyCollection<ContentFileDescriptor> files)
        {
            lock (_pinnedFilesGate)
            {
                foreach (var file in files)
                {
                    var path = CachePath(releaseId, file.Path);
                    if (!_pinnedFiles.TryGetValue(path, out var pin))
                        continue;
                    pin.Count--;
                    if (pin.Count <= 0)
                        _pinnedFiles.Remove(path);
                }
            }
            PruneAsync(null).Forget();
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
            string[] protectedPaths;
            long pinnedBytes;
            lock (_pinnedFilesGate)
            {
                protectedPaths = string.IsNullOrEmpty(protectedPath)
                    ? _pinnedFiles.Keys.ToArray()
                    : _pinnedFiles.Keys.Append(protectedPath).ToArray();
                pinnedBytes = _pinnedFiles.Values.Sum(value => value.Size);
            }
            await UniTask.SwitchToThreadPool();
            try
            {
                lock (_pruneGate)
                {
                    _cache.PruneBySize(
                        "RemoteFiles",
                        Math.Max(_cacheLimit, pinnedBytes),
                        protectedPaths);
                }
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

        private static string CachePath(string releaseId, string path) =>
            $"RemoteFiles/{releaseId}/{path}";
    }
}
