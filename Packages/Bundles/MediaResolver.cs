using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Bundles
{
    internal sealed class MediaResolver
    {
        private readonly Func<string, UniTask<string>> _resolveFileUrl;
        private readonly string _prefix;
        private readonly ContentReleaseSession _session;
        private readonly HashSet<string> _videoDeliveryGroups;
        private readonly MediaManifest _manifest;

        internal MediaResolver(
            ContentReleaseSession session,
            string prefix,
            IEnumerable<string> videoDeliveryGroups,
            MediaManifest manifest,
            Func<string, UniTask<string>> resolveFileUrl)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException("Media prefix is required.", nameof(prefix));
            _prefix = prefix;
            _videoDeliveryGroups = new HashSet<string>(
                (videoDeliveryGroups ?? Array.Empty<string>())
                    .Where(value => !string.IsNullOrWhiteSpace(value)),
                StringComparer.OrdinalIgnoreCase);
            if (_videoDeliveryGroups.Count == 0)
            {
                throw new ArgumentException(
                    "At least one video delivery group is required.",
                    nameof(videoDeliveryGroups));
            }
            _manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
            _resolveFileUrl = resolveFileUrl
                ?? throw new ArgumentNullException(nameof(resolveFileUrl));
        }

        internal async UniTask<string> ResolveVideoUrl(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName))
                return null;
            var path = $"NovelsVideos/{_prefix}/{assetName}.mp4";
            var descriptor = _session.FindFile(path);
            if (descriptor == null
                || !_videoDeliveryGroups.Contains(descriptor.DeliveryGroup))
                return null;
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
    }
}
