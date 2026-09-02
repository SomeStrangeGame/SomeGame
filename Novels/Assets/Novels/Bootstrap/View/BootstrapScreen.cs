using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Novels.Bootstrap.View
{
    public sealed class BootstrapScreen : MonoBehaviour
    {
        private const string _resourcePath = "Novels/BootstrapScreen";
        [SerializeField] private Text _message;
        [SerializeField] private Text _retryLabel;
        [SerializeField] private Button _retry;

        public static BootstrapScreen Create()
        {
            EnsureEventSystem();
            var prefab = Resources.Load<BootstrapScreen>(_resourcePath);
            if (prefab == null)
                throw new InvalidOperationException(
                    $"Required bootstrap prefab is missing from Resources/{_resourcePath}.prefab.");
            return Instantiate(prefab);
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
    }
}
