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
                parseResult = Parse(string.Empty, true);
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
                        string.Empty,
                        StoryCommandMapper.ParseCharacterPresentation(
                            string.Empty,
                            Array.Empty<string>()))
                    : StoryCommand.CreateEmpty(source);
                return StoryParseResult.Success(command);
            }

            var separatorIndex = StorySyntaxTokenizer.IndexOfUnescaped(
                normalizedSource,
                ':');
            string prefix;
            string value;
            if (separatorIndex >= 0)
            {
                prefix = normalizedSource.Substring(0, separatorIndex).Trim();
                value = normalizedSource.Substring(separatorIndex + 1).Trim();
            }
            else if (!StoryCommandSyntax.TrySplitMissingSeparator(
                         normalizedSource,
                         out prefix,
                         out value))
            {
                prefix = normalizedSource;
                value = normalizedSource;
            }

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

            if (StoryCommandSyntax.DialogueOnlyNames.Contains(name))
            {
                var text = value;
                if (separatorIndex < 0 || string.IsNullOrWhiteSpace(text))
                    text = arguments.Length == 0 ? string.Empty : arguments[0];
                if (StoryContracts.StorySpeakers.IsWardrobe(name)
                    || StoryContracts.StorySpeakers.IsChoose(name))
                {
                    return StoryParseResult.Success(StoryCommand.CreateDialogue(
                        source,
                        name,
                        text,
                        StoryCommandMapper.ParsePresentation(name, arguments),
                        StoryCommandMapper.ParseChoiceActions(name, arguments),
                        StoryCommandMapper.ParseChoiceConfirmation(name, arguments),
                        StoryCommandMapper.ParseCharacterPresentation(name, arguments)));
                }
                return StoryParseResult.Success(StoryCommand.CreateDialogue(
                    source,
                    name,
                    text,
                    StoryContracts.DialoguePresentation.Narrator,
                    StoryCommandMapper.ParseChoiceActions(name, arguments),
                    string.Empty,
                    StoryCommandMapper.ParseCharacterPresentation(
                        string.Empty,
                        Array.Empty<string>())));
            }

            if (name.IndexOf(StoryCommandSyntax.Keyboard, StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Клавиатура", StringComparison.OrdinalIgnoreCase) >= 0)
                return StoryParseResult.Success(StoryCommand.CreateKeyboard(source));

            if (!StoryCommandSyntax.CommandTypes.TryGetValue(name, out var commandType))
            {
                var dialogueText = value;
                if (string.IsNullOrWhiteSpace(dialogueText)
                    && (StoryContracts.StorySpeakers.IsWardrobe(name)
                        || StoryContracts.StorySpeakers.IsChoose(name))
                    && arguments.Length > 0)
                {
                    dialogueText = arguments[0];
                }
                return StoryParseResult.Success(StoryCommand.CreateDialogue(
                    source,
                    name,
                    dialogueText,
                    StoryCommandMapper.ParsePresentation(name, arguments),
                    StoryCommandMapper.ParseChoiceActions(name, arguments),
                    StoryCommandMapper.ParseChoiceConfirmation(name, arguments),
                    StoryCommandMapper.ParseCharacterPresentation(name, arguments)));
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
                        StoryCommandMapper.NormalizeResourceValue(value),
                        StoryCommandMapper.ParseBackgroundPresentation(commandType, arguments)));

                case StoryCommandType.Music:
                case StoryCommandType.Sound:
                case StoryCommandType.Ambient:
                    return StoryParseResult.Success(StoryCommand.CreateAudio(
                        commandType,
                        source,
                        StoryCommandMapper.NormalizeResourceValue(value)));

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
