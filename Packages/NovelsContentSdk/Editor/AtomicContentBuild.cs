using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Bundles;
using UnityEditor;
using UnityEngine;

namespace Novels.ContentSdk.Editor
{
    public static class AtomicContentBuild
    {
        private const int _contentSchemaVersion = 5;
        private const string _outputPath = "Build/LocalContent";
        private const string _stagingPath = "Build/AtomicContentStaging";

        private readonly struct ProjectContent
        {
            internal ProjectContent(
                string bundleName,
                string deliveryGroup,
                string minimumClientVersion)
            {
                BundleName = bundleName;
                DeliveryGroup = deliveryGroup;
                MinimumClientVersion = minimumClientVersion;
            }

            internal string BundleName { get; }
            internal string DeliveryGroup { get; }
            internal string MinimumClientVersion { get; }
        }

        [Serializable]
        private sealed class BuildConfiguration
        {
            public int schemaVersion;
            public string minimumClientVersion;
        }

        [MenuItem("Novels/Content/Build Atomic Local Content")]
        public static void BuildLocal()
        {
            var content = InspectProject();
            RecreateDirectory(_outputPath);
            RecreateDirectory(_stagingPath);
            try
            {
                var files = BuildFilePayloads(content.DeliveryGroup);
                foreach (var target in new[]
                         {
                             BuildTarget.StandaloneOSX,
                             BuildTarget.Android,
                             BuildTarget.iOS,
                         })
                {
                    BuildTargetRelease(content, files, target);
                }
                Debug.Log(
                    $"Atomic content '{content.DeliveryGroup}' built to "
                    + Path.GetFullPath(_outputPath));
            }
            finally
            {
                if (Directory.Exists(_stagingPath))
                    Directory.Delete(_stagingPath, true);
                AssetDatabase.Refresh();
            }
        }

        private static ProjectContent InspectProject()
        {
            var buildConfiguration = LoadBuildConfiguration();
            var contentGuids = AssetDatabase.FindAssets("t:NovelContentAsset");
            if (contentGuids.Length == 1)
            {
                var path = AssetDatabase.GUIDToAssetPath(contentGuids[0]);
                var asset = AssetDatabase.LoadAssetAtPath<Content.NovelContentAsset>(path)
                    ?? throw new InvalidOperationException(
                        $"NovelContentAsset cannot be loaded: {path}");
                var definition = asset.ToDefinition();
                var expected = ContentAddressing.ContentPackageConvention
                    .ContentBundle(definition.Id);
                ValidateBundleRoot(path, expected);
                ValidateStorySources(definition);
                ValidateStoryCard(definition.Id);
                return new ProjectContent(
                    expected,
                    definition.Id,
                    buildConfiguration.minimumClientVersion);
            }
            if (contentGuids.Length > 1)
            {
                throw new InvalidOperationException(
                    "An atomic story project must contain exactly one NovelContentAsset.");
            }

            const string catalogScreen = "Assets/RemoteAssets/catalog/screen.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(catalogScreen) == null)
            {
                throw new InvalidOperationException(
                    "The project contains neither one story nor the Catalog UI prefab.");
            }
            ValidateBundleRoot(
                catalogScreen,
                ContentAddressing.ContentPackageConvention.CatalogBundleName);
            ValidateCatalogRegistry();
            return new ProjectContent(
                ContentAddressing.ContentPackageConvention.CatalogBundleName,
                ContentAddressing.ContentPackageConvention.ApplicationDeliveryGroup,
                buildConfiguration.minimumClientVersion);
        }

        private static BuildConfiguration LoadBuildConfiguration()
        {
            var path = Path.Combine(ProjectRoot(), "Config", "build.json");
            if (!File.Exists(path))
                throw new FileNotFoundException("Content build configuration is missing.", path);
            BuildConfiguration configuration;
            try
            {
                configuration = JsonUtility.FromJson<BuildConfiguration>(
                    File.ReadAllText(path));
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"Content build configuration is invalid: {path}",
                    exception);
            }
            if (configuration == null
                || configuration.schemaVersion != 1
                || string.IsNullOrWhiteSpace(configuration.minimumClientVersion))
            {
                throw new InvalidOperationException(
                    $"Content build configuration must use schemaVersion 1 and define "
                    + $"minimumClientVersion: {path}");
            }
            configuration.minimumClientVersion =
                configuration.minimumClientVersion.Trim();
            return configuration;
        }

        private static void ValidateBundleRoot(string assetPath, string expectedBundle)
        {
            var actual = AssetDatabase.GetImplicitAssetBundleName(assetPath);
            if (!string.Equals(actual, expectedBundle, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Asset '{assetPath}' belongs to '{actual}', expected "
                    + $"'{expectedBundle}'. Assign the bundle to the content root folder.");
            }
            var assigned = AssetDatabase.GetAllAssetBundleNames();
            if (assigned.Length != 1
                || !string.Equals(
                    assigned[0],
                    expectedBundle,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "An atomic content project must assign exactly one AssetBundle: "
                    + expectedBundle);
            }
        }

        private static void ValidateStorySources(Content.NovelDefinition definition)
        {
            foreach (var episode in definition.Episodes)
            {
                var path = Path.Combine(
                    Application.streamingAssetsPath,
                    "noveltexts",
                    definition.Prefix,
                    episode.StoryPath.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(path))
                    throw new FileNotFoundException("Ink story source is missing.", path);
            }
        }

