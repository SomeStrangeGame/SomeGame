using Disposable;

namespace Novels.PathGetter
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public string Prefix;
        }

        private Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public string GetNovelTextPath(string path) =>
            ContentAddressing.ContentAddressConvention.NovelText(_ctx.Prefix, path);

        public string GetMainLoadingPrefabAssetName(string assetName) =>
            ContentAddressing.ContentAddressConvention.MainLoadingPrefab(assetName);

        public string GetLoadingPrefabAssetName(string assetName) =>
            ContentAddressing.ContentAddressConvention.LoadingPrefab(
                _ctx.Prefix,
                assetName);

        public string GetSettingPrefabAssetName(string assetName) =>
            ContentAddressing.ContentAddressConvention.SettingPrefab(
                _ctx.Prefix,
                assetName);

        public string GetBubblePrefabAssetName(string assetName) =>
            ContentAddressing.ContentAddressConvention.BubblePrefab(
                _ctx.Prefix,
                assetName);

        public string GetLocationPrefabAssetName(string assetName) =>
            ContentAddressing.ContentAddressConvention.LocationPrefab(
                _ctx.Prefix,
                assetName);

        public string GetLocationImagePath(string assetName) =>
            ContentAddressing.ContentAddressConvention.LocationImage(
                _ctx.Prefix,
                assetName);

        public string GetCharacterPrefabAssetName(string assetName) =>
            ContentAddressing.ContentAddressConvention.CharacterPrefab(
                _ctx.Prefix,
                assetName);

        public string GetNotificationPrefabAssetName(string assetName) =>
            ContentAddressing.ContentAddressConvention.NotificationPrefab(
                _ctx.Prefix,
                assetName);

        public string GetLocalizationDataAssetName(string assetName) =>
            ContentAddressing.ContentAddressConvention.LocalizationAsset(
                _ctx.Prefix,
                assetName);
    }
}
