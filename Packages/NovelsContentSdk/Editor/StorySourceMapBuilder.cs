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
                    $"Ink source-map compilation failed for '{sourcePath}':\n"
                    + string.Join("\n", errors));
            }

            var entries = new List<StorySourceMap.Entry>();
            Visit(
                story.mainContentContainer,
                new HashSet<Ink.Runtime.Object>(),
                entries);
            var outputPath = compiledPath + _sourceMapSuffix;
            WriteAtomically(
                outputPath,
                new StorySourceMap(entries.ToArray()).ToJson());
            return new StorySourceMapBuildResult(outputPath, entries.Count);
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
