using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;

namespace Editor
{
    internal sealed class ContentProjectIndex
    {
        internal sealed class Entry
        {
            internal Entry(
                Novels.Catalog.NovelCatalogEntry catalogEntry,
                Novels.Content.NovelContentAsset asset,
                Novels.Content.NovelDefinition definition,
                IDictionary<string, StoryDependencyManifest> storyDependencies)
            {
                CatalogEntry = catalogEntry;
                Asset = asset;
                Definition = definition;
                StoryDependencies = new ReadOnlyDictionary<string, StoryDependencyManifest>(
                    storyDependencies);
            }

            internal Novels.Catalog.NovelCatalogEntry CatalogEntry { get; }
            internal Novels.Content.NovelContentAsset Asset { get; }
            internal Novels.Content.NovelDefinition Definition { get; }
            internal IReadOnlyDictionary<string, StoryDependencyManifest>
                StoryDependencies { get; }
        }

        private ContentProjectIndex(
            Novels.Catalog.NovelCatalogAsset catalog,
            IList<Entry> entries,
            IDictionary<string, string> bundleDeliveryGroups)
        {
            Catalog = catalog;
            Entries = new ReadOnlyCollection<Entry>(entries);
            BundleDeliveryGroups = new ReadOnlyDictionary<string, string>(
                bundleDeliveryGroups);
        }

        internal Novels.Catalog.NovelCatalogAsset Catalog { get; }
        internal IReadOnlyList<Entry> Entries { get; }
        internal IReadOnlyDictionary<string, string> BundleDeliveryGroups { get; }

        internal static ContentProjectIndex BuildOrThrow(string locale)
        {
            var errors = new List<string>();
            if (!TryBuild(locale, errors, out var index))
            {
                throw new InvalidOperationException(
                    "Content project index is invalid:\n- "
                    + string.Join("\n- ", errors));
            }
            return index;
        }

        internal static bool TryBuild(
            string locale,
            ICollection<string> errors,
            out ContentProjectIndex index)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<Novels.Catalog.NovelCatalogAsset>(
                Novels.Catalog.CatalogAddresses.AssetName);
            if (catalog == null)
            {
                errors.Add(
                    $"Novel catalog does not exist: "
                    + Novels.Catalog.CatalogAddresses.AssetName);
                index = null;
                return false;
            }

            var entries = new List<Entry>();
            var contentIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var groups = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [Novels.Catalog.CatalogAddresses.BundleName] =
                    Novels.ContentAddressing.ContentPackageConvention.ApplicationDeliveryGroup,
                [Novels.ContentAddressing.ContentPackageConvention.SharedLoadingBundleName] =
                    Novels.ContentAddressing.ContentPackageConvention.ApplicationDeliveryGroup,
            };

            foreach (var catalogEntry in catalog.Entries)
            {
                if (catalogEntry == null || string.IsNullOrWhiteSpace(catalogEntry.ContentId))
                {
                    errors.Add("Novel catalog contains an entry without content ID.");
                    continue;
                }
                if (!contentIds.Add(catalogEntry.ContentId))
                {
                    errors.Add($"Duplicate catalog content ID: {catalogEntry.ContentId}");
                    continue;
                }

                var asset = AssetDatabase.LoadAssetAtPath<Novels.Content.NovelContentAsset>(
                    catalogEntry.ContentAssetName);
                if (asset == null)
                {
                    errors.Add(
                        $"NovelContentAsset does not exist: {catalogEntry.ContentAssetName}");
                    continue;
                }

                var localizationPath =
                    Novels.ContentAddressing.ContentAddressConvention.LocalizationAsset(
                        catalogEntry.ContentId,
                        Novels.ContentAddressing.ContentAssetNames.LocalizationData);
                var localizationData = AssetDatabase.LoadAssetAtPath<
                    Localization.LocalizationData>(localizationPath);
                if (localizationData == null)
                {
                    errors.Add($"Localization data does not exist: {localizationPath}");
                    continue;
                }

                Novels.Content.NovelDefinition definition;
                try
                {
                    var localization = new Localization.Entity(
                        new Localization.Entity.Ctx
                        {
                            Locale = locale,
                            LocalizationSO = localizationData,
                        });
                    foreach (var key in new[]
                             {
                                 Novels.UiTextKeys.NewGame,
                                 Novels.UiTextKeys.ContinueGame,
                                 Novels.BubbleContracts.BubbleTextKeys.Disclaimer,
                                 Novels.BubbleContracts.BubbleTextKeys.Hint,
                             })
                    {
                        localization.GetRequiredValue(key);
                    }
                    definition = asset.ToDefinition(localization.GetRequiredValue);
                }
                catch (Exception exception)
                {
                    errors.Add($"Content asset '{asset.name}' is invalid: {exception.Message}");
                    continue;
                }
                if (!string.Equals(
                        definition.Id,
                        catalogEntry.ContentId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(
                        $"Catalog content ID '{catalogEntry.ContentId}' does not match "
                        + $"NovelContentAsset ID '{definition.Id}'.");
                    continue;
                }

                var storyDependencies = new Dictionary<string, StoryDependencyManifest>(
                    StringComparer.OrdinalIgnoreCase);
                foreach (var episode in definition.Episodes)
                {
                    storyDependencies.Add(
                        episode.Id,
                        StoryDependencyAnalyzer.Build(
                            definition.Prefix,
                            definition.MainCharacter,
                            episode));
                }
                entries.Add(new Entry(
                    catalogEntry,
                    asset,
                    definition,
                    storyDependencies));
                AddBundle(
                    groups,
                    definition.BundleName,
                    Novels.ContentAddressing.ContentPackageConvention.SharedDeliveryGroup(
                        definition.Id),
                    errors);
                foreach (var episode in definition.Episodes)
                {
                    AddBundle(
                        groups,
                        episode.BundleName,
                        Novels.ContentAddressing.ContentPackageConvention.EpisodeDeliveryGroup(
                            definition.Id,
                            episode.Id),
                        errors);
                }
            }

            index = new ContentProjectIndex(catalog, entries, groups);
            return errors.Count == 0;
        }

        private static void AddBundle(
            IDictionary<string, string> groups,
            string bundleName,
            string groupId,
            ICollection<string> errors)
        {
            if (groups.TryGetValue(bundleName, out var existing)
                && !string.Equals(existing, groupId, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(
                    $"AssetBundle '{bundleName}' belongs to both delivery groups "
                    + $"'{existing}' and '{groupId}'.");
                return;
            }
            groups[bundleName] = groupId;
        }
    }
}
