using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class CreateAssetBundles
    {
        [MenuItem("Assets/Build AssetBundles")]
        private static async void BuildAllAssetBundles()
        {
            var remotePath = $"{Application.streamingAssetsPath}/Remote";
            if (Directory.Exists(remotePath))
                Directory.Delete(remotePath, true);

            if (!Directory.Exists(remotePath))
                Directory.CreateDirectory(remotePath);

            BuildBundles(BuildTarget.WebGL, $"{remotePath}/WebGL");
            BuildBundles(BuildTarget.StandaloneOSX, $"{remotePath}/Mac");
            BuildBundles(BuildTarget.StandaloneWindows64, $"{remotePath}/Win");

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

