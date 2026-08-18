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
            Novels.Content.CharacterAssetProfile characterAssets,
            Novels.Content.EpisodeDefinition episode,
            StoryDependencyManifest index,
            ContentValidationReport errors)
        {
            foreach (var issue in index.Issues)
                errors.Add(issue);
            foreach (var action in index.CameraActions)
            {
                if (!Novels.Location.CameraActionCapabilities.IsSupported(action))
                {
                    errors.Add(ContentValidationIssue.Error(
                        ContentValidationCodes.StoryCameraUnsupported,
                        $"Story camera action is not implemented: {action}",
                        contentId: episode.ContentId,
                        episodeId: episode.Id));
                }
            }
            foreach (var audioId in index.AudioIds)
                ValidateAudio(prefix, episode, audioId, errors);

            var reported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var background in index.Backgrounds)
            {
                var assetPath = LocationPath(prefix, episode.Id, background);
                if (!string.IsNullOrEmpty(assetPath)
                    && AssetDatabase.LoadAssetAtPath<Sprite>(assetPath) == null
                    && reported.Add(assetPath))
                {
                    errors.Add(ContentValidationIssue.Error(
                        ContentValidationCodes.StoryBackgroundMissing,
                        $"Story background does not exist: {assetPath}",
                        assetPath,
                        episode.ContentId,
                        episode.Id));
                }
            }
            foreach (var speaker in index.Speakers)
            {
                if (string.Equals(speaker, "Wardrobe", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(speaker, mainCharacter, StringComparison.OrdinalIgnoreCase))
                    continue;
                var assetPath = CharacterBodyPath(
                    prefix,
                    episode.Id,
                    speaker,
                    characterAssets.ViewRoot);
                if (!string.IsNullOrEmpty(assetPath)
                    && AssetDatabase.LoadAssetAtPath<Sprite>(assetPath) == null
                    && reported.Add(assetPath))
                {
                    errors.Add(ContentValidationIssue.Error(
                        ContentValidationCodes.StoryCharacterMissing,
                        $"Story character body does not exist: {assetPath}",
                        assetPath,
                        episode.ContentId,
                        episode.Id));
                }
            }
        }

        private static void ValidateAudio(
            string prefix,
            Novels.Content.EpisodeDefinition episode,
            string assetName,
            ContentValidationReport errors)
        {
            if (episode.Media.SilentAudioIds.Contains(assetName, StringComparer.OrdinalIgnoreCase))
                return;
            var extension = Path.GetExtension(assetName);
            if (extension.Length == 0)
                extension = Bundles.MediaFileConvention.DefaultAudioExtension;
            var directory = Path.Combine(
                Application.streamingAssetsPath,
                "NovelsAudio",
                prefix);
            var expectedName = assetName
                + (Path.GetExtension(assetName).Length == 0 ? extension : string.Empty);
            var path = Path.Combine(directory, expectedName);
            var available = Directory.Exists(directory)
                ? Directory.EnumerateFiles(directory)
                    .Where(file => string.Equals(
                        Path.GetFileNameWithoutExtension(file),
                        Path.GetFileNameWithoutExtension(assetName),
                        StringComparison.OrdinalIgnoreCase))
                    .ToArray()
                : Array.Empty<string>();
            if (available.Any(file => string.Equals(
                    Path.GetFileName(file),
                    expectedName,
                    StringComparison.OrdinalIgnoreCase)))
                return;

            if (available.Length == 1)
            {
                var actualName = Path.GetFileName(available[0]);
                errors.Add(ContentValidationIssue.Error(
                    ContentValidationCodes.StoryAudioExtensionMismatch,
                    $"Ink audio reference '{assetName}' resolves to '{expectedName}', "
                    + $"but the available file is '{actualName}'. Specify the extension "
                    + $"explicitly in Ink: '{actualName}'.",
                    available[0],
                    episode.ContentId,
                    episode.Id));
                return;
            }

            if (available.Length > 1)
            {
                errors.Add(ContentValidationIssue.Error(
                    ContentValidationCodes.StoryAudioExtensionAmbiguous,
                    $"Ink audio reference '{assetName}' is ambiguous. Specify one of these "
                    + $"file names explicitly in Ink: "
                    + string.Join(", ", available.Select(Path.GetFileName)),
                    directory,
                    episode.ContentId,
                    episode.Id));
                return;
            }

            errors.Add(ContentValidationIssue.Error(
                ContentValidationCodes.StoryAudioMissing,
                $"Story audio does not exist: {path}",
                path,
                episode.ContentId,
                episode.Id));
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
            string speaker,
            string viewRoot)
        {
            if (string.IsNullOrWhiteSpace(speaker) || speaker.StartsWith("{", StringComparison.Ordinal))
                return string.Empty;
            return Novels.ContentAddressing.ContentAddressConvention.CharacterMainBody(
                prefix,
                episodeId,
                speaker,
                viewRoot);
        }
    }
}
