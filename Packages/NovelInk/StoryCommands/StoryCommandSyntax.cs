using System;
using System.Collections.Generic;
using System.Linq;

namespace Novels.StoryCommands
{
    internal static class StoryCommandSyntax
    {
        internal const string Keyboard = "keyboard";
        internal const string InvalidWaitDuration = "INVALID_WAIT_DURATION";
        internal const string InvalidArguments = "INVALID_ARGUMENTS";
        internal const string UnsupportedCameraAction = "UNSUPPORTED_CAMERA_ACTION";

        internal static readonly HashSet<string> MetadataNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "title",
            "Название",
            "series",
            "Серия",
            "genres",
            "Жанры",
            "annotation",
            "Аннотация",
            "stats",
            "Статы",
        };

        internal static readonly HashSet<string> DialogueOnlyNames = new(
            StringComparer.OrdinalIgnoreCase)
        {
            StoryContracts.StorySpeakers.WardrobeRussian,
            StoryContracts.StorySpeakers.ChooseRussian,
            "КОНЕЦ СЕРИИ",
        };

        internal static readonly IReadOnlyDictionary<string, StoryCommandType> CommandTypes =
            new Dictionary<string, StoryCommandType>(StringComparer.OrdinalIgnoreCase)
            {
                ["notification"] = StoryCommandType.Notification,
                ["Уведомление"] = StoryCommandType.Notification,
                ["Уведомления"] = StoryCommandType.Notification,
                ["location"] = StoryCommandType.Location,
                ["Локация"] = StoryCommandType.Location,
                ["cut-scene"] = StoryCommandType.CutScene,
                ["Кат-сцена"] = StoryCommandType.CutScene,
                ["music"] = StoryCommandType.Music,
                ["Музыка"] = StoryCommandType.Music,
                ["sound"] = StoryCommandType.Sound,
                ["Звук"] = StoryCommandType.Sound,
                ["ambient"] = StoryCommandType.Ambient,
                ["Звуки окружения"] = StoryCommandType.Ambient,
                ["Звуковое окружение"] = StoryCommandType.Ambient,
                ["camera"] = StoryCommandType.Camera,
                ["Камера"] = StoryCommandType.Camera,
                ["await"] = StoryCommandType.Wait,
                ["Ожидание"] = StoryCommandType.Wait,
            };

        internal static readonly IReadOnlyDictionary<
            string,
            StoryContracts.StoryChoiceAction> ChoiceActions =
            new Dictionary<string, StoryContracts.StoryChoiceAction>(
                StringComparer.OrdinalIgnoreCase)
            {
                [StoryContracts.StoryChoiceActions.SelectAppearance] =
                    StoryContracts.StoryChoiceAction.SelectAppearance,
                [StoryContracts.StoryChoiceActions.SelectAppearanceFormal] =
                    StoryContracts.StoryChoiceAction.SelectAppearance,
                [StoryContracts.StoryChoiceActions.SelectClothes] =
                    StoryContracts.StoryChoiceAction.SelectClothes,
                [StoryContracts.StoryChoiceActions.SelectClothesFormal] =
                    StoryContracts.StoryChoiceAction.SelectClothes,
                [StoryContracts.StoryChoiceActions.SelectSwimsuit] =
                    StoryContracts.StoryChoiceAction.SelectClothes,
                [StoryContracts.StoryChoiceActions.SelectSwimsuitFormal] =
                    StoryContracts.StoryChoiceAction.SelectClothes,
                [StoryContracts.StoryChoiceActions.ConfirmClothes] =
                    StoryContracts.StoryChoiceAction.SelectClothes,
                [StoryContracts.StoryChoiceActions.SelectHair] =
                    StoryContracts.StoryChoiceAction.SelectHair,
                [StoryContracts.StoryChoiceActions.SelectHairLegacy] =
                    StoryContracts.StoryChoiceAction.SelectHair,
                [StoryContracts.StoryChoiceActions.SelectHairFormal] =
                    StoryContracts.StoryChoiceAction.SelectHair,
                [StoryContracts.StoryChoiceActions.SelectHairFormalLegacy] =
                    StoryContracts.StoryChoiceAction.SelectHair,
                [StoryContracts.StoryChoiceActions.SelectAccessory] =
                    StoryContracts.StoryChoiceAction.SelectAccessory,
                [StoryContracts.StoryChoiceActions.SelectAccessoryFormal] =
                    StoryContracts.StoryChoiceAction.SelectAccessory,
            };

