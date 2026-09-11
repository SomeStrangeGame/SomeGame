using System;

namespace Novels.Catalog
{
    public enum CatalogUpdateState
    {
        Ready,
        Downloading,
        Verifying,
        PermissionRequired,
        Installing,
        Failed,
    }

    public interface ICatalogUpdateAction
    {
        CatalogUpdateState State { get; }
        float Progress { get; }
        string StatusMessage { get; }
        bool CanStart { get; }
        void Start();
    }

    public enum CatalogUpdateMode
    {
        None,
        Soft,
        Hard,
    }

    public readonly struct CatalogUpdatePrompt
    {
        public CatalogUpdatePrompt(CatalogUpdateMode mode, string storeUrl,
            ICatalogUpdateAction action = null)
        {
            Mode = mode;
            StoreUrl = storeUrl ?? string.Empty;
            Action = action;
        }

        public CatalogUpdateMode Mode { get; }
        public string StoreUrl { get; }
        public ICatalogUpdateAction Action { get; }
        public bool IsVisible => Mode != CatalogUpdateMode.None
            && (Action != null
                || Uri.TryCreate(StoreUrl, UriKind.Absolute, out var uri)
                && uri.Scheme == Uri.UriSchemeHttps);

        public static CatalogUpdatePrompt None => default;
    }
}
