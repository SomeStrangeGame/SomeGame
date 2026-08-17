using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor
{
    internal static class NovelContentValidator
    {
        private const string _menuPath = "Novels/Validate Content";
        [MenuItem(_menuPath)]
        private static void ValidateFromMenu()
        {
            var errors = ValidateLoadedConfiguration(true);
            if (errors.Count == 0)
            {
                Debug.Log("Novel content validation completed without errors.");
                return;
            }

            foreach (var error in errors)
                Debug.LogError($"[NovelContent] {error}");
        }

        public static void ValidateBatch()
        {
            EditorSceneManager.OpenScene("Assets/Novels/Novels.unity", OpenSceneMode.Single);
            ValidateOrThrow(true);
            Debug.Log("Novel content batch validation completed without errors.");
        }

        internal static void ValidateOrThrow()
        {
            ValidateOrThrow(false);
        }

        internal static void ValidateBuiltOutputOrThrow()
        {
            ValidateOrThrow(true);
        }

        private static void ValidateOrThrow(bool validateBuiltOutput)
        {
            var errors = ValidateLoadedConfiguration(validateBuiltOutput);
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Novel content validation failed:\n- "
                    + string.Join("\n- ", errors));
            }
        }

        private static IReadOnlyList<string> ValidateLoadedConfiguration(
            bool validateBuiltOutput)
        {
            var errors = new List<string>();
            var entryPoint = UnityEngine.Object.FindFirstObjectByType<Novels.EntryPoint>(
                FindObjectsInactive.Include);
            if (entryPoint == null)
            {
                errors.Add("The loaded scene does not contain Novels.EntryPoint.");
                return errors;
            }

            var catalog = AssetDatabase.LoadAssetAtPath<Novels.Catalog.NovelCatalogAsset>(
                Novels.Catalog.CatalogAddresses.AssetName);
            if (catalog == null)
            {
                errors.Add(
                    $"Novel catalog does not exist: "
                    + Novels.Catalog.CatalogAddresses.AssetName);
                return errors;
            }

            ValidateBundleAssignment(
                Novels.Catalog.CatalogAddresses.AssetName,
                Novels.Catalog.CatalogAddresses.BundleName,
                errors);
            var catalogScreen = AssetDatabase.LoadAssetAtPath<GameObject>(
                Novels.Catalog.CatalogAddresses.ScreenAssetName);
            if (catalogScreen == null)
            {
                errors.Add(
                    $"Catalog screen prefab does not exist: "
                    + Novels.Catalog.CatalogAddresses.ScreenAssetName);
            }
            else
            {
                PrefabContentValidator.ValidateCatalog(catalogScreen, errors);
                ValidateBundleAssignment(
                    Novels.Catalog.CatalogAddresses.ScreenAssetName,
                    Novels.Catalog.CatalogAddresses.BundleName,
                    errors);
            }

            if (string.IsNullOrWhiteSpace(catalog.Resolve("en").Title))
                errors.Add("Novel catalog title is empty.");
            if (catalog.Entries.Count == 0)
                errors.Add("Novel catalog has no entries.");

            var contentIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in catalog.Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ContentId))
                {
                    errors.Add("Novel catalog contains an entry without content ID.");
                    continue;
                }
                if (!contentIds.Add(entry.ContentId))
                    errors.Add($"Duplicate catalog content ID: {entry.ContentId}");
                if (string.IsNullOrWhiteSpace(entry.Resolve("en").Title))
                    errors.Add($"Catalog entry '{entry.ContentId}' has no title.");
                if (string.IsNullOrWhiteSpace(entry.ContentBundleName))
                    errors.Add($"Catalog entry '{entry.ContentId}' has no content bundle.");
                if (string.IsNullOrWhiteSpace(entry.ContentAssetName))
                    errors.Add($"Catalog entry '{entry.ContentId}' has no content asset address.");

                var contentAsset = AssetDatabase.LoadAssetAtPath<
                    Novels.Content.NovelContentAsset>(entry.ContentAssetName);
                if (contentAsset == null)
                {
                    errors.Add(
                        $"NovelContentAsset does not exist: {entry.ContentAssetName}");
                    continue;
                }

                ValidateBundleAssignment(
                    entry.ContentAssetName,
                    entry.ContentBundleName,
                    errors);
                ValidateContentAsset(
                    contentAsset,
                    entry.ContentId,
                    entry.ContentBundleName,
                    errors);
            }
            PrefabContentValidator.ValidateBootstrap(errors);
            if (validateBuiltOutput)
                BuiltReleaseValidator.Validate(errors);
            return errors;
        }

        private static void ValidateBundleAssignment(
            string assetPath,
            string expectedBundle,
            ICollection<string> errors)
        {
            var assignedBundle = AssetDatabase.GetImplicitAssetBundleName(assetPath);
            if (!string.Equals(
                    assignedBundle,
                    expectedBundle,
                    StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(
                    $"Asset '{assetPath}' must belong to "
                    + $"AssetBundle '{expectedBundle}'.");
            }
        }

        private static void ValidateContentAsset(
            Novels.Content.NovelContentAsset contentAsset,
            string expectedContentId,
            string contentBundleName,
            ICollection<string> errors)
        {
            Novels.Content.NovelDefinition definition;
            try
            {
                definition = contentAsset.ToDefinition("en");
            }
            catch (Exception exception)
            {
                errors.Add($"Content asset '{contentAsset.name}' is invalid: {exception.Message}");
                return;
            }

            if (contentAsset.AudioMixer == null)
                errors.Add($"Content asset '{contentAsset.name}' has no AudioMixer.");
            if (!string.Equals(
                    definition.Id,
                    expectedContentId,
                    StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(
                    $"Catalog content ID '{expectedContentId}' does not match "
                    + $"NovelContentAsset ID '{definition.Id}'.");
            }

            ValidateBundles(
                new[]
                {
                    Novels.Catalog.CatalogAddresses.BundleName,
                    contentBundleName,
                    definition.MainLoadingBundleName,
                    definition.LoadingBundleName,
                    definition.SettingBundleName,
                    definition.LocalizationBundleName,
                }.Concat(definition.Episodes.SelectMany(episode => new[]
                {
                    episode.BubbleBundleName,
                    episode.LocationBundleName,
                    episode.CharacterBundleName,
                    episode.NotificationBundleName,
                })),
                errors);

            foreach (var episode in definition.Episodes)
            {
                ValidateMedia(definition.Prefix, episode, errors);
                StoryReferenceValidator.Validate(
                    definition.Prefix,
                    definition.MainCharacter,
                    episode,
                    errors);
            }
        }

        private static void ValidateMedia(
            string prefix,
            Novels.Content.EpisodeDefinition episode,
            ICollection<string> errors)
        {
            foreach (var videoId in episode.Media.VideoIds)
            {
                var path = Path.Combine(
                    Application.streamingAssetsPath,
                    "NovelsVideos",
                    prefix,
                    videoId + ".mp4");
                if (!File.Exists(path))
                    errors.Add($"Configured video does not exist: {path}");
            }

            foreach (var audio in episode.Media.AudioExtensions)
            {
                var path = Path.Combine(
                    Application.streamingAssetsPath,
                    "NovelsAudio",
                    prefix,
                    audio.Key + audio.Value);
                if (!File.Exists(path))
                    errors.Add($"Configured audio override does not exist: {path}");
            }
        }

        private static void ValidateBundles(
            IEnumerable<string> configuredBundles,
            ICollection<string> errors)
        {
            var existingBundles = new HashSet<string>(
                AssetDatabase.GetAllAssetBundleNames(),
                StringComparer.OrdinalIgnoreCase);

            foreach (var bundle in configuredBundles.Where(
                         value => !string.IsNullOrWhiteSpace(value)))
            {
                if (!existingBundles.Contains(bundle))
                    errors.Add($"AssetBundle '{bundle}' is not assigned to any asset.");

            }
        }
    }
}
