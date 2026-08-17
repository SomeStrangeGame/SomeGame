using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bundles
{
    internal sealed class ContentDeliveryCoordinator
    {
        private const int _maximumParallelDownloads = 3;
        private readonly ContentFileStore _files;
        private readonly BundlePayloadLoader _bundles;
        private readonly ContentStoragePlanner _storage;

        internal ContentDeliveryCoordinator(
            ContentFileStore files,
            BundlePayloadLoader bundles,
            ContentStoragePlanner storage)
        {
            _files = files ?? throw new ArgumentNullException(nameof(files));
            _bundles = bundles ?? throw new ArgumentNullException(nameof(bundles));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
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
                    ContentStoragePlanner.FilePath(session, file.Path),
                    file.Size))
                .Concat(bundles.Select(bundle =>
                    _bundles.GetCachePayload(session, bundle)))
                .ToArray();
            var lease = await _storage.Reserve(payloads)
                .AttachExternalCancellation(cancellationToken);
            var itemCount = bundles.Length + files.Length;
            var nextItem = -1;
            var progress = new ContentDeliveryProgressTracker(
                group.Id,
                bundles.Select(value => value.Size)
                    .Concat(files.Select(value => value.Size))
                    .ToArray(),
                group.Size,
                onProgress);
            progress.PublishInitial();
            try
            {
                var workerCount = Math.Min(_maximumParallelDownloads, itemCount);
                var workers = Enumerable.Range(0, workerCount)
                    .Select(_ => DownloadWorker())
                    .ToArray();
                await UniTask.WhenAll(workers);
                return lease;
            }
            catch
            {
                lease.Dispose();
                throw;
            }

            async UniTask DownloadWorker()
            {
                while (true)
                {
                    var index = Interlocked.Increment(ref nextItem);
                    if (index >= itemCount)
                        return;
                    cancellationToken.ThrowIfCancellationRequested();
                    if (index < bundles.Length)
                    {
                        var bundle = bundles[index];
                        await _bundles.Prepare(
                                session,
                                bundle,
                                bytes => progress.ReportBytes(index, bytes))
                            .AttachExternalCancellation(cancellationToken);
                    }
                    else
                    {
                        var file = files[index - bundles.Length];
                        await _files.ResolveUrl(
                                session,
                                file.Path,
                                bytes => progress.ReportBytes(index, bytes))
                            .AttachExternalCancellation(cancellationToken);
                    }
                    progress.Complete(index);
                }
            }
        }
    }
}
