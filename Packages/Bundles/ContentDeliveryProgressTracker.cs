using System;
using UnityEngine;

namespace Bundles
{
    internal sealed class ContentDeliveryProgressTracker
    {
        private readonly string _groupId;
        private readonly long[] _sizes;
        private readonly long[] _downloaded;
        private readonly long _totalBytes;
        private readonly ContentProgressReporter<ContentDeliveryProgress> _progress;
        private readonly object _gate = new();
        private long _downloadedBytes;
        private int _completedItems;
        private int _lastPublishedFrame = -1;

        internal ContentDeliveryProgressTracker(
            string groupId,
            long[] sizes,
            long totalBytes,
            Action<ContentDeliveryProgress> onProgress,
            Action<(LogType type, string message)> onLog)
        {
            _groupId = groupId;
            _sizes = sizes ?? throw new ArgumentNullException(nameof(sizes));
            _downloaded = new long[sizes.Length];
            _totalBytes = totalBytes;
            _progress = new ContentProgressReporter<ContentDeliveryProgress>(
                onProgress,
                onLog);
        }

        internal void PublishInitial() =>
            _progress.Report(CreateSnapshot());

        internal void ReportBytes(int index, long bytes)
        {
            ContentDeliveryProgress? progress = null;
            lock (_gate)
            {
                SetBytes(index, bytes);
                if (_lastPublishedFrame != Time.frameCount)
                {
                    _lastPublishedFrame = Time.frameCount;
                    progress = CreateSnapshot();
                }
            }
            if (progress.HasValue)
                _progress.Report(progress.Value);
        }

        internal void Complete(int index)
        {
            ContentDeliveryProgress progress;
            lock (_gate)
            {
                SetBytes(index, _sizes[index]);
                _completedItems++;
                _lastPublishedFrame = Time.frameCount;
                progress = CreateSnapshot();
            }
            _progress.Report(progress);
        }

        private void SetBytes(int index, long bytes)
        {
            var normalized = Math.Min(_sizes[index], Math.Max(0L, bytes));
            _downloadedBytes += normalized - _downloaded[index];
            _downloaded[index] = normalized;
        }

        private ContentDeliveryProgress CreateSnapshot() =>
            new(
                _groupId,
                _completedItems,
                _sizes.Length,
                _downloadedBytes,
                _totalBytes);
    }
}
