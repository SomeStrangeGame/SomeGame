using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Novels.ContentSdk.Editor
{
    public static class AtomicContentBuild
    {
        [MenuItem("Novels/Content/Validate")]
        public static void Validate()
        {
            CompileAuthoringStory();
            ContentPipeline.Validate();
        }

        [MenuItem("Novels/Content/Build/Editor")]
        public static void BuildEditor()
        {
            CompileAuthoringStory();
            ContentPipeline.Build("editor");
        }

        [MenuItem("Novels/Content/Build/Windows")]
        public static void BuildWindows()
        {
            CompileAuthoringStory();
            ContentPipeline.Build("windows");
        }

        public static void BuildLocal()
        {
            CompileAuthoringStory();
            ContentPipeline.Build(Argument("-contentPlatform", "editor"));
        }

        private static void CompileAuthoringStory()
        {
            var definitions = AssetDatabase
                .FindAssets("t:NovelContentAsset", new[] {"Assets"})
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Content.NovelContentAsset>)
                .Where(value => value != null)
                .ToArray();
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            var isCatalog = !string.IsNullOrWhiteSpace(projectRoot)
                && File.Exists(Path.Combine(projectRoot, "Config", "catalog.json"));
            if (isCatalog && definitions.Length == 0)
                return;
            if (definitions.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Expected one story definition, found {definitions.Length}.");
            }

            var sourcePath = StoryChunkAuthoring.RootInkPath(definitions[0]);
            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new InvalidOperationException("Root Ink source is unavailable.");
            StoryInkAuthoring.Compile(definitions[0], sourcePath);
        }

        private static string Argument(string name, string fallback)
        {
            var arguments = Environment.GetCommandLineArgs();
            var index = Array.IndexOf(arguments, name);
            return index >= 0 && index + 1 < arguments.Length
                ? arguments[index + 1]
                : fallback;
        }
    }
}
