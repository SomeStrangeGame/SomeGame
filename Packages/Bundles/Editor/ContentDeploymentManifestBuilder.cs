using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bundles.Editor
{
    [Serializable]
    internal sealed class ContentDeploymentManifest
    {
        public string deploymentId;
        public string createdAtUtc;
        public ContentDeploymentPlatform[] platforms;
        public ContentDeploymentPayload[] payloads;
    }

    [Serializable]
    internal sealed class ContentDeploymentPlatform
    {
        public string platform;
        public string releaseId;
        public string releasePath;
    }

    [Serializable]
    internal sealed class ContentDeploymentPayload
    {
        public string path;
        public long size;
        public string sha256;
        public bool activateLast;
    }

    public sealed class ContentDeploymentManifestBuilder
    {
        private readonly Dictionary<string, ContentDeploymentPayload> _payloads = new(
            StringComparer.Ordinal);
        private readonly List<ContentDeploymentPlatform> _platforms = new();

        public void AddPayload(string path, long size, string sha256, bool activateLast = false)
        {
            var value = new ContentDeploymentPayload
            {
                path = path,
                size = size,
                sha256 = sha256,
                activateLast = activateLast,
            };
            if (_payloads.TryGetValue(path, out var existing))
            {
                if (existing.size != size
                    || !string.Equals(existing.sha256, sha256, StringComparison.OrdinalIgnoreCase)
                    || existing.activateLast != activateLast)
                {
                    throw new InvalidOperationException(
                        $"Deployment payload '{path}' has conflicting metadata.");
                }
                return;
            }
            _payloads.Add(path, value);
        }

        public void AddPlatform(string platform, string releaseId, string releasePath)
        {
            if (_platforms.Any(value => string.Equals(
                    value.platform, platform, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(
                    $"Deployment contains duplicate platform '{platform}'.");
            }
            _platforms.Add(new ContentDeploymentPlatform
            {
                platform = platform,
                releaseId = releaseId,
                releasePath = releasePath,
            });
        }

        public string Serialize()
        {
            var platforms = _platforms
                .OrderBy(value => value.platform, StringComparer.Ordinal)
                .ToArray();
            var payloads = _payloads.Values
                .OrderBy(value => value.activateLast)
                .ThenBy(value => value.path, StringComparer.Ordinal)
                .ToArray();
            var canonical = platforms.Select(value =>
                    $"P:{value.platform}:{value.releaseId}:{value.releasePath}")
                .Concat(payloads.Select(value =>
                    $"F:{value.path}:{value.size}:{value.sha256}:{value.activateLast}"));
            return JsonUtility.ToJson(new ContentDeploymentManifest
            {
                deploymentId = ContentHash.ComputeSha256(
                    Encoding.UTF8.GetBytes(string.Join("\n", canonical))),
                createdAtUtc = DateTime.UtcNow.ToString("O"),
                platforms = platforms,
                payloads = payloads,
            }, true);
        }
    }
}
