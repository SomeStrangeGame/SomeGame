using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Bundles;
using UnityEditor;

namespace Novels.ContentSdk.Editor
{
    internal sealed class ExperimentalStreamingBuildPlan
    {
        internal ExperimentalStreamingBuildPlan(
            IReadOnlyList<string[]> chunks,
            IReadOnlyList<ContentStreamingMediaEntry> media)
        {
            Chunks = chunks;
            Media = media;
        }

        internal IReadOnlyList<string[]> Chunks { get; }
        internal IReadOnlyList<ContentStreamingMediaEntry> Media { get; }
    }

    internal enum StoryAssetUsageKind
    {
        Art,
        Prefab,
        Video,
        Audio,
    }

    internal enum StoryAssetUsageMatch
    {
        Unknown,
        Direct,
        Dynamic,
    }

    internal sealed class StoryAssetUsageEntry
    {
        internal StoryAssetUsageEntry(
            string path,
            StoryAssetUsageKind kind,
            int firstUse,
            long sourceBytes,
            StoryAssetUsageMatch match = StoryAssetUsageMatch.Direct)
        {
            Path = path;
            Kind = kind;
            FirstUse = firstUse;
            SourceBytes = sourceBytes;
            Match = firstUse == int.MaxValue
                ? StoryAssetUsageMatch.Unknown
                : match;
        }

        internal string Path { get; }
        internal StoryAssetUsageKind Kind { get; }
        internal int FirstUse { get; }
        internal long SourceBytes { get; }
        internal StoryAssetUsageMatch Match { get; }
        internal bool IsReferenced => FirstUse != int.MaxValue;
    }

    [Serializable]
    internal sealed class StoryChunkLayout
    {
        public StoryChunkLayoutEntry[] chunks = Array.Empty<StoryChunkLayoutEntry>();
    }

    [Serializable]
    internal sealed class StoryChunkLayoutEntry
    {
        public int index;
        public string[] assets = Array.Empty<string>();
    }

    internal static class ExperimentalStreamingPlan
    {
        private const long _defaultChunkSourceBytes = 16L * 1024L * 1024L;
        private static readonly HashSet<string> _technicalAssetTokens = new(
            StringComparer.Ordinal)
        {
            "story",
            "assets",
            "presentation",
            "choose",
            "choices",
            "character",
            "characters",
            "maincharacter",
            "location",
            "locations",
            "view",
            "emotions",
            "clothes",
            "hair",
            "hairs",
            "back",
            "front",
            "accessory",
            "accessories",
            "child",
            "main",
        };

        internal static ExperimentalStreamingBuildPlan Create(
            string storyId,
            IReadOnlyCollection<string> assets,
            IReadOnlyCollection<string> filePaths)
        {
            var authored = TryCreateAuthoredPlan(storyId, assets, filePaths);
            if (authored != null)
                return authored;

            var storyText = ReadStoryText(storyId);
            var targetBytes = ReadChunkTarget();
            var firstSceneEnd = FindFirstSceneEnd(storyText);
            var bootstrapAssets = FindBootstrapAssets(assets);
            var orderedAssets = assets
                .Select(path => new
                {
                    Path = path,
                    FirstUse = FirstAssetUse(storyText, path),
                    Size = SourceSize(path),
                    Bootstrap = bootstrapAssets.Contains(path),
                })
                .Select(value => new
                {
                    value.Path,
                    value.FirstUse,
                    value.Size,
                    value.Bootstrap,
                    Startup = firstSceneEnd > 0
                        && value.FirstUse >= 0
                        && value.FirstUse < firstSceneEnd,
                })
                .OrderBy(value => value.Bootstrap ? 0 : value.Startup ? 1 : 2)
                .ThenBy(value => value.FirstUse)
                .ThenBy(value => value.Path, StringComparer.Ordinal)
                .ToArray();
            var chunks = new List<string[]>();
            var current = new List<string>();
            long currentBytes = 0;
            foreach (var asset in orderedAssets)
            {
                if (current.Count > 0
                    && !asset.Bootstrap
                    && currentBytes + asset.Size > targetBytes)
                {
                    chunks.Add(current.ToArray());
                    current.Clear();
                    currentBytes = 0;
                }
                current.Add(asset.Path);
                currentBytes += asset.Size;
            }
            if (current.Count > 0)
                chunks.Add(current.ToArray());
            if (chunks.Count == 0)
                throw new InvalidOperationException("Streaming plan contains no art chunks.");

            var media = filePaths
                .Where(IsMediaPath)
                .Select(path => new
                {
                    Path = path,
                    FirstUse = FirstMediaUse(storyText, path),
                })
                .OrderBy(value => value.FirstUse)
                .ThenBy(value => value.Path, StringComparer.Ordinal)
                .Select((value, index) => new ContentStreamingMediaEntry
                {
                    order = index,
                    path = value.Path,
                    deliveryGroup = ContentAddressing.ContentPackageConvention
                        .StoryMediaDeliveryGroup(storyId, index),
                })
                .ToArray();
            return new ExperimentalStreamingBuildPlan(chunks, media);
        }

