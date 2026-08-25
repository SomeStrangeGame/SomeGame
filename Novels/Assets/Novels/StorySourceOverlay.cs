using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

namespace Novels
{
    internal sealed class StorySourceOverlay : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const float _margin = 24f;
        private const float _maxWidth = 620f;
        private const int _fontSize = 22;
        private const float _refreshInterval = 0.25f;

        private StoryProcessor.StorySourceLocation _location;
        private GUIStyle _style;
        private GUIStyle _buttonStyle;
        private string _cachedText = "";
        private float _nextRefresh;
        private float _smoothedFrameMilliseconds;
        private System.Action _coldRestart;
        private System.Action _warmRestart;

        internal void Configure(
            System.Action coldRestart,
            System.Action warmRestart)
        {
            _coldRestart = coldRestart;
            _warmRestart = warmRestart;
        }

        internal void Show(StoryProcessor.StorySourceLocation location)
        {
            _location = location;
        }

        private void Update()
        {
            _smoothedFrameMilliseconds = Mathf.Lerp(
                _smoothedFrameMilliseconds,
                Time.unscaledDeltaTime * 1000f,
                0.05f);
        }

        private void OnGUI()
        {
            if (Time.realtimeSinceStartup >= _nextRefresh)
            {
                _nextRefresh = Time.realtimeSinceStartup + _refreshInterval;
                var source = _location.IsValid
                    ? $"Ink: {Path.GetFileName(_location.FileName)}:{_location.LineNumber}"
                    : "Ink: -";
                var memory = Profiler.GetTotalAllocatedMemoryLong()
                    / (1024f * 1024f);
                var fps = _smoothedFrameMilliseconds <= 0.01f
                    ? 0f
                    : 1000f / _smoothedFrameMilliseconds;
                _cachedText = $"{source}\n{fps:F0} FPS · "
                    + $"{_smoothedFrameMilliseconds:F1} ms · RAM {memory:F0} MiB\n"
                    + StreamingExperimentDiagnostics.Snapshot();
            }
            _style ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(12, 12, 8, 8),
                normal = {textColor = Color.white},
            };
            _style.fontSize = _fontSize;
            _buttonStyle ??= new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
            };

            var safeArea = Screen.safeArea;
            var rect = new Rect(
                safeArea.xMin + _margin,
                Screen.height - safeArea.yMax + _margin,
                Mathf.Min(_maxWidth, safeArea.width - _margin * 2f),
                96f);
            GUI.depth = int.MinValue;
            var previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.9f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previousColor;
            GUI.Label(
                rect,
                _cachedText,
                _style);
            var buttonY = rect.yMax + 4f;
            if (GUI.Button(new Rect(rect.x, buttonY, 132f, 34f), "Cold App", _buttonStyle))
                _coldRestart?.Invoke();
            if (GUI.Button(new Rect(rect.x + 138f, buttonY, 110f, 34f), "Warm", _buttonStyle))
                _warmRestart?.Invoke();
        }
#else
        internal void Show(StoryProcessor.StorySourceLocation location)
        {
        }

        internal void Configure(System.Action coldRestart, System.Action warmRestart)
        {
        }
#endif
    }
}
