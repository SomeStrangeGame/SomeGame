using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
        public static void Report() => Process(apply: false, _defaultPadding);

        [MenuItem("Novels/Content/Character Sprites/Apply Alpha Trim")]
        public static void Apply() => Process(apply: true, _defaultPadding);

        public static void Run()
        {
            var mode = Argument("-spriteTrimMode", "report");
            var paddingText = Argument(
                "-spriteTrimPadding",
                _defaultPadding.ToString(CultureInfo.InvariantCulture));
            if (!int.TryParse(paddingText, out var padding) || padding < 0)
                throw new ArgumentException($"Invalid sprite trim padding: '{paddingText}'.");
            Process(string.Equals(mode, "apply", StringComparison.OrdinalIgnoreCase), padding);
        }

        private static void Process(bool apply, int padding)
        {
            var characterRoot = FindCharacterRoot();
            var manifestPath = $"{characterRoot}/{_manifestName}";
            var manifest = AssetDatabase.LoadAssetAtPath<Character.CharacterSpriteTrimManifest>(
                manifestPath);
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
            var changed = 0;
            var skipped = 0;
            var entries = new Dictionary<string, Character.CharacterSpriteTrimEntry>(
                existing,
                StringComparer.OrdinalIgnoreCase);
            var backupRoot = apply
                ? Path.Combine(
                    Directory.GetParent(Application.dataPath)!.FullName,
                    "Build",
                    "SpriteTrimBackup",
                    DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture))
                : null;

            foreach (var path in paths)
            {
                if (existing.TryGetValue(path, out var existingEntry))
                {
                    originalPixels += (long)existingEntry.OriginalWidth * existingEntry.OriginalHeight;
                    trimmedPixels += (long)existingEntry.Crop.width * existingEntry.Crop.height;
                    skipped++;
                    continue;
                }

                var source = LoadPng(path);
                try
                {
                    var crop = AlphaBounds(source, padding);
                    originalPixels += (long)source.width * source.height;
                    trimmedPixels += (long)crop.width * crop.height;
                    if (crop.width == source.width && crop.height == source.height)
                    {
                        skipped++;
                        continue;
                    }

                    changed++;
                    var entry = new Character.CharacterSpriteTrimEntry(
                        path,
                        source.width,
                        source.height,
                        crop);
                    entries[path] = entry;
                    if (apply)
                    {
                        Backup(path, backupRoot);
                        WriteCrop(path, source, crop);
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(source);
                }
            }

            if (apply && changed > 0)
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                if (manifest == null)
                {
                    manifest = ScriptableObject.CreateInstance<Character.CharacterSpriteTrimManifest>();
                    AssetDatabase.CreateAsset(manifest, manifestPath);
                }
                manifest.ReplaceEntries(entries.Values);
                EditorUtility.SetDirty(manifest);
                AssetDatabase.SaveAssets();
            }

            var saved = originalPixels == 0
                ? 0d
                : 100d * (originalPixels - trimmedPixels) / originalPixels;
            Debug.Log(
                $"Character sprite alpha trim {(apply ? "applied" : "report")}: "
                + $"{paths.Length} files, {changed} new trims, {skipped} unchanged/already trimmed, "
                + $"useful-area reduction {saved:F2}%"
                + (apply ? $", backup: {backupRoot}" : string.Empty));
        }

        private static string FindCharacterRoot()
        {
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

        private static Texture2D LoadPng(string assetPath)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(Absolute(assetPath)), false))
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

        private static void WriteCrop(string assetPath, Texture2D source, RectInt crop)
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
                File.WriteAllBytes(Absolute(assetPath), result.EncodeToPNG());
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
}
