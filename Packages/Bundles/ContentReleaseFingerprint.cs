using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bundles
{
    public static class ContentReleaseFingerprint
    {
        public static string Compute(ContentReleaseDto release)
        {
            if (release == null)
                throw new ArgumentNullException(nameof(release));
            var lines = new List<string>
            {
                $"S:{release.contentSchemaVersion}",
                $"C:{release.minimumClientVersion ?? string.Empty}",
            };
            if (release.contentSchemaVersion >= 2)
                lines.Add($"D:{(int)release.deliveryMode}");
            lines.AddRange((release.bundles ?? Array.Empty<BundleReleaseEntry>())
                .OrderBy(value => value?.name, StringComparer.Ordinal)
                .Select(value =>
                    $"B:{value?.name}:{value?.version}:{value?.size}:{value?.sha256}:"
                    + $"{value?.crc}:{value?.deliveryGroup}"));
            lines.AddRange((release.files ?? Array.Empty<ContentFileEntry>())
                .OrderBy(value => value?.path, StringComparer.Ordinal)
                .Select(value =>
                    $"F:{value?.path}:{value?.payloadPath}:{value?.size}:"
                    + $"{value?.sha256}:{value?.deliveryGroup}"));
            lines.AddRange((release.deliveryGroups ?? Array.Empty<ContentDeliveryGroupEntry>())
                .OrderBy(value => value?.id, StringComparer.Ordinal)
                .Select(value => $"G:{value?.id}:{value?.payloadCount}:{value?.size}"));
            if (release.streamingPlan != null)
            {
                lines.Add(
                    $"P:{release.streamingPlan.previewBundle}:"
                    + release.streamingPlan.previewDeliveryGroup);
                lines.AddRange((release.streamingPlan.chunks
                        ?? Array.Empty<ContentStreamingChunkEntry>())
                    .OrderBy(value => value?.index)
                    .Select(value =>
                        $"K:{value?.index}:{value?.bundle}:{value?.deliveryGroup}:"
                        + string.Join(",", value?.assets ?? Array.Empty<string>())));
                lines.AddRange((release.streamingPlan.media
                        ?? Array.Empty<ContentStreamingMediaEntry>())
                    .OrderBy(value => value?.order)
                    .Select(value =>
                        $"M:{value?.order}:{value?.path}:{value?.deliveryGroup}"));
            }
            return ContentHash.ComputeSha256(
                Encoding.UTF8.GetBytes(string.Join("\n", lines)));
        }
    }
}
