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

            return StoryContracts.DialoguePresentation.Character;
        }

        internal static StoryContracts.StoryChoiceAction ParseChoiceActions(
            string[] arguments)
        {
            var result = StoryContracts.StoryChoiceAction.None;

            if (HasArgument(arguments, StoryContracts.StoryChoiceActions.SelectAppearance)
                || HasArgument(
                    arguments,
                    StoryContracts.StoryChoiceActions.SelectAppearanceFormal))
                result |= StoryContracts.StoryChoiceAction.SelectAppearance;

            if (HasArgument(arguments, StoryContracts.StoryChoiceActions.SelectClothes)
                || HasArgument(
                    arguments,
                    StoryContracts.StoryChoiceActions.SelectClothesFormal)
                || HasArgument(arguments, StoryContracts.StoryChoiceActions.SelectSwimsuit)
                || HasArgument(arguments, StoryContracts.StoryChoiceActions.SelectSwimsuitFormal)
                || HasArgument(arguments, StoryContracts.StoryChoiceActions.ConfirmClothes))
                result |= StoryContracts.StoryChoiceAction.SelectClothes;

            if (HasArgument(arguments, StoryContracts.StoryChoiceActions.SelectHair)
                || HasArgument(arguments, StoryContracts.StoryChoiceActions.SelectHairLegacy)
                || HasArgument(arguments, StoryContracts.StoryChoiceActions.SelectHairFormal)
                || HasArgument(
                    arguments,
                    StoryContracts.StoryChoiceActions.SelectHairFormalLegacy))
            {
                result |= StoryContracts.StoryChoiceAction.SelectHair;
            }

            if (HasArgument(arguments, StoryContracts.StoryChoiceActions.SelectAccessory)
                || HasArgument(
                    arguments,
                    StoryContracts.StoryChoiceActions.SelectAccessoryFormal))
            {
                result |= StoryContracts.StoryChoiceAction.SelectAccessory;
            }

            return result;
        }

        internal static string ParseChoiceConfirmation(string speaker, string[] arguments)
        {
            return StoryContracts.StorySpeakers.IsWardrobe(speaker)
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
                if (StoryContracts.StorySpeakers.IsWardrobe(speaker) && index < 2)
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
            value = NormalizeCameraAction(value);
            if (IsArgument(value, StoryContracts.StoryCameraActions.FadeIn))
                action = StoryContracts.StoryCameraAction.FadeIn;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.FadeInRussian))
                action = StoryContracts.StoryCameraAction.FadeIn;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.LeftRight))
                action = StoryContracts.StoryCameraAction.PanLeftToRight;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.LeftRightRussian))
                action = StoryContracts.StoryCameraAction.PanLeftToRight;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.RightLeft))
                action = StoryContracts.StoryCameraAction.PanRightToLeft;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.RightLeftRussian))
                action = StoryContracts.StoryCameraAction.PanRightToLeft;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.ToCenter))
                action = StoryContracts.StoryCameraAction.MoveToCenter;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.ToCenterRussian))
                action = StoryContracts.StoryCameraAction.MoveToCenter;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.ToLeft))
                action = StoryContracts.StoryCameraAction.MoveToLeft;
            else if (IsArgument(value, StoryContracts.StoryArguments.PositionLeft))
                action = StoryContracts.StoryCameraAction.MoveToLeft;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.ToRight))
                action = StoryContracts.StoryCameraAction.MoveToRight;
            else if (IsArgument(
                         value,
                         StoryContracts.StoryCameraActions.MoveToRightRussian))
                action = StoryContracts.StoryCameraAction.MoveToRight;
            else if (IsArgument(value, StoryContracts.StoryArguments.PositionRight))
                action = StoryContracts.StoryCameraAction.MoveToRight;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.Shaking))
                action = StoryContracts.StoryCameraAction.Shake;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.ShakingRussian)
                || IsArgument(
                    value,
                    StoryContracts.StoryCameraActions.ShakingScreenRussian))
            {
                action = StoryContracts.StoryCameraAction.Shake;
            }
            else if (IsArgument(value, StoryContracts.StoryCameraActions.Injury))
                action = StoryContracts.StoryCameraAction.Injury;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.InjuryRussian))
                action = StoryContracts.StoryCameraAction.Injury;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.Splashes))
                action = StoryContracts.StoryCameraAction.Splashes;
            else if (IsArgument(value, StoryContracts.StoryCameraActions.SplashesRussian)
                || IsArgument(value, StoryContracts.StoryCameraActions.WavesRussian))
            {
                action = StoryContracts.StoryCameraAction.Splashes;
            }
            else if (IsArgument(
                         value,
                         StoryContracts.StoryCameraActions.WhiteFlashRussian)
                || IsArgument(value, StoryContracts.StoryCameraActions.FlashRussian))
            {
                action = StoryContracts.StoryCameraAction.Splashes;
            }
            else
            {
                action = default;
                return false;
            }

            return true;
        }

        private static string NormalizeCameraAction(string value)
        {
            return NormalizeResourceValue(value).TrimEnd('.').TrimEnd();
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
                || IsArgument(argument, StoryContracts.StoryArguments.HideCharacter)
                || IsArgument(argument, StoryContracts.StoryArguments.ShowCharacter)
                || IsArgument(argument, StoryContracts.StoryArguments.ShowCharacterLegacy)
                || IsArgument(argument, StoryContracts.StoryArguments.ChangeClothes)
                || IsArgument(argument, StoryContracts.StoryChoiceActions.SelectAppearance)
                || IsArgument(argument, StoryContracts.StoryChoiceActions.SelectClothes)
                || IsArgument(argument, StoryContracts.StoryChoiceActions.SelectHair)
                || IsArgument(argument, StoryContracts.StoryChoiceActions.SelectHairLegacy);
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
