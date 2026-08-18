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
            NovelContentBuildProfile profile,
            string outputRoot)
        {
            if (string.IsNullOrWhiteSpace(result.PublishPath)
                || !Directory.Exists(result.PublishPath))
            {
                throw new InvalidOperationException(
                    "Publish artifact must be built before the player content seed.");
            }
            if (Directory.Exists(outputRoot))
                throw new InvalidOperationException($"Player seed workspace is not empty: {outputRoot}");
            Directory.CreateDirectory(outputRoot);
            var releaseSource = Path.Combine(result.RemotePath, "release.json");
            var release = JsonUtility.FromJson<ContentReleaseDto>(
                File.ReadAllText(releaseSource));
            var seedRemote = Path.Combine(outputRoot, "Remote", result.Platform);
            Directory.CreateDirectory(seedRemote);
            File.Copy(releaseSource, Path.Combine(seedRemote, "release.json"), true);

            if (profile.DeliveryMode == ContentDeliveryMode.Embedded)
            {
                CopyDirectory(result.PublishPath, outputRoot);
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
                        CopyContentFile(file.path, file.payloadPath, outputRoot);
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

            var size = Directory.GetFiles(outputRoot, "*", SearchOption.AllDirectories)
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

            result.PlayerSeedPath = outputRoot;
            Debug.Log(
                $"Novel player content seed workspace ({profile.DeliveryMode}) completed: "
                + outputRoot);
            return outputRoot;
        }

        private static void CopyContentFile(
            string relativePath,
            string payloadPath,
            string destinationRoot)
        {
            var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
            var source = Path.Combine(Application.streamingAssetsPath, normalized);
            var destination = Path.Combine(
                destinationRoot,
                payloadPath.Replace('/', Path.DirectorySeparatorChar));
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
