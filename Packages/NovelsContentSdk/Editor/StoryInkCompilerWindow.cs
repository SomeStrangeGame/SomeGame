using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Novels.ContentSdk.Editor
{
    internal sealed class StoryInkCompilerWindow : EditorWindow
    {
        private const string _mainMenuPath = "Novels/Content/Ink Compiler";
        private const string _contextMenuPath = "Assets/Novels/Open Ink Compiler";
        [SerializeField] private DefaultAsset _rootInk;
        private string _lastResult = string.Empty;

        [MenuItem(_mainMenuPath)]
        [MenuItem(_contextMenuPath, false, 2001)]
        private static void Open()
        {
            var window = GetWindow<StoryInkCompilerWindow>("Ink Compiler");
            window.minSize = new Vector2(620f, 190f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Компиляция истории Ink", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Перетащите корневой .ink в поле ниже. Он компилируется "
                + "официальным Ink Compiler. "
                + "Compiled JSON и source map обновляются рядом только после "
                + "успешного завершения.",
                MessageType.Info);
            var selected = (DefaultAsset)EditorGUILayout.ObjectField(
                "Корневой Ink",
                _rootInk,
                typeof(DefaultAsset),
                false);
            if (selected != _rootInk)
            {
                _rootInk = selected;
                _lastResult = string.Empty;
            }
            var sourcePath = RootInkPath();
            if (_rootInk != null && string.IsNullOrEmpty(sourcePath))
            {
                EditorGUILayout.HelpBox(
                    "Перетащите файл с расширением .ink, а не папку или другой asset.",
                    MessageType.Warning);
            }
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(sourcePath));
            if (GUILayout.Button("Скомпилировать", GUILayout.Height(30f)))
                CompileSelected(sourcePath);
            EditorGUI.EndDisabledGroup();
            if (!string.IsNullOrEmpty(_lastResult))
                EditorGUILayout.HelpBox(_lastResult, MessageType.None);
        }

        private void CompileSelected(string sourcePath)
        {
            try
            {
                var result = StorySourceMapBuilder.CompileArtifacts(
                    sourcePath);
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
