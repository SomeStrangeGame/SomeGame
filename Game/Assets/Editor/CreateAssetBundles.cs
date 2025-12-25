using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Editor
{
    public class CreateAssetBundles
    {
        [MenuItem("Assets/Build AssetBundles")]
        private static async void BuildAllAssetBundles()
        {
            var bundlesVersion = string.Empty;
            var bundlesVersionPath = GetPath("BundlesVersion.json");
            using (var request = UnityWebRequest.Get(bundlesVersionPath))
            {
                SetHeaders(request);
                await request.SendWebRequest();
                bundlesVersion = request.downloadHandler.text;
            }
            
            var remotePath = "Assets/StreamingAssets/Remote";
            if (!Directory.Exists(remotePath))
            {
                Directory.CreateDirectory(remotePath);
                Debug.Log($"Create remote folder {remotePath}");
            }

            var versionPath = $"Assets/StreamingAssets/Remote/{bundlesVersion}";
            if (!Directory.Exists(versionPath))
            {
                Directory.CreateDirectory(versionPath);
                Debug.Log($"Create version folder {versionPath}");
            }

            var assetBundleDirectoryWebGL = $"{versionPath}/WebGL";
            if (!Directory.Exists(assetBundleDirectoryWebGL)) 
            {
                Directory.CreateDirectory(assetBundleDirectoryWebGL);
                Debug.Log($"Create remote folder {assetBundleDirectoryWebGL}");
            }
            BuildPipeline.BuildAssetBundles(assetBundleDirectoryWebGL, BuildAssetBundleOptions.None, BuildTarget.WebGL);

            string GetPath(string localPath)
            {
                var result = $"{Application.streamingAssetsPath}/{localPath}";
                #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                result = $"file://{result}";
                #endif
                return result;
            }

            void SetHeaders(UnityWebRequest request)
            {
                request.SetRequestHeader("Access-Control-Allow-Credentials", "true");
                request.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
                request.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                request.SetRequestHeader("Access-Control-Allow-Origin", "*");
            }
        }
    }
}

