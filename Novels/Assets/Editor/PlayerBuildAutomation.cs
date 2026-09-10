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
            var signing = AndroidSigningSnapshot.Capture();
            IsEmbeddedPlayerBuild = true;
            try
            {
                ApplyTestSigning(arguments, isDevelopmentBuild);
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
                signing.Restore();
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
            var contentChannel = GetArgument(arguments, "-contentChannel");
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
            if (string.IsNullOrWhiteSpace(contentChannel)
                || contentChannel.Any(character =>
                    !(character is >= 'a' and <= 'z'
                        or >= '0' and <= '9' or '_' or '-')))
            {
                throw new InvalidOperationException(
                    "-contentChannel must be a lowercase path segment.");
            }

            AssertRemoteContentExcluded();
            AssertReleasePlayerSettings(isDevelopmentBuild);
            EditorSceneManager.OpenScene(
                "Assets/Novels/Novels.unity",
                OpenSceneMode.Single);
            CreateRuntimeConfiguration(uri.AbsoluteUri.TrimEnd('/'), contentChannel);

            var scenes = EditorBuildSettings.scenes
                .Where(value => value.enabled)
                .Select(value => value.path)
                .ToArray();
            BuildReport report;
            var buildIdentity = ApplyBuildIdentity(arguments);
            IsRemotePlayerBuild = true;
            var signing = AndroidSigningSnapshot.Capture();
            var stripEngineCode = PlayerSettings.stripEngineCode;
            try
            {
                ApplyTestSigning(arguments, isDevelopmentBuild);
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
                    extraScriptingDefines = Array.Empty<string>(),
                });
            }
            finally
            {
                buildIdentity.Restore();
                PlayerSettings.stripEngineCode = stripEngineCode;
                signing.Restore();
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

        private static void CreateRuntimeConfiguration(string remoteUrl, string contentChannel)
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
            serialized.FindProperty("_contentChannel").stringValue = contentChannel;
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

        private static void ApplyTestSigning(string[] arguments, bool isDevelopmentBuild)
        {
            if (!arguments.Contains("-testSigning"))
                return;
            if (isDevelopmentBuild)
                throw new InvalidOperationException("Test signing cannot be combined with a Development Build.");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                throw new InvalidOperationException("Test signing is supported only for Android Player builds.");

            var keystore = Environment.GetEnvironmentVariable("NOVELS_TEST_KEYSTORE_PATH");
            var keystorePassword = Environment.GetEnvironmentVariable("NOVELS_TEST_KEYSTORE_PASSWORD");
            var alias = Environment.GetEnvironmentVariable("NOVELS_TEST_KEYALIAS");
            var aliasPassword = Environment.GetEnvironmentVariable("NOVELS_TEST_KEYALIAS_PASSWORD");
            if (string.IsNullOrWhiteSpace(keystore) || !File.Exists(keystore)
                || string.IsNullOrWhiteSpace(keystorePassword)
                || string.IsNullOrWhiteSpace(alias)
                || string.IsNullOrWhiteSpace(aliasPassword))
            {
                throw new InvalidOperationException(
                    "Test signing requires a valid local keystore and all NOVELS_TEST_KEY* environment variables.");
            }

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = Path.GetFullPath(keystore);
            PlayerSettings.Android.keystorePass = keystorePassword;
            PlayerSettings.Android.keyaliasName = alias;
            PlayerSettings.Android.keyaliasPass = aliasPassword;
            Debug.Log("Android test signing enabled for a non-development build.");
        }

        private readonly struct AndroidSigningSnapshot
        {
            private readonly bool _useCustomKeystore;
            private readonly string _keystoreName;
            private readonly string _keystorePass;
            private readonly string _keyaliasName;
            private readonly string _keyaliasPass;

            private AndroidSigningSnapshot(bool useCustomKeystore, string keystoreName,
                string keystorePass, string keyaliasName, string keyaliasPass)
            {
                _useCustomKeystore = useCustomKeystore;
                _keystoreName = keystoreName;
                _keystorePass = keystorePass;
                _keyaliasName = keyaliasName;
                _keyaliasPass = keyaliasPass;
            }

            internal static AndroidSigningSnapshot Capture() => new(
                PlayerSettings.Android.useCustomKeystore,
                PlayerSettings.Android.keystoreName,
                PlayerSettings.Android.keystorePass,
                PlayerSettings.Android.keyaliasName,
                PlayerSettings.Android.keyaliasPass);

            internal void Restore()
            {
                PlayerSettings.Android.useCustomKeystore = _useCustomKeystore;
                PlayerSettings.Android.keystoreName = _keystoreName;
                PlayerSettings.Android.keystorePass = _keystorePass;
                PlayerSettings.Android.keyaliasName = _keyaliasName;
                PlayerSettings.Android.keyaliasPass = _keyaliasPass;
            }
        }

        private static BuildIdentitySnapshot ApplyBuildIdentity(string[] arguments)
        {
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(
                BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            var snapshot = new BuildIdentitySnapshot(
                PlayerSettings.productName,
                namedBuildTarget,
                PlayerSettings.GetApplicationIdentifier(namedBuildTarget),
                PlayerSettings.GetIcons(namedBuildTarget, IconKind.Application),
                PlayerSettings.bundleVersion,
                PlayerSettings.Android.bundleVersionCode,
                PlayerSettings.iOS.buildNumber,
                PlayerSettings.macOS.buildNumber);
            var profile = LoadApplicationProfile(arguments);
            var version = GetArgument(arguments, "-playerVersion");
            var buildNumber = GetArgument(arguments, "-playerBuildNumber");
            if (string.IsNullOrWhiteSpace(version)
                || !int.TryParse(buildNumber, out var numericBuild)
                || numericBuild <= 0)
            {
                throw new InvalidOperationException(
                    "-playerVersion and a positive -playerBuildNumber are required.");
            }

            var applicationIdentifier = profile.ApplicationIdentifier(
                EditorUserBuildSettings.activeBuildTarget);
            if (string.IsNullOrWhiteSpace(applicationIdentifier))
            {
                throw new InvalidOperationException(
                    $"Application profile '{profile.id}' has no identifier for "
                    + $"{EditorUserBuildSettings.activeBuildTarget}.");
            }
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(profile.IconAssetPath);
            if (icon == null)
            {
                throw new InvalidOperationException(
                    $"Application icon is missing or not importable: {profile.IconAssetPath}");
            }
            var iconSizes = PlayerSettings.GetIconSizes(namedBuildTarget, IconKind.Application);
            var icons = Enumerable.Repeat(icon, Math.Max(1, iconSizes.Length)).ToArray();

            PlayerSettings.productName = profile.productName;
            PlayerSettings.SetApplicationIdentifier(namedBuildTarget, applicationIdentifier);
            PlayerSettings.SetIcons(namedBuildTarget, icons, IconKind.Application);
            PlayerSettings.bundleVersion = version;
            PlayerSettings.Android.bundleVersionCode = numericBuild;
            PlayerSettings.iOS.buildNumber = buildNumber;
            PlayerSettings.macOS.buildNumber = buildNumber;
            Debug.Log(
                $"Player build identity: app={profile.id}, product={profile.productName}, "
                + $"identifier={applicationIdentifier}, version={version}, build={buildNumber}.");
            return snapshot;
        }

        private static ApplicationProfile LoadApplicationProfile(string[] arguments)
        {
            var profileAssetPath = GetArgument(arguments, "-playerProfile").Replace('\\', '/');
            var profileSegments = profileAssetPath.Split('/');
            if (string.IsNullOrWhiteSpace(profileAssetPath)
                || profileSegments.Length != 5
                || profileSegments[0] != "Assets"
                || profileSegments[1] != "BuildProfiles"
                || profileSegments[3] != "Config"
                || profileSegments[4] != "player.json")
            {
                throw new InvalidOperationException(
                    "-playerProfile must reference Assets/<profile>/Config/player.json.");
            }
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Unity project root is unavailable.");
            var profilePath = Path.GetFullPath(Path.Combine(projectRoot, profileAssetPath));
            var assetsRoot = Path.GetFullPath(Application.dataPath) + Path.DirectorySeparatorChar;
            if (!profilePath.StartsWith(assetsRoot, StringComparison.Ordinal)
                || !File.Exists(profilePath))
            {
                throw new InvalidOperationException(
                    $"Application profile is missing or outside Assets: {profileAssetPath}");
            }
            var profile = JsonUtility.FromJson<ApplicationProfile>(File.ReadAllText(profilePath));
            if (profile == null || profile.schemaVersion != 1
                || string.IsNullOrWhiteSpace(profile.id)
                || profile.id != profileSegments[2]
                || profile.id.Any(character =>
                    !(character is >= 'a' and <= 'z' or >= '0' and <= '9' or '-'))
                || string.IsNullOrWhiteSpace(profile.productName)
                || string.IsNullOrWhiteSpace(profile.icon))
            {
                throw new InvalidOperationException(
                    $"Application profile is invalid: {profileAssetPath}");
            }
            var profileDirectory = Path.GetDirectoryName(profileAssetPath)?.Replace('\\', '/');
            var iconAssetPath = Path.GetFullPath(Path.Combine(
                    projectRoot,
                    profileDirectory ?? string.Empty,
                    profile.icon))
                .Replace('\\', '/');
            var normalizedProjectRoot = projectRoot.Replace('\\', '/').TrimEnd('/') + "/";
            if (!iconAssetPath.StartsWith(normalizedProjectRoot + "Assets/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Application profile icon escapes Assets: {profile.icon}");
            }
            profile.IconAssetPath = iconAssetPath.Substring(normalizedProjectRoot.Length);
            AssetDatabase.ImportAsset(profile.IconAssetPath, ImportAssetOptions.ForceSynchronousImport);
            return profile;
        }

        [Serializable]
        private sealed class ApplicationProfile
        {
            public int schemaVersion;
            public string id;
            public string productName;
            public string androidApplicationId;
            public string iosApplicationId;
            public string standaloneApplicationId;
            public string icon;

            [NonSerialized] public string IconAssetPath;

            internal string ApplicationIdentifier(BuildTarget target) => target switch
            {
                BuildTarget.Android => androidApplicationId,
                BuildTarget.iOS => iosApplicationId,
                BuildTarget.StandaloneWindows64 or BuildTarget.StandaloneOSX => standaloneApplicationId,
                _ => string.Empty,
            };
        }

        private readonly struct BuildIdentitySnapshot
        {
            private readonly string _productName;
            private readonly NamedBuildTarget _namedBuildTarget;
            private readonly string _applicationIdentifier;
            private readonly Texture2D[] _icons;
            private readonly string _version;
            private readonly int _androidBuild;
            private readonly string _iosBuild;
            private readonly string _macBuild;

            internal BuildIdentitySnapshot(
                string productName,
                NamedBuildTarget namedBuildTarget,
                string applicationIdentifier,
                Texture2D[] icons,
                string version,
                int androidBuild,
                string iosBuild,
                string macBuild)
            {
                _productName = productName;
                _namedBuildTarget = namedBuildTarget;
                _applicationIdentifier = applicationIdentifier;
                _icons = icons;
                _version = version;
                _androidBuild = androidBuild;
                _iosBuild = iosBuild;
                _macBuild = macBuild;
            }

            internal void Restore()
            {
                PlayerSettings.productName = _productName;
                PlayerSettings.SetApplicationIdentifier(
                    _namedBuildTarget,
                    _applicationIdentifier);
                PlayerSettings.SetIcons(_namedBuildTarget, _icons, IconKind.Application);
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
