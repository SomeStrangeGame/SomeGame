using System;
using System.Text;

namespace Novels.ContentAddressing
{
    public static class ContentAssetNames
    {
        public const string Screen = "screen";
        public const string EpisodeScreen = "screen-variant";
    }

    public static class ContentPackageConvention
    {
        public const string CatalogBundleName = "novels_catalog";
        public const string ApplicationDeliveryGroup = "application";

        public const string CatalogRegistryPath = "catalog/registry/catalog.json";
        public const string CatalogUiPrefix = "catalog/ui";

        private const string _remoteAssetsRoot = "Assets/RemoteAssets";

        public static string ContentRoot(string contentId) =>
            $"{_remoteAssetsRoot}/content/{RequireId(contentId, nameof(contentId))}";

        public static string StoryPrefix(string contentId) =>
            $"stories/{RequireId(contentId, nameof(contentId))}";

        public static string StoryCardPath(string contentId) =>
            $"{StoryPrefix(contentId)}/card.json";

        public static string StoryPreviewPath(string contentId, string platform) =>
            $"{StoryPrefix(contentId)}/Remote/{RequireFileName(platform, nameof(platform))}/catalog-preview.json";

        public static string StoryCoverPath(string contentId, string fileName = "cover.webp") =>
            $"{StoryPrefix(contentId)}/{RequireFileName(fileName, nameof(fileName))}";

        public static string EpisodeCoverFileName(string fileName)
        {
            var value = RequireFileName(fileName, nameof(fileName));
            foreach (var character in value)
                if (!(character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9'
                    or '_' or '-' or '.'))
                    throw new ArgumentException("Episode cover must be a plain ASCII image file name.", nameof(fileName));
            if (!value.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                && !value.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                && !value.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Episode cover must be PNG or JPEG.", nameof(fileName));
            return value;
        }

        public static string StoryEpisodeCoverPath(string contentId, string platform, string fileName) =>
            $"{StoryPrefix(contentId)}/Remote/{RequireFileName(platform, nameof(platform))}/episode-covers/{EpisodeCoverFileName(fileName)}";

        public static string CatalogVideoFileName(string fileName)
        {
            var value = RequireFileName(fileName, nameof(fileName));
            foreach (var character in value)
                if (!(character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9'
                    or '_' or '-' or '.'))
                    throw new ArgumentException("Catalog video must be a plain ASCII MP4 file name.", nameof(fileName));
            if (!value.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Catalog video must be MP4.", nameof(fileName));
            return value;
        }

        public static string StoryCatalogVideoPath(string contentId, string platform, string fileName) =>
            $"{StoryPrefix(contentId)}/Remote/{RequireFileName(platform, nameof(platform))}/catalog-videos/{CatalogVideoFileName(fileName)}";

        public static string StoryRoot(string contentId) =>
            $"{ContentRoot(contentId)}/story";

        public static string DefinitionAsset(string contentId)
        {
            var id = RequireId(contentId, nameof(contentId));
            return $"{ContentRoot(id)}/definition/{id}.asset";
        }

        public static string ContentBundle(string contentId) =>
            $"novels_content_{BundleToken(contentId)}";

        public static string StoryChunkBundle(string contentId, int index) =>
            $"{ContentBundle(contentId)}_chunk_{RequireIndex(index)}";

        public static string StoryDeliveryGroup(string contentId) =>
            RequireId(contentId, nameof(contentId));

        public static string StoryChunkDeliveryGroup(string contentId, int index) =>
            $"{RequireId(contentId, nameof(contentId))}-chunk-{RequireIndex(index)}";

        public static string StoryMediaDeliveryGroup(string contentId, int index) =>
            $"{RequireId(contentId, nameof(contentId))}-media-{RequireIndex(index)}";

        public static string StoryMediaDeliveryGroup(string contentId) =>
            $"{RequireId(contentId, nameof(contentId))}-media";

        public static string ContentPayload(string sha256) =>
            $"Files/{RequireId(sha256, nameof(sha256))}.bin";

        private static string BundleToken(string value)
        {
            var source = RequireId(value, nameof(value));
            var result = new StringBuilder(source.Length);
            foreach (var character in source)
            {
                result.Append(char.IsLetterOrDigit(character)
                    ? char.ToLowerInvariant(character)
                    : '_');
            }
            return result.ToString();
        }

        private static int RequireIndex(int value) => value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value));

        private static string RequireId(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Content identifier must not be empty.", parameterName);
            var result = value.Trim();
            foreach (var character in result)
            {
                var isAsciiLetter = character is >= 'A' and <= 'Z'
                    or >= 'a' and <= 'z';
                var isDigit = character is >= '0' and <= '9';
                if (!isAsciiLetter && !isDigit && character != '_' && character != '-')
                {
                    throw new ArgumentException(
                        "Content identifier may contain only ASCII letters, digits, "
                        + "underscores, and hyphens.",
                        parameterName);
                }
            }
            return result.ToLowerInvariant();
        }

        private static string RequireFileName(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("File name must not be empty.", parameterName);
            var result = value.Trim();
            if (result.Contains("/") || result.Contains("\\") || result.Contains(".."))
                throw new ArgumentException("File name must not contain a path.", parameterName);
            return result;
        }
    }
}
