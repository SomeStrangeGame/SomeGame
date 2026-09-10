using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Novels.Catalog
{
    public readonly struct CatalogText
    {
        public CatalogText(string title, string genre, string description)
        {
            Title = title ?? string.Empty;
            Genre = genre ?? string.Empty;
            Description = description ?? string.Empty;
        }

        public string Title { get; }
        public string Genre { get; }
        public string Description { get; }
    }

    [Serializable]
    public sealed class NovelCatalogEpisodeEntry
    {
        public NovelCatalogEpisodeEntry(string id, string title, string description)
        {
            Id = string.IsNullOrWhiteSpace(id)
                ? throw new ArgumentException("Episode id must not be empty.", nameof(id))
                : id;
            Title = string.IsNullOrWhiteSpace(title)
                ? throw new ArgumentException("Episode title must not be empty.", nameof(title))
                : title;
            Description = description?.Trim() ?? string.Empty;
        }

        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
    }

    [Serializable]
    public sealed class NovelCatalogEntry
    {
        [SerializeField] private string _contentId;
        [SerializeField] private string _title;
        [SerializeField] private string _genre;
        [SerializeField] private string _description;
        [SerializeField] private bool _disabled;
        [SerializeField] private string _author;

        public NovelCatalogEntry(
            string contentId,
            string title,
            string genre,
            string description,
            IEnumerable<NovelCatalogEpisodeEntry> episodes = null,
            bool isEnabled = true,
            string author = null)
        {
            _contentId = contentId ?? throw new ArgumentNullException(nameof(contentId));
            _title = title ?? string.Empty;
            _genre = genre ?? string.Empty;
            _description = description ?? string.Empty;
            _disabled = !isEnabled;
            _author = author?.Trim() ?? string.Empty;
            Episodes = Array.AsReadOnly(
                (episodes ?? Array.Empty<NovelCatalogEpisodeEntry>())
                    .Where(episode => episode != null)
                    .ToArray());
        }

        public string ContentId => _contentId;
        public string Author => _author;
        public bool IsEnabled => !_disabled;
        public string ContentBundleName =>
            ContentAddressing.ContentPackageConvention.ContentBundle(_contentId);
        public string ContentAssetName =>
            ContentAddressing.ContentPackageConvention.DefinitionAsset(_contentId);
        public CatalogText Text => new(_title, _genre, _description);
        public IReadOnlyList<NovelCatalogEpisodeEntry> Episodes { get; }
    }
}
