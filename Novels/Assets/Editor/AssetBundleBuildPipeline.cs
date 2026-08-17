using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    internal static class AssetBundleBuildPipeline
    {
        internal static IReadOnlyList<ContentBuildResult> Build(
            NovelContentBuildProfile profile)
        {
            var targets = profile?.Targets
                ?? throw new ArgumentNullException(nameof(profile));
            profile.Validate();
            if (targets == null || targets.Length == 0)
                throw new ArgumentException("At least one build target is required.", nameof(targets));

            var projectIndex = ContentProjectIndex.BuildOrThrow("en");
            var remotePath = Path.Combine(Application.streamingAssetsPath, "Remote");
            var projectPath = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Unity project path cannot be resolved.");
            var stagingPath = Path.Combine(
                projectPath,
                "Library",
                $"NovelBundleStaging-{Guid.NewGuid():N}");
            var backupPath = remotePath + ".previous";

            try
            {
                Directory.CreateDirectory(stagingPath);
                var releases = new Dictionary<BuildTarget, string>();
                foreach (var target in targets)
                {
                    releases[target] = BuildTargetBundles(
                        target,
                        Path.Combine(stagingPath, GetPlatformName(target)),
                        profile,
                        projectIndex.BundleDeliveryGroups);
                }

                if (Directory.Exists(backupPath))
                    Directory.Delete(backupPath, true);
                if (Directory.Exists(remotePath))
                    Directory.Move(remotePath, backupPath);
                Directory.Move(stagingPath, remotePath);
                if (Directory.Exists(backupPath))
                    Directory.Delete(backupPath, true);

                AssetDatabase.Refresh();
                Debug.Log($"AssetBundle build completed: {remotePath}");
                return targets.Select(target => new ContentBuildResult(
                        target,
                        GetPlatformName(target),
                        releases[target],
                        Path.Combine(remotePath, GetPlatformName(target))))
                    .ToArray();
            }
            catch
            {
                if (Directory.Exists(stagingPath))
                    Directory.Delete(stagingPath, true);
                if (!Directory.Exists(remotePath) && Directory.Exists(backupPath))
                    Directory.Move(backupPath, remotePath);
                throw;
            }
        }

        private static string BuildTargetBundles(
            BuildTarget target,
            string targetPath,
            NovelContentBuildProfile profile,
            IReadOnlyDictionary<string, string> bundleDeliveryGroups)
        {
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
                var sha256 = ComputeSha256(sourceFile);

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

            var releaseFiles = BuildReleaseFiles();
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
                JsonUtility.ToJson(release, true),
                new UTF8Encoding(false));
            ContentBuildReport.Log(
                releaseFiles,
                deliveryGroups,
                releaseBundles.Sum(bundle => bundle.size),
                profile);
            return release.releaseId;
        }

        private static List<Bundles.ContentFileEntry> BuildReleaseFiles()
        {
            var result = new List<Bundles.ContentFileEntry>();
            var deliveryGroups = ContentDeliveryIndexBuilder.Build();
            foreach (var file in ContentFilePolicy.EnumerateFiles())
            {
                var relative = ContentFilePolicy.GetRelativePath(file);
                if (!deliveryGroups.TryGetValue(relative, out var deliveryGroup))
                    continue;
                var info = new FileInfo(file);
                result.Add(new Bundles.ContentFileEntry
                {
                    path = relative,
                    size = info.Length,
                    sha256 = ComputeSha256(file),
                    deliveryGroup = deliveryGroup,
                });
            }
            return result.OrderBy(file => file.path, StringComparer.Ordinal)
                .ToList();
        }

        internal static string ComputeSha256(string path)
        {
            using var sha = SHA256.Create();
            using var stream = File.OpenRead(path);
            return ToHex(sha.ComputeHash(stream));
        }

        private static string ToHex(byte[] data)
        {
            return BitConverter.ToString(data)
                .Replace("-", string.Empty)
                .ToLowerInvariant();
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
}
