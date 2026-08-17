using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Novels.Catalog
{
    [Serializable]
    public sealed class NovelCatalogEntry
    {
        [SerializeField] private string _contentId;
        [SerializeField] private string _title;
        [SerializeField] private string _description;

        public string ContentId => _contentId;
        public string Title => _title;
        public string Description => _description;
    }

    [CreateAssetMenu(fileName = "NovelCatalog", menuName = "Novels/Catalog")]
    public sealed class NovelCatalogAsset : ScriptableObject
    {
        [SerializeField] private string _title;
        [SerializeField] private NovelCatalogEntry[] _entries;

        private ReadOnlyCollection<NovelCatalogEntry> _readOnlyEntries;

        public string Title => _title;
        public IReadOnlyList<NovelCatalogEntry> Entries =>
            _readOnlyEntries ??= Array.AsReadOnly(
                _entries ?? Array.Empty<NovelCatalogEntry>());
    }
}
