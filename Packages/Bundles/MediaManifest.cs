using System;
using System.Collections.Generic;
using System.Linq;

namespace Bundles
{
    public sealed class MediaManifest
    {
        private readonly HashSet<string> _videoIds;
        private readonly Dictionary<string, string> _audioExtensions;
        private readonly HashSet<string> _silentAudioIds;

        public MediaManifest(
            IEnumerable<string> videoIds,
            IEnumerable<KeyValuePair<string, string>> audioExtensions = null,
            string defaultAudioExtension = ".wav",
            IEnumerable<string> silentAudioIds = null)
        {
            _videoIds = new HashSet<string>(
                videoIds ?? Array.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);
            _audioExtensions = audioExtensions == null
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(
                    audioExtensions.ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value,
                        StringComparer.OrdinalIgnoreCase),
                    StringComparer.OrdinalIgnoreCase);
            DefaultAudioExtension = defaultAudioExtension;
            _silentAudioIds = new HashSet<string>(
                silentAudioIds ?? Array.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);
        }

        internal string DefaultAudioExtension { get; }
        internal bool ContainsVideo(string assetName) => _videoIds.Contains(assetName);
        internal string GetAudioExtension(string assetName) =>
            _audioExtensions.TryGetValue(assetName, out var extension)
                ? extension
                : DefaultAudioExtension;
        internal bool IsSilentAudio(string assetName) =>
            _silentAudioIds.Contains(assetName);
    }
}
