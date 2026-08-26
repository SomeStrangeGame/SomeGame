using System;
using System.Collections;
using UnityEngine;

namespace Novels
{
    internal sealed class StoryDownloadOverlay : MonoBehaviour
    {
        private const float _referenceWidth = 465f;
        private const float _referenceHeight = 1024f;
        private const float _minimumRate = 1024f;
        private const float _etaWarmup = 1.5f;
        private const float _stalledAfter = 3f;

        private readonly object _gate = new();
        private GUIStyle _titleStyle;
        private GUIStyle _detailsStyle;
        private RenderTexture _blurredFrame;
        private string _group = "-";
        private long _completedBytes;
        private long _totalBytes;
        private long _lastBytes;
        private float _shownAt;
        private float _lastSampleAt;
        private float _lastProgressAt;
        private float _bytesPerSecond;
        private bool _capturePending;
        private bool _visible;

        internal static StoryDownloadOverlay Create()
        {
            var root = new GameObject(nameof(StoryDownloadOverlay));
            return root.AddComponent<StoryDownloadOverlay>();
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
                    _capturePending = true;
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
                _capturePending = false;
                StopAllCoroutines();
                ReleaseFrame();
            }
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
            var width = Mathf.Max(32, Screen.width / 12);
            var height = Mathf.Max(32, Screen.height / 12);
            _blurredFrame = new RenderTexture(width, height, 0)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "Story Download Blur",
            };
            _blurredFrame.Create();
            Graphics.Blit(screenshot, _blurredFrame);
            Destroy(screenshot);
            _capturePending = false;
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

        private void OnGUI()
        {
            string group;
            long completed;
            long total;
            float speed;
            float shownAt;
            float lastProgressAt;
            lock (_gate)
            {
                if (!_visible || _capturePending)
                    return;
                group = _group;
                completed = _completedBytes;
                total = _totalBytes;
                speed = _bytesPerSecond;
                shownAt = _shownAt;
                lastProgressAt = _lastProgressAt;
            }

            GUI.depth = int.MinValue;
            var full = new Rect(0f, 0f, Screen.width, Screen.height);
            if (_blurredFrame != null)
                GUI.DrawTexture(full, _blurredFrame, ScaleMode.ScaleAndCrop);
            var previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.48f);
            GUI.DrawTexture(full, Texture2D.whiteTexture);
            GUI.color = previousColor;

            var scale = Mathf.Clamp(
                Mathf.Min(
                    Screen.width / _referenceWidth,
                    Screen.height / _referenceHeight),
                0.75f,
                1.7f);
            EnsureStyles(scale);
            var width = Mathf.Min(390f * scale, Screen.safeArea.width - 32f * scale);
            var height = 196f * scale;
            var box = new Rect(
                Screen.safeArea.center.x - width * 0.5f,
                Screen.height - Screen.safeArea.center.y - height * 0.5f,
                width,
                height);
            GUI.color = new Color(0.055f, 0.07f, 0.1f, 0.96f);
            GUI.DrawTexture(box, Texture2D.whiteTexture);
            GUI.color = previousColor;

            var padding = 22f * scale;
            GUI.Label(
                new Rect(box.x + padding, box.y + 20f * scale,
                    box.width - padding * 2f, 34f * scale),
                "Загружаем продолжение",
                _titleStyle);

            var ratio = total <= 0
                ? 0f
                : Mathf.Clamp01((float)completed / total);
            var bar = new Rect(
                box.x + padding,
                box.y + 78f * scale,
                box.width - padding * 2f,
                14f * scale);
            GUI.color = new Color(1f, 1f, 1f, 0.16f);
            GUI.DrawTexture(bar, Texture2D.whiteTexture);
            GUI.color = new Color(0.18f, 0.63f, 1f, 1f);
            GUI.DrawTexture(
                new Rect(bar.x, bar.y, bar.width * ratio, bar.height),
                Texture2D.whiteTexture);
            GUI.color = previousColor;

            var details = total > 0
                ? $"{ratio:P0} · {FormatBytes(completed)} из {FormatBytes(total)}"
                : group;
            GUI.Label(
                new Rect(box.x + padding, box.y + 104f * scale,
                    box.width - padding * 2f, 28f * scale),
                details,
                _detailsStyle);
            GUI.Label(
                new Rect(box.x + padding, box.y + 140f * scale,
                    box.width - padding * 2f, 28f * scale),
                RemainingText(
                    completed,
                    total,
                    speed,
                    Time.realtimeSinceStartup - shownAt,
                    Time.realtimeSinceStartup - lastProgressAt),
                _detailsStyle);
        }

        private void EnsureStyles(float scale)
        {
            _titleStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = {textColor = Color.white},
            };
            _detailsStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = {textColor = new Color(0.88f, 0.91f, 0.96f)},
            };
            _titleStyle.fontSize = Mathf.RoundToInt(24f * scale);
            _detailsStyle.fontSize = Mathf.RoundToInt(18f * scale);
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
