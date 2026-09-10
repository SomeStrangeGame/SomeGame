using System;

namespace Novels.Catalog
{
    public enum CatalogUpdateMode
    {
        None,
        Soft,
        Hard,
    }

    public readonly struct CatalogUpdatePrompt
    {
        public CatalogUpdatePrompt(CatalogUpdateMode mode, string storeUrl)
        {
            Mode = mode;
            StoreUrl = storeUrl ?? string.Empty;
        }

        public CatalogUpdateMode Mode { get; }
        public string StoreUrl { get; }
        public bool IsVisible => Mode != CatalogUpdateMode.None
            && Uri.TryCreate(StoreUrl, UriKind.Absolute, out var uri)
            && uri.Scheme == Uri.UriSchemeHttps;

        public static CatalogUpdatePrompt None => default;
    }
}
