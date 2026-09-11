using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Novels.Catalog.View
{
    public sealed class CatalogUpdatePopup : MonoBehaviour, ICancelHandler
    {
        [SerializeField] private CanvasGroup _catalogContent;

        private static bool _softShown;
        private GameObject _popup;
        private Button _laterButton;
        private Button _updateButton;
        private Text _message;
        private Text _updateLabel;
        private ICatalogUpdateAction _action;
        private string _storeUrl;
        private bool _hard;
        private bool _contentInteractable;
        private bool _contentRaycasts;

        public bool IsOpen => _popup != null && _popup.activeSelf;

        public void Configure(CatalogUpdatePrompt prompt)
        {
            if (!prompt.IsVisible || prompt.Mode == CatalogUpdateMode.Soft && _softShown)
                return;
            EnsureView();
            _storeUrl = prompt.StoreUrl;
            _action = prompt.Action;
            _hard = prompt.Mode == CatalogUpdateMode.Hard;
            var texts = _popup.GetComponentsInChildren<Text>(true);
            texts[0].text = _hard ? "Нужно обновить приложение" : "Доступно обновление";
            _message = texts[1];
            _message.text = _hard
                ? "Эта версия больше не поддерживается. Обновите приложение, чтобы продолжить."
                : "Рекомендуем обновить приложение, чтобы получить улучшения и новые истории.";
            _laterButton.gameObject.SetActive(!_hard);
            Open();
            if (!_hard)
                _softShown = true;
        }

        private void EnsureView()
        {
            if (_popup != null)
                return;
            _popup = CreateObject("Update Popup", transform, typeof(RectTransform));
            Stretch(_popup.GetComponent<RectTransform>());
            var backdrop = _popup.AddComponent<Image>();
            backdrop.color = new Color(0f, 0f, 0f, .78f);

            var panel = CreateObject("Panel", _popup.transform, typeof(RectTransform));
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(.08f, .5f);
            panelRect.anchorMax = new Vector2(.92f, .5f);
            panelRect.sizeDelta = new Vector2(0f, 310f);
            panelRect.anchoredPosition = Vector2.zero;
            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(.105f, .125f, .15f, 1f);

            CreateText(panel.transform, "Title", new Vector2(24f, -30f), new Vector2(-24f, 66f),
                28, FontStyle.Bold, TextAnchor.MiddleCenter);
            CreateText(panel.transform, "Message", new Vector2(28f, -108f), new Vector2(-28f, 116f),
                18, FontStyle.Normal, TextAnchor.UpperCenter);
            _updateButton = CreateButton(panel.transform, "Обновить", new Vector2(24f, 22f),
                new Vector2(-24f, 64f), StartUpdate, new Color(.82f, .62f, .18f, 1f));
            _updateLabel = _updateButton.GetComponentInChildren<Text>();
            _laterButton = CreateButton(panel.transform, "Позже", new Vector2(24f, 94f),
                new Vector2(-24f, 48f), Close, new Color(.18f, .22f, .27f, 1f));
            _popup.SetActive(false);
        }

        private void Open()
        {
            if (_catalogContent != null)
            {
                _contentInteractable = _catalogContent.interactable;
                _contentRaycasts = _catalogContent.blocksRaycasts;
                _catalogContent.interactable = false;
                _catalogContent.blocksRaycasts = false;
            }
            _popup.transform.SetAsLastSibling();
            _popup.SetActive(true);
        }

        private void Close()
        {
            if (_hard || !IsOpen)
                return;
            _popup.SetActive(false);
            if (_catalogContent != null)
            {
                _catalogContent.interactable = _contentInteractable;
                _catalogContent.blocksRaycasts = _contentRaycasts;
            }
        }

        private void StartUpdate()
        {
            if (_action != null)
            {
                if (_action.CanStart)
                    _action.Start();
                return;
            }
            if (!string.IsNullOrWhiteSpace(_storeUrl))
                Application.OpenURL(_storeUrl);
        }

        public void OnCancel(BaseEventData eventData) => Close();
        private void Update()
        {
            if (IsOpen && Input.GetKeyDown(KeyCode.Escape)) Close();
            if (!IsOpen || _action == null) return;
            _message.text = _action.StatusMessage;
            _updateButton.interactable = _action.CanStart;
            _updateLabel.text = _action.State switch
            {
                CatalogUpdateState.Downloading => $"Загрузка {Mathf.RoundToInt(_action.Progress * 100f)}%",
                CatalogUpdateState.Verifying => "Проверяем…",
                CatalogUpdateState.Installing => "Открываем установку…",
                CatalogUpdateState.PermissionRequired => "Открыть настройки",
                CatalogUpdateState.Failed => "Повторить",
                _ => "Обновить",
            };
        }

        private static GameObject CreateObject(string name, Transform parent, params System.Type[] components)
        {
            var value = new GameObject(name, components);
            value.layer = parent.gameObject.layer;
            value.transform.SetParent(parent, false);
            return value;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Text CreateText(Transform parent, string name, Vector2 topLeft,
            Vector2 size, int fontSize, FontStyle style, TextAnchor alignment)
        {
            var value = CreateObject(name, parent, typeof(RectTransform));
            var rect = value.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(.5f, 1f);
            rect.offsetMin = new Vector2(topLeft.x, -topLeft.y - size.y);
            rect.offsetMax = new Vector2(size.x, -topLeft.y);
            var text = value.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = Color.white;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 14;
            text.resizeTextMaxSize = fontSize;
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 bottomLeft,
            Vector2 size, UnityEngine.Events.UnityAction action, Color color)
        {
            var value = CreateObject(label, parent, typeof(RectTransform));
            var rect = value.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(.5f, 0f);
            rect.offsetMin = bottomLeft;
            rect.offsetMax = new Vector2(size.x, bottomLeft.y + size.y);
            var image = value.AddComponent<Image>();
            image.color = color;
            var button = value.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            var text = CreateText(value.transform, "Label", Vector2.zero, Vector2.zero,
                19, FontStyle.Bold, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            text.text = label;
            return button;
        }
    }
}
