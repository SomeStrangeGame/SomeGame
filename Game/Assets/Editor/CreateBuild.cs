using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Build.Profile;

namespace Editor
{
    public class CreateBuild
    {
        [MenuItem("Build/Build All")]
        public static void BuildAll()
        {
            BuildGzipCompression();
            BuildBrotliCompression();
            BuildNoCompression();
        }

        [MenuItem("Build/Build All And Run")]
        public static void BuildAllAndRun()
        {
            BuildGzipCompression();
            BuildBrotliCompression();
            BuildNoCompressionAndRun();
        }

        [MenuItem("Build/Build GzipCompression")]
        public static void BuildGzipCompression()
        {
            Build("Assets/Settings/Build Profiles/GzipCompression.asset", "../GzipBuild", BuildOptions.None);
        }

        [MenuItem("Build/Build BrotliCompression")]
        public static void BuildBrotliCompression()
        {
            Build("Assets/Settings/Build Profiles/BrotliCompression.asset", "../BrotliBuild", BuildOptions.None);
        }

        [MenuItem("Build/Build DevBuild")]
        public static void BuildNoCompression()
        {
            Build("Assets/Settings/Build Profiles/NoCompression.asset", "../DevBuild", BuildOptions.None);
        }

        [MenuItem("Build/Build DevBuild And Run")]
        public static void BuildNoCompressionAndRun()
        {
            Build("Assets/Settings/Build Profiles/NoCompression.asset", "../DevBuild", BuildOptions.AutoRunPlayer);
        }

        private static void Build(string profilePath, string locationPath, BuildOptions buildOptions)
        {
            BuildProfile buildProfile = AssetDatabase.LoadAssetAtPath<BuildProfile>(profilePath);
            BuildPlayerWithProfileOptions options = new()
            {
                buildProfile = buildProfile,
                locationPathName = locationPath,
                options = buildOptions,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }
        }
    }
}

