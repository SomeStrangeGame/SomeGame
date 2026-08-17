using System;

namespace Novels.Diagnostics
{
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
            Exception exception = null)
        {
            Code = code ?? string.Empty;
            Severity = severity;
            Message = message ?? string.Empty;
            Source = source ?? string.Empty;
            Exception = exception;
        }

        public string Code { get; }
        public NovelErrorSeverity Severity { get; }
        public string Message { get; }
        public string Source { get; }
        public Exception Exception { get; }

        public override string ToString()
        {
            var source = string.IsNullOrEmpty(Source)
                ? string.Empty
                : $"\nSource: {Source}";
            var exception = Exception == null
                ? string.Empty
                : $"\n{Exception}";
            return $"[{Code}] {Message}{source}{exception}";
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
