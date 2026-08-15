using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class CreateAssetBundles
    {
        private class BuildBundleLog : IDisposable
        {
            private readonly string _bundleName;

            public BuildBundleLog(string bundleName)
            {
                _bundleName = bundleName;
                Debug.Log($"Start {_bundleName} building");
            }

            public void Dispose()
            {
                Debug.Log($"Done {_bundleName} building");
            }
        }

        private static void ClearConsole()
        {
            var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }

        [MenuItem("Build/Clear Cache")]
        private static void ClearCache()
        {
            var cachePath = Path.Combine(Application.persistentDataPath, "CachedFiles", "Remote");
            if (Directory.Exists(cachePath))
            {
                Directory.Delete(cachePath, true);
                Debug.Log("Clear cache files done!");
            }
            else
            {
                Debug.Log($"No cache files in {cachePath}");
            }
        }

        [MenuItem("Build/All Bundles")]
        private static void BuildAllAssetBundles()
        {
            NovelContentValidator.ValidateOrThrow();
            ClearConsole();

            var remotePath = $"{Application.streamingAssetsPath}/Remote";
            if (Directory.Exists(remotePath))
                Directory.Delete(remotePath, true);

            if (!Directory.Exists(remotePath))
                Directory.CreateDirectory(remotePath);

            var bundlePath = string.Empty;
            //var bundlePath = $"{remotePath}/WebGL";
            //using (new BuildBundleLog(bundlePath))
            //    BuildBundles(BuildTarget.WebGL, bundlePath);
            //bundlePath = $"{remotePath}/Mac";
            //using (new BuildBundleLog(bundlePath))
            //    BuildBundles(BuildTarget.StandaloneOSX, bundlePath);
            bundlePath = $"{remotePath}/Android";
            using (new BuildBundleLog(bundlePath))
                BuildBundles(BuildTarget.Android, bundlePath);
            //bundlePath = $"{remotePath}/Win";
            //using (new BuildBundleLog(bundlePath))
            //    BuildBundles(BuildTarget.StandaloneWindows64, bundlePath);
            Debug.Log("All bundles building done!");

            void BuildBundles(BuildTarget buildTarget, string targetFolderPath)
            {
                if (!Directory.Exists(targetFolderPath))
                    Directory.CreateDirectory(targetFolderPath);
                var manifest = BuildPipeline.BuildAssetBundles(targetFolderPath, BuildAssetBundleOptions.None, buildTarget);
                UpdateBundleFolders(manifest, targetFolderPath);
            }

            void UpdateBundleFolders(AssetBundleManifest manifest, string targetFolderPath)
            {
                foreach (var bundle in manifest.GetAllAssetBundles())
                {
                    var hash = manifest.GetAssetBundleHash(bundle);
                    var newFilePath = $"{targetFolderPath}/{hash}";
                    File.Move($"{targetFolderPath}/{bundle}", newFilePath);

                    var bundlePath = $"{targetFolderPath}/{bundle}";
                    if (!Directory.Exists(bundlePath))
                        Directory.CreateDirectory(bundlePath);

                    File.Move(newFilePath, $"{bundlePath}/{hash}");

                    ByteArrayToCash(Encoding.UTF8.GetBytes($"{hash}"), $"{bundlePath}/version.txt");
                }
            }

            void ByteArrayToCash(byte[] data, string filePath)
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);

                using (var fs = File.Create(filePath))
                    fs.Write(data, 0, data.Length);
            }
        }
    }
}
