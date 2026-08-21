using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    internal static class AssetBundleBuildPipeline
    {
        internal static IReadOnlyList<ContentBuildResult> Build(
            NovelContentBuildProfile profile,
            string remotePath,
            ContentBuildSnapshot snapshot)
        {
            var targets = profile?.Targets
                ?? throw new ArgumentNullException(nameof(profile));
            profile.Validate();
            if (targets == null || targets.Length == 0)
                throw new ArgumentException("At least one build target is required.", nameof(targets));

            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));
            if (string.IsNullOrWhiteSpace(remotePath))
                throw new ArgumentException("Remote output path is required.", nameof(remotePath));
            if (Directory.Exists(remotePath))
                throw new InvalidOperationException(
                    $"AssetBundle output path must be empty: {remotePath}");
            Directory.CreateDirectory(remotePath);
            var releases = new Dictionary<BuildTarget, string>();
            foreach (var target in targets)
            {
                releases[target] = BuildTargetBundles(
                    target,
                    Path.Combine(remotePath, GetPlatformName(target)),
                    profile,
                    snapshot.Project.BundleDeliveryGroups,
                    snapshot.Files);
            }
            Debug.Log($"AssetBundle workspace build completed: {remotePath}");
            return targets.Select(target => new ContentBuildResult(
                    target,
                    GetPlatformName(target),
                    releases[target],
                    Path.Combine(remotePath, GetPlatformName(target))))
                .ToArray();
        }

        private static string BuildTargetBundles(
            BuildTarget target,
            string targetPath,
            NovelContentBuildProfile profile,
            IReadOnlyDictionary<string, string> bundleDeliveryGroups,
            IReadOnlyList<ContentBuildFile> buildFiles)
        {
            var releaseFiles = buildFiles
                .Select(file => file.ToDto())
                .ToArray();
            Directory.CreateDirectory(targetPath);
            var manifest = BuildPipeline.BuildAssetBundles(
                targetPath,
                BuildAssetBundleOptions.None,
                target);
            if (manifest == null)
                throw new InvalidOperationException($"AssetBundle build failed for {target}.");

            var releaseBundles = new List<Bundles.BundleReleaseEntry>();
            foreach (var bundle in manifest.GetAllAssetBundles())
            {
                var hash = manifest.GetAssetBundleHash(bundle).ToString();
                var sourceFile = Path.Combine(targetPath, bundle);
                if (!File.Exists(sourceFile))
                    throw new FileNotFoundException("Built bundle is missing.", sourceFile);

                if (!BuildPipeline.GetCRCForAssetBundle(sourceFile, out var crc))
                    throw new InvalidOperationException(
                        $"Could not calculate CRC for bundle '{bundle}'.");
                var fileInfo = new FileInfo(sourceFile);
                var bundleSize = fileInfo.Length;
                var sha256 = Bundles.ContentHash.ComputeSha256(sourceFile);

                var bundleDirectory = Path.Combine(targetPath, bundle);
                var temporaryFile = sourceFile + ".built";
                File.Move(sourceFile, temporaryFile);
                Directory.CreateDirectory(bundleDirectory);
                File.Move(temporaryFile, Path.Combine(bundleDirectory, hash));
                releaseBundles.Add(new Bundles.BundleReleaseEntry
                {
                    name = bundle,
                    version = hash,
                    size = bundleSize,
                    sha256 = sha256,
                    crc = crc,
                    deliveryGroup = bundleDeliveryGroups.TryGetValue(
                        bundle,
                        out var groupId)
                            ? groupId
                            : throw new InvalidOperationException(
                                $"AssetBundle '{bundle}' has no delivery-group owner."),
                });
            }

            var deliveryGroups = ContentBuildReport.BuildGroups(
                releaseBundles,
                releaseFiles);
            var release = new Bundles.ContentReleaseDto
            {
                minimumClientVersion = profile.MinimumClientVersion,
                contentSchemaVersion = profile.ContentSchemaVersion,
                deliveryMode = profile.DeliveryMode,
                bundles = releaseBundles.ToArray(),
                files = releaseFiles.ToArray(),
                deliveryGroups = deliveryGroups,
            };
            release.releaseId = Bundles.ContentReleaseFingerprint.Compute(release);
            Bundles.ContentReleaseValidator.Validate(
                release,
                Application.version,
                profile.ContentSchemaVersion,
                profile.ContentSchemaVersion);
            File.WriteAllText(
                Path.Combine(targetPath, "release.json"),
                Bundles.ContentReleaseCodec.Serialize(release),
                new UTF8Encoding(false));
            ContentBuildReport.Log(
                releaseFiles,
                deliveryGroups,
                releaseBundles.Sum(bundle => bundle.size),
                profile);
            return release.releaseId;
        }

        internal static string GetPlatformName(BuildTarget target)
        {
            return target switch
            {
                BuildTarget.Android => "Android",
                BuildTarget.iOS => "iOS",
                BuildTarget.WebGL => "WebGL",
                BuildTarget.StandaloneOSX => "Mac",
                BuildTarget.StandaloneWindows64 => "Win",
                _ => throw new NotSupportedException(
                    $"AssetBundle output is not configured for {target}."),
            };
        }
    }

    internal sealed class ContentBuildSnapshot
    {
        private ContentBuildSnapshot(
            ContentProjectIndex project,
            IDictionary<string, string> deliveryIndex,
            IList<ContentBuildFile> files)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));
            DeliveryIndex = new ReadOnlyDictionary<string, string>(deliveryIndex);
            Files = new ReadOnlyCollection<ContentBuildFile>(files);
        }

        internal ContentProjectIndex Project { get; }
        internal IReadOnlyDictionary<string, string> DeliveryIndex { get; }
        internal IReadOnlyList<ContentBuildFile> Files { get; }

        internal static ContentBuildSnapshot Create(ContentProjectIndex project)
        {
            var deliveryIndex = ContentDeliveryIndexBuilder.Build(project);
            var files = new List<ContentBuildFile>();
            foreach (var file in ContentFilePolicy.EnumerateFiles())
            {
                var relative = ContentFilePolicy.GetRelativePath(file);
                if (!deliveryIndex.TryGetValue(relative, out var deliveryGroup))
                    continue;
                var info = new FileInfo(file);
                var sha256 = Bundles.ContentHash.ComputeSha256(file);
                files.Add(new ContentBuildFile(
                    relative,
                    Novels.ContentAddressing.ContentPackageConvention.ContentPayload(sha256),
                    info.Length,
                    sha256,
                    deliveryGroup));
            }
            return new ContentBuildSnapshot(
                project,
                new Dictionary<string, string>(
                    deliveryIndex,
                    StringComparer.OrdinalIgnoreCase),
                files.OrderBy(value => value.Path, StringComparer.Ordinal).ToList());
        }
    }

    internal readonly struct ContentBuildFile
    {
        internal ContentBuildFile(
            string path,
            string payloadPath,
            long size,
            string sha256,
            string deliveryGroup)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            PayloadPath = payloadPath ?? throw new ArgumentNullException(nameof(payloadPath));
            Size = size;
            Sha256 = sha256 ?? throw new ArgumentNullException(nameof(sha256));
            DeliveryGroup = deliveryGroup
                ?? throw new ArgumentNullException(nameof(deliveryGroup));
        }

        internal string Path { get; }
        internal string PayloadPath { get; }
        internal long Size { get; }
        internal string Sha256 { get; }
        internal string DeliveryGroup { get; }

        internal Bundles.ContentFileEntry ToDto() => new()
        {
            path = Path,
            payloadPath = PayloadPath,
            size = Size,
            sha256 = Sha256,
            deliveryGroup = DeliveryGroup,
        };
    }
}
