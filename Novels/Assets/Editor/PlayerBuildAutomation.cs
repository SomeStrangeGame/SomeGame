using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor
{
    public static class PlayerBuildAutomation
    {
        internal static bool IsRemotePlayerBuild { get; private set; }
        internal static bool IsEmbeddedPlayerBuild { get; private set; }
        internal static bool IsAuthorizedPlayerBuild =>
            IsRemotePlayerBuild || IsEmbeddedPlayerBuild;

        public static void BuildEmbeddedPlayerBatch()
        {
            var arguments = Environment.GetCommandLineArgs();
            var output = GetArgument(
                arguments,
                "-playerOutput");
            if (string.IsNullOrWhiteSpace(output))
                throw new InvalidOperationException("-playerOutput is required.");
            var contentRoot = Path.Combine(
                Application.streamingAssetsPath,
                "NovelContent");
            if (!File.Exists(Path.Combine(
                    contentRoot,
                    "catalog",
                    "registry",
                    "catalog.json")))
            {
                throw new InvalidOperationException(
                    $"Embedded content is missing: {contentRoot}");
            }

            EditorSceneManager.OpenScene(
                "Assets/Novels/Novels.unity",
                OpenSceneMode.Single);
            BuildReport report;
            var buildIdentity = ApplyBuildIdentity(arguments);
            var isDevelopmentBuild = arguments.Contains("-developmentBuild");
            var useCustomKeystore = PlayerSettings.Android.useCustomKeystore;
            IsEmbeddedPlayerBuild = true;
            try
            {
                if (isDevelopmentBuild && EditorUserBuildSettings.activeBuildTarget
                    == BuildTarget.Android)
                {
                    PlayerSettings.Android.useCustomKeystore = false;
                }
                report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = EditorBuildSettings.scenes
                        .Where(value => value.enabled)
                        .Select(value => value.path)
                        .ToArray(),
                    locationPathName = Path.GetFullPath(output),
                    target = EditorUserBuildSettings.activeBuildTarget,
                    options = isDevelopmentBuild
                        ? BuildOptions.Development
                        : BuildOptions.None,
                    extraScriptingDefines = new[] {"NOVELS_EMBEDDED_CONTENT"},
                });
            }
            finally
            {
                buildIdentity.Restore();
                PlayerSettings.Android.useCustomKeystore = useCustomKeystore;
                IsEmbeddedPlayerBuild = false;
            }
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Embedded Player build failed: {report.summary.result}, "
                    + $"{report.summary.totalErrors} errors.");
            }
            Debug.Log(
                $"Embedded Player build completed: {report.summary.outputPath} "
                + $"({report.summary.totalSize / (1024f * 1024f):F1} MiB)");
        }
        public static void BuildRemotePlayerBatch()
        {
            var arguments = Environment.GetCommandLineArgs();
            var remoteUrl = GetArgument(arguments, "-remoteContentBaseUrl");
            var output = GetArgument(arguments, "-playerOutput");
            var isDevelopmentBuild = arguments.Contains("-developmentBuild");
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
            AssertReleasePlayerSettings(isDevelopmentBuild);
            EditorSceneManager.OpenScene(
                "Assets/Novels/Novels.unity",
                OpenSceneMode.Single);
            CreateRuntimeConfiguration(uri.AbsoluteUri.TrimEnd('/'));

            var scenes = EditorBuildSettings.scenes
                .Where(value => value.enabled)
                .Select(value => value.path)
                .ToArray();
            BuildReport report;
            var buildIdentity = ApplyBuildIdentity(arguments);
            IsRemotePlayerBuild = true;
            var useCustomKeystore = PlayerSettings.Android.useCustomKeystore;
            var stripEngineCode = PlayerSettings.stripEngineCode;
            try
            {
                if (isDevelopmentBuild && EditorUserBuildSettings.activeBuildTarget
                    == BuildTarget.Android)
                {
                    PlayerSettings.Android.useCustomKeystore = false;
                    PlayerSettings.stripEngineCode = false;
                }
                report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = Path.GetFullPath(output),
                    target = EditorUserBuildSettings.activeBuildTarget,
                    options = isDevelopmentBuild
                        ? BuildOptions.Development
                        : BuildOptions.None,
                });
            }
            finally
            {
                buildIdentity.Restore();
                PlayerSettings.stripEngineCode = stripEngineCode;
                PlayerSettings.Android.useCustomKeystore = useCustomKeystore;
                IsRemotePlayerBuild = false;
            }
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Remote Player build failed: {report.summary.result}, "
                    + $"{report.summary.totalErrors} errors.");
            }
            Debug.Log(
                $"Remote Player build completed: {report.summary.outputPath} "
                + $"({report.summary.totalSize / (1024f * 1024f):F1} MiB)");
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

        private static BuildIdentitySnapshot ApplyBuildIdentity(string[] arguments)
        {
            var snapshot = new BuildIdentitySnapshot(
                PlayerSettings.bundleVersion,
                PlayerSettings.Android.bundleVersionCode,
                PlayerSettings.iOS.buildNumber,
                PlayerSettings.macOS.buildNumber);
            var version = GetArgument(arguments, "-playerVersion");
            var buildNumber = GetArgument(arguments, "-playerBuildNumber");
            if (string.IsNullOrWhiteSpace(version)
                || !int.TryParse(buildNumber, out var numericBuild)
                || numericBuild <= 0)
            {
                throw new InvalidOperationException(
                    "-playerVersion and a positive -playerBuildNumber are required.");
            }

            PlayerSettings.bundleVersion = version;
            PlayerSettings.Android.bundleVersionCode = numericBuild;
            PlayerSettings.iOS.buildNumber = buildNumber;
            PlayerSettings.macOS.buildNumber = buildNumber;
            Debug.Log($"Player build identity: version={version}, build={buildNumber}.");
            return snapshot;
        }

        private readonly struct BuildIdentitySnapshot
        {
            private readonly string _version;
            private readonly int _androidBuild;
            private readonly string _iosBuild;
            private readonly string _macBuild;

            internal BuildIdentitySnapshot(
                string version,
                int androidBuild,
                string iosBuild,
                string macBuild)
            {
                _version = version;
                _androidBuild = androidBuild;
                _iosBuild = iosBuild;
                _macBuild = macBuild;
            }

            internal void Restore()
            {
                PlayerSettings.bundleVersion = _version;
                PlayerSettings.Android.bundleVersionCode = _androidBuild;
                PlayerSettings.iOS.buildNumber = _iosBuild;
                PlayerSettings.macOS.buildNumber = _macBuild;
            }
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
                         "noveltexts",
                         "novelsaudio",
                         "novelsvideos",
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

        private static void AssertReleasePlayerSettings(bool isDevelopmentBuild)
        {
            if (isDevelopmentBuild
                || EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                return;
            }

            if (PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android)
                != ScriptingImplementation.IL2CPP)
            {
                throw new InvalidOperationException(
                    "Android release Player must use IL2CPP.");
            }
            if (PlayerSettings.Android.targetArchitectures
                != AndroidArchitecture.ARM64)
            {
                throw new InvalidOperationException(
                    "Android release Player must target ARM64 only.");
            }
            if (!PlayerSettings.stripEngineCode)
            {
                throw new InvalidOperationException(
                    "Android release Player must strip unused engine code.");
            }
            var stripping = PlayerSettings.GetManagedStrippingLevel(
                NamedBuildTarget.Android);
            if ((int)stripping < (int)ManagedStrippingLevel.Medium)
            {
                throw new InvalidOperationException(
                    "Android release Player must use Medium or High managed stripping.");
            }
        }
    }
}
