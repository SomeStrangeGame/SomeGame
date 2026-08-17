using System;

namespace Bundles
{
    public sealed class ContentRequestPolicy
    {
        public static ContentRequestPolicy RemoteDefault { get; } = new(3, 30, 500, 4000);
        public static ContentRequestPolicy LocalDefault { get; } = new(1, 0, 0, 0);

        public ContentRequestPolicy(
            int maximumAttempts,
            int timeoutSeconds,
            int initialRetryDelayMilliseconds,
            int maximumRetryDelayMilliseconds)
        {
            if (maximumAttempts <= 0)
                throw new ArgumentOutOfRangeException(nameof(maximumAttempts));
            if (timeoutSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(timeoutSeconds));
            if (initialRetryDelayMilliseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(initialRetryDelayMilliseconds));
            if (maximumRetryDelayMilliseconds < initialRetryDelayMilliseconds)
                throw new ArgumentOutOfRangeException(nameof(maximumRetryDelayMilliseconds));
            MaximumAttempts = maximumAttempts;
            TimeoutSeconds = timeoutSeconds;
            InitialRetryDelayMilliseconds = initialRetryDelayMilliseconds;
            MaximumRetryDelayMilliseconds = maximumRetryDelayMilliseconds;
        }

        public int MaximumAttempts { get; }
        public int TimeoutSeconds { get; }
        public int InitialRetryDelayMilliseconds { get; }
        public int MaximumRetryDelayMilliseconds { get; }

        internal int GetRetryDelayMilliseconds(int failedAttempt)
        {
            if (InitialRetryDelayMilliseconds == 0)
                return 0;
            var multiplier = 1L << Math.Min(20, Math.Max(0, failedAttempt - 1));
            return (int)Math.Min(
                MaximumRetryDelayMilliseconds,
                InitialRetryDelayMilliseconds * multiplier);
        }
    }
}
