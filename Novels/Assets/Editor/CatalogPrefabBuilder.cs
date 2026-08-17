using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Editor
{
    internal static class CatalogPrefabBuilder
    {
        private const string _prefabPath =
            "Assets/RemoteAssets/Catalog/Screen.prefab";

        public static void BuildBatch()
        {
            Build();
            Debug.Log($"Catalog screen prefab generated: {_prefabPath}");
        }

        [MenuItem("Novels/Rebuild Catalog Screen")]
        private static void Build()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_prefabPath));

            var root = CreateUiObject(
                "Screen",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(Setting.View.Screen));
            try
            {
                var canvas = root.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 10;

                var scaler = root.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(465f, 1024f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

                var background = CreateUiObject("Background", typeof(Image));
                background.transform.SetParent(root.transform, false);
                Stretch(background.GetComponent<RectTransform>());
                background.GetComponent<Image>().color = new Color32(178, 178, 178, 255);

                var content = CreateUiObject(
                    "Content",
                    typeof(Image),
                    typeof(VerticalLayoutGroup));
                content.transform.SetParent(root.transform, false);
                var contentRect = content.GetComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0.5f, 0.5f);
                contentRect.anchorMax = new Vector2(0.5f, 0.5f);
                contentRect.sizeDelta = new Vector2(400f, 600f);
                content.GetComponent<Image>().color = new Color32(255, 255, 255, 235);
                var layout = content.GetComponent<VerticalLayoutGroup>();
                layout.padding = new RectOffset(32, 32, 32, 32);
                layout.spacing = 24f;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;

                var description = CreateText("Description", 32, FontStyle.Bold);
                description.transform.SetParent(content.transform, false);
                description.alignment = TextAnchor.MiddleCenter;
                description.color = new Color32(21, 21, 21, 255);
                description.gameObject.AddComponent<LayoutElement>().preferredHeight = 140f;

                var buttonObject = CreateUiObject(
                    "ButtonTemplate",
                    typeof(Image),
                    typeof(Button),
                    typeof(LayoutElement));
                buttonObject.transform.SetParent(content.transform, false);
                var buttonImage = buttonObject.GetComponent<Image>();
                buttonImage.color = new Color32(63, 94, 140, 255);
                var button = buttonObject.GetComponent<Button>();
                button.targetGraphic = buttonImage;
                buttonObject.GetComponent<LayoutElement>().preferredHeight = 96f;

                var label = CreateText("Label", 24, FontStyle.Normal);
                label.transform.SetParent(buttonObject.transform, false);
                Stretch(label.rectTransform);
                label.alignment = TextAnchor.MiddleCenter;
                label.color = Color.white;
                label.raycastTarget = false;

                var screen = new SerializedObject(root.GetComponent<Setting.View.Screen>());
                screen.FindProperty("_description").objectReferenceValue = description;
                screen.FindProperty("_buttonPrefab").objectReferenceValue = button;
                screen.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, _prefabPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateUiObject(
            string name,
            params System.Type[] components)
        {
            var allComponents = new System.Type[components.Length + 1];
            allComponents[0] = typeof(RectTransform);
            components.CopyTo(allComponents, 1);
            return new GameObject(name, allComponents);
        }

        private static Text CreateText(string name, int size, FontStyle style)
        {
            var text = CreateUiObject(name, typeof(CanvasRenderer), typeof(Text))
                .GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = style;
            text.supportRichText = true;
            return text;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
