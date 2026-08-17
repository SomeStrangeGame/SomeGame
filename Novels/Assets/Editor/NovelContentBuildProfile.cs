using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CreateAssetMenu(fileName = "NovelContentBuildProfile", menuName = "Novels/Content Build Profile")]
    internal sealed class NovelContentBuildProfile : ScriptableObject
    {
        internal const string AssetPath = "Assets/Editor/NovelContentBuildProfile.asset";

        [SerializeField] private BuildTarget[] _targets = { BuildTarget.Android };
        [SerializeField] private int _contentSchemaVersion = 1;
        [SerializeField] private string _minimumClientVersion;
        [SerializeField] private string _publishRoot = "Build/NovelContent";
        [SerializeField] private long _totalBudgetBytes = 1024L * 1024L * 1024L;
        [SerializeField] private long _largeFileWarningBytes = 64L * 1024L * 1024L;
        [SerializeField] private long _largeWavWarningBytes = 16L * 1024L * 1024L;

        internal BuildTarget[] Targets =>
            _targets == null || _targets.Length == 0
                ? new[] { BuildTarget.Android }
                : (BuildTarget[])_targets.Clone();
        internal int ContentSchemaVersion => _contentSchemaVersion;
        internal string MinimumClientVersion => string.IsNullOrWhiteSpace(_minimumClientVersion)
            ? Application.version
            : _minimumClientVersion;
        internal string PublishRoot => string.IsNullOrWhiteSpace(_publishRoot)
            ? "Build/NovelContent"
            : _publishRoot;
        internal long TotalBudgetBytes => _totalBudgetBytes;
        internal long LargeFileWarningBytes => _largeFileWarningBytes;
        internal long LargeWavWarningBytes => _largeWavWarningBytes;

        internal static NovelContentBuildProfile Load()
        {
            return AssetDatabase.LoadAssetAtPath<NovelContentBuildProfile>(AssetPath)
                ?? throw new InvalidOperationException(
                    $"Novel content build profile is missing: {AssetPath}");
        }
    }
}
