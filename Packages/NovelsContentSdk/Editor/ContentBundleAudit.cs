using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Novels.ContentSdk.Editor
{
    internal static class ContentBundleAudit
    {
        private const string _catalogRoot = "Assets/RemoteAssets/catalog/";
        private const long _catalogWarningSize = 100 * 1024;
        private const long _catalogMaximumSize = 500 * 1024;

        internal static void Audit(
            ContentBuildPlan plan,
            IReadOnlyCollection<string> rootAssets,
            string bundlePath,
            long declaredSize)
        {
            var errors = new List<string>();
            if (rootAssets == null || rootAssets.Count == 0)
                errors.Add("The bundle contains no root assets.");
            if (!File.Exists(bundlePath))
                errors.Add($"The bundle file does not exist: {bundlePath}");

            var actualSize = File.Exists(bundlePath)
                ? new FileInfo(bundlePath).Length
                : 0;
            if (actualSize != declaredSize)
            {
                errors.Add(
                    $"The declared size is {declaredSize} bytes, "
                    + $"but the file size is {actualSize} bytes.");
            }

            var assets = rootAssets?
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray() ?? Array.Empty<string>();
            foreach (var path in assets)
            {
                if (!ContentAssets.IsBundleSource(path))
                {
                    errors.Add($"Unsupported root bundle asset: {path}");
                }
            }

            var dependencies = plan.Kind == ContentProjectKind.Catalog
                ? AuditCatalog(assets, actualSize, errors)
                : Array.Empty<string>();

            if (errors.Count > 0)
            {
                Debug.LogError(
                    $"Content bundle '{plan.BundleName}' root assets:\n- "
                    + string.Join("\n- ", assets));
                if (dependencies.Length > 0)
                {
                    Debug.LogError(
                        "Catalog bundle dependencies:\n- "
                        + string.Join("\n- ", dependencies));
                }
                throw new InvalidOperationException(
                    "Content bundle audit failed:\n- "
                    + string.Join("\n- ", errors));
            }

            Debug.Log(
                $"Content bundle '{plan.BundleName}' audit passed: "
                + $"{assets.Length} root assets, {FormatSize(actualSize)}.");
        }

        private static string[] AuditCatalog(
            string[] rootAssets,
            long size,
            ICollection<string> errors)
        {
            var dependencies = AssetDatabase.GetDependencies(rootAssets, true)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            foreach (var path in dependencies)
            {
                if (path.StartsWith("Assets/", StringComparison.Ordinal)
                    && !path.StartsWith(_catalogRoot, StringComparison.Ordinal))
                {
                    errors.Add(
                        $"Catalog depends on a project asset outside "
                        + $"{_catalogRoot}: {path}");
                }
            }

            if (size > _catalogMaximumSize)
            {
                errors.Add(
                    $"Catalog bundle is {FormatSize(size)}; maximum is "
                    + $"{FormatSize(_catalogMaximumSize)}.");
            }
            else if (size > _catalogWarningSize)
            {
                Debug.LogWarning(
                    $"Catalog bundle is {FormatSize(size)}; warning threshold is "
                    + $"{FormatSize(_catalogWarningSize)}.");
            }
            return dependencies;
        }

        private static string FormatSize(long bytes) =>
            $"{bytes / 1024f:0.0} KiB";
    }
}
