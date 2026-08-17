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

        public BundleReleaseDescriptor FindBundle(string name) =>
            name != null && _bundlesByName.TryGetValue(name, out var value) ? value : null;

        public ContentFileDescriptor FindFile(string path) =>
            path != null && _filesByPath.TryGetValue(path, out var value) ? value : null;
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
            Size = source.size;
            Sha256 = source.sha256;
            DeliveryGroup = source.deliveryGroup;
        }

        public string Path { get; }
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
}
