using System;
using System.Collections.Generic;

namespace Novels.StoryCommands
{
    internal static class StoryPrefixParser
    {
        internal static PrefixParseResult Parse(string prefix, string source)
        {
            var openParenthesisIndex = prefix.IndexOf('(');
            if (openParenthesisIndex < 0)
                return PrefixParseResult.Success(prefix.Trim(), Array.Empty<string>());

            var closeParenthesisIndex = prefix.LastIndexOf(')');
            if (closeParenthesisIndex < openParenthesisIndex)
            {
                return PrefixParseResult.Failure(
                    StoryCommandSyntax.InvalidArguments,
                    "Command arguments have no closing parenthesis.",
                    source);
            }

            if (prefix.Substring(closeParenthesisIndex + 1).Trim().Length > 0)
            {
                return PrefixParseResult.Failure(
                    StoryCommandSyntax.InvalidArguments,
                    "Unexpected text after command arguments.",
                    source);
            }

            var name = prefix.Substring(0, openParenthesisIndex).Trim();
            var argumentsSource = prefix.Substring(
                openParenthesisIndex + 1,
                closeParenthesisIndex - openParenthesisIndex - 1);
            var rawArguments = argumentsSource.Split(',');
            var arguments = new List<string>(rawArguments.Length);

            foreach (var rawArgument in rawArguments)
            {
                var argument = rawArgument.Trim();
                if (argument.Length > 0)
                    arguments.Add(argument);
            }

            return PrefixParseResult.Success(name, arguments.ToArray());
        }
    }

    internal readonly struct ParsedPrefix
    {
        internal ParsedPrefix(string name, string[] arguments)
        {
            Name = name;
            Arguments = arguments;
        }

        internal string Name { get; }
        internal string[] Arguments { get; }
    }

    internal readonly struct PrefixParseResult
    {
        private PrefixParseResult(
            bool isSuccess,
            ParsedPrefix prefix,
            StoryParseError error)
        {
            IsSuccess = isSuccess;
            Prefix = prefix;
            Error = error;
        }

        internal bool IsSuccess { get; }
        internal ParsedPrefix Prefix { get; }
        internal StoryParseError Error { get; }

        internal static PrefixParseResult Success(
            string name,
            string[] arguments)
        {
            return new PrefixParseResult(
                true,
                new ParsedPrefix(name, arguments),
                default);
        }

        internal static PrefixParseResult Failure(
            string code,
            string message,
            string source)
        {
            return new PrefixParseResult(
                false,
                default,
                new StoryParseError(code, message, source));
        }
    }
}
