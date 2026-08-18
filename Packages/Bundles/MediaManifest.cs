using System;
using System.Collections.Generic;

namespace Bundles
{
    public static class MediaFileConvention
    {
        public const string DefaultAudioExtension = ".wav";
        public const string VideoExtension = ".mp4";
    }

    public sealed class MediaManifest
    {
        private readonly HashSet<string> _silentAudioIds;

        public MediaManifest(IEnumerable<string> silentAudioIds = null)
        {
            _silentAudioIds = new HashSet<string>(
                silentAudioIds ?? Array.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);
        }

        internal bool IsSilentAudio(string assetName) =>
            _silentAudioIds.Contains(assetName);
    }
}
