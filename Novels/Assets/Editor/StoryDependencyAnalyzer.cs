using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Editor
{
    internal static class StoryFileConvention
    {
        private const string _compiledExtension = ".ink.json";

        internal static string GetStoryName(string path)
        {
            var fileName = Path.GetFileName(path);
            return fileName.EndsWith(
                    _compiledExtension,
                    StringComparison.OrdinalIgnoreCase)
                ? fileName.Substring(0, fileName.Length - _compiledExtension.Length)
                : Path.GetFileNameWithoutExtension(fileName);
        }

        internal static string GetSourcePath(string compiledPath)
        {
            var directory = Path.GetDirectoryName(compiledPath);
            return Path.Combine(
                directory ?? string.Empty,
                GetStoryName(compiledPath) + ".ink");
        }
    }

    internal static class StoryDependencyAnalyzer
    {
        private static readonly Regex _variable = new(
            "^\\s*VAR\\s+(?<name>[^=\\s]+)\\s*=\\s*\"(?<value>[^\"]*)\"",
            RegexOptions.Multiline);

        internal static StoryDependencyManifest Build(
            string prefix,
            string mainCharacter,
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
            var sourcePath = StoryFileConvention.GetSourcePath(compiledPath);
            if (!File.Exists(sourcePath))
            {
                issues.Add(ContentValidationIssue.Error(
                    ContentValidationCodes.StorySourceFileMissing,
                    $"Ink source story does not exist: {sourcePath}",
                    sourcePath,
                    episode.ContentId,
                    episode.Id));
            }
            else
            {
                AnalyzeSourceStory(
                    sourcePath,
                    episode,
                    mainCharacter,
                    parser,
                    audio,
                    backgrounds,
                    speakers,
                    cameraActions,
                    issues);
            }
            return CreateManifest();

            StoryDependencyManifest CreateManifest() => new(
                audio,
                backgrounds,
                speakers,
                cameraActions,
                issues);
        }

        private static void AnalyzeSourceStory(
            string sourcePath,
            Novels.Content.EpisodeDefinition episode,
            string mainCharacter,
            Novels.StoryCommands.Entity parser,
            ICollection<string> audio,
            ICollection<string> backgrounds,
            ICollection<string> speakers,
            ICollection<Novels.StoryContracts.StoryCameraAction> cameraActions,
            ICollection<ContentValidationIssue> issues)
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
                if (!line.Contains(":"))
                    continue;
                var result = parser.Parse(line, false);
                if (!result.IsSuccess)
                    continue;
                if (result.Command is Novels.StoryCommands.BackgroundStoryCommand background)
                {
                    AddResolvedReference(
                        "background",
                        background.Data.AssetName,
                        line,
                        variables,
                        backgrounds,
                        sourcePath,
                        episode,
                        issues);
                }
                else if (result.Command is Novels.StoryCommands.AudioStoryCommand audioCommand)
                {
                    AddResolvedReference(
                        "audio",
                        audioCommand.Data.AssetName,
                        line,
                        variables,
                        audio,
                        sourcePath,
                        episode,
                        issues);
                }
                else if (result.Command is Novels.StoryCommands.DialogueStoryCommand dialogue
                    && IsVariableReference(dialogue.Data.Speaker))
                {
                    AddSpeakerReference(dialogue, mainCharacter);
                }
                else if (result.Command is Novels.StoryCommands.CameraStoryCommand camera)
                    cameraActions.Add(camera.Data.Action);

                void AddSpeakerReference(
                    Novels.StoryCommands.DialogueStoryCommand command,
                    string configuredMainCharacter)
                {
                    var reference = command.Data.Speaker.Trim();
                    var key = reference.Substring(1, reference.Length - 2);
                    if (!variables.TryGetValue(key, out var resolved)
                        || string.IsNullOrWhiteSpace(resolved))
                    {
                        issues.Add(ContentValidationIssue.Error(
                            ContentValidationCodes.StoryResourceUnresolved,
                            $"Ink speaker reference cannot be resolved statically. Source: {line}",
                            sourcePath,
                            episode.ContentId,
                            episode.Id));
                        return;
                    }
                    resolved = resolved.Trim();
                    var role = Novels.StoryContracts.StorySpeakerRoleResolver.Resolve(
                        resolved,
                        command.Data.Presentation,
                        configuredMainCharacter);
                    if (Novels.StoryContracts.StorySpeakerRoleResolver
                            .RequiresCharacterAsset(role)
                        && Novels.BubbleContracts.BubbleTriggers.Resolve(resolved)
                            == Novels.BubbleContracts.BubblePresentationKind.Dialogue)
                    {
                        speakers.Add(resolved);
                    }
                }
            }
        }

        private static void AddResolvedReference(
            string kind,
            string value,
            string sourceLine,
            IReadOnlyDictionary<string, string> variables,
            ICollection<string> target,
            string sourcePath,
            Novels.Content.EpisodeDefinition episode,
            ICollection<ContentValidationIssue> issues)
        {
            var trimmed = value?.Trim();
            if (!IsVariableReference(trimmed))
            {
                if (!string.IsNullOrEmpty(trimmed))
                {
                    target.Add(trimmed);
                    return;
                }
                AddUnresolvedIssue();
                return;
            }

            var key = trimmed.Substring(1, trimmed.Length - 2);
            if (variables.TryGetValue(key, out var resolved)
                && !string.IsNullOrWhiteSpace(resolved))
            {
                target.Add(resolved.Trim());
                return;
            }
            AddUnresolvedIssue();

            void AddUnresolvedIssue() => issues.Add(ContentValidationIssue.Error(
                ContentValidationCodes.StoryResourceUnresolved,
                $"Ink {kind} reference cannot be resolved statically. Source: {sourceLine}",
                sourcePath,
                episode.ContentId,
                episode.Id));
        }

        private static bool IsVariableReference(string value) =>
            value != null
            && value.Length >= 3
            && value[0] == '{'
            && value[value.Length - 1] == '}';
    }
}
