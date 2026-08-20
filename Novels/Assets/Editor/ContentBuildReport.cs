using System.Collections.Generic;
using System.Linq;
using Bundles;
using UnityEngine;

namespace Editor
{
    internal static class ContentBuildReport
    {
        internal static ContentDeliveryGroupEntry[] BuildGroups(
            IReadOnlyCollection<BundleReleaseEntry> bundles,
            IReadOnlyCollection<ContentFileEntry> files)
        {
            var payloads = bundles
                .Select(bundle => (bundle.deliveryGroup, bundle.size))
                .Concat(files.Select(file => (file.deliveryGroup, file.size)));
            return payloads.GroupBy(
                    payload => string.IsNullOrWhiteSpace(payload.deliveryGroup)
                        ? "shared"
                        : payload.deliveryGroup,
                    System.StringComparer.OrdinalIgnoreCase)
                .OrderBy(group => group.Key, System.StringComparer.Ordinal)
                .Select(group => new ContentDeliveryGroupEntry
                {
                    id = group.Key,
                    payloadCount = group.Count(),
                    size = group.Sum(payload => payload.size),
                })
                .ToArray();
        }

        internal static void Log(
            IReadOnlyCollection<ContentFileEntry> files,
            IReadOnlyCollection<ContentDeliveryGroupEntry> groups,
            long bundleBytes,
            NovelContentBuildProfile profile)
        {
            var externalBytes = files.Sum(file => file.size);
            var total = externalBytes + bundleBytes;
            Debug.Log(
                $"Novel content payload: {files.Count} external files, "
                + $"{FormatBytes(externalBytes)} external, {FormatBytes(bundleBytes)} bundles, "
                + $"{FormatBytes(total)} total, {groups.Count} delivery groups.");
            foreach (var group in groups)
            {
                Debug.Log(
                    $"Novel content group '{group.id}': {group.payloadCount} payloads, "
                    + FormatBytes(group.size));
            }
            if (total > profile.TotalBudgetBytes)
            {
                Debug.LogWarning(
                    $"Novel content payload exceeds {FormatBytes(profile.TotalBudgetBytes)}: "
                    + FormatBytes(total));
                if (profile.EnforceTotalBudget)
                {
                    throw new System.InvalidOperationException(
                        $"Novel content payload exceeds enforced budget "
                        + $"{FormatBytes(profile.TotalBudgetBytes)}: {FormatBytes(total)}.");
                }
            }
            foreach (var file in files.Where(
                         file => file.size > profile.LargeFileWarningBytes))
            {
                Debug.LogWarning(
                    $"Large content file '{file.path}': {FormatBytes(file.size)}");
            }
            foreach (var file in files.Where(file =>
                         file.path.StartsWith(
                             "novelsaudio/",
                             System.StringComparison.OrdinalIgnoreCase)
                         && file.path.EndsWith(
                             ".wav",
                             System.StringComparison.OrdinalIgnoreCase)
                         && file.size > profile.LargeWavWarningBytes))
            {
                var allowed = profile.AllowedLargeWavPaths.Contains(
                    file.path,
                    System.StringComparer.OrdinalIgnoreCase);
                if (profile.EnforceLargeWavPolicy && !allowed)
                {
                    throw new System.InvalidOperationException(
                        $"Large WAV '{file.path}' ({FormatBytes(file.size)}) must be "
                        + "converted to OGG or added to the explicit build-profile exceptions.");
                }
                Debug.LogWarning(
                    $"Large WAV '{file.path}' ({FormatBytes(file.size)}) is "
                    + (allowed
                        ? "explicitly allowed by the build profile."
                        : "not blocked because the policy is disabled."));
            }
        }

        private static string FormatBytes(long bytes) =>
            $"{bytes / (1024d * 1024d):0.0} MiB";
    }
}
