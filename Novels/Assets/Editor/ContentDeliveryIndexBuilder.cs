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
            var project = ContentProjectIndex.BuildOrThrow("en");
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
            yield return $"NovelTexts/{prefix}/{normalizedStoryPath}";

            foreach (var audioId in references.AudioIds)
            {
                if (episode.Media.SilentAudioIds.Contains(
                        audioId,
                        StringComparer.OrdinalIgnoreCase))
                    continue;
                var extension = Path.GetExtension(audioId);
                var fileName = extension.Length > 0
                    ? audioId
                    : audioId + Bundles.MediaFileConvention.DefaultAudioExtension;
                yield return $"NovelsAudio/{prefix}/{fileName}";
            }
            foreach (var backgroundId in references.Backgrounds)
            {
                yield return $"NovelsVideos/{prefix}/{backgroundId}"
                    + Bundles.MediaFileConvention.VideoExtension;
            }
        }
    }
}
