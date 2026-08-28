using System;
using System.Collections.Generic;
using System.IO;
using Ink;
using Ink.Runtime;
using Novels.StoryProcessor;
using IOPath = System.IO.Path;

namespace Novels.ContentSdk.Editor
{
    internal readonly struct StorySourceMapBuildResult
    {
        internal StorySourceMapBuildResult(string outputPath, int entryCount)
        {
            OutputPath = outputPath;
            EntryCount = entryCount;
        }

        internal string OutputPath { get; }
        internal int EntryCount { get; }
    }

    internal readonly struct StoryCompilationBuildResult
    {
        internal StoryCompilationBuildResult(
            string compiledPath,
            string sourceMapPath,
            int sourceMapEntryCount)
        {
            CompiledPath = compiledPath;
            SourceMapPath = sourceMapPath;
            SourceMapEntryCount = sourceMapEntryCount;
        }

        internal string CompiledPath { get; }
        internal string SourceMapPath { get; }
        internal int SourceMapEntryCount { get; }
    }

    internal static class StorySourceMapBuilder
    {
        private const string _compiledSuffix = ".ink.json";
        private const string _sourceMapSuffix = ".source-map.json";

        internal static StorySourceMapBuildResult Build(string compiledPath)
        {
            if (string.IsNullOrWhiteSpace(compiledPath)
                || !compiledPath.EndsWith(
                    _compiledSuffix,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Compiled story path must end with '.ink.json'.",
                    nameof(compiledPath));
            }

            var sourcePath = compiledPath.Substring(
                0,
                compiledPath.Length - ".json".Length);
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException(
                    "Root Ink source for source-map generation is missing.",
                    sourcePath);
            }

            var story = Compile(sourcePath);
            var entries = SourceMapEntries(story);
            var outputPath = compiledPath + _sourceMapSuffix;
            WriteAtomically(
                outputPath,
                new StorySourceMap(entries.ToArray()).ToJson());
            return new StorySourceMapBuildResult(outputPath, entries.Count);
        }

        internal static StoryCompilationBuildResult CompileArtifacts(
            string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath)
                || !sourcePath.EndsWith(".ink", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Root story path must end with '.ink'.",
                    nameof(sourcePath));
            }
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("Root Ink source is missing.", sourcePath);

            var story = Compile(sourcePath);
            var entries = SourceMapEntries(story);
            var compiledPath = sourcePath + ".json";
            var sourceMapPath = compiledPath + _sourceMapSuffix;
            WritePairSafely(
                compiledPath,
                story.ToJson(),
                sourceMapPath,
                new StorySourceMap(entries.ToArray()).ToJson());
            return new StoryCompilationBuildResult(
                compiledPath,
                sourceMapPath,
                entries.Count);
        }

        private static Ink.Runtime.Story Compile(string sourcePath)
        {
            var errors = new List<string>();
            var sourceDirectory = IOPath.GetDirectoryName(sourcePath)
                ?? string.Empty;
            var compiler = new Compiler(
                File.ReadAllText(sourcePath),
                new Compiler.Options
                {
                    countAllVisits = true,
                    sourceFilename = sourcePath,
                    fileHandler = new RelativeInkFileHandler(sourceDirectory),
                    errorHandler = (message, type) =>
                    {
                        if (type == ErrorType.Error)
                            errors.Add(message);
                    },
                });
            var story = compiler.Compile();
            if (story == null || errors.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Ink compilation failed for '{sourcePath}':\n"
                    + string.Join("\n", errors));
            }
            return story;
        }

        private static List<StorySourceMap.Entry> SourceMapEntries(
            Ink.Runtime.Story story)
        {
            var entries = new List<StorySourceMap.Entry>();
            Visit(
                story.mainContentContainer,
                new HashSet<Ink.Runtime.Object>(),
                entries);
            return entries;
        }

        private static void Visit(
            Ink.Runtime.Object value,
            ISet<Ink.Runtime.Object> visited,
            ICollection<StorySourceMap.Entry> entries)
        {
            if (value == null || !visited.Add(value))
                return;
            var metadata = value.debugMetadata;
            var runtimePath = value.path?.ToString();
            if (metadata != null
                && metadata.startLineNumber > 0
                && !string.IsNullOrEmpty(runtimePath))
            {
                entries.Add(new StorySourceMap.Entry
                {
                    Path = runtimePath,
                    FileName = IOPath.GetFileName(metadata.fileName),
                    LineNumber = metadata.startLineNumber,
                });
            }

            if (!(value is Container container))
                return;
            foreach (var child in container.content)
                Visit(child, visited, entries);
            if (container.namedOnlyContent == null)
                return;
            foreach (var child in container.namedOnlyContent.Values)
                Visit(child, visited, entries);
        }

        private static void WriteAtomically(string outputPath, string contents)
        {
            var temporaryPath = outputPath + ".tmp";
            try
            {
                File.WriteAllText(temporaryPath, contents);
                if (File.Exists(outputPath))
                    File.Replace(temporaryPath, outputPath, null);
                else
                    File.Move(temporaryPath, outputPath);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
        }

        private static void WritePairSafely(
            string firstPath,
            string firstContents,
            string secondPath,
            string secondContents)
        {
            var firstTemporary = firstPath + ".tmp";
            var secondTemporary = secondPath + ".tmp";
            var firstBackup = firstPath + ".backup.tmp";
            var secondBackup = secondPath + ".backup.tmp";
            var firstExisted = File.Exists(firstPath);
            var secondExisted = File.Exists(secondPath);
            try
            {
                File.WriteAllText(firstTemporary, firstContents);
                File.WriteAllText(secondTemporary, secondContents);
                if (firstExisted)
                    File.Copy(firstPath, firstBackup, true);
                if (secondExisted)
                    File.Copy(secondPath, secondBackup, true);
                ReplaceOrMove(firstTemporary, firstPath);
                ReplaceOrMove(secondTemporary, secondPath);
            }
            catch
            {
                Restore(firstPath, firstBackup, firstExisted);
                Restore(secondPath, secondBackup, secondExisted);
                throw;
            }
            finally
            {
                DeleteIfExists(firstTemporary);
                DeleteIfExists(secondTemporary);
                DeleteIfExists(firstBackup);
                DeleteIfExists(secondBackup);
            }
        }

        private static void ReplaceOrMove(string temporaryPath, string outputPath)
        {
            if (File.Exists(outputPath))
                File.Replace(temporaryPath, outputPath, null);
            else
                File.Move(temporaryPath, outputPath);
        }

        private static void Restore(
            string outputPath,
            string backupPath,
            bool outputExisted)
        {
            if (outputExisted && File.Exists(backupPath))
                File.Copy(backupPath, outputPath, true);
            else if (!outputExisted)
                DeleteIfExists(outputPath);
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        private sealed class RelativeInkFileHandler : IFileHandler
        {
            private readonly string _rootDirectory;

            internal RelativeInkFileHandler(string rootDirectory)
            {
                _rootDirectory = rootDirectory;
            }

            public string ResolveInkFilename(string includeName) =>
                IOPath.GetFullPath(IOPath.Combine(_rootDirectory, includeName));

            public string LoadInkFileContents(string fullFilename) =>
                File.ReadAllText(fullFilename);
        }
    }
}