        private static void ValidateStoryCard(string storyId)
        {
            var configRoot = Path.Combine(ProjectRoot(), "Config");
            var cardPath = Path.Combine(configRoot, "card.json");
            if (!File.Exists(cardPath))
                throw new FileNotFoundException("Story card is missing.", cardPath);
            var card = Catalog.Contracts.CatalogContractCodec.DeserializeCard(
                File.ReadAllText(cardPath),
                storyId);
            var coverPath = Path.Combine(configRoot, card.cover);
            if (!File.Exists(coverPath))
            {
                throw new FileNotFoundException(
                    $"Story cover '{card.cover}' is missing.",
                    coverPath);
            }
        }

        private static void ValidateCatalogRegistry()
        {
            var path = Path.Combine(ProjectRoot(), "Config", "catalog.json");
            if (!File.Exists(path))
                throw new FileNotFoundException("Catalog registry is missing.", path);
            Catalog.Contracts.CatalogContractCodec.DeserializeRegistry(
                File.ReadAllText(path));
        }

        private static string ProjectRoot() =>
            Directory.GetParent(Application.dataPath)?.FullName
            ?? throw new InvalidOperationException("Unity project root cannot be resolved.");

        private static ContentFileEntry[] BuildFilePayloads(string deliveryGroup)
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
                return Array.Empty<ContentFileEntry>();
            var result = new List<ContentFileEntry>();
            foreach (var source in Directory.EnumerateFiles(
                         Application.streamingAssetsPath,
                         "*",
                         SearchOption.AllDirectories)
                     .Where(path => !path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                     .OrderBy(path => path, StringComparer.Ordinal))
            {
                var relative = Path.GetRelativePath(
                        Application.streamingAssetsPath,
                        source)
                    .Replace('\\', '/');
                var hash = ContentHash.ComputeSha256(source);
                var payloadPath = ContentAddressing.ContentPackageConvention
                    .ContentPayload(hash);
                var destination = Path.Combine(
                    _outputPath,
                    payloadPath.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                if (!File.Exists(destination))
                    File.Copy(source, destination);
                result.Add(new ContentFileEntry
                {
                    path = relative,
                    payloadPath = payloadPath,
                    size = new FileInfo(source).Length,
                    sha256 = hash,
                    deliveryGroup = deliveryGroup,
                });
            }
            return result.ToArray();
        }

        private static void BuildTargetRelease(
            ProjectContent content,
            ContentFileEntry[] files,
            BuildTarget target)
        {
            var platform = GetPlatform(target);
            var staging = Path.Combine(_stagingPath, platform);
            Directory.CreateDirectory(staging);
            var manifest = BuildPipeline.BuildAssetBundles(
                staging,
                BuildAssetBundleOptions.None,
                target) ?? throw new InvalidOperationException(
                    $"AssetBundle build failed for {target}.");
            var bundles = manifest.GetAllAssetBundles();
            if (bundles.Length != 1
                || !string.Equals(
                    bundles[0],
                    content.BundleName,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Expected one bundle '{content.BundleName}', built: "
                    + string.Join(", ", bundles));
            }

            var source = Path.Combine(staging, content.BundleName);
            var version = manifest.GetAssetBundleHash(content.BundleName).ToString();
            if (!BuildPipeline.GetCRCForAssetBundle(source, out var crc))
                throw new InvalidOperationException("AssetBundle CRC cannot be calculated.");
            var destinationDirectory = Path.Combine(
                _outputPath,
                "Remote",
                platform,
                content.BundleName);
            Directory.CreateDirectory(destinationDirectory);
            var destination = Path.Combine(destinationDirectory, version);
            File.Copy(source, destination, true);
            var bundle = new BundleReleaseEntry
            {
                name = content.BundleName,
                version = version,
                size = new FileInfo(destination).Length,
                sha256 = ContentHash.ComputeSha256(destination),
                crc = crc,
                deliveryGroup = content.DeliveryGroup,
            };
            var release = new ContentReleaseDto
            {
                minimumClientVersion = content.MinimumClientVersion,
                contentSchemaVersion = _contentSchemaVersion,
                deliveryMode = ContentDeliveryMode.Remote,
                bundles = new[] {bundle},
                files = files,
                deliveryGroups = new[]
                {
                    new ContentDeliveryGroupEntry
                    {
                        id = content.DeliveryGroup,
                        payloadCount = 1 + files.Length,
                        size = bundle.size + files.Sum(value => value.size),
                    },
                },
            };
            release.releaseId = ContentReleaseFingerprint.Compute(release);
            ContentReleaseValidator.Validate(
                release,
                content.MinimumClientVersion,
                _contentSchemaVersion,
                _contentSchemaVersion);
            var releasePath = Path.Combine(
                _outputPath,
                "Remote",
                platform,
                "release.json");
            File.WriteAllText(
                releasePath,
                ContentReleaseCodec.Serialize(release),
                new UTF8Encoding(false));
        }

        private static string GetPlatform(BuildTarget target) => target switch
        {
            BuildTarget.StandaloneOSX => "Mac",
            BuildTarget.Android => "Android",
            BuildTarget.iOS => "iOS",
            _ => throw new NotSupportedException($"Unsupported content target: {target}"),
        };

        private static void RecreateDirectory(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }
    }
}
