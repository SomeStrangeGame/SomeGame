using System;
using System.Collections.Generic;
using UnityEngine;

namespace Novels.Catalog.Contracts
{
    [Serializable]
    public sealed class CatalogRegistry
    {
        public int schemaVersion = 2;
        public string minimumClientVersion;
        public string[] stories = Array.Empty<string>();
    }

    [Serializable]
    public sealed class StoryCard
    {
        public int schemaVersion = 1;
        public string minimumClientVersion;
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
            RequireSchema(value.schemaVersion, 2, "catalog registry");
            value.stories ??= Array.Empty<string>();
            var identifiers = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < value.stories.Length; index++)
            {
                var storyId = RequireCanonicalStoryId(value.stories[index]);
                value.stories[index] = storyId;
                if (!identifiers.Add(storyId))
                {
                    throw new InvalidOperationException(
                        $"Catalog registry contains duplicate story '{storyId}'.");
                }
            }
            return value;
        }

        public static StoryCard DeserializeCard(string json, string expectedStoryId)
        {
            var value = Deserialize<StoryCard>(json, "story card");
            RequireSchema(value.schemaVersion, 1, "story card");
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

        private static void RequireSchema(
            int schemaVersion,
            int expectedVersion,
            string contractName)
        {
            if (schemaVersion != expectedVersion)
            {
                throw new InvalidOperationException(
                    $"Unsupported {contractName} schema version: {schemaVersion}. "
                    + $"Expected {expectedVersion}.");
            }
        }
    }
}
