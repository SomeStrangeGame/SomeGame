using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Novels.ContentSdk.Editor
{
    internal sealed class StoryInkCompilerWindow : EditorWindow
    {
        private const string _mainMenuPath = "Novels/Content/Ink Compiler";
        private const string _contextMenuPath = "Assets/Novels/Open Ink Compiler";
        private string[] _rootInkFiles = Array.Empty<string>();
        private int _selectedRoot;
        private string _lastResult = string.Empty;

        [MenuItem(_mainMenuPath)]
        [MenuItem(_contextMenuPath, false, 2001)]
        private static void Open()
        {
            var window = GetWindow<StoryInkCompilerWindow>("Ink Compiler");
            window.minSize = new Vector2(620f, 190f);
            window.Show();
        }

        private void OnEnable() => FindRootInkFiles();

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Компиляция истории Ink", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Корневой .ink компилируется официальным Ink Compiler. "
                + "Compiled JSON и source map обновляются рядом только после "
                + "успешного завершения.",
                MessageType.Info);
            DrawRootSelection();
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginDisabledGroup(_rootInkFiles.Length == 0);
                if (GUILayout.Button("Скомпилировать", GUILayout.Height(30f)))
                    CompileSelected();
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button(
                        "Обновить список",
                        GUILayout.Width(130f),
                        GUILayout.Height(30f)))
                {
                    FindRootInkFiles();
                }
            }
            if (!string.IsNullOrEmpty(_lastResult))
                EditorGUILayout.HelpBox(_lastResult, MessageType.None);
        }

        private void DrawRootSelection()
        {
            if (_rootInkFiles.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "Корневой .ink не найден. Ожидается файл с INCLUDE или "
                    + "существующим соседним .ink.json.",
                    MessageType.Warning);
                return;
            }
            if (_rootInkFiles.Length == 1)
            {
                EditorGUILayout.LabelField(
                    "Корневой Ink",
                    ProjectPath(_rootInkFiles[0]));
                return;
            }
            var selected = EditorGUILayout.Popup(
                "Корневой Ink",
                _selectedRoot,
                _rootInkFiles.Select(ProjectPath).ToArray());
            if (selected != _selectedRoot)
            {
                _selectedRoot = selected;
                _lastResult = string.Empty;
            }
        }

        private void CompileSelected()
        {
            try
            {
                var result = StorySourceMapBuilder.CompileArtifacts(
                    _rootInkFiles[_selectedRoot]);
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

        private void FindRootInkFiles()
        {
            var previous = _rootInkFiles.Length > 0
                && _selectedRoot < _rootInkFiles.Length
                ? _rootInkFiles[_selectedRoot]
                : string.Empty;
            _rootInkFiles = Directory.Exists(Application.dataPath)
                ? Directory.EnumerateFiles(
                        Application.dataPath,
                        "*.ink",
                        SearchOption.AllDirectories)
                    .Where(IsRootInk)
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .ToArray()
                : Array.Empty<string>();
            var previousIndex = Array.IndexOf(_rootInkFiles, previous);
            _selectedRoot = previousIndex >= 0 ? previousIndex : 0;
            _lastResult = string.Empty;
            Repaint();
        }

        private static bool IsRootInk(string path)
        {
            if (File.Exists(path + ".json"))
                return true;
            return File.ReadLines(path).Any(line => line
                .TrimStart()
                .StartsWith("INCLUDE ", StringComparison.OrdinalIgnoreCase));
        }

        private static string ProjectPath(string absolutePath) =>
            "Assets" + absolutePath.Substring(Application.dataPath.Length)
                .Replace('\\', '/');
    }
}
