using System;
using UnityEngine;

namespace Novels
{
    internal readonly struct NovelRuntimeTuning
    {
        internal NovelRuntimeTuning(
            int targetFrameRate,
            float notificationDurationSeconds,
            int cutSceneFallbackDelayMilliseconds,
            Bundles.ContentDeliveryOptions contentDelivery)
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
            ContentDelivery = contentDelivery
                ?? Bundles.ContentDeliveryOptions.Default;
        }

        internal int TargetFrameRate { get; }
        internal TimeSpan NotificationDuration { get; }
        internal int CutSceneFallbackDelayMilliseconds { get; }
        internal Bundles.ContentDeliveryOptions ContentDelivery { get; }
    }

    [CreateAssetMenu(
        fileName = "NovelRuntimeSettings",
        menuName = "Novels/Runtime Settings")]
    public sealed class NovelRuntimeSettings : ScriptableObject
    {
        private const string _resourcePath = "Novels/NovelRuntimeSettings";
        private const int _defaultContentCacheLimitMegabytes = 768;

        [SerializeField] private int _targetFrameRate = 30;
        [SerializeField] private float _notificationDurationSeconds = 3f;
        [SerializeField] private int _cutSceneFallbackDelayMilliseconds = 3000;

        [Header("Content Delivery")]
        [SerializeField] private int _contentCacheLimitMegabytes =
            _defaultContentCacheLimitMegabytes;
        [SerializeField] private int _maximumParallelDownloads = 3;
        [SerializeField] private int _stagingLifetimeHours = 24;
        [SerializeField] private int _remoteMaximumAttempts = 3;
        [SerializeField] private int _remoteTimeoutSeconds = 30;
        [SerializeField] private int _remoteInitialRetryDelayMilliseconds = 500;
        [SerializeField] private int _remoteMaximumRetryDelayMilliseconds = 4000;

        internal static NovelRuntimeTuning Load()
        {
            var settings = Resources.Load<NovelRuntimeSettings>(_resourcePath);
            return settings == null
                ? new NovelRuntimeTuning(
                    30,
                    3f,
                    3000,
                    CreateDefaultContentDeliveryOptions())
                : settings.CreateTuning();
        }

        private static Bundles.ContentDeliveryOptions
            CreateDefaultContentDeliveryOptions()
        {
            var defaults = Bundles.ContentDeliveryOptions.Default;
            return new Bundles.ContentDeliveryOptions(
                _defaultContentCacheLimitMegabytes * 1024L * 1024L,
                defaults.MaximumParallelDownloads,
                defaults.StagingLifetime,
                defaults.RemoteRequestPolicy,
                defaults.LocalRequestPolicy);
        }

        private NovelRuntimeTuning CreateTuning() =>
            new(
                _targetFrameRate,
                _notificationDurationSeconds,
                _cutSceneFallbackDelayMilliseconds,
                CreateContentDeliveryOptions());

        private Bundles.ContentDeliveryOptions CreateContentDeliveryOptions()
        {
            var defaults = Bundles.ContentDeliveryOptions.Default;
            var cacheMegabytes = _contentCacheLimitMegabytes > 0
                ? _contentCacheLimitMegabytes
                : _defaultContentCacheLimitMegabytes;
            var parallelDownloads = _maximumParallelDownloads > 0
                ? _maximumParallelDownloads
                : defaults.MaximumParallelDownloads;
            var stagingHours = _stagingLifetimeHours > 0
                ? _stagingLifetimeHours
                : (int)defaults.StagingLifetime.TotalHours;
            var maximumAttempts = _remoteMaximumAttempts > 0
                ? _remoteMaximumAttempts
                : defaults.RemoteRequestPolicy.MaximumAttempts;
            var timeoutSeconds = _remoteTimeoutSeconds > 0
                ? _remoteTimeoutSeconds
                : defaults.RemoteRequestPolicy.TimeoutSeconds;
            var initialRetry = _remoteInitialRetryDelayMilliseconds > 0
                ? _remoteInitialRetryDelayMilliseconds
                : defaults.RemoteRequestPolicy.InitialRetryDelayMilliseconds;
            var maximumRetry = Math.Max(
                initialRetry,
                _remoteMaximumRetryDelayMilliseconds > 0
                    ? _remoteMaximumRetryDelayMilliseconds
                    : defaults.RemoteRequestPolicy.MaximumRetryDelayMilliseconds);
            return new Bundles.ContentDeliveryOptions(
                cacheMegabytes * 1024L * 1024L,
                parallelDownloads,
                TimeSpan.FromHours(stagingHours),
                new Bundles.ContentRequestPolicy(
                    maximumAttempts,
                    timeoutSeconds,
                    initialRetry,
                    maximumRetry),
                defaults.LocalRequestPolicy);
        }
    }
}
