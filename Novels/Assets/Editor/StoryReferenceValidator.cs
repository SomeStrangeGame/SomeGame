using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    internal static class StoryReferenceValidator
    {
        internal static void Validate(
            string prefix,
            string mainCharacter,
            Novels.Content.EpisodeDefinition episode,
            ICollection<string> errors)
        {
            var index = StoryReferenceIndex.Build(prefix, episode);
            foreach (var error in index.Errors)
                errors.Add(error);
            foreach (var audioId in index.AudioIds)
                ValidateAudio(prefix, episode, audioId, errors);

            var reported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var background in index.Backgrounds)
            {
                var assetPath = LocationPath(prefix, episode.Id, background);
                if (!string.IsNullOrEmpty(assetPath)
                    && AssetDatabase.LoadAssetAtPath<Sprite>(assetPath) == null
                    && reported.Add(assetPath))
                    errors.Add($"Story background does not exist: {assetPath}");
            }
            foreach (var speaker in index.Speakers)
            {
                if (string.Equals(speaker, "Wardrobe", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(speaker, mainCharacter, StringComparison.OrdinalIgnoreCase))
                    continue;
                var assetPath = CharacterBodyPath(prefix, episode.Id, speaker);
                if (!string.IsNullOrEmpty(assetPath)
                    && AssetDatabase.LoadAssetAtPath<Sprite>(assetPath) == null
                    && reported.Add(assetPath))
                    errors.Add($"Story character body does not exist: {assetPath}");
            }
        }

        private static void ValidateAudio(
            string prefix,
            Novels.Content.EpisodeDefinition episode,
            string assetName,
            ICollection<string> errors)
        {
            if (episode.Media.SilentAudioIds.Contains(assetName, StringComparer.OrdinalIgnoreCase))
                return;
            var extension = Path.GetExtension(assetName);
            if (extension.Length == 0)
                extension = episode.Media.AudioExtensions.TryGetValue(assetName, out var configured)
                    ? configured
                    : episode.Media.DefaultAudioExtension;
            var path = Path.Combine(
                Application.streamingAssetsPath,
                "NovelsAudio",
                prefix,
                assetName + (Path.GetExtension(assetName).Length == 0 ? extension : string.Empty));
            if (!File.Exists(path))
                errors.Add($"Story audio does not exist: {path}");
        }

        private static string LocationPath(
            string prefix,
            string episodeId,
            string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName) || assetName.StartsWith("{", StringComparison.Ordinal))
                return string.Empty;
            return Novels.ContentAddressing.ContentAddressConvention.LocationImage(
                prefix,
                episodeId,
                assetName);
        }

        private static string CharacterBodyPath(
            string prefix,
            string episodeId,
            string speaker)
        {
            if (string.IsNullOrWhiteSpace(speaker) || speaker.StartsWith("{", StringComparison.Ordinal))
                return string.Empty;
            return Novels.ContentAddressing.ContentAddressConvention.CharacterMainBody(
                prefix,
                episodeId,
                speaker,
                "View");
        }
    }
}
