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
            foreach (var camera in index.CameraReferences)
            {
                if (!Novels.Location.CameraActionCapabilities.IsSupported(camera.Action))
                {
                    errors.Add(ContentValidationIssue.Error(
                        ContentValidationCodes.StoryCameraUnsupported,
                        $"Story camera action is not implemented at {camera.Location}: "
                        + camera.Action,
                        camera.SourcePath,
                        contentId: episode.ContentId,
                        episodeId: episode.Id));
                }
            }
            foreach (var audio in DistinctById(index.AudioReferences))
                ValidateAudio(prefix, episode, audio, errors);

            var reported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var background in DistinctById(index.BackgroundReferences))
            {
                var assetPath = LocationPath(prefix, episode.Id, background.Id);
                if (!string.IsNullOrEmpty(assetPath)
                    && AssetDatabase.LoadAssetAtPath<Sprite>(assetPath) == null
                    && reported.Add(assetPath))
                {
                    errors.Add(ContentValidationIssue.Error(
                        ContentValidationCodes.StoryBackgroundMissing,
                        $"Story background does not exist: {assetPath}. "
                        + $"Referenced at {background.Location}.",
                        assetPath,
                        episode.ContentId,
                        episode.Id));
                }
            }
            foreach (var speaker in DistinctById(index.SpeakerReferences))
            {
                if (string.Equals(speaker.Id, "Wardrobe", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(speaker.Id, mainCharacter, StringComparison.OrdinalIgnoreCase))
                    continue;
                var assetPath = CharacterBodyPath(
                    prefix,
                    episode.Id,
                    speaker.Id,
                    characterAssets.ViewRoot);
                if (!string.IsNullOrEmpty(assetPath)
                    && AssetDatabase.LoadAssetAtPath<Sprite>(assetPath) == null
                    && reported.Add(assetPath))
                {
                    errors.Add(ContentValidationIssue.Error(
                        ContentValidationCodes.StoryCharacterMissing,
                        $"Story character body does not exist: {assetPath}. "
                        + $"Referenced at {speaker.Location}.",
                        assetPath,
                        episode.ContentId,
                        episode.Id));
                }
            }
        }

        private static void ValidateAudio(
            string prefix,
            Novels.Content.EpisodeDefinition episode,
            StoryDependencyReference reference,
            ContentValidationReport errors)
        {
            var assetName = reference.Id;
            if (!StoryAudioFileResolver.IsBareFileName(assetName))
            {
                errors.Add(ContentValidationIssue.Error(
                    ContentValidationCodes.StoryAudioNameInvalid,
                    $"Ink audio reference '{assetName}' must contain only a file name "
                    + $"without an extension. Referenced at {reference.Location}.",
                    reference.SourcePath,
                    episode.ContentId,
                    episodeId: episode.Id));
                return;
            }
            if (episode.Media.SilentAudioIds.Contains(assetName, StringComparer.OrdinalIgnoreCase))
                return;
            var directory = Path.Combine(
                Application.streamingAssetsPath,
                "NovelsAudio",
                prefix);
            var available = StoryAudioFileResolver.FindCandidates(prefix, assetName);
            if (available.Length == 1)
                return;
            if (available.Length > 1)
            {
                errors.Add(ContentValidationIssue.Error(
                    ContentValidationCodes.StoryAudioFormatAmbiguous,
                    $"Ink audio reference '{assetName}' matches multiple formats: "
                    + string.Join(", ", available.Select(Path.GetFileName))
                    + $". Referenced at {reference.Location}.",
                    reference.SourcePath,
                    episode.ContentId,
                    episode.Id));
                return;
            }

            errors.Add(ContentValidationIssue.Error(
                ContentValidationCodes.StoryAudioMissing,
                $"Story audio '{assetName}' does not exist in: {directory}. "
                + $"Referenced at {reference.Location}.",
                reference.SourcePath,
                episode.ContentId,
                episode.Id));
        }

        private static IEnumerable<StoryDependencyReference> DistinctById(
            IEnumerable<StoryDependencyReference> references) =>
            references
                .GroupBy(reference => reference.Id, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First());

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
