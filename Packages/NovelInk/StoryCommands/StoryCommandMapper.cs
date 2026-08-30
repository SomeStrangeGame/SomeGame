using System;
using System.Collections.Generic;

namespace Novels.StoryCommands
{
    internal static class StoryCommandMapper
    {
        internal static StoryContracts.DialoguePresentation ParsePresentation(
            string speaker,
            string[] arguments)
        {
            if (HasArgument(arguments, StoryContracts.StoryArguments.Disclaimer))
                return StoryContracts.DialoguePresentation.Disclaimer;

            if (HasArgument(arguments, StoryContracts.StoryArguments.Hint))
                return StoryContracts.DialoguePresentation.Hint;

            if (HasArgument(arguments, StoryContracts.StoryArguments.Thoughts)
                || HasPrefixedArgument(arguments, StoryContracts.StoryArguments.Thoughts))
                return StoryContracts.DialoguePresentation.Thoughts;

            if (StoryContracts.StorySpeakers.IsNarrator(speaker))
                return StoryContracts.DialoguePresentation.Narrator;

            if (StoryContracts.StorySpeakers.IsWardrobe(speaker))
                return StoryContracts.DialoguePresentation.Wardrobe;

            if (StoryContracts.StorySpeakers.IsChoose(speaker))
                return StoryContracts.DialoguePresentation.Choose;

            return StoryContracts.DialoguePresentation.Character;
        }

        internal static StoryContracts.StoryChoiceAction ParseChoiceActions(
            string[] arguments)
        {
            var result = StoryContracts.StoryChoiceAction.None;

            foreach (var argument in arguments)
                if (!string.IsNullOrEmpty(argument)
                    && StoryCommandSyntax.ChoiceActions.TryGetValue(argument, out var action))
                    result |= action;

            return result;
        }

        internal static StoryContracts.StoryChoiceAction ParseChoiceActions(
            string speaker,
            string[] arguments)
        {
            var result = ParseChoiceActions(arguments);
            if (result == StoryContracts.StoryChoiceAction.None
                && StoryContracts.StorySpeakers.IsChoose(speaker)
                && arguments.Length > 1
                && string.Equals(
                    arguments[1]?.Trim(),
                    "Надеть",
                    StringComparison.OrdinalIgnoreCase))
            {
                result = StoryContracts.StoryChoiceAction.SelectAccessory;
            }
            return result;
        }

        internal static string ParseChoiceConfirmation(string speaker, string[] arguments)
        {
            return (StoryContracts.StorySpeakers.IsWardrobe(speaker)
                    || StoryContracts.StorySpeakers.IsChoose(speaker))
                && arguments.Length > 1
                ? arguments[1]?.Trim() ?? string.Empty
                : string.Empty;
        }

        internal static StoryContracts.CharacterPresentation ParseCharacterPresentation(
            string speaker,
            string[] arguments)
        {
            var assetCandidates = new List<string>(arguments.Length);
            var displayName = ParseDisplayName(speaker, arguments, out var displayNameIndex);
            var position = ParseCharacterPosition(arguments);
            var visibility = ParseCharacterVisibility(arguments);
            var hasUnsupportedTimedChoice = false;

            for (var index = 0; index < arguments.Length; index++)
            {
                var argument = arguments[index];
                if ((StoryContracts.StorySpeakers.IsWardrobe(speaker)
                        || StoryContracts.StorySpeakers.IsChoose(speaker))
                    && index < 2)
                    continue;
                if (index == displayNameIndex)
                    continue;
                if (TryStripPrefix(
                        argument,
                        StoryContracts.StoryArguments.Thoughts,
                        out var prefixedCandidate))
                {
                    assetCandidates.Add(prefixedCandidate);
                    continue;
                }
                if (IsUnsupportedTimedChoiceArgument(argument))
                {
                    hasUnsupportedTimedChoice = true;
                }
                else if (!IsDialogueControlArgument(argument)
                    && !TryParseCharacterPosition(argument, out _)
                    && (index != 0 || string.IsNullOrEmpty(displayName)))
                {
                    assetCandidates.Add(argument);
                }
            }

            return new StoryContracts.CharacterPresentation(
                HasArgument(arguments, StoryContracts.StoryArguments.Child),
                HasArgument(arguments, StoryContracts.StoryArguments.RemoveClothes),
                HasArgument(arguments, StoryContracts.StoryArguments.RemoveHair)
                    || HasArgument(arguments, StoryContracts.StoryArguments.RemoveHairLegacy),
                HasArgument(arguments, StoryContracts.StoryArguments.RemoveAccessory),
                displayName,
                position,
                visibility,
                hasUnsupportedTimedChoice,
                assetCandidates.ToArray());
        }

