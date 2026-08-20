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
                : $"NovelTexts/{TechnicalAssetIdConvention.Canonicalize(prefix)}/{path}";

        public static string NovelSourceMap(string prefix, string path) =>
            NovelText(prefix, path) + ".source-map.json";

        public static string MainLoadingPrefab(string assetName) =>
            Prefab("Assets/RemoteAssets/Loading", assetName);

        public static string LoadingPrefab(
            string prefix,
            string episodeId,
            string assetName) =>
            Prefab($"{EpisodeRoot(prefix, episodeId)}/Loading", assetName);

        public static string SharedLoadingPrefab(string prefix, string assetName) =>
            SharedPresentationPrefab(prefix, "Loading", assetName);

        public static string SettingPrefab(string prefix, string assetName) =>
            Prefab($"{ContentPackageConvention.ContentRoot(prefix)}/Application/Setting", assetName);

        public static string BubblePrefab(
            string prefix,
            string episodeId,
            string assetName) =>
            Prefab($"{EpisodeRoot(prefix, episodeId)}/Bubble", assetName);

        public static string SharedBubblePrefab(string prefix, string assetName) =>
            SharedPresentationPrefab(prefix, "Bubble", assetName);

        public static string LocationPrefab(
            string prefix,
            string episodeId,
            string assetName) =>
            Prefab($"{EpisodeRoot(prefix, episodeId)}/Location", assetName);

        public static string SharedLocationPrefab(string prefix, string assetName) =>
            SharedPresentationPrefab(prefix, "Location", assetName);

        public static string LocationImage(
            string prefix,
            string episodeId,
            string assetName) =>
            IsMissing(assetName)
                ? string.Empty
                : $"{EpisodeRoot(prefix, episodeId)}/Location/Locations/"
                    + $"{TechnicalAssetIdConvention.Canonicalize(assetName)}.png";

        public static string CharacterPrefab(
            string prefix,
            string episodeId,
            string assetName) =>
            Prefab($"{EpisodeRoot(prefix, episodeId)}/Character", assetName);

        public static string SharedCharacterPrefab(string prefix, string assetName) =>
            SharedPresentationPrefab(prefix, "Character", assetName);

        public static string CharacterMainBody(
            string prefix,
            string episodeId,
            string name,
            string view,
            string candidate = null) =>
            IsMissing(name)
                ? string.Empty
                : $"{CharacterRoot(prefix, episodeId)}/{Canonical(name)}/{NormalizeView(view)}/"
                    + $"{NormalizeSelector(candidate, "Main")}.png";

        public static string CharacterEmotion(
            string prefix,
            string episodeId,
            string name,
            string view,
            string candidate) =>
            NamedCharacterPath(
                name,
                candidate,
                value => $"{CharacterRoot(prefix, episodeId)}/{Canonical(name)}/{NormalizeView(view)}"
                    + $"/Emotions/{CharacterAssetNameConvention.NormalizeSelector(value)}.png");

        public static string CharacterClothes(
            string prefix,
            string episodeId,
            string name,
            string candidate,
            int index) =>
            NamedCharacterPath(
                name,
                candidate,
                value => $"{CharacterRoot(prefix, episodeId)}/{Canonical(name)}/Clothes/"
                    + $"{CharacterAssetNameConvention.NormalizeSelector(value)}/{index}.png");

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
                value => $"{CharacterRoot(prefix, episodeId)}/{Canonical(name)}/Hairs/{layer}/"
                    + $"{CharacterAssetNameConvention.NormalizeSelector(value)}/"
                    + $"{CharacterAssetNameConvention.NormalizeSelector(color)}.png");

        public static string CharacterAccessory(
            string prefix,
            string episodeId,
            string name,
            string candidate,
            string layer) =>
            NamedCharacterPath(
                name,
                candidate,
                value => $"{CharacterRoot(prefix, episodeId)}/{Canonical(name)}/Accessories/{layer}/"
                    + $"{CharacterAssetNameConvention.NormalizeSelector(value)}.png");

        public static string SharedCharacterAsset(string prefix, string episodeAssetPath)
        {
            var episodeCharacters = $"{ContentPackageConvention.ContentRoot(prefix)}/Episodes/";
            if (IsMissing(episodeAssetPath)
                || !episodeAssetPath.StartsWith(episodeCharacters, StringComparison.Ordinal))
            {
                return string.Empty;
            }

            var characterMarker = "/Character/Characters/";
            var markerIndex = episodeAssetPath.IndexOf(
                characterMarker,
                episodeCharacters.Length,
                StringComparison.Ordinal);
            return markerIndex < 0
                ? string.Empty
                : $"{ContentPackageConvention.ContentRoot(prefix)}/Shared"
                    + episodeAssetPath.Substring(markerIndex);
        }

        public static string NotificationPrefab(
            string prefix,
            string episodeId,
            string assetName) =>
            Prefab($"{EpisodeRoot(prefix, episodeId)}/Notification", assetName);

        public static string SharedNotificationPrefab(string prefix, string assetName) =>
            SharedPresentationPrefab(prefix, "Notification", assetName);

        private static string EpisodeRoot(string prefix, string episodeId) =>
            ContentPackageConvention.EpisodeRoot(prefix, episodeId);

        private static string CharacterRoot(string prefix, string episodeId) =>
            $"{EpisodeRoot(prefix, episodeId)}/Character/Characters";

        private static string NormalizeSelector(string value, string fallback) =>
            string.IsNullOrWhiteSpace(value)
                ? fallback
                : CharacterAssetNameConvention.NormalizeSelector(value);

        private static string NormalizeView(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            var parts = value.Split('/');
            // The first segment is the fixed Unity schema folder (View).
            for (var index = 1; index < parts.Length; index++)
                parts[index] = CharacterAssetNameConvention.NormalizeSelector(parts[index]);
            return string.Join("/", parts);
        }

        private static string Canonical(string value) =>
            TechnicalAssetIdConvention.Canonicalize(value);

        private static string SharedPresentationPrefab(
            string prefix,
            string feature,
            string assetName) =>
            Prefab(
                $"{ContentPackageConvention.ContentRoot(prefix)}/Shared/Presentation/{feature}",
                assetName);

        private static string Prefab(string root, string assetName) =>
            IsMissing(assetName) ? string.Empty : $"{root}/{assetName}.prefab";

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
