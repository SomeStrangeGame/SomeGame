using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bundles
{
    internal readonly struct ContentPayloadRequest
    {
        internal ContentPayloadRequest(
            string name,
            string sourcePath,
            string cachePath,
            long size,
            string sha256)
        {
            Name = string.IsNullOrWhiteSpace(name)
                ? throw new ArgumentException("Payload name must not be empty.", nameof(name))
                : name;
            SourcePath = string.IsNullOrWhiteSpace(sourcePath)
                ? throw new ArgumentException("Source path must not be empty.", nameof(sourcePath))
                : sourcePath;
            CachePath = string.IsNullOrWhiteSpace(cachePath)
                ? throw new ArgumentException("Cache path must not be empty.", nameof(cachePath))
                : cachePath;
            Size = size >= 0 ? size : throw new ArgumentOutOfRangeException(nameof(size));
            Sha256 = sha256;
        }

        internal string Name { get; }
        internal string SourcePath { get; }
        internal string CachePath { get; }
        internal long Size { get; }
        internal string Sha256 { get; }
        internal ContentCachePayload CachePayload => new(CachePath, Size);
    }

    internal sealed class ContentPayloadMaterializer
    {
        private const string _stagingRoot = "ContentStaging";

        private sealed class Operation
        {
            internal UniTask<string> Task;
        }

        private readonly IContentSource _source;
        private readonly Cache.Entity _cache;
        private readonly ContentIntegrityVerifier _integrity;
        private readonly ContentStoragePlanner _storage;
        private readonly CancellationToken _cancellationToken;
        private readonly Action<(LogType type, string message)> _onLog;
        private readonly Dictionary<string, Operation> _operations = new(
            StringComparer.OrdinalIgnoreCase);
        private readonly object _gate = new();

        internal ContentPayloadMaterializer(
            IContentSource source,
            Cache.Entity cache,
            ContentIntegrityVerifier integrity,
            ContentStoragePlanner storage,
            CancellationToken cancellationToken,
            Action<(LogType type, string message)> onLog)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _integrity = integrity ?? throw new ArgumentNullException(nameof(integrity));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _cancellationToken = cancellationToken;
            _onLog = onLog;
        }

        internal async UniTask<string> Materialize(
            ContentPayloadRequest request,
            Action<long> onDownloadedBytes = null)
        {
            var progress = new ContentProgressReporter<long>(
                onDownloadedBytes,
                _onLog);
            Operation operation;
            var ownsOperation = false;
            lock (_gate)
            {
                if (!_operations.TryGetValue(request.CachePath, out operation))
                {
                    operation = new Operation();
                    operation.Task = MaterializeCore(request, progress).Preserve();
                    _operations.Add(request.CachePath, operation);
                    ownsOperation = true;
                }
            }

            try
            {
                var path = await operation.Task;
                if (!ownsOperation)
                    progress.Report(request.Size);
                return path;
            }
            finally
            {
                lock (_gate)
                {
                    if (_operations.TryGetValue(request.CachePath, out var current)
                        && ReferenceEquals(current, operation))
                    {
                        _operations.Remove(request.CachePath);
                    }
                }
            }
        }

        private async UniTask<string> MaterializeCore(
            ContentPayloadRequest request,
            ContentProgressReporter<long> progress)
        {
            var localPath = _cache.GetLocalPath(request.CachePath, false);
            var downloaded = false;
            try
            {
                await _integrity.VerifyAsync(
                    request.Name,
                    request.Size,
                    request.Sha256,
                    localPath,
                    true);
                progress.Report(request.Size);
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
                    $"Download content '{request.Name}' because its cache is invalid: "
                    + exception.Message));
                _cache.Delete(request.CachePath);
                var temporaryPath = _cache.CreateTemporaryFile(_stagingRoot);
                try
                {
                    await _source.DownloadFile(
                        request.SourcePath,
                        temporaryPath,
                        bytes => progress.Report(Math.Min(bytes, request.Size)));
                    _cancellationToken.ThrowIfCancellationRequested();
                    await _integrity.VerifyAsync(
                        request.Name,
                        request.Size,
                        request.Sha256,
                        temporaryPath,
                        true);
                    _cache.CommitTemporaryFile(temporaryPath, request.CachePath);
                    _integrity.Trust(
                        localPath,
                        request.Size,
                        request.Sha256,
                        true);
                    downloaded = true;
                    progress.Report(request.Size);
                }
                finally
                {
                    if (File.Exists(temporaryPath))
                        File.Delete(temporaryPath);
                }
            }

            _cache.Touch(request.CachePath);
            if (downloaded)
                _storage.SchedulePrune(request.CachePath);
            return localPath;
        }
    }
}
