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

        public static string MainLoadingPrefab(string assetName) =>
            Prefab("Assets/RemoteAssets/loading", assetName);

        public static string LoadingPrefab(
            string prefix,
            string episodeId,
            string assetName) =>
            Prefab($"{EpisodeRoot(prefix, episodeId)}/loading", assetName);

        public static string SharedLoadingPrefab(string prefix, string assetName) =>
            SharedPresentationPrefab(prefix, "loading", assetName);

        public static string SettingPrefab(string prefix, string assetName) =>
            Prefab($"{ContentPackageConvention.ContentRoot(prefix)}/application/setting", assetName);

        public static string BubblePrefab(
            string prefix,
            string episodeId,
            string assetName) =>
            Prefab($"{EpisodeRoot(prefix, episodeId)}/bubble", assetName);

        public static string SharedBubblePrefab(string prefix, string assetName) =>
            SharedPresentationPrefab(prefix, "bubble", assetName);

        public static string LocationPrefab(
            string prefix,
            string episodeId,
            string assetName) =>
            Prefab($"{EpisodeRoot(prefix, episodeId)}/location", assetName);

        public static string SharedLocationPrefab(string prefix, string assetName) =>
            SharedPresentationPrefab(prefix, "location", assetName);

        public static string LocationImage(
            string prefix,
            string episodeId,
            string assetName) =>
            IsMissing(assetName)
                ? string.Empty
                : $"{EpisodeRoot(prefix, episodeId)}/location/locations/"
                    + $"{TechnicalAssetIdConvention.Canonicalize(assetName)}.png";

        public static string CharacterPrefab(
            string prefix,
            string episodeId,
            string assetName) =>
            Prefab($"{EpisodeRoot(prefix, episodeId)}/character", assetName);

        public static string SharedCharacterPrefab(string prefix, string assetName) =>
            SharedPresentationPrefab(prefix, "character", assetName);

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
                    + $"/emotions/{CharacterAssetNameConvention.NormalizeSelector(value)}.png");

        public static string CharacterClothes(
            string prefix,
            string episodeId,
            string name,
            string candidate,
            int index) =>
            NamedCharacterPath(
                name,
                candidate,
                value => $"{CharacterRoot(prefix, episodeId)}/{Canonical(name)}/clothes/"
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
                value => $"{CharacterRoot(prefix, episodeId)}/{Canonical(name)}/hairs/{Canonical(layer)}/"
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
                value => $"{CharacterRoot(prefix, episodeId)}/{Canonical(name)}/accessories/{Canonical(layer)}/"
                    + $"{CharacterAssetNameConvention.NormalizeSelector(value)}.png");

        public static string SharedCharacterAsset(string prefix, string episodeAssetPath)
        {
            var episodeCharacters = $"{ContentPackageConvention.ContentRoot(prefix)}/episodes/";
            if (IsMissing(episodeAssetPath)
                || !episodeAssetPath.StartsWith(episodeCharacters, StringComparison.Ordinal))
            {
                return string.Empty;
            }

            var characterMarker = "/character/characters/";
            var markerIndex = episodeAssetPath.IndexOf(
                characterMarker,
                episodeCharacters.Length,
                StringComparison.Ordinal);
            return markerIndex < 0
                ? string.Empty
                : $"{ContentPackageConvention.ContentRoot(prefix)}/shared"
                    + episodeAssetPath.Substring(markerIndex);
        }

        public static string NotificationPrefab(
            string prefix,
            string episodeId,
            string assetName) =>
            Prefab($"{EpisodeRoot(prefix, episodeId)}/notification", assetName);

        public static string SharedNotificationPrefab(string prefix, string assetName) =>
            SharedPresentationPrefab(prefix, "notification", assetName);

        private static string EpisodeRoot(string prefix, string episodeId) =>
            ContentPackageConvention.EpisodeRoot(prefix, episodeId);

        private static string CharacterRoot(string prefix, string episodeId) =>
            $"{EpisodeRoot(prefix, episodeId)}/character/characters";

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

        private static string SharedPresentationPrefab(
            string prefix,
            string feature,
            string assetName) =>
            Prefab(
                $"{ContentPackageConvention.ContentRoot(prefix)}/shared/presentation/{feature}",
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
