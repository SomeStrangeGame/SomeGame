using System.IO;
using UnityEngine;

namespace Novels
{
    internal sealed class StorySourceOverlay : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const float _margin = 24f;
        private const float _maxWidth = 900f;
        private const float _verticalPadding = 24f;
        private const int _minimumFontSize = 28;
        private const int _maximumFontSize = 48;

        private StoryProcessor.StorySourceLocation _location;
        private GUIStyle _style;

        internal void Show(StoryProcessor.StorySourceLocation location)
        {
            _location = location;
        }

        private void OnGUI()
        {
            if (!_location.IsValid)
                return;
            var fontSize = Mathf.Clamp(
                Mathf.RoundToInt(Screen.height * 0.035f),
                _minimumFontSize,
                _maximumFontSize);
            _style ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(18, 18, 0, 0),
                normal = {textColor = new Color(1f, 0.9f, 0.15f)},
            };
            _style.fontSize = fontSize;

            var safeArea = Screen.safeArea;
            var rect = new Rect(
                safeArea.xMin + _margin,
                Screen.height - safeArea.yMax + _margin,
                Mathf.Min(_maxWidth, safeArea.width - _margin * 2f),
                fontSize + _verticalPadding);
            GUI.depth = int.MinValue;
            var previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.9f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previousColor;
            GUI.Label(
                rect,
                $"Ink: {Path.GetFileName(_location.FileName)}:{_location.LineNumber}",
                _style);
        }
#else
        internal void Show(StoryProcessor.StorySourceLocation location)
        {
        }
#endif
    }
}
