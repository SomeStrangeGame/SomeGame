using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Novels.ContentSdk.Editor
{
    internal sealed class StoryAssetOrderWindow : EditorWindow
    {
        private const string _mainMenuPath = "Novels/Content/Ink Tools";
        private const string _contextMenuPath = "Assets/Novels/Open Ink Tools";
        [SerializeField] private DefaultAsset _rootInk;
        private StoryAssetUsageEntry[] _entries = Array.Empty<StoryAssetUsageEntry>();
        private Vector2 _scroll;
        private string _filter = string.Empty;
        private string _lastResult = string.Empty;

        [MenuItem(_mainMenuPath)]
        [MenuItem(_contextMenuPath, false, 2000)]
        private static void Open()
        {
            var window = GetWindow<StoryAssetOrderWindow>("Ink Tools");
            window.minSize = new Vector2(720f, 360f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Инструменты Ink", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Перетащите корневой .ink. В этом окне можно скомпилировать "
                + "историю и затем рассчитать линейный порядок ассетов.",
                MessageType.Info);
            var selected = (DefaultAsset)EditorGUILayout.ObjectField(
                "Корневой Ink",
                _rootInk,
                typeof(DefaultAsset),
                false);
            if (selected != _rootInk)
            {
                _rootInk = selected;
                _entries = Array.Empty<StoryAssetUsageEntry>();
                _lastResult = string.Empty;
            }
            var sourcePath = RootInkPath();
            var compiledPath = string.IsNullOrEmpty(sourcePath)
                ? string.Empty
                : sourcePath + ".json";
            if (_rootInk != null && string.IsNullOrEmpty(sourcePath))
            {
                EditorGUILayout.HelpBox(
                    "Перетащите файл с расширением .ink, а не папку или другой asset.",
                    MessageType.Warning);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(sourcePath));
                if (GUILayout.Button("Скомпилировать", GUILayout.Width(150f)))
                    Compile(sourcePath);
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(
                    string.IsNullOrEmpty(compiledPath)
                    || !File.Exists(compiledPath));
                if (GUILayout.Button("Рассчитать ассеты", GUILayout.Width(160f)))
                    Generate(compiledPath);
                EditorGUI.EndDisabledGroup();
                _filter = EditorGUILayout.TextField("Фильтр", _filter);
            }
            if (!string.IsNullOrEmpty(_lastResult))
                EditorGUILayout.HelpBox(_lastResult, MessageType.None);

            if (_entries.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    string.IsNullOrEmpty(compiledPath)
                        ? "Выберите корневой Ink."
                        : !File.Exists(compiledPath)
                            ? "Compiled JSON ещё не создан. Нажмите «Скомпилировать»."
                            : "Нажмите «Рассчитать ассеты».",
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

        private void Generate(string compiledPath)
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
                    compiledPath,
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

        private void Compile(string sourcePath)
        {
            try
            {
                var result = StorySourceMapBuilder.CompileArtifacts(sourcePath);
                AssetDatabase.Refresh();
                _lastResult = "Созданы:\n"
                    + ProjectPath(result.CompiledPath) + "\n"
                    + ProjectPath(result.SourceMapPath) + "\n"
                    + $"Source map: {result.SourceMapEntryCount} записей.";
                Debug.Log(
                    $"Ink story compiled: '{result.CompiledPath}', source map "
                    + $"'{result.SourceMapPath}', "
                    + $"{result.SourceMapEntryCount} entries.");
                ShowNotification(new GUIContent("Ink успешно скомпилирован"));
            }
            catch (Exception exception)
            {
                _lastResult = "Компиляция не выполнена. Подробности в Console.";
                Debug.LogException(exception);
                ShowNotification(new GUIContent("Ошибка компиляции Ink"));
            }
            Repaint();
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

        private string RootInkPath()
        {
            if (_rootInk == null)
                return string.Empty;
            var assetPath = AssetDatabase.GetAssetPath(_rootInk);
            if (!assetPath.EndsWith(".ink", StringComparison.OrdinalIgnoreCase))
                return string.Empty;
            var absolutePath = Path.GetFullPath(assetPath);
            return File.Exists(absolutePath) ? absolutePath : string.Empty;
        }

        private static string ProjectPath(string absolutePath) =>
            "Assets" + absolutePath.Substring(Application.dataPath.Length)
                .Replace('\\', '/');
    }
}
