using System;
using System.Collections;
using UnityEngine;

namespace Novels
{
    internal sealed class StoryDownloadOverlay : MonoBehaviour
    {
        private const string _fallbackResource =
            "Fallbacks/StoryDownload/screen";
        private const float _minimumRate = 1024f;
        private const float _etaWarmup = 1.5f;
        private const float _stalledAfter = 3f;
        private const float _viewRefreshInterval = 0.2f;

        private readonly object _gate = new();
        private StoryDownloadScreen _screen;
        private RenderTexture _blurredFrame;
        private string _group = "-";
        private long _completedBytes;
        private long _totalBytes;
        private long _lastBytes;
        private float _shownAt;
        private float _lastSampleAt;
        private float _lastProgressAt;
        private float _nextViewRefresh;
        private float _bytesPerSecond;
        private bool _visible;

        internal void BindDownloadAll(Catalog.CatalogAction action)
        {
            _screen.BindDownloadAll(action);
        }

        internal static StoryDownloadOverlay Create()
        {
            var prefab = Resources.Load<GameObject>(_fallbackResource);
            if (prefab == null)
            {
                throw new InvalidOperationException(
                    $"Story download fallback prefab '{_fallbackResource}' is missing.");
            }
            var instance = Instantiate(prefab);
            instance.name = nameof(StoryDownloadOverlay);
            return instance.GetComponent<StoryDownloadOverlay>()
                ?? throw new InvalidOperationException(
                    "Story download fallback prefab has no overlay controller.");
        }

        private void Awake()
        {
            _screen = GetComponent<StoryDownloadScreen>()
                ?? throw new InvalidOperationException(
                    "Story download fallback prefab has no screen view.");
            _screen.SetVisible(false);
        }

        internal void Show(
            string group,
            Bundles.ContentDeliveryProgress? progress)
        {
            lock (_gate)
            {
                var changedGroup = !string.Equals(
                    _group,
                    group,
                    StringComparison.OrdinalIgnoreCase);
                if (!_visible || changedGroup)
                {
                    StopAllCoroutines();
                    _screen.SetVisible(false);
                    StartCoroutine(CaptureFrame());
                    _shownAt = Time.realtimeSinceStartup;
                    _lastSampleAt = _shownAt;
                    _lastProgressAt = _shownAt;
                    _lastBytes = 0;
                    _bytesPerSecond = 0f;
                }
                _visible = true;
                _group = string.IsNullOrWhiteSpace(group) ? "-" : group;
                if (progress.HasValue)
                    ApplyProgress(progress.Value);
                _nextViewRefresh = 0f;
            }
        }

        internal void Report(Bundles.ContentDeliveryProgress progress)
        {
            lock (_gate)
            {
                if (!_visible || !string.Equals(
                        _group,
                        progress.GroupId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                ApplyProgress(progress);
            }
        }

        internal void Hide()
        {
            lock (_gate)
            {
                _visible = false;
                StopAllCoroutines();
                _screen.SetVisible(false);
                _screen.SetFrame(null);
                ReleaseFrame();
            }
        }

        private void Update()
        {
            if (!_visible || Time.realtimeSinceStartup < _nextViewRefresh)
            {
                return;
            }
            _nextViewRefresh = Time.realtimeSinceStartup + _viewRefreshInterval;
            RefreshView();
        }

        private void ApplyProgress(Bundles.ContentDeliveryProgress progress)
        {
            var now = Time.realtimeSinceStartup;
            var elapsed = now - _lastSampleAt;
            if (progress.CompletedBytes > _lastBytes)
            {
                _lastProgressAt = now;
                if (elapsed >= 0.25f)
                {
                    var current = (progress.CompletedBytes - _lastBytes) / elapsed;
                    _bytesPerSecond = _bytesPerSecond <= 0f
                        ? current
                        : Mathf.Lerp(_bytesPerSecond, current, 0.25f);
                    _lastBytes = progress.CompletedBytes;
                    _lastSampleAt = now;
                }
            }
            _completedBytes = progress.CompletedBytes;
            _totalBytes = progress.TotalBytes;
        }

        private IEnumerator CaptureFrame()
        {
            yield return new WaitForEndOfFrame();
            if (!_visible)
                yield break;
            ReleaseFrame();
            var screenshot = new Texture2D(
                Screen.width,
                Screen.height,
                TextureFormat.RGB24,
                false);
            screenshot.ReadPixels(
                new Rect(0f, 0f, Screen.width, Screen.height),
                0,
                0,
                false);
            screenshot.Apply(false, false);
            var width = Mathf.Max(64, Screen.width / 4);
            var height = Mathf.Max(64, Screen.height / 4);
            _blurredFrame = new RenderTexture(width, height, 0)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "Story Download Blur",
            };
            _blurredFrame.Create();
            var intermediate = RenderTexture.GetTemporary(
                Mathf.Max(width, Screen.width / 2),
                Mathf.Max(height, Screen.height / 2),
                0,
                RenderTextureFormat.ARGB32);
            intermediate.filterMode = FilterMode.Bilinear;
            try
            {
                Graphics.Blit(screenshot, intermediate);
                Graphics.Blit(intermediate, _blurredFrame);
            }
            finally
            {
                RenderTexture.ReleaseTemporary(intermediate);
                Destroy(screenshot);
            }
            _screen.SetFrame(_blurredFrame);
            RefreshView();
            _screen.SetVisible(true);
        }

        private void RefreshView()
        {
            string group;
            long completed;
            long total;
            float speed;
            float shownAt;
            float lastProgressAt;
            lock (_gate)
            {
                group = _group;
                completed = _completedBytes;
                total = _totalBytes;
                speed = _bytesPerSecond;
                shownAt = _shownAt;
                lastProgressAt = _lastProgressAt;
            }
            var ratio = total <= 0
                ? 0f
                : Mathf.Clamp01((float)completed / total);
            var details = total > 0
                ? $"{ratio:P0} · {FormatBytes(completed)} из {FormatBytes(total)}"
                : group;
            _screen.SetProgress(
                ratio,
                details,
                RemainingText(
                    completed,
                    total,
                    speed,
                    Time.realtimeSinceStartup - shownAt,
                    Time.realtimeSinceStartup - lastProgressAt));
        }

        private void ReleaseFrame()
        {
            if (_blurredFrame == null)
                return;
            _blurredFrame.Release();
            Destroy(_blurredFrame);
            _blurredFrame = null;
        }

        private void OnDestroy()
        {
            ReleaseFrame();
        }

        private static string RemainingText(
            long completed,
            long total,
            float bytesPerSecond,
            float visibleDuration,
            float stalledDuration)
        {
            if (total > 0 && completed >= total)
                return "Подготавливаем продолжение…";
            if (stalledDuration >= _stalledAfter)
                return "Ожидаем соединение…";
            if (visibleDuration < _etaWarmup || bytesPerSecond < _minimumRate)
                return "Оцениваем оставшееся время…";
            var seconds = Math.Max(1d, (total - completed) / bytesPerSecond);
            if (seconds < 60d)
            {
                var rounded = Math.Max(5, (int)Math.Ceiling(seconds / 5d) * 5);
                return $"Осталось примерно {rounded} сек.";
            }
            var minutes = Math.Max(1, (int)Math.Ceiling(seconds / 60d));
            return $"Осталось примерно {minutes} мин.";
        }

        private static string FormatBytes(long value)
        {
            const float mebibyte = 1024f * 1024f;
            return value >= mebibyte
                ? $"{value / mebibyte:F1} МБ"
                : $"{value / 1024f:F0} КБ";
        }
    }
}
