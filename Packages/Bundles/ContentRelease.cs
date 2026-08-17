using System;
using System.Collections.Generic;

namespace Bundles
{
    [Serializable]
    public sealed class ContentRelease
    {
        public string releaseId;
        public string minimumClientVersion;
        public int contentSchemaVersion;
        public BundleReleaseEntry[] bundles;
        public ContentFileEntry[] files;

        [NonSerialized] private Dictionary<string, BundleReleaseEntry> _bundleMap;
        [NonSerialized] private Dictionary<string, ContentFileEntry> _fileMap;

        public BundleReleaseEntry FindBundle(string name)
        {
            _bundleMap ??= BuildMap(
                bundles,
                entry => entry.name,
                StringComparer.OrdinalIgnoreCase);
            return name != null && _bundleMap.TryGetValue(name, out var entry)
                ? entry
                : null;
        }

        public ContentFileEntry FindFile(string path)
        {
            _fileMap ??= BuildMap(
                files,
                entry => entry.path,
                StringComparer.OrdinalIgnoreCase);
            return path != null && _fileMap.TryGetValue(path, out var entry)
                ? entry
                : null;
        }

        private static Dictionary<string, T> BuildMap<T>(
            IEnumerable<T> entries,
            Func<T, string> getKey,
            IEqualityComparer<string> comparer)
            where T : class
        {
            var result = new Dictionary<string, T>(comparer);
            if (entries == null)
                return result;
            foreach (var entry in entries)
            {
                if (entry == null)
                    continue;
                var key = getKey(entry);
                if (!string.IsNullOrWhiteSpace(key))
                    result[key] = entry;
            }
            return result;
        }
    }

    [Serializable]
    public sealed class BundleReleaseEntry
    {
        public string name;
        public string version;
        public long size;
        public string sha256;
        public uint crc;

        public BundleManifest ToManifest()
        {
            return new BundleManifest
            {
                version = version,
                size = size,
                sha256 = sha256,
                crc = crc,
            };
        }
    }

    [Serializable]
    public sealed class ContentFileEntry
    {
        public string path;
        public long size;
        public string sha256;
    }
}
