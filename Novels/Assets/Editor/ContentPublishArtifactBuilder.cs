using System;
using System.IO;
using Bundles;
using UnityEngine;

namespace Editor
{
    internal static class ContentPublishArtifactBuilder
    {
        internal static string Build(
            ContentBuildResult result,
            NovelContentBuildProfile profile,
            string outputRoot)
        {
            var platform = result.Platform;
            if (Directory.Exists(outputRoot))
                throw new InvalidOperationException($"Publish workspace is not empty: {outputRoot}");
            Directory.CreateDirectory(outputRoot);
            var remoteSource = result.RemotePath;
            CopyDirectory(
                remoteSource,
                Path.Combine(outputRoot, "Remote", platform));
            var releasePath = Path.Combine(remoteSource, "release.json");
            var release = JsonUtility.FromJson<ContentReleaseDto>(
                File.ReadAllText(releasePath));
            ContentReleaseValidator.Validate(
                release,
                Application.version,
                profile.ContentSchemaVersion,
                profile.ContentSchemaVersion);
            foreach (var file in release.files ?? Array.Empty<ContentFileEntry>())
            {
                var source = Path.Combine(
                    Application.streamingAssetsPath,
                    file.path.Replace('/', Path.DirectorySeparatorChar));
                var destination = Path.Combine(
                    outputRoot,
                    file.path.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                File.Copy(source, destination, true);
            }

            Debug.Log($"Novel publish workspace completed: {outputRoot}");
            result.PublishPath = outputRoot;
            return outputRoot;
        }

        private static void CopyDirectory(string source, string destination)
        {
            if (!Directory.Exists(source))
                throw new DirectoryNotFoundException(source);
            foreach (var directory in Directory.GetDirectories(
                         source,
                         "*",
                         SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(directory.Replace(source, destination));
            }
            Directory.CreateDirectory(destination);
            foreach (var file in Directory.GetFiles(
                         source,
                         "*",
                         SearchOption.AllDirectories))
            {
                if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    continue;
                var target = file.Replace(source, destination);
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                File.Copy(file, target, true);
            }
        }
    }
}
