using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Editor
{
    internal enum ContentValidationSeverity
    {
        Warning,
        Error,
    }

    internal static class ContentValidationCodes
    {
        internal const string Generic = "CONTENT_VALIDATION";
        internal const string StoryCompiledFileMissing = "STORY_COMPILED_FILE_MISSING";
        internal const string StorySourceFileMissing = "STORY_SOURCE_FILE_MISSING";
        internal const string StorySourceIncludeMissing = "STORY_SOURCE_INCLUDE_MISSING";
        internal const string StorySourceIncludeCycle = "STORY_SOURCE_INCLUDE_CYCLE";
        internal const string StorySourceReadFailed = "STORY_SOURCE_READ_FAILED";
        internal const string StoryEpisodeBoundaryMissing =
            "STORY_EPISODE_BOUNDARY_MISSING";
        internal const string StoryCommandInvalid = "STORY_COMMAND_INVALID";
        internal const string StoryTimedChoiceUnsupported =
            "STORY_TIMED_CHOICE_UNSUPPORTED";
        internal const string StoryResourceUnresolved = "STORY_RESOURCE_UNRESOLVED";
        internal const string StoryCameraUnsupported = "STORY_CAMERA_UNSUPPORTED";
        internal const string StoryAudioMissing = "STORY_AUDIO_MISSING";
        internal const string StoryAudioNameInvalid = "STORY_AUDIO_NAME_INVALID";
        internal const string StoryAudioFormatAmbiguous = "STORY_AUDIO_FORMAT_AMBIGUOUS";
        internal const string StoryBackgroundMissing = "STORY_BACKGROUND_MISSING";
        internal const string StoryCharacterMissing = "STORY_CHARACTER_MISSING";
        internal const string StoryCharacterTextureImportInvalid =
            "STORY_CHARACTER_TEXTURE_IMPORT_INVALID";
        internal const string StoryCharacterCaseMismatch = "STORY_CHARACTER_CASE_MISMATCH";
        internal const string StoryCharacterCaseAmbiguous = "STORY_CHARACTER_CASE_AMBIGUOUS";
        internal const string StoryCharacterAssetMissing = "STORY_CHARACTER_ASSET_MISSING";
        internal const string StoryCharacterAssetCaseMismatch =
            "STORY_CHARACTER_ASSET_CASE_MISMATCH";
        internal const string EpisodeUiPrefabMissing = "EPISODE_UI_PREFAB_MISSING";
    }

    internal readonly struct ContentValidationIssue
    {
        private ContentValidationIssue(
            string code,
            ContentValidationSeverity severity,
            string message,
            string assetPath,
            string contentId,
            string episodeId)
        {
            Code = string.IsNullOrWhiteSpace(code)
                ? ContentValidationCodes.Generic
                : code;
            Severity = severity;
            Message = message ?? string.Empty;
            AssetPath = assetPath ?? string.Empty;
            ContentId = contentId ?? string.Empty;
            EpisodeId = episodeId ?? string.Empty;
        }

        internal string Code { get; }
        internal ContentValidationSeverity Severity { get; }
        internal string Message { get; }
        internal string AssetPath { get; }
        internal string ContentId { get; }
        internal string EpisodeId { get; }

        internal static ContentValidationIssue Error(
            string code,
            string message,
            string assetPath = null,
            string contentId = null,
            string episodeId = null) =>
            new(
                code,
                ContentValidationSeverity.Error,
                message,
                assetPath,
                contentId,
                episodeId);

        internal static ContentValidationIssue Warning(
            string code,
            string message,
            string assetPath = null,
            string contentId = null,
            string episodeId = null) =>
            new(
                code,
                ContentValidationSeverity.Warning,
                message,
                assetPath,
                contentId,
                episodeId);

        public override string ToString()
        {
            var context = string.Join(
                "/",
                new[] { ContentId, EpisodeId }
                    .Where(value => !string.IsNullOrWhiteSpace(value)));
            var prefix = string.IsNullOrEmpty(context)
                ? $"[{Code}]"
                : $"[{Code}] [{context}]";
            return $"{prefix} {Message}";
        }
    }

    internal sealed class ContentValidationReport : IReadOnlyList<string>, ICollection<string>
    {
        private readonly List<ContentValidationIssue> _issues = new();

        internal IReadOnlyList<ContentValidationIssue> Issues => _issues;
        public int Count => _issues.Count;
        public bool IsReadOnly => false;
        public string this[int index] => _issues[index].ToString();

        public void Add(string message) =>
            Add(ContentValidationIssue.Error(
                ContentValidationCodes.Generic,
                message));

        internal void Add(ContentValidationIssue issue) => _issues.Add(issue);

        public void Clear() => _issues.Clear();

        public bool Contains(string item) =>
            _issues.Any(issue => string.Equals(
                issue.ToString(),
                item,
                StringComparison.Ordinal));

        public void CopyTo(string[] array, int arrayIndex)
        {
            foreach (var issue in _issues)
                array[arrayIndex++] = issue.ToString();
        }

        public bool Remove(string item)
        {
            for (var index = 0; index < _issues.Count; index++)
            {
                if (!string.Equals(
                        _issues[index].ToString(),
                        item,
                        StringComparison.Ordinal))
                    continue;
                _issues.RemoveAt(index);
                return true;
            }
            return false;
        }

        public IEnumerator<string> GetEnumerator() =>
            _issues.Select(issue => issue.ToString()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal static class ContentValidationReportFormatter
    {
        internal static string Format(
            ContentValidationReport report,
            ContentValidationSeverity severity,
            string phase)
        {
            var issues = report.Issues
                .Where(issue => issue.Severity == severity)
                .ToArray();
            if (issues.Length == 0)
                return string.Empty;

            var title = severity == ContentValidationSeverity.Warning
                ? "warnings"
                : "errors";
            var result = new StringBuilder()
                .Append("[NovelContent] [")
                .Append(phase)
                .Append("] Validation ")
                .Append(title)
                .Append(" (")
                .Append(issues.Length)
                .AppendLine("):");

            foreach (var contextGroup in issues
                         .GroupBy(
                             issue => (issue.ContentId, issue.EpisodeId),
                             ContentValidationContextComparer.Instance)
                         .OrderBy(group => group.Key.ContentId, StringComparer.Ordinal)
                         .ThenBy(group => group.Key.EpisodeId, StringComparer.Ordinal))
            {
                result.AppendLine();
                result.AppendLine(FormatContext(
                    contextGroup.Key.ContentId,
                    contextGroup.Key.EpisodeId));

                foreach (var codeGroup in contextGroup
                             .GroupBy(issue => issue.Code, StringComparer.Ordinal)
                             .OrderBy(group => group.Key, StringComparer.Ordinal))
                {
                    var messages = codeGroup
                        .GroupBy(issue => issue.Message, StringComparer.Ordinal)
                        .Select(group => (Message: group.Key, Count: group.Count()))
                        .OrderBy(item => item.Message, StringComparer.Ordinal)
                        .ToArray();
                    result.Append("  ")
                        .Append(codeGroup.Key)
                        .Append(" (")
                        .Append(codeGroup.Count())
                        .AppendLine("):");
                    foreach (var message in messages)
                    {
                        result.Append("  - ").Append(message.Message);
                        if (message.Count > 1)
                            result.Append(" (repeated ").Append(message.Count).Append(" times)");
                        result.AppendLine();
                    }
                }
            }
            return result.ToString().TrimEnd();
        }

        private static string FormatContext(string contentId, string episodeId)
        {
            if (string.IsNullOrWhiteSpace(contentId))
                return "General";
            return string.IsNullOrWhiteSpace(episodeId)
                ? contentId
                : $"{contentId}/{episodeId}";
        }

        private sealed class ContentValidationContextComparer
            : IEqualityComparer<(string ContentId, string EpisodeId)>
        {
            internal static readonly ContentValidationContextComparer Instance = new();

            public bool Equals(
                (string ContentId, string EpisodeId) left,
                (string ContentId, string EpisodeId) right) =>
                string.Equals(left.ContentId, right.ContentId, StringComparison.Ordinal)
                && string.Equals(left.EpisodeId, right.EpisodeId, StringComparison.Ordinal);

            public int GetHashCode((string ContentId, string EpisodeId) value)
            {
                unchecked
                {
                    return ((value.ContentId?.GetHashCode() ?? 0) * 397)
                        ^ (value.EpisodeId?.GetHashCode() ?? 0);
                }
            }
        }
    }
}
