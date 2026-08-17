namespace Novels.Catalog
{
    public sealed class CatalogItem
    {
        public CatalogItem(
            string id,
            string title,
            string description = null,
            string status = null,
            bool isEnabled = true)
        {
            Id = id;
            Title = title;
            Description = description;
            Status = status;
            IsEnabled = isEnabled;
        }

        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public string Status { get; }
        public bool IsEnabled { get; }
    }
}
