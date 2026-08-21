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
