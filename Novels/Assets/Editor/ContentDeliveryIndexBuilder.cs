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
            var prefixToContent = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var item in project.Entries)
            {
                var definition = item.Definition;
                prefixToContent[definition.Prefix] = definition.Id;
                foreach (var episode in definition.Episodes)
                {
                    var group = EpisodeGroup(definition.Id, episode.Id);
                    foreach (var path in EpisodeFiles(definition.Prefix, episode))
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
            foreach (var file in ContentFilePolicy.EnumerateFiles())
            {
                var relative = ContentFilePolicy.GetRelativePath(file);
                if (owners.TryGetValue(relative, out var groups) && groups.Count == 1)
                {
                    result[relative] = groups.Single();
                    continue;
                }
                var segments = relative.Split('/');
                var prefix = segments.Length > 1 ? segments[1] : string.Empty;
                result[relative] = prefixToContent.TryGetValue(prefix, out var contentId)
                    ? SharedGroup(contentId)
                    : "shared";
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
            Novels.Content.EpisodeDefinition episode)
        {
            var storyName = Path.GetFileNameWithoutExtension(episode.StoryPath);
            var storyDirectory = $"NovelTexts/{prefix}";
            yield return $"{storyDirectory}/{episode.StoryPath}";
            yield return $"{storyDirectory}/{storyName}.ink";
            yield return $"{storyDirectory}/{storyName}.json";
            yield return $"{storyDirectory}/{storyName}.ink.json";

            var references = StoryReferenceIndex.Build(prefix, episode);
            foreach (var audioId in references.AudioIds)
            {
                if (episode.Media.SilentAudioIds.Contains(
                        audioId,
                        StringComparer.OrdinalIgnoreCase))
                    continue;
                var extension = Path.GetExtension(audioId);
                var fileName = extension.Length > 0
                    ? audioId
                    : audioId + (episode.Media.AudioExtensions.TryGetValue(
                        audioId,
                        out var configured)
                            ? configured
                            : episode.Media.DefaultAudioExtension);
                yield return $"NovelsAudio/{prefix}/{fileName}";
            }
            foreach (var videoId in episode.Media.VideoIds)
                yield return $"NovelsVideos/{prefix}/{videoId}.mp4";
        }
    }
}
