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

        internal ContentReleaseSnapshot Current { get; private set; }

        internal async UniTask<ContentReleaseSnapshot> LoadAsync(
            string clientVersion,
            int supportedSchemaVersion)
        {
            var path = $"Remote/{_platform}/release.json";
            var cachePath = $"Remote/{_platform}/Releases/current.json";
            ContentReleaseDto release;
            try
            {
                var json = await _source.DownloadText(path);
                release = JsonUtility.FromJson<ContentReleaseDto>(json);
                ContentReleaseValidator.Validate(
                    release,
                    clientVersion,
                    supportedSchemaVersion);
                _cache.TextToCache(cachePath, json);
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
                if (!_cache.Exists(cachePath))
                {
                    throw new ContentSourceException(
                        "Content release is unavailable and no cached release exists.",
                        exception);
                }
                release = JsonUtility.FromJson<ContentReleaseDto>(
                    _cache.TextFromCache(cachePath));
                ContentReleaseValidator.Validate(
                    release,
                    clientVersion,
                    supportedSchemaVersion);
                _onLog?.Invoke((
                    LogType.Warning,
                    $"Use cached content release '{release.releaseId}'."));
            }

            Current = new ContentReleaseSnapshot(release);
            return Current;
        }
    }
}