        private static string ParseDisplayName(
            string speaker,
            string[] arguments,
            out int displayNameIndex)
        {
            displayNameIndex = -1;
            if (string.IsNullOrEmpty(speaker) || arguments.Length == 0)
            {
                return string.Empty;
            }

            for (var index = 0; index < arguments.Length; index++)
            {
                var candidate = arguments[index]?.Trim();
                if (string.IsNullOrEmpty(candidate)
                    || IsDialogueControlArgument(candidate)
                    || TryParseCharacterPosition(candidate, out _)
                    || candidate[0] == '{'
                    || !StoryContracts.StoryDisplayNames.IsKnown(candidate))
                {
                    continue;
                }
                displayNameIndex = index;
                return candidate;
            }
            return string.Empty;
        }

        private static StoryContracts.StoryCharacterPosition? ParseCharacterPosition(
            string[] arguments)
        {
            foreach (var argument in arguments)
            {
                if (TryParseCharacterPosition(argument, out var position))
                    return position;
            }

            return null;
        }

        private static bool TryParseCharacterPosition(
            string argument,
            out StoryContracts.StoryCharacterPosition position)
        {
            if (IsArgument(argument, StoryContracts.StoryArguments.PositionLeft))
                position = StoryContracts.StoryCharacterPosition.Left;
            else if (IsArgument(argument, StoryContracts.StoryArguments.PositionRight))
                position = StoryContracts.StoryCharacterPosition.Right;
            else if (IsArgument(argument, StoryContracts.StoryArguments.PositionCenter))
                position = StoryContracts.StoryCharacterPosition.Center;
            else
            {
                position = default;
                return false;
            }

            return true;
        }

        private static StoryContracts.StoryCharacterVisibilityCommand ParseCharacterVisibility(
            string[] arguments)
        {
            if (HasArgument(arguments, StoryContracts.StoryArguments.HideCharacter))
                return StoryContracts.StoryCharacterVisibilityCommand.Hide;

            if (HasArgument(arguments, StoryContracts.StoryArguments.ShowCharacter)
                || HasArgument(
                    arguments,
                    StoryContracts.StoryArguments.ShowCharacterLegacy))
                return StoryContracts.StoryCharacterVisibilityCommand.Show;

            return StoryContracts.StoryCharacterVisibilityCommand.Unchanged;
        }

        internal static StoryContracts.StoryBackgroundPresentation ParseBackgroundPresentation(
            StoryCommandType commandType,
            string[] arguments)
        {
            var type = commandType == StoryCommandType.CutScene
                ? StoryContracts.StoryBackgroundType.CutScene
                : StoryContracts.StoryBackgroundType.Location;
            var color = HasArgument(arguments, StoryContracts.StoryArguments.WhiteBackground)
                || HasArgument(
                    arguments,
                    StoryContracts.StoryArguments.WhiteBackgroundRussian)
                ? StoryContracts.StoryBackgroundColor.White
                : StoryContracts.StoryBackgroundColor.Black;
            var keepFinalVideoFrame = type == StoryContracts.StoryBackgroundType.CutScene
                && HasArgument(arguments, StoryContracts.StoryArguments.EndCutScene);

            return new StoryContracts.StoryBackgroundPresentation(
                type,
                color,
                keepFinalVideoFrame);
        }

        internal static string NormalizeResourceValue(string value)
        {
            value ??= string.Empty;
            var comment = value.IndexOf(
                StoryContracts.StorySyntaxTokens.InlineComment,
                StringComparison.Ordinal);
            if (comment >= 0)
                value = value.Substring(0, comment);
            return value.Trim();
        }

        internal static bool TryParseCameraAction(
            string value,
            out StoryContracts.StoryCameraAction action)
        {
            return StoryCommandSyntax.CameraActions.TryGetValue(
                NormalizeCameraAction(value),
                out action);
        }

        private static string NormalizeCameraAction(string value)
        {
            return NormalizeResourceValue(value).TrimEnd('.').TrimEnd();
        }

        private static bool IsDialogueControlArgument(string argument)
        {
            return !string.IsNullOrEmpty(argument)
                && StoryCommandSyntax.DialogueControlArguments.Contains(argument);
        }

        private static bool IsUnsupportedTimedChoiceArgument(string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
                return false;

            var prefix = StoryContracts.StoryArguments.TimedChoicePrefix;
            return argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                && (argument.Length == prefix.Length
                    || char.IsWhiteSpace(argument[prefix.Length]));
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

        private static bool HasPrefixedArgument(string[] arguments, string prefix)
        {
            foreach (var argument in arguments)
            {
                if (TryStripPrefix(argument, prefix, out _))
                    return true;
            }
            return false;
        }

        private static bool TryStripPrefix(
            string argument,
            string prefix,
            out string remainder)
        {
            remainder = string.Empty;
            if (string.IsNullOrWhiteSpace(argument)
                || !argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                || argument.Length == prefix.Length
                || !char.IsWhiteSpace(argument[prefix.Length]))
            {
                return false;
            }
            remainder = argument.Substring(prefix.Length).Trim();
            return remainder.Length > 0;
        }

        private static bool IsArgument(string argument, string expected)
        {
            return string.Equals(
                argument,
                expected,
                StringComparison.OrdinalIgnoreCase);
        }

    }
}
