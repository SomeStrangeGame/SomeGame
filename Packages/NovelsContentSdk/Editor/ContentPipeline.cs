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
    internal static class ContentPipeline
    {
        private const int _contentSchemaVersion = 5;
        private const string _outputPath = "Build/LocalContent";
        private const string _stagingPath = "Build/AtomicContentStaging";

        internal static void Validate() => ContentValidator.Validate();

        internal static void Build(string platform)
        {
            var plan = ContentValidator.Validate();
            var target = ContentPlatform.Resolve(platform);
            var streamingExperiment = plan.Kind == ContentProjectKind.Story
                && string.Equals(
                    Environment.GetEnvironmentVariable("NOVELS_STREAMING_EXPERIMENT"),
                    "1",
                    StringComparison.Ordinal);
            Directory.CreateDirectory(_outputPath);
            RecreateDirectory(_stagingPath);
            RecreateDirectory(Path.Combine(
                _outputPath,
                "Remote",
                ContentPlatform.Name(target)));
            try
            {
                var files = BuildFilePayloads(plan, streamingExperiment);
                BuildTargetRelease(plan, files, target, streamingExperiment);
                Debug.Log(
                    $"Atomic content '{plan.DeliveryGroup}' built for "
                    + $"{platform} to {Path.GetFullPath(_outputPath)}");
            }
            finally
            {
                if (Directory.Exists(_stagingPath))
                    Directory.Delete(_stagingPath, true);
                AssetDatabase.Refresh();
            }
        }

        private static ContentFileEntry[] BuildFilePayloads(
            ContentBuildPlan plan,
            bool streamingExperiment)
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
                return Array.Empty<ContentFileEntry>();
            var result = new List<ContentFileEntry>();
            foreach (var source in Directory.EnumerateFiles(
                         Application.streamingAssetsPath,
                         "*",
                         SearchOption.AllDirectories)
                     .Where(path => !path.EndsWith(
                         ".meta",
                         StringComparison.OrdinalIgnoreCase))
                     .OrderBy(path => path, StringComparer.Ordinal))
            {
                var relative = Path.GetRelativePath(
                        Application.streamingAssetsPath,
                        source)
                    .Replace('\\', '/');
                if (!ShouldPublishStreamingAsset(source, relative))
                    continue;
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
                    deliveryGroup = DeliveryGroupForFile(
                        plan,
                        relative,
                        streamingExperiment),
                });
            }
            return result.ToArray();
        }

        private static bool ShouldPublishStreamingAsset(
            string sourcePath,
            string relativePath)
        {
            if (!relativePath.StartsWith(
                    "noveltexts/",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Raw Ink remains in the authoring project because the streaming
            // planner analyzes it at build time, but runtime reads compiled JSON.
            if (relativePath.EndsWith(".ink", StringComparison.OrdinalIgnoreCase))
                return false;

            // Compiled stories and source maps are runtime/analytics artifacts.
            if (!relativePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                || relativePath.EndsWith(
                    ".ink.json",
                    StringComparison.OrdinalIgnoreCase)
                || relativePath.EndsWith(
                    ".source-map.json",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // A legacy plain JSON is safe to omit only when its compiled Ink
            // sibling exists and contains exactly the same text. File.ReadAllText
            // handles an optional UTF-8 BOM, which is the known legacy difference.
            var directory = Path.GetDirectoryName(sourcePath);
            var compiledPath = Path.Combine(
                directory ?? string.Empty,
                Path.GetFileNameWithoutExtension(sourcePath) + ".ink.json");
            return !File.Exists(compiledPath)
                || !string.Equals(
                    File.ReadAllText(sourcePath),
                    File.ReadAllText(compiledPath),
                    StringComparison.Ordinal);
        }

        private static string DeliveryGroupForFile(
            ContentBuildPlan plan,
            string relativePath,
            bool streamingExperiment)
        {
            if (!streamingExperiment)
                return plan.DeliveryGroup;
            if (relativePath.StartsWith("noveltexts/", StringComparison.OrdinalIgnoreCase))
            {
                return ContentAddressing.ContentPackageConvention
                    .StoryChunkDeliveryGroup(plan.DeliveryGroup, 0);
            }
            if (relativePath.StartsWith("novelsvideos/", StringComparison.OrdinalIgnoreCase)
                || relativePath.StartsWith("novelsaudio/", StringComparison.OrdinalIgnoreCase))
            {
                return ContentAddressing.ContentPackageConvention
                    .StoryMediaDeliveryGroup(plan.DeliveryGroup);
            }
            return plan.DeliveryGroup;
        }

        private static void BuildTargetRelease(
            ContentBuildPlan plan,
            ContentFileEntry[] files,
            BuildTarget target,
            bool streamingExperiment)
        {
            var platform = ContentPlatform.Name(target);
            var staging = Path.Combine(_stagingPath, platform);
            Directory.CreateDirectory(staging);
            var assets = ContentAssets.FindBundleAssets();
            if (streamingExperiment)
            {
                BuildStreamingTargetRelease(plan, files, target, platform, staging, assets);
                return;
            }
            var build = new AssetBundleBuild
            {
                assetBundleName = plan.BundleName,
                assetNames = assets,
            };
            var manifest = BuildPipeline.BuildAssetBundles(
                    staging,
                    new[] {build},
                    BuildAssetBundleOptions.None,
                    target)
                ?? throw new InvalidOperationException(
                    $"AssetBundle build failed for {target}.");
            var source = Path.Combine(staging, plan.BundleName);
            var version = manifest.GetAssetBundleHash(plan.BundleName).ToString();
            if (!BuildPipeline.GetCRCForAssetBundle(source, out var crc))
                throw new InvalidOperationException(
                    "AssetBundle CRC cannot be calculated.");
            var destinationDirectory = Path.Combine(
                _outputPath,
                "Remote",
                platform,
                plan.BundleName);
            Directory.CreateDirectory(destinationDirectory);
            var destination = Path.Combine(destinationDirectory, version);
            File.Copy(source, destination, true);
            var bundle = new BundleReleaseEntry
            {
                name = plan.BundleName,
                version = version,
                size = new FileInfo(destination).Length,
                sha256 = ContentHash.ComputeSha256(destination),
                crc = crc,
                deliveryGroup = plan.DeliveryGroup,
            };
            ContentBundleAudit.Audit(
                plan,
                build.assetNames,
                destination,
                bundle.size);
            var release = CreateRelease(plan, files, new[] {bundle});
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

        private static void BuildStreamingTargetRelease(
            ContentBuildPlan plan,
            ContentFileEntry[] files,
            BuildTarget target,
            string platform,
            string staging,
            string[] assets)
        {
            var streaming = ExperimentalStreamingPlan.Create(
                plan.DeliveryGroup,
                assets,
                files.Select(value => value.path).ToArray());
            var mediaGroups = streaming.Media.ToDictionary(
                value => value.path,
                value => value.deliveryGroup,
                StringComparer.OrdinalIgnoreCase);
            foreach (var file in files)
            {
                if (mediaGroups.TryGetValue(file.path, out var group))
                    file.deliveryGroup = group;
            }

            var chunkBuilds = streaming.Chunks
                .Select((chunkAssets, index) => new AssetBundleBuild
                {
                    assetBundleName = ContentAddressing.ContentPackageConvention
                        .StoryChunkBundle(plan.DeliveryGroup, index),
                    assetNames = chunkAssets,
                })
                .ToArray();
            var manifest = BuildPipeline.BuildAssetBundles(
                    staging,
                    chunkBuilds,
                    BuildAssetBundleOptions.None,
                    target)
                ?? throw new InvalidOperationException(
                    $"Streaming AssetBundle build failed for {target}.");
            var bundles = new List<BundleReleaseEntry>();
            var chunks = new ContentStreamingChunkEntry[chunkBuilds.Length];
            for (var index = 0; index < chunkBuilds.Length; index++)
            {
                var build = chunkBuilds[index];
                var group = ContentAddressing.ContentPackageConvention
                    .StoryChunkDeliveryGroup(plan.DeliveryGroup, index);
                bundles.Add(CopyBuiltBundle(
                    plan,
                    manifest,
                    staging,
                    platform,
                    build,
                    group));
                chunks[index] = new ContentStreamingChunkEntry
                {
                    index = index,
                    bundle = build.assetBundleName,
                    deliveryGroup = group,
                    assets = build.assetNames,
                };
            }
            var release = CreateRelease(
                plan,
                files,
                bundles.ToArray(),
                new ContentStreamingPlanEntry
                {
                    chunks = chunks,
                    media = streaming.Media.ToArray(),
                });
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

        private static BundleReleaseEntry CopyBuiltBundle(
            ContentBuildPlan plan,
            AssetBundleManifest manifest,
            string staging,
            string platform,
            AssetBundleBuild build,
            string deliveryGroup)
        {
            var source = Path.Combine(staging, build.assetBundleName);
            var version = manifest.GetAssetBundleHash(build.assetBundleName).ToString();
            if (!BuildPipeline.GetCRCForAssetBundle(source, out var crc))
                throw new InvalidOperationException(
                    $"AssetBundle CRC cannot be calculated for '{build.assetBundleName}'.");
            var directory = Path.Combine(
                _outputPath,
                "Remote",
                platform,
                build.assetBundleName);
            Directory.CreateDirectory(directory);
            var destination = Path.Combine(directory, version);
            File.Copy(source, destination, true);
            var result = new BundleReleaseEntry
            {
                name = build.assetBundleName,
                version = version,
                size = new FileInfo(destination).Length,
                sha256 = ContentHash.ComputeSha256(destination),
                crc = crc,
                deliveryGroup = deliveryGroup,
            };
            ContentBundleAudit.Audit(
                plan,
                build.assetNames,
                destination,
                result.size);
            return result;
        }

        private static ContentReleaseDto CreateRelease(
            ContentBuildPlan plan,
            ContentFileEntry[] files,
            BundleReleaseEntry[] bundles,
            ContentStreamingPlanEntry streamingPlan = null)
        {
            var release = new ContentReleaseDto
            {
                minimumClientVersion = plan.MinimumClientVersion,
                contentSchemaVersion = _contentSchemaVersion,
                deliveryMode = ContentDeliveryMode.Remote,
                bundles = bundles,
                files = files,
                streamingPlan = streamingPlan,
                deliveryGroups = bundles
                    .Select(value => value.deliveryGroup)
                    .Concat(files.Select(value => value.deliveryGroup))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(group => new ContentDeliveryGroupEntry
                    {
                        id = group,
                        payloadCount = bundles.Count(value => string.Equals(
                                value.deliveryGroup, group, StringComparison.OrdinalIgnoreCase))
                            + files.Count(value => string.Equals(
                                value.deliveryGroup, group, StringComparison.OrdinalIgnoreCase)),
                        size = bundles.Where(value => string.Equals(
                                value.deliveryGroup, group, StringComparison.OrdinalIgnoreCase))
                            .Sum(value => value.size)
                            + files.Where(value => string.Equals(
                                value.deliveryGroup, group, StringComparison.OrdinalIgnoreCase))
                            .Sum(value => value.size),
                    })
                    .ToArray(),
            };
            release.releaseId = ContentReleaseFingerprint.Compute(release);
            ContentReleaseValidator.Validate(
                release,
                plan.MinimumClientVersion,
                _contentSchemaVersion,
                _contentSchemaVersion);
            return release;
        }

        private static void RecreateDirectory(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }
    }

    internal static class ContentPlatform
    {
        internal static BuildTarget Resolve(string value) =>
            value?.ToLowerInvariant() switch
            {
                "editor" => BuildTarget.StandaloneOSX,
                "android" => BuildTarget.Android,
                "ios" => BuildTarget.iOS,
                _ => throw new ArgumentException(
                    $"Unknown content platform '{value}'. "
                    + "Use editor, android or ios."),
            };

        internal static string Name(BuildTarget target) => target switch
        {
            BuildTarget.StandaloneOSX => "Mac",
            BuildTarget.Android => "Android",
            BuildTarget.iOS => "iOS",
            _ => throw new NotSupportedException(
                $"Unsupported content target: {target}"),
        };
    }

    internal static class ContentAssets
    {
        private const string _root = "Assets/RemoteAssets";

        internal static string[] FindBundleAssets() =>
            AssetDatabase.FindAssets(string.Empty, new[] {_root})
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => !AssetDatabase.IsValidFolder(path))
                .Where(path => !path.EndsWith(
                    ".cs",
                    StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
    }
}
