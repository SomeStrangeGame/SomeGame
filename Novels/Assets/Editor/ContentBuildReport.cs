using System.Collections.Generic;
using System.Linq;
using Bundles;
using UnityEngine;

namespace Editor
{
    internal static class ContentBuildReport
    {
        internal static ContentDeliveryGroupEntry[] BuildGroups(
            IReadOnlyCollection<ContentFileEntry> files)
        {
            return files.GroupBy(
                    file => string.IsNullOrWhiteSpace(file.deliveryGroup)
                        ? "shared"
                        : file.deliveryGroup,
                    System.StringComparer.OrdinalIgnoreCase)
                .OrderBy(group => group.Key, System.StringComparer.Ordinal)
                .Select(group => new ContentDeliveryGroupEntry
                {
                    id = group.Key,
                    fileCount = group.Count(),
                    size = group.Sum(file => file.size),
                })
                .ToArray();
        }

        internal static void Log(
            IReadOnlyCollection<ContentFileEntry> files,
            IReadOnlyCollection<ContentDeliveryGroupEntry> groups,
            NovelContentBuildProfile profile)
        {
            var total = files.Sum(file => file.size);
            Debug.Log(
                $"Novel content payload: {files.Count} files, "
                + $"{FormatBytes(total)}, {groups.Count} delivery groups.");
            foreach (var group in groups)
            {
                Debug.Log(
                    $"Novel content group '{group.id}': {group.fileCount} files, "
                    + FormatBytes(group.size));
            }
            if (total > profile.TotalBudgetBytes)
            {
                Debug.LogWarning(
                    $"Novel content payload exceeds {FormatBytes(profile.TotalBudgetBytes)}: "
                    + FormatBytes(total));
            }
            foreach (var file in files.Where(
                         file => file.size > profile.LargeFileWarningBytes))
            {
                Debug.LogWarning(
                    $"Large content file '{file.path}': {FormatBytes(file.size)}");
            }
            foreach (var file in files.Where(file =>
                         file.path.StartsWith(
                             "NovelsAudio/",
                             System.StringComparison.OrdinalIgnoreCase)
                         && file.path.EndsWith(
                             ".wav",
                             System.StringComparison.OrdinalIgnoreCase)
                         && file.size > profile.LargeWavWarningBytes))
            {
                Debug.LogWarning(
                    $"Large WAV '{file.path}' ({FormatBytes(file.size)}). "
                    + "Consider OGG/MP3 for streamed Music or Ambient content.");
            }
        }

        private static string FormatBytes(long bytes) =>
            $"{bytes / (1024d * 1024d):0.0} MiB";
    }
}
