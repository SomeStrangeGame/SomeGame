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
        internal ContentProjectKind Kind;
        internal int DefinitionCount;
        internal string DefinitionPath;
        internal string BundleName;
        internal string DeliveryGroup;
        internal string MinimumClientVersion;
        internal Content.NovelDefinition Story;
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
            new ConfigurationRule(),
            new StoryRule(),
            new CatalogRule(),
            new BundleRule(),
        };

        internal static ContentProject Validate()
        {
            var report = new ValidationReport();
            var project = ContentProjectInspector.Inspect();
            foreach (var rule in _rules)
                rule.Validate(project, report);
            report.LogWarnings();
            report.ThrowIfInvalid();
            return project;
        }
    }

    internal static class ContentProjectInspector
    {
        internal const string CatalogConfig = "Config/catalog.json";
        internal const string StoryConfig = "Config/card.json";
        internal const string CatalogScreen =
            "Assets/RemoteAssets/catalog/screen.prefab";

        internal static ContentProject Inspect()
        {
            var definitions = AssetDatabase.FindAssets("t:NovelContentAsset");
            if (definitions.Length > 0)
            {
                return new ContentProject
                {
                    Kind = ContentProjectKind.Story,
                    DefinitionCount = definitions.Length,
                    DefinitionPath = AssetDatabase.GUIDToAssetPath(definitions[0]),
                };
            }
            return new ContentProject
            {
                Kind = AssetDatabase.LoadAssetAtPath<GameObject>(CatalogScreen) == null
                    ? ContentProjectKind.Unknown
                    : ContentProjectKind.Catalog,
            };
        }

        internal static string Absolute(string relativePath) =>
            Path.Combine(
                ProjectRoot(),
                relativePath.Replace('/', Path.DirectorySeparatorChar));

        private static string ProjectRoot() =>
            Directory.GetParent(Application.dataPath)?.FullName
            ?? throw new InvalidOperationException(
                "Unity project root cannot be resolved.");
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

    internal sealed class ConfigurationRule : IContentValidationRule
    {
        [Serializable]
        private sealed class Metadata
        {
            public int schemaVersion;
            public string minimumClientVersion;
        }

        public void Validate(ContentProject project, ValidationReport report)
        {
            if (project.Kind == ContentProjectKind.Unknown)
                return;
            var relativePath = project.Kind == ContentProjectKind.Catalog
                ? ContentProjectInspector.CatalogConfig
                : ContentProjectInspector.StoryConfig;
            var path = ContentProjectInspector.Absolute(relativePath);
            if (!File.Exists(path))
            {
                report.Error(
                    "CONTENT_CONFIG_MISSING",
                    "Configuration file is missing.",
                    relativePath);
                return;
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
                    return;
                }
                project.MinimumClientVersion = value.minimumClientVersion.Trim();
            }
            catch (Exception exception)
            {
                report.Error(
                    "CONTENT_CONFIG_INVALID",
                    exception.Message,
                    relativePath);
            }
        }
    }

    internal sealed class StoryRule : IContentValidationRule
    {
        public void Validate(ContentProject project, ValidationReport report)
        {
            if (project.Kind != ContentProjectKind.Story)
                return;
            var asset = AssetDatabase.LoadAssetAtPath<Content.NovelContentAsset>(
                project.DefinitionPath);
            if (asset == null)
            {
                report.Error(
                    "CONTENT_DEFINITION_INVALID",
                    "NovelContentAsset cannot be loaded.",
                    project.DefinitionPath);
                return;
            }
            try
            {
                project.Story = asset.ToDefinition();
                project.BundleName = ContentAddressing.ContentPackageConvention
                    .ContentBundle(project.Story.Id);
                project.DeliveryGroup = ContentAddressing.ContentPackageConvention
                    .StoryDeliveryGroup(project.Story.Id);
            }
            catch (Exception exception)
            {
                report.Error(
                    "CONTENT_DEFINITION_INVALID",
                    exception.Message,
                    project.DefinitionPath);
                return;
            }
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
            project.BundleName = ContentAddressing.ContentPackageConvention
                .CatalogBundleName;
            project.DeliveryGroup = ContentAddressing.ContentPackageConvention
                .ApplicationDeliveryGroup;
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
