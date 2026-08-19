using System;

namespace Novels
{
    internal enum EpisodeRunStatus
    {
        Completed,
        Failed,
        Cancelled,
    }

    internal readonly struct EpisodeRunResult
    {
        private EpisodeRunResult(
            EpisodeRunStatus status,
            Diagnostics.NovelError? error,
            string continuationState)
        {
            Status = status;
            Error = error;
            ContinuationState = continuationState;
        }

        internal EpisodeRunStatus Status { get; }
        internal Diagnostics.NovelError? Error { get; }
        internal string ContinuationState { get; }

        internal static EpisodeRunResult Completed() =>
            new(EpisodeRunStatus.Completed, null, null);

        internal static EpisodeRunResult Completed(string continuationState) =>
            new(EpisodeRunStatus.Completed, null, continuationState);

        internal static EpisodeRunResult Cancelled() =>
            new(EpisodeRunStatus.Cancelled, null, null);

        internal static EpisodeRunResult Failed(Diagnostics.NovelError error) =>
            new(EpisodeRunStatus.Failed, error, null);
    }
}
