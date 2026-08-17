using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
                    $"F:{value?.path}:{value?.size}:{value?.sha256}:{value?.deliveryGroup}"));
            lines.AddRange((release.deliveryGroups ?? Array.Empty<ContentDeliveryGroupEntry>())
                .OrderBy(value => value?.id, StringComparer.Ordinal)
                .Select(value => $"G:{value?.id}:{value?.payloadCount}:{value?.size}"));
            using var sha = SHA256.Create();
            return ToHex(sha.ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n", lines))));
        }

        private static string ToHex(byte[] data) =>
            BitConverter.ToString(data).Replace("-", string.Empty).ToLowerInvariant();
    }
}
