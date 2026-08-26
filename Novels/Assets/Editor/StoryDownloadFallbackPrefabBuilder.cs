using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UI;

namespace Novels.Editor
{
    internal static class StoryDownloadFallbackPrefabBuilder
    {
        private const string _assetPath =
            "Assets/Novels/Resources/Fallbacks/StoryDownload/screen.prefab";

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            EnsureExists();
        }

        [MenuItem("Novels/Rebuild Story Download Fallback")]
        private static void Rebuild()
        {
            Build();
            Debug.Log($"Rebuilt story download fallback prefab: {_assetPath}");
        }

        private static void EnsureExists()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(_assetPath) == null)
                Build();
        }

        private static void Build()
        {
            var screenType = Type.GetType("Novels.StoryDownloadScreen, Novels")
                ?? throw new InvalidOperationException(
                    "Novels.StoryDownloadScreen type is unavailable.");
            var overlayType = Type.GetType("Novels.StoryDownloadOverlay, Novels")
                ?? throw new InvalidOperationException(
                    "Novels.StoryDownloadOverlay type is unavailable.");
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root = new GameObject(
                "Story Download Fallback",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(CanvasGroup));
            try
            {
                var canvas = root.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                var scaler = root.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(465f, 1024f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

                var blurredFrame = CreateRawImage(
                    "Blurred Frame",
                    root.transform,
                    Color.white,
                    Stretch());
                blurredFrame.raycastTarget = false;
                var shade = CreateImage(
                    "Shade",
                    root.transform,
                    new Color(0f, 0f, 0f, 0.48f),
                    Stretch());
                shade.raycastTarget = true;

                var panel = CreateImage(
                    "Panel",
                    root.transform,
                    new Color(0.055f, 0.07f, 0.1f, 0.96f),
                    Centered(new Vector2(390f, 196f)));
                panel.raycastTarget = false;
                var title = CreateText(
                    "Title",
                    panel.transform,
                    "Загружаем продолжение",
                    font,
                    24,
                    FontStyle.Bold,
                    Color.white,
                    Anchored(22f, -20f, -22f, 54f));
                title.alignment = TextAnchor.MiddleCenter;

                var track = CreateImage(
                    "Progress Track",
                    panel.transform,
                    new Color(1f, 1f, 1f, 0.16f),
                    Anchored(22f, -78f, -22f, 92f));
                track.raycastTarget = false;
                var fill = CreateImage(
                    "Progress Fill",
                    track.transform,
                    new Color(0.18f, 0.63f, 1f, 1f),
                    Stretch());
                fill.raycastTarget = false;
                fill.rectTransform.anchorMax = new Vector2(0f, 1f);

                var details = CreateText(
                    "Details",
                    panel.transform,
                    "0%",
                    font,
                    18,
                    FontStyle.Normal,
                    new Color(0.88f, 0.91f, 0.96f),
                    Anchored(22f, -104f, -22f, 132f));
                var remaining = CreateText(
                    "Remaining",
                    panel.transform,
                    "Оцениваем оставшееся время…",
                    font,
                    18,
                    FontStyle.Normal,
                    new Color(0.88f, 0.91f, 0.96f),
                    Anchored(22f, -140f, -22f, 168f));
                details.alignment = TextAnchor.MiddleCenter;
                remaining.alignment = TextAnchor.MiddleCenter;

                var screen = root.AddComponent(screenType);
                root.AddComponent(overlayType);
                var serialized = new SerializedObject(screen);
                serialized.FindProperty("_canvasGroup").objectReferenceValue =
                    root.GetComponent<CanvasGroup>();
                serialized.FindProperty("_blurredFrame").objectReferenceValue =
                    blurredFrame;
                serialized.FindProperty("_progressFill").objectReferenceValue =
                    fill.rectTransform;
                serialized.FindProperty("_details").objectReferenceValue = details;
                serialized.FindProperty("_remaining").objectReferenceValue = remaining;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, _assetPath);
                AssetDatabase.SaveAssets();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static Image CreateImage(
            string name,
            Transform parent,
            Color color,
            RectLayout layout)
        {
            var gameObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            gameObject.transform.SetParent(parent, false);
            Apply(gameObject.GetComponent<RectTransform>(), layout);
            var image = gameObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static RawImage CreateRawImage(
            string name,
            Transform parent,
            Color color,
            RectLayout layout)
        {
            var gameObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(RawImage));
            gameObject.transform.SetParent(parent, false);
            Apply(gameObject.GetComponent<RectTransform>(), layout);
            var image = gameObject.GetComponent<RawImage>();
            image.color = color;
            return image;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            string value,
            Font font,
            int fontSize,
            FontStyle fontStyle,
            Color color,
            RectLayout layout)
        {
            var gameObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            gameObject.transform.SetParent(parent, false);
            Apply(gameObject.GetComponent<RectTransform>(), layout);
            var text = gameObject.GetComponent<Text>();
            text.text = value;
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static RectLayout Stretch() => new(
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            Vector2.zero);

        private static RectLayout Centered(Vector2 size) => new(
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            -size * 0.5f,
            size * 0.5f);

        private static RectLayout Anchored(
            float left,
            float top,
            float right,
            float bottom) => new(
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(left, -bottom),
            new Vector2(right, -top));

        private static void Apply(RectTransform rect, RectLayout layout)
        {
            rect.anchorMin = layout.AnchorMin;
            rect.anchorMax = layout.AnchorMax;
            rect.offsetMin = layout.OffsetMin;
            rect.offsetMax = layout.OffsetMax;
        }

        private readonly struct RectLayout
        {
            internal RectLayout(
                Vector2 anchorMin,
                Vector2 anchorMax,
                Vector2 offsetMin,
                Vector2 offsetMax)
            {
                AnchorMin = anchorMin;
                AnchorMax = anchorMax;
                OffsetMin = offsetMin;
                OffsetMax = offsetMax;
            }

            internal Vector2 AnchorMin { get; }
            internal Vector2 AnchorMax { get; }
            internal Vector2 OffsetMin { get; }
            internal Vector2 OffsetMax { get; }
        }
    }
}
