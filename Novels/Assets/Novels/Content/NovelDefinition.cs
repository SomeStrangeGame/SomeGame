using System;

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
        {
            Id = Require(id, nameof(id));
            Prefix = Require(prefix, nameof(prefix));
            MainCharacter = Require(mainCharacter, nameof(mainCharacter));
            LoadingBundleName = Require(loadingBundleName, nameof(loadingBundleName));
            SettingBundleName = Require(settingBundleName, nameof(settingBundleName));
            LocalizationBundleName = Require(localizationBundleName, nameof(localizationBundleName));
            Episode = episode ?? throw new ArgumentNullException(nameof(episode));
        }

        public string Id { get; }
        public string Prefix { get; }
        public string MainCharacter { get; }
        public string LoadingBundleName { get; }
        public string SettingBundleName { get; }
        public string LocalizationBundleName { get; }
        public EpisodeDefinition Episode { get; }

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
            string notificationBundleName)
        {
            Id = Require(id, nameof(id));
            StoryPath = Require(storyPath, nameof(storyPath));
            ContentVersion = Require(contentVersion, nameof(contentVersion));
            BubbleBundleName = Require(bubbleBundleName, nameof(bubbleBundleName));
            LocationBundleName = Require(locationBundleName, nameof(locationBundleName));
            CharacterBundleName = Require(characterBundleName, nameof(characterBundleName));
            NotificationBundleName = Require(notificationBundleName, nameof(notificationBundleName));
        }

        public string Id { get; }
        public string StoryPath { get; }
        public string ContentVersion { get; }
        public string BubbleBundleName { get; }
        public string LocationBundleName { get; }
        public string CharacterBundleName { get; }
        public string NotificationBundleName { get; }

        private static string Require(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Content value must not be empty.", parameterName);
            return value;
        }
    }
}
