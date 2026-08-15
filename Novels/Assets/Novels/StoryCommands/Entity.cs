using System;
using System.Collections.Generic;

namespace Novels.StoryCommands
{
    public sealed class Entity
    {
        public StoryStepResult ParseStep(string source, StoryContracts.StoryChoice[] choices)
        {
            choices ??= Array.Empty<StoryContracts.StoryChoice>();

            var parseResult = Parse(source, choices.Length > 0);
            if (!parseResult.IsSuccess)
                return StoryStepResult.Failure(parseResult.Error);

            if (choices.Length > 0 && parseResult.Command.Type != StoryCommandType.Dialogue)
            {
                return StoryStepResult.Failure(
                    StoryCommandSyntax.ChoicesWithoutDialogue,
                    "Choices can only belong to a dialogue command.",
                    source);
            }

            return StoryStepResult.Success(new StoryStep(parseResult.Command, choices));
        }

        public StoryParseResult Parse(string source, bool hasChoices)
        {
            source ??= string.Empty;
            var normalizedSource = source.Trim();

            if (normalizedSource.Length == 0)
            {
                var command = hasChoices
                    ? StoryCommand.CreateDialogue(
                        source,
                        string.Empty,
                        string.Empty,
                        StoryContracts.DialoguePresentation.Character,
                        StoryContracts.StoryChoiceAction.None,
                        ParseCharacterPresentation(Array.Empty<string>()))
                    : StoryCommand.CreateEmpty(source);
                return StoryParseResult.Success(command);
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
            {
                return StoryParseResult.Failure(
                    prefixResult.Error.Code,
                    prefixResult.Error.Message,
                    prefixResult.Error.Source);
            }

            var parsedPrefix = prefixResult.Prefix;
            var name = parsedPrefix.Name;
            var arguments = parsedPrefix.Arguments;

            if (StoryCommandSyntax.MetadataNames.Contains(name))
                return StoryParseResult.Success(StoryCommand.CreateMetadata(source));

            if (name.IndexOf(StoryCommandSyntax.Keyboard, StringComparison.OrdinalIgnoreCase) >= 0)
                return StoryParseResult.Success(StoryCommand.CreateKeyboard(source));

            if (!StoryCommandSyntax.CommandTypes.TryGetValue(name, out var commandType))
            {
                return StoryParseResult.Success(StoryCommand.CreateDialogue(
                    source,
                    name,
                    value,
                    ParsePresentation(name, arguments),
                    ParseChoiceActions(arguments),
                    ParseCharacterPresentation(arguments)));
            }

            switch (commandType)
            {
                case StoryCommandType.Notification:
                    return StoryParseResult.Success(StoryCommand.CreateNotification(source, value));

                case StoryCommandType.Location:
                case StoryCommandType.CutScene:
                    return StoryParseResult.Success(StoryCommand.CreateBackground(
                        commandType,
                        source,
                        value,
                        ParseBackgroundPresentation(commandType, arguments)));

                case StoryCommandType.Music:
                case StoryCommandType.Sound:
                case StoryCommandType.Ambient:
                    return StoryParseResult.Success(StoryCommand.CreateAudio(commandType, source, value));

                case StoryCommandType.Camera:
                    if (!TryParseCameraAction(value, out var cameraAction))
                    {
                        return StoryParseResult.Failure(
                            StoryCommandSyntax.UnsupportedCameraAction,
                            $"Unsupported camera action '{value}'.",
                            source);
                    }

                    return StoryParseResult.Success(StoryCommand.CreateCamera(source, cameraAction));

                case StoryCommandType.Wait:
                    if (!int.TryParse(value, out var waitDuration))
                    {
                        return StoryParseResult.Failure(
                            StoryCommandSyntax.InvalidWaitDuration,
                            $"Expected an integer wait duration, got '{value}'.",
                            source);
                    }

                    return StoryParseResult.Success(StoryCommand.CreateWait(source, waitDuration));

                default:
                    throw new ArgumentOutOfRangeException(nameof(commandType), commandType, null);
            }
        }

        private static StoryContracts.DialoguePresentation ParsePresentation(
            string speaker,
            string[] arguments)
        {
            if (HasArgument(arguments, StoryContracts.StoryArguments.Disclaimer))
                return StoryContracts.DialoguePresentation.Disclaimer;

            if (HasArgument(arguments, StoryContracts.StoryArguments.Hint))
                return StoryContracts.DialoguePresentation.Hint;

            if (HasArgument(arguments, StoryContracts.StoryArguments.Thoughts))
                return StoryContracts.DialoguePresentation.Thoughts;

            if (speaker == StoryContracts.StorySpeakers.Narrator)
                return StoryContracts.DialoguePresentation.Narrator;

            if (speaker == StoryContracts.StorySpeakers.Wardrobe)
                return StoryContracts.DialoguePresentation.Wardrobe;

            return StoryContracts.DialoguePresentation.Character;
        }

        private static StoryContracts.StoryChoiceAction ParseChoiceActions(string[] arguments)
        {
            var result = StoryContracts.StoryChoiceAction.None;

            if (HasArgument(arguments, StoryContracts.StoryChoiceActions.SelectAppearance))
                result |= StoryContracts.StoryChoiceAction.SelectAppearance;

            if (HasArgument(arguments, StoryContracts.StoryChoiceActions.SelectClothes))
                result |= StoryContracts.StoryChoiceAction.SelectClothes;

            if (HasArgument(arguments, StoryContracts.StoryChoiceActions.SelectHair)
                || HasArgument(arguments, StoryContracts.StoryChoiceActions.SelectHairLegacy))
            {
                result |= StoryContracts.StoryChoiceAction.SelectHair;
            }

            return result;
        }

        private static StoryContracts.CharacterPresentation ParseCharacterPresentation(string[] arguments)
        {
            var assetCandidates = new List<string>(arguments.Length);

            foreach (var argument in arguments)
            {
                if (!IsDialogueControlArgument(argument))
                    assetCandidates.Add(argument);
            }

            return new StoryContracts.CharacterPresentation(
                HasArgument(arguments, StoryContracts.StoryArguments.Child),
                HasArgument(arguments, StoryContracts.StoryArguments.RemoveClothes),
                HasArgument(arguments, StoryContracts.StoryArguments.RemoveHair)
                    || HasArgument(arguments, StoryContracts.StoryArguments.RemoveHairLegacy),
                HasArgument(arguments, StoryContracts.StoryArguments.RemoveAccessory),
                assetCandidates.ToArray());
        }

        private static StoryContracts.StoryBackgroundPresentation ParseBackgroundPresentation(
            StoryCommandType commandType,
            string[] arguments)
        {
            var type = commandType == StoryCommandType.CutScene
                ? StoryContracts.StoryBackgroundType.CutScene
                : StoryContracts.StoryBackgroundType.Location;
            var color = HasArgument(arguments, StoryContracts.StoryArguments.WhiteBackground)
                ? StoryContracts.StoryBackgroundColor.White
                : StoryContracts.StoryBackgroundColor.Black;
            var keepFinalVideoFrame = type == StoryContracts.StoryBackgroundType.CutScene
                && HasArgument(arguments, StoryContracts.StoryArguments.EndCutScene);

            return new StoryContracts.StoryBackgroundPresentation(
                type,
                color,
                keepFinalVideoFrame);
        }

        private static bool TryParseCameraAction(
            string value,
            out StoryContracts.StoryCameraAction action)
        {
            if (IsArgument(value, StoryContracts.StoryCameraActions.FadeIn))
                action = StoryContracts.StoryCameraAction.FadeIn;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.LeftRight))
                action = StoryContracts.StoryCameraAction.PanLeftToRight;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.RightLeft))
                action = StoryContracts.StoryCameraAction.PanRightToLeft;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.ToCenter))
                action = StoryContracts.StoryCameraAction.MoveToCenter;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.ToLeft))
                action = StoryContracts.StoryCameraAction.MoveToLeft;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.Shaking))
                action = StoryContracts.StoryCameraAction.Shake;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.Injury))
                action = StoryContracts.StoryCameraAction.Injury;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.Splashes))
                action = StoryContracts.StoryCameraAction.Splashes;
            else
            {
                action = default;
                return false;
            }

            return true;
        }

        private static bool IsDialogueControlArgument(string argument)
        {
            return IsArgument(argument, StoryContracts.StoryArguments.Child)
                || IsArgument(argument, StoryContracts.StoryArguments.Disclaimer)
                || IsArgument(argument, StoryContracts.StoryArguments.Hint)
                || IsArgument(argument, StoryContracts.StoryArguments.Thoughts)
                || IsArgument(argument, StoryContracts.StoryArguments.RemoveClothes)
                || IsArgument(argument, StoryContracts.StoryArguments.RemoveHair)
                || IsArgument(argument, StoryContracts.StoryArguments.RemoveHairLegacy)
                || IsArgument(argument, StoryContracts.StoryArguments.RemoveAccessory)
                || IsArgument(argument, StoryContracts.StoryChoiceActions.SelectAppearance)
                || IsArgument(argument, StoryContracts.StoryChoiceActions.SelectClothes)
                || IsArgument(argument, StoryContracts.StoryChoiceActions.SelectHair)
                || IsArgument(argument, StoryContracts.StoryChoiceActions.SelectHairLegacy);
        }

        private static bool HasArgument(string[] arguments, string expected)
        {
            foreach (var argument in arguments)
            {
                if (IsArgument(argument, expected))
                    return true;
            }

            return false;
        }

        private static bool IsArgument(string argument, string expected)
        {
            return string.Equals(argument, expected, StringComparison.OrdinalIgnoreCase);
        }

        private static PrefixParseResult ParsePrefix(string prefix, string source)
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
            var argumentsSource = prefix.Substring(openParenthesisIndex + 1, closeParenthesisIndex - openParenthesisIndex - 1);
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

        private readonly struct ParsedPrefix
        {
            internal ParsedPrefix(string name, string[] arguments)
            {
                Name = name;
                Arguments = arguments;
            }

            internal string Name { get; }
            internal string[] Arguments { get; }
        }

        private readonly struct PrefixParseResult
        {
            private PrefixParseResult(bool isSuccess, ParsedPrefix prefix, StoryParseError error)
            {
                IsSuccess = isSuccess;
                Prefix = prefix;
                Error = error;
            }

            internal bool IsSuccess { get; }
            internal ParsedPrefix Prefix { get; }
            internal StoryParseError Error { get; }

            internal static PrefixParseResult Success(string name, string[] arguments)
            {
                return new PrefixParseResult(true, new ParsedPrefix(name, arguments), default);
            }

            internal static PrefixParseResult Failure(string code, string message, string source)
            {
                return new PrefixParseResult(false, default, new StoryParseError(code, message, source));
            }
        }
    }
}
