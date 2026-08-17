using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor
{
    public static class CreateAssetBundles
    {
        [MenuItem("Build/Clear Cache")]
        private static void ClearCache()
        {
            var cachePath = Path.Combine(
                Application.persistentDataPath,
                "CachedFiles",
                "Remote");
            if (!Directory.Exists(cachePath))
            {
                Debug.Log($"No cache files in {cachePath}");
                return;
            }

            Directory.Delete(cachePath, true);
            Debug.Log("Clear cache files done!");
        }

        [MenuItem("Build/All Bundles")]
        private static void BuildAllAssetBundles()
        {
            BuildAndroidBundles();
        }

        public static void BuildAndroidBundles()
        {
            NovelContentValidator.ValidateOrThrow();
            var profile = NovelContentBuildProfile.Load();
            var results = AssetBundleBuildPipeline.Build(profile);
            NovelContentValidator.ValidateBuiltOutputOrThrow();
            foreach (var result in results)
            {
                ContentPublishArtifactBuilder.Build(result, profile);
                if (profile.DeliveryMode != Bundles.ContentDeliveryMode.Remote)
                    PlayerContentSeedBuilder.Build(result, profile);
            }
        }

        public static void BuildAndroidBundlesBatch()
        {
            EditorSceneManager.OpenScene(
                "Assets/Novels/Novels.unity",
                OpenSceneMode.Single);
            BuildAndroidBundles();
        }
    }
}
