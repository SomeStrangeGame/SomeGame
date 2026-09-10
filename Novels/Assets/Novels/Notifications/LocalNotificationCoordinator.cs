using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Novels.Notifications
{
    internal sealed class LocalNotificationCoordinator
    {
        private const string _readingId = "reading-reminder";
        private const string _testId = "development-test";
        private const string _readingTargetKey = "Novels.Notifications.ReadingTarget";
        private const string _publicationIdsKey = "Novels.Notifications.PublicationIds";
        private const string _lastConsumedKey = "Novels.Notifications.LastConsumed";
        private const string _cachedScheduleKey = "Novels.Notifications.CachedSchedule";
        private readonly LocalNotificationPlatform _platform = new();
        private readonly Action<(LogType type, string message)> _onLog;
        private NotificationSchedule _schedule = NotificationSchedule.Defaults();
        private NotificationRoute _readingTarget;
        private Candidate[] _candidates = Array.Empty<Candidate>();

        [Serializable]
        private sealed class IdList { public string[] values; }

        private readonly struct Candidate
        {
            internal Candidate(string storyId, string episodeId)
            {
                StoryId = storyId;
                EpisodeId = episodeId;
            }
            internal string StoryId { get; }
            internal string EpisodeId { get; }
        }

        internal LocalNotificationCoordinator(
            Action<(LogType type, string message)> onLog)
        {
            _onLog = onLog;
            _readingTarget = NotificationRoute.Deserialize(
                PlayerPrefs.GetString(_readingTargetKey, string.Empty));
            var cached = PlayerPrefs.GetString(_cachedScheduleKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(cached))
            {
                try
                {
                    var parsed = JsonUtility.FromJson<NotificationSchedule>(cached);
                    if (parsed == null || parsed.schemaVersion != 1)
                        throw new InvalidOperationException();
                    parsed.readingReminder ??= NotificationSchedule.Defaults().readingReminder;
                    parsed.publications ??= Array.Empty<NotificationSchedule.Publication>();
                    Validate(parsed);
                    _schedule = parsed;
                }
                catch (Exception) { PlayerPrefs.DeleteKey(_cachedScheduleKey); }
            }
        }

        internal void Initialize()
        {
            _platform.Initialize();
            ReconcilePublications();
        }

        internal void SetCatalog(IReadOnlyList<Catalog.NovelCatalogEntry> entries)
        {
            _candidates = entries == null
                ? Array.Empty<Candidate>()
                : entries.Where(entry => entry?.IsEnabled == true)
                    .SelectMany(entry => entry.Episodes.Select(episode =>
                        new Candidate(entry.ContentId, episode.Id)))
                    .ToArray();
        }

        internal void SetReadingTarget(string storyId, string episodeId, string storyTitle)
        {
            _readingTarget = new NotificationRoute
            {
                notificationId = storyTitle ?? string.Empty,
                storyId = storyId,
                episodeId = episodeId,
            };
            PlayerPrefs.SetString(
                _readingTargetKey,
                NotificationRoute.Serialize(storyTitle, storyId, episodeId));
            PlayerPrefs.Save();
        }

        internal void ClearReadingTarget(string storyId, string episodeId)
        {
            if (_readingTarget == null
                || !string.Equals(_readingTarget.storyId, storyId, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(_readingTarget.episodeId, episodeId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            _readingTarget = null;
            PlayerPrefs.DeleteKey(_readingTargetKey);
            PlayerPrefs.Save();
            _platform.Cancel(_readingId);
        }

        internal void ApplyServerSchedule(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return;
            var parsed = JsonUtility.FromJson<NotificationSchedule>(json);
            if (parsed == null || parsed.schemaVersion != 1)
                throw new InvalidOperationException("Unsupported notification schedule schema.");
            parsed.readingReminder ??= NotificationSchedule.Defaults().readingReminder;
            parsed.publications ??= Array.Empty<NotificationSchedule.Publication>();
            Validate(parsed);
            _schedule = parsed;
            PlayerPrefs.SetString(_cachedScheduleKey, json);
            ReconcilePublications();
        }

        internal void OnBackground()
        {
            ScheduleReadingReminder();
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            ScheduleDevelopmentTest();
#endif
        }

        internal NotificationRoute OnForeground()
        {
            _platform.Cancel(_readingId);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            _platform.Cancel(_testId);
#endif
            var route = NotificationRoute.Deserialize(_platform.GetLaunchPayload());
            if (route == null)
                return null;
            if (string.IsNullOrWhiteSpace(route.notificationId))
                return route;
            var last = PlayerPrefs.GetString(_lastConsumedKey, string.Empty);
            if (string.Equals(last, route.notificationId, StringComparison.Ordinal))
                return null;
            PlayerPrefs.SetString(_lastConsumedKey, route.notificationId);
            PlayerPrefs.Save();
            return route;
        }

        private void ScheduleReadingReminder()
        {
            _platform.Cancel(_readingId);
            var config = _schedule.readingReminder;
            if (config?.enabled != true || _readingTarget == null)
                return;
            var now = DateTime.Now;
            var fireTime = now.Date.AddDays(1)
                .AddHours(config.hour)
                .AddMinutes(config.minute);
            var occurrence = $"{_readingId}:{fireTime.Ticks}";
            var title = Text(config.title, "История ждёт продолжения");
            var body = Text(config.body, "Вернитесь к истории и продолжите чтение.")
                .Replace("{storyTitle}", _readingTarget.notificationId ?? string.Empty);
            _platform.Schedule(
                _readingId,
                title,
                body,
                fireTime,
                NotificationRoute.Serialize(
                    occurrence,
                    _readingTarget.storyId,
                    _readingTarget.episodeId));
        }

        private void ScheduleDevelopmentTest()
        {
            _platform.Cancel(_testId);
            if (_candidates.Length == 0)
                return;
            var candidate = _candidates[UnityEngine.Random.Range(0, _candidates.Length)];
            var fireTime = DateTime.Now.AddHours(1);
            _platform.Schedule(
                _testId,
                "Тестовое уведомление",
                "Откройте случайный эпизод в каталоге.",
                fireTime,
                NotificationRoute.Serialize(
                    $"{_testId}:{fireTime.Ticks}",
                    candidate.StoryId,
                    candidate.EpisodeId));
        }

        private void ReconcilePublications()
        {
            foreach (var id in ReadPublicationIds())
                _platform.Cancel(PublicationPlatformId(id));
            var scheduled = new List<string>();
            foreach (var publication in _schedule.publications.Take(64))
            {
                if (publication?.enabled != true
                    || !DateTimeOffset.TryParse(
                        publication.notifyAt,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind,
                        out var timestamp)
                    || timestamp <= DateTimeOffset.Now)
                {
                    continue;
                }
                var platformId = PublicationPlatformId(publication.id);
                _platform.Schedule(
                    platformId,
                    publication.title,
                    publication.body,
                    timestamp.LocalDateTime,
                    NotificationRoute.Serialize(
                        $"{publication.id}:{timestamp.UtcDateTime.Ticks}",
                        publication.storyId,
                        publication.episodeId));
                scheduled.Add(publication.id);
            }
            PlayerPrefs.SetString(
                _publicationIdsKey,
                JsonUtility.ToJson(new IdList { values = scheduled.ToArray() }));
            PlayerPrefs.Save();
        }

        private string[] ReadPublicationIds()
        {
            try
            {
                return JsonUtility.FromJson<IdList>(
                    PlayerPrefs.GetString(_publicationIdsKey, string.Empty))?.values
                    ?? Array.Empty<string>();
            }
            catch (Exception)
            {
                return Array.Empty<string>();
            }
        }

        private static void Validate(NotificationSchedule schedule)
        {
            var reminder = schedule.readingReminder;
            if (reminder.hour is < 0 or > 23 || reminder.minute is < 0 or > 59)
                throw new InvalidOperationException("Reading reminder time is invalid.");
            foreach (var publication in schedule.publications)
            {
                if (publication == null || !publication.enabled)
                    continue;
                if (string.IsNullOrWhiteSpace(publication.id)
                    || string.IsNullOrWhiteSpace(publication.storyId)
                    || string.IsNullOrWhiteSpace(publication.title)
                    || string.IsNullOrWhiteSpace(publication.body)
                    || publication.id.Length > 128
                    || publication.storyId.Length > 128
                    || publication.title.Length > 128
                    || publication.body.Length > 512)
                {
                    throw new InvalidOperationException("Publication notification is invalid.");
                }
            }
        }

        private static string PublicationPlatformId(string id) => $"publication:{id}";

        private static string Text(string value, string fallback) =>
            string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}
