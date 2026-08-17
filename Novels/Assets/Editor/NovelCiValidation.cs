using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor
{
    public static class NovelCiValidation
    {
        public static void ValidateExistingContentBatch()
        {
            EditorSceneManager.OpenScene(
                "Assets/Novels/Novels.unity",
                OpenSceneMode.Single);
            NovelContentValidator.ValidateOrThrow();
            NovelContentValidator.ValidateBuiltOutputOrThrow();
            Debug.Log("Novel CI validation completed without errors.");
        }

        public static void BuildAndValidateContentBatch()
        {
            EditorSceneManager.OpenScene(
                "Assets/Novels/Novels.unity",
                OpenSceneMode.Single);
            CreateAssetBundles.BuildAndroidBundles();
            Debug.Log("Novel CI content build and validation completed without errors.");
        }
    }
}
