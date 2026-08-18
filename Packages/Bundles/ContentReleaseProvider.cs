using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bundles
{
    internal sealed class ContentReleaseProvider
    {
        private readonly IContentSource _source;
        private readonly Cache.Entity _cache;
        private readonly string _platform;
        private readonly CancellationToken _cancellationToken;
        private readonly Action<(LogType type, string message)> _onLog;
        private ContentReleaseSession _activatedSession;

        internal ContentReleaseProvider(
            IContentSource source,
            Cache.Entity cache,
            string platform,
            CancellationToken cancellationToken,
            Action<(LogType type, string message)> onLog)
        {
            _source = source;
            _cache = cache;
            _platform = platform;
            _cancellationToken = cancellationToken;
            _onLog = onLog;
        }

        internal ContentReleaseSession Current { get; private set; }
        internal CancellationToken CancellationToken => _cancellationToken;

        internal async UniTask<ContentReleaseSession> LoadAsync(
            string clientVersion,
            int minimumSupportedSchemaVersion,
            int maximumSupportedSchemaVersion)
        {
            var path = $"Remote/{_platform}/release.json";
            var cachePath = $"Remote/{_platform}/Releases/current.json";
            var previousCachePath = $"Remote/{_platform}/Releases/previous.json";
            ContentReleaseDto release;
            string candidateJson = null;
            try
            {
                var json = await _source.DownloadText(path, _cancellationToken);
                release = ContentReleaseCodec.DeserializeAndValidate(
                    json,
                    clientVersion,
                    minimumSupportedSchemaVersion,
                    maximumSupportedSchemaVersion);
                candidateJson = json;
            }
            catch (OperationCanceledException)
                when (_cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (ContentCompatibilityException)
            {
                throw;
            }
            catch (Exception exception)
            {
                try
                {
                    release = LoadCached(
                        cachePath,
                        clientVersion,
                        minimumSupportedSchemaVersion,
                        maximumSupportedSchemaVersion);
                }
                catch (Exception currentFailure)
                {
                    try
                    {
                        release = LoadCached(
                            previousCachePath,
                            clientVersion,
                            minimumSupportedSchemaVersion,
                            maximumSupportedSchemaVersion);
                    }
                    catch (Exception previousFailure)
                    {
                        throw new ContentSourceException(
                            "Content release is unavailable and no valid active release exists.",
                            new AggregateException(
                                exception,
                                currentFailure,
                                previousFailure));
                    }
                }
                _onLog?.Invoke((
                    LogType.Warning,
                    $"Use active cached content release '{release.releaseId}'."));
            }

            Current = new ContentReleaseSession(
                new ContentReleaseSnapshot(release),
                candidateJson);
            return Current;
        }

        internal void ActivateCurrent()
        {
            if (Current == null)
                throw new ContentConfigurationException(
                    "Content release must be loaded before activation.");
            if (ReferenceEquals(Current, _activatedSession)
                || string.IsNullOrEmpty(Current.CandidateJson))
                return;
            var cachePath = $"Remote/{_platform}/Releases/current.json";
            var previousCachePath = $"Remote/{_platform}/Releases/previous.json";
            if (_cache.Exists(cachePath))
            {
                var activeJson = _cache.TextFromCache(cachePath);
                if (TryGetFingerprintValidRelease(activeJson, out var active)
                    && !string.Equals(
                        active.releaseId,
                        Current.ReleaseId,
                        StringComparison.Ordinal))
                {
                    _cache.TextToCache(previousCachePath, activeJson);
                }
            }
            _cache.TextToCache(cachePath, Current.CandidateJson);
            _activatedSession = Current;
            _onLog?.Invoke((
                LogType.Log,
                $"Activate content release '{Current.ReleaseId}'."));
        }

        private static bool HasValidFingerprint(ContentReleaseDto release)
        {
            if (release == null || string.IsNullOrWhiteSpace(release.releaseId))
                return false;
            try
            {
                return string.Equals(
                    release.releaseId,
                    ContentReleaseFingerprint.Compute(release),
                    StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static bool TryGetFingerprintValidRelease(
            string json,
            out ContentReleaseDto release)
        {
            try
            {
                release = ContentReleaseCodec.Deserialize(json);
                return HasValidFingerprint(release);
            }
            catch
            {
                release = null;
                return false;
            }
        }

        private ContentReleaseDto LoadCached(
            string cachePath,
            string clientVersion,
            int minimumSupportedSchemaVersion,
            int maximumSupportedSchemaVersion)
        {
            if (!_cache.Exists(cachePath))
                throw new ContentSourceException($"Cached release is missing: {cachePath}");
            return ContentReleaseCodec.DeserializeAndValidate(
                _cache.TextFromCache(cachePath),
                clientVersion,
                minimumSupportedSchemaVersion,
                maximumSupportedSchemaVersion);
        }
    }
}
