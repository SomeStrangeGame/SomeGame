using System;
using System.Collections.Generic;
using UnityEngine;

namespace Novels.Catalog.Contracts
{
    [Serializable]
    public sealed class CatalogRegistry
    {
        public int schemaVersion = 1;
        public CatalogRegistryEntry[] stories = Array.Empty<CatalogRegistryEntry>();
    }

    [Serializable]
    public sealed class CatalogRegistryEntry
    {
        public string storyId;
        public int order;
        public bool enabled = true;
    }

    [Serializable]
    public sealed class StoryCard
    {
        public int schemaVersion = 1;
        public string storyId;
        public string title;
        public string description;
        public string cover = "cover.webp";
    }

    public static class CatalogContractCodec
    {
        public static CatalogRegistry DeserializeRegistry(string json)
        {
            var value = Deserialize<CatalogRegistry>(json, "catalog registry");
            RequireSchema(value.schemaVersion, "catalog registry");
            value.stories ??= Array.Empty<CatalogRegistryEntry>();
            var identifiers = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < value.stories.Length; index++)
            {
                var entry = value.stories[index]
                    ?? throw new InvalidOperationException(
                        $"Catalog registry entry at index {index} is null.");
                entry.storyId = RequireCanonicalStoryId(entry.storyId);
                if (!identifiers.Add(entry.storyId))
                {
                    throw new InvalidOperationException(
                        $"Catalog registry contains duplicate story '{entry.storyId}'.");
                }
            }
            return value;
        }

        public static StoryCard DeserializeCard(string json, string expectedStoryId)
        {
            var value = Deserialize<StoryCard>(json, "story card");
            RequireSchema(value.schemaVersion, "story card");
            value.storyId = RequireCanonicalStoryId(value.storyId);
            var expected = RequireCanonicalStoryId(expectedStoryId);
            if (!string.Equals(value.storyId, expected, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Story card id '{value.storyId}' does not match '{expected}'.");
            }
            if (string.IsNullOrWhiteSpace(value.title))
                throw new InvalidOperationException($"Story '{expected}' has no title.");
            if (string.IsNullOrWhiteSpace(value.cover))
                throw new InvalidOperationException($"Story '{expected}' has no cover path.");
            value.cover = value.cover.Trim();
            if (value.cover.Contains("/")
                || value.cover.Contains("\\")
                || value.cover.Contains(".."))
            {
                throw new InvalidOperationException(
                    $"Story '{expected}' cover must be a file name, not a path.");
            }
            return value;
        }

        public static string Serialize(CatalogRegistry value, bool prettyPrint = true) =>
            JsonUtility.ToJson(value ?? throw new ArgumentNullException(nameof(value)), prettyPrint);

        public static string Serialize(StoryCard value, bool prettyPrint = true) =>
            JsonUtility.ToJson(value ?? throw new ArgumentNullException(nameof(value)), prettyPrint);

        private static T Deserialize<T>(string json, string contractName)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException($"The {contractName} JSON is empty.");
            T value;
            try
            {
                value = JsonUtility.FromJson<T>(json);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"The {contractName} JSON is invalid.",
                    exception);
            }
            return value ?? throw new InvalidOperationException(
                $"The {contractName} JSON contains no object.");
        }

        private static string RequireCanonicalStoryId(string value)
        {
            var prefix = global::Novels.ContentAddressing.ContentPackageConvention
                .StoryPrefix(value);
            return prefix.Substring(prefix.LastIndexOf('/') + 1);
        }

        private static void RequireSchema(int schemaVersion, string contractName)
        {
            if (schemaVersion != 1)
            {
                throw new InvalidOperationException(
                    $"Unsupported {contractName} schema version: {schemaVersion}.");
            }
        }
    }
}
