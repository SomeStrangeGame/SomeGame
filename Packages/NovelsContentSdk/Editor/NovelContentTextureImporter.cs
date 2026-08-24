using System;
using UnityEditor;

namespace Novels.ContentSdk.Editor
{
    internal sealed class NovelContentTextureImporter : AssetPostprocessor
    {
        private const string _contentRoot = "Assets/RemoteAssets/content/";
        private const string _locationSegment = "/location/locations/";
        private const string _characterSegment = "/character/characters/";

        public override uint GetVersion() => 3;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(_contentRoot, StringComparison.Ordinal)
                || !IsNovelSprite(assetPath))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 100f;
            if (assetPath.IndexOf(_characterSegment, StringComparison.Ordinal) >= 0)
                importer.alphaIsTransparency = true;
            ConfigureMobile(importer, "Android");
            ConfigureMobile(importer, "iPhone");
        }

        private static void ConfigureMobile(TextureImporter importer, string platform)
        {
            var settings = importer.GetPlatformTextureSettings(platform);
            settings.name = platform;
            settings.overridden = true;
            settings.maxTextureSize = 4096;
            settings.format = TextureImporterFormat.ASTC_6x6;
            settings.compressionQuality = 100;
            importer.SetPlatformTextureSettings(settings);
        }

        private static bool IsNovelSprite(string path) =>
            path.IndexOf(_locationSegment, StringComparison.Ordinal) >= 0
            || path.IndexOf(_characterSegment, StringComparison.Ordinal) >= 0;
    }
}
