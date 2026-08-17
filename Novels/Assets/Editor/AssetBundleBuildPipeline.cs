using System;
using System.IO;
using System.Text;
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

            foreach (var bundle in manifest.GetAllAssetBundles())
            {
                var hash = manifest.GetAssetBundleHash(bundle).ToString();
                var sourceFile = Path.Combine(targetPath, bundle);
                if (!File.Exists(sourceFile))
                    throw new FileNotFoundException("Built bundle is missing.", sourceFile);

                var bundleDirectory = Path.Combine(targetPath, bundle);
                var temporaryFile = sourceFile + ".built";
                File.Move(sourceFile, temporaryFile);
                Directory.CreateDirectory(bundleDirectory);
                File.Move(temporaryFile, Path.Combine(bundleDirectory, hash));
                File.WriteAllText(
                    Path.Combine(bundleDirectory, "version.txt"),
                    hash,
                    new UTF8Encoding(false));
            }
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
