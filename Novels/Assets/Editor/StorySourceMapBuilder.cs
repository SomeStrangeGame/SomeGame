using System;
using System.Collections.Generic;
using System.IO;
using Ink;
using Ink.Runtime;
using Novels.StoryProcessor;
using IOPath = System.IO.Path;

namespace Editor
{
    internal static class StorySourceMapBuilder
    {
        internal const string FileSuffix = ".source-map.json";

        internal static void Build(ContentProjectIndex project)
        {
            var generated = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in project.Entries)
            {
                foreach (var episode in item.Definition.Episodes)
                {
                    var compiledPath = IOPath.Combine(
                        UnityEngine.Application.streamingAssetsPath,
                        "noveltexts",
                        item.Definition.Prefix,
                        episode.StoryPath);
                    if (!generated.Add(compiledPath))
                        continue;
                    Build(compiledPath);
                }
            }
        }

        private static void Build(string compiledPath)
        {
            var sourcePath = StoryFileConvention.GetSourcePath(compiledPath);
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("Ink source map source is missing.", sourcePath);

            var errors = new List<string>();
            var compiler = new Compiler(
                File.ReadAllText(sourcePath),
                new Compiler.Options
                {
                    countAllVisits = true,
                    sourceFilename = sourcePath,
                    fileHandler = new RelativeInkFileHandler(
                        IOPath.GetDirectoryName(sourcePath) ?? string.Empty),
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
                    $"Ink source map compilation failed for '{sourcePath}':\n"
                    + string.Join("\n", errors));
            }

            var entries = new List<StorySourceMap.Entry>();
            var visited = new HashSet<Ink.Runtime.Object>();
            Visit(story.mainContentContainer, visited, entries);
            File.WriteAllText(
                compiledPath + FileSuffix,
                new StorySourceMap(entries.ToArray()).ToJson());
        }

        private static void Visit(
            Ink.Runtime.Object value,
            ISet<Ink.Runtime.Object> visited,
            ICollection<StorySourceMap.Entry> entries)
        {
            if (value == null || !visited.Add(value))
                return;
            var metadata = value.debugMetadata;
            var path = value.path?.ToString();
            if (metadata != null && metadata.startLineNumber > 0 && !string.IsNullOrEmpty(path))
            {
                entries.Add(new StorySourceMap.Entry
                {
                    Path = path,
                    FileName = IOPath.GetFileName(metadata.fileName),
                    LineNumber = metadata.startLineNumber,
                });
            }

            if (!(value is Container container))
                return;
            foreach (var child in container.content)
                Visit(child, visited, entries);
            var namedOnly = container.namedOnlyContent;
            if (namedOnly == null)
                return;
            foreach (var child in namedOnly.Values)
                Visit(child, visited, entries);
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
