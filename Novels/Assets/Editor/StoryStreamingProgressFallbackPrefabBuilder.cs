using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UI;

namespace Novels.Editor
{
    internal static class StoryStreamingProgressFallbackPrefabBuilder
    {
        private const string _assetPath =
            "Assets/Novels/Resources/Fallbacks/StoryStreamingProgress/screen.prefab";

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(_assetPath) == null)
                Build();
        }

        [MenuItem("Novels/Rebuild Story Streaming Progress Fallback")]
        private static void Rebuild()
        {
            Build();
            Debug.Log($"Rebuilt story streaming progress prefab: {_assetPath}");
        }

        private static void Build()
        {
            EnsureFolder("Assets/Novels/Resources/Fallbacks/StoryStreamingProgress");
            var screenType = Type.GetType("Novels.StoryStreamingProgressScreen, Novels")
                ?? throw new InvalidOperationException(
                    "Novels.StoryStreamingProgressScreen type is unavailable.");
            var overlayType = Type.GetType("Novels.StoryStreamingProgressOverlay, Novels")
                ?? throw new InvalidOperationException(
                    "Novels.StoryStreamingProgressOverlay type is unavailable.");
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root = new GameObject(
                "Story Streaming Progress Fallback",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(CanvasGroup));
            try
            {
                var canvas = root.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 90;
                var scaler = root.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(465f, 1024f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                var canvasGroup = root.GetComponent<CanvasGroup>();
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;

                var panel = CreateImage(
                    "Panel",
                    root.transform,
                    new Color(0.035f, 0.05f, 0.075f, 0.92f));
                var panelRect = panel.rectTransform;
                panelRect.anchorMin = new Vector2(0.5f, 0f);
                panelRect.anchorMax = new Vector2(0.5f, 0f);
                panelRect.pivot = new Vector2(0.5f, 0f);
                panelRect.anchoredPosition = new Vector2(0f, 20f);
                panelRect.sizeDelta = new Vector2(390f, 46f);

                var label = CreateText("Label", panel.transform, font);
                Stretch(label.rectTransform, new Vector2(14f, 6f), new Vector2(-14f, -9f));

                var track = CreateImage(
                    "Progress Track",
                    panel.transform,
                    new Color(1f, 1f, 1f, 0.16f));
                var trackRect = track.rectTransform;
                trackRect.anchorMin = new Vector2(0f, 0f);
                trackRect.anchorMax = new Vector2(1f, 0f);
                trackRect.pivot = new Vector2(0.5f, 0f);
                trackRect.offsetMin = new Vector2(0f, 0f);
                trackRect.offsetMax = new Vector2(0f, 4f);

                var fill = CreateImage(
                    "Progress Fill",
                    track.transform,
                    new Color(0.18f, 0.63f, 1f, 1f));
                Stretch(fill.rectTransform, Vector2.zero, Vector2.zero);
                fill.rectTransform.anchorMax = new Vector2(0f, 1f);

                var screen = root.AddComponent(screenType);
                root.AddComponent(overlayType);
                var serialized = new SerializedObject(screen);
                serialized.FindProperty("_canvasGroup").objectReferenceValue = canvasGroup;
                serialized.FindProperty("_progressFill").objectReferenceValue = fill.rectTransform;
                serialized.FindProperty("_label").objectReferenceValue = label;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, _assetPath);
                AssetDatabase.SaveAssets();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void EnsureFolder(string path)
        {
            var segments = path.Split('/');
            var current = segments[0];
            for (var index = 1; index < segments.Length; index++)
            {
                var next = $"{current}/{segments[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, segments[index]);
                current = next;
            }
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var gameObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            gameObject.transform.SetParent(parent, false);
            var image = gameObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text CreateText(string name, Transform parent, Font font)
        {
            var gameObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            gameObject.transform.SetParent(parent, false);
            var text = gameObject.GetComponent<Text>();
            text.text = "Загрузка истории · 0%";
            text.font = font;
            text.fontSize = 17;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static void Stretch(
            RectTransform rect,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
