using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Novels.ContentSdk.Editor
{
    public static class CharacterSpriteAlphaTrim
    {
        private const int _defaultPadding = 4;
        private const string _manifestName = "sprite-trim-manifest.asset";
        private static readonly string[] _targetSegments =
        {
            "/emotions/",
            "/hairs/",
            "/accessories/",
            "/clothes/",
        };

        [MenuItem("Novels/Content/Character Sprites/Report Alpha Trim")]
        public static void Report()
        {
            var plan = InspectCurrent(out _, _defaultPadding);
            Debug.Log(plan.Summary);
        }

        [MenuItem("Novels/Content/Character Sprites/Apply Alpha Trim")]
        public static void Apply()
        {
            var plan = InspectCurrent(out var manifest, _defaultPadding);
            if (plan.TrimCount == 0)
            {
                Debug.Log(plan.Summary);
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "Обрезать новые или заменённые PNG?",
                    plan.Confirmation,
                    $"Обрезать {plan.TrimCount}",
                    "Отмена"))
            {
                return;
            }

            Debug.Log(ApplyTrim(manifest, plan));
        }

        internal static CharacterSpriteTrimPlan Inspect(
            Character.CharacterSpriteTrimManifest manifest) =>
            Analyze(manifest, ManifestPath(manifest), _defaultPadding);

        internal static string UpdateIndex(
            Character.CharacterSpriteTrimManifest manifest,
            CharacterSpriteTrimPlan expectedPlan)
        {
            var currentPlan = Analyze(manifest, ManifestPath(manifest), _defaultPadding);
            EnsurePlanIsCurrent(expectedPlan, currentPlan);
            if (!currentPlan.HasIndexChanges)
                return "Индекс уже актуален. PNG не изменялись.";

            Undo.RecordObject(manifest, "Update character sprite trim index");
            manifest.ReplaceEntries(currentPlan.IndexEntries.Values);
            EditorUtility.SetDirty(manifest);
            AssetDatabase.SaveAssets();

            var summary =
                $"Индекс обновлён: новых хешей {currentPlan.IndexUpdates}, "
                + $"удалено устаревших записей {currentPlan.IndexRemovedEntries}. "
                + "PNG не изменялись.";
            Debug.Log(summary);
            return summary;
        }

        internal static string ApplyTrim(
            Character.CharacterSpriteTrimManifest manifest,
            CharacterSpriteTrimPlan expectedPlan)
        {
            if (expectedPlan == null)
                throw new InvalidOperationException(
                    "Сначала нажмите «Проверить изменения».");
            var manifestPath = manifest == null
                ? expectedPlan.ManifestPath
                : ManifestPath(manifest);
            var currentPlan = Analyze(manifest, manifestPath, expectedPlan.Padding);
            EnsurePlanIsCurrent(expectedPlan, currentPlan);
            return ApplyPlan(manifest, currentPlan);
        }

        public static void Run()
        {
            var mode = Argument("-spriteTrimMode", "report");
            var paddingText = Argument(
                "-spriteTrimPadding",
                _defaultPadding.ToString(CultureInfo.InvariantCulture));
            if (!int.TryParse(paddingText, out var padding) || padding < 0)
                throw new ArgumentException($"Invalid sprite trim padding: '{paddingText}'.");
            var plan = InspectCurrent(out var manifest, padding);
            if (string.Equals(mode, "apply", StringComparison.OrdinalIgnoreCase))
                Debug.Log(ApplyTrim(manifest, plan));
            else
                Debug.Log(plan.Summary);
        }

        private static CharacterSpriteTrimPlan InspectCurrent(
            out Character.CharacterSpriteTrimManifest manifest,
            int padding)
        {
            var characterRoot = FindCharacterRoot();
            var manifestPath = $"{characterRoot}/{_manifestName}";
            manifest = AssetDatabase.LoadAssetAtPath<
                Character.CharacterSpriteTrimManifest>(manifestPath);
            return Analyze(manifest, manifestPath, padding);
        }

        private static CharacterSpriteTrimPlan Analyze(
            Character.CharacterSpriteTrimManifest manifest,
            string manifestPath,
            int padding)
        {
            ValidateManifestPath(manifestPath);
            var characterRoot = Path.GetDirectoryName(manifestPath)?.Replace('\\', '/')
                ?? throw new InvalidOperationException(
                    $"Cannot resolve character root for {manifestPath}.");
            var existing = manifest == null
                ? new Dictionary<string, Character.CharacterSpriteTrimEntry>(
                    StringComparer.OrdinalIgnoreCase)
                : manifest.Entries.ToDictionary(
                    entry => entry.AssetAddress,
                    StringComparer.OrdinalIgnoreCase);
            var paths = Directory.GetFiles(Absolute(characterRoot), "*.png", SearchOption.AllDirectories)
                .Select(AssetPath)
                .Where(IsTarget)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();

            long originalPixels = 0;
            long trimmedPixels = 0;
            var indexUpdates = 0;
            var unchanged = 0;
            var noTrimNeeded = 0;
            var indexEntries = new Dictionary<string, Character.CharacterSpriteTrimEntry>(
                StringComparer.OrdinalIgnoreCase);
            var trimCandidates = new List<CharacterSpriteTrimCandidate>();
            var storyId = CurrentStoryId();
            var signature = new StringBuilder();
            foreach (var entry in existing.Values.OrderBy(
                         value => value.AssetAddress,
                         StringComparer.Ordinal))
            {
                signature.Append("entry|")
                    .Append(entry.AssetAddress).Append('|')
                    .Append(entry.OriginalWidth).Append('|')
                    .Append(entry.OriginalHeight).Append('|')
                    .Append(entry.Crop.x).Append('|')
                    .Append(entry.Crop.y).Append('|')
                    .Append(entry.Crop.width).Append('|')
                    .Append(entry.Crop.height).Append('|')
                    .Append(entry.TrimmedSha256).AppendLine();
            }

            foreach (var path in paths)
            {
                var address = ContentAssets.BundleAddress(storyId, path);
                var png = File.ReadAllBytes(Absolute(path));
                var currentSha256 = Sha256(png);
                signature.Append("file|")
                    .Append(path).Append('|')
                    .Append(address).Append('|')
                    .Append(currentSha256).Append('|');
                if (existing.TryGetValue(address, out var existingEntry)
                    && string.Equals(
                        existingEntry.TrimmedSha256,
                        currentSha256,
                        StringComparison.OrdinalIgnoreCase))
                {
                    signature.AppendLine("unchanged");
                    indexEntries[address] = existingEntry;
                    originalPixels += (long)existingEntry.OriginalWidth
                        * existingEntry.OriginalHeight;
                    trimmedPixels += (long)existingEntry.Crop.width
                        * existingEntry.Crop.height;
                    unchanged++;
                    continue;
                }

                var source = LoadPng(path, png);
                try
                {
                    if (existing.TryGetValue(address, out existingEntry)
                        && existingEntry.Crop.width == source.width
                        && existingEntry.Crop.height == source.height)
                    {
                        signature.AppendLine("index-update");
                        indexEntries[address] = new Character.CharacterSpriteTrimEntry(
                            existingEntry.AssetAddress,
                            existingEntry.OriginalWidth,
                            existingEntry.OriginalHeight,
                            existingEntry.Crop,
                            currentSha256);
                        originalPixels += (long)existingEntry.OriginalWidth
                            * existingEntry.OriginalHeight;
                        trimmedPixels += (long)existingEntry.Crop.width
                            * existingEntry.Crop.height;
                        indexUpdates++;
                        continue;
                    }

                    var crop = AlphaBounds(source, padding);
                    originalPixels += (long)source.width * source.height;
                    trimmedPixels += (long)crop.width * crop.height;
                    if (crop.width == source.width && crop.height == source.height)
                    {
                        signature.AppendLine("no-trim-needed");
                        noTrimNeeded++;
                        continue;
                    }

                    signature.Append("trim|")
                        .Append(source.width).Append('|')
                        .Append(source.height).Append('|')
                        .Append(crop.x).Append('|')
                        .Append(crop.y).Append('|')
                        .Append(crop.width).Append('|')
                        .Append(crop.height).AppendLine();
                    trimCandidates.Add(new CharacterSpriteTrimCandidate(
                        path,
                        address,
                        currentSha256,
                        source.width,
                        source.height,
                        crop));
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(source);
                }
            }

            var indexRemovedEntries = existing.Keys.Count(
                address => !indexEntries.ContainsKey(address));
            var candidateAddresses = new HashSet<string>(
                trimCandidates.Select(candidate => candidate.AssetAddress),
                StringComparer.OrdinalIgnoreCase);
            var applyRemovedEntries = existing.Keys.Count(
                address => !indexEntries.ContainsKey(address)
                    && !candidateAddresses.Contains(address));
            var saved = originalPixels == 0
                ? 0d
                : 100d * (originalPixels - trimmedPixels) / originalPixels;
            return new CharacterSpriteTrimPlan(
                manifestPath,
                padding,
                paths.Length,
                existing,
                indexEntries,
                trimCandidates,
                indexUpdates,
                indexRemovedEntries,
                applyRemovedEntries,
                unchanged,
                noTrimNeeded,
                saved,
                Sha256(Encoding.UTF8.GetBytes(signature.ToString())));
        }

        private static string ApplyPlan(
            Character.CharacterSpriteTrimManifest manifest,
            CharacterSpriteTrimPlan plan)
        {
            var finalEntries = new Dictionary<string, Character.CharacterSpriteTrimEntry>(
                plan.IndexEntries,
                StringComparer.OrdinalIgnoreCase);
            var backupRoot = Path.Combine(
                Directory.GetParent(Application.dataPath)!.FullName,
                "Build",
                "SpriteTrimBackup",
                DateTime.UtcNow.ToString(
                    "yyyyMMddTHHmmssfffZ",
                    CultureInfo.InvariantCulture));
            var backedUpPaths = new List<string>();
            var manifestUpdated = false;
            var createdManifest = false;

            try
            {
                foreach (var candidate in plan.TrimCandidates)
                {
                    var png = File.ReadAllBytes(Absolute(candidate.AssetPath));
                    if (!string.Equals(
                            Sha256(png),
                            candidate.SourceSha256,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"PNG changed after preview: {candidate.AssetPath}");
                    }

                    var source = LoadPng(candidate.AssetPath, png);
                    try
                    {
                        var crop = AlphaBounds(source, plan.Padding);
                        if (source.width != candidate.OriginalWidth
                            || source.height != candidate.OriginalHeight
                            || crop != candidate.Crop)
                        {
                            throw new InvalidOperationException(
                                $"PNG geometry changed after preview: {candidate.AssetPath}");
                        }

                        Backup(candidate.AssetPath, backupRoot);
                        backedUpPaths.Add(candidate.AssetPath);
                        var trimmedSha256 = WriteCrop(candidate.AssetPath, source, crop);
                        finalEntries[candidate.AssetAddress] =
                            new Character.CharacterSpriteTrimEntry(
                                candidate.AssetAddress,
                                source.width,
                                source.height,
                                crop,
                                trimmedSha256);
                    }
                    finally
                    {
                        UnityEngine.Object.DestroyImmediate(source);
                    }
                }

                var manifestChanged = plan.TrimCount > 0
                    || plan.IndexUpdates > 0
                    || plan.ApplyRemovedEntries > 0;
                if (manifestChanged)
                {
                    if (plan.TrimCount > 0)
                        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                    if (manifest == null)
                    {
                        manifest = ScriptableObject.CreateInstance<
                            Character.CharacterSpriteTrimManifest>();
                        AssetDatabase.CreateAsset(manifest, plan.ManifestPath);
                        createdManifest = true;
                    }
                    manifestUpdated = true;
                    manifest.ReplaceEntries(finalEntries.Values);
                    EditorUtility.SetDirty(manifest);
                    AssetDatabase.SaveAssets();
                }
            }
            catch (Exception exception)
            {
                if (backedUpPaths.Count == 0 && !manifestUpdated)
                    throw;
                try
                {
                    foreach (var path in backedUpPaths)
                        Restore(path, backupRoot);
                    if (backedUpPaths.Count > 0)
                        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                    if (createdManifest)
                    {
                        AssetDatabase.DeleteAsset(plan.ManifestPath);
                    }
                    else if (manifestUpdated && manifest != null)
                    {
                        manifest.ReplaceEntries(plan.ExistingEntries.Values);
                        EditorUtility.SetDirty(manifest);
                        AssetDatabase.SaveAssets();
                    }
                }
                catch (Exception rollbackException)
                {
                    throw new AggregateException(
                        "Character sprite trim failed and rollback also failed.",
                        exception,
                        rollbackException);
                }
                throw new InvalidOperationException(
                    "Character sprite trim failed. Modified PNGs and the manifest "
                    + "were restored from backup.",
                    exception);
            }

            var summary = plan.TrimCount == 0
                ? $"PNG не изменялись. Индекс: новых хешей {plan.IndexUpdates}, "
                    + $"удалено устаревших записей {plan.ApplyRemovedEntries}."
                : $"Обрезано PNG: {plan.TrimCount}. "
                    + $"Индекс: новых хешей {plan.IndexUpdates}, "
                    + $"удалено устаревших записей {plan.ApplyRemovedEntries}. "
                    + $"Backup: {backupRoot}";
            return summary;
        }

        private static void EnsurePlanIsCurrent(
            CharacterSpriteTrimPlan expected,
            CharacterSpriteTrimPlan current)
        {
            if (expected == null)
                throw new InvalidOperationException(
                    "Сначала нажмите «Проверить изменения».");
            if (!string.Equals(
                    expected.Signature,
                    current.Signature,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "PNG или манифест изменились после проверки. Ничего не записано; "
                    + "нажмите «Проверить изменения» ещё раз.");
            }
        }

        private static string ManifestPath(
            Character.CharacterSpriteTrimManifest manifest)
        {
            if (manifest == null)
                throw new ArgumentNullException(nameof(manifest));
            var manifestPath = AssetDatabase.GetAssetPath(manifest);
            if (string.IsNullOrEmpty(manifestPath))
                throw new InvalidOperationException("Trim manifest is not a project asset.");
            return manifestPath;
        }

        private static void ValidateManifestPath(string manifestPath)
        {
            if (!string.Equals(
                    Path.GetFileName(manifestPath),
                    _manifestName,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Trim manifest must be named '{_manifestName}': {manifestPath}");
            }
        }

        private static string FindCharacterRoot()
        {
            const string simpleRoot = "Assets/Characters";
            if (Directory.Exists(Absolute(simpleRoot)))
                return simpleRoot;

            var contentRoot = "Assets/RemoteAssets/content";
            var roots = Directory.GetDirectories(Absolute(contentRoot), "characters", SearchOption.AllDirectories)
                .Select(path => AssetPath(Directory.GetParent(path)!.FullName))
                .Where(path => path.EndsWith("/story/character", StringComparison.Ordinal))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (roots.Length != 1)
                throw new InvalidOperationException(
                    $"Expected one story character root below {contentRoot}, found {roots.Length}.");
            return roots[0];
        }

        private static string CurrentStoryId()
        {
            var definitions = AssetDatabase.FindAssets("t:NovelContentAsset")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath<Content.NovelContentAsset>(path))
                .Where(value => value != null)
                .ToArray();
            if (definitions.Length != 1)
                throw new InvalidOperationException(
                    $"Expected one story definition, found {definitions.Length}.");
            return definitions[0].ToDefinition().Id;
        }

        private static Texture2D LoadPng(string assetPath, byte[] png)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            if (!ImageConversion.LoadImage(texture, png, false))
                throw new InvalidDataException($"Cannot decode PNG: {assetPath}");
            return texture;
        }

        private static RectInt AlphaBounds(Texture2D texture, int padding)
        {
            var pixels = texture.GetPixels32();
            var minX = texture.width;
            var minY = texture.height;
            var maxX = -1;
            var maxY = -1;
            for (var y = 0; y < texture.height; y++)
            for (var x = 0; x < texture.width; x++)
            {
                if (pixels[y * texture.width + x].a == 0)
                    continue;
                minX = Mathf.Min(minX, x);
                minY = Mathf.Min(minY, y);
                maxX = Mathf.Max(maxX, x);
                maxY = Mathf.Max(maxY, y);
            }

            if (maxX < minX || maxY < minY)
                return new RectInt(0, 0, texture.width, texture.height);
            minX = Mathf.Max(0, minX - padding);
            minY = Mathf.Max(0, minY - padding);
            maxX = Mathf.Min(texture.width - 1, maxX + padding);
            maxY = Mathf.Min(texture.height - 1, maxY + padding);
            return new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        private static string WriteCrop(string assetPath, Texture2D source, RectInt crop)
        {
            var sourcePixels = source.GetPixels32();
            var cropPixels = new Color32[crop.width * crop.height];
            for (var y = 0; y < crop.height; y++)
            {
                Array.Copy(
                    sourcePixels,
                    (crop.y + y) * source.width + crop.x,
                    cropPixels,
                    y * crop.width,
                    crop.width);
            }

            var result = new Texture2D(crop.width, crop.height, TextureFormat.RGBA32, false, true);
            try
            {
                result.SetPixels32(cropPixels);
                result.Apply(false, false);
                var png = result.EncodeToPNG();
                File.WriteAllBytes(Absolute(assetPath), png);
                return Sha256(png);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(result);
            }
        }

        private static void Backup(string assetPath, string backupRoot)
        {
            var destination = Path.Combine(backupRoot, assetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(Absolute(assetPath), destination, overwrite: false);
            var meta = Absolute(assetPath) + ".meta";
            if (File.Exists(meta))
                File.Copy(meta, destination + ".meta", overwrite: false);
        }

        private static void Restore(string assetPath, string backupRoot)
        {
            var source = Path.Combine(backupRoot, assetPath);
            File.Copy(source, Absolute(assetPath), overwrite: true);
            var meta = source + ".meta";
            if (File.Exists(meta))
                File.Copy(meta, Absolute(assetPath) + ".meta", overwrite: true);
        }

        private static string Sha256(byte[] bytes)
        {
            using var sha256 = SHA256.Create();
            return BitConverter.ToString(sha256.ComputeHash(bytes))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private static bool IsTarget(string path) =>
            IsLayer(path) || IsCharacterBody(path);

        private static bool IsLayer(string path) =>
            Array.Exists(
                _targetSegments,
                segment => path.IndexOf(segment, StringComparison.Ordinal) >= 0);

        private static bool IsCharacterBody(string path)
        {
            const string viewSegment = "/view/";
            var viewIndex = path.IndexOf(viewSegment, StringComparison.Ordinal);
            if (viewIndex < 0)
                return false;

            var relative = path.Substring(viewIndex + viewSegment.Length);
            var parts = relative.Split('/');
            if (parts.Length == 1)
                return true;
            if (parts.Length == 2)
                return string.Equals(parts[1], "main.png", StringComparison.OrdinalIgnoreCase);
            return parts.Length == 3
                && string.Equals(parts[1], "child", StringComparison.OrdinalIgnoreCase)
                && string.Equals(parts[2], "main.png", StringComparison.OrdinalIgnoreCase);
        }

        private static string Absolute(string assetPath) =>
            Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, assetPath);

        private static string AssetPath(string absolutePath) =>
            absolutePath.Replace('\\', '/').Substring(
                Directory.GetParent(Application.dataPath)!.FullName.Length + 1);

        private static string Argument(string name, string fallback)
        {
            var arguments = Environment.GetCommandLineArgs();
            var index = Array.IndexOf(arguments, name);
            return index >= 0 && index + 1 < arguments.Length
                ? arguments[index + 1]
                : fallback;
        }
    }

    internal sealed class CharacterSpriteTrimPlan
    {
        internal CharacterSpriteTrimPlan(
            string manifestPath,
            int padding,
            int totalFiles,
            Dictionary<string, Character.CharacterSpriteTrimEntry> existingEntries,
            Dictionary<string, Character.CharacterSpriteTrimEntry> indexEntries,
            List<CharacterSpriteTrimCandidate> trimCandidates,
            int indexUpdates,
            int indexRemovedEntries,
            int applyRemovedEntries,
            int unchanged,
            int noTrimNeeded,
            double usefulAreaReduction,
            string signature)
        {
            ManifestPath = manifestPath;
            Padding = padding;
            TotalFiles = totalFiles;
            ExistingEntries = existingEntries;
            IndexEntries = indexEntries;
            TrimCandidates = trimCandidates;
            IndexUpdates = indexUpdates;
            IndexRemovedEntries = indexRemovedEntries;
            ApplyRemovedEntries = applyRemovedEntries;
            Unchanged = unchanged;
            NoTrimNeeded = noTrimNeeded;
            UsefulAreaReduction = usefulAreaReduction;
            Signature = signature;
        }

        internal string ManifestPath { get; }
        internal int Padding { get; }
        internal int TotalFiles { get; }
        internal Dictionary<string, Character.CharacterSpriteTrimEntry> ExistingEntries { get; }
        internal Dictionary<string, Character.CharacterSpriteTrimEntry> IndexEntries { get; }
        internal List<CharacterSpriteTrimCandidate> TrimCandidates { get; }
        internal int IndexUpdates { get; }
        internal int IndexRemovedEntries { get; }
        internal int ApplyRemovedEntries { get; }
        internal int Unchanged { get; }
        internal int NoTrimNeeded { get; }
        internal double UsefulAreaReduction { get; }
        internal string Signature { get; }
        internal int TrimCount => TrimCandidates.Count;
        internal bool HasIndexChanges => IndexUpdates > 0 || IndexRemovedEntries > 0;

        internal string Summary =>
            "Проверка завершена. PNG не изменены.\n"
            + $"Всего PNG: {TotalFiles}; к физической обрезке: {TrimCount}; "
            + $"только обновить хеш: {IndexUpdates}; "
            + $"удалить устаревших записей индекса: {IndexRemovedEntries}; "
            + $"без изменений: {Unchanged}; обрезка не нужна: {NoTrimNeeded}.\n"
            + $"Сокращение площади по текущему плану: {UsefulAreaReduction:F2}%.";

        internal string Confirmation
        {
            get
            {
                const int maxVisiblePaths = 12;
                var paths = TrimCandidates
                    .Take(maxVisiblePaths)
                    .Select(candidate => candidate.AssetPath)
                    .ToArray();
                var hidden = TrimCount - paths.Length;
                var pathList = string.Join("\n", paths);
                if (hidden > 0)
                    pathList += $"\n…и ещё {hidden}";
                return $"Будут физически перезаписаны только эти PNG ({TrimCount}):\n\n"
                    + pathList
                    + "\n\nПеред каждой записью будет создан backup. "
                    + "Если файлы изменились после проверки, операция отменится до записи.";
            }
        }
    }

    internal sealed class CharacterSpriteTrimCandidate
    {
        internal CharacterSpriteTrimCandidate(
            string assetPath,
            string assetAddress,
            string sourceSha256,
            int originalWidth,
            int originalHeight,
            RectInt crop)
        {
            AssetPath = assetPath;
            AssetAddress = assetAddress;
            SourceSha256 = sourceSha256;
            OriginalWidth = originalWidth;
            OriginalHeight = originalHeight;
            Crop = crop;
        }

        internal string AssetPath { get; }
        internal string AssetAddress { get; }
        internal string SourceSha256 { get; }
        internal int OriginalWidth { get; }
        internal int OriginalHeight { get; }
        internal RectInt Crop { get; }
    }

    [CustomEditor(typeof(Character.CharacterSpriteTrimManifest))]
    public sealed class CharacterSpriteTrimManifestEditor : UnityEditor.Editor
    {
        private const int _maxVisiblePaths = 100;

        private CharacterSpriteTrimPlan _plan;
        private bool _showTrimPaths = true;
        private Vector2 _trimPathScroll;
        private string _message;
        private MessageType _messageType = MessageType.None;

        public override void OnInspectorGUI()
        {
            var manifest = (Character.CharacterSpriteTrimManifest)target;
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.ObjectField("Manifest", manifest, manifest.GetType(), false);
            EditorGUILayout.HelpBox(
                "Служебный индекс читает поддерживаемые PNG из этой папки и "
                + "подпапок. Сначала выполните проверку: она ничего не записывает. "
                + "Обновление индекса никогда не меняет PNG. Физическая обрезка "
                + "доступна отдельно и только для показанного списка файлов.",
                MessageType.Info);
            EditorGUILayout.LabelField(
                "Записей",
                manifest.Entries.Count.ToString(CultureInfo.InvariantCulture));

            var busy = EditorApplication.isCompiling || EditorApplication.isUpdating;
            using (new EditorGUI.DisabledScope(busy))
            {
                if (GUILayout.Button("1. Проверить изменения (без записи)"))
                    Preview(manifest);
            }

            if (_plan != null)
            {
                EditorGUILayout.HelpBox(
                    _plan.Summary,
                    _plan.TrimCount > 0 ? MessageType.Warning : MessageType.Info);

                if (_plan.TrimCount > 0)
                {
                    _showTrimPaths = EditorGUILayout.Foldout(
                        _showTrimPaths,
                        $"PNG к обрезке ({_plan.TrimCount})",
                        true);
                    if (_showTrimPaths)
                    {
                        _trimPathScroll = EditorGUILayout.BeginScrollView(
                            _trimPathScroll,
                            GUILayout.MaxHeight(180));
                        foreach (var candidate in _plan.TrimCandidates.Take(_maxVisiblePaths))
                            EditorGUILayout.LabelField(
                                candidate.AssetPath,
                                EditorStyles.wordWrappedMiniLabel);
                        if (_plan.TrimCount > _maxVisiblePaths)
                        {
                            EditorGUILayout.LabelField(
                                $"…и ещё {_plan.TrimCount - _maxVisiblePaths}",
                                EditorStyles.miniBoldLabel);
                        }
                        EditorGUILayout.EndScrollView();
                    }
                }

                using (new EditorGUI.DisabledScope(busy || !_plan.HasIndexChanges))
                {
                    if (GUILayout.Button(
                            $"2. Обновить индекс без изменения PNG "
                            + $"({_plan.IndexUpdates} хешей, "
                            + $"{_plan.IndexRemovedEntries} удалений)"))
                    {
                        ExecuteAndRefresh(
                            manifest,
                            () => CharacterSpriteAlphaTrim.UpdateIndex(manifest, _plan));
                    }
                }

                using (new EditorGUI.DisabledScope(busy || _plan.TrimCount == 0))
                {
                    if (GUILayout.Button(
                            $"3. Обрезать {_plan.TrimCount} новых/заменённых PNG")
                    && EditorUtility.DisplayDialog(
                        "Обрезать новые или заменённые PNG?",
                        _plan.Confirmation,
                        $"Обрезать {_plan.TrimCount}",
                        "Отмена"))
                    {
                        ExecuteAndRefresh(
                            manifest,
                            () => CharacterSpriteAlphaTrim.ApplyTrim(manifest, _plan));
                    }
                }
            }

            if (!string.IsNullOrEmpty(_message))
                EditorGUILayout.HelpBox(_message, _messageType);
        }

        private void Preview(Character.CharacterSpriteTrimManifest manifest)
        {
            try
            {
                _plan = CharacterSpriteAlphaTrim.Inspect(manifest);
                _message = _plan.Summary;
                _messageType = MessageType.Info;
                Debug.Log(_plan.Summary);
            }
            catch (Exception exception)
            {
                _plan = null;
                ShowError(exception);
            }
            Repaint();
        }

        private void ExecuteAndRefresh(
            Character.CharacterSpriteTrimManifest manifest,
            Func<string> action)
        {
            try
            {
                _message = action();
                _messageType = MessageType.Info;
                _plan = CharacterSpriteAlphaTrim.Inspect(manifest);
            }
            catch (Exception exception)
            {
                ShowError(exception);
                try
                {
                    _plan = CharacterSpriteAlphaTrim.Inspect(manifest);
                }
                catch
                {
                    _plan = null;
                }
            }
            Repaint();
        }

        private void ShowError(Exception exception)
        {
            _message = exception.Message;
            _messageType = MessageType.Error;
            Debug.LogException(exception);
        }
    }
}
