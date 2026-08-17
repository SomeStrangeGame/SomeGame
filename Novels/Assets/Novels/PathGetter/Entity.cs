using Disposable;

namespace Novels.PathGetter
{
    public class Entity : BaseDisposable
    {
        private const string _remoteAssetsRoot = "Assets/RemoteAssets";
        private const string _prefabExtension = ".prefab";
        private const string _imageExtension = ".png";
        private const string _assetExtension = ".asset";

        public struct Ctx
        {
            public string Prefix;
        }

        private Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public string GetNovelTextPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            return $"NovelTexts/{_ctx.Prefix}/{path}";
        }

        public string GetMainLoadingPrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"{_remoteAssetsRoot}/Loading/{assetName}{_prefabExtension}";
        }

        public string GetLoadingPrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"{_remoteAssetsRoot}/Loading/{_ctx.Prefix}/{assetName}{_prefabExtension}";
        }

        public string GetSettingPrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"{_remoteAssetsRoot}/Setting/{_ctx.Prefix}/{assetName}{_prefabExtension}";
        }

        public string GetBubblePrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"{_remoteAssetsRoot}/Bubble/{_ctx.Prefix}/{assetName}{_prefabExtension}";
        }

        public string GetLocationPrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"{_remoteAssetsRoot}/Location/{_ctx.Prefix}/{assetName}{_prefabExtension}";
        }

        public string GetLocationImagePath(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            var firstChar = char.ToUpperInvariant(assetName[0]);
            var otherText = assetName.Substring(1).ToLowerInvariant();
            return $"{_remoteAssetsRoot}/Location/{_ctx.Prefix}/Locations/{firstChar}{otherText}{_imageExtension}";
        }

        public string GetCharacterPrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"{_remoteAssetsRoot}/Character/{_ctx.Prefix}/{assetName}{_prefabExtension}";
        }

        public string GetNotificationPrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"{_remoteAssetsRoot}/Notification/{_ctx.Prefix}/{assetName}{_prefabExtension}";
        }

        public string GetLocalizationDataAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"{_remoteAssetsRoot}/Localization/{_ctx.Prefix}/{assetName}{_assetExtension}";
        }
    }
}
