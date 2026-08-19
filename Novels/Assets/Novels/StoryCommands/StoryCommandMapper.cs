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

            if (HasArgument(arguments, StoryContracts.StoryArguments.Thoughts))
                return StoryContracts.DialoguePresentation.Thoughts;

            if (speaker == StoryContracts.StorySpeakers.Narrator)
                return StoryContracts.DialoguePresentation.Narrator;

            if (speaker == StoryContracts.StorySpeakers.Wardrobe)
                return StoryContracts.DialoguePresentation.Wardrobe;

            return StoryContracts.DialoguePresentation.Character;
        }

        internal static StoryContracts.StoryChoiceAction ParseChoiceActions(
            string[] arguments)
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

        internal static StoryContracts.CharacterPresentation ParseCharacterPresentation(
            string speaker,
            string[] arguments)
        {
            var assetCandidates = new List<string>(arguments.Length);
            var displayName = ParseDisplayName(speaker, arguments);
            var position = ParseCharacterPosition(arguments);

            for (var index = 0; index < arguments.Length; index++)
            {
                var argument = arguments[index];
                if (!IsDialogueControlArgument(argument)
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
                assetCandidates.ToArray());
        }

        private static string ParseDisplayName(string speaker, string[] arguments)
        {
            if (string.IsNullOrEmpty(speaker)
                || speaker[0] != '{'
                || arguments.Length == 0)
            {
                return string.Empty;
            }

            var candidate = arguments[0]?.Trim();
            if (string.IsNullOrEmpty(candidate)
                || IsDialogueControlArgument(candidate)
                || TryParseCharacterPosition(candidate, out _)
                || candidate[0] == '{'
                || !char.IsUpper(candidate[0]))
            {
                return string.Empty;
            }

            return candidate;
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

        internal static StoryContracts.StoryBackgroundPresentation ParseBackgroundPresentation(
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

        internal static bool TryParseCameraAction(
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
            return string.Equals(
                argument,
                expected,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
