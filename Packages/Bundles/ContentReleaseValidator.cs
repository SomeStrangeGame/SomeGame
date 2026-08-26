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
            int minimumSupportedSchemaVersion,
            int maximumSupportedSchemaVersion)
        {
            if (release == null || string.IsNullOrWhiteSpace(release.releaseId))
                throw new ContentIntegrityException("Content release ID is missing.");
            if (release.contentSchemaVersion <= 0)
                throw new ContentIntegrityException("Content schema version is invalid.");
            if (minimumSupportedSchemaVersion <= 0
                || maximumSupportedSchemaVersion < minimumSupportedSchemaVersion)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumSupportedSchemaVersion),
                    "Supported content schema range is invalid.");
            }
            if (release.contentSchemaVersion < minimumSupportedSchemaVersion
                || release.contentSchemaVersion > maximumSupportedSchemaVersion)
            {
                throw new ContentCompatibilityException(
                    $"Content schema {release.contentSchemaVersion} is incompatible "
                    + $"with this client (supported: {minimumSupportedSchemaVersion}-"
                    + $"{maximumSupportedSchemaVersion}).");
            }
            if (release.deliveryMode != ContentDeliveryMode.Remote)
                throw new ContentIntegrityException("Only remote content delivery is supported.");
            if (!ClientVersion.TryParse(release.minimumClientVersion, out var minimum))
            {
                throw new ContentIntegrityException(
                    $"Minimum client version '{release.minimumClientVersion}' is invalid.");
            }
            if (!ClientVersion.TryParse(clientVersion, out var current))
            {
                throw new ContentCompatibilityException(
                    $"Current client version '{clientVersion}' is invalid.");
            }
            if (current.CompareTo(minimum) < 0)
            {
                throw new ContentCompatibilityException(
                    $"Content requires client {release.minimumClientVersion} or newer; "
                    + $"current is {clientVersion}.");
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
            var payloadMetadata = new Dictionary<string, (long size, string sha256)>(
                StringComparer.Ordinal);
            foreach (var file in release.files ?? Array.Empty<ContentFileEntry>())
            {
                if (file == null
                    || !IsNormalizedRelativePath(file.path)
                    || !IsNormalizedRelativePath(file.payloadPath))
                    throw new ContentIntegrityException("Release contains an invalid file path.");
                if (!filePaths.Add(file.path))
                    throw new ContentIntegrityException($"Duplicate file '{file.path}'.");
                if (file.size < 0 || !IsSha256(file.sha256))
                    throw new ContentIntegrityException($"File '{file.path}' metadata is invalid.");
                var expectedPayloadPath =
                    $"Files/{file.sha256.ToLowerInvariant()}.bin";
                if (!string.Equals(
                        file.payloadPath,
                        expectedPayloadPath,
                        StringComparison.Ordinal))
                {
                    throw new ContentIntegrityException(
                        $"File '{file.path}' payload path must be '{expectedPayloadPath}'.");
                }
                if (payloadMetadata.TryGetValue(file.payloadPath, out var existing)
                    && (existing.size != file.size
                        || !string.Equals(
                            existing.sha256,
                            file.sha256,
                            StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ContentIntegrityException(
                        $"Payload '{file.payloadPath}' has conflicting metadata.");
                }
                payloadMetadata[file.payloadPath] = (file.size, file.sha256);
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
            if (groupIds.Count == 0)
            {
                throw new ContentIntegrityException(
                    "Content release has no delivery groups.");
            }
            ValidateStreamingPlan(release.streamingPlan, bundleNames, filePaths, groupIds);
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

        private static void ValidateStreamingPlan(
            ContentStreamingPlanEntry plan,
            ISet<string> bundleNames,
            ISet<string> filePaths,
            ISet<string> groupIds)
        {
            if (plan == null)
                return;
            if (plan.chunks == null || plan.chunks.Length == 0)
            {
                throw new ContentIntegrityException(
                    "Streaming plan must contain at least one art chunk.");
            }
            var expectedChunk = 0;
            foreach (var chunk in plan.chunks ?? Array.Empty<ContentStreamingChunkEntry>())
            {
                if (chunk == null
                    || chunk.index != expectedChunk++
                    || string.IsNullOrWhiteSpace(chunk.bundle)
                    || !bundleNames.Contains(chunk.bundle)
                    || string.IsNullOrWhiteSpace(chunk.deliveryGroup)
                    || !groupIds.Contains(chunk.deliveryGroup)
                    || chunk.assets == null
                    || chunk.assets.Length == 0)
                {
                    throw new ContentIntegrityException(
                        "Streaming chunks must be contiguous and reference known payloads.");
                }
            }
            var expectedMedia = 0;
            foreach (var media in plan.media ?? Array.Empty<ContentStreamingMediaEntry>())
            {
                if (media == null
                    || media.order != expectedMedia++
                    || !filePaths.Contains(media.path)
                    || !groupIds.Contains(media.deliveryGroup))
                {
                    throw new ContentIntegrityException(
                        "Streaming media order references an unknown payload.");
                }
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

    public readonly struct ClientVersion : IComparable<ClientVersion>
    {
        private readonly int _major;
        private readonly int _minor;
        private readonly int _patch;
        private readonly int _revision;

        private ClientVersion(int major, int minor, int patch, int revision)
        {
            _major = major;
            _minor = minor;
            _patch = patch;
            _revision = revision;
        }

        public static bool TryParse(string value, out ClientVersion version)
        {
            version = default;
            if (string.IsNullOrWhiteSpace(value)
                || !string.Equals(value, value.Trim(), StringComparison.Ordinal))
                return false;
            var parts = value.Split('.');
            if (parts.Length < 1 || parts.Length > 4)
                return false;
            var numbers = new int[4];
            for (var index = 0; index < parts.Length; index++)
            {
                if (parts[index].Length == 0
                    || ContainsNonDigit(parts[index])
                    || !int.TryParse(parts[index], out numbers[index])
                    || numbers[index] < 0)
                    return false;
            }
            version = new ClientVersion(numbers[0], numbers[1], numbers[2], numbers[3]);
            return true;
        }

        private static bool ContainsNonDigit(string value)
        {
            foreach (var character in value)
            {
                if (character < '0' || character > '9')
                    return true;
            }
            return false;
        }

        public int CompareTo(ClientVersion other)
        {
            var result = _major.CompareTo(other._major);
            if (result != 0)
                return result;
            result = _minor.CompareTo(other._minor);
            if (result != 0)
                return result;
            result = _patch.CompareTo(other._patch);
            return result != 0 ? result : _revision.CompareTo(other._revision);
        }
    }
}
