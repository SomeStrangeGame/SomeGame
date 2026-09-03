namespace Novels.Catalog
{
    public sealed class CatalogItem
    {
        public CatalogItem(
            string id,
            string title,
            string genre = null,
            string description = null,
            string status = null,
            string actionLabel = null,
            string secondaryActionLabel = null,
            bool isEnabled = true,
            UnityEngine.Sprite cover = null)
        {
            Id = id;
            Title = title;
            Genre = genre;
            Description = description;
            Status = status;
            ActionLabel = actionLabel;
            SecondaryActionLabel = secondaryActionLabel;
            IsEnabled = isEnabled;
            Cover = cover;
        }

        public string Id { get; }
        public string Title { get; }
        public string Genre { get; }
        public string Description { get; }
        public string Status { get; }
        public string ActionLabel { get; }
        public string SecondaryActionLabel { get; }
        public bool IsEnabled { get; }
        public UnityEngine.Sprite Cover { get; }
    }
}
