using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    internal static class StoryReferenceValidator
    {
        private static readonly Regex _compiledString = new(
            "\"\\^(?<text>(?:\\\\.|[^\"\\\\])*)\"");
        private static readonly Regex _variable = new(
            "^\\s*VAR\\s+(?<name>[^=\\s]+)\\s*=\\s*\"(?<value>[^\"]*)\"",
            RegexOptions.Multiline);

        internal static void Validate(
            string prefix,
            string mainCharacter,
            Novels.Content.EpisodeDefinition episode,
            ICollection<string> errors)
        {
            var compiledPath = Path.Combine(
                Application.streamingAssetsPath,
                "NovelTexts",
                prefix,
                episode.StoryPath);
            if (!File.Exists(compiledPath))
            {
                errors.Add($"Compiled Ink story does not exist: {compiledPath}");
                return;
            }
            ValidateCompiledSyntax(prefix, episode, compiledPath, errors);
            var sourcePath = Path.ChangeExtension(compiledPath, ".ink");
            if (File.Exists(sourcePath))
                ValidateSourceReferences(prefix, mainCharacter, sourcePath, errors);
        }

        private static void ValidateCompiledSyntax(
            string prefix,
            Novels.Content.EpisodeDefinition episode,
            string path,
            ICollection<string> errors)
        {
            var parser = new Novels.StoryCommands.Entity();
            var json = File.ReadAllText(path);
            foreach (Match match in _compiledString.Matches(json))
            {
                var source = Regex.Unescape(match.Groups["text"].Value);
                if (!source.Contains(":"))
                    continue;
                var result = parser.Parse(source, false);
                if (!result.IsSuccess)
                {
                    errors.Add(
                        $"Story command [{result.Error.Code}] in '{episode.StoryPath}': "
                        + $"{result.Error.Message} Source: {source}");
                }
                else if (result.Command is Novels.StoryCommands.AudioStoryCommand audio)
                {
                    ValidateAudio(prefix, episode, audio.Data.AssetName, errors);
                }
            }
        }

        private static void ValidateSourceReferences(
            string prefix,
            string mainCharacter,
            string path,
            ICollection<string> errors)
        {
            var source = File.ReadAllText(path);
            var variables = _variable.Matches(source)
                .Cast<Match>()
                .ToDictionary(
                    match => match.Groups["name"].Value,
                    match => match.Groups["value"].Value,
                    StringComparer.Ordinal);
            var parser = new Novels.StoryCommands.Entity();
            var reported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var rawLine in File.ReadLines(path))
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal))
                    continue;
                var result = parser.Parse(line, false);
                if (!result.IsSuccess)
                    continue;
                if (result.Command is Novels.StoryCommands.BackgroundStoryCommand background)
                {
                    var assetName = ResolveVariable(background.Data.AssetName, variables);
                    var assetPath = LocationPath(prefix, assetName);
                    if (!string.IsNullOrEmpty(assetPath)
                        && AssetDatabase.LoadAssetAtPath<Sprite>(assetPath) == null
                        && reported.Add(assetPath))
                        errors.Add($"Story background does not exist: {assetPath}");
                }
                else if (result.Command is Novels.StoryCommands.DialogueStoryCommand dialogue)
                {
                    var speaker = ResolveVariable(dialogue.Data.Speaker, variables);
                    if (dialogue.Data.Presentation == Novels.StoryContracts.DialoguePresentation.Narrator
                        || string.Equals(speaker, "Wardrobe", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(speaker, mainCharacter, StringComparison.OrdinalIgnoreCase))
                        continue;
                    var assetPath = CharacterBodyPath(prefix, speaker);
                    if (!string.IsNullOrEmpty(assetPath)
                        && AssetDatabase.LoadAssetAtPath<Sprite>(assetPath) == null
                        && reported.Add(assetPath))
                        errors.Add($"Story character body does not exist: {assetPath}");
                }
            }
        }

        private static void ValidateAudio(
            string prefix,
            Novels.Content.EpisodeDefinition episode,
            string assetName,
            ICollection<string> errors)
        {
            if (episode.Media.SilentAudioIds.Contains(assetName, StringComparer.OrdinalIgnoreCase))
                return;
            var extension = Path.GetExtension(assetName);
            if (extension.Length == 0)
                extension = episode.Media.AudioExtensions.TryGetValue(assetName, out var configured)
                    ? configured
                    : episode.Media.DefaultAudioExtension;
            var path = Path.Combine(
                Application.streamingAssetsPath,
                "NovelsAudio",
                prefix,
                assetName + (Path.GetExtension(assetName).Length == 0 ? extension : string.Empty));
            if (!File.Exists(path))
                errors.Add($"Story audio does not exist: {path}");
        }

        private static string ResolveVariable(
            string value,
            IReadOnlyDictionary<string, string> variables)
        {
            var trimmed = value?.Trim();
            if (trimmed == null || trimmed.Length < 3
                || trimmed[0] != '{' || trimmed[trimmed.Length - 1] != '}')
                return trimmed;
            var key = trimmed.Substring(1, trimmed.Length - 2);
            return variables.TryGetValue(key, out var resolved) ? resolved : trimmed;
        }

        private static string LocationPath(string prefix, string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName) || assetName.StartsWith("{", StringComparison.Ordinal))
                return string.Empty;
            return Novels.ContentAddressing.ContentAddressConvention.LocationImage(
                prefix,
                assetName);
        }

        private static string CharacterBodyPath(string prefix, string speaker)
        {
            if (string.IsNullOrWhiteSpace(speaker) || speaker.StartsWith("{", StringComparison.Ordinal))
                return string.Empty;
            return Novels.ContentAddressing.ContentAddressConvention.CharacterMainBody(
                prefix,
                speaker,
                "View");
        }
    }
}
