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

        public static string StoryCoverPath(string contentId, string fileName = "cover.webp") =>
            $"{StoryPrefix(contentId)}/{RequireFileName(fileName, nameof(fileName))}";

        public static string EpisodeRoot(string contentId, string episodeId) =>
            $"{ContentRoot(contentId)}/episodes/{RequireId(episodeId, nameof(episodeId))}";

        public static string DefinitionAsset(string contentId)
        {
            var id = RequireId(contentId, nameof(contentId));
            return $"{ContentRoot(id)}/definition/{id}.asset";
        }

        public static string ContentBundle(string contentId) =>
            $"novels_content_{BundleToken(contentId)}";

        public static string EpisodeBundle(string contentId, string episodeId)
        {
            RequireId(episodeId, nameof(episodeId));
            return ContentBundle(contentId);
        }

        public static string SharedDeliveryGroup(string contentId) =>
            RequireId(contentId, nameof(contentId));

        public static string EpisodeDeliveryGroup(string contentId, string episodeId)
        {
            RequireId(episodeId, nameof(episodeId));
            return RequireId(contentId, nameof(contentId));
        }

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
