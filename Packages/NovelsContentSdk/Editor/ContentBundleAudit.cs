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
        private const long _catalogTargetSize = 50 * 1024;
        private const long _catalogWarningSize = 100 * 1024;
        private const long _catalogMaximumSize = 500 * 1024;

        internal static void Audit(
            ContentProject project,
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
            Debug.Log(
                $"Content bundle '{project.BundleName}' root assets:\n- "
                + string.Join("\n- ", assets));

            foreach (var path in assets)
            {
                if (!path.StartsWith(
                        "Assets/RemoteAssets/",
                        StringComparison.Ordinal))
                {
                    errors.Add($"Root asset is outside Assets/RemoteAssets: {path}");
                }
            }

            if (project.Kind == ContentProjectKind.Catalog)
                AuditCatalog(assets, actualSize, errors);
            else
                Debug.Log($"Content bundle size: {FormatSize(actualSize)}.");

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Content bundle audit failed:\n- "
                    + string.Join("\n- ", errors));
            }

            Debug.Log("Content bundle audit passed.");
        }

        private static void AuditCatalog(
            string[] rootAssets,
            long size,
            ICollection<string> errors)
        {
            var dependencies = AssetDatabase.GetDependencies(rootAssets, true)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            Debug.Log(
                "Catalog bundle dependencies:\n- "
                + string.Join("\n- ", dependencies));

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
            else if (size > _catalogTargetSize)
            {
                Debug.Log(
                    $"Catalog bundle is {FormatSize(size)}; target is "
                    + $"{FormatSize(_catalogTargetSize)}.");
            }
            else
            {
                Debug.Log(
                    $"Catalog bundle is {FormatSize(size)} and fits the target "
                    + "budget.");
            }
        }

        private static string FormatSize(long bytes) =>
            $"{bytes / 1024f:0.0} KiB";
    }
}
