using System.IO;
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

        [MenuItem("Novels/Rebuild Local Bootstrap Screen")]
        private static void Build()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
            var screen = Novels.Bootstrap.View.Screen.CreateGenerated();
            try
            {
                var prefab = PrefabUtility.SaveAsPrefabAsset(screen.gameObject, _path);
                prefab.transform.localScale = Vector3.one;
                EditorUtility.SetDirty(prefab.transform);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            finally
            {
                Object.DestroyImmediate(screen.gameObject);
            }
        }
    }
}
