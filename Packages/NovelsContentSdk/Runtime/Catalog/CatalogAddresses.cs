namespace Novels.Catalog
{
    public static class CatalogAddresses
    {
        public const string BundleName =
            ContentAddressing.ContentPackageConvention.CatalogBundleName;
#if NOVELS_CHILDREN_CATALOG
        public const string ScreenAssetName =
            "Assets/RemoteAssets/catalog/children/screen.prefab";
#else
        public const string ScreenAssetName =
            "Assets/RemoteAssets/catalog/fallback.prefab";
#endif
    }
}