        internal static StoryChunkLayout CreateChunkLayout(
            IReadOnlyList<StoryAssetUsageEntry> entries,
            long targetBytes)
        {
            if (entries == null)
                throw new ArgumentNullException(nameof(entries));
            if (targetBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(targetBytes));

            var chunks = new List<StoryChunkLayoutEntry>();
            var current = new List<string>();
            long currentBytes = 0;
            foreach (var entry in entries.Where(entry => entry.IsReferenced))
            {
                var contributesToBundle = entry.Kind == StoryAssetUsageKind.Art
                                          || entry.Kind == StoryAssetUsageKind.Prefab;
                if (contributesToBundle
                    && currentBytes > 0
                    && currentBytes + entry.SourceBytes > targetBytes)
                {
                    chunks.Add(new StoryChunkLayoutEntry
                    {
                        index = chunks.Count,
                        assets = current.ToArray(),
                    });
                    current.Clear();
                    currentBytes = 0;
                }
                current.Add(entry.Path);
                if (contributesToBundle)
                    currentBytes += entry.SourceBytes;
            }
            if (current.Count > 0)
            {
                chunks.Add(new StoryChunkLayoutEntry
                {
                    index = chunks.Count,
                    assets = current.ToArray(),
                });
            }
            return new StoryChunkLayout {chunks = chunks.ToArray()};
        }

        internal static StoryAssetUsageEntry[] CreateLinearUsageReport(
            string inkPath,
            IReadOnlyCollection<string> assets,
            IReadOnlyCollection<string> filePaths,
            Content.NovelDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            var compiledText = File.ReadAllText(inkPath)
                .Normalize(NormalizationForm.FormC)
                .ToLowerInvariant();
            var sourcePath = inkPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                ? inkPath.Substring(0, inkPath.Length - ".json".Length)
                : inkPath.EndsWith(".ink", StringComparison.OrdinalIgnoreCase)
                    ? inkPath
                    : string.Empty;
            var storyText = File.Exists(sourcePath)
                ? ReadInkSourceTree(sourcePath)
                    .Normalize(NormalizationForm.FormC)
                    .ToLowerInvariant()
                : compiledText;
            var usage = StoryUsageIndex.Create(storyText, definition);
            var dynamicUses = FindDynamicAssetUses(storyText);
            var art = assets.Select(path =>
            {
                var kind = string.Equals(
                    Path.GetExtension(path),
                    ".prefab",
                    StringComparison.OrdinalIgnoreCase)
                    ? StoryAssetUsageKind.Prefab
                    : StoryAssetUsageKind.Art;
                var dynamicUse = FirstDynamicAssetUse(path, dynamicUses);
                var directUse = FirstAuthoredAssetUse(
                    storyText,
                    path,
                    usage,
                    definition.MainCharacter);
                var isDynamic = dynamicUse < directUse;
                var firstUse = Math.Min(dynamicUse, directUse);
                return new StoryAssetUsageEntry(
                    path,
                    kind,
                    firstUse,
                    SourceSize(path),
                    isDynamic
                        ? StoryAssetUsageMatch.Dynamic
                        : StoryAssetUsageMatch.Direct);
            });
            var media = filePaths
                .Where(IsMediaPath)
                .Select(path => new StoryAssetUsageEntry(
                    path,
                    path.StartsWith(
                        "novelsvideos/",
                        StringComparison.OrdinalIgnoreCase)
                        ? StoryAssetUsageKind.Video
                        : StoryAssetUsageKind.Audio,
                    usage.FirstMediaUse(path),
                    StreamingAssetSourceSize(path)));
            return art
                .Concat(media)
                .OrderBy(value => value.Kind == StoryAssetUsageKind.Prefab ? 0 : 1)
                .ThenBy(value => value.FirstUse)
                .ThenBy(value => value.Path, StringComparer.Ordinal)
                .ToArray();
        }

