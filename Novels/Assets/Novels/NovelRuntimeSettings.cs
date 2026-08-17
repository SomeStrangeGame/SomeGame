using System;
using UnityEngine;

namespace Novels
{
    internal readonly struct NovelRuntimeTuning
    {
        internal NovelRuntimeTuning(
            int targetFrameRate,
            float notificationDurationSeconds,
            int cutSceneFallbackDelayMilliseconds)
        {
            TargetFrameRate = targetFrameRate > 0 ? targetFrameRate : 30;
            NotificationDuration = TimeSpan.FromSeconds(
                notificationDurationSeconds > 0f
                    ? notificationDurationSeconds
                    : 3f);
            CutSceneFallbackDelayMilliseconds =
                cutSceneFallbackDelayMilliseconds > 0
                    ? cutSceneFallbackDelayMilliseconds
                    : 3000;
        }

        internal int TargetFrameRate { get; }
        internal TimeSpan NotificationDuration { get; }
        internal int CutSceneFallbackDelayMilliseconds { get; }
    }

    [CreateAssetMenu(
        fileName = "NovelRuntimeSettings",
        menuName = "Novels/Runtime Settings")]
    public sealed class NovelRuntimeSettings : ScriptableObject
    {
        private const string _resourcePath = "Novels/NovelRuntimeSettings";

        [SerializeField] private int _targetFrameRate = 30;
        [SerializeField] private float _notificationDurationSeconds = 3f;
        [SerializeField] private int _cutSceneFallbackDelayMilliseconds = 3000;

        internal static NovelRuntimeTuning Load()
        {
            var settings = Resources.Load<NovelRuntimeSettings>(_resourcePath);
            return settings == null
                ? new NovelRuntimeTuning(30, 3f, 3000)
                : settings.CreateTuning();
        }

        private NovelRuntimeTuning CreateTuning() =>
            new(
                _targetFrameRate,
                _notificationDurationSeconds,
                _cutSceneFallbackDelayMilliseconds);
    }
}
