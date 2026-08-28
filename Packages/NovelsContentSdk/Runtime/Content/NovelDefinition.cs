using System;
using System.Collections.Generic;
using System.Linq;

namespace Novels.Content
{
    public sealed class NovelDefinition
    {
        private readonly IReadOnlyDictionary<string, string> _videoAliases;
        private readonly IReadOnlyDictionary<string, string> _artAliases;

        public NovelDefinition(
            string id,
            string mainCharacter,
            string contentVersion,
            string endMarker,
            IEnumerable<string> silentAudioIds,
            EpisodeDefinition episode,
            IEnumerable<VideoAliasDefinition> videoAliases = null,
            IEnumerable<CharacterDefaultAppearanceDefinition> characterDefaults = null,
            IEnumerable<ArtAliasDefinition> artAliases = null)
            : this(
                id,
                mainCharacter,
                contentVersion,
                endMarker,
                silentAudioIds,
                new[] { episode },
                videoAliases,
                characterDefaults,
                artAliases)
        {
        }

        public NovelDefinition(
            string id,
            string mainCharacter,
            string contentVersion,
            string endMarker,
            IEnumerable<string> silentAudioIds,
            IEnumerable<EpisodeDefinition> episodes,
            IEnumerable<VideoAliasDefinition> videoAliases = null,
            IEnumerable<CharacterDefaultAppearanceDefinition> characterDefaults = null,
            IEnumerable<ArtAliasDefinition> artAliases = null)
        {
            Id = Require(id, nameof(id));
            MainCharacter = Require(mainCharacter, nameof(mainCharacter));
            StoryPath = Id + ".ink.json";
            ContentVersion = Require(contentVersion, nameof(contentVersion));
            EndMarker = endMarker?.Trim() ?? string.Empty;
            SilentAudioIds = Array.AsReadOnly(
                (silentAudioIds ?? Array.Empty<string>())
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray());
            Prefix = Id;
            BundleName = ContentAddressing.ContentPackageConvention.ContentBundle(Id);
            CharacterAssets = new CharacterAssetProfile(mainCharacter, characterDefaults);
            var episodeArray = episodes?.ToArray() ?? Array.Empty<EpisodeDefinition>();
            if (episodeArray.Length == 0 || episodeArray.Any(episode => episode == null))
                throw new ArgumentException("At least one valid episode is required.", nameof(episodes));
            var episodeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var episode in episodeArray)
            {
                if (!string.Equals(episode.ContentId, Id, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(
                        $"Episode '{episode.Id}' belongs to content '{episode.ContentId}', "
                        + $"not '{Id}'.",
                        nameof(episodes));
                }
                if (!episodeIds.Add(episode.Id))
                {
                    throw new ArgumentException(
                        $"Duplicate episode ID '{episode.Id}'.",
                        nameof(episodes));
                }
            }
            Episodes = Array.AsReadOnly(episodeArray);
            var aliases = (videoAliases ?? Array.Empty<VideoAliasDefinition>()).ToArray();
            if (aliases.Any(alias => alias == null))
            {
                throw new ArgumentException(
                    "Video aliases must not contain null values.",
                    nameof(videoAliases));
            }
            VideoAliases = Array.AsReadOnly(aliases);
            _videoAliases = BuildVideoAliases(aliases);
            var artAliasArray = (artAliases ?? Array.Empty<ArtAliasDefinition>())
                .ToArray();
            if (artAliasArray.Any(alias => alias == null))
            {
                throw new ArgumentException(
                    "Art aliases must not contain null values.",
                    nameof(artAliases));
            }
            ArtAliases = Array.AsReadOnly(artAliasArray);
            _artAliases = BuildArtAliases(artAliasArray);
        }

        public string Id { get; }
        public string Prefix { get; }
        public string MainCharacter { get; }
        public string StoryPath { get; }
        public string ContentVersion { get; }
        public string EndMarker { get; }
        public IReadOnlyList<string> SilentAudioIds { get; }
        public string BundleName { get; }
        public CharacterAssetProfile CharacterAssets { get; }
        public IReadOnlyList<EpisodeDefinition> Episodes { get; }
        public IReadOnlyList<VideoAliasDefinition> VideoAliases { get; }
        public IReadOnlyList<ArtAliasDefinition> ArtAliases { get; }

