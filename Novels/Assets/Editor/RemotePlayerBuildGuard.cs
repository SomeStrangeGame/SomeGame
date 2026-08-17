using System;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Editor
{
    internal sealed class RemotePlayerBuildGuard : IPreprocessBuildWithReport
    {
        public int callbackOrder => int.MinValue;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!NovelCiValidation.IsRemotePlayerBuild)
            {
                throw new BuildFailedException(
                    "Novel Player builds must use Tools/build-remote-player.sh so "
                    + "StreamingAssets content is excluded and the remote URL is injected.");
            }
        }
    }
}
