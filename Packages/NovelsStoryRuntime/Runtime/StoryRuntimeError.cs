using System;

namespace Novels
{
    public enum StoryRuntimeErrorSeverity
    {
        Warning,
        Recoverable,
        Fatal,
    }

    public readonly struct StoryRuntimeError
    {
        public StoryRuntimeError(
            string code,
            StoryRuntimeErrorSeverity severity,
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
        public StoryRuntimeErrorSeverity Severity { get; }
        public string Message { get; }
        public string Source { get; }
        public Exception Exception { get; }
    }

    public static class StoryRuntimeErrorCodes
    {
        public const string QueueExecutionFailed = "QUEUE_EXECUTION_FAILED";
        public const string SaveReadFailed = "SAVE_READ_FAILED";
        public const string SaveWriteFailed = "SAVE_WRITE_FAILED";
    }
}
