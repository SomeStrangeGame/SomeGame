namespace Novels.Catalog
{
    public sealed class CatalogItem
    {
        public CatalogItem(
            string id,
            string title,
            string description = null,
            string status = null,
            bool isEnabled = true,
            UnityEngine.Sprite cover = null)
        {
            Id = id;
            Title = title;
            Description = description;
            Status = status;
            IsEnabled = isEnabled;
            Cover = cover;
        }

        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public string Status { get; }
        public bool IsEnabled { get; }
        public UnityEngine.Sprite Cover { get; }
    }
}