        public string ResolveVideoId(string value)
        {
            var result = ContentAddressing.TechnicalAssetIdConvention.Canonicalize(value);
            while (_videoAliases.TryGetValue(result, out var target))
                result = target;
            return result;
        }

        public string ResolveArtAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            var address = ContentAddressing.TechnicalAssetIdConvention
                .Canonicalize(value)
                .Replace('\\', '/')
                .Trim('/');
            var contentPrefix = ContentAddressing.ContentPackageConvention
                .ContentRoot(Id) + "/";
            if (!address.StartsWith(contentPrefix, StringComparison.OrdinalIgnoreCase))
                return value;
            var relative = address.Substring(contentPrefix.Length);
            while (_artAliases.TryGetValue(relative, out var target))
                relative = target;
            return contentPrefix + relative;
        }

        private static IReadOnlyDictionary<string, string> BuildVideoAliases(
            IEnumerable<VideoAliasDefinition> aliases)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var alias in aliases)
            {
                if (!result.TryAdd(alias.Alias, alias.Target))
                    throw new ArgumentException($"Duplicate video alias '{alias.Alias}'.");
            }
            foreach (var alias in result.Keys)
            {
                var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var current = alias;
                while (result.TryGetValue(current, out current))
                {
                    if (!visited.Add(current))
                        throw new ArgumentException($"Video alias cycle contains '{current}'.");
                }
            }
            return result;
        }

        private static IReadOnlyDictionary<string, string> BuildArtAliases(
            IEnumerable<ArtAliasDefinition> aliases)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var alias in aliases)
            {
                if (!result.TryAdd(alias.Alias, alias.Target))
                    throw new ArgumentException($"Duplicate art alias '{alias.Alias}'.");
            }
            foreach (var alias in result.Keys)
            {
                var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var current = alias;
                while (result.TryGetValue(current, out current))
                {
                    if (!visited.Add(current))
                        throw new ArgumentException($"Art alias cycle contains '{current}'.");
                }
            }
            return result;
        }

        private static string Require(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Content value must not be empty.", parameterName);
            return value;
        }
    }

    public sealed class ArtAliasDefinition
    {
        public ArtAliasDefinition(string alias, string target)
        {
            Alias = Normalize(alias, nameof(alias));
            Target = Normalize(target, nameof(target));
            if (string.Equals(Alias, Target, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Art alias must differ from its target.");
        }

        public string Alias { get; }
        public string Target { get; }

        private static string Normalize(string value, string parameterName)
        {
            var result = ContentAddressing.TechnicalAssetIdConvention
                .Canonicalize(value)
                .Replace('\\', '/')
                .Trim('/');
            if (!result.StartsWith("story/", StringComparison.Ordinal)
                || !result.EndsWith(".png", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Art alias paths must start with 'story/' and end with '.png'.",
                    parameterName);
            }
            if (result.Split('/').Any(part => string.IsNullOrEmpty(part)
                    || part == "."
                    || part == ".."))
            {
                throw new ArgumentException(
                    "Art alias paths must not contain empty or relative segments.",
                    parameterName);
            }
            return result;
        }
    }

    public sealed class CharacterDefaultAppearanceDefinition
    {
        public CharacterDefaultAppearanceDefinition(
            string character,
            string clothes,
            string hair,
            string hairColor,
            string accessory)
        {
            Character = character ?? string.Empty;
            Clothes = clothes ?? string.Empty;
            Hair = hair ?? string.Empty;
            HairColor = hairColor ?? string.Empty;
            Accessory = accessory ?? string.Empty;
        }

        public string Character { get; }
        public string Clothes { get; }
        public string Hair { get; }
        public string HairColor { get; }
        public string Accessory { get; }
    }

    public sealed class EpisodeDefinition
    {
        public EpisodeDefinition(
            string contentId,
            string id,
            string title,
            string description)
        {
            ContentId = Require(contentId, nameof(contentId));
            Id = Require(id, nameof(id));
            Title = Require(title, nameof(title));
            Description = description?.Trim() ?? string.Empty;
        }

        public string ContentId { get; }
        public string Id { get; }
        public string Title { get; }
        public string Description { get; }

        private static string Require(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Content value must not be empty.", parameterName);
            return value;
        }
    }

}
