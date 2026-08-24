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
            Directory.CreateDirectory(_outputPath);
            RecreateDirectory(_stagingPath);
            RecreateDirectory(Path.Combine(
                _outputPath,
                "Remote",
                ContentPlatform.Name(target)));
            try
            {
                var files = BuildFilePayloads(plan.DeliveryGroup);
                BuildTargetRelease(plan, files, target);
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

        private static ContentFileEntry[] BuildFilePayloads(string deliveryGroup)
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
            ContentBuildPlan plan,
            ContentFileEntry[] files,
            BuildTarget target)
        {
            var platform = ContentPlatform.Name(target);
            var staging = Path.Combine(_stagingPath, platform);
            Directory.CreateDirectory(staging);
            var build = new AssetBundleBuild
            {
                assetBundleName = plan.BundleName,
                assetNames = ContentAssets.FindBundleAssets(),
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
            var release = CreateRelease(plan, files, bundle);
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

        private static ContentReleaseDto CreateRelease(
            ContentBuildPlan plan,
            ContentFileEntry[] files,
            BundleReleaseEntry bundle)
        {
            var release = new ContentReleaseDto
            {
                minimumClientVersion = plan.MinimumClientVersion,
                contentSchemaVersion = _contentSchemaVersion,
                deliveryMode = ContentDeliveryMode.Remote,
                bundles = new[] {bundle},
                files = files,
                deliveryGroups = new[]
                {
                    new ContentDeliveryGroupEntry
                    {
                        id = plan.DeliveryGroup,
                        payloadCount = 1 + files.Length,
                        size = bundle.size + files.Sum(value => value.size),
                    },
                },
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
