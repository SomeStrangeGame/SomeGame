using System;
using System.Collections.Generic;
using System.IO;
using Bundles;
using UnityEditor;
using UnityEngine;

namespace Novels.ContentSdk.Editor
{
    internal static class ExperimentalPreviewTextures
    {
        private sealed class ImporterState
        {
            internal string Path;
            internal string Platform;
            internal TextureImporterPlatformSettings Settings;
        }

        private static readonly List<ImporterState> _states = new();
        private static string _builtPath;
        private static string _builtVersion;
        private static uint _builtCrc;

        internal static void Apply(BuildTarget target)
        {
            Restore();
            var platform = PlatformName(target);
            foreach (var path in ContentAssets.FindBundleAssets())
            {
                if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
                    continue;
                var settings = importer.GetPlatformTextureSettings(platform);
                _states.Add(new ImporterState
                {
                    Path = path,
                    Platform = platform,
                    Settings = Copy(settings),
                });
                var preview = Copy(settings);
                preview.name = platform;
                preview.overridden = true;
                preview.maxTextureSize = Math.Max(32, Math.Min(256,
                    settings.maxTextureSize > 0 ? settings.maxTextureSize / 4 : 256));
                importer.SetPlatformTextureSettings(preview);
                importer.SaveAndReimport();
            }
        }

        internal static void Restore()
        {
            foreach (var state in _states)
            {
                if (AssetImporter.GetAtPath(state.Path) is not TextureImporter importer)
                    continue;
                importer.SetPlatformTextureSettings(state.Settings);
                importer.SaveAndReimport();
            }
            _states.Clear();
        }

        internal static void RegisterBuiltBundle(
            AssetBundleManifest manifest,
            string staging,
            string bundleName)
        {
            _builtPath = Path.Combine(staging, bundleName);
            _builtVersion = manifest.GetAssetBundleHash(bundleName).ToString();
            if (!BuildPipeline.GetCRCForAssetBundle(_builtPath, out _builtCrc))
                throw new InvalidOperationException("Preview bundle CRC cannot be calculated.");
        }

        internal static BundleReleaseEntry CopyBuiltBundle(
            string outputRoot,
            string platform,
            string storyId)
        {
            if (string.IsNullOrWhiteSpace(_builtPath) || !File.Exists(_builtPath))
                throw new InvalidOperationException("Preview bundle was not built.");
            var name = ContentAddressing.ContentPackageConvention.PreviewBundle(storyId);
            var directory = Path.Combine(outputRoot, "Remote", platform, name);
            Directory.CreateDirectory(directory);
            var destination = Path.Combine(directory, _builtVersion);
            File.Copy(_builtPath, destination, true);
            return new BundleReleaseEntry
            {
                name = name,
                version = _builtVersion,
                size = new FileInfo(destination).Length,
                sha256 = ContentHash.ComputeSha256(destination),
                crc = _builtCrc,
                deliveryGroup = ContentAddressing.ContentPackageConvention
                    .StoryPreviewDeliveryGroup(storyId),
            };
        }

        private static string PlatformName(BuildTarget target) => target switch
        {
            BuildTarget.Android => "Android",
            BuildTarget.iOS => "iPhone",
            _ => "Standalone",
        };

        private static TextureImporterPlatformSettings Copy(
            TextureImporterPlatformSettings source) => new()
        {
            name = source.name,
            overridden = source.overridden,
            maxTextureSize = source.maxTextureSize,
            resizeAlgorithm = source.resizeAlgorithm,
            format = source.format,
            textureCompression = source.textureCompression,
            compressionQuality = source.compressionQuality,
            crunchedCompression = source.crunchedCompression,
            allowsAlphaSplitting = source.allowsAlphaSplitting,
            androidETC2FallbackOverride = source.androidETC2FallbackOverride,
        };
    }
}
