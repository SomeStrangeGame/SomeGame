using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Novels.Analytics
{
    internal sealed class ProductAnalytics
    {
        private const int SchemaVersion = 1;
        private const int MaximumQueuedEvents = 500;
        private const int BatchSize = 20;
        private const string InstallationIdKey = "novels.analytics.installation-id.v1";
        private const string FirstOpenKey = "novels.analytics.first-open.v1";
        private static readonly int[] ProgressThresholds = {25, 50, 75};
        private static readonly TimeSpan NewSessionAfterBackground = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan DuplicateErrorWindow = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan ErrorRateWindow = TimeSpan.FromMinutes(1);
        private const int MaximumErrorsPerWindow = 8;

        [Serializable]
        private sealed class EventRecord
        {
            public int schema_version;
            public string event_id;
            public string installation_id;
            public string session_id;
            public string event_name;
            public string occurred_at;
            public string app_version;
            public string platform;
            public string story_id;
            public string episode_id;
            public string ending_id;
            public int progress_percent;
            public int duration_seconds;
            public string error_code;
            public string error_context;
        }

        [Serializable]
        private sealed class EventCollection
        {
            public List<EventRecord> events = new();
        }

        [Serializable]
        private sealed class EventBatch
        {
            public int schema_version = SchemaVersion;
            public EventRecord[] events;
        }

        private readonly string _queuePath;
        private readonly string _endpointUrl;
        private readonly string _appVersion;
        private readonly string _platform;
        private readonly bool _deliveryEnabled;
        private readonly CancellationToken _cancellationToken;
        private readonly Action<(LogType type, string message)> _onLog;
        private readonly List<EventRecord> _events;
        private readonly string _installationId;
        private string _sessionId;
        private bool _sending;
        private int _retrySeconds = 2;
        private string _activeStoryId;
        private float _readingStartedAt = -1f;
        private DateTime? _backgroundedAtUtc;
        private readonly Dictionary<string, DateTime> _recentErrors = new(StringComparer.Ordinal);
        private readonly Queue<DateTime> _errorTimestamps = new();
        private bool _capturingLogError;

        internal ProductAnalytics(
            string persistentDataPath,
            string endpointUrl,
            string appVersion,
            string platform,
            bool deliveryEnabled,
            CancellationToken cancellationToken,
            Action<(LogType type, string message)> onLog)
        {
            if (string.IsNullOrWhiteSpace(persistentDataPath))
                throw new ArgumentException("Persistent data path must not be empty.", nameof(persistentDataPath));
            _queuePath = Path.Combine(persistentDataPath, "Analytics", "events-v1.json");
            _endpointUrl = endpointUrl?.Trim() ?? string.Empty;
            _appVersion = appVersion ?? string.Empty;
            _platform = platform ?? string.Empty;
            _deliveryEnabled = deliveryEnabled && IsHttpsUrl(_endpointUrl);
            _cancellationToken = cancellationToken;
            _onLog = onLog;
            _events = LoadQueue();
            _installationId = LoadOrCreateInstallationId();
        }

        internal void StartSession()
        {
            _sessionId = Guid.NewGuid().ToString("N");
            if (PlayerPrefs.GetInt(FirstOpenKey, 0) == 0)
            {
                PlayerPrefs.SetInt(FirstOpenKey, 1);
                PlayerPrefs.Save();
                Enqueue("app_first_open");
            }
            Enqueue("session_started");
            StartSending();
        }

        internal void CatalogOpened() => Enqueue("catalog_opened");

        internal void StoryOpened(string storyId, string episodeId) =>
            Enqueue("story_opened", storyId, episodeId);

        internal void StoryStarted(string storyId, string episodeId)
        {
            StopReading();
            _activeStoryId = storyId ?? string.Empty;
            _readingStartedAt = Time.realtimeSinceStartup;
            Enqueue("story_started", storyId, episodeId);
        }

        internal void StoryProgress(string storyId, string episodeId, float ratio)
        {
            foreach (var percent in ProgressThresholds)
            {
                if (ratio < percent / 100f) continue;
                var key = ProgressKey(storyId, episodeId, percent);
                if (PlayerPrefs.GetInt(key, 0) != 0) continue;
                PlayerPrefs.SetInt(key, 1);
                PlayerPrefs.Save();
                Enqueue("story_progress", storyId, episodeId, progressPercent: percent);
            }
        }

        internal void EpisodeCompleted(string storyId, string episodeId, bool storyCompleted)
        {
            StopReading();
            if (!storyCompleted) return;
            var durationKey = DurationKey(storyId);
            var duration = Mathf.Max(0, Mathf.RoundToInt(PlayerPrefs.GetFloat(durationKey, 0f)));
            PlayerPrefs.DeleteKey(durationKey);
            PlayerPrefs.Save();
            Enqueue("story_completed", storyId, episodeId, durationSeconds: duration);
        }

        internal void EndingReached(string storyId, string episodeId, string endingId) =>
            Enqueue("ending_reached", storyId, episodeId, endingId: endingId);

        internal void FeedbackOpened(string storyId = null) =>
            Enqueue("feedback_opened", storyId);

        internal void RuntimeError(
            string errorCode,
            string storyId,
            string episodeId,
            string safeContext) =>
            Enqueue(
                "runtime_error",
                storyId,
                episodeId,
                errorCode: errorCode,
                errorContext: SafeValue(safeContext, 160));

        internal void CaptureUnityError(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Assert && type != LogType.Exception)
                return;
            if (_capturingLogError)
                return;

            var now = DateTime.UtcNow;
            while (_errorTimestamps.Count > 0 && now - _errorTimestamps.Peek() >= ErrorRateWindow)
                _errorTimestamps.Dequeue();
            if (_errorTimestamps.Count >= MaximumErrorsPerWindow)
                return;

            var fingerprint = ErrorFingerprint(condition, stackTrace, type);
            if (_recentErrors.TryGetValue(fingerprint, out var lastSeen)
                && now - lastSeen < DuplicateErrorWindow)
            {
                return;
            }
            if (_recentErrors.Count >= 128)
            {
                foreach (var expired in _recentErrors
                             .Where(item => now - item.Value >= DuplicateErrorWindow)
                             .Select(item => item.Key)
                             .ToArray())
                {
                    _recentErrors.Remove(expired);
                }
                if (_recentErrors.Count >= 128)
                    _recentErrors.Clear();
            }

            _recentErrors[fingerprint] = now;
            _errorTimestamps.Enqueue(now);
            _capturingLogError = true;
            try
            {
                RuntimeError(
                    type == LogType.Exception ? "unity_exception"
                    : type == LogType.Assert ? "unity_assert"
                    : "unity_error",
                    null,
                    null,
                    $"{type}:{fingerprint}");
            }
            finally
            {
                _capturingLogError = false;
            }
        }

        internal void PauseReading()
        {
            StoreReadingDuration();
            _readingStartedAt = -1f;
        }

        internal void StopReading()
        {
            StoreReadingDuration();
            _activeStoryId = string.Empty;
            _readingStartedAt = -1f;
        }

        internal void ResumeReading()
        {
            if (!string.IsNullOrWhiteSpace(_activeStoryId) && _readingStartedAt < 0f)
                _readingStartedAt = Time.realtimeSinceStartup;
            StartSending();
        }

        internal void ApplicationPaused()
        {
            PauseReading();
            _backgroundedAtUtc = DateTime.UtcNow;
        }

        internal void ApplicationResumed()
        {
            if (_backgroundedAtUtc.HasValue
                && DateTime.UtcNow - _backgroundedAtUtc.Value >= NewSessionAfterBackground)
            {
                _sessionId = Guid.NewGuid().ToString("N");
                Enqueue("session_started");
            }
            _backgroundedAtUtc = null;
            ResumeReading();
        }

        internal void FlushSynchronously()
        {
            StoreReadingDuration();
            SaveQueue();
        }

        private void Enqueue(
            string eventName,
            string storyId = null,
            string episodeId = null,
            string endingId = null,
            int progressPercent = 0,
            int durationSeconds = 0,
            string errorCode = null,
            string errorContext = null)
        {
            _events.Add(new EventRecord
            {
                schema_version = SchemaVersion,
                event_id = Guid.NewGuid().ToString("N"),
                installation_id = _installationId,
                session_id = _sessionId ?? string.Empty,
                event_name = eventName,
                occurred_at = DateTime.UtcNow.ToString("O"),
                app_version = _appVersion,
                platform = _platform,
                story_id = SafeValue(storyId, 80),
                episode_id = SafeValue(episodeId, 80),
                ending_id = SafeValue(endingId, 80),
                progress_percent = progressPercent,
                duration_seconds = durationSeconds,
                error_code = SafeValue(errorCode, 80),
                error_context = SafeValue(errorContext, 160),
            });
            if (_events.Count > MaximumQueuedEvents)
                _events.RemoveRange(0, _events.Count - MaximumQueuedEvents);
            SaveQueue();
            StartSending();
        }

        private void StartSending()
        {
            if (!_deliveryEnabled || _sending || _events.Count == 0
                || _cancellationToken.IsCancellationRequested)
            {
                return;
            }
            SendLoop().Forget();
        }

        private async UniTaskVoid SendLoop()
        {
            _sending = true;
            try
            {
                while (_events.Count > 0 && !_cancellationToken.IsCancellationRequested)
                {
                    var batch = _events.Take(BatchSize).ToArray();
                    try
                    {
                        var payload = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new EventBatch
                        {
                            events = batch,
                        }));
                        using var request = new UnityWebRequest(_endpointUrl, UnityWebRequest.kHttpVerbPOST)
                        {
                            uploadHandler = new UploadHandlerRaw(payload),
                            downloadHandler = new DownloadHandlerBuffer(),
                            timeout = 15,
                        };
                        request.SetRequestHeader("Content-Type", "application/json");
                        await request.SendWebRequest().ToUniTask(cancellationToken: _cancellationToken);
                        if (request.responseCode < 200 || request.responseCode >= 300)
                            throw new InvalidOperationException($"HTTP {request.responseCode}");
                        var sentIds = new HashSet<string>(batch.Select(item => item.event_id));
                        _events.RemoveAll(item => sentIds.Contains(item.event_id));
                        _retrySeconds = 2;
                        SaveQueue();
                    }
                    catch (OperationCanceledException) when (_cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    catch (Exception exception)
                    {
                        _onLog?.Invoke((LogType.Warning,
                            $"Product analytics delivery deferred: {exception.Message}"));
                        await UniTask.Delay(
                            TimeSpan.FromSeconds(_retrySeconds),
                            cancellationToken: _cancellationToken);
                        _retrySeconds = Math.Min(300, _retrySeconds * 2);
                    }
                }
            }
            catch (OperationCanceledException) when (_cancellationToken.IsCancellationRequested)
            {
            }
            finally
            {
                _sending = false;
            }
        }

        private void StoreReadingDuration()
        {
            if (string.IsNullOrWhiteSpace(_activeStoryId) || _readingStartedAt < 0f) return;
            var elapsed = Mathf.Max(0f, Time.realtimeSinceStartup - _readingStartedAt);
            var key = DurationKey(_activeStoryId);
            PlayerPrefs.SetFloat(key, PlayerPrefs.GetFloat(key, 0f) + elapsed);
            PlayerPrefs.Save();
            _readingStartedAt = Time.realtimeSinceStartup;
        }

        private List<EventRecord> LoadQueue()
        {
            try
            {
                var source = File.Exists(_queuePath) ? _queuePath
                    : File.Exists(_queuePath + ".next") ? _queuePath + ".next"
                    : File.Exists(_queuePath + ".previous") ? _queuePath + ".previous"
                    : null;
                if (source == null) return new List<EventRecord>();
                var collection = JsonUtility.FromJson<EventCollection>(File.ReadAllText(source));
                var valid = collection?.events?.Where(item => item != null).ToList()
                    ?? new List<EventRecord>();
                return valid.Skip(Math.Max(0, valid.Count - MaximumQueuedEvents)).ToList();
            }
            catch (Exception exception)
            {
                _onLog?.Invoke((LogType.Warning,
                    $"Product analytics queue could not be loaded: {exception.Message}"));
                return new List<EventRecord>();
            }
        }

        private void SaveQueue()
        {
            try
            {
                var directory = Path.GetDirectoryName(_queuePath);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                var temporary = _queuePath + ".next";
                var previous = _queuePath + ".previous";
                File.WriteAllText(temporary, JsonUtility.ToJson(new EventCollection {events = _events}));
                if (File.Exists(previous)) File.Delete(previous);
                if (File.Exists(_queuePath)) File.Move(_queuePath, previous);
                File.Move(temporary, _queuePath);
                if (File.Exists(previous)) File.Delete(previous);
            }
            catch (Exception exception)
            {
                _onLog?.Invoke((LogType.Warning,
                    $"Product analytics queue could not be saved: {exception.Message}"));
            }
        }

        private static string LoadOrCreateInstallationId()
        {
            var value = PlayerPrefs.GetString(InstallationIdKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(value)) return value;
            value = Guid.NewGuid().ToString("N");
            PlayerPrefs.SetString(InstallationIdKey, value);
            PlayerPrefs.Save();
            return value;
        }

        private static bool IsHttpsUrl(string value) =>
            Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && uri.Scheme == Uri.UriSchemeHttps;

        private static string SafeValue(string value, int maximumLength)
        {
            value = value?.Trim() ?? string.Empty;
            return value.Length <= maximumLength ? value : value.Substring(0, maximumLength);
        }

        private static string ErrorFingerprint(string condition, string stackTrace, LogType type)
        {
            using var algorithm = SHA256.Create();
            var value = $"{type}\n{condition ?? string.Empty}\n{stackTrace ?? string.Empty}";
            return BitConverter.ToString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(value)))
                .Replace("-", string.Empty)
                .ToLowerInvariant()
                .Substring(0, 16);
        }

        private static string ProgressKey(string storyId, string episodeId, int percent) =>
            $"novels.analytics.progress.v1.{storyId}.{episodeId}.{percent}";

        private static string DurationKey(string storyId) =>
            $"novels.analytics.duration.v1.{storyId}";
    }
}
