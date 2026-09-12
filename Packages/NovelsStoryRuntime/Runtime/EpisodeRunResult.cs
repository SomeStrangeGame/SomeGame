using System;

namespace Novels
{
    public enum EpisodeRunStatus
    {
        Completed,
        Failed,
        Cancelled,
    }

    public readonly struct EpisodeRunResult
    {
        private EpisodeRunResult(
            EpisodeRunStatus status,
            StoryRuntimeError? error,
            string continuationState)
        {
            Status = status;
            Error = error;
            ContinuationState = continuationState;
        }

        public EpisodeRunStatus Status { get; }
        public StoryRuntimeError? Error { get; }
        public string ContinuationState { get; }

        public static EpisodeRunResult Completed() =>
            new(EpisodeRunStatus.Completed, null, null);

        public static EpisodeRunResult Completed(string continuationState) =>
            new(EpisodeRunStatus.Completed, null, continuationState);

        public static EpisodeRunResult Cancelled() =>
            new(EpisodeRunStatus.Cancelled, null, null);

        public static EpisodeRunResult Failed(StoryRuntimeError error) =>
            new(EpisodeRunStatus.Failed, error, null);
    }
}
