using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Novels.Catalog
{
    public readonly struct CatalogText
    {
        public CatalogText(string title, string description)
        {
            Title = title ?? string.Empty;
            Description = description ?? string.Empty;
        }

        public string Title { get; }
        public string Description { get; }
    }

    [Serializable]
    public sealed class NovelCatalogEntry
    {
        [SerializeField] private string _contentId;
        [SerializeField] private string _title;
        [SerializeField] private string _description;
        [SerializeField] private bool _disabled;

        public NovelCatalogEntry(
            string contentId,
            string title,
            string description,
            bool isEnabled = true)
        {
            _contentId = contentId ?? throw new ArgumentNullException(nameof(contentId));
            _title = title ?? string.Empty;
            _description = description ?? string.Empty;
            _disabled = !isEnabled;
        }

        public string ContentId => _contentId;
        public bool IsEnabled => !_disabled;
        public string ContentBundleName =>
            ContentAddressing.ContentPackageConvention.ContentBundle(_contentId);
        public string ContentAssetName =>
            ContentAddressing.ContentPackageConvention.DefinitionAsset(_contentId);
        public CatalogText Text => new(_title, _description);
    }

    [CreateAssetMenu(fileName = "NovelCatalog", menuName = "Novels/Catalog")]
    public sealed class NovelCatalogAsset : ScriptableObject
    {
        [SerializeField] private string _title;
        [SerializeField] private string _description;
        [SerializeField] private NovelCatalogEntry[] _entries;

        private ReadOnlyCollection<NovelCatalogEntry> _readOnlyEntries;

        public CatalogText Text => new(_title, _description);
        public IReadOnlyList<NovelCatalogEntry> Entries =>
            _readOnlyEntries ??= Array.AsReadOnly(
                _entries ?? Array.Empty<NovelCatalogEntry>());
    }
}
