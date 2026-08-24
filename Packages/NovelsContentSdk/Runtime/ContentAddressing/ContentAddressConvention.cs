using System;
using System.Text;

namespace Novels.ContentAddressing
{
    public static class TechnicalAssetIdConvention
    {
        public static string Canonicalize(string value) =>
            string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Normalize(NormalizationForm.FormC)
                    .Trim()
                    .ToLowerInvariant();
    }

    public static class CharacterAssetNameConvention
    {
        public static string NormalizeSelector(string value) =>
            TechnicalAssetIdConvention.Canonicalize(value);
    }

    public static class ContentAddressConvention
    {
        public static string NovelText(string prefix, string path) =>
            IsMissing(path)
                ? string.Empty
                : $"noveltexts/{TechnicalAssetIdConvention.Canonicalize(prefix)}/{path}";

        public static string NovelSourceMap(string prefix, string path) =>
            NovelText(prefix, path) + ".source-map.json";

        public static string LoadingPrefab(
            string prefix,
            string assetName) =>
            PresentationPrefab(prefix, "loading", assetName);

        public static string SettingPrefab(string prefix, string assetName) =>
            Prefab($"{ContentPackageConvention.ContentRoot(prefix)}/application/setting", assetName);

        public static string BubblePrefab(
            string prefix,
            string assetName) =>
            PresentationPrefab(prefix, "bubble", assetName);

        public static string ChooseItem(
            string prefix,
            string assetName) =>
            StoryAsset(prefix, "choose/items", assetName);

        public static string LocationPrefab(
            string prefix,
            string assetName) =>
            PresentationPrefab(prefix, "location", assetName);

        public static string LocationImage(
            string prefix,
            string assetName) =>
            StoryAsset(prefix, "location/locations", assetName);

        public static string CharacterPrefab(
            string prefix,
            string assetName) =>
            PresentationPrefab(prefix, "character", assetName);

        public static string CharacterSpriteTrimManifest(string prefix) =>
            $"{ContentPackageConvention.StoryRoot(prefix)}/character/sprite-trim-manifest.asset";

        public static string CharacterMainBody(
            string prefix,
            string name,
            string view,
            string candidate = null) =>
            IsMissing(name)
                ? string.Empty
                : $"{CharacterRoot(prefix)}/{Canonical(name)}/{NormalizeView(view)}/"
                    + $"{NormalizeSelector(candidate, "Main")}.png";

        public static string CharacterEmotion(
            string prefix,
            string name,
            string view,
            string candidate) =>
            NamedCharacterPath(
                name,
                candidate,
                value => $"{CharacterRoot(prefix)}/{Canonical(name)}/{NormalizeView(view)}"
                    + $"/emotions/{CharacterAssetNameConvention.NormalizeSelector(value)}.png");

        public static string CharacterClothes(
            string prefix,
            string name,
            string candidate,
            int index) =>
            NamedCharacterPath(
                name,
                candidate,
                value => $"{CharacterRoot(prefix)}/{Canonical(name)}/clothes/"
                    + $"{CharacterAssetNameConvention.NormalizeSelector(value)}/{index}.png");

        public static string CharacterHair(
            string prefix,
            string name,
            string candidate,
            string layer,
            string color) =>
            NamedCharacterPath(
                name,
                candidate,
                value => $"{CharacterRoot(prefix)}/{Canonical(name)}/hairs/{Canonical(layer)}/"
                    + $"{CharacterAssetNameConvention.NormalizeSelector(value)}/"
                    + $"{CharacterAssetNameConvention.NormalizeSelector(color)}.png");

        public static string CharacterAccessory(
            string prefix,
            string name,
            string candidate,
            string layer) =>
            NamedCharacterPath(
                name,
                candidate,
                value => $"{CharacterRoot(prefix)}/{Canonical(name)}/accessories/{Canonical(layer)}/"
                    + $"{CharacterAssetNameConvention.NormalizeSelector(value)}.png");

        public static string NotificationPrefab(
            string prefix,
            string assetName) =>
            PresentationPrefab(prefix, "notification", assetName);

        private static string CharacterRoot(string prefix) =>
            $"{ContentPackageConvention.StoryRoot(prefix)}/character/characters";

        private static string NormalizeSelector(string value, string fallback) =>
            string.IsNullOrWhiteSpace(value)
                ? Canonical(fallback)
                : CharacterAssetNameConvention.NormalizeSelector(value);

        private static string NormalizeView(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            var parts = value.Split('/');
            for (var index = 0; index < parts.Length; index++)
                parts[index] = CharacterAssetNameConvention.NormalizeSelector(parts[index]);
            return string.Join("/", parts);
        }

        private static string Canonical(string value) =>
            TechnicalAssetIdConvention.Canonicalize(value);

        private static string PresentationPrefab(
            string prefix,
            string feature,
            string assetName) =>
            Prefab(
                $"{ContentPackageConvention.StoryRoot(prefix)}/presentation/{feature}",
                assetName);

        private static string StoryAsset(
            string prefix,
            string folder,
            string assetName) =>
            Asset($"{ContentPackageConvention.StoryRoot(prefix)}/{folder}", assetName);

        private static string Prefab(string root, string assetName) =>
            IsMissing(assetName) ? string.Empty : $"{root}/{assetName}.prefab";

        private static string Asset(string root, string assetName) =>
            IsMissing(assetName)
                ? string.Empty
                : $"{root}/{TechnicalAssetIdConvention.Canonicalize(assetName)}.png";

        private static string NamedCharacterPath(
            string name,
            string candidate,
            Func<string, string> build) =>
            IsMissing(name) || IsMissing(candidate)
                ? string.Empty
                : build(candidate);

        private static bool IsMissing(string value) => string.IsNullOrEmpty(value);
    }
}
