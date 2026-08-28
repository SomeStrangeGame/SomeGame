using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Novels.ContentSdk.Editor
{
    [CustomEditor(typeof(Content.NovelContentAsset))]
    public sealed class NovelContentAssetEditor : UnityEditor.Editor
    {
        private const int _usageRowsPerPage = 40;
        private const int _chunkRowsPerPage = 30;
        private readonly HashSet<int> _expandedChunks = new();
        private readonly Dictionary<int, long> _chunkBytes = new();
        private readonly Dictionary<int, int> _chunkPages = new();
        private StoryAssetUsageEntry[] _usageEntries =
            Array.Empty<StoryAssetUsageEntry>();
        private StoryAssetUsageEntry[] _cachedUsageEntries;
        private readonly Dictionary<string, int> _usageChunksByPath = new(
            StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _usageUnusedPaths = new(
            StringComparer.OrdinalIgnoreCase);
        private int[] _visibleUsageIndices = Array.Empty<int>();
        private long[] _usageChunkBytes = Array.Empty<long>();
        private string _usageFilter = string.Empty;
        private string _cachedUsageFilter;
        private string _usageLayoutLabel = string.Empty;
        private int _cachedUsageChunkSizeMiB = -1;
        private int _usagePage;
        private bool _usageExpanded;
        private bool _chunksExpanded;
        private bool _unusedExpanded;
        private int _unusedPage;
        private HashSet<string> _videoPosterIds;
        private string _message = string.Empty;
        private MessageType _messageType = MessageType.None;

        private void OnEnable()
        {
            EditorApplication.projectChanged += InvalidateVideoPosterIds;
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= InvalidateVideoPosterIds;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_id"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_mainCharacter"));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("_contentVersion"),
                new GUIContent("Версия истории"));

            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Ink и доставка", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Здесь находится весь authoring истории: компиляция Ink, "
                + "эпизоды, порядок ассетов и ручная разметка чанков. "
                + "GUID-строки не создают Unity-зависимости между чанками.",
                MessageType.Info);

            DrawRootInk();
            DrawInkActions();
            var chunkSize = serializedObject.FindProperty(
                StoryChunkAuthoring.ChunkSizeProperty);
            if (chunkSize.intValue <= 0)
                chunkSize.intValue = 16;
            chunkSize.intValue = Math.Max(
                1,
                EditorGUILayout.IntField("Размер чанка, MiB", chunkSize.intValue));

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Рассчитать ассеты и чанки"))
                {
                    serializedObject.ApplyModifiedProperties();
                    GenerateChunks();
                    GUIUtility.ExitGUI();
                }
                if (GUILayout.Button("Проверить разметку"))
                {
                    serializedObject.ApplyModifiedProperties();
                    ValidateChunks();
                    serializedObject.Update();
                }
            }

            if (!string.IsNullOrEmpty(_message))
                EditorGUILayout.HelpBox(_message, _messageType);

            DrawUsageReport();
            DrawChunks();
            DrawUnusedAssets();

            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Данные истории", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("_endMarker"),
                new GUIContent("Маркер конца эпизода"));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("_silentAudioIds"),
                new GUIContent("ID тишины"),
                true);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("_episodes"),
                true);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("_videoAliases"),
                true);
            if (EditorGUI.EndChangeCheck())
                InvalidateVideoPosterIds();
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("_artAliases"),
                new GUIContent("Алиасы арта"),
                true);
            EditorGUILayout.HelpBox(
                "Пути задаются относительно content/<story-id>: "
                + "например story/choose/items/старое.png. "
                + "Alias может отсутствовать физически, target обязан существовать.",
                MessageType.Info);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("_characterDefaults"),
                true);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRootInk()
        {
            var property = serializedObject.FindProperty(
                StoryChunkAuthoring.RootInkGuidProperty);
            var currentPath = AssetDatabase.GUIDToAssetPath(property.stringValue);
            var current = string.IsNullOrEmpty(currentPath)
                ? null
                : AssetDatabase.LoadMainAssetAtPath(currentPath);
            EditorGUI.BeginChangeCheck();
            var selected = EditorGUILayout.ObjectField(
                "Корневой Ink",
                current,
                typeof(Object),
                false);
            if (!EditorGUI.EndChangeCheck())
                return;
            if (selected == null)
            {
                property.stringValue = string.Empty;
                ClearUsageReport();
                return;
            }
            var selectedPath = AssetDatabase.GetAssetPath(selected);
            if (!selectedPath.EndsWith(".ink", StringComparison.OrdinalIgnoreCase))
            {
                _message = "Корневым файлом можно назначить только .ink.";
                _messageType = MessageType.Error;
                return;
            }
            property.stringValue = AssetDatabase.AssetPathToGUID(selectedPath);
            ClearUsageReport();
        }

        private void DrawInkActions()
        {
            var definition = (Content.NovelContentAsset)target;
            var rootInkPath = StoryChunkAuthoring.RootInkPath(definition);
            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(rootInkPath)))
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Скомпилировать Ink"))
                {
                    serializedObject.ApplyModifiedProperties();
                    ExecuteAuthoringAction(() => StoryInkAuthoring.Compile(
                        definition,
                        rootInkPath));
                    serializedObject.Update();
                }
                if (GUILayout.Button("Обновить эпизоды"))
                {
                    serializedObject.ApplyModifiedProperties();
                    ExecuteAuthoringAction(() => StoryInkAuthoring.UpdateEpisodes(
                        definition,
                        rootInkPath));
                    serializedObject.Update();
                }
            }
        }

        private void ExecuteAuthoringAction(Func<string> action)
        {
            try
            {
                _message = action();
                _messageType = MessageType.Info;
            }
            catch (Exception exception)
            {
                _message = exception.Message;
                _messageType = MessageType.Error;
                Debug.LogException(exception);
            }
        }

        private void GenerateChunks()
        {
            try
            {
                var definition = (Content.NovelContentAsset)target;
                var existing = StoryChunkAuthoring.ChunkCount(definition);
                if (existing > 0
                    && !EditorUtility.DisplayDialog(
                        "Пересчитать чанки?",
                        $"Текущая ручная разметка из {existing} чанков будет заменена.",
                        "Пересчитать",
                        "Отмена"))
                {
                    return;
                }

                var rootInkPath = StoryChunkAuthoring.RootInkPath(definition);
                if (string.IsNullOrEmpty(rootInkPath))
                {
                    throw new InvalidOperationException(
                        "Назначьте корневой .ink или положите его по пути "
                        + "Assets/Ink/<story-id>.ink.");
                }
                var entries = StoryInkAuthoring.CreateUsageReport(
                    definition,
                    rootInkPath);
                var unusedPaths = StoryChunkAuthoring.UnusedPaths(definition);
                var chunkEntries = entries
                    .Where(entry => !unusedPaths.Contains(entry.Path))
                    .ToArray();
                var chunkSizeMiB = StoryChunkAuthoring.ChunkSizeMiB(definition);
                var layout = StoryStreamingPlan.CreateChunkLayout(
                    chunkEntries,
                    (long)chunkSizeMiB * 1024L * 1024L);
                StoryChunkAuthoring.WriteLayout(
                    definition,
                    rootInkPath,
                    chunkSizeMiB,
                    layout);
                _usageEntries = entries;
                _usageExpanded = true;
                InvalidateUsageTable();
                InvalidateChunkPresentation();
                var unknown = entries.Count(value => !value.IsReferenced);
                _message = $"Создано чанков: {layout.chunks.Length}. "
                    + $"Не используется: {unusedPaths.Count}. "
                    + $"Не включено неизвестных ассетов: {unknown}.";
                _messageType = MessageType.Info;
            }
            catch (Exception exception)
            {
                _message = exception.Message;
                _messageType = MessageType.Error;
                Debug.LogException(exception);
            }
        }

        private void ValidateChunks()
        {
            try
            {
                _message = StoryChunkAuthoring.Validate(
                    (Content.NovelContentAsset)target);
                _messageType = MessageType.Info;
            }
            catch (Exception exception)
            {
                _message = exception.Message;
                _messageType = MessageType.Error;
                Debug.LogException(exception);
            }
        }

        private void DrawUsageReport()
        {
            EditorGUILayout.Space(8f);
            _usageExpanded = EditorGUILayout.Foldout(
                _usageExpanded,
                $"Линейный список ассетов ({_usageEntries.Length})",
                true);
            if (!_usageExpanded)
                return;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Обновить список", GUILayout.Width(140f)))
                {
                    serializedObject.ApplyModifiedProperties();
                    RefreshUsageReport();
                    serializedObject.Update();
                }
                EditorGUI.BeginChangeCheck();
                var filter = EditorGUILayout.TextField("Фильтр", _usageFilter);
                if (EditorGUI.EndChangeCheck())
                {
                    _usageFilter = filter;
                    InvalidateUsageTable();
                }
            }

            if (_usageEntries.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "Нажмите «Обновить список» или «Рассчитать ассеты и чанки».",
                    MessageType.Info);
                return;
            }

            EnsureUsageTableCache();
            var referenced = _usageEntries.Count(value => value.IsReferenced);
            var dynamic = _usageEntries.Count(value =>
                value.Match == StoryAssetUsageMatch.Dynamic);
            var prefabs = _usageEntries.Count(value =>
                value.Kind == StoryAssetUsageKind.Prefab);
            var unknown = _usageEntries.Length - referenced;
            var sourceBytes = _usageEntries.Sum(value => value.SourceBytes);
            EditorGUILayout.LabelField(
                $"Найдено: {referenced}/{_usageEntries.Length} · "
                + $"динамических: {dynamic} · префабов: {prefabs} · "
                + $"неизвестных: {unknown}",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField(
                $"Исходный размер: {EditorUtility.FormatBytes(sourceBytes)} · "
                + _usageLayoutLabel,
                EditorStyles.wordWrappedMiniLabel);

            var pageCount = Math.Max(
                1,
                (_visibleUsageIndices.Length + _usageRowsPerPage - 1)
                / _usageRowsPerPage);
            _usagePage = Mathf.Clamp(_usagePage, 0, pageCount - 1);
            DrawUsagePagination(pageCount);
            var first = _usagePage * _usageRowsPerPage;
            var last = Math.Min(
                first + _usageRowsPerPage,
                _visibleUsageIndices.Length);
            var lastChunk = -1;
            var unusedShown = false;
            var unassignedShown = false;
            for (var visibleIndex = first; visibleIndex < last; visibleIndex++)
            {
                var index = _visibleUsageIndices[visibleIndex];
                var entry = _usageEntries[index];
                if (_usageChunksByPath.TryGetValue(entry.Path, out var chunk))
                {
                    if (lastChunk != chunk)
                    {
                        DrawUsageSeparator(
                            $"Чанк {chunk} · "
                            + EditorUtility.FormatBytes(_usageChunkBytes[chunk]));
                        lastChunk = chunk;
                    }
                }
                else if (_usageUnusedPaths.Contains(entry.Path))
                {
                    if (!unusedShown)
                    {
                        DrawUsageSeparator("Не используется");
                        unusedShown = true;
                    }
                }
                else if (!unassignedShown)
                {
                    DrawUsageSeparator("Не входят в чанки");
                    unassignedShown = true;
                }
                DrawUsageRow(index, entry);
            }
        }

        private void RefreshUsageReport()
        {
            try
            {
                var definition = (Content.NovelContentAsset)target;
                var rootInkPath = StoryChunkAuthoring.RootInkPath(definition);
                _usageEntries = StoryInkAuthoring.CreateUsageReport(
                    definition,
                    rootInkPath);
                InvalidateUsageTable();
                _message = $"Линейный список обновлён: {_usageEntries.Length} ассетов.";
                _messageType = MessageType.Info;
            }
            catch (Exception exception)
            {
                ClearUsageReport();
                _message = exception.Message;
                _messageType = MessageType.Error;
                Debug.LogException(exception);
            }
        }

        private void EnsureUsageTableCache()
        {
            var chunkSizeMiB = StoryChunkAuthoring.ChunkSizeMiB(
                (Content.NovelContentAsset)target);
            if (ReferenceEquals(_cachedUsageEntries, _usageEntries)
                && string.Equals(
                    _cachedUsageFilter,
                    _usageFilter,
                    StringComparison.Ordinal)
                && _cachedUsageChunkSizeMiB == chunkSizeMiB)
            {
                return;
            }

            _cachedUsageEntries = _usageEntries;
            _cachedUsageFilter = _usageFilter;
            _cachedUsageChunkSizeMiB = chunkSizeMiB;
            StoryChunkLayout layout;
            HashSet<string> unusedPaths;
            try
            {
                unusedPaths = StoryChunkAuthoring.UnusedPaths(
                    (Content.NovelContentAsset)target);
                if (StoryChunkAuthoring.TryReadLayout(
                        (Content.NovelContentAsset)target,
                        out layout))
                {
                    _usageLayoutLabel = "сохранённая разметка";
                }
                else
                {
                    layout = StoryStreamingPlan.CreateChunkLayout(
                        _usageEntries
                            .Where(entry => !unusedPaths.Contains(entry.Path))
                            .ToArray(),
                        (long)chunkSizeMiB * 1024L * 1024L);
                    _usageLayoutLabel = "предпросмотр до сохранения";
                }
            }
            catch (Exception exception)
            {
                layout = new StoryChunkLayout();
                unusedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _usageLayoutLabel = "разметка требует исправления";
                Debug.LogException(exception);
            }

            _usageChunksByPath.Clear();
            _usageUnusedPaths.Clear();
            _usageUnusedPaths.UnionWith(unusedPaths);
            foreach (var chunk in layout.chunks)
            {
                foreach (var path in chunk.assets ?? Array.Empty<string>())
                    _usageChunksByPath[path] = chunk.index;
            }
            _visibleUsageIndices = Enumerable.Range(0, _usageEntries.Length)
                .Where(index => string.IsNullOrWhiteSpace(_usageFilter)
                    || _usageEntries[index].Path.IndexOf(
                        _usageFilter,
                        StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(index => _usageChunksByPath.TryGetValue(
                        _usageEntries[index].Path,
                        out var chunk)
                    ? chunk
                    : _usageUnusedPaths.Contains(_usageEntries[index].Path)
                        ? int.MaxValue - 1
                    : int.MaxValue)
                .ThenBy(index => index)
                .ToArray();
            var sourceBytesByPath = _usageEntries.ToDictionary(
                entry => entry.Path,
                entry => entry.SourceBytes,
                StringComparer.OrdinalIgnoreCase);
            _usageChunkBytes = layout.chunks
                .Select(chunk => (chunk.assets ?? Array.Empty<string>())
                    .Sum(path => sourceBytesByPath.TryGetValue(path, out var bytes)
                        ? bytes
                        : 0L))
                .ToArray();
            _usagePage = 0;
        }

        private void DrawUsagePagination(int pageCount)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                using (new EditorGUI.DisabledScope(_usagePage <= 0))
                {
                    if (GUILayout.Button(
                            "◀",
                            EditorStyles.toolbarButton,
                            GUILayout.Width(28f)))
                    {
                        _usagePage--;
                    }
                }
                GUILayout.Label(
                    $"Страница {_usagePage + 1}/{pageCount} · "
                    + $"показано {_visibleUsageIndices.Length}",
                    EditorStyles.miniLabel);
                using (new EditorGUI.DisabledScope(_usagePage >= pageCount - 1))
                {
                    if (GUILayout.Button(
                            "▶",
                            EditorStyles.toolbarButton,
                            GUILayout.Width(28f)))
                    {
                        _usagePage++;
                    }
                }
            }
        }

        private static void DrawUsageSeparator(string title)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                GUILayout.Label(title, EditorStyles.boldLabel);
        }

        private static void DrawUsageRow(int index, StoryAssetUsageEntry entry)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(
                    $"{index + 1}. {entry.Kind} · {UsageMatchLabel(entry.Match)} · "
                    + (entry.IsReferenced
                        ? $"строка {entry.FirstUse}"
                        : "не найден в Ink")
                    + $" · {EditorUtility.FormatBytes(entry.SourceBytes)}",
                    EditorStyles.wordWrappedMiniLabel);
                if (GUILayout.Button(new GUIContent(entry.Path, "Показать ассет")))
                    PingUsageAsset(entry.Path);
            }
        }

        private static string UsageMatchLabel(StoryAssetUsageMatch match) => match switch
        {
            StoryAssetUsageMatch.Direct => "прямая ссылка",
            StoryAssetUsageMatch.Dynamic => "динамическая ссылка",
            _ => "неизвестно",
        };

        private static void PingUsageAsset(string contentPath)
        {
            var assetPath = ContentAssets.UnityAssetPath(contentPath);
            var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (asset != null)
                EditorGUIUtility.PingObject(asset);
        }

        private void ClearUsageReport()
        {
            _usageEntries = Array.Empty<StoryAssetUsageEntry>();
            InvalidateUsageTable();
        }

        private void InvalidateUsageTable()
        {
            _cachedUsageEntries = null;
            _cachedUsageFilter = null;
            _cachedUsageChunkSizeMiB = -1;
        }

        private void DrawChunks()
        {
            var chunks = serializedObject.FindProperty(
                StoryChunkAuthoring.ChunksProperty);
            EditorGUILayout.Space(10f);
            _chunksExpanded = EditorGUILayout.Foldout(
                _chunksExpanded,
                $"Ручной состав чанков ({chunks.arraySize})",
                true);
            if (!_chunksExpanded)
                return;

            EditorGUILayout.HelpBox(
                "Размер файла рассчитывается только для раскрытого чанка. "
                + "Одновременно раскрывается один чанк, чтобы Inspector не "
                + "загружал сотни объектов на каждом кадре.",
                MessageType.None);
            for (var chunkIndex = 0; chunkIndex < chunks.arraySize; chunkIndex++)
                DrawChunk(chunks, chunkIndex);
            if (GUILayout.Button("Добавить пустой чанк"))
            {
                chunks.InsertArrayElementAtIndex(chunks.arraySize);
                var chunk = chunks.GetArrayElementAtIndex(chunks.arraySize - 1);
                chunk.FindPropertyRelative(
                    StoryChunkAuthoring.AssetGuidsProperty).arraySize = 0;
                _expandedChunks.Clear();
                _expandedChunks.Add(chunks.arraySize - 1);
                ApplyAndExit();
            }
        }

        private void DrawUnusedAssets()
        {
            var assets = serializedObject.FindProperty(
                StoryChunkAuthoring.UnusedAssetGuidsProperty);
            EditorGUILayout.Space(10f);
            _unusedExpanded = EditorGUILayout.Foldout(
                _unusedExpanded,
                $"Не используется ({assets.arraySize})",
                true);
            if (!_unusedExpanded)
                return;

            EditorGUILayout.HelpBox(
                "Эти Unity-ассеты остаются в проекте, но намеренно не входят "
                + "в чанки истории. Повторный расчёт чанков не добавит их обратно. "
                + "Метка «Постер видео» вычисляется по MP4 и video aliases.",
                MessageType.None);
            var pageCount = Math.Max(
                1,
                (assets.arraySize + _chunkRowsPerPage - 1) / _chunkRowsPerPage);
            _unusedPage = Mathf.Clamp(_unusedPage, 0, pageCount - 1);
            if (pageCount > 1)
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    using (new EditorGUI.DisabledScope(_unusedPage == 0))
                    {
                        if (GUILayout.Button(
                                "◀",
                                EditorStyles.toolbarButton,
                                GUILayout.Width(28f)))
                        {
                            _unusedPage--;
                        }
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(
                        $"Объекты: страница {_unusedPage + 1}/{pageCount}",
                        EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                    using (new EditorGUI.DisabledScope(_unusedPage >= pageCount - 1))
                    {
                        if (GUILayout.Button(
                                "▶",
                                EditorStyles.toolbarButton,
                                GUILayout.Width(28f)))
                        {
                            _unusedPage++;
                        }
                    }
                }
            }
            var first = _unusedPage * _chunkRowsPerPage;
            var last = Math.Min(first + _chunkRowsPerPage, assets.arraySize);
            for (var assetIndex = first; assetIndex < last; assetIndex++)
                DrawAsset(assets, assetIndex, true);
            if (GUILayout.Button("Добавить объект"))
            {
                assets.InsertArrayElementAtIndex(assets.arraySize);
                assets.GetArrayElementAtIndex(assets.arraySize - 1).stringValue =
                    string.Empty;
                _unusedPage = (assets.arraySize - 1) / _chunkRowsPerPage;
                ApplyAndExit();
            }
        }

        private void DrawChunk(SerializedProperty chunks, int chunkIndex)
        {
            var chunk = chunks.GetArrayElementAtIndex(chunkIndex);
            var assets = chunk.FindPropertyRelative(
                StoryChunkAuthoring.AssetGuidsProperty);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var wasExpanded = _expandedChunks.Contains(chunkIndex);
                    var size = wasExpanded
                        ? " · " + EditorUtility.FormatBytes(ChunkBytes(
                            chunkIndex,
                            assets))
                        : string.Empty;
                    var expanded = wasExpanded;
                    expanded = EditorGUILayout.Foldout(
                        expanded,
                        $"Чанк {chunkIndex} · {assets.arraySize} объектов{size}",
                        true);
                    if (expanded && !wasExpanded)
                    {
                        _expandedChunks.Clear();
                        _expandedChunks.Add(chunkIndex);
                    }
                    else if (!expanded && wasExpanded)
                    {
                        _expandedChunks.Remove(chunkIndex);
                    }

                    using (new EditorGUI.DisabledScope(chunkIndex == 0))
                    {
                        if (GUILayout.Button("↑", GUILayout.Width(26f)))
                        {
                            chunks.MoveArrayElement(chunkIndex, chunkIndex - 1);
                            ApplyAndExit();
                        }
                    }
                    using (new EditorGUI.DisabledScope(
                               chunkIndex >= chunks.arraySize - 1))
                    {
                        if (GUILayout.Button("↓", GUILayout.Width(26f)))
                        {
                            chunks.MoveArrayElement(chunkIndex, chunkIndex + 1);
                            ApplyAndExit();
                        }
                    }
                    if (GUILayout.Button("×", GUILayout.Width(26f))
                        && EditorUtility.DisplayDialog(
                            "Удалить чанк?",
                            $"Чанк {chunkIndex} и его список будут удалены.",
                            "Удалить",
                            "Отмена"))
                    {
                        chunks.DeleteArrayElementAtIndex(chunkIndex);
                        ApplyAndExit();
                    }
                }

                if (!_expandedChunks.Contains(chunkIndex))
                    return;
                DrawChunkPagination(
                    chunkIndex,
                    assets.arraySize,
                    out var firstAsset,
                    out var lastAsset);
                for (var assetIndex = firstAsset; assetIndex < lastAsset; assetIndex++)
                    DrawAsset(assets, assetIndex);
                if (GUILayout.Button("Добавить объект"))
                {
                    assets.InsertArrayElementAtIndex(assets.arraySize);
                    assets.GetArrayElementAtIndex(assets.arraySize - 1).stringValue =
                        string.Empty;
                    ApplyAndExit();
                }
            }
        }

        private void DrawChunkPagination(
            int chunkIndex,
            int assetCount,
            out int firstAsset,
            out int lastAsset)
        {
            var pageCount = Math.Max(
                1,
                (assetCount + _chunkRowsPerPage - 1) / _chunkRowsPerPage);
            _chunkPages.TryGetValue(chunkIndex, out var page);
            page = Mathf.Clamp(page, 0, pageCount - 1);
            if (pageCount > 1)
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    using (new EditorGUI.DisabledScope(page == 0))
                    {
                        if (GUILayout.Button(
                                "◀",
                                EditorStyles.toolbarButton,
                                GUILayout.Width(28f)))
                        {
                            page--;
                        }
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(
                        $"Объекты: страница {page + 1}/{pageCount}",
                        EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                    using (new EditorGUI.DisabledScope(page >= pageCount - 1))
                    {
                        if (GUILayout.Button(
                                "▶",
                                EditorStyles.toolbarButton,
                                GUILayout.Width(28f)))
                        {
                            page++;
                        }
                    }
                }
            }
            _chunkPages[chunkIndex] = page;
            firstAsset = page * _chunkRowsPerPage;
            lastAsset = Math.Min(firstAsset + _chunkRowsPerPage, assetCount);
        }

        private void DrawAsset(
            SerializedProperty assets,
            int assetIndex,
            bool unused = false)
        {
            var guid = assets.GetArrayElementAtIndex(assetIndex);
            var path = AssetDatabase.GUIDToAssetPath(guid.stringValue);
            var current = string.IsNullOrEmpty(path)
                ? null
                : AssetDatabase.LoadMainAssetAtPath(path);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                var selected = EditorGUILayout.ObjectField(
                    current,
                    typeof(Object),
                    false);
                if (EditorGUI.EndChangeCheck())
                {
                    guid.stringValue = selected == null
                        ? string.Empty
                        : unused
                            ? StoryChunkAuthoring.GuidForUnusedObject(selected)
                            : StoryChunkAuthoring.GuidForChunkObject(selected);
                    InvalidateUsageTable();
                    InvalidateChunkPresentation();
                }
                if (unused && IsVideoPoster(path))
                {
                    GUILayout.Label(
                        new GUIContent(
                            "Постер видео",
                            "Для этой локации существует прямой или "
                            + "alias-resolved MP4; PNG намеренно исключён "
                            + "из чанков."),
                        EditorStyles.miniButton,
                        GUILayout.Width(96f));
                }
                using (new EditorGUI.DisabledScope(assetIndex == 0))
                {
                    if (GUILayout.Button("↑", GUILayout.Width(24f)))
                    {
                        assets.MoveArrayElement(assetIndex, assetIndex - 1);
                        ApplyAndExit();
                    }
                }
                using (new EditorGUI.DisabledScope(assetIndex >= assets.arraySize - 1))
                {
                    if (GUILayout.Button("↓", GUILayout.Width(24f)))
                    {
                        assets.MoveArrayElement(assetIndex, assetIndex + 1);
                        ApplyAndExit();
                    }
                }
                if (GUILayout.Button("×", GUILayout.Width(24f)))
                {
                    assets.DeleteArrayElementAtIndex(assetIndex);
                    ApplyAndExit();
                }
            }
            if (current == null && !string.IsNullOrWhiteSpace(guid.stringValue))
            {
                EditorGUILayout.HelpBox(
                    $"GUID не найден: {guid.stringValue}",
                    MessageType.Error);
            }
        }

        private long ChunkBytes(int chunkIndex, SerializedProperty assets)
        {
            if (_chunkBytes.TryGetValue(chunkIndex, out var cached))
                return cached;

            long result = 0;
            for (var index = 0; index < assets.arraySize; index++)
            {
                var guid = assets.GetArrayElementAtIndex(index).stringValue;
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var absolute = StoryChunkAuthoring.AbsolutePath(path);
                if (File.Exists(absolute))
                    result += new FileInfo(absolute).Length;
            }
            _chunkBytes[chunkIndex] = result;
            return result;
        }

        private void InvalidateChunkPresentation()
        {
            _chunkBytes.Clear();
        }

        private bool IsVideoPoster(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)
                || !assetPath.StartsWith(
                    "Assets/Locations/",
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            var id = ContentAddressing.TechnicalAssetIdConvention.Canonicalize(
                Path.GetFileNameWithoutExtension(assetPath));
            return VideoPosterIds().Contains(id);
        }

        private HashSet<string> VideoPosterIds()
        {
            if (_videoPosterIds != null)
                return _videoPosterIds;

            var storyId = serializedObject.FindProperty("_id").stringValue;
            var videoIds = new HashSet<string>(ContentAssets
                .FindContentFiles(storyId)
                .Select(value => value.ContentPath)
                .Where(path => path.StartsWith(
                    "novelsvideos/",
                    StringComparison.OrdinalIgnoreCase))
                .Select(Path.GetFileNameWithoutExtension)
                .Select(ContentAddressing.TechnicalAssetIdConvention.Canonicalize),
                StringComparer.OrdinalIgnoreCase);
            var result = new HashSet<string>(
                videoIds,
                StringComparer.OrdinalIgnoreCase);
            try
            {
                var definition = ((Content.NovelContentAsset)target).ToDefinition();
                foreach (var alias in definition.VideoAliases)
                {
                    if (videoIds.Contains(definition.ResolveVideoId(alias.Alias)))
                        result.Add(alias.Alias);
                }
            }
            catch (ArgumentException)
            {
                // Keep direct video markers visible while an alias is being edited.
            }
            _videoPosterIds = result;
            return _videoPosterIds;
        }

        private void InvalidateVideoPosterIds()
        {
            _videoPosterIds = null;
        }

        private void ApplyAndExit()
        {
            serializedObject.ApplyModifiedProperties();
            InvalidateUsageTable();
            InvalidateChunkPresentation();
            GUIUtility.ExitGUI();
        }
    }

    internal static class StoryChunkAuthoring
    {
        internal const string RootInkGuidProperty = "_authoringRootInkGuid";
        internal const string ChunkSizeProperty = "_authoringChunkSizeMiB";
        internal const string ChunksProperty = "_authoringChunks";
        internal const string AssetGuidsProperty = "_assetGuids";
        internal const string UnusedAssetGuidsProperty = "_authoringUnusedAssetGuids";

        internal static int ChunkCount(Content.NovelContentAsset definition)
        {
            var serialized = new SerializedObject(definition);
            return serialized.FindProperty(ChunksProperty).arraySize;
        }

        internal static int ChunkSizeMiB(Content.NovelContentAsset definition)
        {
            var serialized = new SerializedObject(definition);
            return Math.Max(1, serialized.FindProperty(ChunkSizeProperty).intValue);
        }

        internal static string RootInkPath(Content.NovelContentAsset definition)
        {
            var serialized = new SerializedObject(definition);
            var guid = serialized.FindProperty(RootInkGuidProperty).stringValue;
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(path) && File.Exists(AbsolutePath(path)))
                return AbsolutePath(path);

            var id = serialized.FindProperty("_id").stringValue;
            var fallback = ContentAssets.UnityAssetPath($"noveltexts/{id}/{id}.ink");
            return File.Exists(AbsolutePath(fallback))
                ? AbsolutePath(fallback)
                : string.Empty;
        }

        internal static void WriteLayout(
            Content.NovelContentAsset definition,
            string rootInkPath,
            int chunkSizeMiB,
            StoryChunkLayout layout)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            if (layout == null)
                throw new ArgumentNullException(nameof(layout));
            Undo.RecordObject(definition, "Generate story chunks");
            var serialized = new SerializedObject(definition);
            var rootAssetPath = ProjectAssetPath(rootInkPath);
            var rootGuid = AssetDatabase.AssetPathToGUID(rootAssetPath);
            if (string.IsNullOrEmpty(rootGuid))
                throw new InvalidOperationException($"Ink asset is not imported: {rootAssetPath}");
            serialized.FindProperty(RootInkGuidProperty).stringValue = rootGuid;
            serialized.FindProperty(ChunkSizeProperty).intValue = Math.Max(1, chunkSizeMiB);
            var chunks = serialized.FindProperty(ChunksProperty);
            chunks.arraySize = layout.chunks.Length;
            for (var chunkIndex = 0; chunkIndex < layout.chunks.Length; chunkIndex++)
            {
                var target = chunks.GetArrayElementAtIndex(chunkIndex)
                    .FindPropertyRelative(AssetGuidsProperty);
                var paths = layout.chunks[chunkIndex].assets ?? Array.Empty<string>();
                target.arraySize = paths.Length;
                for (var assetIndex = 0; assetIndex < paths.Length; assetIndex++)
                {
                    var assetPath = ContentAssets.UnityAssetPath(paths[assetIndex]);
                    var guid = AssetDatabase.AssetPathToGUID(assetPath);
                    if (string.IsNullOrEmpty(guid))
                    {
                        throw new InvalidOperationException(
                            $"Chunk asset is not imported: {assetPath}");
                    }
                    target.GetArrayElementAtIndex(assetIndex).stringValue = guid;
                }
            }
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
        }

        internal static bool TryReadLayout(
            string storyId,
            out StoryChunkLayout layout)
        {
            var definition = FindDefinition(storyId);
            if (definition == null)
            {
                layout = null;
                return false;
            }
            return TryReadLayout(definition, out layout);
        }

        internal static bool TryReadLayout(
            Content.NovelContentAsset definition,
            out StoryChunkLayout layout)
        {
            var serialized = new SerializedObject(definition);
            var chunks = serialized.FindProperty(ChunksProperty);
            if (chunks == null || chunks.arraySize == 0)
            {
                layout = null;
                return false;
            }

            var unused = UnusedPaths(definition);
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var entries = new StoryChunkLayoutEntry[chunks.arraySize];
            for (var chunkIndex = 0; chunkIndex < chunks.arraySize; chunkIndex++)
            {
                var guids = chunks.GetArrayElementAtIndex(chunkIndex)
                    .FindPropertyRelative(AssetGuidsProperty);
                var paths = new string[guids.arraySize];
                for (var assetIndex = 0; assetIndex < guids.arraySize; assetIndex++)
                {
                    var guid = guids.GetArrayElementAtIndex(assetIndex).stringValue;
                    if (string.IsNullOrWhiteSpace(guid))
                    {
                        throw new InvalidOperationException(
                            $"Chunk {chunkIndex} contains an empty asset slot.");
                    }
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(assetPath))
                    {
                        throw new InvalidOperationException(
                            $"Chunk {chunkIndex} contains missing GUID '{guid}'.");
                    }
                    var path = ContentPath(assetPath);
                    if (unused.Contains(path))
                    {
                        throw new InvalidOperationException(
                            $"Asset is both chunked and unused: {path}");
                    }
                    if (!seen.Add(path))
                        throw new InvalidOperationException($"Duplicate chunk asset: {path}");
                    paths[assetIndex] = path;
                }
                entries[chunkIndex] = new StoryChunkLayoutEntry
                {
                    index = chunkIndex,
                    assets = paths,
                };
            }
            layout = new StoryChunkLayout {chunks = entries};
            return true;
        }

        internal static string Validate(Content.NovelContentAsset definition)
        {
            var aliasCount = ArtAliasAuthoring.Validate(definition);
            if (!TryReadLayout(definition, out var layout))
                throw new InvalidOperationException("Разметка чанков ещё не создана.");
            var availableArt = new HashSet<string>(
                ContentAssets.FindBundleAssets(),
                StringComparer.Ordinal);
            var availableFiles = new HashSet<string>(ContentAssets
                .FindContentFiles(definition.ToDefinition().Id)
                .Select(value => value.ContentPath),
                StringComparer.OrdinalIgnoreCase);
            var assigned = layout.chunks
                .SelectMany(chunk => chunk.assets)
                .ToArray();
            var missing = assigned.Where(path => path.StartsWith(
                    "Assets/",
                    StringComparison.Ordinal)
                    ? !availableArt.Contains(path)
                    : !availableFiles.Contains(path))
                .ToArray();
            if (missing.Length > 0)
            {
                throw new InvalidOperationException(
                    "Разметка содержит недоступные файлы:\n" + string.Join("\n", missing));
            }
            var emptyChunks = layout.chunks.Count(chunk => !chunk.assets.Any(path =>
                path.StartsWith("Assets/", StringComparison.Ordinal)));
            if (emptyChunks > 0)
            {
                throw new InvalidOperationException(
                    $"Чанков без Unity-ассетов: {emptyChunks}. "
                    + "Добавьте хотя бы один арт или prefab в каждый чанк.");
            }
            var assignedArt = new HashSet<string>(
                assigned.Where(path => path.StartsWith("Assets/", StringComparison.Ordinal)),
                StringComparer.Ordinal);
            var unusedArt = UnusedPaths(definition);
            var missingUnused = unusedArt.Where(path => !availableArt.Contains(path)).ToArray();
            if (missingUnused.Length > 0)
            {
                throw new InvalidOperationException(
                    "Неиспользуемые ассеты недоступны:\n"
                    + string.Join("\n", missingUnused));
            }
            var unassigned = availableArt.Count(path =>
                !assignedArt.Contains(path) && !unusedArt.Contains(path));
            return $"Разметка корректна. Чанков: {layout.chunks.Length}, "
                + $"назначено объектов: {assigned.Length}, "
                + $"алиасов арта: {aliasCount}, "
                + $"не используется Unity-ассетов: {unusedArt.Count}, "
                + $"не распределено Unity-ассетов: {unassigned}.";
        }

        internal static HashSet<string> UnusedPaths(
            Content.NovelContentAsset definition)
        {
            var serialized = new SerializedObject(definition);
            var assets = serialized.FindProperty(UnusedAssetGuidsProperty);
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (assets == null)
                return result;
            for (var index = 0; index < assets.arraySize; index++)
            {
                var guid = assets.GetArrayElementAtIndex(index).stringValue;
                if (string.IsNullOrWhiteSpace(guid))
                {
                    throw new InvalidOperationException(
                        $"Не используется: пустой слот {index}.");
                }
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(assetPath))
                {
                    throw new InvalidOperationException(
                        $"Не используется: GUID не найден '{guid}'.");
                }
                if (!ContentAssets.IsBundleSource(assetPath))
                {
                    throw new InvalidOperationException(
                        $"Не используется: ожидается Unity-ассет из чанков, получен {assetPath}");
                }
                var path = ContentPath(assetPath);
                if (!result.Add(path))
                {
                    throw new InvalidOperationException(
                        $"Не используется: ассет указан повторно: {path}");
                }
            }
            return result;
        }

        internal static string GuidForChunkObject(Object value)
        {
            var path = AssetDatabase.GetAssetPath(value);
            ContentPath(path);
            var guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid))
                throw new InvalidOperationException($"Asset has no GUID: {path}");
            return guid;
        }

        internal static string GuidForUnusedObject(Object value)
        {
            var path = AssetDatabase.GetAssetPath(value);
            if (!ContentAssets.IsBundleSource(path))
            {
                throw new InvalidOperationException(
                    $"В «Не используется» можно добавить только Unity-ассет из чанков: {path}");
            }
            return GuidForChunkObject(value);
        }

        internal static string AbsolutePath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return string.Empty;
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Unity project root is unavailable.");
            return Path.GetFullPath(Path.Combine(projectRoot, assetPath));
        }

        private static Content.NovelContentAsset FindDefinition(string storyId)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:NovelContentAsset"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var definition = AssetDatabase.LoadAssetAtPath<Content.NovelContentAsset>(path);
                if (definition == null)
                    continue;
                var serialized = new SerializedObject(definition);
                if (string.Equals(
                        serialized.FindProperty("_id").stringValue,
                        storyId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return definition;
                }
            }
            return null;
        }

        private static string ProjectAssetPath(string absolutePath)
        {
            var full = Path.GetFullPath(absolutePath).Replace('\\', '/');
            var data = Application.dataPath.Replace('\\', '/');
            if (!full.StartsWith(data + "/", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Path is outside Assets: {absolutePath}");
            return "Assets" + full.Substring(data.Length);
        }

        private static string ContentPath(string assetPath)
            => ContentAssets.ContentPath(assetPath);
    }

    internal static class ArtAliasAuthoring
    {
        internal static int Validate(Content.NovelContentAsset authoring)
        {
            if (authoring == null)
                throw new ArgumentNullException(nameof(authoring));
            var definition = authoring.ToDefinition();
            if (definition.ArtAliases.Count == 0)
                return 0;

            var unused = StoryChunkAuthoring.UnusedPaths(authoring);
            var available = new HashSet<string>(
                ContentAssets.FindBundleAssets()
                    .Where(path => !unused.Contains(path))
                    .Select(path => ContentAddressing.TechnicalAssetIdConvention
                        .Canonicalize(ContentAssets.BundleAddress(definition.Id, path))
                        .Replace('\\', '/')
                        .Trim('/')),
                StringComparer.OrdinalIgnoreCase);
            var contentPrefix = ContentAddressing.ContentPackageConvention
                .ContentRoot(definition.Id) + "/";
            foreach (var alias in definition.ArtAliases)
            {
                var finalTarget = definition.ResolveArtAddress(
                    contentPrefix + alias.Target);
                if (!available.Contains(finalTarget))
                {
                    throw new InvalidOperationException(
                        $"Art alias target is missing or unused: "
                        + $"{alias.Alias} -> {finalTarget}");
                }
            }
            return definition.ArtAliases.Count;
        }
    }
}
