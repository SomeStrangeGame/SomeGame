using System;
using UnityEditor;

namespace Editor
{
    internal sealed class NovelContentTextureImporter : AssetPostprocessor
    {
        private const string _contentRoot = "Assets/RemoteAssets/Content/";
        private const string _locationSegment = "/Location/Locations/";
        private const string _characterSegment = "/Character/Characters/";

        public override uint GetVersion() => 2;

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
        }

        private static bool IsNovelSprite(string path) =>
            path.IndexOf(_locationSegment, StringComparison.Ordinal) >= 0
            || path.IndexOf(_characterSegment, StringComparison.Ordinal) >= 0;
    }
}
