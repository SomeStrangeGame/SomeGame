using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Novels.WebPlayer.Editor
{
    public static class WebPlayerBuild
    {
        public static void Build()
        {
            BuildPlayer("Build/WebGL", BuildOptions.Development);
        }

        public static void BuildRelease()
        {
            var compression = PlayerSettings.WebGL.compressionFormat;
            var fallback = PlayerSettings.WebGL.decompressionFallback;
            try
            {
                // The existing host supports gzip; retain a fallback until native decoding is verified.
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
                PlayerSettings.WebGL.decompressionFallback = true;
                BuildPlayer("Build/WebGLRelease", BuildOptions.None);
            }
            finally
            {
                PlayerSettings.WebGL.compressionFormat = compression;
                PlayerSettings.WebGL.decompressionFallback = fallback;
            }
        }

        private static void BuildPlayer(string output, BuildOptions options)
        {
            PrepareFallbacks();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled)
                    .Select(scene => scene.path).ToArray(),
                locationPathName = output,
                target = BuildTarget.WebGL,
                options = options,
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Web player build failed: " + report.summary.result);
        }

        // Derive defaults from the existing SDK screens, never alter authoring assets.
        public static void PrepareFallbacks()
        {
            var project = Path.GetDirectoryName(UnityEngine.Application.dataPath);
            var output = Path.Combine(project, "Assets/WebPlayer/Generated/Resources/WebFallbacks");
            Directory.CreateDirectory(output);
            var sdk = Path.GetFullPath(Path.Combine(project, "../../Packages/NovelsContentSdk/BaseUI/Base"));
            var location = File.ReadAllText(Path.Combine(sdk, "location/screen.prefab"));
            foreach (Match match in Regex.Matches(location,
                         @"(?ms)^--- !u!328 &(\d+)\r?\n.*?(?=^--- !u!|\z)"))
            {
                var id = match.Groups[1].Value;
                location = location.Replace(match.Value, string.Empty);
                location = Regex.Replace(location,
                    @"(?m)^  - component: \{fileID: " + id + @"\}\r?\n", string.Empty);
                location = location.Replace("_video: {fileID: " + id + "}", "_video: {fileID: 0}");
            }
            File.WriteAllText(Path.Combine(output, "location.prefab"), location);
            File.Copy(Path.Combine(sdk, "notification/screen.prefab"),
                Path.Combine(output, "notification.prefab"), true);
            var fallback = Path.GetFullPath(Path.Combine(project, "../../Novels/Assets/Novels/Fallbacks"));
            foreach (var name in new[] { "missing-background.png", "missing-character.png" })
            {
                File.Copy(Path.Combine(fallback, name), Path.Combine(output, name), true);
                File.Copy(Path.Combine(fallback, name + ".meta"), Path.Combine(output, name + ".meta"), true);
            }
            const string font = "liberationsans-regular.ttf";
            foreach (var suffix in new[] { "", ".meta" })
                File.Copy(Path.Combine(fallback, "EpisodeUI/notification/" + font + suffix),
                    Path.Combine(output, font + suffix), true);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }
    }
}
