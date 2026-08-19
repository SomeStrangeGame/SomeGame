using UnityEditor;
using UnityEngine;

namespace Editor
{
    internal static class BootstrapPrefabBuilder
    {
        private const string _path =
            "Assets/Resources/Novels/BootstrapScreen.prefab";

        public static void BuildBatch()
        {
            Build();
            Debug.Log($"Local bootstrap prefab generated: {_path}");
        }

        [MenuItem("Novels/UI/Rebuild Bootstrap Screen")]
        private static void Build()
        {
            var screen = Novels.Bootstrap.View.Screen.CreateGenerated();
            try
            {
                GeneratedPrefabWriter.Save(screen.gameObject, _path);
            }
            finally
            {
                Object.DestroyImmediate(screen.gameObject);
            }
        }
    }
}
