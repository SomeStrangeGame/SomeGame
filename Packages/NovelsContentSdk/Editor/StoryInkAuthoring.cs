using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Novels.ContentSdk.Editor
{
    internal static class StoryInkAuthoring
    {
        internal static StoryAssetUsageEntry[] CreateUsageReport(
            Content.NovelContentAsset definition,
            string sourcePath)
        {
            ValidateSource(definition, sourcePath);
            var runtimeDefinition = definition.ToDefinition();
            var files = ContentAssets
                .FindContentFiles(runtimeDefinition.Id)
                .Select(value => value.ContentPath)
                .ToArray();
            return StoryStreamingPlan.CreateLinearUsageReport(
                sourcePath,
                ContentAssets.FindBundleAssets(),
                files,
                runtimeDefinition);
        }

        internal static string Compile(
            Content.NovelContentAsset definition,
            string sourcePath)
        {
            ValidateSource(definition, sourcePath);
            var result = StorySourceMapBuilder.CompileArtifacts(sourcePath);
            AssetDatabase.Refresh();
            Debug.Log(
                $"Ink story compiled: '{result.CompiledPath}', source map "
                + $"'{result.SourceMapPath}', "
                + $"{result.SourceMapEntryCount} entries.");
            return "Созданы:\n"
                + ProjectPath(result.CompiledPath) + "\n"
                + ProjectPath(result.SourceMapPath) + "\n"
                + $"Source map: {result.SourceMapEntryCount} записей.";
        }

        internal static string UpdateEpisodes(
            Content.NovelContentAsset definition,
            string sourcePath)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            ValidateSource(definition, sourcePath);
            var sources = IncludedEpisodeSources(sourcePath);
            if (sources.Length == 0)
            {
                throw new InvalidOperationException(
                    "В корневом Ink не найдены INCLUDE с ID эпизода вида s01e01.");
            }
            var episodeSources = sources
                .Select(source => (source, id: EpisodeId(source)))
                .ToArray();
            var duplicate = episodeSources
                .GroupBy(value => value.id, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
            {
                throw new InvalidOperationException(
                    $"ID эпизода '{duplicate.Key}' встречается в нескольких INCLUDE.");
            }

            Undo.RecordObject(definition, "Update story episodes from Ink");
            var serialized = new SerializedObject(definition);
            serialized.Update();
            var episodes = serialized.FindProperty("_episodes");
            episodes.arraySize = episodeSources.Length;
            for (var index = 0; index < episodeSources.Length; index++)
            {
                var (source, id) = episodeSources[index];
                var sourceLines = File.ReadLines(source).ToArray();
                var episode = episodes.GetArrayElementAtIndex(index);
                episode.FindPropertyRelative("_id").stringValue = id;
                episode.FindPropertyRelative("_title").stringValue = EpisodeTitle(
                    id,
                    sourceLines);
                episode.FindPropertyRelative("_description").stringValue =
                    EpisodeDescription(sourceLines);
            }
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            var definitionPath = AssetDatabase.GetAssetPath(definition);
            Debug.Log(
                $"Story episodes updated from Ink: {episodeSources.Length}, "
                + $"'{definitionPath}'.");
            return $"Эпизоды обновлены: {episodeSources.Length}\n{definitionPath}";
        }

        private static string[] IncludedEpisodeSources(string sourcePath)
        {
            var directory = Path.GetDirectoryName(sourcePath) ?? string.Empty;
            var includes = File.ReadLines(sourcePath)
                .Select(line => Regex.Match(
                    line,
                    @"^\s*INCLUDE\s+([^\s]+\.ink)\s*$",
                    RegexOptions.IgnoreCase))
                .Where(match => match.Success)
                .Select(match => Path.GetFullPath(
                    Path.Combine(directory, match.Groups[1].Value)))
                .Where(HasEpisodeId)
                .ToArray();
            if (includes.Length == 0
                && HasEpisodeId(sourcePath))
            {
                return new[] {sourcePath};
            }
            return includes;
        }

        private static bool HasEpisodeId(string sourcePath) => Regex.IsMatch(
            Path.GetFileNameWithoutExtension(sourcePath),
            @"s\d+e\d+",
            RegexOptions.IgnoreCase);

        private static string EpisodeId(string sourcePath)
        {
            var matches = Regex.Matches(
                Path.GetFileNameWithoutExtension(sourcePath),
                @"s\d+e\d+",
                RegexOptions.IgnoreCase);
            if (matches.Count != 1)
            {
                throw new InvalidOperationException(
                    $"Имя '{Path.GetFileName(sourcePath)}' должно содержать "
                    + "ровно один ID эпизода вида s01e01.");
            }
            return matches[0].Value.ToLowerInvariant();
        }

        private static string EpisodeTitle(string id, IEnumerable<string> sourceLines)
        {
            foreach (var sourceLine in sourceLines ?? Array.Empty<string>())
            {
                var authored = Regex.Match(
                    sourceLine,
                    @"^\s*\.{2,}\s*\([^)]*\)\s*:\s*Серия\s+\d+\s*:\s*(.+?)\s*$",
                    RegexOptions.IgnoreCase);
                if (authored.Success)
                    return authored.Groups[1].Value.Trim();
            }

            var match = Regex.Match(id, @"^s(\d+)e(\d+)$", RegexOptions.IgnoreCase);
            return match.Success
                ? $"Сезон {int.Parse(match.Groups[1].Value)}, "
                  + $"эпизод {int.Parse(match.Groups[2].Value)}"
                : id;
        }

        private static string EpisodeDescription(IEnumerable<string> sourceLines)
        {
            var lines = sourceLines?.ToArray() ?? Array.Empty<string>();
            foreach (var prefix in new[] {"Описание", "Аннотация"})
            {
                var pattern = $@"^\s*{Regex.Escape(prefix)}\s*:\s*(.+?)\s*$";
                foreach (var sourceLine in lines)
                {
                    var match = Regex.Match(
                        sourceLine,
                        pattern,
                        RegexOptions.IgnoreCase);
                    if (match.Success)
                        return match.Groups[1].Value.Trim();
                }
            }
            return string.Empty;
        }

        private static void ValidateSource(
            Content.NovelContentAsset definition,
            string sourcePath)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            if (string.IsNullOrWhiteSpace(sourcePath)
                || !sourcePath.EndsWith(".ink", StringComparison.OrdinalIgnoreCase)
                || !File.Exists(sourcePath))
            {
                throw new InvalidOperationException(
                    "Назначьте существующий корневой файл с расширением .ink.");
            }
            var serialized = new SerializedObject(definition);
            var storyId = serialized.FindProperty("_id").stringValue;
            var expectedFileName = storyId + ".ink";
            if (!string.Equals(
                    Path.GetFileName(sourcePath),
                    expectedFileName,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Корневой Ink истории '{storyId}' должен называться "
                    + $"'{expectedFileName}'.");
            }
        }

        private static string ProjectPath(string absolutePath)
        {
            var fullPath = Path.GetFullPath(absolutePath).Replace('\\', '/');
            var dataPath = Application.dataPath.Replace('\\', '/');
            return fullPath.StartsWith(dataPath + "/", StringComparison.OrdinalIgnoreCase)
                ? "Assets" + fullPath.Substring(dataPath.Length)
                : fullPath;
        }
    }
}
