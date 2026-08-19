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
            LogWarnings(errors);
            var actualErrors = errors.Issues
                .Where(issue => issue.Severity == ContentValidationSeverity.Error)
                .ToArray();
            if (actualErrors.Length == 0)
            {
                Debug.Log("Novel content validation completed without errors.");
                return;
            }

            foreach (var error in actualErrors)
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

        internal static void ValidateOrThrow(ContentProjectIndex project)
        {
            var errors = ValidateLoadedConfiguration(false, project: project);
            ThrowIfInvalid(errors);
        }

        internal static void ValidateBuiltOutputOrThrow()
        {
            ValidateOrThrow(true);
        }

        internal static void ValidateBuiltOutputOrThrow(string remoteBasePath)
        {
            var errors = ValidateLoadedConfiguration(true, remoteBasePath);
            ThrowIfInvalid(errors);
        }

        internal static void ValidateBuiltOutputOrThrow(
            string remoteBasePath,
            ContentBuildSnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));
            var errors = ValidateLoadedConfiguration(
                true,
                remoteBasePath,
                snapshot.Project,
                snapshot.DeliveryIndex);
            ThrowIfInvalid(errors);
        }

        private static void ValidateOrThrow(bool validateBuiltOutput)
        {
            var errors = ValidateLoadedConfiguration(validateBuiltOutput);
            ThrowIfInvalid(errors);
        }

        private static ContentValidationReport ValidateLoadedConfiguration(
            bool validateBuiltOutput,
            string remoteBasePath = null,
            ContentProjectIndex project = null,
            IReadOnlyDictionary<string, string> deliveryIndex = null)
        {
            var errors = new ContentValidationReport();
            var entryPoint = UnityEngine.Object.FindFirstObjectByType<Novels.EntryPoint>(
                FindObjectsInactive.Include);
            if (entryPoint == null)
            {
                errors.Add("The loaded scene does not contain Novels.EntryPoint.");
                return errors;
            }
            var entryPointData = new SerializedObject(entryPoint);
            if (entryPointData.FindProperty("_targetCamera")?.objectReferenceValue == null)
                errors.Add("Novels.EntryPoint has no target Camera reference.");

            ValidateApplicationLocalization(errors);

            if (project == null)
                ContentProjectIndex.TryBuild(errors, out project);
            if (project == null)
                return errors;
            var catalog = project.Catalog;

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

            foreach (var locale in Novels.Locale.LocalePolicy.SupportedLocales)
            {
                if (!catalog.TryResolveExact(locale, out var catalogText)
                    || string.IsNullOrWhiteSpace(catalogText.Title))
                    errors.Add($"Novel catalog title is empty for locale '{locale}'.");
            }
            if (catalog.Entries.Count == 0)
                errors.Add("Novel catalog has no entries.");

            foreach (var item in project.Entries)
            {
                var entry = item.CatalogEntry;
                foreach (var locale in Novels.Locale.LocalePolicy.SupportedLocales)
                {
                    if (!entry.TryResolveExact(locale, out var entryText)
                        || string.IsNullOrWhiteSpace(entryText.Title))
                    {
                        errors.Add(
                            $"Catalog entry '{entry.ContentId}' has no title "
                            + $"for locale '{locale}'.");
                    }
                }

                ValidateBundleAssignment(
                    entry.ContentAssetName,
                    entry.ContentBundleName,
                    errors);
                ValidateContentAsset(
                    item.Asset,
                    item.Definition,
                    item.StoryDependencies,
                    entry.ContentId,
                    errors);
            }
            PrefabContentValidator.ValidateBootstrap(errors);
            if (validateBuiltOutput)
                BuiltReleaseValidator.Validate(
                    catalog.Entries
                        .Where(entry => entry != null)
                        .Select(entry => entry.ContentId),
                    errors,
                    remoteBasePath,
                    deliveryIndex);
            return errors;
        }

        private static void ValidateApplicationLocalization(
            ICollection<string> errors)
        {
            var data = AssetDatabase.LoadAssetAtPath<Localization.LocalizationData>(
                Novels.ApplicationLocalizationContract.AssetPath);
            if (data == null)
            {
                errors.Add(
                    "Application localization does not exist: "
                    + Novels.ApplicationLocalizationContract.AssetPath);
                return;
            }
            foreach (var locale in Novels.Locale.LocalePolicy.SupportedLocales)
            {
                try
                {
                    var localization = new Localization.Entity(
                        new Localization.Entity.Ctx
                        {
                            Locale = locale,
                            LocalizationSO = data,
                            RequireExactLocale = true,
                        });
                    foreach (var key in Novels.ApplicationLocalizationContract.RequiredKeys)
                        localization.GetRequiredValue(key);
                }
                catch (Exception exception)
                {
                    errors.Add(
                        $"Application localization '{locale}' is invalid: "
                        + exception.Message);
                }
            }
        }

        private static void ThrowIfInvalid(ContentValidationReport errors)
        {
            LogWarnings(errors);
            var actualErrors = errors.Issues
                .Where(issue => issue.Severity == ContentValidationSeverity.Error)
                .ToArray();
            if (actualErrors.Length > 0)
            {
                throw new InvalidOperationException(
                    "Novel content validation failed:\n- "
                    + string.Join("\n- ", actualErrors.Select(issue => issue.ToString())));
            }
        }

        private static void LogWarnings(ContentValidationReport report)
        {
            foreach (var warning in report.Issues.Where(
                         issue => issue.Severity == ContentValidationSeverity.Warning))
            {
                Debug.LogWarning($"[NovelContent] {warning}");
            }
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
            Novels.Content.NovelDefinition definition,
            IReadOnlyDictionary<string, StoryDependencyManifest> storyDependencies,
            string expectedContentId,
            ContentValidationReport errors)
        {
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
                    definition.BundleName,
                    definition.MainLoadingBundleName,
                    definition.BundleName,
                }.Concat(definition.Episodes.Select(episode => episode.BundleName)),
                errors);

            ValidateBundleAssignment(
                Novels.ContentAddressing.ContentAddressConvention.SettingPrefab(
                    definition.Prefix,
                    Novels.ContentAddressing.ContentAssetNames.Screen),
                definition.BundleName,
                errors);
            ValidateBundleAssignment(
                Novels.ContentAddressing.ContentAddressConvention.LocalizationAsset(
                    definition.Prefix,
                    Novels.ContentAddressing.ContentAssetNames.LocalizationData),
                definition.BundleName,
                errors);

            foreach (var episode in definition.Episodes)
            {
                ValidateEpisodeBundleAssignments(definition.Prefix, episode, errors);
                StoryReferenceValidator.Validate(
                    definition.Prefix,
                    definition.MainCharacter,
                    definition.CharacterAssets,
                    episode,
                    storyDependencies[episode.Id],
                    errors);
            }
        }

        private static void ValidateEpisodeBundleAssignments(
            string prefix,
            Novels.Content.EpisodeDefinition episode,
            ICollection<string> errors)
        {
            PrefabContentValidator.ValidateEpisode(prefix, episode.Id, errors);
            foreach (var assetPath in new[]
                     {
                         Novels.ContentAddressing.ContentAddressConvention.LoadingPrefab(
                             prefix, episode.Id, Novels.ContentAddressing.ContentAssetNames.Screen),
                         Novels.ContentAddressing.ContentAddressConvention.BubblePrefab(
                             prefix, episode.Id, Novels.ContentAddressing.ContentAssetNames.Screen),
                         Novels.ContentAddressing.ContentAddressConvention.LocationPrefab(
                             prefix, episode.Id, Novels.ContentAddressing.ContentAssetNames.Screen),
                         Novels.ContentAddressing.ContentAddressConvention.CharacterPrefab(
                             prefix, episode.Id, Novels.ContentAddressing.ContentAssetNames.Screen),
                         Novels.ContentAddressing.ContentAddressConvention.NotificationPrefab(
                             prefix, episode.Id, Novels.ContentAddressing.ContentAssetNames.Screen),
                     })
            {
                ValidateBundleAssignment(assetPath, episode.BundleName, errors);
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
