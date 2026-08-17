using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bundles;
using UnityEngine;

namespace Editor
{
    internal static class PlayerContentSeedBuilder
    {
        internal static string Build(
            ContentBuildResult result,
            NovelContentBuildProfile profile)
        {
            if (string.IsNullOrWhiteSpace(result.PublishPath)
                || !Directory.Exists(result.PublishPath))
            {
                throw new InvalidOperationException(
                    "Publish artifact must be built before the player content seed.");
            }
            var projectPath = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Project path cannot be resolved.");
            var outputRoot = Path.Combine(
                projectPath,
                profile.PlayerSeedRoot.Replace('/', Path.DirectorySeparatorChar),
                result.Platform);
            var stagingRoot = outputRoot + ".staging";
            var backupRoot = outputRoot + ".previous";
            if (Directory.Exists(stagingRoot))
                Directory.Delete(stagingRoot, true);
            Directory.CreateDirectory(stagingRoot);

            try
            {
                var releaseSource = Path.Combine(result.RemotePath, "release.json");
                var release = JsonUtility.FromJson<ContentReleaseDto>(
                    File.ReadAllText(releaseSource));
                var seedRemote = Path.Combine(stagingRoot, "Remote", result.Platform);
                Directory.CreateDirectory(seedRemote);
                File.Copy(releaseSource, Path.Combine(seedRemote, "release.json"), true);

                if (profile.DeliveryMode == ContentDeliveryMode.Embedded)
                {
                    CopyDirectory(result.PublishPath, stagingRoot);
                }
                else if (profile.DeliveryMode == ContentDeliveryMode.Hybrid)
                {
                    var embeddedGroups = new HashSet<string>(
                        profile.EmbeddedDeliveryGroups,
                        StringComparer.OrdinalIgnoreCase);
                    var knownGroups = new HashSet<string>(
                        (release.deliveryGroups ?? Array.Empty<ContentDeliveryGroupEntry>())
                            .Select(group => group.id),
                        StringComparer.OrdinalIgnoreCase);
                    foreach (var group in embeddedGroups)
                    {
                        if (!knownGroups.Contains(group))
                        {
                            throw new InvalidOperationException(
                                $"Embedded delivery group '{group}' is absent from release.");
                        }
                    }
                    foreach (var file in release.files ?? Array.Empty<ContentFileEntry>())
                    {
                        if (embeddedGroups.Contains(file.deliveryGroup))
                            CopyContentFile(file.path, stagingRoot);
                    }
                    foreach (var bundle in release.bundles ?? Array.Empty<BundleReleaseEntry>())
                    {
                        if (embeddedGroups.Contains(bundle.deliveryGroup))
                        {
                            CopyBundlePayload(
                                bundle,
                                result.RemotePath,
                                seedRemote);
                        }
                    }
                }

                var size = Directory.GetFiles(stagingRoot, "*", SearchOption.AllDirectories)
                    .Where(path => !path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    .Sum(path => new FileInfo(path).Length);
                if (size > profile.EmbeddedBudgetBytes)
                {
                    var message = $"Player content seed exceeds "
                        + $"{FormatBytes(profile.EmbeddedBudgetBytes)}: {FormatBytes(size)}.";
                    if (profile.EnforceEmbeddedBudget)
                        throw new InvalidOperationException(message);
                    Debug.LogWarning(message);
                }

                if (Directory.Exists(backupRoot))
                    Directory.Delete(backupRoot, true);
                if (Directory.Exists(outputRoot))
                    Directory.Move(outputRoot, backupRoot);
                Directory.Move(stagingRoot, outputRoot);
                if (Directory.Exists(backupRoot))
                    Directory.Delete(backupRoot, true);
                result.PlayerSeedPath = outputRoot;
                Debug.Log(
                    $"Novel player content seed ({profile.DeliveryMode}) completed: "
                    + outputRoot);
                return outputRoot;
            }
            catch
            {
                if (Directory.Exists(stagingRoot))
                    Directory.Delete(stagingRoot, true);
                if (!Directory.Exists(outputRoot) && Directory.Exists(backupRoot))
                    Directory.Move(backupRoot, outputRoot);
                throw;
            }
        }

        private static void CopyContentFile(string relativePath, string destinationRoot)
        {
            var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
            var source = Path.Combine(Application.streamingAssetsPath, normalized);
            var destination = Path.Combine(destinationRoot, normalized);
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            File.Copy(source, destination, true);
        }

        private static void CopyBundlePayload(
            BundleReleaseEntry bundle,
            string remoteSource,
            string remoteDestination)
        {
            var source = Path.Combine(remoteSource, bundle.name, bundle.version);
            var destination = Path.Combine(
                remoteDestination,
                bundle.name,
                bundle.version);
            if (!File.Exists(source))
                throw new FileNotFoundException("Bundle payload is missing.", source);
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            File.Copy(source, destination, true);
        }

        private static void CopyDirectory(string source, string destination)
        {
            if (!Directory.Exists(source))
                throw new DirectoryNotFoundException(source);
            Directory.CreateDirectory(destination);
            foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    continue;
                var relative = file.Substring(source.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var target = Path.Combine(destination, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                File.Copy(file, target, true);
            }
        }

        private static string FormatBytes(long bytes) =>
            $"{bytes / (1024d * 1024d):0.0} MiB";
    }
}
