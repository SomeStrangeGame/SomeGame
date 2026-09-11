using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Novels.Catalog.View
{
    public sealed class CatalogSettingsPopup : MonoBehaviour, ICancelHandler
    {
        [SerializeField] private Button _openButton;
        [SerializeField] private GameObject _popup;
        [SerializeField] private CanvasGroup _catalogContent;
        [SerializeField] private Button _backdrop;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _doneButton;
        [SerializeField] private Slider _volume;
        [SerializeField] private Text _volumeLabel;
        [SerializeField] private Text _version;
        [SerializeField] private Button _privacy;
        [SerializeField] private Button _terms;
        [SerializeField] private Button _support;
        [Header("Application links (empty means unavailable)")]
        [SerializeField] private string _privacyUrl;
        [SerializeField] private string _termsUrl;
        [SerializeField] private string _supportUrl;

        private ICatalogSettings _settings;
        private GameObject _previousSelection;
        private bool _contentInteractable;
        private bool _contentRaycasts;
        private Action _onSupportOpened;
        public bool IsOpen => _popup != null && _popup.activeSelf;

        private void Awake()
        {
            _popup.SetActive(false);
            _openButton.interactable = false;
            _openButton.onClick.AddListener(Open);
            _backdrop.onClick.AddListener(Close);
            _closeButton.onClick.AddListener(Close);
            _doneButton.onClick.AddListener(Close);
            _volume.onValueChanged.AddListener(ChangeVolume);
            BindLink(_privacy, _privacyUrl, null);
            BindLink(_terms, _termsUrl, null);
            BindLink(_support, _supportUrl, () => _onSupportOpened?.Invoke());
            _version.text = "Версия " + Application.version;
        }

        public void Configure(ICatalogSettings settings, Action onSupportOpened = null)
        {
            Close();
            _settings = settings;
            _onSupportOpened = onSupportOpened;
            _openButton.interactable = settings != null;
        }

        public void Open()
        {
            if (_settings == null || IsOpen) return;
            _contentInteractable = _catalogContent.interactable;
            _contentRaycasts = _catalogContent.blocksRaycasts;
            _catalogContent.interactable = false;
            _catalogContent.blocksRaycasts = false;
            // Stop inertia without changing the user's reading position.
            foreach (var scroll in _catalogContent.GetComponentsInChildren<ScrollRect>())
            {
                scroll.StopMovement();
                scroll.GetComponent<CatalogScrollSnap>()?.Cancel();
            }
            _previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            _openButton.interactable = false;
            _volume.SetValueWithoutNotify(_settings.Volume);
            UpdateVolumeLabel(_settings.Volume);
            _popup.SetActive(true);
            _volume.Select();
        }

        public void Close()
        {
            if (!IsOpen) return;
            _popup.SetActive(false);
            _catalogContent.interactable = _contentInteractable;
            _catalogContent.blocksRaycasts = _contentRaycasts;
            _openButton.interactable = _settings != null;
            _settings?.Save();
            if (isActiveAndEnabled && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(_previousSelection != null
                    && _previousSelection.activeInHierarchy ? _previousSelection : _openButton.gameObject);
        }

        private void ChangeVolume(float value)
        {
            if (_settings == null || !IsOpen) return;
            _settings.SetVolume(value);
            UpdateVolumeLabel(_settings.Volume);
        }

        private void UpdateVolumeLabel(float value) =>
            _volumeLabel.text = value <= 0 ? "Выключен" : Mathf.RoundToInt(value * 100) + "%";

        private static bool IsWebUrl(string value) =>
            Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;

        private static void BindLink(Button button, string url, Action onOpened)
        {
            button.interactable = IsWebUrl(url);
            var label = button.GetComponentInChildren<Text>();
            if (!button.interactable && label != null)
                label.color = new Color(.5f, .55f, .6f, 1f);
            if (button.interactable) button.onClick.AddListener(() =>
            {
                onOpened?.Invoke();
                Application.OpenURL(url);
            });
        }

        public void OnCancel(BaseEventData eventData) => Close();
        private void Update() { if (IsOpen && Input.GetKeyDown(KeyCode.Escape)) Close(); }
        private void OnDisable() => Close();
        private void OnApplicationPause(bool paused) { if (paused) _settings?.Save(); }

        private void OnDestroy()
        {
            _openButton.onClick.RemoveListener(Open);
            _backdrop.onClick.RemoveListener(Close);
            _closeButton.onClick.RemoveListener(Close);
            _doneButton.onClick.RemoveListener(Close);
            _volume.onValueChanged.RemoveListener(ChangeVolume);
        }
    }
}
