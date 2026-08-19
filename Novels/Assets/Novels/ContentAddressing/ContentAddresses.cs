using System;

namespace Novels.ContentAddressing
{
    public sealed class ContentAddresses
    {
        public ContentAddresses(string contentId, string episodeId)
        {
            if (string.IsNullOrWhiteSpace(contentId))
                throw new ArgumentException("Content ID must not be empty.", nameof(contentId));
            if (string.IsNullOrWhiteSpace(episodeId))
                throw new ArgumentException("Episode ID must not be empty.", nameof(episodeId));
            ContentId = contentId;
            EpisodeId = episodeId;
        }

        public string ContentId { get; }
        public string EpisodeId { get; }

        public string NovelText(string path) =>
            ContentAddressConvention.NovelText(ContentId, path);

        public string MainLoadingPrefab(string assetName) =>
            ContentAddressConvention.MainLoadingPrefab(assetName);

        public string LoadingPrefab(string assetName) =>
            ContentAddressConvention.LoadingPrefab(ContentId, EpisodeId, assetName);

        public string SharedLoadingPrefab(string assetName) =>
            ContentAddressConvention.SharedLoadingPrefab(ContentId, assetName);

        public string SettingPrefab(string assetName) =>
            ContentAddressConvention.SettingPrefab(ContentId, assetName);

        public string BubblePrefab(string assetName) =>
            ContentAddressConvention.BubblePrefab(ContentId, EpisodeId, assetName);

        public string SharedBubblePrefab(string assetName) =>
            ContentAddressConvention.SharedBubblePrefab(ContentId, assetName);

        public string LocationPrefab(string assetName) =>
            ContentAddressConvention.LocationPrefab(ContentId, EpisodeId, assetName);

        public string SharedLocationPrefab(string assetName) =>
            ContentAddressConvention.SharedLocationPrefab(ContentId, assetName);

        public string LocationImage(string assetName) =>
            ContentAddressConvention.LocationImage(ContentId, EpisodeId, assetName);

        public string CharacterPrefab(string assetName) =>
            ContentAddressConvention.CharacterPrefab(ContentId, EpisodeId, assetName);

        public string SharedCharacterPrefab(string assetName) =>
            ContentAddressConvention.SharedCharacterPrefab(ContentId, assetName);

        public string NotificationPrefab(string assetName) =>
            ContentAddressConvention.NotificationPrefab(ContentId, EpisodeId, assetName);

        public string SharedNotificationPrefab(string assetName) =>
            ContentAddressConvention.SharedNotificationPrefab(ContentId, assetName);

    }
}
