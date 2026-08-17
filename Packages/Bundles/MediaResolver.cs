using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bundles
{
    internal sealed class MediaResolver
    {
        private readonly Cache.Entity _cache;
        private readonly IContentSource _source;
        private readonly CancellationToken _cancellationToken;
        private readonly Dictionary<string, string> _videos = new(StringComparer.OrdinalIgnoreCase);
        private string _prefix;
        private MediaManifest _manifest = new(Array.Empty<string>());

        internal MediaResolver(
            Cache.Entity cache,
            IContentSource source,
            CancellationToken cancellationToken)
        {
            _cache = cache;
            _source = source;
            _cancellationToken = cancellationToken;
        }

        internal void Configure(string prefix, MediaManifest manifest)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException("Media prefix is required.", nameof(prefix));

            _prefix = prefix;
            _manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
            _videos.Clear();
        }

        internal async UniTask<string> ResolveVideoUrl(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName) || !_manifest.ContainsVideo(assetName))
                return null;
            if (_videos.TryGetValue(assetName, out var existing))
                return existing;

            var path = $"NovelsVideos/{_prefix}/{assetName}.mp4";
            if (!_cache.Exists(path))
            {
                var data = await _source.DownloadBytes(path);
                _cancellationToken.ThrowIfCancellationRequested();
                _cache.WriteBytes(path, data);
            }

            var url = new Uri(_cache.ConvertLocalPath(path)).AbsoluteUri;
            _videos[assetName] = url;
            return url;
        }

        internal UniTask<string> ResolveAudioUrl(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName))
                return UniTask.FromResult<string>(null);
            if (_manifest.IsSilentAudio(assetName))
                return UniTask.FromResult<string>(null);

            var extension = Path.GetExtension(assetName);
            var fileName = extension.Length == 0
                ? assetName + _manifest.GetAudioExtension(assetName)
                : assetName;
            return UniTask.FromResult(_source.GetUrl($"NovelsAudio/{_prefix}/{fileName}"));
        }

        internal void Clear()
        {
            _videos.Clear();
        }
    }
}
