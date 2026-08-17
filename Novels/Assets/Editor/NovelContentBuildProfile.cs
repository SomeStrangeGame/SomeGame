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
        [SerializeField] private int _contentSchemaVersion = 2;
        [SerializeField] private Bundles.ContentDeliveryMode _deliveryMode =
            Bundles.ContentDeliveryMode.Embedded;
        [SerializeField] private string[] _embeddedDeliveryGroups = Array.Empty<string>();
        [SerializeField] private string _minimumClientVersion;
        [SerializeField] private string _publishRoot = "Build/NovelContent";
        [SerializeField] private string _playerSeedRoot = "Build/NovelContentSeed";
        [SerializeField] private long _totalBudgetBytes = 1024L * 1024L * 1024L;
        [SerializeField] private long _embeddedBudgetBytes = 256L * 1024L * 1024L;
        [SerializeField] private bool _enforceTotalBudget;
        [SerializeField] private bool _enforceEmbeddedBudget;
        [SerializeField] private long _largeFileWarningBytes = 64L * 1024L * 1024L;
        [SerializeField] private long _largeWavWarningBytes = 16L * 1024L * 1024L;

        internal BuildTarget[] Targets =>
            _targets == null || _targets.Length == 0
                ? new[] { BuildTarget.Android }
                : (BuildTarget[])_targets.Clone();
        internal int ContentSchemaVersion => _contentSchemaVersion;
        internal Bundles.ContentDeliveryMode DeliveryMode => _deliveryMode;
        internal string[] EmbeddedDeliveryGroups =>
            _embeddedDeliveryGroups == null
                ? Array.Empty<string>()
                : (string[])_embeddedDeliveryGroups.Clone();
        internal string MinimumClientVersion => string.IsNullOrWhiteSpace(_minimumClientVersion)
            ? Application.version
            : _minimumClientVersion;
        internal string PublishRoot => string.IsNullOrWhiteSpace(_publishRoot)
            ? "Build/NovelContent"
            : _publishRoot;
        internal string PlayerSeedRoot => string.IsNullOrWhiteSpace(_playerSeedRoot)
            ? "Build/NovelContentSeed"
            : _playerSeedRoot;
        internal long TotalBudgetBytes => _totalBudgetBytes;
        internal long EmbeddedBudgetBytes => _embeddedBudgetBytes;
        internal bool EnforceTotalBudget => _enforceTotalBudget;
        internal bool EnforceEmbeddedBudget => _enforceEmbeddedBudget;
        internal long LargeFileWarningBytes => _largeFileWarningBytes;
        internal long LargeWavWarningBytes => _largeWavWarningBytes;

        internal static NovelContentBuildProfile Load()
        {
            return AssetDatabase.LoadAssetAtPath<NovelContentBuildProfile>(AssetPath)
                ?? throw new InvalidOperationException(
                    $"Novel content build profile is missing: {AssetPath}");
        }

        internal void Validate()
        {
            if (_contentSchemaVersion < 2)
            {
                throw new InvalidOperationException(
                    "Content schema version 2 or newer is required for delivery modes.");
            }
            if (_totalBudgetBytes <= 0 || _embeddedBudgetBytes <= 0)
                throw new InvalidOperationException("Content budgets must be positive.");
            if (_deliveryMode == Bundles.ContentDeliveryMode.Hybrid
                && EmbeddedDeliveryGroups.Length == 0)
            {
                throw new InvalidOperationException(
                    "Hybrid delivery requires at least one embedded delivery group.");
            }
        }
    }
}
