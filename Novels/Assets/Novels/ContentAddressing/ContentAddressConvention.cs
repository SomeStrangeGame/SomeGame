using System;

namespace Novels.ContentAddressing
{
    public static class ContentAddressConvention
    {
        private const string _remoteAssetsRoot = "Assets/RemoteAssets";

        public static string NovelText(string prefix, string path) =>
            IsMissing(path) ? string.Empty : $"NovelTexts/{prefix}/{path}";

        public static string MainLoadingPrefab(string assetName) =>
            Prefab($"{_remoteAssetsRoot}/Loading", assetName);

        public static string LoadingPrefab(
            string prefix,
            string episodeId,
            string assetName) =>
            Prefab($"{EpisodeRoot(prefix, episodeId)}/Loading", assetName);

        public static string SettingPrefab(string prefix, string assetName) =>
            Prefab($"{ContentRoot(prefix)}/Application/Setting", assetName);

        public static string BubblePrefab(
            string prefix,
            string episodeId,
            string assetName) =>
            Prefab($"{EpisodeRoot(prefix, episodeId)}/Bubble", assetName);

        public static string LocationPrefab(
            string prefix,
            string episodeId,
            string assetName) =>
            Prefab($"{EpisodeRoot(prefix, episodeId)}/Location", assetName);

        public static string LocationImage(
            string prefix,
            string episodeId,
            string assetName) =>
            IsMissing(assetName)
                ? string.Empty
                : $"{EpisodeRoot(prefix, episodeId)}/Location/Locations/"
                    + $"{NormalizeAssetName(assetName)}.png";

        public static string CharacterPrefab(
            string prefix,
            string episodeId,
            string assetName) =>
            Prefab($"{EpisodeRoot(prefix, episodeId)}/Character", assetName);

        public static string CharacterMainBody(
            string prefix,
            string episodeId,
            string name,
            string view,
            string candidate = null) =>
            IsMissing(name)
                ? string.Empty
                : $"{CharacterRoot(prefix, episodeId)}/{name}/{view}/{candidate ?? "Main"}.png";

        public static string CharacterEmotion(
            string prefix,
            string episodeId,
            string name,
            string view,
            string candidate) =>
            NamedCharacterPath(
                name,
                candidate,
                value => $"{CharacterRoot(prefix, episodeId)}/{name}/{view}/Emotions/{value}.png");

        public static string CharacterClothes(
            string prefix,
            string episodeId,
            string name,
            string candidate,
            int index) =>
            NamedCharacterPath(
                name,
                candidate,
                value => $"{CharacterRoot(prefix, episodeId)}/{name}/Clothes/{value}/{index}.png");

        public static string CharacterHair(
            string prefix,
            string episodeId,
            string name,
            string candidate,
            string layer,
            string color) =>
            NamedCharacterPath(
                name,
                candidate,
                value => $"{CharacterRoot(prefix, episodeId)}/{name}/Hairs/{layer}/{value}/{color}.png");

        public static string CharacterAccessory(
            string prefix,
            string episodeId,
            string name,
            string candidate,
            string layer) =>
            NamedCharacterPath(
                name,
                candidate,
                value => $"{CharacterRoot(prefix, episodeId)}/{name}/Accessories/{layer}/{value}.png");

        public static string NotificationPrefab(
            string prefix,
            string episodeId,
            string assetName) =>
            Prefab($"{EpisodeRoot(prefix, episodeId)}/Notification", assetName);

        public static string LocalizationAsset(string prefix, string assetName) =>
            IsMissing(assetName)
                ? string.Empty
                : $"{ContentRoot(prefix)}/Application/Localization/{assetName}.asset";

        public static string NormalizeAssetName(string value) =>
            IsMissing(value)
                ? string.Empty
                : char.ToUpperInvariant(value[0])
                    + value.Substring(1).ToLowerInvariant();

        private static string ContentRoot(string prefix) =>
            $"{_remoteAssetsRoot}/Content/{prefix}";

        private static string EpisodeRoot(string prefix, string episodeId) =>
            $"{ContentRoot(prefix)}/Episodes/{episodeId}";

        private static string CharacterRoot(string prefix, string episodeId) =>
            $"{EpisodeRoot(prefix, episodeId)}/Character/Characters";

        private static string Prefab(string root, string assetName) =>
            IsMissing(assetName) ? string.Empty : $"{root}/{assetName}.prefab";

        private static string NamedCharacterPath(
            string name,
            string candidate,
            Func<string, string> build) =>
            IsMissing(name) || IsMissing(candidate)
                ? string.Empty
                : build(NormalizeAssetName(candidate));

        private static bool IsMissing(string value) => string.IsNullOrEmpty(value);
    }
}
