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

        public static string LoadingPrefab(string prefix, string assetName) =>
            Prefab($"{_remoteAssetsRoot}/Loading/{prefix}", assetName);

        public static string SettingPrefab(string prefix, string assetName) =>
            Prefab($"{_remoteAssetsRoot}/Setting/{prefix}", assetName);

        public static string BubblePrefab(string prefix, string assetName) =>
            Prefab($"{_remoteAssetsRoot}/Bubble/{prefix}", assetName);

        public static string LocationPrefab(string prefix, string assetName) =>
            Prefab($"{_remoteAssetsRoot}/Location/{prefix}", assetName);

        public static string LocationImage(string prefix, string assetName) =>
            IsMissing(assetName)
                ? string.Empty
                : $"{_remoteAssetsRoot}/Location/{prefix}/Locations/"
                    + $"{NormalizeAssetName(assetName)}.png";

        public static string CharacterPrefab(string prefix, string assetName) =>
            Prefab($"{_remoteAssetsRoot}/Character/{prefix}", assetName);

        public static string CharacterMainBody(
            string prefix,
            string name,
            string view,
            string candidate = null) =>
            IsMissing(name)
                ? string.Empty
                : $"{CharacterRoot(prefix)}/{name}/{view}/{candidate ?? "Main"}.png";

        public static string CharacterEmotion(
            string prefix,
            string name,
            string view,
            string candidate) =>
            NamedCharacterPath(
                name,
                candidate,
                value => $"{CharacterRoot(prefix)}/{name}/{view}/Emotions/{value}.png");

        public static string CharacterClothes(
            string prefix,
            string name,
            string candidate,
            int index) =>
            NamedCharacterPath(
                name,
                candidate,
                value => $"{CharacterRoot(prefix)}/{name}/Clothes/{value}/{index}.png");

        public static string CharacterHair(
            string prefix,
            string name,
            string candidate,
            string layer,
            string color) =>
            NamedCharacterPath(
                name,
                candidate,
                value => $"{CharacterRoot(prefix)}/{name}/Hairs/{layer}/{value}/{color}.png");

        public static string CharacterAccessory(
            string prefix,
            string name,
            string candidate,
            string layer) =>
            NamedCharacterPath(
                name,
                candidate,
                value => $"{CharacterRoot(prefix)}/{name}/Accessories/{layer}/{value}.png");

        public static string NotificationPrefab(string prefix, string assetName) =>
            Prefab($"{_remoteAssetsRoot}/Notification/{prefix}", assetName);

        public static string LocalizationAsset(string prefix, string assetName) =>
            IsMissing(assetName)
                ? string.Empty
                : $"{_remoteAssetsRoot}/Localization/{prefix}/{assetName}.asset";

        public static string NormalizeAssetName(string value) =>
            IsMissing(value)
                ? string.Empty
                : char.ToUpperInvariant(value[0])
                    + value.Substring(1).ToLowerInvariant();

        private static string CharacterRoot(string prefix) =>
            $"{_remoteAssetsRoot}/Character/{prefix}/Characters";

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
