using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Novels.ContentSdk.Editor
{
    internal sealed class StoryAssetOrderWindow : EditorWindow
    {
        private const string _menuPath = "Assets/Novels/Show Linear Asset Order";
        private StoryAssetUsageEntry[] _entries = Array.Empty<StoryAssetUsageEntry>();
        private Vector2 _scroll;
        private string _storyId = string.Empty;
        private string _filter = string.Empty;

        [MenuItem(_menuPath, false, 2000)]
        private static void Open()
        {
            if (!TrySelectedStoryId(out var storyId))
                return;
            var window = GetWindow<StoryAssetOrderWindow>("Story Asset Order");
            window.minSize = new Vector2(720f, 360f);
            window.Generate(storyId);
            window.Show();
        }

        [MenuItem(_menuPath, true)]
        private static bool ValidateOpen() => TrySelectedStoryId(out _);

        private void OnGUI()
        {
            EditorGUILayout.LabelField(
                string.IsNullOrEmpty(_storyId)
                    ? "Select an Ink file or its story folder in Project."
                    : $"Linear first-use order: {_storyId}",
                EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_storyId));
                if (GUILayout.Button("Generate", GUILayout.Width(100f)))
                    Generate(_storyId);
                EditorGUI.EndDisabledGroup();
                _filter = EditorGUILayout.TextField("Filter", _filter);
            }

            if (_entries.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "The report contains no assets.",
                    MessageType.Info);
                return;
            }

            var visible = _entries
                .Select((entry, index) => (entry, index))
                .Where(value => string.IsNullOrWhiteSpace(_filter)
                    || value.entry.Path.IndexOf(
                        _filter,
                        StringComparison.OrdinalIgnoreCase) >= 0)
                .ToArray();
            var referenced = _entries.Count(value => value.IsReferenced);
            var sourceBytes = _entries.Sum(value => value.SourceBytes);
            EditorGUILayout.LabelField(
                $"{_entries.Length} assets · {referenced} referenced · "
                + $"{EditorUtility.FormatBytes(sourceBytes)} source size");

            DrawHeader();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var value in visible)
                DrawRow(value.index, value.entry);
            EditorGUILayout.EndScrollView();
        }

        private void Generate(string storyId)
        {
            try
            {
                var files = Directory
                    .EnumerateFiles(
                        Application.streamingAssetsPath,
                        "*",
                        SearchOption.AllDirectories)
                    .Where(path => !path.EndsWith(
                        ".meta",
                        StringComparison.OrdinalIgnoreCase))
                    .Select(path => Path.GetRelativePath(
                            Application.streamingAssetsPath,
                            path)
                        .Replace('\\', '/'))
                    .ToArray();
                _storyId = storyId;
                _entries = ExperimentalStreamingPlan.CreateLinearUsageReport(
                    storyId,
                    ContentAssets.FindBundleAssets(),
                    files);
                _scroll = Vector2.zero;
                Repaint();
            }
            catch (Exception exception)
            {
                _entries = Array.Empty<StoryAssetUsageEntry>();
                Debug.LogException(exception);
                ShowNotification(new GUIContent("Asset report failed. See Console."));
            }
        }

        private static void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("#", GUILayout.Width(45f));
                GUILayout.Label("Type", GUILayout.Width(60f));
                GUILayout.Label("First use", GUILayout.Width(75f));
                GUILayout.Label("Size", GUILayout.Width(80f));
                GUILayout.Label("Asset path");
            }
        }

        private static void DrawRow(int index, StoryAssetUsageEntry entry)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label((index + 1).ToString(), GUILayout.Width(45f));
                GUILayout.Label(entry.Kind.ToString(), GUILayout.Width(60f));
                GUILayout.Label(
                    entry.IsReferenced ? entry.FirstUse.ToString() : "Not found",
                    GUILayout.Width(75f));
                GUILayout.Label(
                    EditorUtility.FormatBytes(entry.SourceBytes),
                    GUILayout.Width(80f));
                if (GUILayout.Button(
                        entry.Path,
                        EditorStyles.label))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(
                        entry.Path);
                    if (asset != null)
                        EditorGUIUtility.PingObject(asset);
                }
            }
        }

        private static bool TrySelectedStoryId(out string storyId)
        {
            storyId = string.Empty;
            var path = AssetDatabase.GetAssetPath(Selection.activeObject)
                .Replace('\\', '/');
            const string marker = "/noveltexts/";
            var markerIndex = path.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex < 0)
                return false;
            var remainder = path.Substring(markerIndex + marker.Length);
            var separator = remainder.IndexOf('/');
            storyId = separator >= 0
                ? remainder.Substring(0, separator)
                : Path.GetFileNameWithoutExtension(remainder);
            return !string.IsNullOrWhiteSpace(storyId);
        }
    }
}
