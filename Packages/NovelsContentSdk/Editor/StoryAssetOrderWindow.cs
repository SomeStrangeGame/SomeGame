using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Novels.ContentSdk.Editor
{
    internal sealed class StoryAssetOrderWindow : EditorWindow
    {
        private const string _mainMenuPath = "Novels/Content/Story Asset Order";
        private const string _contextMenuPath = "Assets/Novels/Open Story Asset Order";
        private StoryAssetUsageEntry[] _entries = Array.Empty<StoryAssetUsageEntry>();
        private string[] _inkFiles = Array.Empty<string>();
        private Vector2 _scroll;
        private string _filter = string.Empty;
        private int _selectedInk;

        [MenuItem(_mainMenuPath)]
        [MenuItem(_contextMenuPath, false, 2000)]
        private static void Open()
        {
            var window = GetWindow<StoryAssetOrderWindow>("Story Asset Order");
            window.minSize = new Vector2(720f, 360f);
            window.Show();
        }

        private void OnEnable() => FindInkFiles();

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Линейный порядок ассетов", EditorStyles.boldLabel);
            DrawInkSelection();
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginDisabledGroup(_inkFiles.Length == 0);
                if (GUILayout.Button("Рассчитать", GUILayout.Width(110f)))
                    Generate();
                EditorGUI.EndDisabledGroup();
                _filter = EditorGUILayout.TextField("Фильтр", _filter);
            }

            if (_entries.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    _inkFiles.Length == 0
                        ? "В проекте не найдено ни одного .ink файла."
                        : "Выберите Ink-файл и нажмите «Рассчитать».",
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
                $"Ассетов: {_entries.Length} · найдено в Ink: {referenced} · "
                + $"исходный размер: {EditorUtility.FormatBytes(sourceBytes)}");

            DrawHeader();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var value in visible)
                DrawRow(value.index, value.entry);
            EditorGUILayout.EndScrollView();
        }

        private void DrawInkSelection()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (_inkFiles.Length == 1)
                {
                    EditorGUILayout.LabelField("Ink-файл", ProjectPath(_inkFiles[0]));
                }
                else if (_inkFiles.Length > 1)
                {
                    var labels = _inkFiles.Select(ProjectPath).ToArray();
                    var selected = EditorGUILayout.Popup(
                        "Ink-файл",
                        _selectedInk,
                        labels);
                    if (selected != _selectedInk)
                    {
                        _selectedInk = selected;
                        _entries = Array.Empty<StoryAssetUsageEntry>();
                    }
                }
                if (GUILayout.Button("Обновить", GUILayout.Width(90f)))
                    FindInkFiles();
            }
        }

        private void Generate()
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
                _entries = ExperimentalStreamingPlan.CreateLinearUsageReport(
                    _inkFiles[_selectedInk],
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
                GUILayout.Label("Тип", GUILayout.Width(60f));
                GUILayout.Label("Первое", GUILayout.Width(75f));
                GUILayout.Label("Размер", GUILayout.Width(80f));
                GUILayout.Label("Путь ассета");
            }
        }

        private static void DrawRow(int index, StoryAssetUsageEntry entry)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label((index + 1).ToString(), GUILayout.Width(45f));
                GUILayout.Label(entry.Kind.ToString(), GUILayout.Width(60f));
                GUILayout.Label(
                    entry.IsReferenced ? entry.FirstUse.ToString() : "Не найден",
                    GUILayout.Width(75f));
                GUILayout.Label(
                    EditorUtility.FormatBytes(entry.SourceBytes),
                    GUILayout.Width(80f));
                if (GUILayout.Button(
                        entry.Path,
                        EditorStyles.label))
                {
                    var assetPath = entry.Path.StartsWith(
                        "Assets/",
                        StringComparison.OrdinalIgnoreCase)
                        ? entry.Path
                        : "Assets/StreamingAssets/" + entry.Path;
                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                    if (asset != null)
                        EditorGUIUtility.PingObject(asset);
                }
            }
        }

        private void FindInkFiles()
        {
            var previous = _inkFiles.Length > 0 && _selectedInk < _inkFiles.Length
                ? _inkFiles[_selectedInk]
                : string.Empty;
            _inkFiles = Directory.Exists(Application.dataPath)
                ? Directory.EnumerateFiles(
                        Application.dataPath,
                        "*.ink",
                        SearchOption.AllDirectories)
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .ToArray()
                : Array.Empty<string>();
            var previousIndex = Array.IndexOf(_inkFiles, previous);
            _selectedInk = previousIndex >= 0 ? previousIndex : 0;
            _entries = Array.Empty<StoryAssetUsageEntry>();
            Repaint();
        }

        private static string ProjectPath(string absolutePath) =>
            "Assets" + absolutePath.Substring(Application.dataPath.Length)
                .Replace('\\', '/');
    }
}
