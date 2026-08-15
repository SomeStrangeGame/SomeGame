using Disposable;

namespace Novels.PathGetter
{
    public class Entity : BaseDisposable
    {
        private const string _remoteAssetsRoot = "Assets/RemoteAssets";
        private const string _prefabExtension = ".prefab";
        private const string _imageExtension = ".png";
        private const string _assetExtension = ".asset";
        private const string _mainBodyAssetName = "Main";

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

        public string GetCharacterMainBodyPath(string name, string view, string arg)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            return $"{_remoteAssetsRoot}/Character/{_ctx.Prefix}/Characters/{name}/{view}/{arg ?? _mainBodyAssetName}{_imageExtension}";
        }

        public string GetCharacterEmotionPath(string name, string view, string arg)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            if (string.IsNullOrEmpty(arg)) return string.Empty;

            var firstChar = char.ToUpperInvariant(arg[0]);
            var otherText = arg.Substring(1).ToLowerInvariant();
            return $"{_remoteAssetsRoot}/Character/{_ctx.Prefix}/Characters/{name}/{view}/Emotions/{firstChar}{otherText}{_imageExtension}";
        }

        public string GetCharacterClothesPath(string name, string arg, int index)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            if (string.IsNullOrEmpty(arg)) return string.Empty;

            var firstChar = char.ToUpperInvariant(arg[0]);
            var otherText = arg.Substring(1).ToLowerInvariant();
            return $"{_remoteAssetsRoot}/Character/{_ctx.Prefix}/Characters/{name}/Clothes/{firstChar}{otherText}/{index}{_imageExtension}";
        }

        public string GetCharacterHairPath(string name, string arg, string direction, string color)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            if (string.IsNullOrEmpty(arg)) return string.Empty;

            var firstChar = char.ToUpperInvariant(arg[0]);
            var otherText = arg.Substring(1).ToLowerInvariant();
            return $"{_remoteAssetsRoot}/Character/{_ctx.Prefix}/Characters/{name}/Hairs/{direction}/{firstChar}{otherText}/{color}{_imageExtension}";
        }

        public string GetCharacterAccessoriesPath(string name, string arg, string direction)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            if (string.IsNullOrEmpty(arg)) return string.Empty;

            var firstChar = char.ToUpperInvariant(arg[0]);
            var otherText = arg.Substring(1).ToLowerInvariant();
            return $"{_remoteAssetsRoot}/Character/{_ctx.Prefix}/Characters/{name}/Accessories/{direction}/{firstChar}{otherText}{_imageExtension}";
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
