using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Editor
{
    internal static class CatalogPrefabBuilder
    {
        private const string _prefabPath =
            Novels.Catalog.CatalogAddresses.ScreenAssetName;

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
                typeof(Novels.Catalog.View.Screen));
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

                var title = CreateText("Title", 32, FontStyle.Bold);
                title.transform.SetParent(content.transform, false);
                title.alignment = TextAnchor.MiddleCenter;
                title.color = new Color32(21, 21, 21, 255);
                title.gameObject.AddComponent<LayoutElement>().preferredHeight = 100f;

                var cardObject = CreateUiObject(
                    "CardTemplate",
                    typeof(Image),
                    typeof(Button),
                    typeof(LayoutElement),
                    typeof(VerticalLayoutGroup),
                    typeof(Novels.Catalog.View.Card));
                cardObject.transform.SetParent(content.transform, false);
                var buttonImage = cardObject.GetComponent<Image>();
                buttonImage.color = new Color32(63, 94, 140, 255);
                var button = cardObject.GetComponent<Button>();
                button.targetGraphic = buttonImage;
                cardObject.GetComponent<LayoutElement>().preferredHeight = 120f;
                var cardLayout = cardObject.GetComponent<VerticalLayoutGroup>();
                cardLayout.padding = new RectOffset(20, 20, 12, 12);
                cardLayout.childAlignment = TextAnchor.MiddleCenter;
                cardLayout.childControlHeight = true;
                cardLayout.childControlWidth = true;
                cardLayout.childForceExpandHeight = false;

                var cardTitle = CreateText("Title", 24, FontStyle.Bold);
                cardTitle.transform.SetParent(cardObject.transform, false);
                cardTitle.alignment = TextAnchor.MiddleCenter;
                cardTitle.color = Color.white;
                cardTitle.raycastTarget = false;
                var cardDescription = CreateText("Description", 18, FontStyle.Normal);
                cardDescription.transform.SetParent(cardObject.transform, false);
                cardDescription.alignment = TextAnchor.MiddleCenter;
                cardDescription.color = Color.white;
                cardDescription.raycastTarget = false;

                var card = new SerializedObject(
                    cardObject.GetComponent<Novels.Catalog.View.Card>());
                card.FindProperty("_title").objectReferenceValue = cardTitle;
                card.FindProperty("_description").objectReferenceValue = cardDescription;
                card.FindProperty("_button").objectReferenceValue = button;
                card.ApplyModifiedPropertiesWithoutUndo();

                var screen = new SerializedObject(
                    root.GetComponent<Novels.Catalog.View.Screen>());
                screen.FindProperty("_title").objectReferenceValue = title;
                screen.FindProperty("_cardPrefab").objectReferenceValue =
                    cardObject.GetComponent<Novels.Catalog.View.Card>();
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
