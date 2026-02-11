using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Build.Profile;

namespace Editor
{
    public class CreateBuild
    {
        [MenuItem("Build/BattleStory/Build All")]
        public static void BuildAll()
        {
            BuildGzipCompression();
            BuildBrotliCompression();
            BuildNoCompression();
        }

        [MenuItem("Build/BattleStory/Build All And Run")]
        public static void BuildAllAndRun()
        {
            BuildGzipCompression();
            BuildBrotliCompression();
            BuildNoCompressionAndRun();
        }

        [MenuItem("Build/BattleStory/Build GzipCompression")]
        public static void BuildGzipCompression()
        {
            Build("Assets/Settings/Build Profiles/BattleStoryGzipCompression.asset", "../GzipBuild", BuildOptions.None);
        }

        [MenuItem("Build/BattleStory/Build BrotliCompression")]
        public static void BuildBrotliCompression()
        {
            Build("Assets/Settings/Build Profiles/BattleStoryBrotliCompression.asset", "../BrotliBuild", BuildOptions.None);
        }

        [MenuItem("Build/BattleStory/Build DevBuild")]
        public static void BuildNoCompression()
        {
            Build("Assets/Settings/Build Profiles/BattleStoryNoCompression.asset", "../DevBuild", BuildOptions.None);
        }

        [MenuItem("Build/BattleStory/Build DevBuild And Run")]
        public static void BuildNoCompressionAndRun()
        {
            Build("Assets/Settings/Build Profiles/BattleStoryNoCompression.asset", "../DevBuild", BuildOptions.AutoRunPlayer);
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

