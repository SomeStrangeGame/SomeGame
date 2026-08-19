using System;
using System.Collections.Generic;
using UnityEngine;

namespace Localization
{
    [CreateAssetMenu(fileName = "LocalizationData", menuName = "ScriptableObjects/LocalizationData")]
    public sealed class LocalizationData : ScriptableObject
    {
        [Serializable]
        private struct LocalizedValue
        {
            [SerializeField] private string _locale;
            [SerializeField] private string _value;

            internal readonly string Locale => _locale;
            internal readonly string Value => _value;
        }

        [Serializable]
        private struct Pair
        {
            [SerializeField] private string _key;
            [SerializeField] private LocalizedValue[] _localizations;

            internal string Key => _key;

            internal string Resolve(
                string locale,
                string fallbackLocale,
                bool requireExactLocale)
            {
                var values = _localizations ?? Array.Empty<LocalizedValue>();
                var locales = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var hasRequested = false;
                var requested = string.Empty;
                var hasFallback = false;
                var fallback = string.Empty;
                foreach (var value in values)
                {
                    var valueLocale = Normalize(value.Locale);
                    if (!locales.Add(valueLocale))
                    {
                        throw new InvalidOperationException(
                            $"Duplicate localization locale '{valueLocale}' for key '{_key}'.");
                    }
                    if (string.Equals(
                            valueLocale,
                            locale,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        hasRequested = true;
                        requested = value.Value ?? string.Empty;
                    }
                    if (string.Equals(
                            valueLocale,
                            fallbackLocale,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        hasFallback = true;
                        fallback = value.Value ?? string.Empty;
                    }
                }
                if (!hasFallback)
                {
                    throw new InvalidOperationException(
                        $"Localization key '{_key}' has no fallback locale "
                        + $"'{fallbackLocale}'.");
                }
                if (requireExactLocale && !hasRequested)
                {
                    throw new InvalidOperationException(
                        $"Localization key '{_key}' has no locale '{locale}'.");
                }
                return hasRequested ? requested : fallback;
            }
        }

        [SerializeField] private string _fallbackLocale = "en";
        [SerializeField] private Pair[] _pairs;

        internal IReadOnlyDictionary<string, string> CreateSnapshot(
            string locale,
            bool requireExactLocale)
        {
            var requested = Normalize(locale);
            var fallback = Normalize(_fallbackLocale);
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var pair in _pairs ?? Array.Empty<Pair>())
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                    throw new InvalidOperationException("Localization key must not be empty.");
                if (!result.TryAdd(
                        pair.Key,
                        pair.Resolve(requested, fallback, requireExactLocale)))
                    throw new InvalidOperationException(
                        $"Duplicate localization key '{pair.Key}'.");
            }
            return result;
        }

        private static string Normalize(string locale) =>
            string.IsNullOrWhiteSpace(locale)
                ? throw new InvalidOperationException("Localization locale must not be empty.")
                : locale.Trim().ToLowerInvariant();
    }
}
