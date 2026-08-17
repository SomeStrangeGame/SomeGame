using System;

namespace Bundles
{
    public sealed class ContentDeliveryOptions
    {
        public const long DefaultCacheLimitBytes = 512L * 1024L * 1024L;
        public const int DefaultMaximumParallelDownloads = 3;

        public static ContentDeliveryOptions Default { get; } = new(
            DefaultCacheLimitBytes,
            DefaultMaximumParallelDownloads,
            TimeSpan.FromDays(1),
            ContentRequestPolicy.RemoteDefault,
            ContentRequestPolicy.LocalDefault);

        public ContentDeliveryOptions(
            long cacheLimitBytes,
            int maximumParallelDownloads,
            TimeSpan stagingLifetime,
            ContentRequestPolicy remoteRequestPolicy,
            ContentRequestPolicy localRequestPolicy)
        {
            CacheLimitBytes = cacheLimitBytes > 0
                ? cacheLimitBytes
                : throw new ArgumentOutOfRangeException(nameof(cacheLimitBytes));
            MaximumParallelDownloads = maximumParallelDownloads > 0
                ? maximumParallelDownloads
                : throw new ArgumentOutOfRangeException(nameof(maximumParallelDownloads));
            StagingLifetime = stagingLifetime > TimeSpan.Zero
                ? stagingLifetime
                : throw new ArgumentOutOfRangeException(nameof(stagingLifetime));
            RemoteRequestPolicy = remoteRequestPolicy
                ?? throw new ArgumentNullException(nameof(remoteRequestPolicy));
            LocalRequestPolicy = localRequestPolicy
                ?? throw new ArgumentNullException(nameof(localRequestPolicy));
        }

        public long CacheLimitBytes { get; }
        public int MaximumParallelDownloads { get; }
        public TimeSpan StagingLifetime { get; }
        public ContentRequestPolicy RemoteRequestPolicy { get; }
        public ContentRequestPolicy LocalRequestPolicy { get; }
    }
}
