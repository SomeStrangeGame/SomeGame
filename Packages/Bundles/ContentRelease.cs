using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Bundles
{
    [Serializable]
    public sealed class ContentReleaseDto
    {
        public string releaseId;
        public string minimumClientVersion;
        public int contentSchemaVersion;
        public ContentDeliveryMode deliveryMode;
        public BundleReleaseEntry[] bundles;
        public ContentFileEntry[] files;
        public ContentDeliveryGroupEntry[] deliveryGroups;
        public ContentStreamingPlanEntry streamingPlan;
    }

    [Serializable]
    public sealed class BundleReleaseEntry
    {
        public string name;
        public string version;
        public long size;
        public string sha256;
        public uint crc;
        public string deliveryGroup;
    }

    [Serializable]
    public sealed class ContentFileEntry
    {
        public string path;
        public string payloadPath;
        public long size;
        public string sha256;
        public string deliveryGroup;
    }

    [Serializable]
    public sealed class ContentDeliveryGroupEntry
    {
        public string id;
        public int payloadCount;
        public long size;
    }

    [Serializable]
    public sealed class ContentStreamingPlanEntry
    {
        public string previewBundle;
        public string previewDeliveryGroup;
        public ContentStreamingChunkEntry[] chunks;
        public ContentStreamingMediaEntry[] media;
    }

    [Serializable]
    public sealed class ContentStreamingChunkEntry
    {
        public int index;
        public string bundle;
        public string deliveryGroup;
        public string[] assets;
    }

    [Serializable]
    public sealed class ContentStreamingMediaEntry
    {
        public int order;
        public string path;
        public string deliveryGroup;
    }

    public sealed class ContentReleaseSnapshot
    {
        private readonly IReadOnlyDictionary<string, BundleReleaseDescriptor> _bundlesByName;
        private readonly IReadOnlyDictionary<string, ContentFileDescriptor> _filesByPath;

        internal ContentReleaseSnapshot(ContentReleaseDto source)
        {
            ReleaseId = source.releaseId;
            MinimumClientVersion = source.minimumClientVersion;
            ContentSchemaVersion = source.contentSchemaVersion;
            DeliveryMode = source.deliveryMode;
            var bundles = (source.bundles ?? Array.Empty<BundleReleaseEntry>())
                .Select(value => new BundleReleaseDescriptor(value))
                .ToArray();
            var files = (source.files ?? Array.Empty<ContentFileEntry>())
                .Select(value => new ContentFileDescriptor(value))
                .ToArray();
            var groups = (source.deliveryGroups ?? Array.Empty<ContentDeliveryGroupEntry>())
                .Select(value => new ContentDeliveryGroupDescriptor(value))
                .ToArray();
            Bundles = Array.AsReadOnly(bundles);
            Files = Array.AsReadOnly(files);
            DeliveryGroups = Array.AsReadOnly(groups);
            StreamingPlan = source.streamingPlan;
            _bundlesByName = new ReadOnlyDictionary<string, BundleReleaseDescriptor>(
                bundles.ToDictionary(value => value.Name, StringComparer.OrdinalIgnoreCase));
            _filesByPath = new ReadOnlyDictionary<string, ContentFileDescriptor>(
                files.ToDictionary(value => value.Path, StringComparer.OrdinalIgnoreCase));
        }

        public string ReleaseId { get; }
        public string MinimumClientVersion { get; }
        public int ContentSchemaVersion { get; }
        public ContentDeliveryMode DeliveryMode { get; }
        public IReadOnlyList<BundleReleaseDescriptor> Bundles { get; }
        public IReadOnlyList<ContentFileDescriptor> Files { get; }
        public IReadOnlyList<ContentDeliveryGroupDescriptor> DeliveryGroups { get; }
        public ContentStreamingPlanEntry StreamingPlan { get; }

        public BundleReleaseDescriptor FindBundle(string name) =>
            name != null && _bundlesByName.TryGetValue(name, out var value) ? value : null;

        public ContentFileDescriptor FindFile(string path) =>
            path != null && _filesByPath.TryGetValue(path, out var value) ? value : null;
    }

    internal sealed class ContentReleaseSession
    {
        internal ContentReleaseSession(
            ContentReleaseSnapshot release,
            string candidateJson)
        {
            Release = release ?? throw new ArgumentNullException(nameof(release));
            CandidateJson = candidateJson;
        }

        internal ContentReleaseSnapshot Release { get; }
        internal string CandidateJson { get; }
        internal string ReleaseId => Release.ReleaseId;
        internal ContentDeliveryMode DeliveryMode => Release.DeliveryMode;

        internal BundleReleaseDescriptor FindBundle(string name) =>
            Release.FindBundle(name);

        internal ContentFileDescriptor FindFile(string path) =>
            Release.FindFile(path);
    }

    public sealed class BundleReleaseDescriptor
    {
        internal BundleReleaseDescriptor(BundleReleaseEntry source)
        {
            Name = source.name;
            Version = source.version;
            Size = source.size;
            Sha256 = source.sha256;
            Crc = source.crc;
            DeliveryGroup = source.deliveryGroup;
        }

        public string Name { get; }
        public string Version { get; }
        public long Size { get; }
        public string Sha256 { get; }
        public uint Crc { get; }
        public string DeliveryGroup { get; }
    }

    public sealed class ContentFileDescriptor
    {
        internal ContentFileDescriptor(ContentFileEntry source)
        {
            Path = source.path;
            PayloadPath = source.payloadPath;
            Size = source.size;
            Sha256 = source.sha256;
            DeliveryGroup = source.deliveryGroup;
        }

        public string Path { get; }
        public string PayloadPath { get; }
        public long Size { get; }
        public string Sha256 { get; }
        public string DeliveryGroup { get; }
    }

    public sealed class ContentDeliveryGroupDescriptor
    {
        internal ContentDeliveryGroupDescriptor(ContentDeliveryGroupEntry source)
        {
            Id = source.id;
            PayloadCount = source.payloadCount;
            Size = source.size;
        }

        public string Id { get; }
        public int PayloadCount { get; }
        public long Size { get; }
    }

    public static class ContentReleaseCodec
    {
        public static ContentReleaseDto Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ContentIntegrityException("Content release JSON is empty.");
            try
            {
                var release = UnityEngine.JsonUtility.FromJson<ContentReleaseDto>(json)
                    ?? throw new ContentIntegrityException(
                        "Content release JSON produced no document.");
                // JsonUtility materializes an explicit JSON null nested object as an
                // empty instance. Keep the optional plan absent for legacy/catalog
                // releases, while partially populated malformed plans still reach
                // the validator and fail closed.
                if (IsEmptyStreamingPlan(release.streamingPlan))
                    release.streamingPlan = null;
                return release;
            }
            catch (ContentIntegrityException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new ContentIntegrityException(
                    "Content release JSON is invalid.",
                    exception);
            }
        }

        private static bool IsEmptyStreamingPlan(ContentStreamingPlanEntry plan) =>
            plan != null
            && string.IsNullOrWhiteSpace(plan.previewBundle)
            && string.IsNullOrWhiteSpace(plan.previewDeliveryGroup)
            && (plan.chunks == null || plan.chunks.Length == 0)
            && (plan.media == null || plan.media.Length == 0);

        public static ContentReleaseDto DeserializeAndValidate(
            string json,
            string clientVersion,
            int minimumSupportedSchemaVersion,
            int maximumSupportedSchemaVersion)
        {
            var release = Deserialize(json);
            ContentReleaseValidator.Validate(
                release,
                clientVersion,
                minimumSupportedSchemaVersion,
                maximumSupportedSchemaVersion);
            return release;
        }

        public static ContentReleaseSnapshot CreateSnapshot(
            string json,
            string clientVersion,
            int minimumSupportedSchemaVersion,
            int maximumSupportedSchemaVersion) =>
            new(DeserializeAndValidate(
                json,
                clientVersion,
                minimumSupportedSchemaVersion,
                maximumSupportedSchemaVersion));

        public static string Serialize(ContentReleaseDto release, bool prettyPrint = true)
        {
            if (release == null)
                throw new ArgumentNullException(nameof(release));
            return UnityEngine.JsonUtility.ToJson(release, prettyPrint);
        }
    }
}
