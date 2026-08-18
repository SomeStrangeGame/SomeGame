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
            CreateAssetBundles.BuildConfiguredBundles();
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
            EditorSceneManager.OpenScene(
                "Assets/Novels/Novels.unity",
                OpenSceneMode.Single);
            CreateRuntimeConfiguration(uri.AbsoluteUri.TrimEnd('/'));

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

        private static void CreateRuntimeConfiguration(string remoteUrl)
        {
            if (AssetDatabase.LoadMainAssetAtPath(
                    Novels.ContentRuntimeConfiguration.AssetPath) != null)
            {
                throw new InvalidOperationException(
                    $"Generated runtime configuration already exists: "
                    + Novels.ContentRuntimeConfiguration.AssetPath);
            }
            EnsureFolder("Assets/Resources/Novels");
            var configuration = ScriptableObject.CreateInstance<
                Novels.ContentRuntimeConfiguration>();
            var serialized = new SerializedObject(configuration);
            serialized.FindProperty("_remoteContentBaseUrl").stringValue = remoteUrl;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(
                configuration,
                Novels.ContentRuntimeConfiguration.AssetPath);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureFolder(string path)
        {
            var segments = path.Split('/');
            var current = segments[0];
            for (var index = 1; index < segments.Length; index++)
            {
                var next = current + "/" + segments[index];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, segments[index]);
                current = next;
            }
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
            var remoteAssets = Path.Combine(Application.dataPath, "RemoteAssets");
            if (Directory.Exists(remoteAssets))
            {
                throw new InvalidOperationException(
                    $"Remote Player staging project still contains '{remoteAssets}'.");
            }
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
