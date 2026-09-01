using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Novels.ContentSdk.Editor
{
    public static class OptionListFallbackPrefabBuilder
    {
        private const string PrefabPath =
            "Packages/somegame.novels-content-sdk/Runtime/Features/OptionSelection/Resources/OptionListScreen.prefab";

        private static readonly Color PanelColor = new(0.965f, 0.965f, 0.925f, 0.99f);
        private static readonly Color PrimaryBlue = new(0.02f, 0.28f, 0.65f, 1f);
        private static readonly Color BrightBlue = new(0.04f, 0.58f, 0.92f, 1f);
        private static readonly Color HeaderBlue = new(0.015f, 0.20f, 0.52f, 0.98f);
        private static readonly Color DarkText = new(0.035f, 0.12f, 0.30f, 1f);
        private static readonly Color ArrowColor = new(0.06f, 0.32f, 0.68f, 1f);

        [MenuItem("Novels/UI/Rebuild Option List Fallback")]
        public static void Build()
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var screenType = Type.GetType(
                    "Novels.OptionSelection.OptionListScreen, Novels.OptionSelection")
                    ?? throw new InvalidOperationException(
                        "Novels.OptionSelection.OptionListScreen type is unavailable.");
                var screen = root.GetComponent(screenType)
                    ?? throw new InvalidOperationException(
                        "OptionListScreen component is missing from fallback prefab.");
                var existing = root.transform.Find("WardrobeRoot");
                if (existing != null)
                    UnityEngine.Object.DestroyImmediate(existing.gameObject);

                var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                var wardrobeRoot = CreateRect("WardrobeRoot", root.transform, Stretch());

                var header = CreateImage(
                    "CharacterHeader",
                    wardrobeRoot.transform,
                    HeaderBlue,
                    Anchor(new Vector2(0.5f, 1f), new Vector2(540f, 110f), new Vector2(0f, -140f)));
                var headerText = CreateText(
                    "CharacterName",
                    header.transform,
                    "Персонаж",
                    font,
                    44,
                    Color.white,
                    Stretch(24f, 12f));

                var wardrobeMode = CreateButton(
                    "WardrobeMode",
                    wardrobeRoot.transform,
                    BrightBlue,
                    Anchor(new Vector2(1f, 1f), new Vector2(125f, 125f), new Vector2(-110f, -140f)),
                    "♛",
                    font,
                    52);

                var previousCharacter = CreateButton(
                    "PreviousCharacter",
                    wardrobeRoot.transform,
                    BrightBlue,
                    Anchor(new Vector2(0f, 0.5f), new Vector2(120f, 120f), new Vector2(130f, 40f)),
                    "≪",
                    font,
                    56);
                var nextCharacter = CreateButton(
                    "NextCharacter",
                    wardrobeRoot.transform,
                    BrightBlue,
                    Anchor(new Vector2(1f, 0.5f), new Vector2(120f, 120f), new Vector2(-130f, 40f)),
                    "≫",
                    font,
                    56);

                var panel = CreateImage(
                    "WardrobePanel",
                    wardrobeRoot.transform,
                    PanelColor,
                    Bottom(new Vector2(854f, 625f), Vector2.zero, new Vector2(0.5f, 0f)));

                var tabsRoot = CreateRect(
                    "Tabs",
                    panel.transform,
                    TopStretch(0f, 0f, 150f));
                var tabsLayout = tabsRoot.AddComponent<HorizontalLayoutGroup>();
                tabsLayout.spacing = 2f;
                tabsLayout.childAlignment = TextAnchor.MiddleCenter;
                tabsLayout.childControlWidth = true;
                tabsLayout.childControlHeight = true;
                tabsLayout.childForceExpandWidth = true;
                tabsLayout.childForceExpandHeight = true;
                var tabs = new Button[4];
                var tabLabels = new Text[4];
                var tabNames = new[] { "◉\nЛицо", "≋\nВолосы", "♙\nОдежда", "◇\nАксессуары" };
                for (var index = 0; index < tabs.Length; index++)
                {
                    tabs[index] = CreateButton(
                        $"Tab{index}",
                        tabsRoot.transform,
                        PrimaryBlue,
                        Stretch(),
                        tabNames[index],
                        font,
                        28);
                    tabLabels[index] = tabs[index].transform.Find("Label").GetComponent<Text>();
                    var layout = tabs[index].gameObject.AddComponent<LayoutElement>();
                    layout.flexibleWidth = 1f;
                }

                var title = CreateText(
                    "CategoryTitle",
                    panel.transform,
                    "Выберите вариант",
                    font,
                    24,
                    DarkText,
                    Anchor(new Vector2(0.5f, 1f), new Vector2(700f, 42f), new Vector2(0f, -185f)));
                title.gameObject.SetActive(false);
                var selection = CreateText(
                    "Selection",
                    panel.transform,
                    "Вариант",
                    font,
                    58,
                    Color.black,
                    Anchor(new Vector2(0.5f, 1f), new Vector2(560f, 100f), new Vector2(0f, -290f)));

                var previous = CreateButton(
                    "PreviousItem",
                    panel.transform,
                    Color.clear,
                    Anchor(new Vector2(0.5f, 1f), new Vector2(130f, 170f), new Vector2(-350f, -290f)),
                    "<",
                    font,
                    76,
                    ArrowColor);
                var next = CreateButton(
                    "NextItem",
                    panel.transform,
                    Color.clear,
                    Anchor(new Vector2(0.5f, 1f), new Vector2(130f, 170f), new Vector2(350f, -290f)),
                    ">",
                    font,
                    76,
                    ArrowColor);

                var confirm = CreateButton(
                    "Confirm",
                    panel.transform,
                    BrightBlue,
                    Bottom(new Vector2(380f, 130f), new Vector2(0f, 125f)),
                    "Готово",
                    font,
                    44);
                var confirmLabel = confirm.transform.Find("Label").GetComponent<Text>();
                var cancel = CreateButton(
                    "Cancel",
                    panel.transform,
                    PrimaryBlue,
                    Bottom(new Vector2(180f, 130f), new Vector2(250f, 125f)),
                    "×",
                    font,
                    62);
                cancel.gameObject.SetActive(false);

                var collapse = CreateButton(
                    "Collapse",
                    wardrobeRoot.transform,
                    Color.clear,
                    Bottom(new Vector2(140f, 80f), new Vector2(0f, 635f)),
                    "⌄",
                    font,
                    54,
                    ArrowColor);
                var collapseLabel = collapse.transform.Find("Label").GetComponent<Text>();

                var serialized = new SerializedObject(screen);
                Set(serialized, "_wardrobeRoot", wardrobeRoot);
                Set(serialized, "_wardrobePanel", panel.rectTransform);
                Set(serialized, "_wardrobeTitle", title);
                Set(serialized, "_wardrobeSelection", selection);
                Set(serialized, "_wardrobeConfirm", confirm);
                Set(serialized, "_wardrobeConfirmLabel", confirmLabel);
                Set(serialized, "_wardrobeHeader", headerText);
                SetArray(serialized, "_wardrobeTabs", tabs);
                SetArray(serialized, "_wardrobeTabLabels", tabLabels);
                Set(serialized, "_previous", previous);
                Set(serialized, "_next", next);
                Set(serialized, "_previousCharacter", previousCharacter);
                Set(serialized, "_nextCharacter", nextCharacter);
                Set(serialized, "_cancel", cancel);
                Set(serialized, "_collapse", collapse);
                Set(serialized, "_collapseRect", collapse.GetComponent<RectTransform>());
                Set(serialized, "_collapseLabel", collapseLabel);
                Set(serialized, "_wardrobeMode", wardrobeMode);
                serialized.ApplyModifiedPropertiesWithoutUndo();

                wardrobeRoot.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"Rebuilt authored option-list fallback prefab: {PrefabPath}");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static GameObject CreateRect(string name, Transform parent, RectLayout layout)
        {
            var value = new GameObject(name, typeof(RectTransform));
            value.transform.SetParent(parent, false);
            Apply(value.GetComponent<RectTransform>(), layout);
            return value;
        }

        private static Image CreateImage(
            string name,
            Transform parent,
            Color color,
            RectLayout layout)
        {
            var value = new GameObject(
                name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            value.transform.SetParent(parent, false);
            Apply(value.GetComponent<RectTransform>(), layout);
            var image = value.GetComponent<Image>();
            image.sprite = null;
            image.type = Image.Type.Simple;
            image.color = color;
            return image;
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            Color color,
            RectLayout layout,
            string label,
            Font font,
            int fontSize,
            Color? labelColor = null)
        {
            var image = CreateImage(name, parent, color, layout);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            CreateText(
                "Label",
                image.transform,
                label,
                font,
                fontSize,
                labelColor ?? Color.white,
                Stretch(8f, 6f));
            return button;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            string value,
            Font font,
            int fontSize,
            Color color,
            RectLayout layout)
        {
            var gameObject = new GameObject(
                name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            gameObject.transform.SetParent(parent, false);
            Apply(gameObject.GetComponent<RectTransform>(), layout);
            var text = gameObject.GetComponent<Text>();
            text.text = value;
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static void Set(SerializedObject serialized, string name, UnityEngine.Object value)
        {
            var property = serialized.FindProperty(name)
                ?? throw new InvalidOperationException($"Serialized property '{name}' is missing.");
            property.objectReferenceValue = value;
        }

        private static void SetArray<T>(SerializedObject serialized, string name, T[] values)
            where T : UnityEngine.Object
        {
            var property = serialized.FindProperty(name)
                ?? throw new InvalidOperationException($"Serialized property '{name}' is missing.");
            property.arraySize = values.Length;
            for (var index = 0; index < values.Length; index++)
                property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
        }

        private static RectLayout Stretch(float horizontal = 0f, float vertical = 0f) => new(
            Vector2.zero,
            Vector2.one,
            new Vector2(horizontal, vertical),
            new Vector2(-horizontal, -vertical),
            Vector2.zero,
            new Vector2(0.5f, 0.5f));

        private static RectLayout Anchor(Vector2 anchor, Vector2 size, Vector2 position) => new(
            anchor,
            anchor,
            Vector2.zero,
            size,
            position,
            new Vector2(0.5f, 0.5f));

        private static RectLayout Bottom(Vector2 size, Vector2 position, Vector2? pivot = null) => new(
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            Vector2.zero,
            size,
            position,
            pivot ?? new Vector2(0.5f, 0.5f));

        private static RectLayout TopStretch(float left, float right, float height) => new(
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(left, -height),
            new Vector2(-right, 0f),
            Vector2.zero,
            new Vector2(0.5f, 0.5f));

        private static void Apply(RectTransform rect, RectLayout layout)
        {
            rect.anchorMin = layout.AnchorMin;
            rect.anchorMax = layout.AnchorMax;
            rect.pivot = layout.Pivot;
            rect.anchoredPosition = layout.Position;
            if (layout.AnchorMin == layout.AnchorMax)
                rect.sizeDelta = layout.Size;
            else
            {
                rect.offsetMin = layout.OffsetMin;
                rect.offsetMax = layout.OffsetMax;
            }
        }

        private readonly struct RectLayout
        {
            internal RectLayout(
                Vector2 anchorMin,
                Vector2 anchorMax,
                Vector2 offsetMin,
                Vector2 sizeOrOffsetMax,
                Vector2 position,
                Vector2 pivot)
            {
                AnchorMin = anchorMin;
                AnchorMax = anchorMax;
                OffsetMin = offsetMin;
                OffsetMax = sizeOrOffsetMax;
                Size = sizeOrOffsetMax;
                Position = position;
                Pivot = pivot;
            }

            internal Vector2 AnchorMin { get; }
            internal Vector2 AnchorMax { get; }
            internal Vector2 OffsetMin { get; }
            internal Vector2 OffsetMax { get; }
            internal Vector2 Size { get; }
            internal Vector2 Position { get; }
            internal Vector2 Pivot { get; }
        }
    }
}
