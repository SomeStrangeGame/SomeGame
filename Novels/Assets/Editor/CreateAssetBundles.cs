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
            BuildConfiguredBundles();
        }

        public static void BuildConfiguredBundles()
        {
            NovelContentValidator.ValidateOrThrow();
            var profile = NovelContentBuildProfile.Load();
            ContentBuildTransaction.Build(profile);
        }

        public static void BuildConfiguredBundlesBatch()
        {
            EditorSceneManager.OpenScene(
                "Assets/Novels/Novels.unity",
                OpenSceneMode.Single);
            BuildConfiguredBundles();
        }
    }
}
