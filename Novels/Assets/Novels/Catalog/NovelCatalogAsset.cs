using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Novels.Catalog
{
    [Serializable]
    public sealed class CatalogLocalization
    {
        [SerializeField] private string _locale;
        [SerializeField] private string _title;
        [SerializeField] private string _description;

        public string Locale => _locale;
        public string Title => _title;
        public string Description => _description;
    }

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
        [SerializeField] private CatalogLocalization[] _localizations;

        public string ContentId => _contentId;
        public string ContentBundleName =>
            ContentAddressing.ContentPackageConvention.ContentBundle(_contentId);
        public string ContentAssetName =>
            ContentAddressing.ContentPackageConvention.DefinitionAsset(_contentId);
        public CatalogText Resolve(string locale) =>
            NovelCatalogAsset.Resolve(_localizations, locale);
    }

    [CreateAssetMenu(fileName = "NovelCatalog", menuName = "Novels/Catalog")]
    public sealed class NovelCatalogAsset : ScriptableObject
    {
        [SerializeField] private CatalogLocalization[] _localizations;
        [SerializeField] private NovelCatalogEntry[] _entries;

        private ReadOnlyCollection<NovelCatalogEntry> _readOnlyEntries;

        public CatalogText Resolve(string locale) =>
            Resolve(_localizations, locale);
        public IReadOnlyList<NovelCatalogEntry> Entries =>
            _readOnlyEntries ??= Array.AsReadOnly(
                _entries ?? Array.Empty<NovelCatalogEntry>());

        internal static CatalogText Resolve(
            CatalogLocalization[] localizations,
            string locale)
        {
            var values = localizations ?? Array.Empty<CatalogLocalization>();
            var found = Locale.LocaleSelector.TryFind(
                values.Where(item => item != null),
                item => item.Locale,
                locale,
                out var value);
            return !found || value == null
                ? new CatalogText(string.Empty, string.Empty)
                : new CatalogText(value.Title, value.Description);
        }
    }
}
