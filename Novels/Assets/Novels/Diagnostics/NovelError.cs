using System;

namespace Novels.Diagnostics
{
    public readonly struct NovelErrorContext
    {
        public NovelErrorContext(
            string releaseId = "",
            string contentId = "",
            string episodeId = "",
            string deliveryMode = "")
        {
            ReleaseId = releaseId ?? string.Empty;
            ContentId = contentId ?? string.Empty;
            EpisodeId = episodeId ?? string.Empty;
            DeliveryMode = deliveryMode ?? string.Empty;
        }

        public string ReleaseId { get; }
        public string ContentId { get; }
        public string EpisodeId { get; }
        public string DeliveryMode { get; }
        public bool IsEmpty => string.IsNullOrEmpty(ReleaseId)
            && string.IsNullOrEmpty(ContentId)
            && string.IsNullOrEmpty(EpisodeId)
            && string.IsNullOrEmpty(DeliveryMode);

        public override string ToString() =>
            $"release={Value(ReleaseId)}, content={Value(ContentId)}, "
            + $"episode={Value(EpisodeId)}, delivery={Value(DeliveryMode)}";

        private static string Value(string value) =>
            string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    public enum NovelErrorSeverity
    {
        Warning,
        Recoverable,
        Fatal,
    }

    public readonly struct NovelError
    {
        public NovelError(
            string code,
            NovelErrorSeverity severity,
            string message,
            string source = "",
            Exception exception = null,
            NovelErrorContext context = default)
        {
            Code = code ?? string.Empty;
            Severity = severity;
            Message = message ?? string.Empty;
            Source = source ?? string.Empty;
            Exception = exception;
            Context = context;
        }

        public string Code { get; }
        public NovelErrorSeverity Severity { get; }
        public string Message { get; }
        public string Source { get; }
        public Exception Exception { get; }
        public NovelErrorContext Context { get; }

        public NovelError WithContext(NovelErrorContext context) =>
            new(Code, Severity, Message, Source, Exception, context);

        public override string ToString()
        {
            var source = string.IsNullOrEmpty(Source)
                ? string.Empty
                : $"\nSource: {Source}";
            var exception = Exception == null
                ? string.Empty
                : $"\n{Exception}";
            var context = Context.IsEmpty ? string.Empty : $"\nContext: {Context}";
            return $"[{Code}] {Message}{context}{source}{exception}";
        }
    }

    public static class NovelErrorCodes
    {
        public const string InitializationFailed = "INITIALIZATION_FAILED";
        public const string StoryParseFailed = "STORY_PARSE_FAILED";
        public const string SaveReadFailed = "SAVE_READ_FAILED";
        public const string SaveWriteFailed = "SAVE_WRITE_FAILED";
        public const string SaveContentMismatch = "SAVE_CONTENT_MISMATCH";
        public const string QueueExecutionFailed = "QUEUE_EXECUTION_FAILED";
        public const string AudioPlaybackFailed = "AUDIO_PLAYBACK_FAILED";
        public const string UnsupportedCameraAction = "UNSUPPORTED_CAMERA_ACTION";
        public const string BundleFailure = "BUNDLE_FAILURE";
        public const string ContentPreparationFailed = "CONTENT_PREPARATION_FAILED";
        public const string NotificationFailed = "NOTIFICATION_FAILED";
        public const string VideoPlaybackFailed = "VIDEO_PLAYBACK_FAILED";
    }
}
