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
            internal CancellationTokenSource Cancellation;
            internal int Subscribers;
            internal bool Completed;
            internal UniTask<string> Task;
            internal readonly Dictionary<int, ContentProgressReporter<long>> Progress = new();
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
        private int _nextSubscriberId;

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
            Action<long> onDownloadedBytes = null,
            CancellationToken cancellationToken = default)
        {
            var subscriberProgress = new ContentProgressReporter<long>(
                onDownloadedBytes,
                _onLog);
            Operation operation;
            int subscriberId;
            lock (_gate)
            {
                if (!_operations.TryGetValue(request.CachePath, out operation))
                {
                    operation = new Operation
                    {
                        Cancellation = CancellationTokenSource.CreateLinkedTokenSource(
                            _cancellationToken),
                    };
                    _operations.Add(request.CachePath, operation);
                    operation.Task = RunOperation(
                            operation,
                            request)
                        .Preserve();
                }
                subscriberId = ++_nextSubscriberId;
                operation.Progress.Add(subscriberId, subscriberProgress);
                operation.Subscribers++;
            }

            try
            {
                return await operation.Task.AttachExternalCancellation(cancellationToken);
            }
            finally
            {
                ReleaseSubscriber(operation, subscriberId);
            }
        }

        private async UniTask<string> RunOperation(
            Operation operation,
            ContentPayloadRequest request)
        {
            try
            {
                return await MaterializeCore(
                    request,
                    bytes => ReportProgress(operation, bytes),
                    operation.Cancellation.Token);
            }
            finally
            {
                var disposeCancellation = false;
                lock (_gate)
                {
                    operation.Completed = true;
                    if (_operations.TryGetValue(request.CachePath, out var current)
                        && ReferenceEquals(current, operation))
                    {
                        _operations.Remove(request.CachePath);
                    }
                    disposeCancellation = operation.Subscribers == 0;
                }
                if (disposeCancellation)
                    operation.Cancellation.Dispose();
            }
        }

        private void ReleaseSubscriber(Operation operation, int subscriberId)
        {
            var cancel = false;
            var dispose = false;
            lock (_gate)
            {
                operation.Progress.Remove(subscriberId);
                operation.Subscribers--;
                if (operation.Subscribers == 0)
                {
                    cancel = !operation.Completed;
                    dispose = operation.Completed;
                }
            }
            if (cancel)
                operation.Cancellation.Cancel();
            if (dispose)
                operation.Cancellation.Dispose();
        }

        private async UniTask<string> MaterializeCore(
            ContentPayloadRequest request,
            Action<long> reportProgress,
            CancellationToken cancellationToken)
        {
            var localPath = _cache.GetLocalPath(request.CachePath, false);
            var downloaded = false;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _integrity.VerifyAsync(
                    request.Name,
                    request.Size,
                    request.Sha256,
                    localPath,
                    true);
                reportProgress(request.Size);
            }
            catch (OperationCanceledException)
                when (_cancellationToken.IsCancellationRequested
                    || cancellationToken.IsCancellationRequested)
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
                        bytes => reportProgress(Math.Min(bytes, request.Size)),
                        cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
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
                    reportProgress(request.Size);
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

        private void ReportProgress(Operation operation, long bytes)
        {
            ContentProgressReporter<long>[] reporters;
            lock (_gate)
                reporters = new List<ContentProgressReporter<long>>(
                    operation.Progress.Values).ToArray();
            foreach (var reporter in reporters)
                reporter.Report(bytes);
        }
    }
}
