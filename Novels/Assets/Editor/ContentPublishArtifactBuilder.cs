using System;
using System.IO;
using Bundles;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    internal static class ContentPublishArtifactBuilder
    {
        internal static string Build(
            ContentBuildResult result,
            NovelContentBuildProfile profile)
        {
            var platform = result.Platform;
            var projectPath = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Project path cannot be resolved.");
            var outputRoot = Path.Combine(
                projectPath,
                profile.PublishRoot.Replace('/', Path.DirectorySeparatorChar),
                platform);
            var stagingRoot = outputRoot + ".staging";
            var backupRoot = outputRoot + ".previous";
            if (Directory.Exists(stagingRoot))
                Directory.Delete(stagingRoot, true);
            Directory.CreateDirectory(stagingRoot);

            try
            {
                var remoteSource = Path.Combine(
                    Application.streamingAssetsPath,
                    "Remote",
                    platform);
                CopyDirectory(
                    remoteSource,
                    Path.Combine(stagingRoot, "Remote", platform));
                var releasePath = Path.Combine(remoteSource, "release.json");
                var release = JsonUtility.FromJson<ContentReleaseDto>(
                    File.ReadAllText(releasePath));
                ContentReleaseValidator.Validate(
                    release,
                    Application.version,
                    profile.ContentSchemaVersion);
                foreach (var file in release.files ?? Array.Empty<ContentFileEntry>())
                {
                    var source = Path.Combine(
                        Application.streamingAssetsPath,
                        file.path.Replace('/', Path.DirectorySeparatorChar));
                    var destination = Path.Combine(
                        stagingRoot,
                        file.path.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(destination));
                    File.Copy(source, destination, true);
                }

                if (Directory.Exists(backupRoot))
                    Directory.Delete(backupRoot, true);
                if (Directory.Exists(outputRoot))
                    Directory.Move(outputRoot, backupRoot);
                Directory.Move(stagingRoot, outputRoot);
                if (Directory.Exists(backupRoot))
                    Directory.Delete(backupRoot, true);
                Debug.Log($"Novel publish artifact completed: {outputRoot}");
                result.PublishPath = outputRoot;
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
