using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Editor
{
    internal static class BuiltReleaseValidator
    {
        internal static void Validate(ICollection<string> errors)
        {
            var profile = NovelContentBuildProfile.Load();
            foreach (var target in profile.Targets)
                ValidateTarget(target, profile, errors);
        }

        private static void ValidateTarget(
            UnityEditor.BuildTarget target,
            NovelContentBuildProfile profile,
            ICollection<string> errors)
        {
            var platform = AssetBundleBuildPipeline.GetPlatformName(target);
            var remoteRoot = Path.Combine(Application.streamingAssetsPath, "Remote", platform);
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
                    profile.ContentSchemaVersion);
            }
            catch (Exception exception)
            {
                errors.Add($"Content release is invalid: {exception.Message}");
                return;
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
            foreach (var file in ContentFilePolicy.EnumerateFiles())
            {
                var relative = ContentFilePolicy.GetRelativePath(file);
                if (!releasedFiles.Contains(relative))
                    errors.Add($"Release does not describe file '{relative}'.");
            }
        }
    }
}
