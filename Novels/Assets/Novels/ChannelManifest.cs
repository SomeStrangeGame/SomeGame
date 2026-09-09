using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Novels
{
    internal sealed class ChannelManifest
    {
        private readonly IReadOnlyDictionary<string, string> _versions;

        private ChannelManifest(string[] storyIds, IReadOnlyDictionary<string, string> versions)
        {
            StoryIds = storyIds;
            _versions = versions;
        }

        internal IReadOnlyList<string> StoryIds { get; }

        internal static string FileName(string channel) =>
            $"{CanonicalSegment(channel, "channel")}.json";

        internal string StoryRoot(string storyId)
        {
            if (!_versions.TryGetValue(storyId, out var version))
                throw new InvalidOperationException($"Channel does not contain story '{storyId}'.");
            return $"stories/{storyId}/{version}";
        }

        internal static ChannelManifest Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException("Channel manifest is empty.");
            JObject root;
            try
            {
                root = JObject.Parse(json);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Channel manifest JSON is invalid.", exception);
            }
            if (root.Value<int?>("schema") != 1)
                throw new InvalidOperationException("Unsupported channel manifest schema.");
            if (root["stories"] is not JObject stories || !stories.HasValues)
                throw new InvalidOperationException("Channel manifest has no stories.");

            var ids = new List<string>();
            var versions = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var property in stories.Properties())
            {
                var id = CanonicalSegment(property.Name, "story id");
                var version = property.Value.Type == JTokenType.String
                    ? CanonicalSegment(property.Value.Value<string>(), "story version")
                    : throw new InvalidOperationException($"Story '{id}' version must be a string.");
                if (!versions.TryAdd(id, version))
                    throw new InvalidOperationException($"Channel contains duplicate story '{id}'.");
                ids.Add(id);
            }
            return new ChannelManifest(ids.ToArray(), versions);
        }

        private static string CanonicalSegment(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"Channel {name} is empty.");
            var result = value.Trim();
            if (result == "." || result == ".." || result.Contains("/")
                || result.Contains("\\") || result.Contains(":")
                || !string.Equals(result, result.ToLowerInvariant(), StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Channel {name} '{value}' is not a canonical path segment.");
            }
            return result;
        }
    }
}
