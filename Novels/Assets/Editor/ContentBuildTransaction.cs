using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    internal static class ContentBuildTransaction
    {
        internal static IReadOnlyList<ContentBuildResult> Build(
            NovelContentBuildProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));
            using var workspace = new ContentBuildWorkspace();
            try
            {
                var results = AssetBundleBuildPipeline.Build(
                    profile,
                    workspace.RemoteRoot);
                NovelContentValidator.ValidateBuiltOutputOrThrow(
                    workspace.RemoteRoot);
                foreach (var result in results)
                {
                    ContentPublishArtifactBuilder.Build(
                        result,
                        profile,
                        workspace.PublishPath(result.Platform));
                    if (profile.DeliveryMode != Bundles.ContentDeliveryMode.Remote)
                    {
                        PlayerContentSeedBuilder.Build(
                            result,
                            profile,
                            workspace.PlayerSeedPath(result.Platform));
                    }
                }
                workspace.Commit(profile, results);
                AssetDatabase.Refresh();
                Debug.Log("Novel content build workspace committed successfully.");
                return results;
            }
            catch
            {
                AssetDatabase.Refresh();
                throw;
            }
        }
    }
}
