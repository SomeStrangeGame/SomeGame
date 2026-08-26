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
        private const float _referenceWidth = 465f;
        private const float _referenceHeight = 1024f;
        private const float _minimumScale = 0.75f;
        private const float _maximumScale = 2f;
        private const float _refreshInterval = 0.25f;

        private StoryProcessor.StorySourceLocation _location;
        private GUIStyle _style;
        private GUIStyle _buttonStyle;
        private readonly GUIContent _content = new();
        private float _styleScale;
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
                    + StreamingExperimentDiagnostics.Snapshot() + "\n"
                    + Location.View.LocationScreen.GetPresentationDebugSnapshot();
                _content.text = _cachedText;
            }
            _style ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
                normal = {textColor = Color.white},
            };
            _buttonStyle ??= new GUIStyle(GUI.skin.button);

            var scale = Mathf.Clamp(
                Mathf.Min(
                    Screen.width / _referenceWidth,
                    Screen.height / _referenceHeight),
                _minimumScale,
                _maximumScale);
            if (!Mathf.Approximately(_styleScale, scale))
            {
                _styleScale = scale;
                _style.fontSize = Mathf.RoundToInt(_fontSize * scale);
                _style.padding = new RectOffset(
                    Mathf.RoundToInt(12f * scale),
                    Mathf.RoundToInt(12f * scale),
                    Mathf.RoundToInt(8f * scale),
                    Mathf.RoundToInt(8f * scale));
                _buttonStyle.fontSize = Mathf.RoundToInt(18f * scale);
            }

            var safeArea = Screen.safeArea;
            var margin = _margin * scale;
            var width = Mathf.Min(
                _maxWidth * scale,
                safeArea.width - margin * 2f);
            var rect = new Rect(
                safeArea.xMin + margin,
                Screen.height - safeArea.yMax + margin,
                width,
                _style.CalcHeight(_content, width));
            GUI.depth = int.MinValue;
            var previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.9f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previousColor;
            GUI.Label(
                rect,
                _cachedText,
                _style);
            var buttonY = rect.yMax + 4f * scale;
            if (GUI.Button(
                    new Rect(rect.x, buttonY, 132f * scale, 34f * scale),
                    "Cold App",
                    _buttonStyle))
                _coldRestart?.Invoke();
            if (GUI.Button(
                    new Rect(
                        rect.x + 138f * scale,
                        buttonY,
                        110f * scale,
                        34f * scale),
                    "Warm",
                    _buttonStyle))
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
