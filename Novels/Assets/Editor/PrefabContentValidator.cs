using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Editor
{
    internal static class PrefabContentValidator
    {
        internal static void ValidateCatalog(GameObject prefab, ICollection<string> errors)
        {
            var screen = prefab.GetComponent<Novels.Catalog.View.Screen>();
            if (screen == null)
            {
                errors.Add("Catalog screen prefab has no Catalog.View.Screen component.");
                return;
            }
            var serializedScreen = new SerializedObject(screen);
            ValidateReferences(serializedScreen, "Catalog screen prefab", errors, "_title", "_cardPrefab");
            var card = serializedScreen.FindProperty("_cardPrefab")?.objectReferenceValue
                as Novels.Catalog.View.Card;
            if (card != null)
            {
                ValidateReferences(
                    new SerializedObject(card),
                    "Catalog card prefab",
                    errors,
                    "_title",
                    "_description",
                    "_status",
                    "_button");
            }
            if (prefab.transform.localScale == Vector3.zero)
                errors.Add("Catalog screen prefab root has zero scale.");
            var viewport = prefab.transform.Find("Content/Viewport");
            if (viewport == null || viewport.GetComponent<RectMask2D>() == null)
                errors.Add("Catalog screen viewport must use RectMask2D.");
        }

        internal static void ValidateBootstrap(ICollection<string> errors)
        {
            const string path = "Assets/Resources/Novels/BootstrapScreen.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                errors.Add($"Local bootstrap prefab is missing: {path}");
                return;
            }
            var screen = prefab.GetComponent<Novels.Bootstrap.View.Screen>();
            if (screen == null)
            {
                errors.Add($"Local bootstrap prefab has no Screen component: {path}");
                return;
            }
            ValidateReferences(
                new SerializedObject(screen),
                "Local bootstrap prefab",
                errors,
                "_message",
                "_retryLabel",
                "_retry");
            if (prefab.transform.localScale == Vector3.zero)
                errors.Add("Local bootstrap prefab root has zero scale.");
        }

        private static void ValidateReferences(
            SerializedObject target,
            string owner,
            ICollection<string> errors,
            params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (target.FindProperty(propertyName)?.objectReferenceValue == null)
                    errors.Add($"{owner} has no '{propertyName}' reference.");
            }
        }
    }
}
