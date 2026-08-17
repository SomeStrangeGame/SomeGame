using System;
using System.Collections.Generic;
using System.Linq;

namespace Novels.Content
{
    public sealed class NovelDefinition
    {
        public NovelDefinition(
            string id,
            string prefix,
            string mainCharacter,
            string loadingBundleName,
            string settingBundleName,
            string localizationBundleName,
            EpisodeDefinition episode)
            : this(
                id,
                prefix,
                mainCharacter,
                loadingBundleName,
                settingBundleName,
                localizationBundleName,
                new[] { episode })
        {
        }

        public NovelDefinition(
            string id,
            string prefix,
            string mainCharacter,
            string loadingBundleName,
            string settingBundleName,
            string localizationBundleName,
            IEnumerable<EpisodeDefinition> episodes)
        {
            Id = Require(id, nameof(id));
            Prefix = Require(prefix, nameof(prefix));
            MainCharacter = Require(mainCharacter, nameof(mainCharacter));
            LoadingBundleName = Require(loadingBundleName, nameof(loadingBundleName));
            SettingBundleName = Require(settingBundleName, nameof(settingBundleName));
            LocalizationBundleName = Require(localizationBundleName, nameof(localizationBundleName));
            var episodeArray = episodes?.ToArray() ?? Array.Empty<EpisodeDefinition>();
            if (episodeArray.Length == 0 || episodeArray.Any(episode => episode == null))
                throw new ArgumentException("At least one valid episode is required.", nameof(episodes));
            Episodes = Array.AsReadOnly(episodeArray);
        }

        public string Id { get; }
        public string Prefix { get; }
        public string MainCharacter { get; }
        public string LoadingBundleName { get; }
        public string SettingBundleName { get; }
        public string LocalizationBundleName { get; }
        public IReadOnlyList<EpisodeDefinition> Episodes { get; }
        public EpisodeDefinition Episode => Episodes[0];

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
            string id,
            string storyPath,
            string contentVersion,
            string bubbleBundleName,
            string locationBundleName,
            string characterBundleName,
            string notificationBundleName,
            EpisodeMediaDefinition media)
        {
            Id = Require(id, nameof(id));
            StoryPath = Require(storyPath, nameof(storyPath));
            ContentVersion = Require(contentVersion, nameof(contentVersion));
            BubbleBundleName = Require(bubbleBundleName, nameof(bubbleBundleName));
            LocationBundleName = Require(locationBundleName, nameof(locationBundleName));
            CharacterBundleName = Require(characterBundleName, nameof(characterBundleName));
            NotificationBundleName = Require(notificationBundleName, nameof(notificationBundleName));
            Media = media ?? throw new ArgumentNullException(nameof(media));
        }

        public string Id { get; }
        public string StoryPath { get; }
        public string ContentVersion { get; }
        public string BubbleBundleName { get; }
        public string LocationBundleName { get; }
        public string CharacterBundleName { get; }
        public string NotificationBundleName { get; }
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
        public EpisodeMediaDefinition(
            IEnumerable<string> videoIds,
            IDictionary<string, string> audioExtensions = null,
            string defaultAudioExtension = ".wav",
            IEnumerable<string> silentAudioIds = null)
        {
            var videos = (videoIds ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            VideoIds = Array.AsReadOnly(videos);
            AudioExtensions = new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(
                audioExtensions == null
                    ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, string>(audioExtensions, StringComparer.OrdinalIgnoreCase));
            DefaultAudioExtension = string.IsNullOrWhiteSpace(defaultAudioExtension)
                ? ".wav"
                : defaultAudioExtension;
            SilentAudioIds = Array.AsReadOnly(
                (silentAudioIds ?? Array.Empty<string>())
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray());
        }

        public IReadOnlyList<string> VideoIds { get; }
        public IReadOnlyDictionary<string, string> AudioExtensions { get; }
        public string DefaultAudioExtension { get; }
        public IReadOnlyList<string> SilentAudioIds { get; }
    }
}
