using System;
using System.Collections.Generic;
using System.Linq;

namespace Novels.Content
{
    public sealed class EpisodeContentDependencies
    {
        public static EpisodeContentDependencies Empty { get; } = new(null, null, null);

        public EpisodeContentDependencies(
            IEnumerable<string> audioIds,
            IEnumerable<string> backgroundIds = null,
            IEnumerable<string> speakerIds = null)
        {
            AudioIds = Normalize(audioIds);
            BackgroundIds = Normalize(backgroundIds);
            SpeakerIds = Normalize(speakerIds);
        }

        public IReadOnlyList<string> AudioIds { get; }
        public IReadOnlyList<string> BackgroundIds { get; }
        public IReadOnlyList<string> SpeakerIds { get; }

        private static IReadOnlyList<string> Normalize(IEnumerable<string> values) =>
            Array.AsReadOnly(
                (values ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray());
    }
}
