using System;
using System.Collections.Generic;
using System.Linq;

namespace Novels.Content
{
    public sealed class NovelDefinition
    {
        public NovelDefinition(
            string id,
            string mainCharacter,
            EpisodeDefinition episode)
            : this(
                id,
                mainCharacter,
                new[] { episode })
        {
        }

        public NovelDefinition(
            string id,
            string mainCharacter,
            IEnumerable<EpisodeDefinition> episodes)
        {
            Id = Require(id, nameof(id));
            MainCharacter = Require(mainCharacter, nameof(mainCharacter));
            Prefix = Id;
            MainLoadingBundleName =
                ContentAddressing.ContentPackageConvention.SharedLoadingBundleName;
            BundleName = ContentAddressing.ContentPackageConvention.ContentBundle(Id);
            CharacterAssets = CharacterAssetProfile.Default;
            var episodeArray = episodes?.ToArray() ?? Array.Empty<EpisodeDefinition>();
            if (episodeArray.Length == 0 || episodeArray.Any(episode => episode == null))
                throw new ArgumentException("At least one valid episode is required.", nameof(episodes));
            var episodeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var episode in episodeArray)
            {
                if (!string.Equals(episode.ContentId, Id, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(
                        $"Episode '{episode.Id}' belongs to content '{episode.ContentId}', "
                        + $"not '{Id}'.",
                        nameof(episodes));
                }
                if (!episodeIds.Add(episode.Id))
                {
                    throw new ArgumentException(
                        $"Duplicate episode ID '{episode.Id}'.",
                        nameof(episodes));
                }
            }
            Episodes = Array.AsReadOnly(episodeArray);
        }

        public string Id { get; }
        public string Prefix { get; }
        public string MainCharacter { get; }
        public string MainLoadingBundleName { get; }
        public string BundleName { get; }
        public CharacterAssetProfile CharacterAssets { get; }
        public IReadOnlyList<EpisodeDefinition> Episodes { get; }

        private static string Require(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Content value must not be empty.", parameterName);
            return value;
        }
    }

    public sealed class EpisodeDefinition
    {
        public EpisodeDefinition(
            string contentId,
            string id,
            string title,
            string storyPath,
            string contentVersion,
            EpisodeMediaDefinition media)
        {
            ContentId = Require(contentId, nameof(contentId));
            Id = Require(id, nameof(id));
            Title = Require(title, nameof(title));
            StoryPath = Require(storyPath, nameof(storyPath));
            ContentVersion = Require(contentVersion, nameof(contentVersion));
            BundleName = ContentAddressing.ContentPackageConvention.EpisodeBundle(
                ContentId,
                Id);
            Media = media ?? throw new ArgumentNullException(nameof(media));
        }

        public string ContentId { get; }
        public string Id { get; }
        public string Title { get; }
        public string StoryPath { get; }
        public string ContentVersion { get; }
        public string BundleName { get; }
        public EpisodeMediaDefinition Media { get; }

        private static string Require(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Content value must not be empty.", parameterName);
            return value;
        }
    }

    public sealed class EpisodeMediaDefinition
    {
        public EpisodeMediaDefinition(IEnumerable<string> silentAudioIds = null)
        {
            SilentAudioIds = Array.AsReadOnly(
                (silentAudioIds ?? Array.Empty<string>())
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray());
        }

        public IReadOnlyList<string> SilentAudioIds { get; }
    }
}
