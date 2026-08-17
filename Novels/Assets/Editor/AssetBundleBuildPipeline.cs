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
        internal static void Build(params BuildTarget[] targets)
        {
            if (targets == null || targets.Length == 0)
                throw new ArgumentException("At least one build target is required.", nameof(targets));

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
                foreach (var target in targets)
                    BuildTargetBundles(target, Path.Combine(stagingPath, GetPlatformName(target)));

                if (Directory.Exists(backupPath))
                    Directory.Delete(backupPath, true);
                if (Directory.Exists(remotePath))
                    Directory.Move(remotePath, backupPath);
                Directory.Move(stagingPath, remotePath);
                if (Directory.Exists(backupPath))
                    Directory.Delete(backupPath, true);

                AssetDatabase.Refresh();
                Debug.Log($"AssetBundle build completed: {remotePath}");
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

        private static void BuildTargetBundles(BuildTarget target, string targetPath)
        {
            Directory.CreateDirectory(targetPath);
            var manifest = BuildPipeline.BuildAssetBundles(
                targetPath,
                BuildAssetBundleOptions.None,
                target);
            if (manifest == null)
                throw new InvalidOperationException($"AssetBundle build failed for {target}.");

            var releaseBundles = new List<ReleaseBundle>();
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
                File.WriteAllText(
                    Path.Combine(bundleDirectory, "version.txt"),
                    hash,
                    new UTF8Encoding(false));
                File.WriteAllText(
                    Path.Combine(bundleDirectory, "manifest.json"),
                    JsonUtility.ToJson(
                        new BundleIntegrityManifest
                        {
                            version = hash,
                            size = bundleSize,
                            sha256 = sha256,
                            crc = crc,
                        },
                        true),
                    new UTF8Encoding(false));
                releaseBundles.Add(new ReleaseBundle
                {
                    name = bundle,
                    version = hash,
                    size = bundleSize,
                    sha256 = sha256,
                    crc = crc,
                });
            }

            var releaseFiles = BuildReleaseFiles();
            var releaseId = ComputeReleaseId(releaseBundles, releaseFiles);
            File.WriteAllText(
                Path.Combine(targetPath, "release.json"),
                JsonUtility.ToJson(
                    new ReleaseManifest
                    {
                        releaseId = releaseId,
                        minimumClientVersion = Application.version,
                        contentSchemaVersion = 1,
                        bundles = releaseBundles.ToArray(),
                        files = releaseFiles.ToArray(),
                    },
                    true),
                new UTF8Encoding(false));
        }

        [Serializable]
        private sealed class BundleIntegrityManifest
        {
            public string version;
            public long size;
            public string sha256;
            public uint crc;
        }

        [Serializable]
        private sealed class ReleaseManifest
        {
            public string releaseId;
            public string minimumClientVersion;
            public int contentSchemaVersion;
            public ReleaseBundle[] bundles;
            public ReleaseFile[] files;
        }

        [Serializable]
        private sealed class ReleaseBundle
        {
            public string name;
            public string version;
            public long size;
            public string sha256;
            public uint crc;
        }

        [Serializable]
        private sealed class ReleaseFile
        {
            public string path;
            public long size;
            public string sha256;
        }

        private static List<ReleaseFile> BuildReleaseFiles()
        {
            var result = new List<ReleaseFile>();
            foreach (var directoryName in new[]
                     {
                         "NovelTexts",
                         "NovelsAudio",
                         "NovelsVideos",
                     })
            {
                var directory = Path.Combine(
                    Application.streamingAssetsPath,
                    directoryName);
                if (!Directory.Exists(directory))
                    continue;
                foreach (var file in Directory.GetFiles(
                             directory,
                             "*",
                             SearchOption.AllDirectories))
                {
                    if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                        continue;
                    var relative = file.Substring(
                            Application.streamingAssetsPath.Length + 1)
                        .Replace(Path.DirectorySeparatorChar, '/');
                    var info = new FileInfo(file);
                    result.Add(new ReleaseFile
                    {
                        path = relative,
                        size = info.Length,
                        sha256 = ComputeSha256(file),
                    });
                }
            }
            return result.OrderBy(file => file.path, StringComparer.Ordinal)
                .ToList();
        }

        private static string ComputeReleaseId(
            IEnumerable<ReleaseBundle> bundles,
            IEnumerable<ReleaseFile> files)
        {
            var source = string.Join(
                "\n",
                bundles.OrderBy(bundle => bundle.name, StringComparer.Ordinal)
                    .Select(bundle => $"B:{bundle.name}:{bundle.version}:{bundle.sha256}")
                    .Concat(files.Select(file =>
                        $"F:{file.path}:{file.size}:{file.sha256}")));
            using var sha = SHA256.Create();
            return ToHex(sha.ComputeHash(Encoding.UTF8.GetBytes(source)));
        }

        private static string ComputeSha256(string path)
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

        private static string GetPlatformName(BuildTarget target)
        {
            return target switch
            {
                BuildTarget.Android => "Android",
                BuildTarget.WebGL => "WebGL",
                BuildTarget.StandaloneOSX => "Mac",
                BuildTarget.StandaloneWindows64 => "Win",
                _ => throw new NotSupportedException(
                    $"AssetBundle output is not configured for {target}."),
            };
        }
    }
}
