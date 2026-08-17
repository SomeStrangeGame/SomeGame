using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor
{
    public static class NovelCiValidation
    {
        internal static bool IsRemotePlayerBuild { get; private set; }
        public static void ValidateExistingContentBatch()
        {
            EditorSceneManager.OpenScene(
                "Assets/Novels/Novels.unity",
                OpenSceneMode.Single);
            NovelContentValidator.ValidateOrThrow();
            NovelContentValidator.ValidateBuiltOutputOrThrow();
            Debug.Log("Novel CI validation completed without errors.");
        }

        public static void BuildAndValidateContentBatch()
        {
            EditorSceneManager.OpenScene(
                "Assets/Novels/Novels.unity",
                OpenSceneMode.Single);
            CreateAssetBundles.BuildAndroidBundles();
            Debug.Log("Novel CI content build and validation completed without errors.");
        }

        public static void BuildRemotePlayerBatch()
        {
            var arguments = Environment.GetCommandLineArgs();
            var remoteUrl = GetArgument(arguments, "-remoteContentBaseUrl");
            var output = GetArgument(arguments, "-playerOutput");
            if (!Uri.TryCreate(remoteUrl, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp
                    && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new InvalidOperationException(
                    "-remoteContentBaseUrl must be an absolute HTTP(S) URL.");
            }
            if (string.IsNullOrWhiteSpace(output))
                throw new InvalidOperationException("-playerOutput is required.");

            AssertRemoteContentExcluded();
            var scene = EditorSceneManager.OpenScene(
                "Assets/Novels/Novels.unity",
                OpenSceneMode.Single);
            var entryPoint = UnityEngine.Object.FindFirstObjectByType<Novels.EntryPoint>(
                FindObjectsInactive.Include)
                ?? throw new InvalidOperationException("EntryPoint is absent from the scene.");
            var serialized = new SerializedObject(entryPoint);
            serialized.FindProperty("_remoteContentBaseUrl").stringValue =
                uri.AbsoluteUri.TrimEnd('/');
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene);

            var scenes = EditorBuildSettings.scenes
                .Where(value => value.enabled)
                .Select(value => value.path)
                .ToArray();
            BuildReport report;
            IsRemotePlayerBuild = true;
            try
            {
                report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = Path.GetFullPath(output),
                    target = EditorUserBuildSettings.activeBuildTarget,
                    options = BuildOptions.None,
                });
            }
            finally
            {
                IsRemotePlayerBuild = false;
            }
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Remote Player build failed: {report.summary.result}, "
                    + $"{report.summary.totalErrors} errors.");
            }
            Debug.Log($"Remote Player build completed: {report.summary.outputPath}");
        }

        private static string GetArgument(string[] arguments, string name)
        {
            var index = Array.IndexOf(arguments, name);
            return index >= 0 && index + 1 < arguments.Length
                ? arguments[index + 1]
                : string.Empty;
        }

        private static void AssertRemoteContentExcluded()
        {
            foreach (var directory in new[]
                     {
                         "NovelTexts",
                         "NovelsAudio",
                         "NovelsVideos",
                         "Remote",
                     })
            {
                var path = Path.Combine(Application.streamingAssetsPath, directory);
                if (Directory.Exists(path))
                {
                    throw new InvalidOperationException(
                        $"Remote Player staging project still contains '{path}'.");
                }
            }
        }
    }
}
