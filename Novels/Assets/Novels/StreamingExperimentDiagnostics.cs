using System;
using System.Collections.Generic;
using UnityEngine;

namespace Novels
{
    internal static class StreamingExperimentDiagnostics
    {
        private sealed class ThroughputSample
        {
            internal long Completed;
            internal float Time;
            internal float BytesPerSecond;
        }

        private static readonly object _gate = new();
        private static readonly Dictionary<string, ThroughputSample> _samples =
            new(StringComparer.Ordinal);
        private static string _quality = "Idle";
        private static string _group = "-";
        private static long _completed;
        private static long _total;
        private static float _bytesPerSecond;
        private static string _queue = "-";

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
                if (!_samples.TryGetValue(progress.GroupId, out var sample))
                {
                    sample = new ThroughputSample
                    {
                        Completed = progress.CompletedBytes,
                        Time = now,
                    };
                    _samples.Add(progress.GroupId, sample);
                }
                else
                {
                    var elapsed = now - sample.Time;
                    if (elapsed >= 0.25f)
                    {
                        sample.BytesPerSecond = Math.Max(
                            0f,
                            (progress.CompletedBytes - sample.Completed) / elapsed);
                        sample.Completed = progress.CompletedBytes;
                        sample.Time = now;
                    }
                }
                _group = progress.GroupId;
                _completed = progress.CompletedBytes;
                _total = progress.TotalBytes;
                _bytesPerSecond = sample.BytesPerSecond;
            }
        }

        internal static void SetQueue(string value)
        {
            lock (_gate)
                _queue = string.IsNullOrWhiteSpace(value) ? "-" : value;
        }

        internal static string Snapshot()
        {
            lock (_gate)
            {
                var ratio = _total <= 0 ? 0f : (float)_completed / _total;
                return $"{_quality} · {_group} {ratio:P0} · "
                    + $"{_bytesPerSecond / (1024f * 1024f):F1} MiB/s\n"
                    + $"Queue · {_queue}";
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
                _bytesPerSecond = 0f;
                _queue = "-";
                _samples.Clear();
            }
        }
    }
}
