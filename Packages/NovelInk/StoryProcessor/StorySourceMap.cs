using System;
using System.Collections.Generic;
using UnityEngine;

namespace Novels.StoryProcessor
{
    [Serializable]
    public sealed class StorySourceMap
    {
        [Serializable]
        public sealed class Entry
        {
            public string Path;
            public string FileName;
            public int LineNumber;
        }

        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();

        public StorySourceMap()
        {
        }

        public StorySourceMap(Entry[] entries)
        {
            _entries = entries ?? Array.Empty<Entry>();
        }

        public Entry[] Entries => _entries ?? Array.Empty<Entry>();
        public string ToJson() => JsonUtility.ToJson(this);

        public static StorySourceMap FromJson(string json) =>
            string.IsNullOrWhiteSpace(json)
                ? new StorySourceMap()
                : JsonUtility.FromJson<StorySourceMap>(json) ?? new StorySourceMap();
    }

    internal sealed class StorySourceMapResolver
    {
        private readonly IReadOnlyDictionary<string, StorySourceLocation> _locations;

        internal StorySourceMapResolver(string json)
        {
            var locations = new Dictionary<string, StorySourceLocation>(StringComparer.Ordinal);
            foreach (var entry in StorySourceMap.FromJson(json).Entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Path) || entry.LineNumber <= 0)
                    continue;
                locations[entry.Path] = new StorySourceLocation(entry.FileName, entry.LineNumber);
            }
            _locations = locations;
        }

        internal StorySourceLocation Resolve(string runtimePath)
        {
            var path = runtimePath;
            while (!string.IsNullOrEmpty(path))
            {
                if (_locations.TryGetValue(path, out var location))
                    return location;
                var separator = path.LastIndexOf('.');
                path = separator < 0 ? string.Empty : path.Substring(0, separator);
            }
            return default;
        }
    }
}
