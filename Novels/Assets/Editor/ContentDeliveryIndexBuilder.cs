using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Editor
{
    internal static class ContentDeliveryIndexBuilder
    {
        internal static IReadOnlyDictionary<string, string> Build()
        {
            var project = ContentProjectIndex.BuildOrThrow();
            return Build(project);
        }

        internal static IReadOnlyDictionary<string, string> Build(
            ContentProjectIndex project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            var owners = new Dictionary<string, HashSet<string>>(
                StringComparer.OrdinalIgnoreCase);
            foreach (var item in project.Entries)
            {
                var definition = item.Definition;
                foreach (var episode in definition.Episodes)
                {
                    var group = EpisodeGroup(definition.Id, episode.Id);
                    foreach (var path in EpisodeFiles(
                                 definition.Prefix,
                                 episode,
                                 item.StoryDependencies[episode.Id]))
                    {
                        if (!owners.TryGetValue(path, out var groups))
                        {
                            groups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            owners[path] = groups;
                        }
                        groups.Add(group);
                    }
                }
            }

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var unassigned = new List<string>();
            foreach (var file in ContentFilePolicy.EnumerateFiles())
            {
                var relative = ContentFilePolicy.GetRelativePath(file);
                if (!owners.TryGetValue(relative, out var groups) || groups.Count == 0)
                {
                    unassigned.Add(relative);
                    continue;
                }

                if (groups.Count == 1)
                {
                    result[relative] = groups.Single();
                    continue;
                }

                var contentIds = groups
                    .Select(group => group.Split('/')[0])
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                if (contentIds.Length != 1)
                {
                    throw new InvalidOperationException(
                        $"Content file '{relative}' is referenced by unrelated stories: "
                        + string.Join(", ", groups));
                }
                result[relative] = SharedGroup(contentIds[0]);
            }

            if (unassigned.Count > 0)
            {
                UnityEngine.Debug.LogWarning(
                    $"Exclude {unassigned.Count} unassigned content file(s) from release:\n"
                    + string.Join("\n", unassigned.OrderBy(value => value, StringComparer.Ordinal)));
            }
            return result;
        }

        internal static string EpisodeGroup(string contentId, string episodeId) =>
            Novels.ContentAddressing.ContentPackageConvention.EpisodeDeliveryGroup(
                contentId,
                episodeId);

        internal static string SharedGroup(string contentId) =>
            Novels.ContentAddressing.ContentPackageConvention.SharedDeliveryGroup(contentId);

        private static IEnumerable<string> EpisodeFiles(
            string prefix,
            Novels.Content.EpisodeDefinition episode,
            StoryDependencyManifest references)
        {
            var normalizedStoryPath = episode.StoryPath.Replace('\\', '/');
            var canonicalPrefix = Novels.ContentAddressing.TechnicalAssetIdConvention
                .Canonicalize(prefix);
            yield return $"NovelTexts/{canonicalPrefix}/{normalizedStoryPath}";
            yield return $"NovelTexts/{canonicalPrefix}/{normalizedStoryPath}"
                + StorySourceMapBuilder.FileSuffix;

            foreach (var audioId in references.AudioReferences
                         .Select(reference => reference.Id)
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (episode.Media.SilentAudioIds.Contains(
                        audioId,
                        StringComparer.OrdinalIgnoreCase))
                    continue;
                if (StoryAudioFileResolver.FindCandidates(prefix, audioId).Length == 0)
                    continue;
                yield return StoryAudioFileResolver.ResolveRelativePath(prefix, audioId);
            }
            foreach (var backgroundId in references.BackgroundReferences
                         .Select(reference => reference.Id)
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (Novels.StoryContracts.StoryBackgroundAssets.IsSolidBlack(backgroundId))
                    continue;
                var canonicalBackground = Novels.ContentAddressing.TechnicalAssetIdConvention
                    .Canonicalize(backgroundId);
                yield return $"NovelsVideos/{canonicalPrefix}/{canonicalBackground}"
                    + Bundles.MediaFileConvention.VideoExtension;
            }
        }
    }

    internal static class StoryAudioFileResolver
    {
        internal static bool IsBareFileName(string audioId)
        {
            var value = audioId?.Trim();
            return !string.IsNullOrEmpty(value)
                && string.Equals(Path.GetFileName(value), value, StringComparison.Ordinal)
                && Path.GetExtension(value).Length == 0;
        }

        internal static string[] FindCandidates(string prefix, string audioId)
        {
            if (!IsBareFileName(audioId))
                return Array.Empty<string>();
            var directory = Path.Combine(
                UnityEngine.Application.streamingAssetsPath,
                "NovelsAudio",
                prefix);
            if (!Directory.Exists(directory))
                return Array.Empty<string>();
            return Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly)
                .Where(ContentFilePolicy.IsSupportedAudioFile)
                .Where(file => string.Equals(
                    Path.GetFileNameWithoutExtension(file),
                    audioId.Trim(),
                    StringComparison.OrdinalIgnoreCase))
                .OrderBy(file => file, StringComparer.Ordinal)
                .ToArray();
        }

        internal static string ResolveRelativePath(string prefix, string audioId)
        {
            if (!IsBareFileName(audioId))
            {
                throw new InvalidOperationException(
                    $"Ink audio reference '{audioId}' must contain only a file name "
                    + "without an extension.");
            }
            var candidates = FindCandidates(prefix, audioId);
            if (candidates.Length == 0)
            {
                throw new InvalidOperationException(
                    $"Ink audio reference '{audioId}' has no matching file.");
            }
            if (candidates.Length > 1)
            {
                throw new InvalidOperationException(
                    $"Ink audio reference '{audioId}' is ambiguous: "
                    + string.Join(", ", candidates.Select(Path.GetFileName)));
            }
            return ContentFilePolicy.GetRelativePath(candidates[0]);
        }
    }
}
