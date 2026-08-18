using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bundles
{
    internal readonly struct ContentCachePayload
    {
        internal ContentCachePayload(string path, long size)
        {
            Path = string.IsNullOrWhiteSpace(path)
                ? throw new ArgumentException("Cache path must not be empty.", nameof(path))
                : path;
            Size = size >= 0
                ? size
                : throw new ArgumentOutOfRangeException(nameof(size));
        }

        internal string Path { get; }
        internal long Size { get; }
    }

    internal sealed class ContentStoragePlanner
    {
        private const string _cacheRoot = "RemoteContent";
        private const string _stagingRoot = "ContentStaging";

        private sealed class Pin
        {
            internal Pin(long size)
            {
                Size = size;
                Count = 1;
            }

            internal long Size { get; }
            internal int Count { get; set; }
        }

        private readonly Cache.Entity _cache;
        private readonly long _cacheLimit;
        private readonly TimeSpan _stagingLifetime;
        private readonly CancellationToken _cancellationToken;
        private readonly Action<(LogType type, string message)> _onLog;
        private readonly Dictionary<string, Pin> _pins = new(
            StringComparer.OrdinalIgnoreCase);
        private readonly object _gate = new();

        internal ContentStoragePlanner(
            Cache.Entity cache,
            long cacheLimit,
            TimeSpan stagingLifetime,
            CancellationToken cancellationToken,
            Action<(LogType type, string message)> onLog)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _cacheLimit = cacheLimit > 0
                ? cacheLimit
                : throw new ArgumentOutOfRangeException(nameof(cacheLimit));
            _stagingLifetime = stagingLifetime > TimeSpan.Zero
                ? stagingLifetime
                : throw new ArgumentOutOfRangeException(nameof(stagingLifetime));
            _cancellationToken = cancellationToken;
            _onLog = onLog;
        }

        internal async UniTask<ContentDeliveryLease> Reserve(
            IReadOnlyCollection<ContentCachePayload> payloads)
        {
            if (payloads == null)
                throw new ArgumentNullException(nameof(payloads));
            var normalized = payloads
                .GroupBy(value => value.Path, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToArray();
            Exception failure = null;
            await UniTask.SwitchToThreadPool();
            try
            {
                lock (_gate)
                {
                    _cancellationToken.ThrowIfCancellationRequested();
                    _cache.PruneTemporaryFiles(
                        _stagingRoot,
                        DateTime.UtcNow - _stagingLifetime);
                    var missingBytes = normalized.Sum(GetMissingBytes);
                    var protectedPaths = _pins.Keys
                        .Concat(normalized.Select(value => value.Path))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();
                    var reservedBytes = GetReservedBytes(normalized);
                    _cache.PruneBySize(
                        _cacheRoot,
                        Math.Max(_cacheLimit, reservedBytes),
                        protectedPaths);
                    var available = _cache.GetAvailableFreeSpace();
                    if (available.HasValue && available.Value < missingBytes)
                    {
                        _cache.PruneForAvailableSpace(
                            _cacheRoot,
                            missingBytes,
                            protectedPaths);
                        available = _cache.GetAvailableFreeSpace();
                    }
                    if (available.HasValue && available.Value < missingBytes)
                    {
                        throw new ContentStorageException(
                            $"Content requires {missingBytes} free bytes, but only "
                            + $"{available.Value} bytes are available after cache cleanup.");
                    }
                    foreach (var payload in normalized)
                    {
                        if (_pins.TryGetValue(payload.Path, out var pin))
                            pin.Count++;
                        else
                            _pins[payload.Path] = new Pin(payload.Size);
                    }
                }
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            await UniTask.SwitchToMainThread();
            if (failure != null)
                ExceptionDispatchInfo.Capture(failure).Throw();
            _cancellationToken.ThrowIfCancellationRequested();
            return new ContentDeliveryLease(() => Release(normalized));
        }

        internal void SchedulePrune(string protectedPath = null) =>
            PruneAsync(protectedPath).Forget();

        private async UniTask PruneAsync(string protectedPath)
        {
            Exception failure = null;
            await UniTask.SwitchToThreadPool();
            try
            {
                lock (_gate)
                {
                    var protectedPaths = _pins.Keys
                        .Concat(string.IsNullOrWhiteSpace(protectedPath)
                            ? Array.Empty<string>()
                            : new[] { protectedPath })
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();
                    _cache.PruneBySize(
                        _cacheRoot,
                        Math.Max(_cacheLimit, _pins.Values.Sum(value => value.Size)),
                        protectedPaths);
                }
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            await UniTask.SwitchToMainThread();
            if (failure != null)
            {
                _onLog?.Invoke((
                    LogType.Warning,
                    $"Content cache pruning failed: {failure.Message}"));
            }
        }

        internal static string FilePath(ContentFileDescriptor descriptor) =>
            $"{_cacheRoot}/Files/{descriptor.Sha256.ToLowerInvariant()}";

        internal static string BundlePath(
            ContentReleaseSession session,
            string platform,
            BundleReleaseDescriptor descriptor) =>
            $"{_cacheRoot}/{session.ReleaseId}/Bundles/{platform}/"
            + $"{descriptor.Name}/{descriptor.Version}";

        private long GetMissingBytes(ContentCachePayload payload)
        {
            var localPath = _cache.GetLocalPath(payload.Path, false);
            if (!File.Exists(localPath))
                return payload.Size;
            return Math.Max(0L, payload.Size - new FileInfo(localPath).Length);
        }

        private long GetReservedBytes(IEnumerable<ContentCachePayload> additions)
        {
            var sizes = _pins.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.Size,
                StringComparer.OrdinalIgnoreCase);
            foreach (var payload in additions)
                sizes[payload.Path] = payload.Size;
            return sizes.Values.Sum();
        }

        private void Release(IEnumerable<ContentCachePayload> payloads)
        {
            lock (_gate)
            {
                foreach (var payload in payloads)
                {
                    if (!_pins.TryGetValue(payload.Path, out var pin))
                        continue;
                    pin.Count--;
                    if (pin.Count <= 0)
                        _pins.Remove(payload.Path);
                }
            }
            SchedulePrune();
        }
    }
}
