using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Editor
{
    internal static class BuiltReleaseValidator
    {
        internal static void Validate(
            IEnumerable<string> contentIds,
            ICollection<string> errors,
            string remoteBasePath = null)
        {
            var profile = NovelContentBuildProfile.Load();
            try
            {
                profile.Validate();
            }
            catch (Exception exception)
            {
                errors.Add($"Content build profile is invalid: {exception.Message}");
                return;
            }
            foreach (var target in profile.Targets)
                ValidateTarget(target, profile, contentIds, errors, remoteBasePath);
        }

        private static void ValidateTarget(
            UnityEditor.BuildTarget target,
            NovelContentBuildProfile profile,
            IEnumerable<string> contentIds,
            ICollection<string> errors,
            string remoteBasePath)
        {
            var platform = AssetBundleBuildPipeline.GetPlatformName(target);
            var remoteRoot = Path.Combine(
                string.IsNullOrWhiteSpace(remoteBasePath)
                    ? Path.Combine(Application.streamingAssetsPath, "Remote")
                    : remoteBasePath,
                platform);
            var path = Path.Combine(remoteRoot, "release.json");
            if (!File.Exists(path))
            {
                errors.Add($"Built {platform} content release is missing: {path}");
                return;
            }
            Bundles.ContentReleaseDto release;
            try
            {
                release = JsonUtility.FromJson<Bundles.ContentReleaseDto>(File.ReadAllText(path));
                Bundles.ContentReleaseValidator.Validate(
                    release,
                    Application.version,
                    profile.ContentSchemaVersion,
                    profile.ContentSchemaVersion);
            }
            catch (Exception exception)
            {
                errors.Add($"Content release is invalid: {exception.Message}");
                return;
            }
            if (release.deliveryMode != Bundles.ContentDeliveryMode.Embedded)
            {
                var groups = new HashSet<string>(
                    (release.deliveryGroups ?? Array.Empty<Bundles.ContentDeliveryGroupEntry>())
                        .Where(group => group != null)
                        .Select(group => group.id),
                    StringComparer.OrdinalIgnoreCase);
                foreach (var contentId in contentIds.Where(
                             value => !string.IsNullOrWhiteSpace(value)))
                {
                    var prefix = contentId + "/";
                    if (!groups.Any(group => group.StartsWith(
                            prefix,
                            StringComparison.OrdinalIgnoreCase)))
                    {
                        errors.Add(
                            $"Content '{contentId}' has no shared or episode delivery group "
                            + $"in {platform} release.");
                    }
                }
            }
            foreach (var bundle in release.bundles)
            {
                var payloadPath = Path.Combine(remoteRoot, bundle.name, bundle.version);
                if (!File.Exists(payloadPath))
                    errors.Add($"Release bundle is missing: '{bundle.name}'.");
                else
                {
                    var info = new FileInfo(payloadPath);
                    if (info.Length != bundle.size)
                        errors.Add($"Release size does not match bundle '{bundle.name}'.");
                    else if (!string.Equals(
                                 AssetBundleBuildPipeline.ComputeSha256(payloadPath),
                                 bundle.sha256,
                                 StringComparison.OrdinalIgnoreCase))
                        errors.Add($"Release SHA-256 does not match bundle '{bundle.name}'.");
                }
            }
            var releasedFiles = new HashSet<string>(
                (release.files ?? Array.Empty<Bundles.ContentFileEntry>())
                    .Where(file => file != null)
                    .Select(file => file.path),
                StringComparer.OrdinalIgnoreCase);
            IReadOnlyDictionary<string, string> deliveryIndex;
            try
            {
                deliveryIndex = ContentDeliveryIndexBuilder.Build();
            }
            catch (Exception exception)
            {
                errors.Add($"Content delivery index is invalid: {exception.Message}");
                return;
            }
            foreach (var relative in deliveryIndex.Keys)
                if (!releasedFiles.Contains(relative))
                    errors.Add($"Release does not describe deliverable file '{relative}'.");
            foreach (var relative in releasedFiles)
                if (!deliveryIndex.ContainsKey(relative))
                    errors.Add($"Release describes unassigned file '{relative}'.");
        }
    }
}
