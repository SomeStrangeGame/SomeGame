using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels
{
    internal static class ApplicationUpdatePolicy
    {
        [Serializable]
        private sealed class Configuration
        {
            public PlatformConfiguration android;
            public PlatformConfiguration ios;
        }

        [Serializable]
        private sealed class PlatformConfiguration
        {
            public string softVersion;
            public string hardVersion;
            public string storeUrl;
        }

        internal static string FileName(string channel) =>
            $"updates/{Uri.EscapeDataString(channel)}.json";

        internal static async UniTask<Catalog.CatalogUpdatePrompt> Download(
            Bundles.IContentSource source,
            string channel,
            string currentVersion,
            CancellationToken cancellationToken,
            Action<(LogType type, string message)> onLog)
        {
            try
            {
                var json = await source.DownloadText(FileName(channel), cancellationToken);
                var configuration = JsonUtility.FromJson<Configuration>(json);
                var platform = Application.platform == RuntimePlatform.IPhonePlayer
                    ? configuration?.ios
                    : configuration?.android;
                return Evaluate(platform, currentVersion);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                onLog?.Invoke((LogType.Warning,
                    $"Application update check failed; continuing without a prompt: {exception.Message}"));
                return Catalog.CatalogUpdatePrompt.None;
            }
        }

        private static Catalog.CatalogUpdatePrompt Evaluate(
            PlatformConfiguration configuration,
            string currentVersion)
        {
            if (configuration == null
                || !TryParse(currentVersion, out var current)
                || !TryParse(configuration.hardVersion, out var hard)
                || !TryParse(configuration.softVersion, out var soft)
                || Compare(hard, soft) > 0)
            {
                return Catalog.CatalogUpdatePrompt.None;
            }
            var mode = Compare(current, hard) < 0
                ? Catalog.CatalogUpdateMode.Hard
                : Compare(current, soft) < 0
                    ? Catalog.CatalogUpdateMode.Soft
                    : Catalog.CatalogUpdateMode.None;
            return new Catalog.CatalogUpdatePrompt(mode, configuration.storeUrl);
        }

        private static bool TryParse(string value, out int[] segments)
        {
            segments = null;
            if (string.IsNullOrWhiteSpace(value))
                return false;
            var parts = value.Split('.');
            segments = new int[parts.Length];
            for (var index = 0; index < parts.Length; index++)
            {
                if (!int.TryParse(parts[index], out segments[index]) || segments[index] < 0)
                    return false;
            }
            return segments.Length > 0;
        }

        private static int Compare(int[] left, int[] right)
        {
            for (var index = 0; index < Math.Max(left.Length, right.Length); index++)
            {
                var leftValue = index < left.Length ? left[index] : 0;
                var rightValue = index < right.Length ? right[index] : 0;
                if (leftValue != rightValue)
                    return leftValue.CompareTo(rightValue);
            }
            return 0;
        }
    }
}
