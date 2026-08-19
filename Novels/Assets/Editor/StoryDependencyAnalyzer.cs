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

    internal readonly struct StorySourceLine
    {
        internal StorySourceLine(string sourcePath, int lineNumber, string text)
        {
            SourcePath = sourcePath;
            LineNumber = lineNumber;
            Text = text ?? string.Empty;
        }

        internal string SourcePath { get; }
        internal int LineNumber { get; }
        internal string Text { get; }
        internal string Location => $"{SourcePath}:{LineNumber}";
    }

    internal sealed class StorySourceGraph
    {
        private static readonly Regex _include = new(
            "^\\s*INCLUDE\\s+(?<path>.+?)\\s*$",
            RegexOptions.IgnoreCase);

        private StorySourceGraph(IList<StorySourceLine> lines)
        {
            Lines = Array.AsReadOnly(lines.ToArray());
        }

        internal IReadOnlyList<StorySourceLine> Lines { get; }

        internal static StorySourceGraph Load(
            string rootPath,
            Novels.Content.EpisodeDefinition episode,
            ICollection<ContentValidationIssue> issues)
        {
            var lines = new List<StorySourceLine>();
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Visit(Path.GetFullPath(rootPath), true);
            return new StorySourceGraph(lines);

            void Visit(string sourcePath, bool isRoot)
            {
                sourcePath = Path.GetFullPath(sourcePath);
                if (visited.Contains(sourcePath))
                    return;
                if (!visiting.Add(sourcePath))
                {
                    issues.Add(ContentValidationIssue.Error(
                        ContentValidationCodes.StorySourceIncludeCycle,
                        $"Ink INCLUDE cycle contains: {sourcePath}",
                        sourcePath,
                        episode.ContentId,
                        episode.Id));
                    return;
                }
                if (!File.Exists(sourcePath))
                {
                    issues.Add(ContentValidationIssue.Error(
                        isRoot
                            ? ContentValidationCodes.StorySourceFileMissing
                            : ContentValidationCodes.StorySourceIncludeMissing,
                        isRoot
                            ? $"Ink source story does not exist: {sourcePath}"
                            : $"Included Ink source does not exist: {sourcePath}",
                        sourcePath,
                        episode.ContentId,
                        episode.Id));
                    visiting.Remove(sourcePath);
                    return;
                }

                try
                {
                    var lineNumber = 0;
                    foreach (var text in File.ReadLines(sourcePath))
                    {
                        lineNumber++;
                        var include = _include.Match(text);
                        if (!include.Success)
                        {
                            lines.Add(new StorySourceLine(sourcePath, lineNumber, text));
                            continue;
                        }
                        var includePath = include.Groups["path"].Value.Trim().Trim('"');
                        var directory = Path.GetDirectoryName(sourcePath) ?? string.Empty;
                        Visit(Path.Combine(directory, includePath), false);
                    }
                }
                catch (Exception exception) when (
                    exception is IOException || exception is UnauthorizedAccessException)
                {
                    issues.Add(ContentValidationIssue.Error(
                        ContentValidationCodes.StorySourceReadFailed,
                        $"Could not read Ink source '{sourcePath}': {exception.Message}",
                        sourcePath,
                        episode.ContentId,
                        episode.Id));
                }
                visiting.Remove(sourcePath);
                visited.Add(sourcePath);
            }
        }
    }

    internal static class StoryDependencyAnalyzer
    {
        private static readonly Regex _variable = new(
            "^\\s*VAR\\s+(?<name>[^=\\s]+)\\s*=\\s*\"(?<value>[^\"]*)\"",
            RegexOptions.None);

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
            var dependencies = new List<StoryDependencyReference>();
            var cameras = new List<StoryCameraReference>();
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

            var sourcePath = StoryFileConvention.GetSourcePath(compiledPath);
            var source = StorySourceGraph.Load(sourcePath, episode, issues);
            if (source.Lines.Count > 0)
            {
                AnalyzeSourceStory(
                    source,
                    episode,
                    mainCharacter,
                    new Novels.StoryCommands.Entity(),
                    dependencies,
                    cameras,
                    issues);
            }
            return CreateManifest();

            StoryDependencyManifest CreateManifest() => new(
                dependencies,
                cameras,
                issues);
        }

        private static void AnalyzeSourceStory(
            StorySourceGraph source,
            Novels.Content.EpisodeDefinition episode,
            string mainCharacter,
            Novels.StoryCommands.Entity parser,
            ICollection<StoryDependencyReference> dependencies,
            ICollection<StoryCameraReference> cameras,
            ICollection<ContentValidationIssue> issues)
        {
            var variables = source.Lines
                .Select(line => (line, match: _variable.Match(line.Text)))
                .Where(value => value.match.Success)
                .GroupBy(
                    value => value.match.Groups["name"].Value,
                    StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.Last().match.Groups["value"].Value,
                    StringComparer.Ordinal);

            foreach (var sourceLine in source.Lines)
            {
                var line = sourceLine.Text.Trim();
                if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal))
                    continue;
                if (!line.Contains(":"))
                    continue;
                var result = parser.Parse(line, false);
                if (!result.IsSuccess)
                {
                    issues.Add(ContentValidationIssue.Error(
                        ContentValidationCodes.StoryCommandInvalid,
                        $"Ink command is invalid at {sourceLine.Location}: "
                        + $"[{result.Error.Code}] {result.Error.Message} Source: {line}",
                        sourceLine.SourcePath,
                        episode.ContentId,
                        episode.Id));
                    continue;
                }
                if (result.Command is Novels.StoryCommands.BackgroundStoryCommand background)
                {
                    AddResolvedReference(
                        StoryDependencyKind.Background,
                        background.Data.AssetName,
                        sourceLine,
                        variables,
                        dependencies,
                        episode,
                        issues);
                }
                else if (result.Command is Novels.StoryCommands.AudioStoryCommand audio)
                {
                    AddResolvedReference(
                        StoryDependencyKind.Audio,
                        audio.Data.AssetName,
                        sourceLine,
                        variables,
                        dependencies,
                        episode,
                        issues);
                }
                else if (result.Command is Novels.StoryCommands.DialogueStoryCommand dialogue
                    && IsVariableReference(dialogue.Data.Speaker))
                {
                    AddSpeakerReference(dialogue, sourceLine);
                }
                else if (result.Command is Novels.StoryCommands.CameraStoryCommand camera)
                {
                    cameras.Add(new StoryCameraReference(
                        camera.Data.Action,
                        sourceLine.SourcePath,
                        sourceLine.LineNumber,
                        sourceLine.Text));
                }

                void AddSpeakerReference(
                    Novels.StoryCommands.DialogueStoryCommand command,
                    StorySourceLine authoredLine)
                {
                    var reference = command.Data.Speaker.Trim();
                    var key = reference.Substring(1, reference.Length - 2);
                    if (!variables.TryGetValue(key, out var resolved)
                        || string.IsNullOrWhiteSpace(resolved))
                    {
                        AddUnresolvedIssue("speaker", authoredLine, episode, issues);
                        return;
                    }
                    resolved = resolved.Trim();
                    var role = Novels.StoryContracts.StorySpeakerRoleResolver.Resolve(
                        resolved,
                        command.Data.Presentation,
                        mainCharacter);
                    if (Novels.StoryContracts.StorySpeakerRoleResolver
                            .RequiresCharacterAsset(role)
                        && Novels.BubbleContracts.BubbleTriggers.Resolve(resolved)
                            == Novels.BubbleContracts.BubblePresentationKind.Dialogue)
                    {
                        dependencies.Add(new StoryDependencyReference(
                            StoryDependencyKind.Speaker,
                            resolved,
                            authoredLine.SourcePath,
                            authoredLine.LineNumber,
                            authoredLine.Text));
                    }
                }
            }
        }

        private static void AddResolvedReference(
            StoryDependencyKind kind,
            string value,
            StorySourceLine sourceLine,
            IReadOnlyDictionary<string, string> variables,
            ICollection<StoryDependencyReference> target,
            Novels.Content.EpisodeDefinition episode,
            ICollection<ContentValidationIssue> issues)
        {
            var resolved = value?.Trim();
            if (IsVariableReference(resolved))
            {
                var key = resolved.Substring(1, resolved.Length - 2);
                if (!variables.TryGetValue(key, out resolved)
                    || string.IsNullOrWhiteSpace(resolved))
                {
                    AddUnresolvedIssue(
                        kind.ToString().ToLowerInvariant(),
                        sourceLine,
                        episode,
                        issues);
                    return;
                }
                resolved = resolved.Trim();
            }
            if (string.IsNullOrEmpty(resolved))
            {
                AddUnresolvedIssue(
                    kind.ToString().ToLowerInvariant(),
                    sourceLine,
                    episode,
                    issues);
                return;
            }
            target.Add(new StoryDependencyReference(
                kind,
                resolved,
                sourceLine.SourcePath,
                sourceLine.LineNumber,
                sourceLine.Text));
        }

        private static void AddUnresolvedIssue(
            string kind,
            StorySourceLine sourceLine,
            Novels.Content.EpisodeDefinition episode,
            ICollection<ContentValidationIssue> issues) =>
            issues.Add(ContentValidationIssue.Error(
                ContentValidationCodes.StoryResourceUnresolved,
                $"Ink {kind} reference cannot be resolved statically at "
                + $"{sourceLine.Location}. Source: {sourceLine.Text.Trim()}",
                sourceLine.SourcePath,
                episode.ContentId,
                episode.Id));

        private static bool IsVariableReference(string value) =>
            value != null
            && value.Length >= 3
            && value[0] == '{'
            && value[value.Length - 1] == '}';
    }
}
