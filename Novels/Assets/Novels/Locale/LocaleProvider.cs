using System;
using System.Collections.Generic;
using System.Globalization;

namespace Novels.Locale
{
    public sealed class LocaleProvider
    {
        public LocaleProvider(CultureInfo culture)
        {
            Code = Normalize(culture?.TwoLetterISOLanguageName);
        }

        public string Code { get; }

        public static string Normalize(string locale) =>
            string.IsNullOrWhiteSpace(locale)
                ? "en"
                : locale.Trim().ToLowerInvariant();
    }

    public static class LocaleSelector
    {
        public static bool TryFind<T>(
            IEnumerable<T> values,
            Func<T, string> getLocale,
            string locale,
            out T selected)
        {
            if (getLocale == null)
                throw new ArgumentNullException(nameof(getLocale));
            if (string.IsNullOrWhiteSpace(locale))
                throw new ArgumentException("Locale must not be empty.", nameof(locale));
            var requested = LocaleProvider.Normalize(locale);
            var hasFallback = false;
            var fallback = default(T);
            var locales = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var value in values ?? Array.Empty<T>())
            {
                var valueLocale = LocaleProvider.Normalize(getLocale(value));
                if (!locales.Add(valueLocale))
                {
                    throw new InvalidOperationException(
                        $"Duplicate localization locale '{valueLocale}'.");
                }
                if (!hasFallback)
                {
                    fallback = value;
                    hasFallback = true;
                }
                if (string.Equals(
                        valueLocale,
                        requested,
                        StringComparison.OrdinalIgnoreCase))
                {
                    selected = value;
                    return true;
                }
            }
            selected = fallback;
            return hasFallback;
        }
    }
}
