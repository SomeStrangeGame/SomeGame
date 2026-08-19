using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    internal static class StoryReferenceValidator
    {
        private sealed class AssetPathIndex
        {
            private readonly string[] _allPaths;
            private readonly ILookup<string, string> _paths;

            internal AssetPathIndex(IEnumerable<string> paths)
            {
                _allPaths = (paths ?? Array.Empty<string>()).ToArray();
                _paths = _allPaths.ToLookup(Normalize, StringComparer.OrdinalIgnoreCase);
            }

            internal IEnumerable<string> AllPaths => _allPaths;

            internal IEnumerable<string> Matching(string expected) =>
                _paths[Normalize(expected)];

            internal string Exact(string expected) =>
                Matching(expected).FirstOrDefault(actual => string.Equals(
                    Normalize(actual),
                    Normalize(expected),
                    StringComparison.Ordinal));

            internal static bool EqualsExact(string left, string right) =>
                string.Equals(
                    Normalize(left),
                    Normalize(right),
                    StringComparison.Ordinal);

            private static string Normalize(string path) =>
                (path ?? string.Empty).Normalize(NormalizationForm.FormC);
        }

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

            var reported = new HashSet<string>(StringComparer.Ordinal);
            var assetPaths = new AssetPathIndex(AssetDatabase.GetAllAssetPaths());
            foreach (var background in DistinctById(index.BackgroundReferences))
            {
                if (Novels.StoryContracts.StoryBackgroundAssets
                    .IsSolidBlack(background.Id))
                {
                    continue;
                }
                var assetPath = LocationPath(prefix, episode.Id, background.Id);
                if (!string.IsNullOrEmpty(assetPath)
                    && AssetDatabase.LoadAssetAtPath<Sprite>(assetPath) == null
                    && reported.Add(assetPath))
                {
                    // TODO: Restore Error severity when all authored story backgrounds are delivered.
                    errors.Add(ContentValidationIssue.Warning(
                        ContentValidationCodes.StoryBackgroundMissing,
                        $"Story background does not exist: {assetPath}. "
                        + $"Referenced at {background.Location}.",
                        assetPath,
                        episode.ContentId,
                        episode.Id));
                }
            }
            ValidateSpeakerCaseAmbiguity(episode, index.SpeakerReferences, errors);
            foreach (var speaker in DistinctByExactId(index.SpeakerReferences))
            {
                var role = Novels.StoryContracts.StorySpeakerRoleResolver.Resolve(
                    speaker.Id,
                    Novels.StoryContracts.DialoguePresentation.Character,
                    mainCharacter);
                if (!Novels.StoryContracts.StorySpeakerRoleResolver
                        .RequiresCharacterAsset(role))
                {
                    continue;
                }
                var assetPath = CharacterBodyPath(
                    prefix,
                    episode.Id,
                    speaker.Id,
                    characterAssets.ViewRoot);
                if (string.IsNullOrEmpty(assetPath))
                    continue;

                var possiblePaths = WithSharedCharacterPath(prefix, assetPath).ToArray();
                var matchingPaths = possiblePaths
                    .SelectMany(assetPaths.Matching)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                var exactPath = possiblePaths
                    .Select(assetPaths.Exact)
                    .FirstOrDefault(path => path != null);
                if (exactPath == null
                    && matchingPaths.Length == 1
                    && reported.Add(assetPath))
                {
                    errors.Add(ContentValidationIssue.Error(
                        ContentValidationCodes.StoryCharacterCaseMismatch,
                        $"Ink character '{speaker.Id}' does not match the asset path casing. "
                        + $"Expected exactly: {matchingPaths[0]}. "
                        + $"Referenced at {speaker.Location}.",
                        assetPath,
                        episode.ContentId,
                        episode.Id));
                    continue;
                }
                if (exactPath == null
                    && matchingPaths.Length > 1
                    && reported.Add(assetPath))
                {
                    errors.Add(ContentValidationIssue.Error(
                        ContentValidationCodes.StoryCharacterCaseAmbiguous,
                        $"Character asset path '{assetPath}' is ambiguous because these paths "
                        + "differ only by casing: "
                        + string.Join(", ", matchingPaths.Select(path => $"'{path}'"))
                        + $". Referenced at {speaker.Location}.",
                        assetPath,
                        episode.ContentId,
                        episode.Id));
                    continue;
                }
                var sprite = exactPath == null
                    ? null
                    : AssetDatabase.LoadAssetAtPath<Sprite>(exactPath);
                if (sprite == null && reported.Add(assetPath))
                {
                    var importer = exactPath == null
                        ? null
                        : AssetImporter.GetAtPath(exactPath) as TextureImporter;
                    if (importer != null
                        && (importer.textureType != TextureImporterType.Sprite
                            || importer.spriteImportMode == SpriteImportMode.None))
                    {
                        errors.Add(ContentValidationIssue.Error(
                            ContentValidationCodes.StoryCharacterTextureImportInvalid,
                            $"Story character image is not imported as a single Sprite: "
                            + $"{exactPath}. Referenced at {speaker.Location}.",
                            exactPath,
                            episode.ContentId,
                            episode.Id));
                        continue;
                    }
                    errors.Add(ContentValidationIssue.Error(
                        ContentValidationCodes.StoryCharacterMissing,
                        $"Story character body does not exist: {assetPath}. "
                        + $"Referenced at {speaker.Location}.",
                        assetPath,
                        episode.ContentId,
                        episode.Id));
                }
            }
            ValidateCharacterAssets(
                prefix,
                characterAssets,
                episode,
                index.CharacterAssetReferences,
                assetPaths,
                errors);
        }

        private static void ValidateCharacterAssets(
            string prefix,
            Novels.Content.CharacterAssetProfile profile,
            Novels.Content.EpisodeDefinition episode,
            IEnumerable<StoryCharacterAssetReference> references,
            AssetPathIndex assetPaths,
            ContentValidationReport errors)
        {
            foreach (var reference in references
                         .GroupBy(
                             value => $"{value.Role}\0{value.Speaker}\0"
                                 + $"{value.IsChild}\0{value.Candidate}",
                             StringComparer.Ordinal)
                         .Select(group => group.First()))
            {
                var characterName = reference.Role
                    == Novels.StoryContracts.StorySpeakerRole.MainCharacter
                        ? profile.MainCharacterAssetId
                        : reference.Speaker;
                var sharedPaths = CharacterSharedCandidatePaths(
                    prefix,
                    episode.Id,
                    characterName,
                    reference.Candidate,
                    profile)
                    .SelectMany(path => WithSharedCharacterPath(prefix, path))
                    .ToArray();
                if (sharedPaths.Any(IsResolvableSprite))
                    continue;

                var views = reference.Role
                    == Novels.StoryContracts.StorySpeakerRole.MainCharacter
                        ? MainCharacterViews(
                            prefix,
                            episode.Id,
                            characterName,
                            profile,
                            reference.IsChild,
                            assetPaths)
                        : new[]
                        {
                            reference.IsChild
                                ? $"{profile.ViewRoot}/{profile.ChildView}"
                                : profile.ViewRoot,
                        };
                if (views.Length == 0)
                    views = new[] { profile.ViewRoot };
                var missingViews = new List<string>();
                var caseMatches = new HashSet<string>(StringComparer.Ordinal);
                foreach (var view in views)
                {
                    var viewPaths = CharacterViewCandidatePaths(
                        prefix,
                        episode.Id,
                        characterName,
                        view,
                        reference.Candidate)
                        .SelectMany(path => WithSharedCharacterPath(prefix, path))
                        .ToArray();
                    if (viewPaths.Any(IsResolvableSprite))
                        continue;
                    missingViews.Add(view);
                    AddCaseMatches(sharedPaths.Concat(viewPaths), assetPaths, caseMatches);
                }
                if (missingViews.Count == 0)
                    continue;

                AddCaseMatches(sharedPaths, assetPaths, caseMatches);
                var code = caseMatches.Count > 0
                    ? ContentValidationCodes.StoryCharacterAssetCaseMismatch
                    : ContentValidationCodes.StoryCharacterAssetMissing;
                var detail = caseMatches.Count > 0
                    ? "Matching assets with different casing: "
                        + string.Join(", ", caseMatches.Select(path => $"'{path}'")) + "."
                    : "No matching body, emotion, clothes, hair, or accessory sprite exists.";
                errors.Add(ContentValidationIssue.Error(
                    code,
                    $"Ink character asset '{reference.Candidate}' for "
                    + $"'{reference.Speaker}' cannot be resolved for: "
                    + string.Join(", ", missingViews)
                    + $". {detail} Referenced at {reference.Location}.",
                    reference.SourcePath,
                    episode.ContentId,
                    episode.Id));
            }

            bool IsResolvableSprite(string path)
            {
                return assetPaths.Matching(path)
                    .Any(actual => AssetDatabase.LoadAssetAtPath<Sprite>(actual) != null);
            }
        }

        private static IEnumerable<string> CharacterViewCandidatePaths(
            string prefix,
            string episodeId,
            string character,
            string view,
            string candidate)
        {
            yield return Novels.ContentAddressing.ContentAddressConvention.CharacterMainBody(
                prefix,
                episodeId,
                character,
                view,
                candidate);
            yield return Novels.ContentAddressing.ContentAddressConvention.CharacterEmotion(
                prefix,
                episodeId,
                character,
                view,
                candidate);
        }

        private static IEnumerable<string> CharacterSharedCandidatePaths(
            string prefix,
            string episodeId,
            string character,
            string candidate,
            Novels.Content.CharacterAssetProfile profile)
        {
            yield return Novels.ContentAddressing.ContentAddressConvention.CharacterClothes(
                prefix,
                episodeId,
                character,
                candidate,
                1);
            yield return Novels.ContentAddressing.ContentAddressConvention.CharacterHair(
                prefix,
                episodeId,
                character,
                candidate,
                profile.BackLayer,
                profile.DefaultHairColor);
            yield return Novels.ContentAddressing.ContentAddressConvention.CharacterHair(
                prefix,
                episodeId,
                character,
                candidate,
                profile.FrontLayer,
                profile.DefaultHairColor);
            yield return Novels.ContentAddressing.ContentAddressConvention.CharacterAccessory(
                prefix,
                episodeId,
                character,
                candidate,
                profile.BackLayer);
            yield return Novels.ContentAddressing.ContentAddressConvention.CharacterAccessory(
                prefix,
                episodeId,
                character,
                candidate,
                profile.MiddleLayer);
            yield return Novels.ContentAddressing.ContentAddressConvention.CharacterAccessory(
                prefix,
                episodeId,
                character,
                candidate,
                profile.FrontLayer);
        }

        private static string[] MainCharacterViews(
            string prefix,
            string episodeId,
            string character,
            Novels.Content.CharacterAssetProfile profile,
            bool isChild,
            AssetPathIndex assetPaths)
        {
            var episodeRoot = $"{Novels.ContentAddressing.ContentPackageConvention.EpisodeRoot(prefix, episodeId)}"
                + $"/Character/Characters/{character}/{profile.ViewRoot}/";
            var sharedRoot = $"{Novels.ContentAddressing.ContentPackageConvention.ContentRoot(prefix)}"
                + $"/Shared/Character/Characters/{character}/{profile.ViewRoot}/";
            return assetPaths.AllPaths
                .Select(path => path.StartsWith(episodeRoot, StringComparison.Ordinal)
                    ? path.Substring(episodeRoot.Length)
                    : path.StartsWith(sharedRoot, StringComparison.Ordinal)
                        ? path.Substring(sharedRoot.Length)
                        : null)
                .Where(path => path != null)
                .Select(path => path.Split('/'))
                .Where(parts => isChild
                    ? parts.Length == 3
                        && parts[1] == profile.ChildView
                        && parts[2] == "Main.png"
                    : parts.Length == 2 && parts[1] == "Main.png")
                .Select(parts => isChild
                    ? $"{profile.ViewRoot}/{parts[0]}/{profile.ChildView}"
                    : $"{profile.ViewRoot}/{parts[0]}")
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
        }

        private static IEnumerable<string> WithSharedCharacterPath(
            string prefix,
            string episodePath)
        {
            yield return episodePath;
            var sharedPath = Novels.ContentAddressing.ContentAddressConvention
                .SharedCharacterAsset(prefix, episodePath);
            if (!string.IsNullOrEmpty(sharedPath))
                yield return sharedPath;
        }

        private static void AddCaseMatches(
            IEnumerable<string> expectedPaths,
            AssetPathIndex assetPaths,
            ISet<string> target)
        {
            foreach (var expected in expectedPaths)
            foreach (var actual in assetPaths.Matching(expected))
            {
                if (!AssetPathIndex.EqualsExact(actual, expected)
                    && AssetDatabase.LoadAssetAtPath<Sprite>(actual) != null)
                {
                    target.Add(actual);
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

            errors.Add(ContentValidationIssue.Warning(
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

        private static IEnumerable<StoryDependencyReference> DistinctByExactId(
            IEnumerable<StoryDependencyReference> references) =>
            references
                .GroupBy(reference => reference.Id, StringComparer.Ordinal)
                .Select(group => group.First());

        private static void ValidateSpeakerCaseAmbiguity(
            Novels.Content.EpisodeDefinition episode,
            IEnumerable<StoryDependencyReference> references,
            ContentValidationReport errors)
        {
            foreach (var group in references.GroupBy(
                         reference => reference.Id,
                         StringComparer.OrdinalIgnoreCase))
            {
                var variants = group
                    .Select(reference => reference.Id)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                if (variants.Length < 2)
                    continue;

                errors.Add(ContentValidationIssue.Error(
                    ContentValidationCodes.StoryCharacterCaseAmbiguous,
                    "Ink refers to one character with different casing: "
                    + string.Join(", ", variants.Select(value => $"'{value}'"))
                    + ". Character names and asset paths must match exactly. "
                    + "References: "
                    + string.Join(", ", group.Select(reference => reference.Location)),
                    group.First().SourcePath,
                    episode.ContentId,
                    episode.Id));
            }
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
