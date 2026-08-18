using System;
using System.Collections.Generic;
using System.IO;
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
            foreach (var result in results)
                AddPlatform(result, profile, outputRoot, payloads);
            foreach (var result in results)
                result.PublishPath = outputRoot;
            Debug.Log($"Novel ServerRoot workspace completed: {outputRoot}");
            return outputRoot;
        }

        private static void AddPlatform(
            ContentBuildResult result,
            NovelContentBuildProfile profile,
            string outputRoot,
            IDictionary<string, PayloadMetadata> payloads)
        {
            var remoteSource = result.RemotePath;
            var releaseSource = Path.Combine(remoteSource, "release.json");
            var release = JsonUtility.FromJson<ContentReleaseDto>(
                File.ReadAllText(releaseSource));
            ContentReleaseValidator.Validate(
                release,
                Application.version,
                profile.ContentSchemaVersion,
                profile.ContentSchemaVersion);
            var remoteDestination = Path.Combine(outputRoot, "Remote", result.Platform);
            Directory.CreateDirectory(remoteDestination);
            File.Copy(
                releaseSource,
                Path.Combine(remoteDestination, "release.json"),
                true);
            foreach (var bundle in release.bundles ?? Array.Empty<BundleReleaseEntry>())
            {
                CopyRequiredFile(
                    Path.Combine(remoteSource, bundle.name, bundle.version),
                    Path.Combine(remoteDestination, bundle.name, bundle.version));
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
