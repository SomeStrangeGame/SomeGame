using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bundles
{
    internal sealed class ContentDeliveryCoordinator
    {
        private const int _maximumParallelDownloads = 3;
        private readonly ContentReleaseProvider _releases;
        private readonly ContentFileStore _files;

        internal ContentDeliveryCoordinator(
            ContentReleaseProvider releases,
            ContentFileStore files)
        {
            _releases = releases ?? throw new ArgumentNullException(nameof(releases));
            _files = files ?? throw new ArgumentNullException(nameof(files));
        }

        internal async UniTask Prepare(
            string groupId,
            Action<ContentDeliveryProgress> onProgress,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                throw new ArgumentException("Delivery group ID must not be empty.", nameof(groupId));
            var release = _releases.Current ?? throw new ContentConfigurationException(
                "Content release must be loaded before delivery preparation.");
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
            _files.ReserveGroup(files);
            var downloadedBytes = new long[files.Length];
            var completedFiles = 0;
            var nextFile = -1;
            var progressGate = new object();
            onProgress?.Invoke(new ContentDeliveryProgress(
                group.Id,
                0,
                files.Length,
                0,
                group.Size));
            try
            {
                var workerCount = Math.Min(_maximumParallelDownloads, files.Length);
                var workers = Enumerable.Range(0, workerCount)
                    .Select(_ => DownloadWorker())
                    .ToArray();
                await UniTask.WhenAll(workers);
            }
            catch
            {
                _files.ReleaseGroupReservation(files);
                throw;
            }

            async UniTask DownloadWorker()
            {
                while (true)
                {
                    var index = Interlocked.Increment(ref nextFile);
                    if (index >= files.Length)
                        return;
                    cancellationToken.ThrowIfCancellationRequested();
                    await _files.ResolveUrl(
                            files[index].Path,
                            bytes => ReportProgress(index, bytes))
                        .AttachExternalCancellation(cancellationToken);
                    lock (progressGate)
                    {
                        downloadedBytes[index] = files[index].Size;
                        completedFiles++;
                        PublishProgress();
                    }
                }
            }

            void ReportProgress(int index, long bytes)
            {
                lock (progressGate)
                {
                    downloadedBytes[index] = Math.Min(files[index].Size, bytes);
                    PublishProgress();
                }
            }

            void PublishProgress()
            {
                onProgress?.Invoke(new ContentDeliveryProgress(
                    group.Id,
                    completedFiles,
                    files.Length,
                    downloadedBytes.Sum(),
                    group.Size));
            }
        }
    }
}
