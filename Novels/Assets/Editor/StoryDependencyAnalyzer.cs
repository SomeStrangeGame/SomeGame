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
        private static readonly Regex _variableValue = new(
            "^\\s*(?:VAR\\s+|~\\s*)(?<name>[^=\\s]+)\\s*=\\s*\"(?<value>[^\"]*)\"",
            RegexOptions.None);

        private readonly struct ResolvedStoryValue
        {
            internal ResolvedStoryValue(string value, StorySourceLine sourceLine)
            {
                Value = value ?? string.Empty;
                SourceLine = sourceLine;
            }

            internal string Value { get; }
            internal StorySourceLine SourceLine { get; }
        }

        internal static StoryDependencyManifest Build(
            string prefix,
            string mainCharacter,
            Novels.Content.EpisodeDefinition episode)
        {
            var compiledPath = Path.Combine(
                Application.streamingAssetsPath,
                "noveltexts",
                prefix,
                episode.StoryPath);
            var dependencies = new List<StoryDependencyReference>();
            var cameras = new List<StoryCameraReference>();
            var characterAssets = new List<StoryCharacterAssetReference>();
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

            var sourcePath = string.IsNullOrWhiteSpace(episode.SourcePath)
                ? StoryFileConvention.GetSourcePath(compiledPath)
                : Path.Combine(
                    Application.streamingAssetsPath,
                    "noveltexts",
                    prefix,
                    episode.SourcePath);
            var source = StorySourceGraph.Load(sourcePath, episode, issues);
            if (source.Lines.Count > 0)
            {
                var variableSource = source;
                var rootSourcePath = StoryFileConvention.GetSourcePath(compiledPath);
                if (!string.Equals(
                        Path.GetFullPath(sourcePath),
                        Path.GetFullPath(rootSourcePath),
                        StringComparison.OrdinalIgnoreCase)
                    && File.Exists(rootSourcePath))
                {
                    variableSource = StorySourceGraph.Load(
                        rootSourcePath,
                        episode,
                        new List<ContentValidationIssue>());
                }
                AnalyzeSourceStory(
                    source,
                    variableSource,
                    episode,
                    mainCharacter,
                    new Novels.StoryCommands.Entity(),
                    dependencies,
                    cameras,
                    characterAssets,
                    issues);
            }
            return CreateManifest();

            StoryDependencyManifest CreateManifest() => new(
                dependencies,
                cameras,
                characterAssets,
                issues);
        }

        private static void AnalyzeSourceStory(
            StorySourceGraph source,
            StorySourceGraph variableSource,
            Novels.Content.EpisodeDefinition episode,
            string mainCharacter,
            Novels.StoryCommands.Entity parser,
            ICollection<StoryDependencyReference> dependencies,
            ICollection<StoryCameraReference> cameras,
            ICollection<StoryCharacterAssetReference> characterAssets,
            ICollection<ContentValidationIssue> issues)
        {
            var variables = variableSource.Lines
                .Select(line => (line, match: _variableValue.Match(line.Text)))
                .Where(value => value.match.Success)
                .GroupBy(
                    value => value.match.Groups["name"].Value,
                    StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(value => new ResolvedStoryValue(
                            value.match.Groups["value"].Value.Trim(),
                            value.line))
                        .Where(value => value.Value.Length > 0)
                        .GroupBy(value => value.Value, StringComparer.Ordinal)
                        .Select(values => values.First())
                        .ToArray(),
                    StringComparer.Ordinal);

            var storyBodyStarted = false;
            foreach (var sourceLine in source.Lines)
            {
                var line = sourceLine.Text.Trim();
                if (line.StartsWith("===", StringComparison.Ordinal))
                {
                    storyBodyStarted = true;
                    continue;
                }
                if (!storyBodyStarted || IsInkControlLine(line))
                    continue;
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
                else if (result.Command is Novels.StoryCommands.DialogueStoryCommand dialogue)
                {
                    AddDialogueReferences(dialogue, sourceLine);
                }
                else if (result.Command is Novels.StoryCommands.CameraStoryCommand camera)
                {
                    cameras.Add(new StoryCameraReference(
                        camera.Data.Action,
                        sourceLine.SourcePath,
                        sourceLine.LineNumber,
                        sourceLine.Text));
                }

                void AddDialogueReferences(
                    Novels.StoryCommands.DialogueStoryCommand command,
                    StorySourceLine authoredLine)
                {
                    if (command.Data.Character.HasUnsupportedTimedChoice)
                    {
                        issues.Add(ContentValidationIssue.Warning(
                            ContentValidationCodes.StoryTimedChoiceUnsupported,
                            "Timed choice is not implemented and will be ignored at runtime. "
                            + $"Referenced at {authoredLine.Location}.",
                            authoredLine.SourcePath,
                            episode.ContentId,
                            episode.Id));
                    }

                    var resolvedSpeakers = Resolve(
                        command.Data.Speaker,
                        variables,
                        authoredLine);
                    if (resolvedSpeakers.Count == 0)
                    {
                        AddUnresolvedIssue("speaker", authoredLine, episode, issues);
                        return;
                    }
                    foreach (var resolvedSpeaker in resolvedSpeakers)
                    {
                        var role = Novels.StoryContracts.StorySpeakerRoleResolver.Resolve(
                            resolvedSpeaker.Value,
                            command.Data.Presentation,
                            mainCharacter);
                        if (Novels.StoryContracts.StorySpeakerRoleResolver
                                .RequiresCharacterAsset(role)
                            && Novels.BubbleContracts.BubbleTriggers.Resolve(
                                resolvedSpeaker.Value)
                                == Novels.BubbleContracts.BubblePresentationKind.Dialogue)
                        {
                            dependencies.Add(new StoryDependencyReference(
                                StoryDependencyKind.Speaker,
                                resolvedSpeaker.Value,
                                resolvedSpeaker.SourceLine.SourcePath,
                                resolvedSpeaker.SourceLine.LineNumber,
                                resolvedSpeaker.SourceLine.Text));
                        }
                        if (role != Novels.StoryContracts.StorySpeakerRole.Character
                            && role != Novels.StoryContracts.StorySpeakerRole.MainCharacter)
                        {
                            continue;
                        }
                        if (command.Data.Character.IsChild)
                        {
                            characterAssets.Add(new StoryCharacterAssetReference(
                                resolvedSpeaker.Value,
                                role,
                                string.Empty,
                                true,
                                resolvedSpeaker.SourceLine.SourcePath,
                                resolvedSpeaker.SourceLine.LineNumber,
                                resolvedSpeaker.SourceLine.Text));
                        }
                        foreach (var candidate in command.Data.Character.AssetCandidates)
                        {
                            var resolvedCandidates = Resolve(
                                candidate,
                                variables,
                                authoredLine);
                            if (resolvedCandidates.Count == 0)
                            {
                                AddUnresolvedIssue(
                                    "character asset",
                                    authoredLine,
                                    episode,
                                    issues);
                                continue;
                            }
                            foreach (var resolvedCandidate in resolvedCandidates)
                            {
                                characterAssets.Add(new StoryCharacterAssetReference(
                                    resolvedSpeaker.Value,
                                    role,
                                    resolvedCandidate.Value,
                                    command.Data.Character.IsChild,
                                    resolvedCandidate.SourceLine.SourcePath,
                                    resolvedCandidate.SourceLine.LineNumber,
                                    resolvedCandidate.SourceLine.Text));
                            }
                        }
                    }
                }
            }
        }

        private static IReadOnlyList<ResolvedStoryValue> Resolve(
            string value,
            IReadOnlyDictionary<string, ResolvedStoryValue[]> variables,
            StorySourceLine sourceLine)
        {
            var resolved = value?.Trim();
            if (!IsVariableReference(resolved))
            {
                return string.IsNullOrWhiteSpace(resolved)
                    ? Array.Empty<ResolvedStoryValue>()
                    : new[] { new ResolvedStoryValue(resolved, sourceLine) };
            }

            var key = resolved.Substring(1, resolved.Length - 2);
            return variables.TryGetValue(key, out var values)
                ? values
                : Array.Empty<ResolvedStoryValue>();
        }

        private static void AddResolvedReference(
            StoryDependencyKind kind,
            string value,
            StorySourceLine sourceLine,
            IReadOnlyDictionary<string, ResolvedStoryValue[]> variables,
            ICollection<StoryDependencyReference> target,
            Novels.Content.EpisodeDefinition episode,
            ICollection<ContentValidationIssue> issues)
        {
            if (kind == StoryDependencyKind.Audio && string.IsNullOrWhiteSpace(value))
                return;

            var resolvedValues = Resolve(value, variables, sourceLine);
            if (resolvedValues.Count == 0)
            {
                AddUnresolvedIssue(
                    kind.ToString().ToLowerInvariant(),
                    sourceLine,
                    episode,
                    issues);
                return;
            }
            foreach (var resolved in resolvedValues)
            {
                target.Add(new StoryDependencyReference(
                    kind,
                    resolved.Value,
                    resolved.SourceLine.SourcePath,
                    resolved.SourceLine.LineNumber,
                    resolved.SourceLine.Text));
            }
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

        private static bool IsInkControlLine(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;
            return value[0] == '-'
                || value[0] == '*'
                || value[0] == '+'
                || value[0] == '~'
                || value[0] == '#'
                || value[0] == '{'
                || value[0] == '}'
                || value.StartsWith("->", StringComparison.Ordinal);
        }
    }
}
