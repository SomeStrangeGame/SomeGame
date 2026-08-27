using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Novels.ContentSdk.Editor
{
    internal enum ContentProjectKind
    {
        Unknown,
        Catalog,
        Story,
    }

    internal sealed class ContentBuildPlan
    {
        internal ContentBuildPlan(
            ContentProjectKind kind,
            string bundleName,
            string deliveryGroup,
            string minimumClientVersion)
        {
            Kind = kind;
            BundleName = bundleName;
            DeliveryGroup = deliveryGroup;
            MinimumClientVersion = minimumClientVersion;
        }

        internal ContentProjectKind Kind { get; }
        internal string BundleName { get; }
        internal string DeliveryGroup { get; }
        internal string MinimumClientVersion { get; }
    }

    internal static class ContentValidator
    {
        private const string _catalogConfig = "Config/catalog.json";
        private const string _storyConfig = "Config/card.json";

        [Serializable]
        private sealed class Metadata
        {
            public int schemaVersion;
            public string minimumClientVersion;
        }

        internal static ContentBuildPlan Validate()
        {
            var report = new ValidationReport();
            var kind = FindProjectKind(report);
            var minimumClientVersion = ReadMinimumClientVersion(kind, report);
            var story = kind == ContentProjectKind.Story
                ? LoadStory(report)
                : null;

            if (story != null)
            {
                ValidateStoryCard(story.Id, report);
                ValidateStorySources(story, report);
            }
            else if (kind == ContentProjectKind.Catalog)
                ValidateCatalog(report);

            ValidateDefinitionCount(kind, report);
            ValidateBundle(report, kind);
            report.ThrowIfInvalid();

            if (kind == ContentProjectKind.Catalog)
            {
                return new ContentBuildPlan(
                    kind,
                    ContentAddressing.ContentPackageConvention.CatalogBundleName,
                    ContentAddressing.ContentPackageConvention.ApplicationDeliveryGroup,
                    minimumClientVersion);
            }

            return new ContentBuildPlan(
                kind,
                ContentAddressing.ContentPackageConvention.ContentBundle(story.Id),
                ContentAddressing.ContentPackageConvention.StoryDeliveryGroup(story.Id),
                minimumClientVersion);
        }

        private static ContentProjectKind FindProjectKind(ValidationReport report)
        {
            var hasCatalog = File.Exists(Absolute(_catalogConfig));
            var hasStory = File.Exists(Absolute(_storyConfig));
            if (hasCatalog == hasStory)
            {
                report.Error(
                    hasCatalog
                        ? "CONTENT_PROJECT_AMBIGUOUS"
                        : "CONTENT_PROJECT_UNKNOWN",
                    hasCatalog
                        ? $"Keep only one project marker: {_catalogConfig} or {_storyConfig}."
                        : $"Add one project marker: {_catalogConfig} or {_storyConfig}.");
                return ContentProjectKind.Unknown;
            }
            return hasCatalog ? ContentProjectKind.Catalog : ContentProjectKind.Story;
        }

        private static Content.NovelDefinition LoadStory(ValidationReport report)
        {
            var definitions = FindDefinitions();
            if (definitions.Length != 1)
                return null;
            var path = definitions[0];
            try
            {
                return AssetDatabase.LoadAssetAtPath<Content.NovelContentAsset>(path)
                           ?.ToDefinition()
                       ?? throw new InvalidOperationException(
                           "NovelContentAsset cannot be loaded.");
            }
            catch (Exception exception)
            {
                report.Error("CONTENT_DEFINITION_INVALID", exception.Message, path);
                return null;
            }
        }

        private static void ValidateDefinitionCount(
            ContentProjectKind kind,
            ValidationReport report)
        {
            if (kind == ContentProjectKind.Unknown)
                return;
            var count = FindDefinitions().Length;
            var expected = kind == ContentProjectKind.Story ? 1 : 0;
            if (count != expected)
            {
                report.Error(
                    "CONTENT_DEFINITION_COUNT",
                    kind == ContentProjectKind.Story
                        ? $"A story project must contain exactly one NovelContentAsset; found {count}."
                        : $"A catalog project must not contain NovelContentAsset; found {count}.");
            }
        }

        private static void ValidateStoryCard(
            string storyId,
            ValidationReport report)
        {
            var relativePath = _storyConfig;
            var path = Absolute(relativePath);
            if (!File.Exists(path))
                return;
            try
            {
                var card = Catalog.Contracts.CatalogContractCodec.DeserializeCard(
                    File.ReadAllText(path),
                    storyId);
                if (!File.Exists(Absolute("Config/" + card.cover)))
                {
                    report.Error(
                        "STORY_COVER_MISSING",
                        $"Cover '{card.cover}' does not exist.",
                        relativePath);
                }
            }
            catch (Exception exception)
            {
                report.Error("STORY_CARD_INVALID", exception.Message, relativePath);
            }
        }

        private static void ValidateStorySources(
            Content.NovelDefinition definition,
            ValidationReport report)
        {
            var path = ContentAssets.InkPath(definition.Prefix, definition.StoryPath);
            if (!File.Exists(path))
            {
                report.Error(
                    "STORY_SOURCE_FILE_MISSING",
                    "Compiled Ink story does not exist.",
                    path);
            }
        }

        private static void ValidateCatalog(ValidationReport report)
        {
            var relativePath = _catalogConfig;
            var path = Absolute(relativePath);
            if (!File.Exists(path))
                return;
            try
            {
                Catalog.Contracts.CatalogContractCodec.DeserializeRegistry(
                    File.ReadAllText(path));
            }
            catch (Exception exception)
            {
                report.Error("CATALOG_CONFIG_INVALID", exception.Message, relativePath);
            }
        }

        private static void ValidateBundle(
            ValidationReport report,
            ContentProjectKind kind)
        {
            if (kind == ContentProjectKind.Unknown)
                return;
            if (ContentAssets.FindBundleAssets().Length == 0)
            {
                report.Error(
                    "CONTENT_ASSETS_MISSING",
                    "The project contains no bundle assets.");
            }
            var labels = AssetDatabase.GetAllAssetBundleNames()
                .Where(name => AssetDatabase
                    .GetAssetPathsFromAssetBundle(name).Length > 0)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            if (labels.Length > 0)
            {
                report.Error(
                    "CONTENT_BUNDLE_LABEL_PRESENT",
                    "Manual AssetBundle labels are not supported: "
                    + string.Join(", ", labels));
            }
        }

        private static string[] FindDefinitions() =>
            AssetDatabase.FindAssets("t:NovelContentAsset")
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();

        private static string Absolute(string relativePath) =>
            Path.Combine(
                ProjectRoot(),
                relativePath.Replace('/', Path.DirectorySeparatorChar));

        private static string ProjectRoot() =>
            Directory.GetParent(Application.dataPath)?.FullName
            ?? throw new InvalidOperationException(
                "Unity project root cannot be resolved.");

        private static string ReadMinimumClientVersion(
            ContentProjectKind kind,
            ValidationReport report)
        {
            if (kind == ContentProjectKind.Unknown)
                return string.Empty;
            var relativePath = kind == ContentProjectKind.Catalog
                ? _catalogConfig
                : _storyConfig;
            var path = Absolute(relativePath);
            if (!File.Exists(path))
            {
                report.Error(
                    "CONTENT_CONFIG_MISSING",
                    "Configuration file is missing.",
                    relativePath);
                return string.Empty;
            }
            try
            {
                var value = JsonUtility.FromJson<Metadata>(File.ReadAllText(path));
                var expectedSchemaVersion = kind == ContentProjectKind.Catalog
                    ? 2
                    : 1;
                if (value == null || value.schemaVersion != expectedSchemaVersion)
                {
                    report.Error(
                        "CONTENT_CONFIG_SCHEMA",
                        $"schemaVersion must be {expectedSchemaVersion}.",
                        relativePath);
                }
                if (string.IsNullOrWhiteSpace(value?.minimumClientVersion))
                {
                    report.Error(
                        "CONTENT_MINIMUM_VERSION_MISSING",
                        "minimumClientVersion is required.",
                        relativePath);
                    return string.Empty;
                }
                return value.minimumClientVersion.Trim();
            }
            catch (Exception exception)
            {
                report.Error(
                    "CONTENT_CONFIG_INVALID",
                    exception.Message,
                    relativePath);
                return string.Empty;
            }
        }
    }

}
