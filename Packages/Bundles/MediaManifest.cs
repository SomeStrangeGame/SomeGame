using System;
using System.Collections.Generic;

namespace Bundles
{
    public sealed class MediaManifest
    {
        private readonly HashSet<string> _videoIds;
        private readonly Dictionary<string, string> _audioExtensions;

        public MediaManifest(
            IEnumerable<string> videoIds,
            IDictionary<string, string> audioExtensions = null,
            string defaultAudioExtension = ".wav")
        {
            _videoIds = new HashSet<string>(
                videoIds ?? Array.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);
            _audioExtensions = audioExtensions == null
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(audioExtensions, StringComparer.OrdinalIgnoreCase);
            DefaultAudioExtension = defaultAudioExtension;
        }

        internal string DefaultAudioExtension { get; }
        internal bool ContainsVideo(string assetName) => _videoIds.Contains(assetName);
        internal string GetAudioExtension(string assetName) =>
            _audioExtensions.TryGetValue(assetName, out var extension)
                ? extension
                : DefaultAudioExtension;
    }
}
