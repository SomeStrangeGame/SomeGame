using System;
using UnityEditor;

namespace Novels.ContentSdk.Editor
{
    internal sealed class NovelContentTexturePostprocessor : AssetPostprocessor
    {
        private const string _legacyContentRoot = "Assets/RemoteAssets/content/";
        private const string _characterRoot = "Assets/Characters/";
        private const string _locationRoot = "Assets/Locations/";
        private const string _presentationCharacterRoot =
            "Assets/Presentation/character/characters/";
        private const string _presentationLocationRoot =
            "Assets/Presentation/location/locations/";
        private const string _locationSegment = "/location/locations/";
        private const string _characterSegment = "/character/characters/";

        public override uint GetVersion() => 5;

        private void OnPreprocessTexture()
        {
            if (!IsNovelSprite(assetPath))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 100f;
            if (assetPath.StartsWith(_characterRoot, StringComparison.Ordinal)
                || assetPath.StartsWith(
                    _presentationCharacterRoot,
                    StringComparison.Ordinal)
                || assetPath.IndexOf(_characterSegment, StringComparison.Ordinal) >= 0)
                importer.alphaIsTransparency = true;
            ConfigureMobile(importer, "Android", TextureImporterFormat.ASTC_6x6);
            ConfigureMobile(importer, "iPhone", TextureImporterFormat.ASTC_8x8);
        }

        private static void ConfigureMobile(
            TextureImporter importer,
            string platform,
            TextureImporterFormat format)
        {
            var settings = importer.GetPlatformTextureSettings(platform);
            settings.name = platform;
            settings.overridden = true;
            settings.maxTextureSize = 4096;
            settings.format = format;
            settings.compressionQuality = 100;
            importer.SetPlatformTextureSettings(settings);
        }

        private static bool IsNovelSprite(string path) =>
            path.StartsWith(_locationRoot, StringComparison.Ordinal)
            || path.StartsWith(_characterRoot, StringComparison.Ordinal)
            || path.StartsWith(_presentationLocationRoot, StringComparison.Ordinal)
            || path.StartsWith(_presentationCharacterRoot, StringComparison.Ordinal)
            || path.StartsWith(_legacyContentRoot, StringComparison.Ordinal)
            && (path.IndexOf(_locationSegment, StringComparison.Ordinal) >= 0
                || path.IndexOf(_characterSegment, StringComparison.Ordinal) >= 0);
    }
}
