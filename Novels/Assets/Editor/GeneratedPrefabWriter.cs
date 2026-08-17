using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    internal static class GeneratedPrefabWriter
    {
        internal static GameObject Save(GameObject root, string path)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Prefab path is required.", nameof(path));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            root.transform.localScale = Vector3.one;
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            if (prefab == null)
                throw new InvalidOperationException($"Prefab could not be saved: {path}");
            prefab.transform.localScale = Vector3.one;
            EditorUtility.SetDirty(prefab.transform);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return prefab;
        }
    }
}
