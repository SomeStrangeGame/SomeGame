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
        public int schemaVersion = 2;
        public string minimumClientVersion;
        public string storyId;
        public string title;
        public string genre;
        public string description;
        public string cover = "cover.webp";
        public string author;
    }

    public static class CatalogContractCodec
    {
        public static StoryCatalogPreview DeserializePreview(string json, string storyId)
        {
            var value = Deserialize<StoryCatalogPreview>(json, "story catalog preview");
            RequireSchema(value.schemaVersion, 1, "story catalog preview");
            if (value.storyId != RequireCanonicalStoryId(storyId)
                || string.IsNullOrWhiteSpace(value.releaseId)
                || string.IsNullOrWhiteSpace(value.contentVersion)
                || value.episodes == null || value.episodes.Length == 0)
                throw new InvalidOperationException($"Story '{storyId}' preview is incomplete.");
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            value.video = NormalizeVideo(value.video);
            foreach (var episode in value.episodes)
            {
                if (episode == null || string.IsNullOrWhiteSpace(episode.id)
                    || string.IsNullOrWhiteSpace(episode.title) || !ids.Add(episode.id))
                    throw new InvalidOperationException($"Story '{storyId}' preview has invalid episodes.");
                episode.cover = string.IsNullOrWhiteSpace(episode.cover) ? null
                    : global::Novels.ContentAddressing.ContentPackageConvention.EpisodeCoverFileName(episode.cover);
                episode.author = episode.author?.Trim();
                episode.video = NormalizeVideo(episode.video);
            }
            return value;
        }

        private static string NormalizeVideo(string video) => string.IsNullOrWhiteSpace(video) ? null
            : global::Novels.ContentAddressing.ContentPackageConvention.CatalogVideoFileName(video);

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
            RequireSchema(value.schemaVersion, 2, "story card");
            value.storyId = RequireCanonicalStoryId(value.storyId);
            var expected = RequireCanonicalStoryId(expectedStoryId);
            if (!string.Equals(value.storyId, expected, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Story card id '{value.storyId}' does not match '{expected}'.");
            }
            if (string.IsNullOrWhiteSpace(value.title))
                throw new InvalidOperationException($"Story '{expected}' has no title.");
            if (string.IsNullOrWhiteSpace(value.genre))
                throw new InvalidOperationException($"Story '{expected}' has no genre.");
            value.genre = value.genre.Trim();
            value.author = value.author?.Trim();
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

    // Generated from the authored definition, not a second authoring source.
    [Serializable]
    public sealed class StoryCatalogPreview
    {
        public int schemaVersion = 1;
        public string storyId;
        public string releaseId;
        public string contentVersion;
        public string video;
        public StoryCatalogEpisodePreview[] episodes;
    }

    [Serializable]
    public sealed class StoryCatalogEpisodePreview
    {
        public string id;
        public string title;
        public string description;
        public string cover;
        public string author;
        public string video;
    }
}
