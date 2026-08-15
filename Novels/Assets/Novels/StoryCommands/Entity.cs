using System;
using System.Collections.Generic;

namespace Novels.StoryCommands
{
    public sealed class Entity
    {
        public StoryParseResult Parse(string source, bool hasChoices)
        {
            source ??= string.Empty;
            var normalizedSource = source.Trim();

            if (normalizedSource.Length == 0)
            {
                var emptyCommandType = hasChoices
                    ? StoryCommandType.Dialogue
                    : StoryCommandType.Empty;
                return StoryParseResult.Success(CreateCommand(emptyCommandType, source, string.Empty, string.Empty, Array.Empty<string>()));
            }

            var separatorIndex = normalizedSource.IndexOf(':');
            var prefix = separatorIndex < 0
                ? normalizedSource
                : normalizedSource.Substring(0, separatorIndex).Trim();
            var value = separatorIndex < 0
                ? normalizedSource
                : normalizedSource.Substring(separatorIndex + 1).Trim();

            var prefixResult = ParsePrefix(prefix, source);
            if (!prefixResult.IsSuccess)
                return prefixResult;

            var parsedPrefix = prefixResult.Command;
            var name = parsedPrefix.Name;
            var arguments = parsedPrefix.Arguments;

            if (StoryCommandSyntax.MetadataNames.Contains(name))
                return StoryParseResult.Success(CreateCommand(StoryCommandType.Metadata, source, name, value, arguments));

            if (name.IndexOf(StoryCommandSyntax.Keyboard, StringComparison.OrdinalIgnoreCase) >= 0)
                return StoryParseResult.Success(CreateCommand(StoryCommandType.Keyboard, source, name, value, arguments));

            if (!StoryCommandSyntax.CommandTypes.TryGetValue(name, out var commandType))
                return StoryParseResult.Success(CreateCommand(StoryCommandType.Dialogue, source, name, value, arguments));

            if (commandType != StoryCommandType.Wait)
                return StoryParseResult.Success(CreateCommand(commandType, source, name, value, arguments));

            if (!int.TryParse(value, out var waitDuration))
                return StoryParseResult.Failure(StoryCommandSyntax.InvalidWaitDuration, $"Expected an integer wait duration, got '{value}'.", source);

            return StoryParseResult.Success(CreateCommand(commandType, source, name, value, arguments, waitDuration));
        }

        private static StoryParseResult ParsePrefix(string prefix, string source)
        {
            var openParenthesisIndex = prefix.IndexOf('(');
            if (openParenthesisIndex < 0)
                return StoryParseResult.Success(CreateCommand(StoryCommandType.Empty, source, prefix.Trim(), string.Empty, Array.Empty<string>()));

            var closeParenthesisIndex = prefix.LastIndexOf(')');
            if (closeParenthesisIndex < openParenthesisIndex)
                return StoryParseResult.Failure(StoryCommandSyntax.InvalidArguments, "Command arguments have no closing parenthesis.", source);

            if (prefix.Substring(closeParenthesisIndex + 1).Trim().Length > 0)
                return StoryParseResult.Failure(StoryCommandSyntax.InvalidArguments, "Unexpected text after command arguments.", source);

            var name = prefix.Substring(0, openParenthesisIndex).Trim();
            var argumentsSource = prefix.Substring(openParenthesisIndex + 1, closeParenthesisIndex - openParenthesisIndex - 1);
            var rawArguments = argumentsSource.Split(',');
            var arguments = new List<string>(rawArguments.Length);

            foreach (var rawArgument in rawArguments)
            {
                var argument = rawArgument.Trim();
                if (argument.Length > 0)
                    arguments.Add(argument);
            }

            return StoryParseResult.Success(CreateCommand(StoryCommandType.Empty, source, name, string.Empty, arguments.ToArray()));
        }

        private static StoryCommand CreateCommand(
            StoryCommandType type,
            string source,
            string name,
            string value,
            string[] arguments,
            int waitDuration = 0)
        {
            return new StoryCommand(type, source, name, value, arguments, waitDuration);
        }
    }
}
