namespace Novels.Catalog
{
    public static class CatalogAddresses
    {
        public const string BundleName =
            ContentAddressing.ContentPackageConvention.CatalogBundleName;
#if NOVELS_CHILDREN_CATALOG
        public const string ScreenAssetName =
            "Assets/RemoteAssets/catalog/children/screen.prefab";
#elif NOVELS_NOCHELESSIE_CATALOG
        public const string ScreenAssetName =
            "Assets/RemoteAssets/catalog/nochelessie/screen.prefab";
#elif NOVELS_SCP_CATALOG
        public const string ScreenAssetName =
            "Assets/RemoteAssets/catalog/scp/screen.prefab";
#else
        public const string ScreenAssetName =
            "Assets/RemoteAssets/catalog/fallback.prefab";
#endif
    }
}
