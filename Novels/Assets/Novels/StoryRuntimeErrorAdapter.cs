namespace Novels
{
    internal static class StoryRuntimeErrorAdapter
    {
        internal static Diagnostics.NovelError ToNovelError(
            this StoryRuntimeError error,
            Diagnostics.NovelErrorContext context = default)
        {
            return new Diagnostics.NovelError(
                error.Code,
                (Diagnostics.NovelErrorSeverity)error.Severity,
                error.Message,
                error.Source,
                error.Exception,
                context);
        }
    }
}
