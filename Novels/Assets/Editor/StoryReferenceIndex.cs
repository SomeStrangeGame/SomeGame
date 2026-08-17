using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Editor
{
    internal sealed class StoryReferenceIndex
    {
        private static readonly Regex _compiledString = new(
            "\"\\^(?<text>(?:\\\\.|[^\"\\\\])*)\"");
        private static readonly Regex _variable = new(
            "^\\s*VAR\\s+(?<name>[^=\\s]+)\\s*=\\s*\"(?<value>[^\"]*)\"",
            RegexOptions.Multiline);

        private StoryReferenceIndex(
            IEnumerable<string> audioIds,
            IEnumerable<string> backgrounds,
            IEnumerable<string> speakers,
            IEnumerable<string> errors)
        {
            AudioIds = Array.AsReadOnly(audioIds.Distinct(
                StringComparer.OrdinalIgnoreCase).ToArray());
            Backgrounds = Array.AsReadOnly(backgrounds.Distinct(
                StringComparer.OrdinalIgnoreCase).ToArray());
            Speakers = Array.AsReadOnly(speakers.Distinct(
                StringComparer.OrdinalIgnoreCase).ToArray());
            Errors = Array.AsReadOnly(errors.ToArray());
        }

        internal IReadOnlyList<string> AudioIds { get; }
        internal IReadOnlyList<string> Backgrounds { get; }
        internal IReadOnlyList<string> Speakers { get; }
        internal IReadOnlyList<string> Errors { get; }

        internal static StoryReferenceIndex Build(
            string prefix,
            Novels.Content.EpisodeDefinition episode)
        {
            var compiledPath = Path.Combine(
                Application.streamingAssetsPath,
                "NovelTexts",
                prefix,
                episode.StoryPath);
            var audio = new List<string>();
            var backgrounds = new List<string>();
            var speakers = new List<string>();
            var errors = new List<string>();
            if (!File.Exists(compiledPath))
            {
                errors.Add($"Compiled Ink story does not exist: {compiledPath}");
                return new StoryReferenceIndex(audio, backgrounds, speakers, errors);
            }

            var parser = new Novels.StoryCommands.Entity();
            var json = File.ReadAllText(compiledPath);
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
                else if (result.Command is Novels.StoryCommands.AudioStoryCommand command)
                {
                    audio.Add(command.Data.AssetName);
                }
            }

            var sourcePath = Path.ChangeExtension(compiledPath, ".ink");
            if (!File.Exists(sourcePath))
                return new StoryReferenceIndex(audio, backgrounds, speakers, errors);

            var sourceText = File.ReadAllText(sourcePath);
            var variables = _variable.Matches(sourceText)
                .Cast<Match>()
                .GroupBy(match => match.Groups["name"].Value, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.Last().Groups["value"].Value,
                    StringComparer.Ordinal);
            foreach (var rawLine in File.ReadLines(sourcePath))
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal))
                    continue;
                var result = parser.Parse(line, false);
                if (!result.IsSuccess)
                    continue;
                if (result.Command is Novels.StoryCommands.BackgroundStoryCommand background)
                    backgrounds.Add(ResolveVariable(background.Data.AssetName, variables));
                else if (result.Command is Novels.StoryCommands.DialogueStoryCommand dialogue
                    && dialogue.Data.Presentation
                        != Novels.StoryContracts.DialoguePresentation.Narrator)
                    speakers.Add(ResolveVariable(dialogue.Data.Speaker, variables));
            }
            return new StoryReferenceIndex(audio, backgrounds, speakers, errors);
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
    }
}