        internal static readonly HashSet<string> DialogueControlArguments = new(
            StringComparer.OrdinalIgnoreCase)
        {
            StoryContracts.StoryArguments.Child,
            StoryContracts.StoryArguments.Disclaimer,
            StoryContracts.StoryArguments.Hint,
            StoryContracts.StoryArguments.Thoughts,
            StoryContracts.StoryArguments.RemoveClothes,
            StoryContracts.StoryArguments.RemoveHair,
            StoryContracts.StoryArguments.RemoveHairLegacy,
            StoryContracts.StoryArguments.RemoveAccessory,
            StoryContracts.StoryArguments.HideCharacter,
            StoryContracts.StoryArguments.ShowCharacter,
            StoryContracts.StoryArguments.ShowCharacterLegacy,
            StoryContracts.StoryArguments.ChangeClothes,
            StoryContracts.StoryChoiceActions.SelectAppearance,
            StoryContracts.StoryChoiceActions.SelectClothes,
            StoryContracts.StoryChoiceActions.SelectHair,
            StoryContracts.StoryChoiceActions.SelectHairLegacy,
        };

        internal static readonly IReadOnlyDictionary<string, StoryContracts.StoryCameraAction>
            CameraActions = new Dictionary<string, StoryContracts.StoryCameraAction>(
                StringComparer.OrdinalIgnoreCase)
            {
                [StoryContracts.StoryCameraActions.FadeIn] = StoryContracts.StoryCameraAction.FadeIn,
                [StoryContracts.StoryCameraActions.FadeInRussian] = StoryContracts.StoryCameraAction.FadeIn,
                [StoryContracts.StoryCameraActions.LeftRight] = StoryContracts.StoryCameraAction.PanLeftToRight,
                [StoryContracts.StoryCameraActions.LeftRightRussian] = StoryContracts.StoryCameraAction.PanLeftToRight,
                [StoryContracts.StoryCameraActions.RightLeft] = StoryContracts.StoryCameraAction.PanRightToLeft,
                [StoryContracts.StoryCameraActions.RightLeftRussian] = StoryContracts.StoryCameraAction.PanRightToLeft,
                [StoryContracts.StoryCameraActions.ToCenter] = StoryContracts.StoryCameraAction.MoveToCenter,
                [StoryContracts.StoryCameraActions.ToCenterRussian] = StoryContracts.StoryCameraAction.MoveToCenter,
                [StoryContracts.StoryCameraActions.ToLeft] = StoryContracts.StoryCameraAction.MoveToLeft,
                [StoryContracts.StoryArguments.PositionLeft] = StoryContracts.StoryCameraAction.MoveToLeft,
                [StoryContracts.StoryCameraActions.ToRight] = StoryContracts.StoryCameraAction.MoveToRight,
                [StoryContracts.StoryCameraActions.MoveToRightRussian] = StoryContracts.StoryCameraAction.MoveToRight,
                [StoryContracts.StoryArguments.PositionRight] = StoryContracts.StoryCameraAction.MoveToRight,
                [StoryContracts.StoryCameraActions.Shaking] = StoryContracts.StoryCameraAction.Shake,
                [StoryContracts.StoryCameraActions.ShakingRussian] = StoryContracts.StoryCameraAction.Shake,
                [StoryContracts.StoryCameraActions.ShakingScreenRussian] = StoryContracts.StoryCameraAction.Shake,
                [StoryContracts.StoryCameraActions.Injury] = StoryContracts.StoryCameraAction.Injury,
                [StoryContracts.StoryCameraActions.InjuryRussian] = StoryContracts.StoryCameraAction.Injury,
                [StoryContracts.StoryCameraActions.Splashes] = StoryContracts.StoryCameraAction.Splashes,
                [StoryContracts.StoryCameraActions.SplashesRussian] = StoryContracts.StoryCameraAction.Splashes,
                [StoryContracts.StoryCameraActions.WavesRussian] = StoryContracts.StoryCameraAction.Splashes,
                [StoryContracts.StoryCameraActions.WhiteFlashRussian] = StoryContracts.StoryCameraAction.Splashes,
                [StoryContracts.StoryCameraActions.FlashRussian] = StoryContracts.StoryCameraAction.Splashes,
            };

        internal static bool TrySplitMissingSeparator(
            string source,
            out string command,
            out string value)
        {
            foreach (var candidate in CommandTypes.Keys
                         .OrderByDescending(name => name.Length))
            {
                if (!source.StartsWith(candidate, StringComparison.OrdinalIgnoreCase)
                    || source.Length == candidate.Length
                    || !char.IsWhiteSpace(source[candidate.Length]))
                {
                    continue;
                }
                command = candidate;
                value = source.Substring(candidate.Length).Trim();
                return true;
            }
            command = string.Empty;
            value = string.Empty;
            return false;
        }
    }
}
