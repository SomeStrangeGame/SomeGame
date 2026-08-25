using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

    internal static class ExperimentalStreamingPlan
    {
        private const long _defaultChunkSourceBytes = 96L * 1024L * 1024L;
        private static readonly HashSet<string> _technicalAssetTokens = new(
            StringComparer.Ordinal)
        {
            "story",
            "presentation",
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
            var storyText = ReadStoryText(storyId);
            var targetBytes = ReadChunkTarget();
            var firstSceneEnd = FindFirstSceneEnd(storyText);
            var orderedAssets = assets
                .Select(path => new
                {
                    Path = path,
                    FirstUse = FirstAssetUse(storyText, path),
                    Size = SourceSize(path),
                    Bootstrap = IsBootstrapAsset(path),
                    RuntimeDefault = IsRuntimeDefaultAsset(path),
                })
                .Select(value => new
                {
                    value.Path,
                    value.FirstUse,
                    value.Size,
                    value.Bootstrap,
                    Startup = value.RuntimeDefault
                        || firstSceneEnd > 0
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
                    && !asset.Startup
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

        private static string ReadStoryText(string storyId)
        {
            var directory = Path.Combine(
                UnityEngine.Application.streamingAssetsPath,
                "noveltexts",
                storyId);
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
            if (IsBootstrapAsset(assetPath) || IsRuntimeDefaultAsset(assetPath))
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

        private static bool IsRuntimeDefaultAsset(string path)
        {
            var normalized = path
                .Replace('\\', '/')
                .Normalize(NormalizationForm.FormC)
                .ToLowerInvariant();
            return normalized.Contains(
                       "/characters/maincharacter/hairs/",
                       StringComparison.Ordinal)
                   && normalized.Contains("/распущенные/", StringComparison.Ordinal)
                   && normalized.EndsWith("/блонд.png", StringComparison.Ordinal);
        }

        private static bool IsMediaPath(string path) =>
            path.StartsWith("novelsvideos/", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("novelsaudio/", StringComparison.OrdinalIgnoreCase);

        private static long SourceSize(string assetPath)
        {
            var absolute = Path.GetFullPath(assetPath);
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
