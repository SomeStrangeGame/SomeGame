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

        [MenuItem("Novels/UI/Rebuild Catalog Screen")]
        private static void Build()
        {
            var root = CreateUiObject(
                "Screen",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(Novels.Catalog.View.CatalogScreen));
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

                var viewport = CreateUiObject(
                    "Viewport",
                    typeof(Image),
                    typeof(RectMask2D),
                    typeof(ScrollRect),
                    typeof(LayoutElement));
                viewport.transform.SetParent(content.transform, false);
                viewport.GetComponent<Image>().color = Color.clear;
                viewport.GetComponent<LayoutElement>().preferredHeight = 420f;

                var cardList = CreateUiObject(
                    "Cards",
                    typeof(VerticalLayoutGroup),
                    typeof(ContentSizeFitter));
                cardList.transform.SetParent(viewport.transform, false);
                var cardListRect = cardList.GetComponent<RectTransform>();
                cardListRect.anchorMin = new Vector2(0f, 1f);
                cardListRect.anchorMax = new Vector2(1f, 1f);
                cardListRect.pivot = new Vector2(0.5f, 1f);
                cardListRect.offsetMin = Vector2.zero;
                cardListRect.offsetMax = Vector2.zero;
                var cardListLayout = cardList.GetComponent<VerticalLayoutGroup>();
                cardListLayout.spacing = 16f;
                cardListLayout.childControlWidth = true;
                cardListLayout.childControlHeight = true;
                cardListLayout.childForceExpandWidth = true;
                cardListLayout.childForceExpandHeight = false;
                cardList.GetComponent<ContentSizeFitter>().verticalFit =
                    ContentSizeFitter.FitMode.PreferredSize;
                var scroll = viewport.GetComponent<ScrollRect>();
                scroll.viewport = viewport.GetComponent<RectTransform>();
                scroll.content = cardListRect;
                scroll.horizontal = false;
                scroll.vertical = true;
                scroll.movementType = ScrollRect.MovementType.Clamped;

                var cardObject = CreateUiObject(
                    "CardTemplate",
                    typeof(Image),
                    typeof(Button),
                    typeof(LayoutElement),
                    typeof(VerticalLayoutGroup),
                    typeof(Outline),
                    typeof(Novels.Catalog.View.Card));
                cardObject.transform.SetParent(cardList.transform, false);
                var buttonImage = cardObject.GetComponent<Image>();
                buttonImage.color = new Color32(35, 58, 92, 255);
                var outline = cardObject.GetComponent<Outline>();
                outline.effectColor = new Color32(205, 220, 242, 255);
                outline.effectDistance = new Vector2(3f, -3f);
                var button = cardObject.GetComponent<Button>();
                button.targetGraphic = buttonImage;
                cardObject.GetComponent<LayoutElement>().preferredHeight = 150f;
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
                var cardStatus = CreateText("Status", 16, FontStyle.Italic);
                cardStatus.transform.SetParent(cardObject.transform, false);
                cardStatus.alignment = TextAnchor.MiddleCenter;
                cardStatus.color = new Color32(210, 230, 255, 255);
                cardStatus.raycastTarget = false;

                var card = new SerializedObject(
                    cardObject.GetComponent<Novels.Catalog.View.Card>());
                card.FindProperty("_title").objectReferenceValue = cardTitle;
                card.FindProperty("_description").objectReferenceValue = cardDescription;
                card.FindProperty("_status").objectReferenceValue = cardStatus;
                card.FindProperty("_button").objectReferenceValue = button;
                card.ApplyModifiedPropertiesWithoutUndo();

                var screen = new SerializedObject(
                    root.GetComponent<Novels.Catalog.View.CatalogScreen>());
                screen.FindProperty("_title").objectReferenceValue = title;
                screen.FindProperty("_cardPrefab").objectReferenceValue =
                    cardObject.GetComponent<Novels.Catalog.View.Card>();
                screen.ApplyModifiedPropertiesWithoutUndo();

                GeneratedPrefabWriter.Save(root, _prefabPath);
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
