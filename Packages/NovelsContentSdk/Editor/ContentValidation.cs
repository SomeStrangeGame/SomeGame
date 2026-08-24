using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Novels.ContentSdk.Editor
{
    internal readonly struct ValidationIssue
    {
        internal ValidationIssue(string code, string message, string source = "")
        {
            Code = code;
            Message = message;
            Source = source;
        }

        internal string Code { get; }
        internal string Message { get; }
        internal string Source { get; }
    }

    internal sealed class ValidationReport
    {
        private readonly List<ValidationIssue> _issues = new();

        internal void Error(string code, string message, string source = "") =>
            _issues.Add(new ValidationIssue(code, message, source));

        internal void ThrowIfInvalid()
        {
            if (_issues.Count > 0)
                throw new InvalidOperationException(Format());
        }

        private string Format()
        {
            var text = new StringBuilder("Content validation errors:");
            foreach (var group in _issues
                         .GroupBy(value => value.Code)
                         .OrderBy(value => value.Key, StringComparer.Ordinal))
            {
                text.AppendLine().Append("- [").Append(group.Key).Append(']');
                foreach (var issue in group)
                {
                    text.AppendLine().Append("  ");
                    if (!string.IsNullOrWhiteSpace(issue.Source))
                        text.Append(issue.Source).Append(": ");
                    text.Append(issue.Message);
                }
            }
            return text.ToString();
        }
    }
}