        private sealed class StoryUsageIndex
        {
            private readonly Dictionary<string, int> _backgrounds = new(
                StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<string, int> _videos = new(
                StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<string, int> _audio = new(
                StringComparer.OrdinalIgnoreCase);
            private readonly List<CharacterUse> _characters = new();

            internal static StoryUsageIndex Create(
                string storyText,
                Content.NovelDefinition definition)
            {
                var result = new StoryUsageIndex();
                var parser = new StoryCommands.Entity();
                var lines = (storyText ?? string.Empty).Split('\n');
                for (var index = 0; index < lines.Length; index++)
                {
                    var parsed = parser.Parse(lines[index].TrimEnd('\r'), false);
                    if (!parsed.IsSuccess)
                        continue;
                    var lineNumber = index + 1;
                    if (parsed.Command is StoryCommands.BackgroundStoryCommand background)
                    {
                        var assetName = Canonicalize(background.Data.AssetName);
                        result.Add(result._backgrounds, assetName, lineNumber);
                        result.Add(
                            result._videos,
                            definition.ResolveVideoId(assetName),
                            lineNumber);
                    }
                    else if (parsed.Command is StoryCommands.AudioStoryCommand audio)
                    {
                        result.Add(
                            result._audio,
                            Canonicalize(audio.Data.AssetName),
                            lineNumber);
                    }
                    else if (parsed.Command is StoryCommands.DialogueStoryCommand dialogue)
                    {
                        result._characters.Add(new CharacterUse(
                            lineNumber,
                            Canonicalize(dialogue.Data.Speaker),
                            dialogue.Data.Character.IsChild,
                            dialogue.Data.Character.AssetCandidates
                                .Select(Canonicalize)
                                .Where(value => !string.IsNullOrEmpty(value))
                                .ToArray()));
                    }
                }
                return result;
            }

            internal int FirstMediaUse(string path)
            {
                var name = Canonicalize(Path.GetFileNameWithoutExtension(path));
                var index = path.StartsWith(
                        "novelsvideos/",
                        StringComparison.OrdinalIgnoreCase)
                    ? _videos
                    : _audio;
                return index.TryGetValue(name, out var line)
                    ? line
                    : int.MaxValue;
            }

            internal bool TryFirstAssetUse(
                string assetPath,
                string mainCharacter,
                out int firstUse)
            {
                if (TryLocationName(assetPath, out var location))
                {
                    firstUse = _backgrounds.TryGetValue(location, out var line)
                        ? line
                        : int.MaxValue;
                    return true;
                }
                if (!CharacterAssetUse.TryCreate(assetPath, out var asset))
                {
                    firstUse = int.MaxValue;
                    return false;
                }

                var character = string.Equals(
                        asset.Character,
                        "maincharacter",
                        StringComparison.OrdinalIgnoreCase)
                    ? Canonicalize(mainCharacter)
                    : asset.Character;
                firstUse = int.MaxValue;
                foreach (var use in _characters)
                {
                    if (!string.Equals(
                            use.Character,
                            character,
                            StringComparison.OrdinalIgnoreCase)
                        || asset.IsChild && !use.IsChild)
                    {
                        continue;
                    }
                    var matches = asset.Category == CharacterAssetCategory.View
                        ? !string.Equals(
                            asset.Character,
                            "maincharacter",
                            StringComparison.OrdinalIgnoreCase)
                          || use.Candidates.Contains(
                              asset.Selector,
                              StringComparer.OrdinalIgnoreCase)
                        : use.Candidates.Contains(
                            asset.Selector,
                            StringComparer.OrdinalIgnoreCase);
                    if (matches && use.LineNumber < firstUse)
                        firstUse = use.LineNumber;
                }
                return true;
            }

            private static bool TryLocationName(string path, out string name)
            {
                var normalized = path.Replace('\\', '/');
                if (normalized.StartsWith(
                        "Assets/Locations/",
                        StringComparison.OrdinalIgnoreCase)
                    || normalized.IndexOf(
                        "/story/location/locations/",
                        StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    name = Canonicalize(Path.GetFileNameWithoutExtension(normalized));
                    return true;
                }
                name = string.Empty;
                return false;
            }

            private void Add(
                IDictionary<string, int> index,
                string key,
                int lineNumber)
            {
                if (string.IsNullOrEmpty(key))
                    return;
                if (!index.TryGetValue(key, out var existing)
                    || lineNumber < existing)
                {
                    index[key] = lineNumber;
                }
            }
        }

        private readonly struct CharacterUse
        {
            internal CharacterUse(
                int lineNumber,
                string character,
                bool isChild,
                string[] candidates)
            {
                LineNumber = lineNumber;
                Character = character;
                IsChild = isChild;
                Candidates = candidates ?? Array.Empty<string>();
            }

            internal int LineNumber { get; }
            internal string Character { get; }
            internal bool IsChild { get; }
            internal string[] Candidates { get; }
        }

        private enum CharacterAssetCategory
        {
            View,
            Emotion,
            Clothes,
            Hair,
            Accessory,
        }

        private readonly struct CharacterAssetUse
        {
            private CharacterAssetUse(
                string character,
                CharacterAssetCategory category,
                string selector,
                bool isChild)
            {
                Character = character;
                Category = category;
                Selector = selector;
                IsChild = isChild;
            }

            internal string Character { get; }
            internal CharacterAssetCategory Category { get; }
            internal string Selector { get; }
            internal bool IsChild { get; }

            internal static bool TryCreate(
                string assetPath,
                out CharacterAssetUse result)
            {
                var segments = assetPath.Replace('\\', '/')
                    .Split('/')
                    .Select(Canonicalize)
                    .ToArray();
                if (!TryCharacterRoot(segments, out var characters)
                    || characters + 2 >= segments.Length)
                {
                    result = default;
                    return false;
                }

                var character = segments[characters + 1];
                var isChild = segments.Contains("child");
                if (TrySelector(
                        segments,
                        "emotions",
                        1,
                        out var selector))
                {
                    result = new CharacterAssetUse(
                        character,
                        CharacterAssetCategory.Emotion,
                        selector,
                        isChild);
                    return true;
                }
                if (TrySelector(segments, "clothes", 1, out selector))
                {
                    result = new CharacterAssetUse(
                        character,
                        CharacterAssetCategory.Clothes,
                        selector,
                        isChild);
                    return true;
                }
                if (TrySelector(segments, "hairs", 2, out selector))
                {
                    result = new CharacterAssetUse(
                        character,
                        CharacterAssetCategory.Hair,
                        selector,
                        isChild);
                    return true;
                }
                if (TrySelector(segments, "accessories", 2, out selector))
                {
                    result = new CharacterAssetUse(
                        character,
                        CharacterAssetCategory.Accessory,
                        selector,
                        isChild);
                    return true;
                }
                if (TrySelector(segments, "view", 1, out selector))
                {
                    result = new CharacterAssetUse(
                        character,
                        CharacterAssetCategory.View,
                        selector,
                        isChild);
                    return true;
                }
                result = default;
                return false;
            }

            private static bool TryCharacterRoot(
                string[] segments,
                out int characters)
            {
                if (segments.Length >= 2
                    && string.Equals(segments[0], "assets", StringComparison.Ordinal)
                    && string.Equals(segments[1], "characters", StringComparison.Ordinal))
                {
                    characters = 1;
                    return true;
                }
                for (var index = 2; index < segments.Length; index++)
                {
                    if (string.Equals(segments[index - 2], "story", StringComparison.Ordinal)
                        && string.Equals(segments[index - 1], "character", StringComparison.Ordinal)
                        && string.Equals(segments[index], "characters", StringComparison.Ordinal))
                    {
                        characters = index;
                        return true;
                    }
                }
                characters = -1;
                return false;
            }

            private static bool TrySelector(
                string[] segments,
                string category,
                int offset,
                out string selector)
            {
                var index = Array.FindIndex(
                    segments,
                    value => string.Equals(value, category, StringComparison.Ordinal));
                var selectorIndex = index + offset;
                if (index < 0 || selectorIndex >= segments.Length)
                {
                    selector = string.Empty;
                    return false;
                }
                selector = Path.GetFileNameWithoutExtension(segments[selectorIndex]);
                return !string.IsNullOrEmpty(selector);
            }
        }

        private static int FirstAuthoredAssetUse(
            string storyText,
            string assetPath,
            StoryUsageIndex usage,
            string mainCharacter)
        {
            if (IsBootstrapAsset(assetPath))
                return -1;
            if (IsLegacyPresentationStoryArt(assetPath))
                return int.MaxValue;
            if (usage.TryFirstAssetUse(assetPath, mainCharacter, out var firstUse))
                return firstUse;

            var token = MostSpecificAssetToken(assetPath);
            if (string.IsNullOrEmpty(token))
                return int.MaxValue;
            var position = storyText.IndexOf(token, StringComparison.Ordinal);
            return position >= 0
                ? ToLineNumber(storyText, position)
                : int.MaxValue;
        }

        private static bool IsLegacyPresentationStoryArt(string assetPath)
        {
            var path = assetPath.Replace('\\', '/');
            return path.StartsWith(
                       "Assets/Presentation/character/characters/",
                       StringComparison.OrdinalIgnoreCase)
                   || path.StartsWith(
                       "Assets/Presentation/location/locations/",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static string MostSpecificAssetToken(string assetPath)
        {
            var segments = assetPath.Replace('\\', '/')
                .Split('/');
            for (var index = segments.Length - 1; index >= 0; index--)
            {
                var token = Canonicalize(index == segments.Length - 1
                    ? Path.GetFileNameWithoutExtension(segments[index])
                    : segments[index]);
                if (token.Length < 2
                    || token.All(char.IsDigit)
                    || _technicalAssetTokens.Contains(token))
                {
                    continue;
                }
                return token;
            }
            return string.Empty;
        }

        private static string Canonicalize(string value) =>
            ContentAddressing.TechnicalAssetIdConvention.Canonicalize(value);

        private readonly struct DynamicAssetUse
        {
            internal DynamicAssetUse(
                string character,
                string category,
                string selector,
                int position)
            {
                Character = character;
                Category = category;
                Selector = selector;
                Position = position;
            }

            internal string Character { get; }
            internal string Category { get; }
            internal string Selector { get; }
            internal int Position { get; }
        }

        private static DynamicAssetUse[] FindDynamicAssetUses(string storyText)
        {
            var result = new List<DynamicAssetUse>();
            var lines = Regex.Matches(storyText, @".*(?:\r?\n|$)")
                .Cast<Match>()
                .Where(match => match.Length > 0)
                .ToArray();
            for (var index = 0; index < lines.Length; index++)
            {
                var line = lines[index].Value.Trim();
                if (!line.StartsWith("гардероб", StringComparison.Ordinal))
                    continue;
                var category = WardrobeCategory(line);
                if (string.IsNullOrEmpty(category))
                    continue;
                var character = WardrobeCharacter(line);
                for (var choiceIndex = index + 1;
                     choiceIndex < lines.Length;
                     choiceIndex++)
                {
                    var choiceLine = lines[choiceIndex].Value.Trim();
                    if (choiceLine == "-")
                        break;
                    var choice = Regex.Match(choiceLine, @"^[+*].*?\[([^\]]+)\]");
                    if (!choice.Success)
                        continue;
                    result.Add(new DynamicAssetUse(
                        character,
                        category,
                        ContentAddressing.CharacterAssetNameConvention.NormalizeSelector(
                            choice.Groups[1].Value),
                        ToLineNumber(storyText, lines[index].Index)));
                }
            }

            var assignments = Regex.Matches(
                    storyText,
                    @"(?m)^\s*~\s*([\p{L}\p{N}_]+)\s*=\s*""([^""]+)""")
                .Cast<Match>()
                .GroupBy(match => match.Groups[1].Value, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(match => match.Groups[2].Value).Distinct().ToArray(),
                    StringComparer.Ordinal);
            foreach (var variable in assignments)
            {
                foreach (Match use in Regex.Matches(
                             storyText,
                             $@"(?m)^[^:\r\n]+\([^\r\n)]*\{{{Regex.Escape(variable.Key)}\}}"))
                {
                    foreach (var value in variable.Value)
                    {
                        result.Add(new DynamicAssetUse(
                            "maincharacter",
                            "clothes",
                            ContentAddressing.CharacterAssetNameConvention.NormalizeSelector(value),
                            ToLineNumber(storyText, use.Index)));
                    }
                }
            }
            return result.ToArray();
        }

        private static int ToLineNumber(string text, int position)
        {
            if (position == int.MaxValue)
                return int.MaxValue;
            var line = 1;
            for (var index = 0; index < position && index < text.Length; index++)
            {
                if (text[index] == '\n')
                    line++;
            }
            return line;
        }

        private static string WardrobeCategory(string line)
        {
            if (line.Contains("внешност")) return "view";
            if (line.Contains("прич")) return "hairs";
            if (line.Contains("аксесс")) return "accessories";
            if (line.Contains("одеж") || line.Contains("купальник")) return "clothes";
            return string.Empty;
        }

        private static string WardrobeCharacter(string line)
        {
            var headerEnd = line.IndexOf('(');
            var header = headerEnd >= 0 ? line.Substring(0, headerEnd) : line;
            var value = header.Substring("гардероб".Length).Trim();
            return string.IsNullOrWhiteSpace(value)
                ? "maincharacter"
                : ContentAddressing.CharacterAssetNameConvention.NormalizeSelector(value);
        }

        private static int FirstDynamicAssetUse(
            string assetPath,
            IReadOnlyCollection<DynamicAssetUse> uses)
        {
            var path = assetPath.Replace('\\', '/')
                .Normalize(NormalizationForm.FormC)
                .ToLowerInvariant();
            if (!path.Contains("/story/character/characters/")
                && !path.StartsWith("assets/characters/", StringComparison.Ordinal))
                return int.MaxValue;
            var result = int.MaxValue;
            foreach (var use in uses)
            {
                var character = $"/characters/{use.Character}/";
                if (!path.Contains(character))
                    continue;
                var matches = use.Category switch
                {
                    "view" => path.EndsWith(
                        $"/view/{use.Selector}/main.png",
                        StringComparison.Ordinal),
                    "clothes" => path.Contains($"/clothes/{use.Selector}/"),
                    "hairs" => path.Contains("/hairs/")
                               && path.Contains($"/{use.Selector}/"),
                    "accessories" => path.Contains("/accessories/")
                                     && Path.GetFileNameWithoutExtension(path) == use.Selector,
                    _ => false,
                };
                if (matches && use.Position < result)
                    result = use.Position;
            }
            return result;
        }

        private static string ReadInkSourceTree(string rootPath)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return ReadInkSource(rootPath, visited);
        }

        private static string ReadInkSource(string path, HashSet<string> visited)
        {
            path = Path.GetFullPath(path);
            if (!visited.Add(path))
                return string.Empty;
            var directory = Path.GetDirectoryName(path) ?? string.Empty;
            var result = new StringBuilder();
            foreach (var line in File.ReadLines(path))
            {
                var include = Regex.Match(
                    line,
                    @"^\s*INCLUDE\s+([^\s]+\.ink)\s*$",
                    RegexOptions.IgnoreCase);
                if (include.Success)
                {
                    result.AppendLine(ReadInkSource(
                        Path.Combine(directory, include.Groups[1].Value),
                        visited));
                }
                else
                {
                    result.AppendLine(line);
                }
            }
            return result.ToString();
        }

        private static HashSet<string> FindBootstrapAssets(
            IReadOnlyCollection<string> assets)
        {
            var availableAssets = new HashSet<string>(assets, StringComparer.Ordinal);
            var bootstrapRoots = assets
                .Where(IsBootstrapAsset)
                .ToArray();
            var bootstrapAssets = new HashSet<string>(
                bootstrapRoots,
                StringComparer.Ordinal);
            if (bootstrapRoots.Length == 0)
                return bootstrapAssets;

            foreach (var dependency in AssetDatabase.GetDependencies(
                         bootstrapRoots,
                         true))
            {
                if (availableAssets.Contains(dependency))
                    bootstrapAssets.Add(dependency);
            }
            return bootstrapAssets;
        }

        private static ExperimentalStreamingBuildPlan TryCreateAuthoredPlan(
            string storyId,
            IReadOnlyCollection<string> assets,
            IReadOnlyCollection<string> filePaths)
        {
            if (!StoryChunkAuthoring.TryReadLayout(storyId, out var layout))
                return null;
            var definitions = layout?.chunks ?? Array.Empty<StoryChunkLayoutEntry>();
            if (definitions.Length == 0)
                return null;

            var availableArt = new HashSet<string>(assets, StringComparer.Ordinal);
            var availableFiles = new HashSet<string>(filePaths, StringComparer.OrdinalIgnoreCase);
            var orderedDefinitions = definitions
                .OrderBy(value => value.index)
                .ToArray();
            var unavailable = orderedDefinitions
                .SelectMany(value => value.assets ?? Array.Empty<string>())
                .Where(path => !availableArt.Contains(path)
                               && !(IsMediaPath(path) && availableFiles.Contains(path)))
                .ToArray();
            if (unavailable.Length > 0)
            {
                throw new InvalidOperationException(
                    "Story asset chunk layout contains unavailable files:\n"
                    + string.Join("\n", unavailable));
            }
            var chunks = orderedDefinitions
                .Select(value => (value.assets ?? Array.Empty<string>())
                    .Where(availableArt.Contains)
                    .ToArray())
                .ToArray();
            var media = orderedDefinitions
                .SelectMany((value, chunkIndex) =>
                    (value.assets ?? Array.Empty<string>())
                    .Where(path => IsMediaPath(path) && availableFiles.Contains(path))
                    .Select(path => new ContentStreamingMediaEntry
                    {
                        order = chunkIndex,
                        path = path,
                        deliveryGroup = ContentAddressing.ContentPackageConvention
                            .StoryMediaDeliveryGroup(storyId, chunkIndex),
                    }))
                .ToArray();
            if (chunks.Length == 0 || chunks.Any(value => value.Length == 0))
                throw new InvalidOperationException(
                    $"Story asset '{storyId}' contains a chunk without Unity assets.");
            return new ExperimentalStreamingBuildPlan(chunks, media);
        }

        private static string ReadStoryText(string storyId)
        {
            var directory = ContentAssets.InkDirectory(storyId);
            if (!Directory.Exists(directory))
                return string.Empty;
            return string.Join("\n", Directory
                    .EnumerateFiles(directory, "*.ink", SearchOption.AllDirectories)
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .Select(File.ReadAllText))
                .Normalize(NormalizationForm.FormC)
                .ToLowerInvariant();
        }

        private static int FirstAssetUse(string storyText, string assetPath)
        {
            if (IsBootstrapAsset(assetPath))
                return -1;
            var segments = assetPath
                .Replace('\\', '/')
                .Normalize(NormalizationForm.FormC)
                .ToLowerInvariant()
                .Split('/');
            var storyIndex = Array.FindIndex(
                segments,
                value => string.Equals(value, "story", StringComparison.Ordinal));
            var firstUse = int.MaxValue;
            for (var index = Math.Max(0, storyIndex); index < segments.Length; index++)
            {
                var token = index == segments.Length - 1
                    ? Path.GetFileNameWithoutExtension(segments[index])
                    : segments[index];
                if (token.Length < 2
                    || token.All(char.IsDigit)
                    || _technicalAssetTokens.Contains(token))
                {
                    continue;
                }
                var use = storyText.IndexOf(token, StringComparison.Ordinal);
                if (use >= 0 && use < firstUse)
                    firstUse = use;
            }
            return firstUse;
        }

        private static int FirstMediaUse(string storyText, string path)
        {
            var token = Path.GetFileNameWithoutExtension(path)
                .Normalize(NormalizationForm.FormC)
                .ToLowerInvariant();
            var result = storyText.IndexOf(token, StringComparison.Ordinal);
            return result >= 0 ? result : int.MaxValue;
        }

        private static int FindFirstSceneEnd(string storyText)
        {
            var locationCount = 0;
            var lineStart = 0;
            while (lineStart < storyText.Length)
            {
                var lineEnd = storyText.IndexOf('\n', lineStart);
                if (lineEnd < 0)
                    lineEnd = storyText.Length;
                var line = storyText.Substring(lineStart, lineEnd - lineStart)
                    .TrimStart();
                if (line.StartsWith("локация:", StringComparison.Ordinal)
                    || line.StartsWith("location:", StringComparison.Ordinal))
                {
                    locationCount++;
                    if (locationCount == 3)
                        return lineStart;
                }
                lineStart = lineEnd + 1;
            }
            return -1;
        }

        private static bool IsBootstrapAsset(string path)
        {
            var extension = Path.GetExtension(path);
            return !string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase)
                   && !string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase)
                   && !string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase)
                   && !string.Equals(extension, ".webp", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsMediaPath(string path) =>
            path.StartsWith("novelsvideos/", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("novelsaudio/", StringComparison.OrdinalIgnoreCase);

        private static long SourceSize(string assetPath)
        {
            var absolute = Path.GetFullPath(assetPath);
            return File.Exists(absolute) ? new FileInfo(absolute).Length : 0L;
        }

        private static long StreamingAssetSourceSize(string relativePath)
        {
            var absolute = ContentAssets.SourcePath(relativePath);
            return File.Exists(absolute) ? new FileInfo(absolute).Length : 0L;
        }

        private static long ReadChunkTarget()
        {
            var value = Environment.GetEnvironmentVariable("NOVELS_CHUNK_SOURCE_MIB");
            return long.TryParse(value, out var mebibytes) && mebibytes > 0
                ? mebibytes * 1024L * 1024L
                : _defaultChunkSourceBytes;
        }
    }
}
