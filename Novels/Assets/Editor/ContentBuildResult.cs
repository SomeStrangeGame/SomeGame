using UnityEditor;

namespace Editor
{
    internal sealed class ContentBuildResult
    {
        internal ContentBuildResult(
            BuildTarget target,
            string platform,
            string releaseId,
            string remotePath)
        {
            Target = target;
            Platform = platform;
            ReleaseId = releaseId;
            RemotePath = remotePath;
        }

        internal BuildTarget Target { get; }
        internal string Platform { get; }
        internal string ReleaseId { get; }
        internal string RemotePath { get; set; }
        internal string PublishPath { get; set; }
        internal string PlayerSeedPath { get; set; }
    }
}
