using System;
using UnityEngine;

namespace Novels
{
    internal static class StreamingExperimentDiagnostics
    {
        private static readonly object _gate = new();
        private static string _quality = "Idle";
        private static string _group = "-";
        private static long _completed;
        private static long _total;
        private static long _lastCompleted;
        private static float _lastSampleTime;
        private static float _bytesPerSecond;

        internal static void SetQuality(string value)
        {
            lock (_gate)
                _quality = value ?? "-";
        }

        internal static void ReportDelivery(Bundles.ContentDeliveryProgress progress)
        {
            lock (_gate)
            {
                var now = Time.realtimeSinceStartup;
                var elapsed = now - _lastSampleTime;
                if (_lastSampleTime > 0f && elapsed >= 0.25f)
                {
                    _bytesPerSecond = Math.Max(
                        0f,
                        (progress.CompletedBytes - _lastCompleted) / elapsed);
                    _lastCompleted = progress.CompletedBytes;
                    _lastSampleTime = now;
                }
                else if (_lastSampleTime <= 0f)
                {
                    _lastSampleTime = now;
                    _lastCompleted = progress.CompletedBytes;
                }
                _group = progress.GroupId;
                _completed = progress.CompletedBytes;
                _total = progress.TotalBytes;
            }
        }

        internal static string Snapshot()
        {
            lock (_gate)
            {
                var ratio = _total <= 0 ? 0f : (float)_completed / _total;
                return $"{_quality} · {_group} {ratio:P0} · "
                    + $"{_bytesPerSecond / (1024f * 1024f):F1} MiB/s";
            }
        }

        internal static void Reset()
        {
            lock (_gate)
            {
                _quality = "Idle";
                _group = "-";
                _completed = 0;
                _total = 0;
                _lastCompleted = 0;
                _lastSampleTime = 0f;
                _bytesPerSecond = 0f;
            }
        }
    }
}
