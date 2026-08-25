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

        internal static ExperimentalStreamingBuildPlan Create(
            string storyId,
            IReadOnlyCollection<string> assets,
            IReadOnlyCollection<string> filePaths)
        {
            var storyText = ReadStoryText(storyId);
            var targetBytes = ReadChunkTarget();
            var orderedAssets = assets
                .Select(path => new
                {
                    Path = path,
                    FirstUse = FirstAssetUse(storyText, path),
                    Size = SourceSize(path),
                    Bootstrap = IsBootstrapAsset(path),
                })
                .OrderBy(value => value.Bootstrap ? 0 : 1)
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
            if (IsBootstrapAsset(assetPath))
                return -1;
            var normalized = assetPath.Replace('\\', '/').ToLowerInvariant();
            var token = Path.GetFileNameWithoutExtension(normalized);
            if (string.Equals(token, "main", StringComparison.Ordinal))
            {
                var marker = "/characters/";
                var start = normalized.IndexOf(marker, StringComparison.Ordinal);
                if (start >= 0)
                {
                    start += marker.Length;
                    var end = normalized.IndexOf('/', start);
                    if (end > start)
                    {
                        token = normalized.Substring(start, end - start);
                        if (string.Equals(
                                token,
                                "maincharacter",
                                StringComparison.Ordinal))
                        {
                            var viewMarker = "/view/";
                            var viewStart = normalized.IndexOf(
                                viewMarker,
                                end,
                                StringComparison.Ordinal);
                            if (viewStart >= 0)
                            {
                                viewStart += viewMarker.Length;
                                var viewEnd = normalized.IndexOf('/', viewStart);
                                if (viewEnd > viewStart)
                                {
                                    token = normalized.Substring(
                                        viewStart,
                                        viewEnd - viewStart);
                                }
                            }
                        }
                    }
                }
            }
            if (token.All(char.IsDigit))
                token = Path.GetFileName(Path.GetDirectoryName(normalized));
            token = token.Normalize(NormalizationForm.FormC);
            var result = storyText.IndexOf(token, StringComparison.Ordinal);
            return result >= 0 ? result : int.MaxValue;
        }

        private static int FirstMediaUse(string storyText, string path)
        {
            var token = Path.GetFileNameWithoutExtension(path)
                .Normalize(NormalizationForm.FormC)
                .ToLowerInvariant();
            var result = storyText.IndexOf(token, StringComparison.Ordinal);
            return result >= 0 ? result : int.MaxValue;
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

        private static long ReadChunkTarget()
        {
            var value = Environment.GetEnvironmentVariable("NOVELS_CHUNK_SOURCE_MIB");
            return long.TryParse(value, out var mebibytes) && mebibytes > 0
                ? mebibytes * 1024L * 1024L
                : _defaultChunkSourceBytes;
        }
    }
}
