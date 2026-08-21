using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Bundles;
using UnityEngine;

namespace Editor
{
    internal static class ContentPublishArtifactBuilder
    {
        private readonly struct PayloadMetadata
        {
            internal PayloadMetadata(long size, string sha256)
            {
                Size = size;
                Sha256 = sha256;
            }

            internal long Size { get; }
            internal string Sha256 { get; }
        }

        private readonly struct PendingRelease
        {
            internal PendingRelease(string source, string destination, string relativePath)
            {
                Source = source;
                Destination = destination;
                RelativePath = relativePath;
            }

            internal string Source { get; }
            internal string Destination { get; }
            internal string RelativePath { get; }
        }

        internal static string Build(
            IReadOnlyList<ContentBuildResult> results,
            NovelContentBuildProfile profile,
            string outputRoot)
        {
            if (results == null || results.Count == 0)
                throw new ArgumentException("At least one build result is required.", nameof(results));
            if (Directory.Exists(outputRoot))
                throw new InvalidOperationException($"Publish workspace is not empty: {outputRoot}");
            Directory.CreateDirectory(outputRoot);
            var payloads = new Dictionary<string, PayloadMetadata>(StringComparer.Ordinal);
            var releases = new List<PendingRelease>();
            var deployment = new Bundles.Editor.ContentDeploymentManifestBuilder();
            foreach (var result in results)
                AddPlatform(
                    result,
                    profile,
                    outputRoot,
                    payloads,
                    releases,
                    deployment);
            foreach (var release in releases)
            {
                CopyRequiredFile(release.Source, release.Destination);
                deployment.AddPayload(
                    release.RelativePath,
                    new FileInfo(release.Source).Length,
                    Bundles.ContentHash.ComputeSha256(release.Source),
                    true);
            }
            File.WriteAllText(
                Path.Combine(outputRoot, "deployment.json"),
                deployment.Serialize(),
                new UTF8Encoding(false));
            foreach (var result in results)
                result.PublishPath = outputRoot;
            Debug.Log($"Novel ServerRoot workspace completed: {outputRoot}");
            return outputRoot;
        }

        private static void AddPlatform(
            ContentBuildResult result,
            NovelContentBuildProfile profile,
            string outputRoot,
            IDictionary<string, PayloadMetadata> payloads,
            ICollection<PendingRelease> releases,
            Bundles.Editor.ContentDeploymentManifestBuilder deployment)
        {
            var remoteSource = result.RemotePath;
            var releaseSource = Path.Combine(remoteSource, "release.json");
            var release = ContentReleaseCodec.DeserializeAndValidate(
                File.ReadAllText(releaseSource),
                Application.version,
                profile.ContentSchemaVersion,
                profile.ContentSchemaVersion);
            var remoteDestination = Path.Combine(outputRoot, "Remote", result.Platform);
            Directory.CreateDirectory(remoteDestination);
            var releaseRelativePath = $"Remote/{result.Platform}/release.json";
            releases.Add(new PendingRelease(
                releaseSource,
                Path.Combine(remoteDestination, "release.json"),
                releaseRelativePath));
            deployment.AddPlatform(
                result.Platform,
                release.releaseId,
                releaseRelativePath);
            foreach (var bundle in release.bundles ?? Array.Empty<BundleReleaseEntry>())
            {
                var relativePath =
                    $"Remote/{result.Platform}/{bundle.name}/{bundle.version}";
                CopyRequiredFile(
                    Path.Combine(remoteSource, bundle.name, bundle.version),
                    Path.Combine(remoteDestination, bundle.name, bundle.version));
                deployment.AddPayload(
                    relativePath,
                    bundle.size,
                    bundle.sha256);
            }
            foreach (var file in release.files ?? Array.Empty<ContentFileEntry>())
            {
                if (payloads.TryGetValue(file.payloadPath, out var existing))
                {
                    if (existing.Size != file.size
                        || !string.Equals(
                            existing.Sha256,
                            file.sha256,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"Shared payload '{file.payloadPath}' differs between platforms.");
                    }
                    continue;
                }
                payloads.Add(
                    file.payloadPath,
                    new PayloadMetadata(file.size, file.sha256));
                var source = Path.Combine(
                    Application.streamingAssetsPath,
                    file.path.Replace('/', Path.DirectorySeparatorChar));
                var destination = Path.Combine(
                    outputRoot,
                    file.payloadPath.Replace('/', Path.DirectorySeparatorChar));
                CopyRequiredFile(source, destination);
                deployment.AddPayload(
                    file.payloadPath,
                    file.size,
                    file.sha256);
            }
        }

        private static void CopyRequiredFile(string source, string destination)
        {
            if (!File.Exists(source))
                throw new FileNotFoundException("Publish payload is missing.", source);
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            File.Copy(source, destination, true);
        }
    }

}
