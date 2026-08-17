using System;
using System.Collections.Generic;
using System.IO;

namespace Bundles
{
    public static class ContentReleaseValidator
    {
        public static void Validate(
            ContentReleaseDto release,
            string clientVersion,
            int supportedSchemaVersion)
        {
            if (release == null || string.IsNullOrWhiteSpace(release.releaseId))
                throw new ContentIntegrityException("Content release ID is missing.");
            if (release.contentSchemaVersion <= 0)
                throw new ContentIntegrityException("Content schema version is invalid.");
            if (release.contentSchemaVersion > supportedSchemaVersion)
            {
                throw new ContentCompatibilityException(
                    $"Content schema {release.contentSchemaVersion} requires "
                    + $"a newer client (supported: {supportedSchemaVersion}).");
            }
            if (!Enum.IsDefined(typeof(ContentDeliveryMode), release.deliveryMode))
                throw new ContentIntegrityException("Content delivery mode is invalid.");
            if (!Version.TryParse(release.minimumClientVersion, out var minimum))
            {
                throw new ContentIntegrityException(
                    $"Minimum client version '{release.minimumClientVersion}' is invalid.");
            }
            if (!Version.TryParse(clientVersion, out var current))
            {
                throw new ContentCompatibilityException(
                    $"Current client version '{clientVersion}' is invalid.");
            }
            if (current < minimum)
            {
                throw new ContentCompatibilityException(
                    $"Content requires client {minimum} or newer; current is {current}.");
            }
            if (release.bundles == null || release.bundles.Length == 0)
                throw new ContentIntegrityException("Content release has no bundles.");

            var bundleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var bundle in release.bundles)
            {
                if (bundle == null || string.IsNullOrWhiteSpace(bundle.name))
                    throw new ContentIntegrityException("Release contains an unnamed bundle.");
                if (!bundleNames.Add(bundle.name))
                    throw new ContentIntegrityException($"Duplicate bundle '{bundle.name}'.");
                ValidatePayload(bundle.name, bundle.version, bundle.size, bundle.sha256);
            }

            var filePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in release.files ?? Array.Empty<ContentFileEntry>())
            {
                if (file == null || !IsNormalizedRelativePath(file.path))
                    throw new ContentIntegrityException("Release contains an invalid file path.");
                if (!filePaths.Add(file.path))
                    throw new ContentIntegrityException($"Duplicate file '{file.path}'.");
                if (file.size < 0 || !IsSha256(file.sha256))
                    throw new ContentIntegrityException($"File '{file.path}' metadata is invalid.");
            }

            var groupIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var group in release.deliveryGroups
                         ?? Array.Empty<ContentDeliveryGroupEntry>())
            {
                if (group == null
                    || string.IsNullOrWhiteSpace(group.id)
                    || group.payloadCount <= 0
                    || group.size < 0)
                {
                    throw new ContentIntegrityException(
                        "Release contains an invalid delivery group.");
                }
                if (!groupIds.Add(group.id))
                    throw new ContentIntegrityException(
                        $"Duplicate delivery group '{group.id}'.");
            }
            if (groupIds.Count > 0)
            {
                var actualGroups = new Dictionary<string, (int count, long size)>(
                    StringComparer.OrdinalIgnoreCase);
                foreach (var file in release.files ?? Array.Empty<ContentFileEntry>())
                {
                    if (string.IsNullOrWhiteSpace(file.deliveryGroup)
                        || !groupIds.Contains(file.deliveryGroup))
                    {
                        throw new ContentIntegrityException(
                            $"File '{file.path}' has an unknown delivery group.");
                    }
                    actualGroups.TryGetValue(file.deliveryGroup, out var actual);
                    actualGroups[file.deliveryGroup] = (
                        actual.count + 1,
                        actual.size + file.size);
                }
                foreach (var bundle in release.bundles)
                {
                    if (string.IsNullOrWhiteSpace(bundle.deliveryGroup)
                        || !groupIds.Contains(bundle.deliveryGroup))
                    {
                        throw new ContentIntegrityException(
                            $"Bundle '{bundle.name}' has an unknown delivery group.");
                    }
                    actualGroups.TryGetValue(bundle.deliveryGroup, out var actual);
                    actualGroups[bundle.deliveryGroup] = (
                        actual.count + 1,
                        actual.size + bundle.size);
                }
                foreach (var group in release.deliveryGroups)
                {
                    actualGroups.TryGetValue(group.id, out var actual);
                    if (group.payloadCount != actual.count || group.size != actual.size)
                    {
                        throw new ContentIntegrityException(
                            $"Delivery group '{group.id}' totals do not match its payloads.");
                    }
                }
            }
            if ((release.contentSchemaVersion >= 4
                    || release.deliveryMode != ContentDeliveryMode.Embedded)
                && groupIds.Count == 0)
            {
                throw new ContentIntegrityException(
                    "Non-embedded content release has no delivery groups.");
            }
            var expectedReleaseId = ContentReleaseFingerprint.Compute(release);
            if (!string.Equals(
                    release.releaseId,
                    expectedReleaseId,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ContentIntegrityException(
                    $"Content release ID '{release.releaseId}' does not match its payload.");
            }
        }

        internal static void ValidatePayload(
            string name,
            string version,
            long size,
            string sha256)
        {
            if (string.IsNullOrWhiteSpace(version) || size <= 0 || !IsSha256(sha256))
            {
                throw new ContentIntegrityException(
                    $"Integrity metadata for '{name}' is incomplete.");
            }
        }

        private static bool IsNormalizedRelativePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)
                || Path.IsPathRooted(path)
                || path.Contains('\\')
                || path.StartsWith("./", StringComparison.Ordinal))
            {
                return false;
            }
            foreach (var segment in path.Split('/'))
            {
                if (segment.Length == 0 || segment == "." || segment == "..")
                    return false;
            }
            return true;
        }

        private static bool IsSha256(string value)
        {
            if (value == null || value.Length != 64)
                return false;
            foreach (var character in value)
            {
                if (!Uri.IsHexDigit(character))
                    return false;
            }
            return true;
        }
    }
}
