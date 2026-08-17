using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bundles
{
    internal sealed class MediaResolver
    {
        private readonly Func<string, UniTask<string>> _resolveFileUrl;
        private readonly CancellationToken _cancellationToken;
        private string _prefix;
        private MediaManifest _manifest = new(Array.Empty<string>());

        internal MediaResolver(
            Func<string, UniTask<string>> resolveFileUrl,
            CancellationToken cancellationToken)
        {
            _resolveFileUrl = resolveFileUrl
                ?? throw new ArgumentNullException(nameof(resolveFileUrl));
            _cancellationToken = cancellationToken;
        }

        internal void Configure(string prefix, MediaManifest manifest)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException("Media prefix is required.", nameof(prefix));

            _prefix = prefix;
            _manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        }

        internal async UniTask<string> ResolveVideoUrl(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName) || !_manifest.ContainsVideo(assetName))
                return null;
            var path = $"NovelsVideos/{_prefix}/{assetName}.mp4";
            return await _resolveFileUrl(path);
        }

        internal async UniTask<string> ResolveAudioUrl(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName))
                return null;
            if (_manifest.IsSilentAudio(assetName))
                return null;

            var extension = Path.GetExtension(assetName);
            var fileName = extension.Length == 0
                ? assetName + _manifest.GetAudioExtension(assetName)
                : assetName;
            return await _resolveFileUrl($"NovelsAudio/{_prefix}/{fileName}");
        }

        internal void Clear()
        {
        }
    }
}
