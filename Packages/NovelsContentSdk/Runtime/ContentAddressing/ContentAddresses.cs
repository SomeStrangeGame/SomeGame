using System;

namespace Novels.ContentAddressing
{
    public sealed class ContentAddresses
    {
        public ContentAddresses(string contentId)
        {
            if (string.IsNullOrWhiteSpace(contentId))
                throw new ArgumentException("Content ID must not be empty.", nameof(contentId));
            ContentId = contentId;
        }

        public string ContentId { get; }

        public string NovelText(string path) =>
            ContentAddressConvention.NovelText(ContentId, path);

        public string NovelSourceMap(string path) =>
            ContentAddressConvention.NovelSourceMap(ContentId, path);

        public string LoadingPrefab(string assetName) =>
            ContentAddressConvention.LoadingPrefab(ContentId, assetName);

        public string SettingPrefab(string assetName) =>
            ContentAddressConvention.SettingPrefab(ContentId, assetName);

        public string BubblePrefab(string assetName) =>
            ContentAddressConvention.BubblePrefab(ContentId, assetName);

        public string ChooseItem(string assetName) =>
            ContentAddressConvention.ChooseItem(ContentId, assetName);

        public string LocationPrefab(string assetName) =>
            ContentAddressConvention.LocationPrefab(ContentId, assetName);

        public string LocationImage(string assetName) =>
            ContentAddressConvention.LocationImage(ContentId, assetName);

        public string CharacterPrefab(string assetName) =>
            ContentAddressConvention.CharacterPrefab(ContentId, assetName);

        public string CharacterSpriteTrimManifest() =>
            ContentAddressConvention.CharacterSpriteTrimManifest(ContentId);

        public string CharacterMainBody(string name, string view, string candidate) =>
            ContentAddressConvention.CharacterMainBody(
                ContentId, name, view, candidate);

        public string CharacterEmotion(string name, string view, string candidate) =>
            ContentAddressConvention.CharacterEmotion(
                ContentId, name, view, candidate);

        public string CharacterClothes(string name, string candidate, int index) =>
            ContentAddressConvention.CharacterClothes(
                ContentId, name, candidate, index);

        public string CharacterHair(
            string name,
            string candidate,
            string layer,
            string color) =>
            ContentAddressConvention.CharacterHair(
                ContentId, name, candidate, layer, color);

        public string CharacterAccessory(string name, string candidate, string layer) =>
            ContentAddressConvention.CharacterAccessory(
                ContentId, name, candidate, layer);

        public string NotificationPrefab(string assetName) =>
            ContentAddressConvention.NotificationPrefab(ContentId, assetName);

    }
}
