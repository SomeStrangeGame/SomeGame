using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using UnityEditor.Build.Profile;

namespace Editor
{
    public class CreateBuild
    {
        [MenuItem("Build/Build All And Run")]
        public static void BuildAll()
        {
            BuildGzipCompression();
            BuildBrotliCompression();
            BuildNoCompressionAndRun();
        }

        [MenuItem("Build/Build GzipCompression")]
        public static void BuildGzipCompression()
        {
            BuildProfile buildProfile = AssetDatabase.LoadAssetAtPath<BuildProfile>("Assets/Settings/Build Profiles/GzipCompression.asset");
            BuildPlayerWithProfileOptions options = new BuildPlayerWithProfileOptions()
            {
                buildProfile = buildProfile,
                locationPathName = "../GzipBuild",
                options = BuildOptions.None,
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

        [MenuItem("Build/Build BrotliCompression")]
        public static void BuildBrotliCompression()
        {
            BuildProfile buildProfile = AssetDatabase.LoadAssetAtPath<BuildProfile>("Assets/Settings/Build Profiles/BrotliCompression.asset");
            BuildPlayerWithProfileOptions options = new BuildPlayerWithProfileOptions()
            {
                buildProfile = buildProfile,
                locationPathName = "../BrotliBuild",
                options = BuildOptions.None,
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

        [MenuItem("Build/Build DevBuild And Run")]
        public static void BuildNoCompressionAndRun()
        {
            BuildProfile buildProfile = AssetDatabase.LoadAssetAtPath<BuildProfile>("Assets/Settings/Build Profiles/NoCompression.asset");
            BuildPlayerWithProfileOptions options = new BuildPlayerWithProfileOptions()
            {
                buildProfile = buildProfile,
                locationPathName = "../DevBuild",
                options = BuildOptions.AutoRunPlayer,
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

