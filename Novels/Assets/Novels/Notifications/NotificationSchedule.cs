using System;

namespace Novels.Notifications
{
    [Serializable]
    internal sealed class NotificationSchedule
    {
        public int schemaVersion;
        public string revision;
        public ReadingReminder readingReminder;
        public Publication[] publications;

        [Serializable]
        internal sealed class ReadingReminder
        {
            public bool enabled = true;
            public int hour = 19;
            public int minute;
            public string title = "История ждёт продолжения";
            public string body = "Вернитесь к истории «{storyTitle}» и узнайте, что будет дальше.";
        }

        [Serializable]
        internal sealed class Publication
        {
            public string id;
            public string storyId;
            public string episodeId;
            public string notifyAt;
            public string title;
            public string body;
            public bool enabled = true;
        }

        internal static string FileName(string channel) =>
            $"notifications/{Uri.EscapeDataString(channel ?? string.Empty)}.json";

        internal static NotificationSchedule Defaults() => new()
        {
            schemaVersion = 1,
            readingReminder = new ReadingReminder(),
            publications = Array.Empty<Publication>(),
        };
    }
}
