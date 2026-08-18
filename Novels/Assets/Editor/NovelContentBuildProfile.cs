using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CreateAssetMenu(fileName = "NovelContentBuildProfile", menuName = "Novels/Content Build Profile")]
    internal sealed class NovelContentBuildProfile : ScriptableObject
    {
        internal const string AssetPath = "Assets/Editor/NovelContentBuildProfile.asset";

        [SerializeField] private BuildTarget[] _targets =
        {
            BuildTarget.Android,
            BuildTarget.iOS,
        };
        [SerializeField] private int _contentSchemaVersion = 5;
        [SerializeField] private Bundles.ContentDeliveryMode _deliveryMode =
            Bundles.ContentDeliveryMode.Remote;
        [SerializeField] private string[] _embeddedDeliveryGroups = Array.Empty<string>();
        [SerializeField] private string _minimumClientVersion;
        [SerializeField] private string _publishRoot = "Build/NovelContent/ServerRoot";
        [SerializeField] private string _playerSeedRoot = "Build/NovelContentSeed";
        [SerializeField] private long _totalBudgetBytes = 1024L * 1024L * 1024L;
        [SerializeField] private long _embeddedBudgetBytes = 256L * 1024L * 1024L;
        [SerializeField] private bool _enforceTotalBudget;
        [SerializeField] private bool _enforceEmbeddedBudget;
        [SerializeField] private long _largeFileWarningBytes = 64L * 1024L * 1024L;
        [SerializeField] private long _largeWavWarningBytes = 16L * 1024L * 1024L;
        [SerializeField] private bool _enforceLargeWavPolicy = true;
        [SerializeField] private string[] _allowedLargeWavPaths = Array.Empty<string>();

        internal BuildTarget[] Targets =>
            _targets == null || _targets.Length == 0
                ? new[] { BuildTarget.Android, BuildTarget.iOS }
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
            ? "Build/NovelContent/ServerRoot"
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
        internal bool EnforceLargeWavPolicy => _enforceLargeWavPolicy;
        internal string[] AllowedLargeWavPaths =>
            (_allowedLargeWavPaths ?? Array.Empty<string>())
                .Select(NormalizePath)
                .ToArray();

        internal static NovelContentBuildProfile Load()
        {
            return AssetDatabase.LoadAssetAtPath<NovelContentBuildProfile>(AssetPath)
                ?? throw new InvalidOperationException(
                    $"Novel content build profile is missing: {AssetPath}");
        }

        internal void Validate()
        {
            if (_contentSchemaVersion < 5)
            {
                throw new InvalidOperationException(
                    "Content schema version 5 or newer is required for addressed file payloads.");
            }
            if (!Novels.ContentAddressing.ContentCompatibility.Supports(
                    _contentSchemaVersion))
            {
                throw new InvalidOperationException(
                    $"Content schema {_contentSchemaVersion} is not supported by the player "
                    + $"({Novels.ContentAddressing.ContentCompatibility.MinimumSupportedSchemaVersion}-"
                    + $"{Novels.ContentAddressing.ContentCompatibility.MaximumSupportedSchemaVersion}).");
            }
            if (_totalBudgetBytes <= 0 || _embeddedBudgetBytes <= 0)
                throw new InvalidOperationException("Content budgets must be positive.");
            if (_largeWavWarningBytes <= 0)
                throw new InvalidOperationException("Large WAV threshold must be positive.");
            var duplicateException = AllowedLargeWavPaths
                .GroupBy(value => value, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicateException != null)
            {
                throw new InvalidOperationException(
                    $"Duplicate large WAV exception '{duplicateException.Key}'.");
            }
            if (_deliveryMode == Bundles.ContentDeliveryMode.Hybrid
                && EmbeddedDeliveryGroups.Length == 0)
            {
                throw new InvalidOperationException(
                    "Hybrid delivery requires at least one embedded delivery group.");
            }
        }

        private static string NormalizePath(string path) =>
            (path ?? string.Empty).Trim().Replace('\\', '/').TrimStart('/');
    }
}
