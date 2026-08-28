using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bundles
{
    internal sealed class ContentDeliveryCoordinator
    {
        private sealed class Operation
        {
            internal UniTask Task;
            internal readonly Dictionary<int, ContentProgressReporter<ContentDeliveryProgress>>
                Progress = new();
        }

        private readonly ContentFileStore _files;
        private readonly BundlePayloadLoader _bundles;
        private readonly ContentStoragePlanner _storage;
        private readonly int _maximumParallelDownloads;
        private readonly CancellationToken _cancellationToken;
        private readonly SemaphoreSlim _downloadSlots;
        private readonly Action<(UnityEngine.LogType type, string message)> _onLog;
        private readonly Dictionary<string, Operation> _operations = new(
            StringComparer.OrdinalIgnoreCase);
        private readonly object _gate = new();
        private int _nextSubscriberId;

        internal ContentDeliveryCoordinator(
            ContentFileStore files,
            BundlePayloadLoader bundles,
            ContentStoragePlanner storage,
            int maximumParallelDownloads,
            CancellationToken cancellationToken,
            Action<(UnityEngine.LogType type, string message)> onLog)
        {
            _files = files ?? throw new ArgumentNullException(nameof(files));
            _bundles = bundles ?? throw new ArgumentNullException(nameof(bundles));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _maximumParallelDownloads = maximumParallelDownloads > 0
                ? maximumParallelDownloads
                : throw new ArgumentOutOfRangeException(nameof(maximumParallelDownloads));
            _downloadSlots = new SemaphoreSlim(
                _maximumParallelDownloads,
                _maximumParallelDownloads);
            _cancellationToken = cancellationToken;
            _onLog = onLog;
        }

        internal async UniTask<ContentDeliveryLease> Prepare(
            ContentReleaseSession session,
            string groupId,
            Action<ContentDeliveryProgress> onProgress,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                throw new ArgumentException("Delivery group ID must not be empty.", nameof(groupId));
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            var release = session.Release;
            var group = release.DeliveryGroups.FirstOrDefault(value => string.Equals(
                value.Id,
                groupId,
                StringComparison.OrdinalIgnoreCase)) ?? throw new ContentConfigurationException(
                $"Delivery group '{groupId}' is absent from release '{release.ReleaseId}'.");
            var files = release.Files
                .Where(value => string.Equals(
                    value.DeliveryGroup,
                    group.Id,
                    StringComparison.OrdinalIgnoreCase))
                .ToArray();
            var bundles = release.Bundles
                .Where(value => string.Equals(
                    value.DeliveryGroup,
                    group.Id,
                    StringComparison.OrdinalIgnoreCase))
                .ToArray();
            var payloads = files
                .Select(file => new ContentCachePayload(
                    ContentStoragePlanner.FilePath(file),
                    file.Size))
                .Concat(bundles.Select(bundle =>
                    _bundles.GetCachePayload(session, bundle)))
                .ToArray();
            var lease = await _storage.Reserve(payloads)
                .AttachExternalCancellation(cancellationToken);
            var reporter = new ContentProgressReporter<ContentDeliveryProgress>(
                onProgress,
                _onLog);
            Operation operation;
            int subscriberId;
            var operationKey = $"{session.ReleaseId}:{group.Id}";
            lock (_gate)
            {
                subscriberId = ++_nextSubscriberId;
                if (!_operations.TryGetValue(operationKey, out operation))
                {
                    operation = new Operation();
                    _operations.Add(operationKey, operation);
                    operation.Progress.Add(subscriberId, reporter);
                    operation.Task = DownloadGroup(
                            operationKey,
                            operation,
                            session,
                            group,
                            files,
                            bundles)
                        .Preserve();
                }
                else
                {
                    operation.Progress.Add(subscriberId, reporter);
                }
            }
            try
            {
                await operation.Task.AttachExternalCancellation(cancellationToken);
                return lease;
            }
            catch
            {
                lease.Dispose();
                throw;
            }
            finally
            {
                lock (_gate)
                    operation.Progress.Remove(subscriberId);
            }
        }

        private async UniTask DownloadGroup(
            string operationKey,
            Operation operation,
            ContentReleaseSession session,
            ContentDeliveryGroupDescriptor group,
            ContentFileDescriptor[] files,
            BundleReleaseDescriptor[] bundles)
        {
            var itemCount = bundles.Length + files.Length;
            var nextItem = -1;
            var progress = new ContentDeliveryProgressTracker(
                group.Id,
                bundles.Select(value => value.Size)
                    .Concat(files.Select(value => value.Size))
                    .ToArray(),
                group.Size,
                value => ReportProgress(operation, value),
                _onLog);
            progress.PublishInitial();
            try
            {
                var workerCount = Math.Min(_maximumParallelDownloads, itemCount);
                var workers = Enumerable.Range(0, workerCount)
                    .Select(_ => DownloadWorker())
                    .ToArray();
                await UniTask.WhenAll(workers);
            }
            finally
            {
                lock (_gate)
                {
                    if (_operations.TryGetValue(operationKey, out var current)
                        && ReferenceEquals(current, operation))
                    {
                        _operations.Remove(operationKey);
                    }
                }
            }

            async UniTask DownloadWorker()
            {
                while (true)
                {
                    var index = Interlocked.Increment(ref nextItem);
                    if (index >= itemCount)
                        return;
                    _cancellationToken.ThrowIfCancellationRequested();
                    await _downloadSlots.WaitAsync(_cancellationToken);
                    try
                    {
                        if (index < bundles.Length)
                        {
                            var bundle = bundles[index];
                            await _bundles.Prepare(
                                    session,
                                    bundle,
                                    bytes => progress.ReportBytes(index, bytes),
                                    _cancellationToken);
                        }
                        else
                        {
                            var file = files[index - bundles.Length];
                            await _files.ResolveUrl(
                                    session,
                                    file.Path,
                                    bytes => progress.ReportBytes(index, bytes),
                                    _cancellationToken);
                        }
                        progress.Complete(index);
                    }
                    finally
                    {
                        _downloadSlots.Release();
                    }
                }
            }
        }

        private void ReportProgress(
            Operation operation,
            ContentDeliveryProgress progress)
        {
            ContentProgressReporter<ContentDeliveryProgress>[] reporters;
            lock (_gate)
                reporters = operation.Progress.Values.ToArray();
            foreach (var reporter in reporters)
                reporter.Report(progress);
        }
    }
}
