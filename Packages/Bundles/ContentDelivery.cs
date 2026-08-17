using System;

namespace Bundles
{
    public enum ContentDeliveryMode
    {
        Embedded,
        Hybrid,
        Remote,
    }

    public readonly struct ContentDeliveryProgress
    {
        internal ContentDeliveryProgress(
            string groupId,
            int completedFiles,
            int totalFiles,
            long completedBytes,
            long totalBytes)
        {
            GroupId = groupId;
            CompletedFiles = completedFiles;
            TotalFiles = totalFiles;
            CompletedBytes = completedBytes;
            TotalBytes = totalBytes;
        }

        public string GroupId { get; }
        public int CompletedFiles { get; }
        public int TotalFiles { get; }
        public long CompletedBytes { get; }
        public long TotalBytes { get; }
        public float Ratio => TotalBytes <= 0
            ? 1f
            : Math.Min(1f, (float)CompletedBytes / TotalBytes);
    }
}
