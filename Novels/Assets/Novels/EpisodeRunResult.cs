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
            Diagnostics.NovelError? error)
        {
            Status = status;
            Error = error;
        }

        internal EpisodeRunStatus Status { get; }
        internal Diagnostics.NovelError? Error { get; }

        internal static EpisodeRunResult Completed() =>
            new(EpisodeRunStatus.Completed, null);

        internal static EpisodeRunResult Cancelled() =>
            new(EpisodeRunStatus.Cancelled, null);

        internal static EpisodeRunResult Failed(Diagnostics.NovelError error) =>
            new(EpisodeRunStatus.Failed, error);
    }
}
