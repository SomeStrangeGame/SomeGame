using System;
using System.Collections.Generic;

namespace Novels.StoryCommands
{
    internal static class StoryCommandSyntax
    {
        internal const string Keyboard = "keyboard";
        internal const string InvalidWaitDuration = "INVALID_WAIT_DURATION";
        internal const string InvalidArguments = "INVALID_ARGUMENTS";
        internal const string ChoicesWithoutDialogue = "CHOICES_WITHOUT_DIALOGUE";
        internal const string UnsupportedCameraAction = "UNSUPPORTED_CAMERA_ACTION";

        internal static readonly HashSet<string> MetadataNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "title",
            "series",
            "genres",
            "annotation",
            "stats",
        };

        internal static readonly IReadOnlyDictionary<string, StoryCommandType> CommandTypes =
            new Dictionary<string, StoryCommandType>(StringComparer.OrdinalIgnoreCase)
            {
                ["notification"] = StoryCommandType.Notification,
                ["location"] = StoryCommandType.Location,
                ["cut-scene"] = StoryCommandType.CutScene,
                ["music"] = StoryCommandType.Music,
                ["sound"] = StoryCommandType.Sound,
                ["ambient"] = StoryCommandType.Ambient,
                ["camera"] = StoryCommandType.Camera,
                ["await"] = StoryCommandType.Wait,
            };
    }
}
