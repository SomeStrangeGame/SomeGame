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
        public bool TryResolveExact(string locale, out CatalogText value) =>
            NovelCatalogAsset.TryResolveExact(_localizations, locale, out value);
    }

    [CreateAssetMenu(fileName = "NovelCatalog", menuName = "Novels/Catalog")]
    public sealed class NovelCatalogAsset : ScriptableObject
    {
        [SerializeField] private CatalogLocalization[] _localizations;
        [SerializeField] private NovelCatalogEntry[] _entries;

        private ReadOnlyCollection<NovelCatalogEntry> _readOnlyEntries;

        public CatalogText Resolve(string locale) =>
            Resolve(_localizations, locale);
        public bool TryResolveExact(string locale, out CatalogText value) =>
            TryResolveExact(_localizations, locale, out value);
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

        internal static bool TryResolveExact(
            CatalogLocalization[] localizations,
            string locale,
            out CatalogText text)
        {
            var requested = Locale.LocaleProvider.Normalize(locale);
            CatalogLocalization selected = null;
            var locales = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var value in localizations ?? Array.Empty<CatalogLocalization>())
            {
                if (value == null)
                    continue;
                var valueLocale = Locale.LocaleProvider.NormalizeRequired(value.Locale);
                if (!locales.Add(valueLocale))
                {
                    throw new InvalidOperationException(
                        $"Duplicate localization locale '{valueLocale}'.");
                }
                if (string.Equals(
                        valueLocale,
                        requested,
                        StringComparison.OrdinalIgnoreCase))
                    selected = value;
            }
            text = selected == null
                ? new CatalogText(string.Empty, string.Empty)
                : new CatalogText(selected.Title, selected.Description);
            return selected != null;
        }
    }
}
