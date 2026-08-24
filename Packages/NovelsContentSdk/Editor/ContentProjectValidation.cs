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

    internal sealed class ContentProject
    {
        internal ContentProject(
            ContentProjectKind kind,
            int definitionCount,
            string definitionPath,
            string bundleName,
            string deliveryGroup,
            string minimumClientVersion,
            Content.NovelDefinition story)
        {
            Kind = kind;
            DefinitionCount = definitionCount;
            DefinitionPath = definitionPath;
            BundleName = bundleName;
            DeliveryGroup = deliveryGroup;
            MinimumClientVersion = minimumClientVersion;
            Story = story;
        }

        internal ContentProjectKind Kind { get; }
        internal int DefinitionCount { get; }
        internal string DefinitionPath { get; }
        internal string BundleName { get; }
        internal string DeliveryGroup { get; }
        internal string MinimumClientVersion { get; }
        internal Content.NovelDefinition Story { get; }
    }

    internal interface IContentValidationRule
    {
        void Validate(ContentProject project, ValidationReport report);
    }

    internal static class ContentValidator
    {
        private static readonly IContentValidationRule[] _rules =
        {
            new ProjectStructureRule(),
            new StoryRule(),
            new CatalogRule(),
            new BundleRule(),
        };

        internal static ContentProject Validate()
        {
            var report = new ValidationReport();
            var project = ContentProjectInspector.Inspect(report);
            foreach (var rule in _rules)
                rule.Validate(project, report);
            report.LogWarnings();
            report.ThrowIfInvalid();
            return project;
        }
    }

    internal static class ContentProjectInspector
    {
        [Serializable]
        private sealed class Metadata
        {
            public int schemaVersion;
            public string minimumClientVersion;
        }

        internal const string CatalogConfig = "Config/catalog.json";
        internal const string StoryConfig = "Config/card.json";
        internal const string CatalogScreen =
            "Assets/RemoteAssets/catalog/screen.prefab";

        internal static ContentProject Inspect(ValidationReport report)
        {
            var definitions = AssetDatabase.FindAssets("t:NovelContentAsset");
            var definitionPath = definitions.Length == 0
                ? string.Empty
                : AssetDatabase.GUIDToAssetPath(definitions[0]);
            var kind = definitions.Length > 0
                ? ContentProjectKind.Story
                : AssetDatabase.LoadAssetAtPath<GameObject>(CatalogScreen) == null
                    ? ContentProjectKind.Unknown
                    : ContentProjectKind.Catalog;
            var minimumClientVersion = ReadMinimumClientVersion(kind, report);
            Content.NovelDefinition story = null;
            var bundleName = string.Empty;
            var deliveryGroup = string.Empty;

            if (kind == ContentProjectKind.Story)
            {
                var asset = AssetDatabase.LoadAssetAtPath<Content.NovelContentAsset>(
                    definitionPath);
                try
                {
                    story = asset?.ToDefinition()
                        ?? throw new InvalidOperationException(
                            "NovelContentAsset cannot be loaded.");
                    bundleName = ContentAddressing.ContentPackageConvention
                        .ContentBundle(story.Id);
                    deliveryGroup = ContentAddressing.ContentPackageConvention
                        .StoryDeliveryGroup(story.Id);
                }
                catch (Exception exception)
                {
                    report.Error(
                        "CONTENT_DEFINITION_INVALID",
                        exception.Message,
                        definitionPath);
                }
            }
            else if (kind == ContentProjectKind.Catalog)
            {
                bundleName = ContentAddressing.ContentPackageConvention.CatalogBundleName;
                deliveryGroup = ContentAddressing.ContentPackageConvention
                    .ApplicationDeliveryGroup;
            }

            return new ContentProject(
                kind,
                definitions.Length,
                definitionPath,
                bundleName,
                deliveryGroup,
                minimumClientVersion,
                story);
        }

        internal static string Absolute(string relativePath) =>
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
                ? ContentProjectInspector.CatalogConfig
                : ContentProjectInspector.StoryConfig;
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
                if (value == null || value.schemaVersion != 1)
                {
                    report.Error(
                        "CONTENT_CONFIG_SCHEMA",
                        "schemaVersion must be 1.",
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

    internal sealed class ProjectStructureRule : IContentValidationRule
    {
        public void Validate(ContentProject project, ValidationReport report)
        {
            if (project.Kind == ContentProjectKind.Unknown)
            {
                report.Error(
                    "CONTENT_PROJECT_UNKNOWN",
                    "The project contains neither a NovelContentAsset nor "
                    + "the catalog screen.");
            }
            if (project.DefinitionCount > 1)
            {
                report.Error(
                    "CONTENT_DEFINITION_COUNT",
                    "An atomic project may contain only one NovelContentAsset.");
            }
        }
    }

    internal sealed class StoryRule : IContentValidationRule
    {
        public void Validate(ContentProject project, ValidationReport report)
        {
            if (project.Kind != ContentProjectKind.Story)
                return;
            if (project.Story == null)
                return;
            ValidateCard(project.Story.Id, report);
            ValidateSources(project.Story, report);
        }

        private static void ValidateCard(string storyId, ValidationReport report)
        {
            var relativePath = ContentProjectInspector.StoryConfig;
            var path = ContentProjectInspector.Absolute(relativePath);
            if (!File.Exists(path))
                return;
            try
            {
                var card = Catalog.Contracts.CatalogContractCodec.DeserializeCard(
                    File.ReadAllText(path),
                    storyId);
                var cover = ContentProjectInspector.Absolute(
                    "Config/" + card.cover);
                if (!File.Exists(cover))
                {
                    report.Error(
                        "STORY_COVER_MISSING",
                        $"Cover '{card.cover}' does not exist.",
                        relativePath);
                }
            }
            catch (Exception exception)
            {
                report.Error(
                    "STORY_CARD_INVALID",
                    exception.Message,
                    relativePath);
            }
        }

        private static void ValidateSources(
            Content.NovelDefinition definition,
            ValidationReport report)
        {
            foreach (var episode in definition.Episodes)
            {
                var path = Path.Combine(
                    Application.streamingAssetsPath,
                    "noveltexts",
                    definition.Prefix,
                    episode.StoryPath.Replace(
                        '/',
                        Path.DirectorySeparatorChar));
                if (!File.Exists(path))
                {
                    report.Error(
                        "STORY_SOURCE_FILE_MISSING",
                        "Ink story source does not exist.",
                        path);
                }
            }
        }
    }

    internal sealed class CatalogRule : IContentValidationRule
    {
        public void Validate(ContentProject project, ValidationReport report)
        {
            if (project.Kind != ContentProjectKind.Catalog)
                return;
            var relativePath = ContentProjectInspector.CatalogConfig;
            var path = ContentProjectInspector.Absolute(relativePath);
            if (!File.Exists(path))
                return;
            try
            {
                Catalog.Contracts.CatalogContractCodec.DeserializeRegistry(
                    File.ReadAllText(path));
            }
            catch (Exception exception)
            {
                report.Error(
                    "CATALOG_CONFIG_INVALID",
                    exception.Message,
                    relativePath);
            }
        }
    }

    internal sealed class BundleRule : IContentValidationRule
    {
        public void Validate(ContentProject project, ValidationReport report)
        {
            if (project.Kind == ContentProjectKind.Unknown)
                return;
            if (ContentAssets.FindBundleAssets().Length == 0)
            {
                report.Error(
                    "CONTENT_ASSETS_MISSING",
                    "Assets/RemoteAssets contains no bundle assets.");
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
    }
}
