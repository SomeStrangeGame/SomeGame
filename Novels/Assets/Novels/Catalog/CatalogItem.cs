namespace Novels.Catalog
{
    public sealed class CatalogItem
    {
        public CatalogItem(string id, string title, string description = null)
        {
            Id = id;
            Title = title;
            Description = description;
        }

        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
    }
}
