using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Novels.ContentSdk.Editor
{
    internal enum ValidationSeverity
    {
        Warning,
        Error,
    }

    internal readonly struct ValidationIssue
    {
        internal ValidationIssue(
            ValidationSeverity severity,
            string code,
            string message,
            string source = "")
        {
            Severity = severity;
            Code = code;
            Message = message;
            Source = source;
        }

        internal ValidationSeverity Severity { get; }
        internal string Code { get; }
        internal string Message { get; }
        internal string Source { get; }
    }

    internal sealed class ValidationReport
    {
        private readonly List<ValidationIssue> _issues = new();

        internal bool HasErrors => _issues.Any(value =>
            value.Severity == ValidationSeverity.Error);

        internal void Error(string code, string message, string source = "") =>
            _issues.Add(new ValidationIssue(ValidationSeverity.Error, code, message, source));

        internal void Warning(string code, string message, string source = "") =>
            _issues.Add(new ValidationIssue(ValidationSeverity.Warning, code, message, source));

        internal void LogWarnings() =>
            Log(ValidationSeverity.Warning, Debug.LogWarning);

        internal void ThrowIfInvalid()
        {
            if (HasErrors)
                throw new InvalidOperationException(Format(ValidationSeverity.Error));
        }

        private void Log(ValidationSeverity severity, Action<object> write)
        {
            if (_issues.Any(value => value.Severity == severity))
                write(Format(severity));
        }

        private string Format(ValidationSeverity severity)
        {
            var title = severity == ValidationSeverity.Error
                ? "Content validation errors"
                : "Content validation warnings";
            var text = new StringBuilder(title).Append(':');
            foreach (var group in _issues
                         .Where(value => value.Severity == severity)
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
