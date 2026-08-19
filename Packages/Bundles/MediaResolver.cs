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
        private readonly IReadOnlyDictionary<string, ContentFileDescriptor> _audioByName;
        private readonly HashSet<string> _ambiguousAudioNames;
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
            (_audioByName, _ambiguousAudioNames) = BuildAudioIndex(session.Release, prefix);
        }

        internal async UniTask<string> ResolveVideoUrl(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName))
                return null;
            var path = $"NovelsVideos/{_prefix}/{assetName}"
                + MediaFileConvention.VideoExtension;
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

            var normalized = assetName.Trim();
            if (!string.Equals(Path.GetFileName(normalized), normalized, StringComparison.Ordinal)
                || Path.GetExtension(normalized).Length > 0)
            {
                throw new ContentConfigurationException(
                    $"Audio reference '{assetName}' must contain only a file name "
                    + "without an extension.");
            }
            if (_ambiguousAudioNames.Contains(normalized))
            {
                throw new ContentIntegrityException(
                    $"Audio reference '{normalized}' matches multiple released files.");
            }
            if (!_audioByName.TryGetValue(normalized, out var descriptor))
                return null;
            return await _resolveFileUrl(descriptor.Path);
        }

        private static (
            IReadOnlyDictionary<string, ContentFileDescriptor> files,
            HashSet<string> ambiguousNames) BuildAudioIndex(
                ContentReleaseSnapshot release,
                string prefix)
        {
            var directory = $"NovelsAudio/{prefix}/";
            var files = new Dictionary<string, ContentFileDescriptor>(
                StringComparer.OrdinalIgnoreCase);
            var ambiguous = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in release.Files)
            {
                if (!file.Path.StartsWith(directory, StringComparison.OrdinalIgnoreCase))
                    continue;
                var relative = file.Path.Substring(directory.Length);
                if (relative.Length == 0 || relative.Contains('/'))
                    continue;
                var name = Path.GetFileNameWithoutExtension(relative);
                if (ambiguous.Contains(name))
                    continue;
                if (files.Remove(name))
                {
                    ambiguous.Add(name);
                    continue;
                }
                files.Add(name, file);
            }
            return (files, ambiguous);
        }
    }
}
