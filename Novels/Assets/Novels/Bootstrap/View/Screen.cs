using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Novels.Bootstrap.View
{
    public sealed class Screen : MonoBehaviour
    {
        private const string _resourcePath = "Novels/BootstrapScreen";
        [SerializeField] private Text _message;
        [SerializeField] private Text _retryLabel;
        [SerializeField] private Button _retry;

        public static Screen Create()
        {
            EnsureEventSystem();
            var prefab = Resources.Load<Screen>(_resourcePath);
            if (prefab != null)
                return Instantiate(prefab);
            return CreateGenerated();
        }

        public static Screen CreateGenerated()
        {
            var root = CreateObject(
                "BootstrapScreen",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(Screen));
            var canvas = root.GetComponent<Canvas>();
            root.transform.localScale = Vector3.one;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(465f, 1024f);

            var background = CreateObject("Background", typeof(Image));
            background.transform.SetParent(root.transform, false);
            Stretch(background.GetComponent<RectTransform>());
            background.GetComponent<Image>().color = new Color32(30, 35, 45, 255);

            var panel = CreateObject("Panel", typeof(VerticalLayoutGroup));
            panel.transform.SetParent(background.transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(390f, 260f);
            var layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 28f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;

            var message = CreateText("Message", 28);
            message.transform.SetParent(panel.transform, false);
            message.alignment = TextAnchor.MiddleCenter;
            message.color = Color.white;
            message.gameObject.AddComponent<LayoutElement>().preferredHeight = 120f;

            var retryObject = CreateObject(
                "Retry",
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));
            retryObject.transform.SetParent(panel.transform, false);
            retryObject.GetComponent<Image>().color = new Color32(63, 94, 140, 255);
            retryObject.GetComponent<LayoutElement>().preferredHeight = 76f;
            var retryLabel = CreateText("Label", 24);
            retryLabel.transform.SetParent(retryObject.transform, false);
            Stretch(retryLabel.rectTransform);
            retryLabel.alignment = TextAnchor.MiddleCenter;
            retryLabel.color = Color.white;
            retryLabel.raycastTarget = false;

            var screen = root.GetComponent<Screen>();
            screen._message = message;
            screen._retry = retryObject.GetComponent<Button>();
            screen._retryLabel = retryLabel;
            root.transform.localScale = Vector3.one;
            return screen;
        }

        public void ShowLoading(string message)
        {
            gameObject.SetActive(true);
            _message.text = message ?? string.Empty;
            _retry.gameObject.SetActive(false);
        }

        public void ShowRetry(string message, string retryLabel, Action onRetry)
        {
            gameObject.SetActive(true);
            _message.text = message ?? string.Empty;
            _retryLabel.text = retryLabel ?? string.Empty;
            _retry.onClick.RemoveAllListeners();
            _retry.onClick.AddListener(() => onRetry?.Invoke());
            _retry.gameObject.SetActive(true);
        }

        private static void EnsureEventSystem()
        {
            if (!Application.isPlaying || EventSystem.current != null)
                return;
            var eventSystem = new GameObject(
                "BootstrapEventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule));
            DontDestroyOnLoad(eventSystem);
        }

        private static GameObject CreateObject(
            string name,
            params Type[] components)
        {
            var types = new Type[components.Length + 1];
            types[0] = typeof(RectTransform);
            components.CopyTo(types, 1);
            return new GameObject(name, types);
        }

        private static Text CreateText(string name, int size)
        {
            var text = CreateObject(name, typeof(CanvasRenderer), typeof(Text))
                .GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
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
