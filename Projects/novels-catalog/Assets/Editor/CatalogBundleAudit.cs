using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Novels.Catalog.Editor
{
    public static class CatalogBundleAudit
    {
        private const string _bundleName = "novels_catalog";
        private const string _catalogRoot = "Assets/RemoteAssets/catalog/";
        private const string _screenPath = _catalogRoot + "screen.prefab";
        private const long _targetSize = 50 * 1024;
        private const long _warningSize = 100 * 1024;
        private const long _maximumSize = 500 * 1024;

        [Serializable]
        private sealed class Release
        {
            public Bundle[] bundles = Array.Empty<Bundle>();
        }

        [Serializable]
        private sealed class Bundle
        {
            public string name;
            public string version;
            public long size;
        }

        [MenuItem("Novels/Catalog/Audit Built Content")]
        public static void AuditBuiltContent()
        {
            var errors = new List<string>();
            AuditAssets(errors);
            AuditBuilds(errors);

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Catalog content audit failed:\n- " + string.Join("\n- ", errors));
            }

            Debug.Log("Catalog content audit passed.");
        }

        private static void AuditAssets(ICollection<string> errors)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(_screenPath) == null)
            {
                errors.Add($"Catalog screen is missing: {_screenPath}");
                return;
            }

            var dependencies = AssetDatabase.GetDependencies(_screenPath, true)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();

            Debug.Log("Catalog bundle assets:\n- " + string.Join("\n- ", dependencies));

            foreach (var path in dependencies)
            {
                if (path.StartsWith("Assets/", StringComparison.Ordinal)
                    && !path.StartsWith(_catalogRoot, StringComparison.Ordinal))
                {
                    errors.Add($"Catalog depends on a project asset outside {_catalogRoot}: {path}");
                }
            }
        }

        private static void AuditBuilds(ICollection<string> errors)
        {
            var remoteRoot = Path.Combine(
                Directory.GetParent(Application.dataPath)?.FullName
                    ?? throw new InvalidOperationException("Project root cannot be resolved."),
                "Build",
                "LocalContent",
                "Remote");

            if (!Directory.Exists(remoteRoot))
            {
                errors.Add("No built catalog was found. Run novels-content build catalog first.");
                return;
            }

            var releasePaths = Directory.GetFiles(
                    remoteRoot,
                    "release.json",
                    SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();

            if (releasePaths.Length == 0)
            {
                errors.Add("No release.json was found in Build/LocalContent/Remote.");
                return;
            }

            foreach (var releasePath in releasePaths)
                AuditRelease(releasePath, errors);
        }

        private static void AuditRelease(string releasePath, ICollection<string> errors)
        {
            var platform = new DirectoryInfo(Path.GetDirectoryName(releasePath) ?? string.Empty).Name;
            var release = JsonUtility.FromJson<Release>(File.ReadAllText(releasePath));
            var bundles = release?.bundles ?? Array.Empty<Bundle>();
            var catalogBundles = bundles
                .Where(bundle => bundle != null && bundle.name == _bundleName)
                .ToArray();

            if (bundles.Length != 1 || catalogBundles.Length != 1)
            {
                errors.Add($"{platform}: release must contain exactly one {_bundleName} bundle.");
                return;
            }

            var bundle = catalogBundles[0];
            var bundlePath = Path.Combine(
                Path.GetDirectoryName(releasePath) ?? string.Empty,
                bundle.name,
                bundle.version ?? string.Empty);

            if (!File.Exists(bundlePath))
            {
                errors.Add($"{platform}: bundle file is missing: {bundlePath}");
                return;
            }

            var actualSize = new FileInfo(bundlePath).Length;
            if (actualSize != bundle.size)
                errors.Add($"{platform}: release size is {bundle.size} bytes, file size is {actualSize} bytes.");
            if (actualSize > _maximumSize)
                errors.Add($"{platform}: bundle is {FormatSize(actualSize)}; maximum is {FormatSize(_maximumSize)}.");
            else if (actualSize > _warningSize)
                Debug.LogWarning($"{platform}: catalog bundle is {FormatSize(actualSize)}; warning threshold is {FormatSize(_warningSize)}.");
            else if (actualSize > _targetSize)
                Debug.Log($"{platform}: catalog bundle is {FormatSize(actualSize)}; target is {FormatSize(_targetSize)}.");
            else
                Debug.Log($"{platform}: catalog bundle is {FormatSize(actualSize)} and fits the target budget.");
        }

        private static string FormatSize(long bytes) => $"{bytes / 1024f:0.0} KiB";
    }
}
