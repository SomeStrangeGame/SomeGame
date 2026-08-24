using System;
using UnityEditor;

namespace Novels.ContentSdk.Editor
{
    public static class AtomicContentBuild
    {
        [MenuItem("Novels/Content/Validate")]
        public static void Validate() => ContentPipeline.Validate();

        [MenuItem("Novels/Content/Build/Editor")]
        public static void BuildEditor() => ContentPipeline.Build("editor");

        public static void BuildLocal() => ContentPipeline.Build(
            Argument("-contentPlatform", "editor"));

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
