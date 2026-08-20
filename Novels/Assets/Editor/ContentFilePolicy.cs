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
            "noveltexts",
            "novelsaudio",
            "novelsvideos",
        };

        private static readonly IReadOnlyDictionary<string, HashSet<string>> _extensions =
            new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["noveltexts"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ".ink",
                    ".json",
                },
                ["novelsaudio"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ".wav",
                    ".mp3",
                    ".ogg",
                },
                ["novelsvideos"] = new(StringComparer.OrdinalIgnoreCase)
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

        internal static bool IsSupportedAudioFile(string file) =>
            _extensions["novelsaudio"].Contains(Path.GetExtension(file));

        private static bool IsDeliverable(string file, string rootName)
        {
            var relative = GetRelativePath(file);
            if (relative.Split('/').Any(segment => segment.StartsWith(".", StringComparison.Ordinal)))
                return false;
            return _extensions[rootName].Contains(Path.GetExtension(file));
        }
    }
}
