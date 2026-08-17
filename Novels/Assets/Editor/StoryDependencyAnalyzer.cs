using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Editor
{
    internal static class StoryDependencyAnalyzer
    {
        private static readonly Regex _compiledString = new(
            "\"\\^(?<text>(?:\\\\.|[^\"\\\\])*)\"");
        private static readonly Regex _variable = new(
            "^\\s*VAR\\s+(?<name>[^=\\s]+)\\s*=\\s*\"(?<value>[^\"]*)\"",
            RegexOptions.Multiline);

        internal static StoryDependencyManifest Build(
            string prefix,
            Novels.Content.EpisodeDefinition episode)
        {
            var compiledPath = Path.Combine(
                Application.streamingAssetsPath,
                "NovelTexts",
                prefix,
                episode.StoryPath);
            var audio = new List<string>(episode.Dependencies.AudioIds);
            var backgrounds = new List<string>(episode.Dependencies.BackgroundIds);
            var speakers = new List<string>(episode.Dependencies.SpeakerIds);
            var cameraActions = new List<Novels.StoryContracts.StoryCameraAction>();
            var issues = new List<ContentValidationIssue>();
            if (!File.Exists(compiledPath))
            {
                issues.Add(ContentValidationIssue.Error(
                    ContentValidationCodes.StoryCompiledFileMissing,
                    $"Compiled Ink story does not exist: {compiledPath}",
                    compiledPath,
                    episode.ContentId,
                    episode.Id));
                return CreateManifest();
            }

            var parser = new Novels.StoryCommands.Entity();
            AnalyzeCompiledStory(
                File.ReadAllText(compiledPath),
                episode,
                parser,
                audio,
                cameraActions,
                issues);

            var sourcePath = Path.ChangeExtension(compiledPath, ".ink");
            if (File.Exists(sourcePath))
            {
                AnalyzeSourceStory(
                    sourcePath,
                    parser,
                    backgrounds,
                    speakers,
                    cameraActions);
            }
            return CreateManifest();

            StoryDependencyManifest CreateManifest() => new(
                audio,
                backgrounds,
                speakers,
                cameraActions,
                issues);
        }

        private static void AnalyzeCompiledStory(
            string json,
            Novels.Content.EpisodeDefinition episode,
            Novels.StoryCommands.Entity parser,
            ICollection<string> audio,
            ICollection<Novels.StoryContracts.StoryCameraAction> cameraActions,
            ICollection<ContentValidationIssue> issues)
        {
            foreach (Match match in _compiledString.Matches(json))
            {
                var source = Regex.Unescape(match.Groups["text"].Value);
                if (!source.Contains(":"))
                    continue;
                var result = parser.Parse(source, false);
                if (!result.IsSuccess)
                {
                    issues.Add(ContentValidationIssue.Error(
                        result.Error.Code,
                        $"Story command in '{episode.StoryPath}': "
                        + $"{result.Error.Message} Source: {source}",
                        episode.StoryPath,
                        episode.ContentId,
                        episode.Id));
                }
                else if (result.Command is Novels.StoryCommands.AudioStoryCommand command)
                {
                    audio.Add(command.Data.AssetName);
                }
                else if (result.Command is Novels.StoryCommands.CameraStoryCommand camera)
                {
                    cameraActions.Add(camera.Data.Action);
                }
            }
        }

        private static void AnalyzeSourceStory(
            string sourcePath,
            Novels.StoryCommands.Entity parser,
            ICollection<string> backgrounds,
            ICollection<string> speakers,
            ICollection<Novels.StoryContracts.StoryCameraAction> cameraActions)
        {
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
                else if (result.Command is Novels.StoryCommands.CameraStoryCommand camera)
                    cameraActions.Add(camera.Data.Action);
            }
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
