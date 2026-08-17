using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Editor
{
    internal static class ContentFilePolicy
    {
        internal static readonly string[] RootDirectories =
        {
            "NovelTexts",
            "NovelsAudio",
            "NovelsVideos",
        };

        private static readonly IReadOnlyDictionary<string, HashSet<string>> _extensions =
            new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["NovelTexts"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ".ink",
                    ".json",
                },
                ["NovelsAudio"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ".wav",
                    ".mp3",
                    ".ogg",
                },
                ["NovelsVideos"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ".mp4",
                },
            };

        internal static IEnumerable<string> EnumerateFiles()
        {
            foreach (var rootName in RootDirectories)
            {
                var root = Path.Combine(Application.streamingAssetsPath, rootName);
                if (!Directory.Exists(root))
                    continue;
                foreach (var file in Directory.GetFiles(
                             root,
                             "*",
                             SearchOption.AllDirectories))
                {
                    if (IsDeliverable(file, rootName))
                        yield return file;
                }
            }
        }

        internal static string GetRelativePath(string file)
        {
            return file.Substring(Application.streamingAssetsPath.Length + 1)
                .Replace(Path.DirectorySeparatorChar, '/');
        }

        internal static string GetDeliveryGroup(string relativePath)
        {
            var segments = relativePath.Split('/');
            return segments.Length > 1 && !string.IsNullOrWhiteSpace(segments[1])
                ? segments[1]
                : "shared";
        }

        private static bool IsDeliverable(string file, string rootName)
        {
            var relative = GetRelativePath(file);
            if (relative.Split('/').Any(segment => segment.StartsWith(".", StringComparison.Ordinal)))
                return false;
            return _extensions[rootName].Contains(Path.GetExtension(file));
        }
    }
}
