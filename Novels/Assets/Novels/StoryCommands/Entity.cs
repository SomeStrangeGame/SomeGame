using System;

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
                        StoryCommandMapper.ParseCharacterPresentation(Array.Empty<string>()))
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

            var prefixResult = StoryPrefixParser.Parse(prefix, source);
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
                    StoryCommandMapper.ParsePresentation(name, arguments),
                    StoryCommandMapper.ParseChoiceActions(arguments),
                    StoryCommandMapper.ParseCharacterPresentation(arguments)));
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
                        StoryCommandMapper.ParseBackgroundPresentation(commandType, arguments)));

                case StoryCommandType.Music:
                case StoryCommandType.Sound:
                case StoryCommandType.Ambient:
                    return StoryParseResult.Success(StoryCommand.CreateAudio(commandType, source, value));

                case StoryCommandType.Camera:
                    if (!StoryCommandMapper.TryParseCameraAction(value, out var cameraAction))
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
    }
}
