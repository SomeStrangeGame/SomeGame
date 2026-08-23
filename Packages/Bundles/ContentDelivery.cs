using System;

namespace Bundles
{
    public enum ContentDeliveryMode
    {
        Remote = 2,
    }

    public readonly struct ContentDeliveryProgress
    {
        internal ContentDeliveryProgress(
            string groupId,
            int completedItems,
            int totalItems,
            long completedBytes,
            long totalBytes)
        {
            GroupId = groupId;
            CompletedItems = completedItems;
            TotalItems = totalItems;
            CompletedBytes = completedBytes;
            TotalBytes = totalBytes;
        }

        public string GroupId { get; }
        public int CompletedItems { get; }
        public int TotalItems { get; }
        public long CompletedBytes { get; }
        public long TotalBytes { get; }
        public float Ratio => TotalBytes <= 0
            ? 1f
            : Math.Min(1f, (float)CompletedBytes / TotalBytes);
    }
}
